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
        #region private fields
        //private List<int> _tmpCoordinates;
        //private int _nowRandomCoordinate = (-1);
        //private ChaFileDefine.CoordinateType _nowRandomCategoryType =
        //    ChaFileDefine.CoordinateType.Plain;

        //private readonly Dictionary<ChaFileDefine.CoordinateType, List<int>>
        //    _Coordinates = new()
        //{
        //    {ChaFileDefine.CoordinateType.Plain, new List<int> {}},
        //    {ChaFileDefine.CoordinateType.Swim, new List<int> {}},
        //    {ChaFileDefine.CoordinateType.Pajamas, new List<int> {}},
        //    {ChaFileDefine.CoordinateType.Bathing, new List<int> {}}
        //};

        //private readonly Dictionary<ChaFileDefine.CoordinateType, int>
        //    _nowRandomCoordinateByType = new()
        //{
        //    {ChaFileDefine.CoordinateType.Plain, 0},
        //    {ChaFileDefine.CoordinateType.Swim, 1},
        //    {ChaFileDefine.CoordinateType.Pajamas, 2},
        //    {ChaFileDefine.CoordinateType.Bathing, 3}
        //};
        #endregion

        #region properties
        public bool HasMoreOutfits => (ChaFileControl.coordinate.Length > 4);
        //public bool FirstRun => _firstRandomRequest;
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
                // Sometimes the controller is not loaded maintain a cache lookup table
                if (!GirlsRandomData.ContainsKey(heroine.chaCtrl.name))
                {
                    _Log.Warning($"[OnReload] 00 Name={heroine.Name.Trim()}({heroine.chaCtrl.name}) caching info.");
                    GirlsRandomData[heroine.chaCtrl.name] =
                        new RandomData(heroine);
                }

                if (GirlsRandomData.TryGetValue(heroine.chaCtrl.name, out var girlInfo))
                {
                    // Initialize the coordinates information
                    var nowRandomCoordinate = heroine.StatusCoordinate;
                    var categoryType = girlInfo.GetCategoryType(heroine.StatusCoordinate);
                    var currentRandomCoordinateByType = girlInfo.NowRandomCoordinateByType[categoryType];
                    var currentRandomCoordinate = girlInfo.CoordinateNumber;

                    if (nowRandomCoordinate != currentRandomCoordinateByType)
                    {
                        _Log.Warning($"[OnReload] 01 Name={heroine.Name.Trim()}({heroine.chaCtrl.name}) CT={categoryType} SC={nowRandomCoordinate} RCBT={currentRandomCoordinateByType} CRC={currentRandomCoordinate}.");
                        ChangeCoordinate(heroine, nowRandomCoordinate);
                        girlInfo.SetRandomData(heroine);

                        nowRandomCoordinate = heroine.StatusCoordinate;
                        categoryType = girlInfo.GetCategoryType(heroine.StatusCoordinate);
                        currentRandomCoordinateByType = girlInfo.NowRandomCoordinateByType[categoryType];
                        nowRandomCoordinate = girlInfo.CoordinateNumber;
                    }

                    if (heroine.fixCharaID == -13)
                    {
                        // This is the Guide character needs special treatment
                        SetupGuide(heroine, true);
                        categoryType = GetCategoryType(heroine.StatusCoordinate);
                        _Log.Warning($"[OnReload] 02 Setup Guide Name={heroine.Name.Trim()}({heroine.chaCtrl.name}) synchronize info rcByType[{categoryType}]={heroine.StatusCoordinate} rc={heroine.StatusCoordinate}.");
                    }
#if DEBUG
                    _Log.Debug($"[OnReload] 03 " +
                        $"Name={heroine.Name.Trim()}({heroine.chaCtrl.name}) " +
                        $"heroine.StatusCoordinate={heroine.StatusCoordinate} " +
                        $"nowRCByType[{categoryType}]={currentRandomCoordinateByType} " +
                        $"currentRC={currentRandomCoordinate} " +
                        $"total coordinates={ChaFileControl.coordinate.Length} " +
                        $"random possible={HasMoreOutfits}.");
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

            if (GirlsRandomData.TryGetValue(ChaControl.name, out var rcInfo))
            {
                rc = rcInfo.FirstRun;
            }
            return rc;
        }

        /// <summary>
        /// Sets first run flag in chache.
        /// </summary>
        /// <param name="status"></param>
        public void FirstRun(bool status)
        {
            if (GirlsRandomData.TryGetValue(ChaControl.name, out var rcInfo))
            {
                rcInfo.FirstRun = status;
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
            //if (_nowRandomCoordinate < 0)
            //{
            //    _nowRandomCoordinate = _coordinate;
            //}
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
            GirlsRandomData.TryGetValue(ChaControl.name, out var girlInfo);

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
            if (GirlsRandomData.TryGetValue(ChaControl.name, out var girlInfo))
            {
                rc = girlInfo.CoordinateNumber;
            }
            return rc;
        }

        public ChaFileDefine.CoordinateType NowRandomCategoryType()
        {
            var rc = ChaFileDefine.CoordinateType.Plain;
            if (GirlsRandomData.TryGetValue(ChaControl.name, out var girlInfo))
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
            if (GirlsRandomData.TryGetValue(ChaControl.name, out var girlInfo))
            { 
                var categoryType = girlInfo.GetCategoryType((int)type);

                if (girlInfo.NowRandomCoordinateByType
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
            if (GirlsRandomData.TryGetValue(ChaControl.name, out var girlInfo))
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
            try
            {
                if (GirlsRandomData.TryGetValue(ChaControl.name, out var girlInfo))
                {
                    var name = Utilities.GirlName(ChaControl);
                    var categoryType = girlInfo.GetCategoryType(type);

                    var tmpCoordinates = girlInfo.CoordinatesByType[categoryType];

                    if (tmpCoordinates.Count > 1)
                    {
                        try
                        {
                            // New random coordinate
                            var coordinateIndex = RandCoordinate.Next(0, tmpCoordinates.Count);
                            newCoordinate = tmpCoordinates[coordinateIndex];

                            // save coordinate
                            girlInfo.CategoryType = categoryType;
                            girlInfo.CoordinateNumber = newCoordinate;
                            girlInfo.NowRandomCoordinateByType[categoryType] = newCoordinate;
#if DEBUG
                            _Log.Warning($"[RandomCoordinate] name={name} " +
                                $"_nowRCByType[{categoryType}]=" +
                                $"{girlInfo.NowRandomCoordinateByType[categoryType]} " +
                                $"_nowRT={girlInfo.CategoryType} " +
                                $"_nowRC={girlInfo.CoordinateNumber}.");
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

        /// <summary>
        /// Clears and fill the _Coordinates dictionary
        /// </summary>
        /*private void InitCoordinates()
        {
            var totalCoordinates = ChaControl.chaFile.coordinate.Length;

            for (var i = 0; i < 4; i++)
            {
                // Original 4 coordinates
                _Coordinates[(ChaFileDefine.CoordinateType)i].Clear();
                _Coordinates[(ChaFileDefine.CoordinateType)i].Add(i);
            }

            // Add additional outfits to Plain type for random selection
            for (var i = 4; i < totalCoordinates; i++)
            {
                // TODO: Interface to classify coordinates grater than 3
                _Coordinates[ChaFileDefine.CoordinateType.Plain].Add(i);
            }
        }*/
    }
}
