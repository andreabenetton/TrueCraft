using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using NVorbis;
using TrueCraft.Core;

namespace TrueCraft.Client
{
    public class AudioManager : IDisposable
    {
        public AudioManager()
        {
            AudioPacks = new Dictionary<string, SoundEffect[]>();
            EffectsByPath = new Dictionary<string, SoundEffect>(StringComparer.OrdinalIgnoreCase);
            EffectVolume = MusicVolume = 1;
        }

        private Dictionary<string, SoundEffect[]> AudioPacks { get; }
        private Dictionary<string, SoundEffect> EffectsByPath { get; }
        private bool _disposed;

        public float EffectVolume { get; set; }
        public float MusicVolume { get; set; }

        public void Dispose()
        {
            if (_disposed)
                return;
            foreach (var effect in EffectsByPath.Values)
                effect.Dispose();
            EffectsByPath.Clear();
            AudioPacks.Clear();
            _disposed = true;
        }

        public void LoadDefaultPacks(ContentManager content)
        {
            string[][] packs =
            {
                new[]
                {
                    "footstep.cloth",
                    "default_sand_footstep.1.ogg", "default_sand_footstep.2.ogg" // TODO: Cloth sound effects
                },
                new[]
                {
                    "footstep.grass",
                    "default_grass_footstep.1.ogg", "default_grass_footstep.2.ogg", "default_grass_footstep.3.ogg"
                },
                new[]
                {
                    "footstep.gravel",
                    "default_gravel_footstep.1.ogg", "default_gravel_footstep.2.ogg", "default_gravel_footstep.3.ogg",
                    "default_gravel_footstep.4.ogg"
                },
                new[]
                {
                    "footstep.sand",
                    "default_sand_footstep.1.ogg", "default_sand_footstep.2.ogg"
                },
                new[]
                {
                    "footstep.snow",
                    "default_snow_footstep.1.ogg", "default_snow_footstep.2.ogg", "default_snow_footstep.3.ogg"
                },
                new[]
                {
                    "footstep.stone",
                    "default_hard_footstep.1.ogg", "default_hard_footstep.2.ogg", "default_hard_footstep.3.ogg"
                },
                new[]
                {
                    "footstep.wood",
                    "default_wood_footstep.1.ogg", "default_wood_footstep.2.ogg"
                },
                new[]
                {
                    "footstep.glass",
                    "default_glass_footstep.ogg"
                },
                new[]
                {
                    "hurt",
                    "default_hurt.wav"
                }
            };
            foreach (var pack in packs)
            {
                var name = pack[0];
                var filenames = new string[pack.Length - 1];
                Array.Copy(pack, 1, filenames, 0, filenames.Length);
                LoadAudioPack(name, filenames);
            }
        }

        private SoundEffect LoadOgg(Stream stream)
        {
            using (var reader = new VorbisReader(stream, false))
            {
                var _buffer = new float[reader.TotalSamples];
                var buffer = new byte[reader.TotalSamples * 2];
                reader.ReadSamples(_buffer, 0, _buffer.Length);
                for (var i = 0; i < _buffer.Length; i++)
                {
                    var val = (short) Math.Max(Math.Min(short.MaxValue * _buffer[i], short.MaxValue), short.MinValue);
                    var decoded = BitConverter.GetBytes(val);
                    buffer[i * 2] = decoded[0];
                    buffer[i * 2 + 1] = decoded[1];
                }

                return new SoundEffect(buffer, reader.SampleRate,
                    reader.Channels == 1 ? AudioChannels.Mono : AudioChannels.Stereo);
            }
        }

        public void LoadAudioPack(string pack, string[] filenames)
        {
            var effects = new SoundEffect[filenames.Length];
            for (var i = 0; i < filenames.Length; i++)
                effects[i] = LoadEffect(filenames[i]);

            AudioPacks[pack] = effects;
        }

        private SoundEffect LoadEffect(string filename)
        {
            if (EffectsByPath.TryGetValue(filename, out var cached))
                return cached;

            var path = Path.Combine("Content", "Audio", filename);
            SoundEffect effect;
            using (var f = File.OpenRead(path))
            {
                if (filename.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                    effect = SoundEffect.FromStream(f);
                else if (filename.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                    effect = LoadOgg(f);
                else
                    throw new NotSupportedException($"Unsupported audio format: {filename}");
            }

            EffectsByPath[filename] = effect;
            return effect;
        }

        public void PlayPack(string pack, float volume = 1.0f)
        {
            var i = MathHelper.Random.Next(0, AudioPacks[pack].Length);
            AudioPacks[pack][i].Play(volume * EffectVolume, 1.0f, 0.0f);
        }
    }
}