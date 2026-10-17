using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using System;
using System.Globalization;
using System.Xml;
using static LiveSplit.UI.SettingsHelper;

namespace LiveSplit.Options.SettingsSavers;

public class XMLSettingsSaver : ISettingsSaver
{
    public void Save(ISettings settings, System.IO.Stream stream)
    {
        var document = new XmlDocument();

        XmlNode docNode = document.CreateXmlDeclaration("1.0", "UTF-8", null);
        document.AppendChild(docNode);

        XmlElement parent = document.CreateElement("Settings");
        XmlAttribute version = document.CreateAttribute("version");
        version.Value = "1.8.18";
        parent.Attributes.Append(version);
        document.AppendChild(parent);

        XmlElement hotkeyProfiles = document.CreateElement("HotkeyProfiles");
        foreach (System.Collections.Generic.KeyValuePair<string, HotkeyProfile> hotkeyProfile in settings.HotkeyProfiles)
        {
            XmlElement hotkeyProfileElement = hotkeyProfile.Value.ToXml(document);
            XmlAttribute name = document.CreateAttribute("name");
            name.Value = hotkeyProfile.Key;
            hotkeyProfileElement.Attributes.Append(name);
            hotkeyProfiles.AppendChild(hotkeyProfileElement);
        }

        parent.AppendChild(hotkeyProfiles);

        CreateSetting(document, parent, "WarnOnReset", settings.WarnOnReset);
        CreateSetting(document, parent, "RaceViewer", settings.RaceViewer.Name);
        CreateSetting(document, parent, "AgreedToSRLRules", settings.AgreedToSRLRules);

        if (Environment.OSVersion.Version.Major >= Settings.DPI_AWARENESS_OS_MIN_VERSION)
        {
            CreateSetting(document, parent, "EnableDPIAwareness", settings.EnableDPIAwareness);
        }
        else
        {
            CreateSetting(document, parent, "EnableDPIAwareness", false);
        }

        if (settings is Settings adaptiveSettings)
        {
            CreateSetting(document, parent, "EnableAdaptiveDpiLayout", adaptiveSettings.EnableAdaptiveDpiLayout);
        }
        else
        {
            CreateSetting(document, parent, "EnableAdaptiveDpiLayout", false);
        }

        CreateSetting(document, parent, "UILanguage", settings.UILanguage ?? string.Empty);
        CreateSetting(document, parent, "EnableSpeedrunComIntegration", settings.EnableSpeedrunComIntegration);

        XmlElement recentSplits = document.CreateElement("RecentSplits");
        foreach (RecentSplitsFile splitsFile in settings.RecentSplits)
        {
            XmlElement splitsFileElement = ToElement(document, "SplitsFile", splitsFile.Path);
            splitsFileElement.SetAttribute("gameName", splitsFile.GameName);
            splitsFileElement.SetAttribute("categoryName", splitsFile.CategoryName);
            splitsFileElement.SetAttribute("lastTimingMethod", splitsFile.LastTimingMethod.ToString());
            splitsFileElement.SetAttribute("lastHotkeyProfile", splitsFile.LastHotkeyProfile.ToString());
            recentSplits.AppendChild(splitsFileElement);
        }

        parent.AppendChild(recentSplits);
        XmlElement recentLayouts = document.CreateElement("RecentLayouts");
        foreach (string layout in settings.RecentLayouts)
        {
            CreateSetting(document, recentLayouts, "LayoutPath", layout);
        }

        parent.AppendChild(recentLayouts);

        // LiveSplit YS Stage 4.2: Store only the learned visual DPI baseline.
        // The layout file itself remains untouched until the user makes a real
        // layout edit and explicitly saves it.
        XmlElement layoutDpiReferences = document.CreateElement("LayoutDpiReferences");
        if (settings is Settings concreteSettings)
        {
            foreach (System.Collections.Generic.KeyValuePair<string, float> reference in concreteSettings.LayoutDpiReferences)
            {
                if (string.IsNullOrWhiteSpace(reference.Key)
                    || reference.Value <= 0f
                    || float.IsNaN(reference.Value)
                    || float.IsInfinity(reference.Value))
                {
                    continue;
                }

                XmlElement layoutReference = document.CreateElement("Layout");
                layoutReference.SetAttribute("path", reference.Key);
                layoutReference.SetAttribute("dpi", reference.Value.ToString("0.###", CultureInfo.InvariantCulture));
                layoutDpiReferences.AppendChild(layoutReference);
            }
        }
        parent.AppendChild(layoutDpiReferences);

        CreateSetting(document, parent, "LastComparison", settings.LastComparison);
        CreateSetting(document, parent, "SimpleSumOfBest", settings.SimpleSumOfBest);
        CreateSetting(document, parent, "RefreshRate", settings.RefreshRate);
        CreateSetting(document, parent, "ServerPort", settings.ServerPort);
        CreateSetting(document, parent, "ServerStartup", (int)settings.ServerStartup);
        CreateSetting(document, parent, "ServerState", (int)settings.ServerState);

        XmlElement generatorStates = document.CreateElement("ComparisonGeneratorStates");
        foreach (System.Collections.Generic.KeyValuePair<string, bool> generator in settings.ComparisonGeneratorStates)
        {
            XmlElement generatorElement = document.CreateElement("Generator");
            XmlAttribute name = document.CreateAttribute("name");
            name.Value = generator.Key;
            generatorElement.Attributes.Append(name);
            generatorElement.InnerText = generator.Value.ToString();

            if (generator.Key == HCPComparisonGenerator.ComparisonName)
            {
                XmlAttribute hcpHistorySize = document.CreateAttribute("HcpHistorySize");
                hcpHistorySize.Value = settings.HcpHistorySize.ToString();
                generatorElement.Attributes.Append(hcpHistorySize);

                XmlAttribute hcpNBestRuns = document.CreateAttribute("HcpNBestRuns");
                hcpNBestRuns.Value = settings.HcpNBestRuns.ToString();
                generatorElement.Attributes.Append(hcpNBestRuns);
            }

            generatorStates.AppendChild(generatorElement);
        }

        parent.AppendChild(generatorStates);

        XmlElement raceProviderPlugins = document.CreateElement("RaceProviderPlugins");
        foreach (RaceProviderSettings raceProvider in settings.RaceProvider)
        {
            XmlElement raceProviderElement = raceProvider.ToXml(document);
            raceProviderPlugins.AppendChild(raceProviderElement);
        }

        parent.AppendChild(raceProviderPlugins);

        XmlElement autoSplittersActive = document.CreateElement("ActiveAutoSplitters");
        foreach (string splitter in settings.ActiveAutoSplitters)
        {
            CreateSetting(document, autoSplittersActive, "AutoSplitter", splitter);
        }

        parent.AppendChild(autoSplittersActive);

        AddDriftToSettings(document, parent);

        document.Save(stream);
    }

    public static void AddDriftToSettings(XmlDocument document, XmlElement parent)
    {
        XmlElement element = document.CreateElement("TimerDrift");
        byte[] data = BitConverter.GetBytes(TimeStamp.NewDrift);
        XmlCDataSection cdata = document.CreateCDataSection(Convert.ToBase64String(data));
        element.InnerXml = cdata.OuterXml;
        parent.AppendChild(element);
    }
}
