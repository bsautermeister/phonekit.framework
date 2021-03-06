using System;
using System.Windows.Media;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace PhoneKit.Framework.Audio
{
    /// <summary>
    /// Manages a bunch of XNA audio effects to play simple WAV or MP3 sounds.
    /// </summary>
    public class SoundEffects
    {
        #region Membes

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static SoundEffects _instance;

        /// <summary>
        /// The managed sound effects.
        /// </summary>
        private readonly Dictionary<string, SoundEffect> _soundEffects = new Dictionary<string, SoundEffect>();

        /// <summary>
        /// Indicates whether any sound effect has loaded to prevent multiple sound loading.
        /// </summary>
        private bool _hasLoaded = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new SoundEffects instance.
        /// </summary>
        private SoundEffects()
        {
            Initialize();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads an sound effect whan the file has not been loaded before.
        /// </summary>
        /// <remarks>
        /// You can get resource stream with <code>App.GetResourceStream(new Uri("sound.wav", UriKind.Relative));</code>
        /// or <code> TitleContainer.OpenStream("sound.wav");</code>.
        /// </remarks>
        /// <param name="key">The sound effects key.</param>
        /// <param name="stream">The main applications resource stream or any other stream.</param>
        /// <param name="overridePrevious">
        /// Specifies if there is an exisitng sound for this key, whether it sould be overridden.
        /// </param>
        public void Load(string key, Stream stream, bool overridePrevious = false)
        {
            // verify overriding of previous sound effect
            if (ContainsKey(key))
            {
                if (overridePrevious)
                    Unload(key);
                else
                    return;
            }

            // add sound effect from stream
            SoundEffect sound = null;
            try
            {
                sound = SoundEffect.FromStream(stream);
            }
            catch(InvalidOperationException ioe) 
            {
                // BUSENSE: [15 May 2014 10:16 - 17 May 2014 01:49; 4 times] An unexpected error has occurred
                Debug.WriteLine("Sound loading error: " + ioe.Message);
                return;
            }

            _soundEffects.Add(key, sound);

            // mark that at least on file has been loaded.
            _hasLoaded = true;
        }

        /// <summary>
        /// Unloads the sound effect.
        /// </summary>
        /// <param name="key">The sound effects key.</param>
        public void Unload(string key)
        {
            if (!ContainsKey(key))
                return;

            // free resources and remove the sound effect
            _soundEffects[key].Dispose();
            _soundEffects.Remove(key);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the XNA delegates to simulate the XNA update.
        /// </summary>
        private static void Initialize()
        {
            // Adds an Update delegate, to simulate the XNA update method.
            CompositionTarget.Rendering += delegate(object sender, EventArgs e)
            {
                Microsoft.Xna.Framework.FrameworkDispatcher.Update();
            };

            Microsoft.Xna.Framework.FrameworkDispatcher.Update();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the sound effects singleton instance.
        /// </summary>
        public static SoundEffects Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SoundEffects();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets whether the there is a sound associated with the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>TRUE if the sound was loaded and exists, else FALSE.</returns>
        public bool ContainsKey(string key)
        {
            return _soundEffects.ContainsKey(key);
        }

        /// <summary>
        /// Gets a loaded sound effect.
        /// </summary>
        /// <param name="key">The key of the sound effect.</param>
        /// <returns>The sound effect.</returns>
        public SoundEffect this[string key]
        {
            get
            {
                if (!ContainsKey(key))
                {
                    return null;
                }

                return _soundEffects[key];
            }
        }

        /// <summary>
        /// Gets whether at least one sound effect has been loaded
        /// to prevent multiple sound loading.
        /// </summary>
        /// <remarks>
        /// Surround your Load() calls with this check.
        /// </remarks>
        public bool HasLoaded
        {
            get
            {
                return _hasLoaded;
            }
        }

        #endregion
    }
}
