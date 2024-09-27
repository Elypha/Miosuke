using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using System.Collections.Generic;
using System.Linq;


namespace Miosuke.Messages;

public class Toast
{    public static void ToastNormal(string text)
    {
        Service.Toasts.ShowNormal(text);
    }
}
