using Dalamud.Game.ClientState.Keys;

namespace Miosuke.UiHelper;

public static class SettingsRows
{
    public static DefaultedControlResult DefaultedInputInt(
        AlignedSettingsLayout.Scope layout,
        string label,
        string id,
        ref int value,
        int defaultValue,
        float width = 160f,
        bool managed = false,
        Vector4? labelColour = null,
        Action? onChanged = null)
    {
        layout.BeginRow(label, labelColour ?? Ui.ColourWhiteDim);
        return InvokeOnValueChanged(SettingsControls.DefaultedInputInt(id, ref value, defaultValue, width, managed), onChanged);
    }

    public static DefaultedControlResult DefaultedInputFloat(
        AlignedSettingsLayout.Scope layout,
        string label,
        string id,
        ref float value,
        float defaultValue,
        float width = 160f,
        bool managed = false,
        Vector4? labelColour = null,
        Action? onChanged = null)
    {
        layout.BeginRow(label, labelColour ?? Ui.ColourWhiteDim);
        return InvokeOnValueChanged(SettingsControls.DefaultedInputFloat(id, ref value, defaultValue, width, managed), onChanged);
    }

    public static DefaultedControlResult DefaultedInputUShort(
        AlignedSettingsLayout.Scope layout,
        string label,
        string id,
        ref ushort value,
        ushort defaultValue,
        float width = 160f,
        bool managed = false,
        Vector4? labelColour = null,
        Action? onChanged = null)
    {
        layout.BeginRow(label, labelColour ?? Ui.ColourWhiteDim);
        return InvokeOnValueChanged(SettingsControls.DefaultedInputUShort(id, ref value, defaultValue, width, managed), onChanged);
    }

    public static DefaultedControlResult DefaultedInputText(
        AlignedSettingsLayout.Scope layout,
        string label,
        string id,
        ref string value,
        string defaultValue,
        int maxLength,
        string hint = "",
        float width = 160f,
        bool managed = false,
        Vector4? labelColour = null,
        Action? onChanged = null)
    {
        layout.BeginRow(label, labelColour ?? Ui.ColourWhiteDim);
        return InvokeOnValueChanged(SettingsControls.DefaultedInputText(id, ref value, defaultValue, maxLength, hint, width, managed), onChanged);
    }

    public static DefaultedControlResult DefaultedInputVector2(
        AlignedSettingsLayout.Scope layout,
        string label,
        string id,
        ref Vector2 value,
        Vector2 defaultValue,
        float width = 220f,
        bool managed = false,
        Vector4? labelColour = null,
        Action? onChanged = null)
    {
        layout.BeginRow(label, labelColour ?? Ui.ColourWhiteDim);
        return InvokeOnValueChanged(SettingsControls.DefaultedInputVector2(id, ref value, defaultValue, width, managed), onChanged);
    }

    public static DefaultedControlResult DefaultedInputVector3(
        AlignedSettingsLayout.Scope layout,
        string label,
        string id,
        ref Vector3 value,
        Vector3 defaultValue,
        float width = 260f,
        bool managed = false,
        Vector4? labelColour = null,
        Action? onChanged = null)
    {
        layout.BeginRow(label, labelColour ?? Ui.ColourWhiteDim);
        return InvokeOnValueChanged(SettingsControls.DefaultedInputVector3(id, ref value, defaultValue, width, managed), onChanged);
    }

    public static DefaultedControlResult DefaultedInputVector4(
        AlignedSettingsLayout.Scope layout,
        string label,
        string id,
        ref Vector4 value,
        Vector4 defaultValue,
        float width = 300f,
        bool managed = false,
        Vector4? labelColour = null,
        Action? onChanged = null)
    {
        layout.BeginRow(label, labelColour ?? Ui.ColourWhiteDim);
        return InvokeOnValueChanged(SettingsControls.DefaultedInputVector4(id, ref value, defaultValue, width, managed), onChanged);
    }

    public static DefaultedControlResult DefaultedCheckbox(
        AlignedSettingsLayout.Scope layout,
        string label,
        string id,
        ref bool value,
        bool defaultValue,
        Vector4? labelColour = null,
        Action? onChanged = null)
    {
        layout.BeginRow(label, labelColour ?? Ui.ColourWhiteDim);
        var result = SettingsControls.DefaultedCheckbox(id, ref value, defaultValue);
        if (result.ValueChanged)
        {
            onChanged?.Invoke();
        }

        return result;
    }

    public static bool ManagedCheckbox(
        AlignedSettingsLayout.Scope layout,
        string label,
        string id,
        ref bool value,
        Vector4? labelColour = null,
        Action<bool>? onChanged = null)
    {
        layout.BeginRow(label, labelColour ?? Ui.ColourWhiteDim);
        var changed = SettingsControls.ManagedCheckbox(id, ref value);
        if (changed)
        {
            onChanged?.Invoke(value);
        }

        return changed;
    }

    public static bool ShortcutControls(
        AlignedSettingsLayout.Scope layout,
        string label,
        string checkboxId,
        ref bool enabled,
        HotkeyEditor hotkeyEditor,
        string hotkeyId,
        ref VirtualKey[] hotkey,
        string help,
        float width = 160f,
        Vector4? labelColour = null,
        Action? onChanged = null)
    {
        layout.BeginRow(label, labelColour ?? Ui.ColourWhiteDim);
        var changed = SettingsControls.ShortcutControls(checkboxId, ref enabled, hotkeyEditor, hotkeyId, ref hotkey, help, width);
        if (changed)
        {
            onChanged?.Invoke();
        }

        return changed;
    }

    private static DefaultedControlResult InvokeOnValueChanged(DefaultedControlResult result, Action? onChanged)
    {
        if (result.ValueChanged)
        {
            onChanged?.Invoke();
        }

        return result;
    }
}
