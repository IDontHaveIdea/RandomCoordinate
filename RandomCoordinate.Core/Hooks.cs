//
// Hooks
//
// TODO: Remove bloat

using System;

using ActionGame.Chara;

using HarmonyLib;

using KKAPI;

using IDHIUtils;
using Utils = IDHIUtils.Utilities;
using KKAPI.MainGame;


namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugIn
    {
        internal partial class RandomCoordinatePlugInHooks
        {
            internal static Harmony _hookInstance;

            public static void Init()
            {
                _hookInstance = Harmony.CreateAndPatchAll(typeof(RandomCoordinatePlugInHooks));
            }

            /// <summary>
            /// Control the coordinate use in the room maintain the one use when entering the room
            /// or change to pajamas if option is set
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="_nextAinmInfo"></param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
            private static void ChangeAnimatorPostfix(HSceneProc __instance)
            {
                if (KoikatuAPI.GetCurrentGameMode() != GameMode.MainGame)
                {
                    return;
                }

                // GameMode.MainGame is also valid for FreeH
                var flags = __instance.flags;
                if (flags.isFreeH)
                {
                    return;
                }

                var actScene = ActionScene.instance;
                var currentMapNo = actScene.Map.no;

                var myRoom = currentMapNo == 10
                    || currentMapNo == 18
                    || currentMapNo == 22;

                if (myRoom)
                {
                    // Default coordinate
                    var coordinate = ChaFileDefine.CoordinateType.Plain;
                    // Get controller for heroine
                    var ctrl = GetController(flags.lstHeroine[0].chaCtrl);
                    
                    if (ctrl != null)
                    {
                        // assign coordinate if controller found
                        coordinate = (ChaFileDefine.CoordinateType)ctrl.PreviousRandomCoordinate();
                    }

                    if (PajamasInRoom.Value)
                    {
                        // assign pajamas if option is set
                        coordinate = ChaFileDefine.CoordinateType.Pajamas;
                    }

                    // assign new coordinate
                    if (coordinate != ChaFileDefine.CoordinateType.Plain)
                    {
                        var female = flags.lstHeroine[0].chaCtrl;
                        female.ChangeCoordinateTypeAndReload(coordinate);
                    }
                }
            }

            /// <summary>
            /// First hook on loading characters
            /// </summary>
            /// <param name="__instance">ChaControl</param>
            /// <param name="type">Coordinate</param>
            /// <param name="changeBackCoordinateType"></param>
            /// <returns></returns>
            [HarmonyPrefix]
            [HarmonyPatch(
                typeof(ChaControl),
                nameof(ChaControl.ChangeCoordinateType),
                [
                    typeof(ChaFileDefine.CoordinateType),
                typeof(bool)
                ]
                )]
            private static bool ChangeCoordinateTypePrefix(
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

                try
                {
                    var name = Utils.TranslateName(Utilities.GirlName(__instance), true) +
                        $" ({__instance.name})";

                    var girlKey = Utilities.PseudoKey(__instance);

                    // Check cache if key not found initialize it
                    if (!GirlsRandomData.ContainsKey(girlKey))
                    {
                        // Set cache to current parameter type
                        // This is the first of the hooks to execute when loading.
                        // This condition triggers one time per period or save/load.
#if DEBUG
                        _Log.Warning($"[ChangeCoordinateTypePrefix] 0000: " +
                            $"Name={name} adding data to cache for type={type}.");
#endif
                        var categoryType = Utilities.GetCoordinateType(__instance, (int)type);
                        GirlsRandomData.Add(girlKey, new RandomData(categoryType, (int)type, __instance));

                        // Anything beyond this point is irrelevant so return here
                        return true;
                    }
#if DEBUG
                    //var ctrl = GetController(__instance);
                    //_Log.Warning($"[ChangeCoordinateTypePrefix] 0001: " +
                    //        $"Name={name} type={type}.");

                    //if (ctrl != null)
                    //{
                    //    _Log.Warning($"[ChangeCoordinateTypePrefix] 0002: " +
                    //        $"Name={name} type={type} randomByType={ctrl.GetRandomCoordinateByType(type)}.");
                    //}
#endif
                }
                catch (Exception e)
                {
                    _Log.Error($"[ChangeCoordinateTypePrefix] Error: {e.Message}");
                }
                return true;
            }

            /// <summary>
            /// Prefix for ChaControl.ChangeCoordinateTypeAndReload to manipulate
            /// the value of the coordinate type
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(
                typeof(ChaControl),
                nameof(ChaControl.ChangeCoordinateTypeAndReload),
                [
                    typeof(ChaFileDefine.CoordinateType),
                typeof(bool)
                ]
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

                var name = Utils.TranslateName(Utilities.GirlName(__instance), true) +
                    $" ({__instance.name})";

                var ctrl = GetController(__instance);
                if (ctrl == null)
                {
                    _Log.Error($"[ChangeCoordinateTypeAndReloadPrefix] YYYY: Name={name} controller null.");
                    return true;
                }

                var categoryType = Utilities.GetCoordinateType(__instance, (int)type);

                if (ctrl != null)
                {
                    var mapInfo = ctrl.GetMapInfo();
                    var callingType = type;

                    if ((__instance == _guide?.chaCtrl))
                    {
                        if (_guideMapNo == 4)
                        {
                            // Guide: Change to swimsuit if on the beach
                            type = ChaFileDefine.CoordinateType.Swim;
                            return true;
                        }
                    }

                    if (!ctrl.HasMoreOutfits)
                    {
                        _Log.Debug($"[ChangeCoordinateTypeAndReloadPrefix] 0000: Name={name} " +
                            $"{mapInfo} total coordinates={ctrl.TotalCoordinates} " +
                            "not enough coordinates.");
                        return true;
                    }

                    // Preserve current random coordinate for type change request
                    var randomCoordinateByType = ctrl.GetRandomCoordinateByType(categoryType);
                    if ((randomCoordinateByType >= 0)
                        && (type != (ChaFileDefine.CoordinateType)randomCoordinateByType))
                    {
                        type = (ChaFileDefine.CoordinateType)randomCoordinateByType;
                    }

                    if (DebugInfo.Value)
                    {
                        if (callingType != type)
                        {
                            var callName = "";
                            var newName = "";

                            if ((int)callingType > 3)
                            {
                                callName = $" ({_MoreOutfits
                                    .GetCoordinateName(__instance, (int)callingType)})";
                            }

                            if ((int)type > 3)
                            {
                                newName = $" ({_MoreOutfits
                                    .GetCoordinateName(__instance, (int)type)})";
                            }

                            _Log.Debug($"[ChangeCoordinateTypeAndReloadPrefix] 0001: Name={name} " +
                                $"{mapInfo} paramType={callingType}{callName} " +
                                $"set type={type}{newName}. Called by {Utils.CallingMethod(4)}");
                        }
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

                var name = Utils.TranslateName(Utilities.GirlName(__instance), true) +
                    $" ({__instance.chaCtrl.name})";

                var ctrl = GetController(__instance.chaCtrl);

                if (ctrl == null)
                {
                    _Log.Error($"[SynchroCoordinatePostfix] XXXX: Name={name} controller null.");
                    return;
                }

                var totalCoordinates = ctrl.TotalCoordinates;
                var mapNo = Utils.MapNumber(__instance);
                var mapInfo = ctrl.GetMapInfo();

                // If there no extra outfits
                if (!ctrl.HasMoreOutfits)
                {
                    _Log.Debug($"[SynchroCoordinatePostfix] 0000: Name={name} {mapInfo} " +
                        $"total coordinates={totalCoordinates} not enough coordinates.");
                    return;
                }

                // Active coordinate this happens to be the same shit
                var coordinateNumber = __instance.heroine.StatusCoordinate;
                var nowCoordinate = __instance.chaCtrl.fileStatus.coordinateType;

                // Test for out of bounds adjusting for each character
                if (!MathfEx.RangeEqualOn(0, coordinateNumber, (totalCoordinates - 1)))
                {
                    // Assign Plain coordinate
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

                var nowRandomCoordinate = ctrl.GetRandomCoordinateByType(nowCoordinate);
                var newCoordinate = -1;
                // On first run (of OnReload) get a random coordinate. This causes to have
                // more variety whenever a start game, change period load a save game a
                // random coordinate will be selected.
                var firstRun = ctrl.FirstRun();

                if (firstRun)
                {
                    newCoordinate = ctrl.NewRandomCoordinateByType(
                                    (ChaFileDefine.CoordinateType)nowCoordinate);
#if DEBUG
                    if (newCoordinate != nowCoordinate)
                    {
                        _Log.Error($"[SynchroCoordinatePostfix] 0001: Name={name} {mapInfo} " +
                            $"first run coordinateType={nowCoordinate} " +
                            $"coordinateNumber={coordinateNumber} " +
                            $"newCoordinate={newCoordinate}.");
                    }
#endif
                    coordinateNumber = newCoordinate;
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
                            // Beach Changing Room
                            case 33:
                                // Get new random coordinate in Hotel and Beach changing rooms
                                newCoordinate = ctrl.NewRandomCoordinateByType(
                                    (ChaFileDefine.CoordinateType)nowCoordinate);
#if DEBUG
                                if (newCoordinate != nowCoordinate)
                                {
                                    _Log.Error($"[SynchroCoordinatePostfix] 0002: Name={name} " +
                                        $"{mapInfo} changing room coordinateType={nowCoordinate} " +
                                        $"coordinateNumber={coordinateNumber} " +
                                        $"newCoordinate={newCoordinate}.");
                                }
#endif
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
                        // Get new random coordinate on every opportunity
                        newCoordinate = ctrl.NewRandomCoordinateByType(
                                    (ChaFileDefine.CoordinateType)nowCoordinate);
#if DEBUG
                        if (newCoordinate != nowCoordinate)
                        {
                            _Log.Error($"[SynchroCoordinatePostfix] 0003: Name={name} " +
                                $"{mapInfo} free for all coordinateType={nowCoordinate} " +
                                $"coordinateNumber={coordinateNumber} " +
                                $"newCoordinate={newCoordinate}.");
                        }
#endif
                        coordinateNumber = newCoordinate;
                    }
                }

                if (nowCoordinate != coordinateNumber)
                {
                    ChangeCoordinate(__instance, coordinateNumber);

                    if (DebugInfo.Value)
                    {
                        // Change to new coordinate
                        var nowName = $" {(ChaFileDefine.CoordinateType)nowCoordinate}";
                        var newName = $" {(ChaFileDefine.CoordinateType)coordinateNumber}";

                        if (nowCoordinate > 3)
                        {
                            nowName = $" ({_MoreOutfits
                                .GetCoordinateName(__instance.chaCtrl, nowCoordinate)})";
                        }
                        if (coordinateNumber > 3)
                        {
                            newName = $" ({_MoreOutfits
                                .GetCoordinateName(__instance.chaCtrl, coordinateNumber)})";
                        }

                        _Log.Debug($"[SynchroCoordinatePostfix] 0004: Name={name} in " +
                            $"{mapInfo} current coordinate={nowCoordinate}{nowName} " +
                            $"new={coordinateNumber}{newName}.");
                    }
                }

                //if (isRemove)
                //{
                //    _Log.Debug($"[SynchroCoordinate] 0007: Name={name} {mapInfo} " +
                //        $"calling RandomChangeOfClothesLowPoly.");
                //    __instance.chaCtrl.RandomChangeOfClothesLowPoly(
                //        __instance.heroine.lewdness);
                //}
            }
        }
    }
}
