using System.Text.Json.Serialization;

namespace Test_Project.Services
{
    public class Envelope
    {
        public float Attack { get; set; } = 0.01f;
        public float Decay { get; set; } = 0.05f;
        public float Sustain { get; set; } = 0.8f;
        public float Release { get; set; } = 0.05f;

        public Envelope() { }
        public Envelope(float attack, float decay, float sustain, float release)
        {
            Attack = attack; Decay = decay; Sustain = sustain; Release = release;
        }
    }

    public enum Waveform { Sine, Square, Triangle, Sawtooth, Noise }

    public class SoundPreset
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Waveform Waveform { get; set; } = Waveform.Sine;

        public float Frequency { get; set; } = 440f;
        public float FrequencyVariance { get; set; } = 0f; // +- Hz randomness
        public float Duration { get; set; } = 0.2f;
        public float Volume { get; set; } = 0.6f;
        public float VibratoRate { get; set; } = 0f;
        public float VibratoDepth { get; set; } = 0f;
        public Envelope Envelope { get; set; } = new Envelope();
        public bool BitCrush { get; set; } = false;

        // Optional layered voices for richer SFX
        public SoundPreset[] Layers { get; set; }
    }
}