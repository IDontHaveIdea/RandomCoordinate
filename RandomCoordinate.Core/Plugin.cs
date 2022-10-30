//
// RandomCoordinatePlugin
//
using System;

using BepInEx;
using BepInEx.Logging;

using KKAPI;

using IDHIUtils;
using KKAPI.Chara;

namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        internal static Logg _Log = new();
        internal static Random RandCoordinate = new();

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
