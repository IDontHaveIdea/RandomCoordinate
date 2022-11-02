//
// RandomCoordinatePlugin
//
using System;
using System.Collections.Generic;

using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;

using IDHIUtils;


namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        internal static Logg _Log = new();
        internal static Random RandCoordinate = new();

        // These dictionaries are to cache information

        // Names sometimes fail using ChaControl.GetHeroine()
        internal static Dictionary<string, string> GirlsNames = new();

        // When there is a dating event the game seems to be creating
        // a new instance of the girl which resets the saved information
        // at the end of the date scene the girls seems to be restored. 
        internal static Dictionary<string, int> GirlsNowCoordinate = new();

        private void Awake()
        {
            _Log.LogSource = base.Logger;
            ConfigEntries();
            _Log.Enabled = DebugInfo.Value;
#if DEBUG
            _Log.Level(LogLevel.Info, $"[{PluginName}] Logging set to " +
                $"{_Log.Enabled}");
            _Log.Level(LogLevel.Info, $"[{PluginName}] {PluginDisplayName} " +
                $"loaded.");
#endif
            GirlsNames.Clear();
            GirlsNowCoordinate.Clear();
            CharacterApi.RegisterExtraBehaviour<RandomCoordinateController>(GUID);

            KoikatuAPI.Quitting += OnGameExit;
        }

        private void Start()
        {
#if DEBUG
            var thisAss = typeof(RandomCoordinatePlugin).Assembly;
            _Log.Level(LogLevel.Info, $"[{PluginName}] Assembly {thisAss.FullName}");
#endif
            Hooks.Init();
        }

        /// <summary>
        /// Game exit event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnGameExit(object sender, EventArgs e)
        {
            _Log.Info($"[OnGameExit] RandomCoordinate exiting game.");
        }

        /// <summary>
        /// Get controller for characters
        /// </summary>
        /// <param name="chaControl"></param>
        /// <returns></returns>
        public static RandomCoordinateController GetController(ChaControl chaControl)
        {
            return ((chaControl == null) || (chaControl.gameObject == null))
                ? null : chaControl.GetComponent<RandomCoordinateController>();
        }
    }
}
