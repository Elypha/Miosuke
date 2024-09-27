using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using System.Collections.Generic;
using System.Linq;


namespace Miosuke.Messages;

public class Chat
{
    private static readonly ushort orangeColourKey = 557;

    public static void PluginMessage(XivChatType channel, string prefix, List<Payload> payloadList, DalamudLinkPayload? prefixPayload = null, ushort? prefixColourKey = null)
    {
        var payloads = new List<Payload> {
            new UIForegroundPayload(prefixColourKey ?? orangeColourKey),
            new TextPayload(prefix),
            new UIForegroundPayload(0),
        };

        if (prefixPayload is not null)
        {
            payloads.Insert(1, prefixPayload);
            payloads.Insert(3, RawPayload.LinkTerminator);
        }

        Service.Chat.Print(new XivChatEntry
        {
            Message = new SeString(payloads.Concat(payloadList).ToList()),
            Type = channel,
        });
    }

    public static void Message(XivChatType channel, List<Payload> payloadList)
    {
        Service.Chat.Print(new XivChatEntry
        {
            Message = new SeString(payloadList),
            Type = channel,
        });
    }
}
