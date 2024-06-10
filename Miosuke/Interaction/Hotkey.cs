using Dalamud.Game.ClientState.Keys;
using System.Linq;
using System;


namespace Miosuke;

public class Hotkey
{
    // -------------------------------- hotkey methods --------------------------------
    public static bool IsActive(VirtualKey[] keys)
    {
        foreach (var vk in keys)
        {
            if (!Service.KeyState.IsVirtualKeyValid(vk)) continue;

            if (!Service.KeyState[vk]) return false;
        }
        return true;
    }
}
