using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SX3.Tools.D3DRender64.Utils
{
    public abstract class ConfigUtils
    {
        #region Globals

        protected Hashtable _settingsStorage;

        #endregion

        #region Constructor

        public ConfigUtils()
        {
            this._settingsStorage = new Hashtable();
        }

        #endregion

        #region Methods

        protected ICollection GetKeys()
        {
            return _settingsStorage.Keys;
        }

        /// <summary>
        /// Returns the value associated with the given key
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="key">Key of the value</param>
        /// <returns></returns>
        public T GetValue<T>(string key)
        {
            if (HasKey(key))
                return (T)_settingsStorage[key];
            else
                throw new KeyNotFoundException();
        }

        /// <summary>
        /// Returns the value associated with the given key
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <returns></returns>
        public Object GetValue(string key)
        {
            if (HasKey(key))
                return _settingsStorage[key];
            else
                throw new KeyNotFoundException();
        }

        /// <summary>
        /// Returns whether this ConfigUtils contains the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            return _settingsStorage.ContainsKey(key);
        }

        /// <summary>
        /// Sets the value of an existing key-value pair or adds a new one if it does not exist yet
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, object value)
        {
            _settingsStorage[key] = value;
        }

        /// <summary>
        /// Removes the given key from this ConfigUtils
        /// </summary>
        /// <param name="key">The key of the value</param>
        /// <returns>The value associated with the given key</returns>
        public Object RemoveKey(string key)
        {
            if (HasKey(key))
            {
                Object obj = _settingsStorage[key];
                _settingsStorage.Remove(key);
                return obj;
            }
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Clears all key-value pairs of this ConfigUtils
        /// </summary>
        public void Clear()
        {
            _settingsStorage.Clear();
        }

        /// <summary>
        /// Parses the given enum from string to the desired enum-type
        /// </summary>
        /// <typeparam name="T">Type of the desired enum</typeparam>
        /// <param name="value">Value of the given enum as string</param>
        /// <returns>Parsed enum</returns>
        protected static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Reads a settings-file from disk
        /// </summary>
        /// <param name="file"></param>
        public virtual void ReadSettingsFromFile(string file)
        {
            if (File.Exists(file))
                ReadSettings(File.ReadAllBytes(file));
            else
                throw new FileNotFoundException("Settings-file not found", file);
        }

        /// <summary>
        /// Reads settings from a byte-array
        /// </summary>
        /// <param name="data"></param>
        public abstract void ReadSettings(byte[] data);

        /// <summary>
        /// Safes all key-value pairs to disk
        /// </summary>
        /// <param name="file"></param>
        public virtual void SaveSettingsToFile(string file)
        {
            File.WriteAllBytes(file, SaveSettings());
        }

        /// <summary>
        /// Saves settings to a byte-array
        /// </summary>
        /// <returns></returns>
        public abstract byte[] SaveSettings();

        #endregion
    }
}
