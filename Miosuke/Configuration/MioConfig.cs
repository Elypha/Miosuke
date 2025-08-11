using Miosuke.Messages;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Miosuke.Configuration;

public static class MioConfig
{
    public static string? ConfigDirectoryOverride { get; set; } = null;
    public static string ConfigDirectory
    {
        get => ConfigDirectoryOverride == null ?
            Service.PluginInterface.GetPluginConfigDirectory() :
            Path.Combine(new DirectoryInfo(Service.PluginInterface.GetPluginConfigDirectory()).Parent!.FullName, ConfigDirectoryOverride);
    }
    public static string MainConfigFileName { get; set; } = "main.json";
    public static string MainConfigFile => Path.Combine(ConfigDirectory, MainConfigFileName);
    public static event System.Action? OnSave;
    public static readonly ConfigIo configIo = new();
    public static IMioConfig? Config { get; private set; }



    public static void Setup(string? ConfigDirectoryOverride = null, string? MainConfigFileName = null)
    {
        if (ConfigDirectoryOverride != null) MioConfig.ConfigDirectoryOverride = ConfigDirectoryOverride;
        if (MainConfigFileName != null) MioConfig.MainConfigFileName = MainConfigFileName;
    }

    public static T Init<T>() where T : IMioConfig, new()
    {
        // before load
        Directory.CreateDirectory(ConfigDirectory);

        // load
        Config = LoadConfiguration<T>(MainConfigFile);
        return (T)Config;
    }

    public static void Save()
    {
        if (Config != null) Save(Config, MainConfigFile, false);
    }

    public static void Save(this IMioConfig config, string? path = null, bool isRelativePath = true, bool prettyPrint = true, bool async = true)
    {
        path ??= MainConfigFileName;
        if (isRelativePath) path = Path.Combine(ConfigDirectory, path);

        OnSave?.Invoke();
        var serialized = configIo.Serialize(config, prettyPrint);

        void Write()
        {
            try
            {
                lock (config)
                {
                    var tempConfig = $"{path}.new";
                    if (File.Exists(tempConfig))
                    {
                        var saveTo = $"{tempConfig}.{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
                        Service.Log.Warning($"Detected unsuccessfully saved file {tempConfig}: moving to {saveTo}");
                        Notice.Warning("Detected unsuccessfully saved configuration file.");
                        File.Move(tempConfig, saveTo);
                        Service.Log.Warning($"Success. Please manually check {saveTo} file contents.");
                    }
                    File.WriteAllText(tempConfig, serialized, Encoding.UTF8);
                    File.Move(tempConfig, path, true);
                }
            }
            catch (Exception e)
            {
                Service.Log.Error(e, "Failed to save configuration");
            }
        }

        if (async)
        {
            Task.Run(Write);
        }
        else
        {
            Write();
        }
    }

    private static T LoadConfiguration<T>(string path, bool isPathRelative = true) where T : IMioConfig, new()
    {
        if (isPathRelative) path = Path.Combine(ConfigDirectory, path);
        if (!File.Exists(path))
        {
            return new T();
        }
        return configIo.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8)) ?? new T();
    }

    /// <summary>
    /// Migrates default configuration to MioConfig. Must be called before Init. Get it from: <br />
    /// Service.PluginInterface.ConfigFile <br />
    /// Path.Combine(new DirectoryInfo(Service.PluginInterface.GetPluginConfigDirectory()).Parent!.FullName, "file_name.json")
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="NullReferenceException"></exception>
    public static void Migrate<T>(string oldConfigPath) where T : IMioConfig, new()
    {
        if (Config != null)
        {
            throw new NullReferenceException("Migrate must be called before initialization");
        }
        var path = MainConfigFile;
        var oldConfig = new FileInfo(oldConfigPath);

        // move single file config to a config folder
        if (!File.Exists(path) && oldConfig.Exists)
        {
            Service.Log.Warning($"Migrating {oldConfig} into EzConfig system");
            Config = LoadConfiguration<T>(oldConfig.FullName, false);
            Save();
            Config = null;
            File.Move(oldConfig.FullName, $"{oldConfig.FullName}.old");
        }
        else
        {
            Service.Log.Information($"Migrating conditions are not met, skipping...");
        }

        // api 13: migrate int[] to vectors
    }

    internal static void Dispose()
    {
        OnSave = null;
    }

}



public interface IMioConfig
{
}



public class ConfigIo
{
    public virtual T? Deserialize<T>(string s)
    {
        var settings = new JsonSerializerSettings()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        };
        return JsonConvert.DeserializeObject<T>(s, settings);
    }

    public virtual string Serialize(object config, bool prettyPrint, bool ignoreDefaultValues = true)
    {
        var settings = new JsonSerializerSettings()
        {
            Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
            DefaultValueHandling = ignoreDefaultValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include
        };
        return JsonConvert.SerializeObject(config, settings);
    }
}
