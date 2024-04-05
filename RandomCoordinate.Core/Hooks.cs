//
// Hooks
//
// TODO: Remove bloath
using System;
using ActionGame.Chara;

using HarmonyLib;

using KKAPI;

using IDHIUtils;
using Utils = IDHIUtils.Utilities;

using static IDHIPlugins.RandomCoordinatePlugin;
using KKAPI.MainGame;
using static ChaFileDefine;
using System.Runtime.Remoting.Metadata.W3cXsd2001;


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
        private static void ChangeAnimatorPostfix(HSceneProc __instance)
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
                female.ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.Pajamas);
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
            var name = Utils.TranslateName(Utilities.GirlName(__instance), true) +
                $" ({__instance.name})";
#if DEBUG
            //_Log.Warning($"[ChangeCoordinateTypePrefix] 0000: " +
            //    $"Name={name} called for type={type}.");
#endif
            try
            {
                var callingType = type;
                var categoryType = _nullType;
                var girlKey = Utilities.PseudoKey(__instance);

                // Check cache if key not found create entry
                if (!GirlsRandomData.ContainsKey(girlKey))
                {
                    // Set cache to current parameter type
                    // This is the first of the hook to execute when loading.
                    // This condition triggers one time per period or save loaded.
#if DEBUG
                    _Log.Warning($"[ChangeCoordinateTypePrefix] 0001: " +
                        $"Name={name} adding data to cache for type={callingType}.");
#endif
                    categoryType = Utilities.GetCoordinateType(__instance, (int)type);
                    GirlsRandomData.Add(girlKey, new RandomData(categoryType, (int)type, __instance));

                    // Anything beyond this point is irrevelant so return here
                    return true;
                }

                var randomCoordinateByType = -1;
                var randomType = _nullType;

                if (GirlsRandomData.TryGetValue(girlKey, out var girlInfo))
                {
                    categoryType = girlInfo.GetCategoryType(callingType);
                    randomCoordinateByType = girlInfo.CoordinateByType[categoryType];
                    randomType = girlInfo.CategoryType;
                }

                // TODO: Delete this conditional not necessary so far never triggered on this
                // condition
                if ((randomType == categoryType)
                    && (type != (ChaFileDefine.CoordinateType)randomCoordinateByType))
                {
                    // If type is the current random type set coordinate accordingly
                    type = (ChaFileDefine.CoordinateType)randomCoordinateByType;
#if DEBUG
                    var mapNo = Utils.MapNumber(__instance);
                    var mapName = Utils.MapName(__instance);

                    var callName = $"{callingType}";
                    var newName = $"{type}";

                    var nowCoordinate = __instance.fileStatus.coordinateType;

                    if (nowCoordinate > 3)
                    {
                        callName = $" ({_MoreOutfits
                            .GetCoordinateName(__instance, callingType)})";
                    }
                    if ((int)type > 3)
                    {
                        newName = $" ({_MoreOutfits
                            .GetCoordinateName(__instance, type)})";
                    }

                    _Log.Warning($"[ChangeCoordinateTypePrefix] 0002: Name={name} " +
                        $"map={mapNo} ({mapName}) " +
                        $"d={callingType != type} " +
                        $"categoryType={categoryType} " +
                        $"randomType={randomType} " +
                        $"randomCoord={girlInfo.CoordinateNumber} " +
                        $"randomCoordByType[{categoryType}]={randomCoordinateByType} " +
                        $"paramType={callingType}{callName} " +
                        $"set type={type}{newName}.");
#endif
                }
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
            //_Log.Warning($"[ChangeCoordinateTypeAndReloadPrefix] 0000: Called " +
            //    $"name={name} called for type={type}.");
#endif
            var categoryType = Utilities.GetCoordinateType(__instance, (int)type);

            var randomCoordinateByType = -1;
            var randomCoordinate = -1;
            var randomType = _nullType;

            if (GirlsRandomData.TryGetValue(Utilities.PseudoKey(__instance), out var girlInfo))
            {
                randomCoordinateByType = girlInfo.CoordinateByType[categoryType];
                randomCoordinate = girlInfo.CoordinateNumber;
                randomType = girlInfo.CategoryType;
            }

            var mapNo = Utils.MapNumber(__instance);
            var mapName = Utils.MapName(__instance);

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

            if (callingType != type)
            {
                _Log.Warning($"[ChangeCoordinateTypeAndReloadPrefix] 0004: Name={name} " +
                    $"map={mapNo} ({mapName}) " +
                    $"randomType={randomType} " +
                    $"randomCoord={randomCoordinate} " +
                    $"randomCoordByType[{categoryType}]={randomCoordinateByType} " +
                    $"paramType={callingType}{callName} " +
                    $"set type={type}{newName}.");
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
                return;
            }

            var actScene = ActionScene.instance;
            var currentMapNo = actScene.Map.no;

            // Active coordinate this happens to be the same shit
            var coordinateNumber = __instance.heroine.StatusCoordinate;
            var nowCoordinate = __instance.chaCtrl.fileStatus.coordinateType;
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
                    $"Name={name} no cache.");
#endif
                return;
            }

            var totalCoordinates = __instance.chaCtrl.chaFile.coordinate.Length;
            var mapNo = Utils.MapNumber(__instance);
            var mapName = Utils.MapName(__instance);

            // If there no extra outfits
            if (!girlInfo.HasMoreOutfits)
            {
                _Log.Debug($"[SynchroCoordinatePostfix] 0004: Name={name} map={mapNo} " +
                    $"({mapName}) total coordinates={totalCoordinates} not enough coordinates.");
                return;
            }

            //var nowCoordinate = coordinateNumber;
            var nowCategoryType = categoryType;
            var nowCoordinateByType = girlInfo.CoordinateByType[nowCategoryType];

            var randomCoordinate = girlInfo.CoordinateNumber;
            var randomType = girlInfo.CategoryType;
            var randomCoordinateByType = girlInfo.CoordinateByType[randomType];
            
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
                                (ChaFileDefine.CoordinateType)nowCoordinate);
#if DEBUG
                if (newCoordinate != coordinateNumber)
                {
                    cache = girlInfo.ToString(categoryType);
                    _Log.Error($"[SynchroCoordinatePostfix] 0005: Name={name} in map={mapNo} " +
                        $"({mapName}) coordinateType={nowCoordinate} " +
                        $"coordinateNumber={coordinateNumber} " +
                        $"newCoordinate={newCoordinate} cache={cache}.");
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
                        // Beach Change Room
                        case 33:
                            // Get new random coordinate in Hotel and Beach changing rooms
                            newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)nowCoordinate);
                            coordinateNumber = newCoordinate;
                            break;
                        default:
                            // Preserve random selection
                            coordinateNumber = nowCoordinateByType;
                            break;
                    }
                }
                else
                {
                    // Get new random coordinate on every oportunity
                    newCoordinate = ctrl.NewRandomCoordinateByType(
                                (ChaFileDefine.CoordinateType)nowCoordinate);
                    coordinateNumber = newCoordinate;
                }
            }

            if (nowCoordinate != coordinateNumber)
            {
                // Change to new coordinate
                var nowName = $"{(ChaFileDefine.CoordinateType)nowCoordinate}";
                var newName = $"{(ChaFileDefine.CoordinateType)coordinateNumber}";

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

                ChangeCoordinate(__instance, coordinateNumber);
#if DEBUG
                _Log.Debug($"[SynchroCoordinatePostfix] 0006: Name={name} in " +
                    $"map={mapNo} ({mapName}) " +
                    $"current coordinate={nowCoordinate}{nowName} " +
                    $"new={coordinateNumber}{newName}. Cache={cache}.");
#endif
            }

            if (isRemove)
            {
                _Log.Debug($"[SynchroCoordinate] 0007: Name={name} in map={mapNo} " +
                    $"({mapName}) calling RandomChangeOfClothesLowPoly.");
                __instance.chaCtrl.RandomChangeOfClothesLowPoly(
                    __instance.heroine.lewdness);
            }
        }
    }
}
