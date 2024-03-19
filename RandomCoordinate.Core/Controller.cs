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
        private bool _firstRandomRequest = true;
        private List<int> _tmpCoordinates;
        private int _nowRandomCoordinate = (-1);
        private ChaFileDefine.CoordinateType _nowRandomType = ChaFileDefine.CoordinateType.Plain;

        private readonly Dictionary<ChaFileDefine.CoordinateType, List<int>>
            _Coordinates = new()
        {
            {ChaFileDefine.CoordinateType.Plain, new List<int> {}},
            {ChaFileDefine.CoordinateType.Swim, new List<int> {}},
            {ChaFileDefine.CoordinateType.Pajamas, new List<int> {}},
            {ChaFileDefine.CoordinateType.Bathing, new List<int> {}}
        };

        private readonly Dictionary<ChaFileDefine.CoordinateType, int>
            _nowRandomCoordinateByType = new()
        {
            {ChaFileDefine.CoordinateType.Plain, 0},
            {ChaFileDefine.CoordinateType.Swim, 1},
            {ChaFileDefine.CoordinateType.Pajamas, 2},
            {ChaFileDefine.CoordinateType.Bathing, 3}
        };
        #endregion

        #region properties
        public bool HasMoreOutfits => (ChaFileControl.coordinate.Length > 4);
        public bool FirstRun => _firstRandomRequest;
        public int NowRandomCoordinate => _nowRandomCoordinate;
        public ChaFileDefine.CoordinateType NowRandomType => _nowRandomType;
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
                // Initialize the coordinates information
                InitCoordinates();

                var categoryType = GetCategoryType(heroine.StatusCoordinate);
                var currentRandomCoordinate = _nowRandomCoordinateByType[categoryType];
                _nowRandomCoordinate = heroine.StatusCoordinate;
                if (_nowRandomCoordinateByType[categoryType]
                    != heroine.StatusCoordinate)
                {
                    // Synchronize coordinate information
                    _nowRandomCoordinateByType[categoryType] = heroine.StatusCoordinate;
                    _nowRandomType = categoryType;
                }

                if (heroine.fixCharaID == -13)
                {
                    // This is the Guide character needs special treatment
                    SetupGuide(heroine, true);
                    categoryType = GetCategoryType(heroine.StatusCoordinate);
                }

                // Sometimes the controller is not loaded maintain a cache lookup table
                if (!GirlsRandomCoordinates.ContainsKey(heroine.chaCtrl.name))
                {
                    _Log.Error($"[OnReload] name={heroine.Name.Trim()}({heroine.chaCtrl.name}) caching info.");
                    GirlsRandomCoordinates[heroine.chaCtrl.name] =
                        new RandomInfo(categoryType, currentRandomCoordinate, heroine.StatusCoordinate);
                }
#if DEBUG
                _Log.Debug($"[OnReload] " +
                    $"Name={heroine.Name.Trim()}({heroine.chaCtrl.name}) " +
                    $"heroine.StatusCoordinate={heroine.StatusCoordinate} " +
                    $"nowRCByType={_nowRandomCoordinateByType[categoryType]} " +
                    $"currentRC={currentRandomCoordinate} " +
                    $"total coordinates={ChaFileControl.coordinate.Length} " +
                    $"random possible={HasMoreOutfits}.");
#endif
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
            if (_firstRandomRequest)
            {
                _firstRandomRequest = false;
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
            if (_nowRandomCoordinate < 0)
            {
                _nowRandomCoordinate = _coordinate;
            }
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
            var rc = ChaFileDefine.CoordinateType.Plain;

            if (MathfEx.RangeEqualOn(0, coordinate, 3))
            {
                rc = (ChaFileDefine.CoordinateType)coordinate;
            }
            else
            {
                if (_Coordinates[ChaFileDefine.CoordinateType.Plain].Contains(coordinate))
                {
                    return ChaFileDefine.CoordinateType.Plain;
                }
                if (_Coordinates[ChaFileDefine.CoordinateType.Swim].Contains(coordinate))
                {
                    return ChaFileDefine.CoordinateType.Swim;
                }
                if (_Coordinates[ChaFileDefine.CoordinateType.Pajamas].Contains(coordinate))
                {
                    return ChaFileDefine.CoordinateType.Pajamas;
                }
            }
            return rc;
        }

        /// <summary>
        /// This overload is needed because the game for coordinates > 3 uses the
        /// coordinate number as it type (ex, coordinate 5 has )
        /// </summary>
        /// <param name="type">Type of coordinate</param>
        /// <returns></returns>
        public ChaFileDefine.CoordinateType GetCategoryType(
            ChaFileDefine.CoordinateType type)
        {
            return GetCategoryType((int)type);
        }

        /// <summary>
        /// Return the current coordinate for any given type
        /// </summary>
        /// <param name="type">Type of coordinate</param>
        /// <returns></returns>
        public int NowRandomCoordinateByType(ChaFileDefine.CoordinateType type)
        {
            var categoryType = GetCategoryType(type);
            if (_nowRandomCoordinateByType
                .TryGetValue(categoryType, out var coordinateNumber))
            {
                return coordinateNumber;
            }
            return (int)type;
        }

        /// <summary>
        /// Overload NowRandomCoordinate with integer type
        /// </summary>
        /// <param name="integerType">coordinate type as integer</param>
        /// <returns></returns>
        public int NowRandomCoordinateByType(int integerType)
        {
            return NowRandomCoordinateByType((ChaFileDefine.CoordinateType)integerType);
        }

        public void SetRandomCoordinate(ChaFileDefine.CoordinateType type)
        {
            var categoryType = GetCategoryType(type);
            _nowRandomCoordinate = (int)type;
            _nowRandomType = categoryType;

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
            var name = Utilities.GirlName(ChaControl);

            try
            {
                var categoryType = GetCategoryType(type);
                _tmpCoordinates = _Coordinates[categoryType];

                if (_tmpCoordinates.Count > 1)
                {
                    try
                    {
                        // New random coordinate
                        var coordinateIndex = RandCoordinate
                            .Next(0, _tmpCoordinates.Count);
                        newCoordinate = _tmpCoordinates[coordinateIndex];

                        // save coordinate
                        _nowRandomCoordinateByType[categoryType] = newCoordinate;
                        _nowRandomCoordinate = newCoordinate;
                        _nowRandomType = categoryType;
                        if (GirlsRandomCoordinates.TryGetValue(
                            ChaControl.name, out var girlInfo))
                        {
                            girlInfo.CoordinateType = _nowRandomType;
                            girlInfo.CoordinateNumber = _nowRandomCoordinate;
                            girlInfo.NowRandomCoordinateByType[_nowRandomType] =
                                newCoordinate;
                        }
                        else {
                            GirlsRandomCoordinates[ChaControl.name] =
                                new RandomInfo(
                                    _nowRandomType,
                                    _nowRandomCoordinate,
                                    newCoordinate);
                        }
#if DEBUG
                        _Log.Warning($"[RandomCoordinate] name={name} " +
                            $"_nowRCByType[{categoryType}]=" +
                            $"{_nowRandomCoordinateByType[categoryType]} " +
                            $"_nowRT={_nowRandomType} " +
                            $"_nowRC={_nowRandomCoordinate}.");
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
        private void InitCoordinates()
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
        }
    }
}
