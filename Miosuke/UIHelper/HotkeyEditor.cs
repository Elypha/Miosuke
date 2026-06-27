using Dalamud.Game.ClientState.Keys;
using Miosuke.Extensions;

namespace Miosuke.UiHelper;

public readonly record struct HotkeyEditorResult(
    bool Changed,
    bool StartedEditing,
    bool Cancelled,
    bool IsEditing);

public sealed class HotkeyEditor
{
    private bool _focusInputNextFrame;
    private bool _isEditing;
    private List<VirtualKey> _capturedKeys = [];

    public HotkeyEditorResult Draw(string id, ref VirtualKey[] hotkey, float width = 160f)
    {
        using var idScope = ImRaii.PushId(id);

        var changed = false;
        var started = false;
        var cancelled = _isEditing && CapturePressedKeys();
        var hotkeyText = (_isEditing ? _capturedKeys : hotkey.AsEnumerable()).HotkeyToString();
        var buttonWidth = ButtonWidth();
        var inputWidth = Math.Max(40f, width - ImGui.GetStyle().ItemSpacing.X - buttonWidth);

        DrawInput(ref hotkeyText, inputWidth, out var inputActive);

        ImGui.SameLine();
        if (!_isEditing)
        {
            if (ImGui.Button("Edit", new Vector2(buttonWidth, 0)))
            {
                _isEditing = true;
                _capturedKeys = [];
                _focusInputNextFrame = true;
                started = true;
            }
        }
        else if (_capturedKeys.Count > 0)
        {
            if (ImGui.Button("Save", new Vector2(buttonWidth, 0)))
            {
                hotkey = Normalise(_capturedKeys).ToArray();
                _isEditing = false;
                _focusInputNextFrame = false;
                changed = true;
            }
        }
        else if (ImGui.Button("Cancel", new Vector2(buttonWidth, 0)))
        {
            _isEditing = false;
            _focusInputNextFrame = false;
            cancelled = true;
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                _isEditing
                    ? "Press the new shortcut, then click Save. Press Esc to cancel."
                    : "Click to set a new shortcut.");
        }

        if (_isEditing && !ImGui.IsItemFocused() && !inputActive)
        {
            _focusInputNextFrame = true;
        }

        return new HotkeyEditorResult(changed, started, cancelled, _isEditing);
    }

    private static float ButtonWidth()
    {
        var textWidth = Math.Max(
            ImGui.CalcTextSize("Cancel").X,
            Math.Max(ImGui.CalcTextSize("Save").X, ImGui.CalcTextSize("Edit").X));
        return textWidth + (ImGui.GetStyle().FramePadding.X * 2f);
    }

    private static IEnumerable<VirtualKey> Normalise(IEnumerable<VirtualKey> keys) => keys
        .Distinct()
        .OrderBy(static key => key switch
        {
            VirtualKey.CONTROL => 0,
            VirtualKey.MENU => 1,
            VirtualKey.SHIFT => 2,
            _ => 1000 + (int)key,
        });

    private bool CapturePressedKeys()
    {
        var io = ImGui.GetIO();
        if (IsKeyDown(io, VirtualKey.ESCAPE))
        {
            _isEditing = false;
            _focusInputNextFrame = false;
            return true;
        }

        AddModifier(io.KeyCtrl, VirtualKey.CONTROL);
        AddModifier(io.KeyAlt, VirtualKey.MENU);
        AddModifier(io.KeyShift, VirtualKey.SHIFT);

        for (var keyIndex = 0; keyIndex < io.KeysDown.Length && keyIndex < 160; keyIndex++)
        {
            if (!io.KeysDown[keyIndex]) continue;

            var key = (VirtualKey)keyIndex;
            if (key is VirtualKey.CONTROL or VirtualKey.MENU or VirtualKey.SHIFT or VirtualKey.ESCAPE) continue;

            if (!_capturedKeys.Contains(key))
            {
                _capturedKeys.Add(key);
            }
        }

        _capturedKeys = Normalise(_capturedKeys).ToList();
        return false;
    }

    private static bool IsKeyDown(ImGuiIOPtr io, VirtualKey key)
    {
        var index = (int)key;
        return index < io.KeysDown.Length && io.KeysDown[index];
    }

    private void AddModifier(bool pressed, VirtualKey key)
    {
        if (pressed && !_capturedKeys.Contains(key))
        {
            _capturedKeys.Add(key);
        }
    }

    private void DrawInput(ref string hotkeyText, float width, out bool inputActive)
    {
        using var colour = ImRaii.PushColor(ImGuiCol.Border, Ui.HslaToDecimal(200, 0.85, 0.65, 0.75), _isEditing);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 2f, _isEditing);

        ImGui.SetNextItemWidth(width);
        ImGui.InputText("##Value", ref hotkeyText, 100, ImGuiInputTextFlags.ReadOnly);

        if (_focusInputNextFrame)
        {
            _focusInputNextFrame = false;
            ImGui.SetKeyboardFocusHere(-1);
        }

        inputActive = ImGui.IsItemActive();
    }
}
