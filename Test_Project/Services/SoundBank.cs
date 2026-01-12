using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Test_Project.Services
{
    public class SoundBank
    {
        public Dictionary<string, SoundPreset> Presets { get; } = new Dictionary<string, SoundPreset>();

        public static SoundBank LoadFromFile(string path)
        {
            if (!File.Exists(path)) return new SoundBank();
            var json = File.ReadAllText(path);
            var opt = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            opt.Converters.Add(new JsonStringEnumConverter());

            // Expecting JSON object mapping names to presets
            var dict = JsonSerializer.Deserialize<Dictionary<string, SoundPreset>>(json, opt);
            var bank = new SoundBank();
            if (dict != null)
            {
                foreach (var kv in dict) bank.Presets[kv.Key] = kv.Value;
            }
            return bank;
        }
    }
}