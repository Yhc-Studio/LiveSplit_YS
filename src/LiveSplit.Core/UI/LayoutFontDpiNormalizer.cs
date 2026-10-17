using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;
using System.Drawing;

namespace LiveSplit.UI;

/// <summary>
/// Provides DPI-stable handling for legacy layout fonts that were serialized
/// with GraphicsUnit.Point.
///
/// Modern LiveSplit layout geometry and default fonts are pixel based. A Point
/// font is DPI dependent (12pt is 16 px at 96 DPI, 24 px at 144 DPI), while a
/// Pixel font is stable. Stage 4.2 converts legacy Point fonts to equivalent
/// Pixel fonts in memory using a per-layout reference DPI learned by the host.
/// The .lsl does not need to be marked dirty or saved just because of this
/// compatibility conversion.
/// </summary>
public static class LayoutFontDpiNormalizer
{
    public const float DefaultDpi = 96f;

    /// <summary>
    /// Returns true if the layout still contains at least one legacy Point-unit
    /// font in its global settings or component font overrides.
    /// </summary>
    public static bool HasLegacyPointFonts(ILayout layout)
    {
        if (layout?.Settings == null)
        {
            return false;
        }

        if (IsLegacyPointFont(layout.Settings.TimerFont)
            || IsLegacyPointFont(layout.Settings.TimesFont)
            || IsLegacyPointFont(layout.Settings.TextFont))
        {
            return true;
        }

        foreach (ILayoutComponent layoutComponent in layout.LayoutComponents)
        {
            if (layoutComponent is not LayoutComponent concrete)
            {
                continue;
            }

            FontOverrides overrides = concrete.FontOverrides;
            if (overrides == null)
            {
                continue;
            }

            if (IsLegacyPointFont(overrides.TimerFont)
                || IsLegacyPointFont(overrides.TimesFont)
                || IsLegacyPointFont(overrides.TextFont))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Converts all legacy Point fonts used by a layout to equivalent Pixel
    /// fonts at <paramref name="referenceDpi"/>.
    ///
    /// When <paramref name="markLayoutChanged"/> is false (the Stage 4.2
    /// runtime-adaptive path), the conversion is rendering-only compatibility
    /// work and does not cause LiveSplit to prompt the user to save the layout.
    /// </summary>
    public static bool NormalizeLegacyPointFonts(
        ILayout layout,
        float referenceDpi,
        bool markLayoutChanged = false)
    {
        if (layout?.Settings == null)
        {
            return false;
        }

        referenceDpi = SanitizeDpi(referenceDpi);
        bool changed = false;

        layout.Settings.TimerFont = NormalizeFont(layout.Settings.TimerFont, referenceDpi, ref changed);
        layout.Settings.TimesFont = NormalizeFont(layout.Settings.TimesFont, referenceDpi, ref changed);
        layout.Settings.TextFont = NormalizeFont(layout.Settings.TextFont, referenceDpi, ref changed);

        foreach (ILayoutComponent layoutComponent in layout.LayoutComponents)
        {
            if (layoutComponent is not LayoutComponent concrete)
            {
                continue;
            }

            FontOverrides overrides = concrete.FontOverrides;
            if (overrides == null)
            {
                continue;
            }

            overrides.TimerFont = NormalizeFont(overrides.TimerFont, referenceDpi, ref changed);
            overrides.TimesFont = NormalizeFont(overrides.TimesFont, referenceDpi, ref changed);
            overrides.TextFont = NormalizeFont(overrides.TextFont, referenceDpi, ref changed);
        }

        if (changed && markLayoutChanged)
        {
            layout.HasChanged = true;
        }

        return changed;
    }

    /// <summary>
    /// Returns a Pixel equivalent of a Point font at the supplied DPI. Pixel
    /// fonts and uncommon unit types are returned unchanged.
    /// </summary>
    public static Font ToPixelEquivalent(Font font, float referenceDpi)
    {
        bool ignored = false;
        return NormalizeFont(font, SanitizeDpi(referenceDpi), ref ignored);
    }

    private static bool IsLegacyPointFont(Font font)
    {
        return font != null && font.Unit == GraphicsUnit.Point;
    }

    private static Font NormalizeFont(Font font, float referenceDpi, ref bool changed)
    {
        if (font == null || font.Unit == GraphicsUnit.Pixel)
        {
            return font;
        }

        // Point is the historically problematic unit used by older layouts and
        // older CustomFontDialog behavior. Leave rarer units untouched rather
        // than guessing at their intended semantics.
        if (font.Unit != GraphicsUnit.Point)
        {
            return font;
        }

        float pixelSize = font.Size * referenceDpi / 72f;
        if (pixelSize <= 0f || float.IsNaN(pixelSize) || float.IsInfinity(pixelSize))
        {
            return font;
        }

        changed = true;
        return new Font(
            font.FontFamily,
            pixelSize,
            font.Style,
            GraphicsUnit.Pixel,
            font.GdiCharSet,
            font.GdiVerticalFont);
    }

    private static float SanitizeDpi(float dpi)
    {
        return dpi > 0f && !float.IsNaN(dpi) && !float.IsInfinity(dpi)
            ? dpi
            : DefaultDpi;
    }
}
