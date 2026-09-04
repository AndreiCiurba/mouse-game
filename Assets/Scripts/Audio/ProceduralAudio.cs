using UnityEngine;

namespace MouseGame.Audio
{
    /// <summary>
    /// Generates short placeholder SFX (tones/noise bursts) entirely in code — no audio asset
    /// files exist in the project yet. Same "primitives until real content" approach used for
    /// the mouse/cat models, applied to audio: swap in real recorded clips later by assigning
    /// them wherever these generated ones are used instead.
    /// </summary>
    public static class ProceduralAudio
    {
        /// <summary>A single-frequency tone with a quick attack and exponential decay.</summary>
        public static AudioClip CreateTone(string name, float frequency, float duration, float peakAmplitude = 0.6f, int sampleRate = 44100)
        {
            return CreateClip(name, duration, sampleRate, (t, envelope) =>
                Mathf.Sin(2f * Mathf.PI * frequency * t) * peakAmplitude * envelope);
        }

        /// <summary>Filtered-feeling white noise burst with a quick attack and fast decay — a soft "thud"/"tap".</summary>
        public static AudioClip CreateNoiseBurst(string name, float duration, float peakAmplitude = 0.5f, int sampleRate = 44100)
        {
            var rng = new System.Random(name.GetHashCode());
            float previous = 0f;

            return CreateClip(name, duration, sampleRate, (_, envelope) =>
            {
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                // Cheap one-pole low-pass so it reads as a soft thud instead of harsh static.
                previous = Mathf.Lerp(previous, white, 0.3f);
                return previous * peakAmplitude * envelope;
            });
        }

        private static AudioClip CreateClip(string name, float duration, int sampleRate,
            System.Func<float, float, float> sampleAt)
        {
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(duration * sampleRate));
            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);

            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-8f * t / duration); // quick attack, exponential decay
                samples[i] = Mathf.Clamp(sampleAt(t, envelope), -1f, 1f);
            }

            clip.SetData(samples, 0);
            return clip;
        }
    }
}
