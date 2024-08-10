//
// RandomCoordinateController
//

using System;

using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;

using Utils = IDHIUtils.Utilities;

using static IDHIPlugins.RandomCoordinatePlugIn;
using System.Linq;


namespace IDHIPlugins
{
    public partial class RandomCoordinateController : CharaCustomFunctionController
    {
        public bool HasMoreOutfits => ChaControl.chaFile.coordinate.Length >= 4;
        public int TotalCoordinates => ChaControl.chaFile.coordinate.Length;
        public string GirlKey => PseudoKey();

        private string PseudoKey()
        {
            var name = ChaControl.chaFile.parameter.fullname.Trim();
            var personality = ChaControl.chaFile.parameter.personality.ToString();
            var height = (int)Math.Round(ChaControl.chaFile.custom.body
                .shapeValueBody[(int)ChaFileDefine.BodyShapeIdx.Height] * 100);

            var rc = $"{name}.{personality}.{height}";

            return rc;
        }

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
                var girlKey = GirlKey;

                // Sometimes the controller is not loaded maintain a cache lookup table
                if (!GirlsRandomData.ContainsKey(girlKey))
                {
#if DEBUG
                    Log.Warning($"[OnReload] 0000: Name={heroine.Name.Trim()}" +
                        $"({heroine.chaCtrl.name}) caching info.");
#endif
                    GirlsRandomData.Add(girlKey, new RandomData(heroine));
                }

                if (GirlsRandomData.TryGetValue(girlKey, out var girlInfo))
                {
                    // Initialize the coordinates information
                    var nowCoordinate = heroine.StatusCoordinate;
                    var categoryType = girlInfo.GetCategoryType(heroine.StatusCoordinate);
                    var randomCoordinateByType = girlInfo.CoordinateByType[categoryType];
                    var randomCoordinate = girlInfo.CoordinateNumber;
                    var randomCategory = girlInfo.CategoryType;

                    if (nowCoordinate != randomCoordinateByType)
                    {
                        // Set current random coordinate cache info to what the heroine is
                        // currently wearing. This inconsistency occurs when the game resets the
                        // character controller. Game seems to use the same coordinate for a
                        // save/load cycle.
                        girlInfo.SetRandomData(heroine);
#if DEBUG
                        Log.Warning($"[OnReload] 0001: Name={heroine.Name.Trim()} " +
                            $"({heroine.chaCtrl.name}) update cache categoryType={categoryType} " +
                            $"nowCoordinate={nowCoordinate} " +
                            $"coordinateBT[{categoryType}]={randomCoordinateByType} " +
                            $"randomCategory={randomCategory} " +
                            $"randomCoordinate={randomCoordinate}.");
#endif
                        categoryType = girlInfo.GetCategoryType(heroine.StatusCoordinate);
                        randomCoordinateByType = girlInfo.CoordinateByType[categoryType];
                        randomCoordinate = girlInfo.CoordinateNumber;
                        randomCategory = girlInfo.CategoryType;
                    }

                    if (heroine.fixCharaID == -13)
                    {
                        // This is the Guide character needs special treatment
                        SetupGuide(heroine, true);
                        categoryType = GetCategoryType(heroine.StatusCoordinate);
#if DEBUG
                        Log.Warning($"[OnReload] 0002: Name={heroine.Name.Trim()} setup Guide " +
                            $"({heroine.chaCtrl.name}) synchronize info rcByType[{categoryType}]" +
                            $"={heroine.StatusCoordinate} rc={heroine.StatusCoordinate}.");
#endif
                    }
#if DEBUG
                    Log.Debug($"[OnReload] 0003: " +
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
        /// Heroine name
        /// </summary>
        /// <returns></returns>
        public string GetName(bool withCtrlName = true)
        {
            var rc = Utils.TranslateName(Utilities.GirlName(ChaControl), true);

            if (withCtrlName)
            {
                rc += $" ({ChaControl.name})";
            }

            return rc; 
        }

        /// <summary>
        /// Heroine map location
        /// </summary>
        /// <returns></returns>
        public string GetMapInfo()
        {
            var mapNo = Utils.MapNumber(ChaControl);
            var mapName = Utils.MapName(ChaControl);

            var rc = $"map={mapNo} ({mapName})";

            return rc;
        }

        /// <summary>
        /// Get the RandomData set in cache for this ChaControl
        /// </summary>
        /// <returns></returns>
        public RandomData GirlInfo()
        {
            GirlsRandomData.TryGetValue(GirlKey, out var girlInfo);

            return girlInfo;
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
            GirlsRandomData.TryGetValue(GirlKey, out var girlInfo);

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

        /// <summary>
        /// Get current random coordinate in cache
        /// </summary>
        /// <returns></returns>
        public int GetRandomCoordinate()
        {
            var rc = -1;
            if (GirlsRandomData.TryGetValue(GirlKey, out var girlInfo))
            {
                rc = girlInfo.CoordinateNumber;
            }
            return rc;
        }

        /// <summary>
        /// Get previous random coordinate in chace
        /// </summary>
        /// <returns></returns>
        public int PreviousRandomCoordinate()
        {
            var rc = -1;
            if (GirlsRandomData.TryGetValue(GirlKey, out var girlInfo))
            {
                rc = girlInfo.PreviousCoordinateNumber;
            }
            return rc;
        }

        /// <summary>
        /// Get current coordinate according to a type
        /// </summary>
        /// <returns></returns>
        public ChaFileDefine.CoordinateType GetRandomCategoryType()
        {
            var rc = ChaFileDefine.CoordinateType.Plain;
            if (GirlsRandomData.TryGetValue(GirlKey, out var girlInfo))
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
        public int GetRandomCoordinateByType(ChaFileDefine.CoordinateType type)
        {
            if (GirlsRandomData.TryGetValue(GirlKey, out var girlInfo))
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
        public int GetRandomCoordinateByType(int type)
        {
            return GetRandomCoordinateByType((ChaFileDefine.CoordinateType)type);
        }

        /// <summary>
        /// Return string with the cache info for 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetCache(ChaFileDefine.CoordinateType type)
        {
            var rc = string.Empty;

            if (GirlsRandomData.TryGetValue(GirlKey, out var girlInfo))
            {
                girlInfo.ToString(type);
            }

            return rc;
        }

        /// <summary>
        /// GetCache overload for an int parameter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetCache(int type)
        {
            return GetCache((ChaFileDefine.CoordinateType)type);
        }

        /// <summary>
        /// Manually set current random cache information
        /// </summary>
        /// <param name="type"></param>
        public void SetRandomCoordinate(ChaFileDefine.CoordinateType type)
        {
            if (GirlsRandomData.TryGetValue(GirlKey, out var girlInfo))
            {
                var categoryType = girlInfo.GetCategoryType((int)type);

                girlInfo.SetRandomData(categoryType, (int)type);
            }
        }

        /// <summary>
        /// Look for first run info in cache.
        /// </summary>
        /// <returns></returns>
        public bool FirstRun()
        {
            var rc = false;

            if (GirlsRandomData.TryGetValue(GirlKey, out var girlInfo))
            {
                rc = girlInfo.FirstRun;
            }
            return rc;
        }

        /// <summary>
        /// Sets first run flag in cache.
        /// </summary>
        /// <param name="status"></param>
        public void FirstRun(bool status)
        {
            if (GirlsRandomData.TryGetValue(GirlKey, out var girlInfo))
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
            var name = Utilities.GirlName(ChaControl) + $" ({ChaControl.name})";

            try
            {
                if (GirlsRandomData.TryGetValue(GirlKey, out var girlInfo))
                {
                    var categoryType = girlInfo.GetCategoryType(type);
                    var currentType = ChaControl.fileStatus.coordinateType;
                    var coordinates = girlInfo.CoordinatesByType[categoryType];

                    if (coordinates.Count > 1)
                    {
                        try
                        {
                            var newCoordinates = coordinates.Where(x => x != currentType).ToList();

                            if (newCoordinates.Count > 1)
                            {
                                // New random coordinate
                                var coordinateIndex = RandCoordinate.Next(0, newCoordinates.Count);
                                newCoordinate = newCoordinates[coordinateIndex];
                            }
                            else
                            {
                                if (newCoordinates.Count == 0)
                                {
                                    // This should never be true
                                    newCoordinate = (int)type;
                                }
                                else
                                {
                                    // There were only 2 coordinates in the pool
                                    newCoordinate = coordinates[0];
                                }
                            }

                            // save coordinate
                            girlInfo.CategoryType = categoryType;
                            girlInfo.CoordinateNumber = newCoordinate;
                            girlInfo.CoordinateByType[categoryType] = newCoordinate;
#if DEBUG
                            Log.Warning($"[RandomCoordinate] 0000: Name={name} " +
                                $"({GetMapInfo()}) currentCoordinateType={currentType} " +
                                $"CoordinateByType[{categoryType}]=" +
                                $"{girlInfo.CoordinateByType[categoryType]} " +
                                $"CategoryType={girlInfo.CategoryType} " +
                                $"CoordinateNumber={girlInfo.CoordinateNumber}.");
#endif
                        }
                        catch (Exception e)
                        {
                            Log.Level(LogLevel.Error, $"[RandomCoordinate] " +
                                $"Name={name} Problem generating random " +
                                $"number categoryType={categoryType} " +
                                $"Error code={e.Message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Level(LogLevel.Error, $"[RandomCoordinate] " +
                    $"Name={name} Problem " +
                    $"in random coordinate search type={type} " +
                    $"code={e.Message}");
            }
            return newCoordinate;
        }
    }
}
