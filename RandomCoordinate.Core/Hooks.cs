//
// Hooks
//
using System;

using ActionGame.Chara;
using UnityEngine;

using HarmonyLib;

using KKAPI;

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
        [HarmonyPatch(
            typeof(ChaControl),
            nameof(ChaControl.ChangeCoordinateTypeAndReload),
            new Type[]
                {
                    typeof(ChaFileDefine.CoordinateType),
                    typeof(bool)
                }
            )]
        private static bool ChangeCoordinateTypeAndReloadPrefix(
            ChaControl __instance,
            ref ChaFileDefine.CoordinateType type)
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

            var nowRandomCoordinate = -1;
            var ctrl = GetController(__instance);
            if (ctrl != null)
            {
                nowRandomCoordinate = ctrl.NowRandomCoordinateByType(type);
            }

            var mapNo = Utils.MapNumber(__instance);
            var mapName = Utils.MapName(__instance);
            var name = Utilities.GirlName(__instance);
            var callingType = type;

            var callName = "";
            var newName = ".";
            

            if ((int)callingType > 3)
            {
                callName = $"({_MoreOutfits
                    .GetCoordinateName(__instance, (int)callingType)}) ";
            }

            if ((__instance == _guide.chaCtrl))
            {
                if (_guideMapNo == 4)
                {
                    // Guide: Change to swimsuit if on the beach
                    type = ChaFileDefine.CoordinateType.Swim;
                    if ((int)type > 3)
                    {
                        newName = $" ({_MoreOutfits
                            .GetCoordinateName(__instance, (int)type)}).";
                    }
                    _Log.Debug($"[ChangeCoordinateTypeAndReload] Guide name={name} on " +
                        $"map={mapNo} mapName={mapName} " +
                        $"nowRandomCoordinate={nowRandomCoordinate} " +
                        $"parameter type={callingType} {callName}set " +
                        $"type={type}{newName}");
                    return true;
                }
            }
            if (__instance.chaFile.coordinate.Length <= 4)
            {
                return true;
            }
            if (type == ChaFileDefine.CoordinateType.Plain)
            {
                //ctrl = GetController(__instance);

                if (ctrl != null)
                {
                    // Preserve current random coordinate for type change request
                    //nowRandomCoordinate =
                    //    ctrl.NowRandomCoordinateByType(type);
                    if (nowRandomCoordinate >= 0)
                    {
                        type = (ChaFileDefine.CoordinateType)nowRandomCoordinate;
                    }
                }
            }

            if ((int)type > 3)
            {
                newName = $" ({_MoreOutfits
                    .GetCoordinateName(__instance, (int)type)}).";
            }
            _Log.Debug($"[ChangeCoordinateTypeAndReload] Name={name} on " +
                $"map={mapNo} mapName={mapName} nowRandomCoordinate={nowRandomCoordinate}" +
                $" parameter type={callingType} {callName}set type={type}{newName}");
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
            var mapNo = Utils.MapNumber(__instance);
            var mapName = Utils.MapName(__instance);

            // If there no extra outfits
            if (!ctrl.HasMoreOutfits)
            {
                _Log.Debug($"[SynchroCoordinate] Name={name} in map={mapNo} " +
                    $"mapName={mapName} total coordinates={totalCoordinates}.");
                return;
            }

            var coordinateNumber = __instance.heroine.StatusCoordinate;
            var coordinateType = __instance.chaCtrl.fileStatus.coordinateType;
            var nowCoordinate = coordinateNumber;
            var nowRandomCoordinate = ctrl.NowRandomCoordinateByType(
                    (ChaFileDefine.CoordinateType)coordinateType);

            // Test for out of bounds adjusting for each character
            if (!MathfEx.RangeEqualOn(0, coordinateNumber, (totalCoordinates - 1)))
            {
                coordinateNumber = 0;
            }
            // For Bathing is the standard game code
            else if (coordinateNumber == 3)
            {
                switch (mapNo)
                {
                    // Hotel Public Bath
                    case 13:
                    // Hotel Shower Room
                    case 14:
                    // Hotel Changing Room
                    case 17:
                        break;
                    // Hotel Suite
                    case 36:
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

            int newCoordinate;
            // On first run (of OnReload) get a random coordinate
            // This causes to have more variety whenever a start game, change period
            // load a save game a random coordinate will be selected.
            // FirstRun unreliable game reinitializes controller
            if (ctrl.FirstRun)
            {
                newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                coordinateNumber = newCoordinate;
#if DEBUG
                _Log.Error($"[SynchroCoordinate] Name={name} in map={mapNo} " +
                    $"mapName={mapName} FirstRun.");
#endif
            }
            // coordinateNumber not equal to Bathing
            else if (coordinateNumber != 3)
            {
                if (OnlyChangingRoom.Value)
                {
                    // Get a random coordinate if girl is in a changing room
                    switch (mapNo)
                    {
                        // Hotel Changing Room
                        case 17:
                        // Beach Change Room
                        case 33:
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
                var nowName = "";
                var newName = ".";
                if (nowCoordinate > 3)
                {
                    nowName = $"({_MoreOutfits
                        .GetCoordinateName(__instance.chaCtrl, nowCoordinate)}) ";
                }
                if (coordinateNumber > 3)
                {
                    newName = $" ({_MoreOutfits
                        .GetCoordinateName(__instance.chaCtrl, coordinateNumber)}).";
                }
                ChangeCoordinate(__instance, coordinateNumber);
                _Log.Debug($"[SynchroCoordinate] Name={name} in map={mapNo} " +
                    $"mapName={mapName} " +
                    $"current coordinate={nowCoordinate} {nowName}" +
                    $"new coordinate={coordinateNumber}{newName}");
            }
            if (isRemove)
            {
                _Log.Debug($"[SynchroCoordinate] Name={name} in map={mapNo} " +
                    $"mapName={mapName} calling RandomChangeOfClothesLowPoly.");
                __instance.chaCtrl.RandomChangeOfClothesLowPoly(
                    __instance.heroine.lewdness);
            }
        }
    }
}
