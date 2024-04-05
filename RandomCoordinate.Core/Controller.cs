//
// RandomCoordinateController
//
using System;
using System.Collections.Generic;
using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;

using static IDHIPlugins.RandomCoordinatePlugin;


namespace IDHIPlugins
{
    public partial class RandomCoordinateController : CharaCustomFunctionController
    {
        #region properties
        public bool HasMoreOutfits => (ChaFileControl.coordinate.Length > 4);
        #endregion

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (maintainState)
            {
                return;
            }

            if (currentGameMode != GameMode.MainGame)
            {
                return;
            }

            var heroine = ChaControl.GetHeroine();
            
            if (heroine != null)
            {
                var girlKey = Utilities.PseudoKey(heroine.chaCtrl);

                // Sometimes the controller is not loaded maintain a cache lookup table
                if (!GirlsRandomData.ContainsKey(girlKey))
                {
#if DEBUG
                    _Log.Warning($"[OnReload] 0000: Name={heroine.Name.Trim()}" +
                        $"({heroine.chaCtrl.name}) caching info.");
#endif
                    GirlsRandomData.Add(girlKey, new RandomData(heroine));
                }

                if (GirlsRandomData.TryGetValue(girlKey, out var girlInfo))
                {
                    // Initialize the coordinates information
                    var nowRandomCoordinate = heroine.StatusCoordinate;
                    var categoryType = girlInfo.GetCategoryType(heroine.StatusCoordinate);
                    var randomCoordinateByType = girlInfo.CoordinateByType[categoryType];
                    var randomCoordinate = girlInfo.CoordinateNumber;
                    var randomCategory = girlInfo.CategoryType;

                    if (nowRandomCoordinate != randomCoordinateByType)
                    {
                        // Set current random coordinate cache info to what the heroine is
                        // currently wearing. This inconsistency occurs when the game resets the
                        // character controller. Game seems to use the same coordinate for a
                        // save/load cycle.
                        girlInfo.SetRandomData(heroine);
#if DEBUG
                        _Log.Warning($"[OnReload] 0001: Name={heroine.Name.Trim()} " +
                            $"({heroine.chaCtrl.name}) update cache categoryType={categoryType} " +
                            $"nowCoordinate={nowRandomCoordinate} " +
                            $"coordinateBT[{categoryType}]={randomCoordinateByType} " +
                            $"randomCategory={randomCategory} " +
                            $"randomCoordinate={randomCoordinate}.");
#endif
                    }

                    if (heroine.fixCharaID == -13)
                    {
                        // This is the Guide character needs special treatment
                        SetupGuide(heroine, true);
                        categoryType = GetCategoryType(heroine.StatusCoordinate);
#if DEBUG
                        _Log.Warning($"[OnReload] 0002: Name={heroine.Name.Trim()} setup Guide " +
                            $"({heroine.chaCtrl.name}) synchronize info rcByType[{categoryType}]" +
                            $"={heroine.StatusCoordinate} rc={heroine.StatusCoordinate}.");
#endif
                    }
#if DEBUG
                    _Log.Debug($"[OnReload] 0003: " +
                        $"Name={heroine.Name.Trim()} ({heroine.chaCtrl.name}) " +
                        $"heroine.StatusCoordinate={heroine.StatusCoordinate} " +
                        $"nowRCByType[{categoryType}]={randomCoordinateByType} " +
                        $"randomCategory={randomCategory} " +
                        $"randomCoordinate={randomCoordinate} " +
                        $"total coordinates={ChaFileControl.coordinate.Length} " +
                        $"random possible={girlInfo.HasMoreOutfits}.");
#endif
                }
            }
        }

        /// <summary>
        /// Look for first run info in cache.
        /// </summary>
        /// <returns></returns>
        public bool FirstRun()
        {
            var rc = false;

            if (GirlsRandomData.TryGetValue(Utilities.PseudoKey(ChaControl), out var girlInfo))
            {
                rc = girlInfo.FirstRun;
            }
            return rc;
        }

        /// <summary>
        /// Sets first run flag in chache.
        /// </summary>
        /// <param name="status"></param>
        public void FirstRun(bool status)
        {
            if (GirlsRandomData.TryGetValue(Utilities.PseudoKey(ChaControl), out var girlInfo))
            {
                girlInfo.FirstRun = status;
            }
        }

        /// <summary>
        /// If available select a random coordinate for any given coordinate
        /// type.
        /// TODO: For coordinates other than Plain and UI is needed
        ///
        /// * Plain - every additional coordinate outfit will be assumed is
        ///   for the Plain type
        /// * Swim - can work events depend on this
        /// * Pajamas - TODO: test to see if it is possible
        /// * Bathing - no need events depend on this
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int NewRandomCoordinateByType(ChaFileDefine.CoordinateType type)
        {
            if (FirstRun())
            {
                FirstRun(false);
            }

            var _coordinate = type switch {
                ChaFileDefine.CoordinateType.Swim =>
                    (int)ChaFileDefine.CoordinateType.Swim,
                ChaFileDefine.CoordinateType.Pajamas =>
                    (int)ChaFileDefine.CoordinateType.Pajamas,
                ChaFileDefine.CoordinateType.Bathing =>
                    (int)ChaFileDefine.CoordinateType.Bathing,
                _ => RandomCoordinate(type),
            };

            return _coordinate;
        }

        /// <summary>
        /// When coordinate is grater than 3 (Bathing) try and get the corresponding
        /// type. The function is needed if the type selected by the game is
        /// greater then 3.
        /// </summary>
        /// <param name="type">coordinate in the request for random coordinate</param>
        /// <returns></returns>
        public ChaFileDefine.CoordinateType GetCategoryType(int coordinate)
        {
            GirlsRandomData.TryGetValue(Utilities.PseudoKey(ChaControl), out var girlInfo);

            var rc = ChaFileDefine.CoordinateType.Plain;

            if (girlInfo != null)
            {
                rc = girlInfo.GetCategoryType(coordinate);
            }
            return rc;
        }

        /// <summary>
        /// This overload is needed because the game for coordinates > 3 uses the
        /// coordinate number as it type (ex, coordinate 5 has )
        /// </summary>
        /// <param name="type">Type of coordinate</param>
        /// <returns></returns>
        public ChaFileDefine.CoordinateType GetCategoryType(ChaFileDefine.CoordinateType type)
        {
            return GetCategoryType((int)type);
        }

        public int NowRandomCoordinate()
        {
            var rc = -1;
            if (GirlsRandomData.TryGetValue(Utilities.PseudoKey(ChaControl), out var girlInfo))
            {
                rc = girlInfo.CoordinateNumber;
            }
            return rc;
        }

        public ChaFileDefine.CoordinateType NowRandomCategoryType()
        {
            var rc = ChaFileDefine.CoordinateType.Plain;
            if (GirlsRandomData.TryGetValue(Utilities.PseudoKey(ChaControl), out var girlInfo))
            {
                rc = girlInfo.CategoryType;
            }
            return rc;
        }

        /// <summary>
        /// Return the current coordinate for any given type
        /// </summary>
        /// <param name="type">Type of coordinate</param>
        /// <returns></returns>
        public int NowRandomCoordinateByType(ChaFileDefine.CoordinateType type)
        {
            if (GirlsRandomData.TryGetValue(Utilities.PseudoKey(ChaControl), out var girlInfo))
            { 
                var categoryType = girlInfo.GetCategoryType((int)type);

                if (girlInfo.CoordinateByType
                    .TryGetValue(categoryType, out var coordinate))
                {
                    return coordinate;
                }
            }
            return (int)type;
        }

        /// <summary>
        /// Overload NowRandomCoordinate with integer type
        /// </summary>
        /// <param name="type">coordinate type as integer</param>
        /// <returns></returns>
        public int NowRandomCoordinateByType(int type)
        {
            return NowRandomCoordinateByType((ChaFileDefine.CoordinateType)type);
        }

        public void SetRandomCoordinate(ChaFileDefine.CoordinateType type)
        {
            if (GirlsRandomData.TryGetValue(Utilities.PseudoKey(ChaControl), out var girlInfo))
            {
                var categoryType = girlInfo.GetCategoryType((int)type);

                girlInfo.SetRandomData(categoryType, (int)type, (int)type);
            }
        }

        /// <summary>
        /// Get random coordinate according to type classification
        /// (Plain, Swim, ...) Now only working with Plain coordinate
        /// only i.e., when Plain clothes are selected the function will
        /// select among the Plain coordinate number 0 and anything
        /// above 3 (Bathing)
        /// </summary>
        /// <param name="type">Coordinate type selected by the game</param>
        /// <returns></returns>
        private int RandomCoordinate(ChaFileDefine.CoordinateType type)
        {
            var newCoordinate = (int)type;
            var name = Utilities.GirlName(ChaControl) +$" ({ChaControl.name})";

            try
            {
                if (GirlsRandomData.TryGetValue(Utilities.PseudoKey(ChaControl), out var girlInfo))
                {
                    var categoryType = girlInfo.GetCategoryType(type);
                    var tmpCoordinates = girlInfo.CoordinatesByType[categoryType];

                    if (tmpCoordinates.Count > 1)
                    {
                        try
                        {
                            // New random coordinate
                            var currentType = ChaControl.fileStatus.coordinateType;
                            var coordinateIndex = RandCoordinate.Next(0, tmpCoordinates.Count);
                            newCoordinate = tmpCoordinates[coordinateIndex];

                            // save coordinate
                            girlInfo.CategoryType = categoryType;
                            girlInfo.CoordinateNumber = newCoordinate;
                            girlInfo.CoordinateByType[categoryType] = newCoordinate;
#if DEBUG
                            _Log.Warning($"[RandomCoordinate] Name={name} " +
                                $"currentCoordinateType={currentType} " +
                                $"CoordinateByType[{categoryType}]=" +
                                $"{girlInfo.CoordinateByType[categoryType]} " +
                                $"CategoryType={girlInfo.CategoryType} " +
                                $"CoordinateNumber={girlInfo.CoordinateNumber}.");
#endif
                        }
                        catch (Exception e)
                        {
                            _Log.Level(LogLevel.Error, $"[RandomCoordinate] " +
                                $"Name={name} Problem generating random " +
                                $"number categoryType={categoryType} " +
                                $"Error code={e.Message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _Log.Level(LogLevel.Error, $"[RandomCoordinate] " +
                    $"Name={name} Problem " +
                    $"in random coordinate search type={type} " +
                    $"code={e.Message}");
            }
            return newCoordinate;
        }
    }
}
