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
            if (Utils.InHScene)
            {
                return true;
            }
            var mapNo = Utils.MapNumber(__instance);
#if DEBUG
            var name = Utilities.GirlName(__instance);
            /*if (name == null)
            {
                if (GirlsNames.ContainsKey(__instance.name))
                {
                    name = Utilities.GirlName(__instance);
                }
                else
                {
                    name = "Error not found!";
                }
                _Log.Error(
                    "[ChangeCoordinateTypeAndReload] 01 Name=" +
                    $"{name} from backup MapNo={mapNo} " +
                    $"girl=[{__instance.name}] " +
                    $"type={type} " +
                    $"Flag={Manager.Character.enableCharaLoadGCClear}");
            }*/
#endif
            if (__instance.chaFile.coordinate.Length <= 4)
            {
                return true;
            }

            if (type == ChaFileDefine.CoordinateType.Plain)
            {
                var ctrl = GetController(__instance);
                if (ctrl != null)
                {
                    var newRandomCoordinate = ctrl.NowRandomCoordinateByType(type);
                    var randomCoordinateType = ctrl.GetCoordinateType(newRandomCoordinate);
                    type = randomCoordinateType;
                        
#if DEBUG
                    _Log.Warning("[ChangeCoordinateTypeAndReload] 02 Name=" +
                        $"{name} " +
                        $"MapNo={mapNo} " +
                        $"randomCoordinateType={randomCoordinateType} " +
                        $"newRandomCoordinate={newRandomCoordinate} " +
                        $"Flag={Manager.Character.enableCharaLoadGCClear} " +
                        $"sex={__instance.sex}");
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

            // If 4 or less no extra coordinates
            if (totalCoordinates <= 4)
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
                if (RandomCoordinatePlugin.OnlyChangingRoom.Value)
                {
                    // Get a random coordinate is girl is one changing room
                    switch (__instance.mapNo)
                    {
                        case 17: // Hotel Changing Room
                        case 33: // Hotel Change Room
                            newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                            coordinateNumber = newCoordinate;
                            break;
                        default:
                            // Preserve random selection??
                            coordinateNumber = nowRandomCoordinate;
                            break;
                    }
                }
                else
                {
                    // Try and preserve current random coordinate
                    newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                    coordinateNumber = newCoordinate;
                }
            }

            if (coordinateType != coordinateNumber)
            {
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
#endif
            }
#if DEBUG
            else
            {
                _Log.Warning("[SyncroCoordinate] 04 Name=" +
                    $"{Utilities.GirlName(__instance)} " +
                    $"coordinateType={coordinateType} " +
                    $"coordianteNumber={coordinateNumber} " +
                    $"nowRandomCoordinate={nowRandomCoordinate} " +
                    $"dictNowCoordinate={dictNowCoordinate} no action taken");
            }
#endif
            if (isRemove)
            {
                __instance.chaCtrl.RandomChangeOfClothesLowPoly(__instance.heroine.lewdness);
            }
        }
    }
}
