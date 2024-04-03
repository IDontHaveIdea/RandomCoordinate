//
// Hooks
//
using System;
using ActionGame.Chara;

using HarmonyLib;

using KKAPI;

using IDHIUtils;
using Utils = IDHIUtils.Utilities;

using static IDHIPlugins.RandomCoordinatePlugin;
using ADV.Commands.Chara;
using System.Text.RegularExpressions;


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
        /// Change coordinate to Pajams while in the Room
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_nextAinmInfo"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
        private static void ChangeAnimatorPostfix(
                        HSceneProc __instance,
                        HSceneProc.AnimationListInfo _nextAinmInfo
                    )
        {
            var actScene = ActionScene.instance;
            var currentMapNo = actScene.Map.no;
            var flags = __instance.flags;

            var myRoom = currentMapNo == 10
                || currentMapNo == 18
                || currentMapNo == 22;

            if (myRoom)
            {
                var female = flags.lstHeroine[0].chaCtrl;
                female.ChangeCoordinateTypeAndReload(
                    ChaFileDefine.CoordinateType.Pajamas);
#if DEBUG
                //_Log.Warning("[ChangeAnimatorPostfix] 0000: " +
                //    $"Coordinate set to Pajamas.");
#endif
            }
        }

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
            ChaFileDefine.CoordinateType type,
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
            var name = Utils.TranslateName(Utilities.GirlName(__instance), true) +
                $" ({__instance.name})";
#if DEBUG
            _Log.Warning($"[ChangeCoordinateTypePrefix] 0000: " +
                $"Name={name} called for type={type}.");
#endif
            try
            {
                var girlKey = Utilities.PseudoKey(__instance);
                var randomCoordinateByType = -1;
                var randomCoordinate = -1;
                var randomType = (ChaFileDefine.CoordinateType)(-1);
                var callingType = type;
                var categoryType = ChaFileDefine.CoordinateType.Plain;

                // Check cache if key not found craete entry
                if (!GirlsRandomData.ContainsKey(girlKey))
                {
#if DEBUG
                    _Log.Warning($"[ChangeCoordinateTypePrefix] 0001: " +
                        $"Name={name} Adding data to cache for type={callingType}.");
#endif
                    categoryType = Utilities.GetCoordinateType(__instance, (int)type);
                    GirlsRandomData.Add(girlKey, new RandomData(categoryType, (int)type, __instance));
                }

                if (GirlsRandomData.TryGetValue(girlKey, out var girlInfo))
                {
                    categoryType = girlInfo.GetCategoryType(type);
                    randomCoordinateByType = girlInfo.CoordinateByType[categoryType];
                    randomCoordinate = girlInfo.CoordinateNumber;
                    randomType = girlInfo.CategoryType;
                }

                if (randomType == categoryType)
                {
                    // If type is the current random type set coordinate accordingly
                    type = (ChaFileDefine.CoordinateType)randomCoordinateByType;
                }
#if DEBUG
                var firstRun = girlInfo.FirstRun;
                var actScene = ActionScene.instance;
                var currentMapNo = -1;
                if (actScene != null)
                {
                    currentMapNo = actScene.Map.no;
                }
                var mapNo = Utils.MapNumber(__instance);
                //if (mapNo == currentMapNo)
                //{
                    var mapName = Utils.MapName(__instance);
                    _Log.Warning($"[ChangeCoordinateTypePrefix] 0002: Name={name} on " +
                        $"cMap={currentMapNo} " +
                        $"map={mapNo} ({mapName}) " +
                        $"RCByType[{categoryType}]={randomCoordinateByType} " +
                        $"RT={randomType} " +
                        $"RC={randomCoordinate} " +
                        $"pType={callingType} set type={type} fRun={firstRun}.");
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
#if DEBUG
            _Log.Warning($"[ChangeCoordinateTypeAndReloadPrefix] 0000: Called " +
                $"name={name} called for type={type}.");
#endif
            var randomCoordinateByType = -1;
            var randomCoordinate = -1;
            var plain = -1;
            var randomType = (ChaFileDefine.CoordinateType)(-1);
            var categoryType =
                Utilities.GetCoordinateType(__instance, (int)type);

            if (GirlsRandomData.TryGetValue(Utilities.PseudoKey(__instance), out var girlInfo))
            {
                randomCoordinateByType = girlInfo.CoordinateByType[categoryType];
                plain = girlInfo.CoordinateByType[ChaFileDefine.CoordinateType.Plain];
                randomCoordinate = girlInfo.CoordinateNumber;
                randomType = girlInfo.CategoryType;
#if DEBUG
                //_Log.Warning($"[ChangeCoordinateTypeAndReloadPrefix] 0001: " +
                //    $"Name={name} reading cache. " +
                //    $"nowRCByType[{categoryType}]={nowRandomCoordinateByType} " +
                //    $"nowPlainByType={nowPlain} " +
                //    $"nowRT={nowRandomType} " +
                //    $"nowRC={nowRandomCoordinate}.");

#endif
            }

            var actScene = ActionScene.instance;
            var currentMapNo = actScene.Map.no;
            var mapNo = Utils.MapNumber(__instance);
            var mapName = Utils.MapName(__instance);
            var callingType = type;

            var callName = "";
            var newName = ".";
            
            if ((int)callingType > 3)
            {
                callName = $" ({_MoreOutfits
                    .GetCoordinateName(__instance, (int)callingType)})";
            }

            if ((__instance == _guide?.chaCtrl))
            {
                if (_guideMapNo == 4)
                {
                    // Guide: Change to swimsuit if on the beach
                    type = ChaFileDefine.CoordinateType.Swim;
#if DEBUG
                    _Log.Error($"[ChangeCoordinateTypeAndReloadPrefix] GUIDE: name={name} on " +
                        $"map={mapNo} ({mapName}) " +
                        $"RCByType[{categoryType}]={randomCoordinateByType} " +
                        $"nowPlainByType={plain} " +
                        $"nowRT={randomType} " +
                        $"nowRC={randomCoordinate} " +
                        $"paramT={callingType}{callName} " +
                        $"set type={type}");
#endif
                    return true;
                }
            }
            if (__instance.chaFile.coordinate.Length <= 4)
            {
                return true;
            }
            if (categoryType == ChaFileDefine.CoordinateType.Plain)
            {
                if (girlInfo != null)
                {
                    // Preserve current random coordinate for type change request
                    randomCoordinate = girlInfo.CoordinateByType[categoryType];
                    if (randomCoordinateByType >= 0)
                    {
                        type = (ChaFileDefine.CoordinateType)randomCoordinateByType;
                    }
                }
            }

            if ((int)type > 3)
            {
                newName = $" ({_MoreOutfits
                    .GetCoordinateName(__instance, (int)type)}).";
            }
#if DEBUG
            if (mapNo == currentMapNo)
            {
                _Log.Warning($"[ChangeCoordinateTypeAndReloadPrefix] 0004: Name={name} on " +
                    $"map={mapNo} ({mapName}) " +
                    $"nowRCByType[{categoryType}]={randomCoordinateByType} " +
                    $"nowRT={randomType} " +
                    $"nowRC={randomCoordinate} " +
                    $"paramT={callingType}{callName} " +
                    $"set type={type}{newName}");
            }
#else
            _Log.Debug($"[ChangeCoordinateTypeAndReload] 0005: Name={name} on " +
                $"map={mapNo} mapName={mapName} nowRC={nowRandomCoordinate} " +
                $"paramT={callingType}{callName} set type={type}{newName}");
#endif
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
                return;
            }

            var actScene = ActionScene.instance;
            var currentMapNo = actScene.Map.no;

            // Active coordinate
            var coordinateNumber = __instance.heroine.StatusCoordinate;
            var coordinateType = __instance.chaCtrl.fileStatus.coordinateType;
            ChaFileDefine.CoordinateType categoryType;

            var cache = "";
            if (GirlsRandomData.TryGetValue(Utilities.PseudoKey(__instance.chaCtrl), out var girlInfo))
            {
                categoryType = girlInfo.GetCategoryType(coordinateNumber);
                cache = girlInfo.ToString(categoryType);
#if DEBUG
                _Log.Warning("[SynchroCoordinatePostfix] 0001: " +
                    $"Name={girlInfo.Name} ({girlInfo.CtrlName}) Read cache={cache}.");
#endif
            }
            else
            {
#if DEBUG
                _Log.Warning("[SynchroCoordinatePostfix] 0002: No cache " +
                    $"Name={girlInfo.Name} ({girlInfo.CtrlName}) no cache.");
#endif
                return;
            }

            var totalCoordinates = __instance.chaCtrl.chaFile.coordinate.Length;
            var mapNo = Utils.MapNumber(__instance);
            var mapName = Utils.MapName(__instance);

#if DEBUG
            //if (mapNo == currentMapNo)
            //{
            //    _Log.Warning($"[SynchroCoordinatePostfix] 0003: Name={name} in map={mapNo} " +
            //        $"({mapName}) total coordinates={totalCoordinates} " +
            //        $"actionNo={__instance.AI.actionNo}.");
            //}
#endif
            // If there no extra outfits
            if (!girlInfo.HasMoreOutfits)
            {
                _Log.Debug($"[SynchroCoordinatePostfix] 0004: Name={name} in map={mapNo} " +
                    $"({mapName}) total coordinates={totalCoordinates} not enough coordinates.");
                return;
            }

            var nowCoordinate = coordinateNumber;
            var nowType = (ChaFileDefine.CoordinateType)coordinateType;
            var nowRandomCoordinate = girlInfo.CoordinateNumber;
            var nowRandomType = girlInfo.CategoryType;
            var nowRandomCoordinateByType = girlInfo.CoordinateByType[categoryType];
            
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

            var newCoordinate = -1;
            // On first run (of OnReload) get a random coordinate. This causes to have
            // more variety whenever a start game, change period load a save game a
            // random coordinate will be selected.
            // FirstRun unreliable game reinitializes controller.
            // FirstRun() relies on cache.
            var firstRun = girlInfo.FirstRun;

            if (firstRun)
            {
                newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)coordinateType);
#if DEBUG
                cache = girlInfo.ToString(categoryType);
                _Log.Error($"[SynchroCoordinatePostfix] 0005: Name={name} in map={mapNo} " +
                    $"({mapName}) for FirstRun={girlInfo.FirstRun} " +
                    $"coordinateNumber={coordinateNumber} " +
                    $"newCoordinate={newCoordinate} cache={cache}.");
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
                        // Beach Change Room
                        case 33:
                            // Get new random coordinate in Hotel and Beach changing rooms
                            newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                            coordinateNumber = newCoordinate;
                            break;
                        default:
                            // Preserve random selection
                            coordinateNumber = nowRandomCoordinateByType;
                            break;
                    }
                }
                else
                {
                    // Get new random coordinate on every oportunity
                    newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)coordinateType);
                    coordinateNumber = newCoordinate;
                }
            }
#if !DEBUG
            if (coordinateType != coordinateNumber)
            {
#endif
                // Change to new coordinate
                var nowName = "";
                var newName = ".";
                if (nowCoordinate > 3)
                {
                    nowName = $" ({_MoreOutfits
                        .GetCoordinateName(__instance.chaCtrl, nowCoordinate)})";
                }
                if (coordinateNumber > 3)
                {
                    newName = $" ({_MoreOutfits
                        .GetCoordinateName(__instance.chaCtrl, coordinateNumber)}).";
                }
#if DEBUG
                if (coordinateType != coordinateNumber)
                {
                    ChangeCoordinate(__instance, coordinateNumber);
                }
#else
                ChangeCoordinate(__instance, coordinateNumber);
#endif
                _Log.Debug($"[SynchroCoordinatePostfix] 0006: Name={name} in map={mapNo} " +
                    $"({mapName}) " +
                    $"nowType={nowType} " +
                    $"nowCoord={nowCoordinate} " +
                    $"nowRType={nowRandomType} " +
                    $"nowRCoord={nowRandomCoordinate} " +
                    $"coordinates current={nowCoordinate}{nowName} " +
                    $"new={coordinateNumber}{newName}. Cache={cache}");

#if !DEBUG
            }
#endif

#if !DEBUG
            if (isRemove)
            {
                _Log.Debug($"[SynchroCoordinate] Name={name} in map={mapNo} " +
                    $"mapName={mapName} calling RandomChangeOfClothesLowPoly.");
                __instance.chaCtrl.RandomChangeOfClothesLowPoly(
                    __instance.heroine.lewdness);
            }
#endif
        }
    }
}
