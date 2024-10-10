using Dalamud.Game.ClientState.Keys;
using Miosuke.Action;

namespace Miosuke.Extensions;

public static class Extensions
{
    private static readonly Dictionary<VirtualKey, string> VirtualKeyNames = new() {
        { VirtualKey.KEY_0, "0"},
        { VirtualKey.KEY_1, "1"},
        { VirtualKey.KEY_2, "2"},
        { VirtualKey.KEY_3, "3"},
        { VirtualKey.KEY_4, "4"},
        { VirtualKey.KEY_5, "5"},
        { VirtualKey.KEY_6, "6"},
        { VirtualKey.KEY_7, "7"},
        { VirtualKey.KEY_8, "8"},
        { VirtualKey.KEY_9, "9"},
        { VirtualKey.CONTROL, "Ctrl"},
        { VirtualKey.MENU, "Alt"},
        { VirtualKey.SHIFT, "Shift"},
    };

    public static string GetKeyName(this VirtualKey vk) => VirtualKeyNames.TryGetValue(vk, out var value) ? value : vk.ToString();
    public static string HotkeyToString(this IEnumerable<VirtualKey> hotkey) => string.Join("+", hotkey.Select(k => k.GetKeyName()));
}
