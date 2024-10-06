using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Components;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;


namespace Miosuke.UiHelper;

public static class Ui
{
    // titles
    public static readonly Vector4 ColourTitle = HslaToDecimal(25, 0.75, 0.85, 1.0);
    public static readonly Vector4 ColourSubtitle = HslaToDecimal(175, 0.5, 0.75, 1.0);
    public static readonly Vector4 ColourText = HslaToDecimal(0, 0.0, 0.95, 1.0);

    // colours
    public static readonly Vector4 ColourCyan = HslaToDecimal(200, 0.85, 0.6, 1.0);
    public static readonly Vector4 ColourRedLight = HslaToDecimal(5, 0.75, 0.6, 1.0);
    public static readonly Vector4 ColourKhaki = HslaToDecimal(25, 0.65, 0.75, 1.0);


    // modifiers
    public static readonly Vector4 ColourWhite = HslaToDecimal(0, 0, 1, 1);
    public static readonly Vector4 ColourWhite2 = HslaToDecimal(0, 0, 0.95, 1);
    public static readonly Vector4 ColourWhite3 = HslaToDecimal(0, 0, 0.90, 1);
    public static readonly Vector4 ColourWhite4 = HslaToDecimal(0, 0, 0.85, 1);
    public static readonly Vector4 Alpha = new(1, 1, 1, 0.5f);
    public static readonly Vector4 Alpha2 = new(1, 1, 1, 0.65f);
    public static readonly Vector4 Alpha3 = new(1, 1, 1, 0.80f);
    public static readonly Vector4 Alpha4 = new(1, 1, 1, 0.95f);



    // ffxiv colours
    public static readonly Vector4 ColourHq = HslaToDecimal(45, 0.80, 0.70, 1.0);


    // IMGUI SHORTCUTS

    public static void AlignRight(string text)
    {
        var posX = ImGui.GetCursorPosX()
            + ImGui.GetColumnWidth()
            - ImGui.CalcTextSize(text).X
            - ImGui.GetScrollX()
            - (1 * ImGui.GetStyle().ItemSpacing.X);
        ImGui.SetCursorPosX(posX);
    }


    // FFXIV HELPERS

    public static void RenderSeString(SeString seString)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetStyle().ItemSpacing.X);
        foreach (var payload in seString.Payloads)
        {
            if (payload is TextPayload textPayload)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - ImGui.GetStyle().ItemSpacing.X);
                // show \n as plain text
                var plain_text = textPayload.Text?.Replace("\n", "\\n");
                ImGui.Text(plain_text);
                ImGui.SameLine();
            }
            else if (payload is UIForegroundPayload uiForegroundPayload)
            {
                if (uiForegroundPayload.IsEnabled)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ColourUintToDecimal(uiForegroundPayload.UIColor.UIForeground));
                }
                else
                {
                    ImGui.PopStyleColor();
                }
            }
        }
    }

    private static Vector4 ColourUintToDecimal(uint color)
    {
        var r = (byte)(color >> 24);
        var g = (byte)(color >> 16);
        var b = (byte)(color >> 8);
        var a = (byte)color;

        return new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }


    // COLOUR

    /// <summary>
    /// (255, 255, 255, 1) -> (1, 1, 1, 1)
    /// </summary>
    public static Vector4 RgbaToDecimal(byte r, byte g, byte b, byte a)
    {
        return new Vector4(r / 255f, g / 255f, b / 255f, a / 1f);
    }

    /// <summary>
    /// (360, 1, 1, 1) -> (1, 1, 1, 1)
    /// </summary>
    public static Vector4 HslaToDecimal(double h, double s, double l, double a = 1.0)
    {
        var rgba = HslaToRgba(h, s, l, a);
        return new Vector4(rgba.X / 255f, rgba.Y / 255f, rgba.Z / 255f, rgba.W / 1f);
    }

    /// <summary>
    /// (360, 1, 1, 1) -> (255, 255, 255, 1)
    /// </summary>
    public static Vector4 HslaToRgba(double h, double s, double l, double a = 1.0)
    {
        double v;
        double r, g, b;

        h /= 360.0;

        r = l;   // default to gray
        g = l;
        b = l;
        v = (l <= 0.5) ? (l * (1.0 + s)) : (l + s - l * s);

        if (v > 0)
        {
            double m;
            double sv;
            int sextant;
            double fract, vsf, mid1, mid2;

            m = l + l - v;
            sv = (v - m) / v;
            h *= 6.0;
            sextant = (int)h;
            fract = h - sextant;
            vsf = v * sv * fract;
            mid1 = m + vsf;
            mid2 = v - vsf;

            switch (sextant)
            {
                case 0:
                    r = v;
                    g = mid1;
                    b = m;
                    break;
                case 1:
                    r = mid2;
                    g = v;
                    b = m;
                    break;
                case 2:
                    r = m;
                    g = v;
                    b = mid1;
                    break;
                case 3:
                    r = m;
                    g = mid2;
                    b = v;
                    break;
                case 4:
                    r = mid1;
                    g = m;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = m;
                    b = mid2;
                    break;
            }
        }
        return new Vector4((float)r * 255, (float)g * 255, (float)b * 255, (float)a);
    }
}
