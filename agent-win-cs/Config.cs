using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClipboardAgent
{
    // Config compatible con la del agente Python (%APPDATA%\ClipboardAgent\config.json).
    public class Config
    {
        [JsonPropertyName("server_url")] public string ServerUrl { get; set; }
        [JsonPropertyName("token")] public string Token { get; set; }
        [JsonPropertyName("autostart_setup")] public bool AutostartSetup { get; set; }

        [JsonIgnore] public bool FromEnv { get; set; }

        private static string Dir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClipboardAgent");
        private static string FilePath => Path.Combine(Dir, "config.json");

        private static readonly JsonSerializerOptions Opts = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        public static Config Load()
        {
            var envUrl = Environment.GetEnvironmentVariable("CLIPBOARD_SERVER_URL");
            var envToken = Environment.GetEnvironmentVariable("CLIPBOARD_TOKEN");
            if (!string.IsNullOrWhiteSpace(envUrl) && !string.IsNullOrWhiteSpace(envToken))
                return new Config { ServerUrl = envUrl.TrimEnd('/'), Token = envToken, FromEnv = true };

            try
            {
                if (File.Exists(FilePath))
                {
                    var cfg = JsonSerializer.Deserialize<Config>(File.ReadAllText(FilePath), Opts);
                    if (cfg != null && !string.IsNullOrWhiteSpace(cfg.ServerUrl)
                        && !string.IsNullOrWhiteSpace(cfg.Token))
                        return cfg;
                }
            }
            catch { }
            return null;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, Opts));
            }
            catch { }
        }
    }
}
