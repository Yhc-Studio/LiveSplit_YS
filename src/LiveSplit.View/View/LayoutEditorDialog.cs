using LiveSplit.Localization;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using LiveSplit.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LiveSplit.View;

public partial class LayoutEditorDialog : Form
{
    public event EventHandler OrientationSwitched;
    public event EventHandler LayoutResized;
    public event EventHandler LayoutSettingsAssigned;

    public Form Form { get; set; }

    public LiveSplitState CurrentState { get; set; }

    public List<UI.Components.IComponent> ComponentsToDispose { get; set; }
    public List<Image> ImagesToDispose { get; set; }

    protected new ILayout Layout { get; set; }
    protected BindingList<ILayoutComponent> BindingList { get; set; }
    protected float OverallHeight => BindingList.OfType<ILayoutComponent>().Sum(x => x.Component.VerticalHeight);
    protected bool IsVertical
    {
        get => Layout.Mode == LayoutMode.Vertical;
        set => Layout.Mode = value ? LayoutMode.Vertical : LayoutMode.Horizontal;
    }
    protected bool IsHorizontal { get => !IsVertical; set => IsVertical = !value; }
    public LayoutEditorDialog(ILayout layout, LiveSplitState state, Form form)
    {
        InitializeComponent();
        Form = form;
        Layout = layout;
        BindingList = new BindingList<ILayoutComponent>(Layout.LayoutComponents);
        ComponentsToDispose = [];
        ImagesToDispose = [];
        lbxComponents.DataSource = BindingList;
        lbxComponents.DisplayMember = "Component.ComponentName";
        // ComponentName is an internal/stable English identifier used by factories and
        // layout persistence.  Keep that value untouched and localize only the text
        // presented by the ListBox.
        lbxComponents.FormattingEnabled = true;
        lbxComponents.Format += lbxComponents_Format;
        LoadAllComponentsAvailable();

        rdoVertical.Checked = IsVertical;
        rdoHorizontal.Checked = IsHorizontal;
        rdoVertical.CheckedChanged += rdoVertical_CheckedChanged;

        CurrentState = state;
        var itemDragger = new ListBoxItemDragger(lbxComponents, form)
        {
            DragCursor = Cursors.SizeAll
        };
        UiLocalizer.Apply(this, LanguageResolver.ResolveCurrentCultureLanguage());
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        this.MoveIntoOwnerScreen();
    }

    private void rdoVertical_CheckedChanged(object sender, EventArgs e)
    {
        Layout.HasChanged = true;
        IsVertical = rdoVertical.Checked;
        IsHorizontal = !rdoVertical.Checked;
        OrientationSwitched?.Invoke(this, null);
    }

    private void AddComponent(IComponentFactory factory)
    {
        try
        {
            KeyValuePair<string, IComponentFactory> componentFactory = ComponentManager.ComponentFactories.FirstOrDefault(x => x.Value.ComponentName == factory.ComponentName);
            LayoutComponent component = componentFactory.Value == null
                ? new LayoutComponent("", new SeparatorComponent())
                : new LayoutComponent(componentFactory.Key, componentFactory.Value.Create(CurrentState));

            Form.InvokeIfRequired(() =>
            {
                try
                {
                    BindingList.Add(component);
                    lbxComponents.SelectedItem = component;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });

            Layout.HasChanged = true;
            LayoutResized?.Invoke(this, null);
        }
        catch (Exception e)
        {
            Log.Error(e);
            MessageBox.Show(this, UiLocalizer.Translate("The Component could not be loaded."), UiLocalizer.Translate("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void lbxComponents_Format(object sender, ListControlConvertEventArgs e)
    {
        // Do not mutate IComponent.ComponentName.  It is used as a stable identifier
        // by LiveSplit and third-party components.  Formatting changes display only.
        if (e.ListItem is ILayoutComponent layoutComponent)
        {
            string componentName = layoutComponent.Component?.ComponentName;
            if (!string.IsNullOrEmpty(componentName))
            {
                e.Value = UiLocalizer.Translate(componentName);
            }
        }
        else if (e.Value is string value && !string.IsNullOrEmpty(value))
        {
            e.Value = UiLocalizer.Translate(value);
        }
    }

    private void AddComponentFactory(IComponentFactory factory, ToolStripMenuItem menuItem)
    {
        var item = new ToolStripMenuItem(UiLocalizer.Translate(factory.ComponentName));
        item.Click += (s, e) => AddComponent(factory);
        item.ToolTipText = string.IsNullOrEmpty(factory.Description)
            ? factory.Description
            : UiLocalizer.Translate(factory.Description);
        menuItem.DropDownItems.Add(item);
    }

    private void AddGroup(ComponentCategory groupCategory, IEnumerable<IComponentFactory> factories)
    {
        var groupItem = new ToolStripMenuItem(UiLocalizer.Translate(groupCategory.ToString()));
        foreach (IComponentFactory factory in factories)
        {
            AddComponentFactory(factory, groupItem);
        }

        menuAddComponents.Items.Add(groupItem);
    }

    private void LoadAllComponentsAvailable()
    {
        // Stage 4.4: Full component discovery is no longer part of application
        // startup. The layout editor is an explicit point where the complete
        // component catalog is actually needed, so load it on demand here.
        ComponentManager.EnsureAllFactoriesLoaded();
        IEnumerable<string> autosplitters = AutoSplitterFactory.Instance.AutoSplitters != null
            ? AutoSplitterFactory.Instance.AutoSplitters.Where(x => !x.Value.ShowInLayoutEditor).Select(x => x.Value.FileName)
            : new List<string>();
        IOrderedEnumerable<IGrouping<ComponentCategory, IComponentFactory>> groups = ComponentManager.ComponentFactories.Where(x => !autosplitters.Contains(x.Key)).Select(x => x.Value).GroupBy(x => x.Category, x => x).OrderBy(x => x.Key);
        foreach (IGrouping<ComponentCategory, IComponentFactory> group in groups)
        {
            ComponentCategory category = group.Key;
            IEnumerable<IComponentFactory> componentFactories = group;
            if (category == ComponentCategory.Other)
            {
                componentFactories = componentFactories.Prepend(new SeparatorFactory()).OrderBy(x => x.ComponentName);
            }

            AddGroup(category, componentFactories);
        }

        menuAddComponents.Items.Add(new ToolStripSeparator());
        var downloadMore = new ToolStripMenuItem(UiLocalizer.Translate("Download More..."));
        downloadMore.Click += (s, e) => Process.Start("http://livesplit.org/components/");

        menuAddComponents.Items.Add(downloadMore);
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
        menuAddComponents.Show(this, new Point(btnAdd.Width + btnAdd.Location.X, btnAdd.Location.Y));
    }

    private void btnRemove_Click(object sender, EventArgs e)
    {
        if (BindingList.Count > 1)
        {
            Form.InvokeIfRequired(() =>
            {
                try
                {
                    UI.Components.IComponent component = Layout.Components.ElementAt(lbxComponents.SelectedIndex);
                    if (component is IDeactivatableComponent deactivatable)
                    {
                        deactivatable.Activated = false;
                    }

                    ComponentsToDispose.Add(component);
                    BindingList.RemoveAt(lbxComponents.SelectedIndex);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });

            Layout.HasChanged = true;
            LayoutResized?.Invoke(this, null);
        }
    }

    private void btnMoveUp_Click(object sender, EventArgs e)
    {
        if (lbxComponents.SelectedIndex > 0)
        {
            Form.InvokeIfRequired(() =>
            {
                try
                {
                    BindingList.Insert(lbxComponents.SelectedIndex - 1, BindingList[lbxComponents.SelectedIndex]);
                    lbxComponents.SelectedIndex -= 2;
                    BindingList.RemoveAt(lbxComponents.SelectedIndex + 2);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });

            Layout.HasChanged = true;
        }
    }

    private void btnMoveDown_Click(object sender, EventArgs e)
    {
        if (lbxComponents.SelectedIndex < BindingList.Count - 1)
        {
            Form.InvokeIfRequired(() =>
            {
                try
                {
                    BindingList.Insert(lbxComponents.SelectedIndex + 2, BindingList[lbxComponents.SelectedIndex]);
                    BindingList.RemoveAt(lbxComponents.SelectedIndex);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });

            lbxComponents.SelectedIndex += 1;
            Layout.HasChanged = true;
        }
    }

    private void ShowLayoutSettings(UI.Components.IComponent tabControl = null)
    {
        var oldSettings = (Options.LayoutSettings)Layout.Settings.Clone();
        var settingsDialog = new LayoutSettingsDialog(Layout.Settings, Layout, tabControl);
        DialogResult result = settingsDialog.ShowDialog(this);
        //settingsDialog.Dispose();
        if (result == DialogResult.OK)
        {
            if (oldSettings.BackgroundImage != null && oldSettings.BackgroundImage != Layout.Settings.BackgroundImage)
            {
                ImagesToDispose.Add(oldSettings.BackgroundImage);
            }

            Layout.HasChanged = true;
        }
        else if (result == DialogResult.Cancel)
        {
            if (Layout.Settings.BackgroundImage != null && oldSettings.BackgroundImage != Layout.Settings.BackgroundImage)
            {
                Layout.Settings.BackgroundImage.Dispose();
            }

            Layout.Settings.Assign(oldSettings);
            LayoutSettingsAssigned(null, null);
        }

        BindingList.ResetBindings();
    }

    private void btnLayoutSettings_Click(object sender, EventArgs e)
    {
        ShowLayoutSettings();
    }

    private void btnSetSize_Click(object sender, EventArgs e)
    {
        using var setSizeDialog = new SetSizeForm(CurrentState.Form);
        Size oldSize = CurrentState.Form.Size;
        DialogResult result = setSizeDialog.ShowDialog();

        if (result == DialogResult.Cancel)
        {
            CurrentState.Form.Size = oldSize;
        }
    }

    private void lbxComponents_DoubleClick(object sender, EventArgs e)
    {
    }

    private void lbxComponents_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        int index = lbxComponents.IndexFromPoint(e.Location);
        if (index != ListBox.NoMatches)
        {
            object selectedItem = lbxComponents.Items[index];
            try
            {
                UI.Components.IComponent control = ((ILayoutComponent)selectedItem).Component;
                bool hasSettings = control.GetSettingsControl(Layout.Mode) != null;
                bool hasFontOverrides = control.GetType()
                    .GetCustomAttributes(typeof(GlobalFontConsumerAttribute), true).Length > 0;
                if (hasSettings || hasFontOverrides)
                {
                    ShowLayoutSettings(control);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
