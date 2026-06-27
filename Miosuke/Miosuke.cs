using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace Miosuke;

public static class MiosukeHelper
{
    internal static IDalamudPlugin Plugin = null!;
    internal static string PluginNameShort = "Miosuke";
    internal static DalamudLinkPayload? PluginNameShortPayload;

    public static void Init(
        IDalamudPluginInterface pluginInterface,
        IDalamudPlugin plugin,
        string? pluginNameShort = null,
        DalamudLinkPayload? pluginNameShortPayload = null)
    {
        Service.Init(pluginInterface);
        Plugin = plugin;

        if (pluginNameShort is not null) PluginNameShort = pluginNameShort;
        if (pluginNameShortPayload is not null) PluginNameShortPayload = pluginNameShortPayload;
    }

    public static void Dispose()
    {
    }
}
