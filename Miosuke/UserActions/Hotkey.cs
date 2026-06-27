using Dalamud.Game.ClientState.Keys;

namespace Miosuke.UserActions;

public static class Hotkey
{
    private static readonly List<VirtualKey> ValidVirtualKeys = Service.KeyState.GetValidVirtualKeys().ToList();
    public static IEnumerable<VirtualKey> ActiveKeys() => ValidVirtualKeys.Where(vk => Service.KeyState[vk]);

    public static bool IsActive(VirtualKey[] keys, bool strict = false)
    {
        // check if all keys are active
        foreach (var vk in keys)
        {
            if (!Service.KeyState.IsVirtualKeyValid(vk)) continue;
            if (!Service.KeyState[vk]) return false;
        }

        // if strict, check if all active keys are in the list
        return !strict || ActiveKeys().All(vk => keys.Contains(vk));
    }
}
