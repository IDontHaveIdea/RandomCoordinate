using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using IDHIUtils;
using Utils = IDHIUtils.Utilities;

using static IDHIPlugins.RandomCoordinatePlugin;


namespace IDHIPlugins
{
    internal partial class Hooks
    {
        internal static List<string> _cordNames = [];

        internal static void InitJetPack()
        {
            _cordNames = [.. Enum.GetNames(typeof(ChaFileDefine.CoordinateType))];

            OnChangeCoordinateType += (_sender, _args) =>
            {
                var name = Utilities.GirlName(_args.ChaControl);
                _Log.Warning($"[OnChangeCoordinateType] {name} [{_args.CoordinateType}][{_args.State}][{_args.DuringChange}]");
            };
        }

        public static event EventHandler<ChangeCoordinateTypeEventArgs> OnChangeCoordinateType;
        public class ChangeCoordinateTypeEventArgs : EventArgs
        {
            public ChangeCoordinateTypeEventArgs(ChaControl _chaCtrl, int _coordinateIndex, string _state, int _prev)
            {
                ChaControl = _chaCtrl;
                CoordinateType = _coordinateIndex;
                State = _state;
                if (_state == "Coroutine")
                {
                    DuringChange = false;
                }
                else
                {
                    DuringChange = true;
                }

                PreviousCoordinateType = _prev;
                CoordinateChanged = _coordinateIndex != _prev;
            }

            public ChaControl ChaControl { get; }
            public int CoordinateType { get; }
            public string State { get; }
            public bool DuringChange { get; } = false;

            public int PreviousCoordinateType { get; }
            public bool CoordinateChanged { get; } = false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), typeof(ChaFileDefine.CoordinateType), typeof(bool))]
        private static void ChaControl_ChangeCoordinateType_Prefix(ChaControl __instance, ref int __state, ChaFileDefine.CoordinateType type)
        {
            __state = __instance.fileStatus.coordinateType;

            //if ((int) type != __instance.fileStatus.coordinateType)
            OnChangeCoordinateType?.Invoke(null, new ChangeCoordinateTypeEventArgs(__instance, (int)type, "Prefix", __state));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), typeof(ChaFileDefine.CoordinateType), typeof(bool))]
        private static void ChaControl_ChangeCoordinateType_Postfix(ChaControl __instance, ref int __state, ChaFileDefine.CoordinateType type)
        {
            //if ((int) type == __state) return;

            OnChangeCoordinateType?.Invoke(null, new ChangeCoordinateTypeEventArgs(__instance, (int)type, "Postfix", __state));
            //__instance.StartCoroutine(ChaControl_ChangeCoordinateType_Coroutine(__instance, type, __state));
        }

        //private static IEnumerator ChaControl_ChangeCoordinateType_Coroutine(ChaControl __instance, ChaFileDefine.CoordinateType type, int _prev)
        //{
        //    yield return Toolbox.WaitForEndOfFrame;
        //    yield return Toolbox.WaitForEndOfFrame;
        //    OnChangeCoordinateType?.Invoke(null, new ChangeCoordinateTypeEventArgs(__instance, (int)type, "Coroutine", _prev));
        //}
    }
}
