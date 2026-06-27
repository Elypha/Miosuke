using System.Globalization;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Components;

namespace Miosuke.UiHelper;

public readonly record struct DefaultedControlResult(
    bool Changed,
    bool Reset,
    bool DeactivatedAfterEdit)
{
    public bool ValueChanged => Changed || Reset;
}

public static class SettingsControls
{
    private const float GroupedChildLabelIndent = 26f;
    private const float GroupedChildConnectorThickness = 3f;

    public static readonly Vector4 ModifiedInputBorderColour = Ui.HslaToDecimal(200, 0.85, 0.85, 0.35);
    public static readonly Vector4 ManagedControlBackgroundColour = Ui.HslaToDecimal(25, 0.55, 0.77, 0.30);
    public static readonly Vector4 ManagedControlBackgroundHoveredColour = Ui.HslaToDecimal(25, 0.55, 0.82, 0.36);
    public static readonly Vector4 ManagedControlBackgroundActiveColour = Ui.HslaToDecimal(25, 0.55, 0.87, 0.42);

    public static ImRaii.ColorDisposable PushManagedControlBackground(bool enabled = true) =>
        ImRaii.PushColor(ImGuiCol.FrameBg, ManagedControlBackgroundColour, enabled)
            .Push(ImGuiCol.FrameBgHovered, ManagedControlBackgroundHoveredColour, enabled)
            .Push(ImGuiCol.FrameBgActive, ManagedControlBackgroundActiveColour, enabled);

    public static void WithManagedControlBackground(Action draw, bool enabled = true)
    {
        using var background = PushManagedControlBackground(enabled);
        draw();
    }

    public static T WithManagedControlBackground<T>(Func<T> draw, bool enabled = true)
    {
        using var background = PushManagedControlBackground(enabled);
        return draw();
    }

    public static bool ManagedCheckbox(string id, ref bool value)
    {
        using var background = PushManagedControlBackground();
        return ImGui.Checkbox(id, ref value);
    }

    public static void DrawUiColourKeyHelpMarker()
    {
        ImGuiComponents.HelpMarker("Check Dalamud /xldata -> UIColor -> Row ID. It's a 0-based non-negative integer.");
    }

    public static bool ShortcutControls(
        string checkboxId,
        ref bool enabled,
        HotkeyEditor editor,
        string hotkeyId,
        ref VirtualKey[] hotkey,
        string help,
        float width = 160f)
    {
        var changed = ManagedCheckbox(checkboxId, ref enabled);

        ImGui.SameLine();
        using (ImRaii.Disabled(!enabled))
        {
            using var background = PushManagedControlBackground();
            var result = editor.Draw(hotkeyId, ref hotkey, width);
            changed |= result.Changed;
        }

        if (!string.IsNullOrWhiteSpace(help))
        {
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(help);
        }

        return changed;
    }

    public static void DrawGroupedChildLabel(
        AlignedSettingsLayout.Scope layout,
        string label,
        bool enabled,
        bool last)
    {
        DrawGroupedChildConnector(last);
        var cursorX = ImGui.GetCursorPosX();
        ImGui.SetCursorPosX(cursorX + GroupedChildLabelIndent);
        using (ImRaii.Disabled(!enabled))
        {
            layout.DrawLabel(label, Ui.ColourWhiteDim);
        }

        ImGui.SetCursorPosX(cursorX);
    }

    public static DefaultedControlResult DefaultedInputInt(
        string id,
        ref int value,
        int defaultValue,
        float width = 160f,
        bool managed = false)
    {
        var modified = value != defaultValue;
        ImGui.SetNextItemWidth(width);
        using var background = PushManagedControlBackground(managed);
        using var colour = ImRaii.PushColor(ImGuiCol.Border, ModifiedInputBorderColour, modified);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1f, modified);
        var changed = ImGui.InputInt(id, ref value);
        var deactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit();
        var reset = DrawDefaultValueInteractions(id, modified, FormatDefaultValue(defaultValue), ref value, defaultValue);
        return new DefaultedControlResult(changed, reset, deactivatedAfterEdit);
    }

    public static DefaultedControlResult DefaultedInputFloat(
        string id,
        ref float value,
        float defaultValue,
        float width = 160f,
        bool managed = false)
    {
        var modified = !NearlyEqual(value, defaultValue);
        ImGui.SetNextItemWidth(width);
        using var background = PushManagedControlBackground(managed);
        using var colour = ImRaii.PushColor(ImGuiCol.Border, ModifiedInputBorderColour, modified);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1f, modified);
        var changed = ImGui.InputFloat(id, ref value);
        var deactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit();
        var reset = DrawDefaultValueInteractions(id, modified, FormatDefaultValue(defaultValue), ref value, defaultValue);
        return new DefaultedControlResult(changed, reset, deactivatedAfterEdit);
    }

    public static DefaultedControlResult DefaultedInputUShort(
        string id,
        ref ushort value,
        ushort defaultValue,
        float width = 160f,
        bool managed = false)
    {
        var modified = value != defaultValue;
        ImGui.SetNextItemWidth(width);
        using var background = PushManagedControlBackground(managed);
        using var colour = ImRaii.PushColor(ImGuiCol.Border, ModifiedInputBorderColour, modified);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1f, modified);
        var changed = ImGui.InputUShort(id, ref value);
        var deactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit();
        var reset = DrawDefaultValueInteractions(id, modified, FormatDefaultValue(defaultValue), ref value, defaultValue);
        return new DefaultedControlResult(changed, reset, deactivatedAfterEdit);
    }

    public static DefaultedControlResult DefaultedCheckbox(string id, ref bool value, bool defaultValue)
    {
        var modified = value != defaultValue;
        using var colour = ImRaii.PushColor(ImGuiCol.Border, ModifiedInputBorderColour, modified);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1f, modified);
        var changed = ImGui.Checkbox(id, ref value);
        var deactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit();
        var reset = DrawDefaultValueInteractions(id, modified, FormatDefaultValue(defaultValue), ref value, defaultValue);
        return new DefaultedControlResult(changed, reset, deactivatedAfterEdit);
    }

    public static DefaultedControlResult DefaultedInputText(
        string id,
        ref string value,
        string defaultValue,
        int maxLength,
        string hint = "",
        float width = 160f,
        bool managed = false)
    {
        var modified = !string.Equals(value, defaultValue, StringComparison.Ordinal);
        ImGui.SetNextItemWidth(width);
        using var background = PushManagedControlBackground(managed);
        using var colour = ImRaii.PushColor(ImGuiCol.Border, ModifiedInputBorderColour, modified);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1f, modified);
        var changed = string.IsNullOrEmpty(hint)
            ? ImGui.InputText(id, ref value, maxLength)
            : ImGui.InputTextWithHint(id, hint, ref value, maxLength);
        var deactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit();
        var reset = DrawDefaultValueInteractions(id, modified, FormatDefaultValue(defaultValue), ref value, defaultValue);
        return new DefaultedControlResult(changed, reset, deactivatedAfterEdit);
    }

    public static DefaultedControlResult DefaultedInputVector2(
        string id,
        ref Vector2 value,
        Vector2 defaultValue,
        float width = 220f,
        bool managed = false)
    {
        var modified = !NearlyEqual(value.X, defaultValue.X) || !NearlyEqual(value.Y, defaultValue.Y);
        ImGui.SetNextItemWidth(width);
        using var background = PushManagedControlBackground(managed);
        using var colour = ImRaii.PushColor(ImGuiCol.Border, ModifiedInputBorderColour, modified);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1f, modified);
        var changed = ImGui.InputFloat2(id, ref value);
        var deactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit();
        var reset = DrawDefaultValueInteractions(id, modified, FormatDefaultValue(defaultValue), ref value, defaultValue);
        return new DefaultedControlResult(changed, reset, deactivatedAfterEdit);
    }

    public static DefaultedControlResult DefaultedInputVector3(
        string id,
        ref Vector3 value,
        Vector3 defaultValue,
        float width = 260f,
        bool managed = false)
    {
        var modified = !NearlyEqual(value.X, defaultValue.X)
            || !NearlyEqual(value.Y, defaultValue.Y)
            || !NearlyEqual(value.Z, defaultValue.Z);
        ImGui.SetNextItemWidth(width);
        using var background = PushManagedControlBackground(managed);
        using var colour = ImRaii.PushColor(ImGuiCol.Border, ModifiedInputBorderColour, modified);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1f, modified);
        var changed = ImGui.InputFloat3(id, ref value);
        var deactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit();
        var reset = DrawDefaultValueInteractions(id, modified, FormatDefaultValue(defaultValue), ref value, defaultValue);
        return new DefaultedControlResult(changed, reset, deactivatedAfterEdit);
    }

    public static DefaultedControlResult DefaultedInputVector4(
        string id,
        ref Vector4 value,
        Vector4 defaultValue,
        float width = 300f,
        bool managed = false)
    {
        var modified = !NearlyEqual(value.X, defaultValue.X)
            || !NearlyEqual(value.Y, defaultValue.Y)
            || !NearlyEqual(value.Z, defaultValue.Z)
            || !NearlyEqual(value.W, defaultValue.W);
        ImGui.SetNextItemWidth(width);
        using var background = PushManagedControlBackground(managed);
        using var colour = ImRaii.PushColor(ImGuiCol.Border, ModifiedInputBorderColour, modified);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1f, modified);
        var changed = ImGui.InputFloat4(id, ref value);
        var deactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit();
        var reset = DrawDefaultValueInteractions(id, modified, FormatDefaultValue(defaultValue), ref value, defaultValue);
        return new DefaultedControlResult(changed, reset, deactivatedAfterEdit);
    }

    private static void DrawGroupedChildConnector(bool last)
    {
        const float connectorXOffset = 10f;
        const float connectorHorizontalEnd = GroupedChildLabelIndent - 6f;
        var drawList = ImGui.GetWindowDrawList();
        var style = ImGui.GetStyle();
        var pos = ImGui.GetCursorScreenPos();
        var frameHeight = ImGui.GetFrameHeight();
        var x = pos.X + connectorXOffset;
        var yMid = pos.Y + (frameHeight * 0.5f);
        var yTop = pos.Y - style.CellPadding.Y;
        var yBottom = last ? yMid : pos.Y + frameHeight + style.CellPadding.Y;
        var colour = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.38f));

        drawList.AddLine(new Vector2(x, yTop), new Vector2(x, yBottom), colour, GroupedChildConnectorThickness);
        drawList.AddLine(new Vector2(x, yMid), new Vector2(pos.X + connectorHorizontalEnd, yMid), colour, GroupedChildConnectorThickness);
    }

    private static bool DrawDefaultValueInteractions<T>(
        string id,
        bool modified,
        string defaultText,
        ref T value,
        T defaultValue)
    {
        if (modified && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"Default: {defaultText}");
        }

        var reset = false;
        if (ImGui.BeginPopupContextItem($"{id}DefaultValueContext"))
        {
            using (ImRaii.Disabled(!modified))
            {
                if (ImGui.MenuItem("Reset to default"))
                {
                    value = defaultValue;
                    reset = true;
                }
            }

            ImGui.EndPopup();
        }

        return reset;
    }

    private static bool NearlyEqual(float left, float right) => Math.Abs(left - right) < 0.0001f;

    private static string FormatDefaultValue(int value) => value.ToString(CultureInfo.InvariantCulture);
    private static string FormatDefaultValue(ushort value) => value.ToString(CultureInfo.InvariantCulture);
    private static string FormatDefaultValue(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    private static string FormatDefaultValue(string value) => string.IsNullOrEmpty(value) ? "(empty)" : value;
    private static string FormatDefaultValue(bool value) => value ? "checked" : "unchecked";
    private static string FormatDefaultValue(Vector2 value) => $"{FormatDefaultValue(value.X)}, {FormatDefaultValue(value.Y)}";
    private static string FormatDefaultValue(Vector3 value) => $"{FormatDefaultValue(value.X)}, {FormatDefaultValue(value.Y)}, {FormatDefaultValue(value.Z)}";
    private static string FormatDefaultValue(Vector4 value) => $"{FormatDefaultValue(value.X)}, {FormatDefaultValue(value.Y)}, {FormatDefaultValue(value.Z)}, {FormatDefaultValue(value.W)}";
}
