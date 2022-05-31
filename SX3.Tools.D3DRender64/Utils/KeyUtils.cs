using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SX3.Tools.D3DRender64.Utils
{
    public class KeyUtils
    {
        #region Globals

        private Hashtable _keys;
        private Hashtable _prevKeys;
        private short[] _allKeys;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public KeyUtils()
        {
            _keys = new Hashtable();
            _prevKeys = new Hashtable();
            WinAPI.VirtualKeyShort[] _kkeys = (WinAPI.VirtualKeyShort[])Enum.GetValues(typeof(WinAPI.VirtualKeyShort));
            _allKeys = new short[_kkeys.Length];

            for (int i = 0; i < _allKeys.Length; i++)
                _allKeys[i] = (short)_kkeys[i];

            foreach (Int32 key in _allKeys)
            {
                if (!_prevKeys.ContainsKey(key))
                {
                    _prevKeys.Add(key, false);
                    _keys.Add(key, false);
                }
            }
        }

        ~KeyUtils()
        {
            _keys.Clear();
            _prevKeys.Clear();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the key-states
        /// </summary>
        public void Update()
        {
            _prevKeys = (Hashtable)_keys.Clone();
            foreach (Int32 key in _allKeys)
            {
                _keys[key] = GetKeyDown(key);
            }
        }

        /// <summary>
        /// Returns an array of all keys that went up since the last Update-call
        /// </summary>
        /// <returns></returns>
        public WinAPI.VirtualKeyShort[] KeysThatWentUp()
        {
            List<WinAPI.VirtualKeyShort> keys = new List<WinAPI.VirtualKeyShort>();
            foreach (WinAPI.VirtualKeyShort key in _allKeys)
            {
                if (KeyWentUp(key))
                    keys.Add(key);
            }
            return keys.ToArray();
        }

        /// <summary>
        /// Returns an array of all keys that went down since the last Update-call
        /// </summary>
        /// <returns></returns>
        public WinAPI.VirtualKeyShort[] KeysThatWentDown()
        {
            List<WinAPI.VirtualKeyShort> keys = new List<WinAPI.VirtualKeyShort>();
            foreach (WinAPI.VirtualKeyShort key in _allKeys)
            {
                if (KeyWentDown(key))
                    keys.Add(key);
            }
            return keys.ToArray();
        }

        /// <summary>
        /// Returns an array of all keys that went are down since the last Update-call
        /// </summary>
        /// <returns></returns>
        public WinAPI.VirtualKeyShort[] KeysThatAreDown()
        {
            List<WinAPI.VirtualKeyShort> keys = new List<WinAPI.VirtualKeyShort>();
            foreach (WinAPI.VirtualKeyShort key in _allKeys)
            {
                if (KeyIsDown(key))
                    keys.Add(key);
            }
            return keys.ToArray();
        }

        /// <summary>
        /// Returns whether the given key went up since the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyWentUp(WinAPI.VirtualKeyShort key)
        {
            return KeyWentUp((Int32)key);
        }

        /// <summary>
        /// Returns whether the given key went up since the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyWentUp(Int32 key)
        {
            if (!KeyExists(key))
                return false;
            return (bool)_prevKeys[key] && !(bool)_keys[key];
        }

        /// <summary>
        /// Returns whether the given key went down since the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyWentDown(WinAPI.VirtualKeyShort key)
        {
            return KeyWentDown((Int32)key);
        }

        /// <summary>
        /// Returns whether the given key went down since the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyWentDown(Int32 key)
        {
            if (!KeyExists(key))
                return false;
            return !(bool)_prevKeys[key] && (bool)_keys[key];
        }

        /// <summary>
        /// Returns whether the given key was down at time of the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyIsDown(WinAPI.VirtualKeyShort key)
        {
            return KeyIsDown((Int32)key);
        }

        /// <summary>
        /// Returns whether the given key was down at time of the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyIsDown(Int32 key)
        {
            if (!KeyExists(key))
                return false;
            return (bool)_prevKeys[key] || (bool)_keys[key];
        }

        /// <summary>
        /// Returns whether the given key is contained in the used hashtables
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        private bool KeyExists(Int32 key)
        {
            return (_prevKeys.ContainsKey(key) && _keys.ContainsKey(key));
        }

        #endregion

        #region Statics

        public static bool GetKeyDown(WinAPI.VirtualKeyShort key)
        {
            return GetKeyDown((Int32)key);
        }

        public static void LMouseClick(int sleeptime)
        {
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(sleeptime);
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
        }

        public static bool GetKeyDown(Int32 key)
        {
            return Convert.ToBoolean(WinAPI.GetKeyState(key) & WinAPI.KEY_PRESSED);
        }

        public static bool GetKeyDownAsync(Int32 key)
        {
            return GetKeyDownAsync((WinAPI.VirtualKeyShort)key);
        }

        public static bool GetKeyDownAsync(WinAPI.VirtualKeyShort key)
        {
            return Convert.ToBoolean(WinAPI.GetAsyncKeyState(key) & WinAPI.KEY_PRESSED);
        }

        #endregion
    }
}
