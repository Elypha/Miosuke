using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Components;
using ImGuiNET;
using Miosuke.Action;
using System.Collections.Generic;
using System.Linq;


namespace Miosuke.UiHelper;

public class HotkeyUi
{
    public HotkeyUi()
    {
    }


    // -------------------------------- module --------------------------------
    public bool doSetInputFocused = false;
    public bool isEditingHotkey = false;
    public bool isInputActive = false;
    public List<VirtualKey> userHotkeyList = [];
    public string userHotkeyString = "";
    public bool isHotkeyChanged = false;

    public bool DrawConfigUi(string uniqueName, ref VirtualKey[] hotkey, float inputWidth = 150f)
    {
        // get user hotkey
        userHotkeyString = GetHotkeyString(hotkey);

        // update user hotkey from user input
        if (isEditingHotkey)
        {
            if (ImGui.GetIO().KeyAlt && !userHotkeyList.Contains(VirtualKey.MENU)) userHotkeyList.Add(VirtualKey.MENU);
            if (ImGui.GetIO().KeyShift && !userHotkeyList.Contains(VirtualKey.SHIFT)) userHotkeyList.Add(VirtualKey.SHIFT);
            if (ImGui.GetIO().KeyCtrl && !userHotkeyList.Contains(VirtualKey.CONTROL)) userHotkeyList.Add(VirtualKey.CONTROL);

            for (var i = 0; i < ImGui.GetIO().KeysDown.Count && i < 160; i++)
            {
                if (ImGui.GetIO().KeysDown[i])
                {
                    var vkey = (VirtualKey)i;

                    if (vkey == VirtualKey.ESCAPE)
                    {
                        // cancel editing
                        isEditingHotkey = false;
                        break;
                    }

                    if (!userHotkeyList.Contains(vkey))
                    {
                        userHotkeyList.Add(vkey);
                    }
                }
            }

            userHotkeyList.Sort();
            userHotkeyString = GetHotkeyString(userHotkeyList);
        }

        // draw hotkey input bar
        DrawHotkeyInput(uniqueName, inputWidth);

        // buttons and actions
        ImGui.SameLine();
        if (!isEditingHotkey)
        {
            if (ImGui.Button($"Edit##{uniqueName}-button-edit"))
            {
                // start editing
                isEditingHotkey = true;
                userHotkeyList = [];
            }
        }
        else
        {
            if (userHotkeyList.Count > 0)
            {
                // save and stop editing
                if (ImGui.Button($"Save##{uniqueName}-button-save"))
                {
                    isEditingHotkey = false;
                    if (userHotkeyList.Count > 0)
                    {
                        isHotkeyChanged = true;
                        hotkey = [.. userHotkeyList];
                    }
                }
            }
            else
            {
                // if no hotkey, show cancel button, cancel editing
                if (ImGui.Button($"Cancel##{uniqueName}-button-cancel"))
                {
                    isEditingHotkey = false;
                }
            }
        }

        // when editing, if buttons are not being clicked, and if input is not active, set focus
        if (isEditingHotkey && !ImGui.IsItemFocused() && !isInputActive)
        {
            doSetInputFocused = true;
        }

        ImGui.SameLine();
        ImGuiComponents.HelpMarker(
            "Click 'Edit' to set a new hotkey.\n" +
            "Click 'Save' or a blank area to save.\n" +
            "Press ESC on your keyboard to cancel."
        );

        // if changed, return true
        if (isHotkeyChanged)
        {
            isHotkeyChanged = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void DrawHotkeyInput(string uniqueName, float inputWidth)
    {
        // set style
        if (isEditingHotkey)
        {
            ImGui.PushStyleColor(ImGuiCol.Border, 0xFF00A5FF);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);
        }

        ImGui.SetNextItemWidth(inputWidth);
        ImGui.InputText($"##{uniqueName}-input-hotkey", ref userHotkeyString, 100, ImGuiInputTextFlags.ReadOnly);

        // set focus when required
        if (doSetInputFocused)
        {
            doSetInputFocused = false;
            ImGui.SetKeyboardFocusHere(-1);
        }
        isInputActive = ImGui.IsItemActive();

        // reset style
        if (isEditingHotkey)
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar();
        }
    }

    private static string GetHotkeyString(IEnumerable<VirtualKey> hotkey)
    {
        return string.Join("+", hotkey.Select(k => k.GetKeyName()));
    }
}
