using Dalamud.Plugin;


namespace Miosuke;

public class MiosukeHelper
{
    public IDalamudPlugin Plugin { get; private set; } = null!;


    public MiosukeHelper(IDalamudPluginInterface pluginInterface, IDalamudPlugin plugin)
    {
        pluginInterface.Create<Svc>();
        Plugin = plugin;
    }

    public void Dispose()
    {
    }
}
