//
// Hooks
//
using System;

using ActionGame.Chara;

using HarmonyLib;

using KKAPI;
using KKAPI.MainGame;

using IDHIUtils;

namespace IDHIPlugins
{
    internal partial class Hooks
    {
        internal static Harmony _hookInstance;

        public static void Init()
        {
            _hookInstance = Harmony.CreateAndPatchAll(typeof(Hooks));
        }

        /// <summary>
        /// Set the new original position when changing positions via the H point picker scene
        /// </summary>
        ///
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaControl),
            nameof(ChaControl.ChangeCoordinateTypeAndReload),
            new Type[]
                {
                    typeof(ChaFileDefine.CoordinateType),
                    typeof(bool)
                }
            )]
        private static bool ChangeCoordinateTypeAndReloadPrefix(
            ChaControl __instance,
            ref ChaFileDefine.CoordinateType type,
            bool changeBackCoordinateType)
        {
            if ((KoikatuAPI.GetCurrentGameMode() != GameMode.MainGame)
                || (__instance.sex == (byte)Sex.Male))
            {
                return true;
            }

            if (type == ChaFileDefine.CoordinateType.Plain)
            {
                var ctrl = RandomCoordinatePlugin.GetController(__instance);
                if (ctrl != null)
                {
                    var newType = ctrl.NowRandomCoordinateMethod(type);
                    type = (ChaFileDefine.CoordinateType)ctrl
                        .NowRandomCoordinateMethod(type);
#if DEBUG
                    RandomCoordinatePlugin._Log
                        .Warning("[ChangeCoordinateTypeAndReload] Name=" +
                        $"{ctrl.ChaControl.GetHeroine().Name.Trim()} " +
                        $"NowRandomCoordinate[type]={(int)type} " +
                        $"NowRandomCoordinate={newType} " +
                        $"Flag={Manager.Character.enableCharaLoadGCClear} sex={__instance.sex}");
#endif
                }
            }

            return true;
        }

        /// <summary>
        /// Postfix for NPC.SynchroCoordinate when is not Bathing type
        /// use a random coordinate if character has more than 4
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="isRemove"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NPC), nameof(NPC.SynchroCoordinate))]
        private static void SynchroCoordinatePostfix(NPC __instance, bool isRemove)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.MainGame)
            {
                return;
            }
            if ((__instance.chaCtrl == null)
                || (__instance.chaCtrl.sex == (byte)Sex.Male))
            {
                return;
            }

            var totalCoordinates = __instance.chaCtrl.chaFile.coordinate.Length;
            var ctrl = RandomCoordinatePlugin.GetController(__instance.chaCtrl);
#if DEBUG
            RandomCoordinatePlugin._Log.Warning("[SyncroCoordinate] 01 Name=" +
                $"{__instance.heroine.Name.Trim()}");
#endif
            // If 4 or less no extra coordinates
            if (totalCoordinates <= 4)
            {
#if DEBUG
                RandomCoordinatePlugin._Log.Warning("[SyncroCoordinate] 02 "
                    + $"Name={__instance.heroine.Name.Trim()} Nothing to "
                    + "do move along!!.");
#endif
                return;
            }
            
            var coordinateNumber = __instance.heroine.NowCoordinate;
            var coordinateType = __instance.chaCtrl.fileStatus.coordinateType;

            var nowCoordinate = coordinateNumber;
            var nowRandomCoordinate = ctrl.NowRandomCoordinateMethod(
                    (ChaFileDefine.CoordinateType)coordinateType);
            var newType = -1;

            // Test for out of bounds adjusting for each character
            if (!MathfEx.RangeEqualOn(0, coordinateNumber, (totalCoordinates - 1)))
            {
                coordinateNumber = 0;
            }
            // For Bathing is the standard game code
            else if (coordinateNumber == 3)
            {
                switch (__instance.mapNo)
                {
                    case 13: // Hotel Public Bath
                    case 14: // Hotel Shower Room
                    case 17: // Hotel Changing Room
                        break;
                    case 36: // Hotel Suite
                        if (__instance.AI.actionNo == 23)
                        {
                            coordinateNumber = 0;
                        }
                        break;
                    default:
                        coordinateNumber = 0;
                        break;
                }
            }

            if (ctrl.FirstRun)
            {
                newType = ctrl.GetRandomCoordinateType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                coordinateNumber = newType;
            }
            else if (coordinateNumber != 3)
            {
                if (RandomCoordinatePlugin.OnlyChangingRoom.Value)
                {
                    switch (__instance.mapNo)
                    {
                        case 17: // Hotel Changing Room
                        case 33: // Hotel Change Room
                            newType = ctrl.GetRandomCoordinateType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                            coordinateNumber = newType;
                            break;
                        default:
                            // Preserve random selection??
                            coordinateNumber = nowRandomCoordinate;
                            break;
                    }
                }
                else
                {
                    newType = ctrl.GetRandomCoordinateType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                    coordinateNumber = newType;
                }
            }

            if (coordinateType != coordinateNumber)
            {
                Manager.Character.enableCharaLoadGCClear = false;
                __instance.chaCtrl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)coordinateNumber);
                Manager.Character.enableCharaLoadGCClear = true;
#if DEBUG
                RandomCoordinatePlugin._Log.Warning($"[SynchroCoordinate] 03 " +
                    $"Name={__instance.heroine.Name.Trim()} in map={__instance.mapNo} " +
                    $"NowRandomCoordinate={nowRandomCoordinate} " +
                    $"newType={newType} " +
                    $"NowCoordinate={nowCoordinate} " +
                    $"coordinateType={(ChaFileDefine.CoordinateType)coordinateType} " +
                    $"determined coordinate={(ChaFileDefine.CoordinateType)coordinateNumber} " +
                    "CHANGE");
#endif
            }
#if DEBUG
            else
            {
                RandomCoordinatePlugin._Log.Warning("[SyncroCoordinate] 04 Name=" +
                    $"{__instance.heroine.Name.Trim()} no action taken");
            }
#endif
            if (isRemove)
            {
                __instance.chaCtrl.RandomChangeOfClothesLowPoly(__instance.heroine.lewdness);
            }
        }
    }
}
