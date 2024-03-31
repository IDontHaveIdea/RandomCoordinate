//
// RandomCoordinatePlugin
//
// Ignore Spelling: cha

using System;
using System.Collections.Generic;

using ActionGame.Chara;

using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;

using IDHIUtils;
using KKAPI.MainGame;



namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        internal static Logg _Log = new();
        internal static Random RandCoordinate = new();
        internal static MoreOutfits _MoreOutfits = new();

        // The Character controller is reinitialiezed and even unloaded
        // during the game mantaining persistent data on the controller
        // is just an act of frustration. So persistent runtime data
        // will be saved in this dictionary and used by the controller.
        private static Dictionary<string, RandomData> _GirlsRandomData = [];

        public static Dictionary<string, RandomData> GirlsRandomData =>
            _GirlsRandomData;


        private void Awake()
        {
            _Log.LogSource = base.Logger;
            ConfigEntries();
            _Log.Enabled = DebugInfo.Value;
            _Log.DebugToConsole = DebugToConsole.Value;
            _Log.Level(LogLevel.Info, $"[{PluginName}] {PluginDisplayName} " +
                $"loaded.");
#if DEBUG
            _Log.Level(LogLevel.Info, $"[{PluginName}] Logging set to " +
                $"{_Log.Enabled} DebugToConsole={_Log.DebugToConsole}");
            _Log.Level(LogLevel.Info, $"[ConfigEntries] Random Coordinates Change " +
                $"Room Only set to={OnlyChangingRoom.Value}");
#endif
            _GirlsRandomData.Clear();

            CharacterApi.RegisterExtraBehaviour<RandomCoordinateController>(GUID);

            KoikatuAPI.Quitting += OnGameExit;
            GameAPI.PeriodChange += PeriodChange;
        }

        private void Start()
        {
#if DEBUG
            var thisAss = typeof(RandomCoordinatePlugin).Assembly;
            _Log.Level(LogLevel.Info, $"[{PluginName}] Assembly {thisAss.FullName}");
#endif
            Hooks.Init();
            //Hooks.InitJetPack();
        }

        /// <summary>
        /// Game exit event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnGameExit(object sender, EventArgs e)
        {
            _Log.Info($"[OnGameExit] {PluginName} exiting game.");
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
            girl.ChangeCoordinateTypeAndReload(
                (ChaFileDefine.CoordinateType)coordinateNumber);
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
