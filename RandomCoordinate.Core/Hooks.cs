//
// Hooks
//
using System;

using ActionGame.Chara;
using UnityEngine;

using HarmonyLib;

using KKAPI;
using KKAPI.MainGame;

using IDHIUtils;
using Utils = IDHIUtils.Utilities;

using static IDHIPlugins.RandomCoordinatePlugin;


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
        /// Prefix for ChaControl.ChangeCoordinateTypeAndReload to manipulate
        /// the value of the coordinate type
        /// </summary>
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
            if (Utils.InHScene)
            {
                return true;
            }
            var mapNo = Utils.MapNumber(__instance);
            var name = Utilities.GirlName(__instance);
            var callingType = type;

            if (__instance.chaFile.coordinate.Length <= 4)
            {
                return true;
            }

            if (type == ChaFileDefine.CoordinateType.Plain)
            {
                var ctrl = GetController(__instance);
                if (ctrl != null)
                {
                    // Preserve current random coordinate for type change request
                    var nowRandomCoordinate =
                        ctrl.NowRandomCoordinateByType(type);
                    if (nowRandomCoordinate >= 0)
                    {
                        type = (ChaFileDefine.CoordinateType)nowRandomCoordinate;
                    }
#if DEBUG
                    var randomCoordinateType =
                        ctrl.GetCoordinateType(nowRandomCoordinate);
                    _Log.Warning("[ChangeCoordinateTypeAndReload] 02 Name=" +
                        $"{name} " +
                        $"MapNo={mapNo} " +
                        $"type={callingType} " +
                        $"randomCoordinateType={randomCoordinateType} " +
                        $"nowRandomCoordinate={nowRandomCoordinate} " +
                        $"Flag={Manager.Character.enableCharaLoadGCClear}");
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
        private static void SynchroCoordinatePostfix(
            NPC __instance,
            bool isRemove)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.MainGame)
            {
                return;
            }
            if (Utils.InHScene)
            {
                return;
            }
            if ((__instance.chaCtrl == null)
                || (__instance.chaCtrl.sex == (byte)Sex.Male))
            {
                return;
            }

            var totalCoordinates = __instance.chaCtrl.chaFile.coordinate.Length;
            var ctrl = GetController(__instance.chaCtrl);
            var name = Utilities.GirlName(__instance);

            // If there no extra outfits
            if (!ctrl.RandomPossible)
            {
#if DEBUG
                _Log.Warning("[SyncroCoordinate] 01 Name=" +
                    $"{name} total coordinates={totalCoordinates}");
#endif
                return;
            }

            var coordinateNumber = __instance.heroine.NowCoordinate;
            var coordinateType = __instance.chaCtrl.fileStatus.coordinateType;

            // Check look up table
            GirlsNowCoordinate.TryGetValue(__instance.chaCtrl.name, out var dictNowCoordinate);

            var nowCoordinate = coordinateNumber;
            var nowRandomCoordinate = ctrl.NowRandomCoordinateByType(
                    (ChaFileDefine.CoordinateType)coordinateType);
            var newCoordinate = -1;

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

            // On first run get a random coordinate
            if (ctrl.FirstRun)
            {
                newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                coordinateNumber = newCoordinate;
            }
            else if (coordinateNumber != 3)
            {
                if (OnlyChangingRoom.Value)
                {
                    // Get a random coordinate if girl is in a changing room
                    switch (__instance.mapNo)
                    {
                        case 17: // Hotel Changing Room
                        case 33: // Hotel Change Room
                            newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                            coordinateNumber = newCoordinate;
                            break;
                        default:
                            // Preserve random selection
                            coordinateNumber = nowRandomCoordinate;
                            break;
                    }
                }
                else
                {
                    // Get new random coordinate
                    newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                    coordinateNumber = newCoordinate;
                }
            }

            if (coordinateType != coordinateNumber)
            {
                // Change to new coordinate
                Manager.Character.enableCharaLoadGCClear = false;
                __instance.chaCtrl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)coordinateNumber);
                Manager.Character.enableCharaLoadGCClear = true;
#if DEBUG
                _Log.Warning($"[SynchroCoordinate] 03 " +
                    $"Name={name} in map={__instance.mapNo} " +
                    $"NowCoordinate={nowCoordinate} " +
                    $"NowRandomCoordinate={nowRandomCoordinate} " +
                    $"dictNowCoordinate={dictNowCoordinate} " +
                    $"newCoordinate={newCoordinate} " +
                    $"coordinateType={(ChaFileDefine.CoordinateType)coordinateType} " +
                    $"CHANGE");
            }
            else
            {
                _Log.Warning("[SyncroCoordinate] 04 Name=" +
                    $"{Utilities.GirlName(__instance)} " +
                    $"coordinateType={coordinateType} " +
                    $"coordianteNumber={coordinateNumber} " +
                    $"nowRandomCoordinate={nowRandomCoordinate} " +
                    $"dictNowCoordinate={dictNowCoordinate} " +
                    $"isRemove={isRemove} " +
                    $"no action taken");
            }
#else
            }
#endif
            if (isRemove)
            {
                __instance.chaCtrl.RandomChangeOfClothesLowPoly(__instance.heroine.lewdness);
            }
        }
    }
}
