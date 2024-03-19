using Dalamud.Game.ClientState.Keys;
using System.Linq;
using System;


namespace Miosuke;

public class Hotkey
{
    // -------------------------------- hotkey methods --------------------------------
    public static bool IsActive(VirtualKey[] keys)
    {
        foreach (var vk in Service.KeyState.GetValidVirtualKeys())
        {
            if (keys.Contains(vk))
            {
                if (!Service.KeyState[vk]) return false;
            }
            else
            {
                if (Service.KeyState[vk]) return false;
            }
        }
        return true;
    }
}
