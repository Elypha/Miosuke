using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;

namespace Miosuke;

public class ConfigUiHotkey(VirtualKey[] userConfigHotkey, System.Action saveConfig)
{
    public void Dispose()
    {
    }

    // -------------------------------- module --------------------------------
    private bool doSetInputFocused = false;
    private bool isEditingHotkey = false;
    private bool isInputActive = false;
    public List<VirtualKey> UserHotkeyList = [];
    public string UserHotkeyString = "";


    public void DrawConfigUi(string uniqueName, uint inputWidth = 150)
    {
        // get user hotkey
        UserHotkeyString = GetHotkeyString(userConfigHotkey);

        // update user hotkey from user input
        if (isEditingHotkey)
        {
            if (ImGui.GetIO().KeyAlt && !UserHotkeyList.Contains(VirtualKey.MENU)) UserHotkeyList.Add(VirtualKey.MENU);
            if (ImGui.GetIO().KeyShift && !UserHotkeyList.Contains(VirtualKey.SHIFT)) UserHotkeyList.Add(VirtualKey.SHIFT);
            if (ImGui.GetIO().KeyCtrl && !UserHotkeyList.Contains(VirtualKey.CONTROL)) UserHotkeyList.Add(VirtualKey.CONTROL);

            for (var i = 0; i < ImGui.GetIO().KeysDown.Count && i < 160; i++)
            {
                if (ImGui.GetIO().KeysDown[i])
                {
                    var vkey = (VirtualKey)i;

                    if (vkey == VirtualKey.ESCAPE)
                    {
                        CancelEdit();
                        break;
                    }

                    if (!UserHotkeyList.Contains(vkey))
                    {
                        UserHotkeyList.Add(vkey);
                    }
                }
            }

            UserHotkeyList.Sort();
            UserHotkeyString = GetHotkeyString(UserHotkeyList);
        }

        // draw hotkey input bar
        DrawHotkeyInput(uniqueName, inputWidth);

        // buttons and actions
        ImGui.SameLine();
        if (!isEditingHotkey)
        {
            if (ImGui.Button($"Edit##{uniqueName}-button-edit"))
            {
                StartEdit();
            }
        }
        else
        {
            if (UserHotkeyList.Count > 0)
            {
                if (ImGui.Button($"Save##{uniqueName}-button-save"))
                {
                    SaveEdit();
                }
            }
            else
            {
                if (ImGui.Button($"Cancel##{uniqueName}-button-cancel"))
                {
                    CancelEdit();
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
            "Click 'Edit' to set a new hotkey.\nClick 'Save' or a blank area to save.\nPress ESC on your keyboard to cancel."
        );
    }

    private void DrawHotkeyInput(string uniqueName, uint inputWidth)
    {
        // set style
        if (isEditingHotkey)
        {
            ImGui.PushStyleColor(ImGuiCol.Border, 0xFF00A5FF);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);
        }

        ImGui.SetNextItemWidth(inputWidth);
        ImGui.InputText($"##{uniqueName}-input-hotkey", ref UserHotkeyString, 100, ImGuiInputTextFlags.ReadOnly);

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

    private void StartEdit()
    {
        isEditingHotkey = true;
    }

    private void SaveEdit()
    {
        isEditingHotkey = false;

        if (UserHotkeyList.Count > 0) userConfigHotkey = [.. UserHotkeyList];
        saveConfig();

        UserHotkeyList.Clear();
        Service.PluginLog.Info($"Hotkey set to {GetHotkeyString(userConfigHotkey)}");
    }

    private void CancelEdit()
    {
        isEditingHotkey = false;

        UserHotkeyList.Clear();
        Service.PluginLog.Info("Hotkey edit cancelled");
    }

    public static string GetHotkeyString(IEnumerable<VirtualKey> hotkey)
    {
        return string.Join("+", hotkey.Select(k => k.GetKeyName()));
    }
}