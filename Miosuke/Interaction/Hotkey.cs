using Dalamud.Game.ClientState.Keys;
using System.Linq;
using System;
using System.Collections.Generic;


namespace Miosuke;

public class Hotkey
{
    // -------------------------------- hotkey methods --------------------------------
    public static List<VirtualKey> GetActiveKeys()
    {
        var activeKeys = new List<VirtualKey>();
        foreach (var vk in Service.KeyState.GetValidVirtualKeys())
        {
            if (Service.KeyState[vk]) activeKeys.Add(vk);
        }
        return activeKeys;
    }

    public static bool IsActive(VirtualKey[] keys, bool strict = false)
    {
        // check if all keys are active
        foreach (var vk in keys)
        {
            if (!Service.KeyState.IsVirtualKeyValid(vk)) continue;

            if (!Service.KeyState[vk]) return false;
        }

        // if strict, check if all active keys are in the list
        if (strict)
        {
            foreach (var vk in GetActiveKeys())
            {
                if (!keys.Contains(vk)) return false;
            }
        }

        return true;
    }
}
