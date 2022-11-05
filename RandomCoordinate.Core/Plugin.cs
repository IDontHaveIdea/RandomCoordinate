//
// RandomCoordinatePlugin
//
using System;
using System.Collections.Generic;

using UnityEngine.SceneManagement;

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
        internal static string _guideChaName;
        internal static SaveData.Heroine _guide;

        // This dictionary is for caching some information
        // Names sometimes fail when using ChaControl.GetHeroine()
        internal static Dictionary<string, string> GirlsNames = new();

        private void Awake()
        {
            _Log.LogSource = base.Logger;
            ConfigEntries();
            _Log.Enabled = DebugInfo.Value;
#if DEBUG
            _Log.Level(LogLevel.Info, $"[{PluginName}] {PluginDisplayName} " +
                $"loaded.");
            _Log.Level(LogLevel.Info, $"[{PluginName}] Logging set to " +
                $"{_Log.Enabled}");
#endif
            GirlsNames.Clear();
            CharacterApi.RegisterExtraBehaviour<RandomCoordinateController>(GUID);

            KoikatuAPI.Quitting += OnGameExit;
            SceneManager.sceneLoaded += RoamStart;
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
        /// Check when main game starts
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="loadSceneMode"></param>
        private void RoamStart(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == "Action")
            {
                _guideChaName =
                    $"chaF_{Manager.Game.saveData.heroineList.Count:D3}";
                SceneManager.sceneLoaded -= RoamStart;
            }
        }

        internal static void SetGuide(SaveData.Heroine heroine)
        {
            _guide = heroine;
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
