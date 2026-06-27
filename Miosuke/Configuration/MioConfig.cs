using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Miosuke.Messages;

namespace Miosuke.Configuration;

public static class MioConfig
{
    // paths and state
    // --------------------------------
    public static string? ConfigDirectoryOverride { get; set; }

    public static string ConfigDirectory => ConfigDirectoryOverride == null
        ? Service.PluginInterface.GetPluginConfigDirectory()
        : Path.Combine(
            new DirectoryInfo(Service.PluginInterface.GetPluginConfigDirectory()).Parent!.FullName,
            ConfigDirectoryOverride);

    public static string MainConfigFileName { get; set; } = "main.json";
    public static string MainConfigFile => Path.Combine(ConfigDirectory, MainConfigFileName);
    public static event Action? OnSave;
    public static readonly ConfigIo ConfigIo = new();
    public static IMioConfig? Config { get; private set; }

    // lifecycle
    // --------------------------------
    public static void Setup(string? configDirectoryOverride = null, string? mainConfigFileName = null)
    {
        if (configDirectoryOverride != null) ConfigDirectoryOverride = configDirectoryOverride;
        if (mainConfigFileName != null) MainConfigFileName = mainConfigFileName;
    }

    public static T Init<T>() where T : IMioConfig, new()
    {
        Directory.CreateDirectory(ConfigDirectory);
        Config = LoadConfiguration<T>(MainConfigFile);
        return (T)Config;
    }

    public static void Save()
    {
        if (Config != null) Save(Config, MainConfigFile, false);
    }

    public static void Save(this IMioConfig config, string? path = null, bool isRelativePath = true, bool async = true)
    {
        path ??= MainConfigFileName;
        if (isRelativePath) path = Path.Combine(ConfigDirectory, path);

        OnSave?.Invoke();
        var jsonUtf8Bytes = ConfigIo.Serialize(config);

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

                    File.WriteAllBytes(tempConfig, jsonUtf8Bytes);
                    File.Move(tempConfig, path, true);
                }
            }
            catch (Exception e)
            {
                Service.Log.Error(e, "Failed to save configuration");
            }
        }

        if (async)
            Task.Run(Write);
        else
            Write();
    }

    private static T LoadConfiguration<T>(string path, bool isPathRelative = true) where T : IMioConfig, new()
    {
        if (isPathRelative) path = Path.Combine(ConfigDirectory, path);
        if (!File.Exists(path)) return new T();

        // Keep config files in the current project encoding.
        CheckAndRemoveUtf8Bom(path);

        try
        {
            // this should never return null, but just in case
            return ConfigIo.Deserialize<T>(File.ReadAllBytes(path)) ?? new T();
        }
        catch (Exception e)
        {
            Service.Log.Error(e,
                $"Config file {path} is corrupted or has invalid format. Loading default configuration.");
            Notice.Error($"Config file is corrupted or has invalid format. Loading default configuration.");
            return new T();
        }
    }

    private static void CheckAndRemoveUtf8Bom(string path)
    {
        var bom = Encoding.UTF8.GetPreamble();
        var fileBytes = File.ReadAllBytes(path);
        if (fileBytes.Length >= bom.Length)
        {
            var hasBom = true;
            for (var i = 0; i < bom.Length; i++)
            {
                if (fileBytes[i] != bom[i])
                {
                    hasBom = false;
                    break;
                }
            }

            if (hasBom)
            {
                var newBytes = new byte[fileBytes.Length - bom.Length];
                Array.Copy(fileBytes, bom.Length, newBytes, 0, newBytes.Length);
                File.WriteAllBytes(path, newBytes);
                Service.Log.Info($"Removed UTF-8 BOM from config file {path}");
            }
        }
    }

    internal static void Dispose()
    {
        OnSave = null;
    }
}

public interface IMioConfig;

public class ConfigIo
{
    private readonly JsonSerializerOptions _deserializeOptions = new()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    };

    private readonly JsonSerializerOptions _serializeOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    public virtual T? Deserialize<T>(ReadOnlySpan<byte> utf8Json) =>
        JsonSerializer.Deserialize<T>(utf8Json, _deserializeOptions);

    public virtual byte[] Serialize(object config) => JsonSerializer.SerializeToUtf8Bytes(config, _serializeOptions);
}
