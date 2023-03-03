//
// RandomCoordinatePlugin
//
using System;
using System.Collections.Generic;

using UnityEngine.SceneManagement;

using ActionGame.Chara;

using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;

using IDHIUtils;
using System.Text;

namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        internal static Logg _Log = new();
        internal static Random RandCoordinate = new();

        // This dictionary is for caching some information
        // Names sometimes fail when using ChaControl.GetHeroine()
        internal static Dictionary<string, string> GirlsNames = new();

        private void Awake()
        {
            _Log.LogSource = base.Logger;
            ConfigEntries();
            _Log.Enabled = DebugInfo.Value;
            _Log.DebugToConsole = DebugToConsole.Value;
#if DEBUG
            _Log.Level(LogLevel.Info, $"[{PluginName}] {PluginDisplayName} " +
                $"loaded.");
            _Log.Level(LogLevel.Info, $"[{PluginName}] Logging set to " +
                $"{_Log.Enabled}");
#endif
            GirlsNames.Clear();
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

        internal static void ChangeCoordinate(NPC girl, int coordinateNumber)
        {
            ChangeCoordinate(girl.chaCtrl, coordinateNumber);
        }

        internal static void ChangeCoordinate(SaveData.Heroine girl, int coordinateNumber)
        {
            ChangeCoordinate(girl.chaCtrl, coordinateNumber);
        }

        internal static void ChangeCoordinate(ChaControl girl, int coordinateNumber)
        {
            Manager.Character.enableCharaLoadGCClear = false;
            girl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)coordinateNumber);
            Manager.Character.enableCharaLoadGCClear = true;
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
