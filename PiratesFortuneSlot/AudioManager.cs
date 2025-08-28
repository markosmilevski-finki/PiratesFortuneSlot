using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace PiratesFortuneSlot
{
    public class AudioManager
    {
        private IWavePlayer sndBackgroundPlayer;
        private WaveFileReader sndBackgroundReader;
        private List<IWavePlayer> soundPlayers = new List<IWavePlayer>();
        private List<WaveFileReader> soundReaders = new List<WaveFileReader>();
        private bool closingForm = false;

        public void LoadSounds()
        {
        }

        public void PlaySound(string resourceName)
        {
            try
            {
                var stream = GetEmbeddedResourceStream(resourceName);
                if (stream == null) return;

                var reader = new WaveFileReader(stream);
                var player = new WaveOutEvent();
                player.Init(reader);
                player.Play();

                soundPlayers.Add(player);
                soundReaders.Add(reader);

                player.PlaybackStopped += (s, e) =>
                {
                    soundPlayers.Remove(player);
                    soundReaders.Remove(reader);
                    player.Dispose();
                    reader.Dispose();
                };
            }
            catch (Exception)
            {
            }
        }

        public void PlayBackgroundMusic(string resourceName)
        {
            try
            {
                if (sndBackgroundPlayer != null)
                {
                    sndBackgroundPlayer.Stop();
                    sndBackgroundPlayer.Dispose();
                    sndBackgroundReader?.Dispose();
                    sndBackgroundPlayer = null;
                    sndBackgroundReader = null;
                }

                var stream = GetEmbeddedResourceStream(resourceName);
                if (stream == null) return;

                sndBackgroundReader = new WaveFileReader(stream);
                var loopStream = new NAudio.Wave.WaveChannel32(sndBackgroundReader) { PadWithZeroes = false };
                sndBackgroundPlayer = new WaveOutEvent();
                sndBackgroundPlayer.Init(loopStream);
                sndBackgroundPlayer.PlaybackStopped += (s, e) =>
                {
                    if (sndBackgroundReader != null && !closingForm)
                    {
                        sndBackgroundReader.Position = 0;
                        sndBackgroundPlayer.Play();
                    }
                };
                sndBackgroundPlayer.Play();
            }
            catch (Exception)
            {
            }
        }

        private static Stream GetEmbeddedResourceStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "PiratesFortuneSlot.Resources." + name;
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                MessageBox.Show($"Sound resource not found: {resourceName}");
            }
            return stream;
        }

        public void Dispose()
        {
            closingForm = true;
            if (sndBackgroundPlayer != null)
            {
                sndBackgroundPlayer.Stop();
                sndBackgroundPlayer.Dispose();
                sndBackgroundReader?.Dispose();
            }
            foreach (var player in soundPlayers)
            {
                player.Stop();
                player.Dispose();
            }
            foreach (var reader in soundReaders)
            {
                reader.Dispose();
            }
        }
    }
}