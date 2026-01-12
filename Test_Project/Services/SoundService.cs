using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Test_Project.Services
{

    /// <summary>
    /// Simple procedural sound service for on-demand beeps and tones.
    /// Uses 16-bit PCM mono SoundEffect instances for short sounds.
    /// </summary>
    public class SoundService
    {
        private readonly int _sampleRate;
        private readonly List<SoundEffect> _activeEffects = new();
        private readonly System.Random _rnd = new();

        public SoundService(int sampleRate = 44100)
        {
            _sampleRate = sampleRate;
            _presets = new Dictionary<string, SoundPreset>();
        }

        private readonly Dictionary<string, SoundPreset> _presets;

        // Play all layers of a preset (main + optional layers)
        private void PlayPresetInternal(SoundPreset p)
        {
            if (p == null) return;
            // main voice
            var freq = p.Frequency + (float)(_rnd.NextDouble() * 2.0 - 1.0) * p.FrequencyVariance;
            PlayBeep(freq, p.Duration, p.Volume, p.Waveform, p.VibratoRate, p.VibratoDepth, p.Envelope, p.BitCrush);
            // layers
            if (p.Layers != null)
            {
                foreach (var layer in p.Layers)
                {
                    var lf = layer.Frequency + (float)(_rnd.NextDouble() * 2.0 - 1.0) * layer.FrequencyVariance;
                    PlayBeep(lf, layer.Duration, layer.Volume, layer.Waveform, layer.VibratoRate, layer.VibratoDepth, layer.Envelope, layer.BitCrush);
                }
            }
        }

        /// <summary>
        /// Play a procedural beep/tone with parameters.
        /// </summary>
        public void PlayBeep(float freqHz, float durationSec = 0.2f, float volume = 0.5f, Waveform waveform = Waveform.Sine,
            float vibratoRate = 0f, float vibratoDepth = 0f, Envelope env = null, bool bitCrush = false)
        {
            if (durationSec <= 0f) return;
            env ??= new Envelope();

            int samples = Math.Max(1, (int)(_sampleRate * durationSec));
            short[] pcm = new short[samples];

            for (int i = 0; i < samples; i++)
            {
                var t = i / (float)_sampleRate;

                // Vibrato
                var f = freqHz;
                if (vibratoRate > 0f && vibratoDepth > 0f)
                {
                    f *= 1f + MathF.Sin(2f * MathF.PI * vibratoRate * t) * vibratoDepth;
                }

                float sample = waveform switch
                {
                    Waveform.Sine => MathF.Sin(2f * MathF.PI * f * t),
                    Waveform.Square => MathF.Sign(MathF.Sin(2f * MathF.PI * f * t)),
                    Waveform.Triangle => 2f * MathF.Asin(MathF.Sin(2f * MathF.PI * f * t)) / MathF.PI,
                    Waveform.Sawtooth => (2f * (t * f - MathF.Floor(0.5f + t * f))),
                    Waveform.Noise => (float)(2.0 * _rnd.NextDouble() - 1.0),
                    _ => MathF.Sin(2f * MathF.PI * f * t),
                };

                // Envelope (ADSR)
                var amp = 1f;
                var attackEnd = env.Attack;
                var decayEnd = env.Attack + env.Decay;
                var releaseStart = Math.Max(0f, durationSec - env.Release);

                if (t < attackEnd) amp = t / Math.Max(1e-6f, attackEnd);
                else if (t < decayEnd) amp = 1f - ((t - attackEnd) / Math.Max(1e-6f, env.Decay)) * (1f - env.Sustain);
                else if (t >= releaseStart) amp = MathF.Max(0f, env.Sustain * (1f - (t - releaseStart) / Math.Max(1e-6f, env.Release)));
                else amp = env.Sustain;

                var signed = sample * amp * volume;

                // Bitcrush effect (simple quantization)
                if (bitCrush)
                {
                    var bits = 6; // keep 6 bits by default
                    var maxLevel = (1 << bits) - 1;
                    signed = MathF.Round(signed * maxLevel) / maxLevel;
                }

                var clamped = signed < -1f ? -1f : (signed > 1f ? 1f : signed);
                pcm[i] = (short)(clamped * short.MaxValue);
            }

            var buffer = new byte[pcm.Length * 2];
            Buffer.BlockCopy(pcm, 0, buffer, 0, buffer.Length);

            var sfx = new SoundEffect(buffer, _sampleRate, Microsoft.Xna.Framework.Audio.AudioChannels.Mono);
            sfx.Play(volume, 0f, 0f);

            // Keep reference briefly so it isn't GC'd immediately while playing
            _activeEffects.Add(sfx);
            // Schedule cleanup asynchronously (dispose after sound finished)
            _ = CleanupLater(sfx, durationSec + 0.25f);
        }

        /// <summary>
        /// Register a preset with a name
        /// </summary>
        public void AddPreset(string name, SoundPreset preset)
        {
            _presets[name] = preset;
        }

        /// <summary>
        /// Register a whole bank of presets
        /// </summary>
        public void RegisterBank(SoundBank bank)
        {
            foreach (var kv in bank.Presets) _presets[kv.Key] = kv.Value;
        }

        /// <summary>
        /// Play a named preset if available
        /// </summary>
        public void PlayPreset(string name)
        {
            if (!_presets.TryGetValue(name, out var p))
            {
                Console.WriteLine($"Sound preset not found: {name}");
                return;
            }
            // Apply some randomness based on variance
            var freq = p.Frequency + (float)(_rnd.NextDouble() * 2.0 - 1.0) * p.FrequencyVariance;
            PlayPresetInternal(p);
        }

        private async System.Threading.Tasks.Task CleanupLater(SoundEffect sfx, float delaySeconds)
        {
            await System.Threading.Tasks.Task.Delay((int)(delaySeconds * 1000));
            sfx.Dispose();
            _activeEffects.Remove(sfx);
        }
    }
}
