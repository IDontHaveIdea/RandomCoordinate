//
// RandomCoordinateController
//
using System;
using System.Collections.Generic;
using System.Diagnostics;

using ADV.Commands.Base;
using ActionGame.Chara;

using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;

using Utils = IDHIUtils.Utilities;

using static IDHIPlugins.RandomCoordinatePlugin;
using IDHIUtils;

namespace IDHIPlugins
{
    public partial class RandomCoordinateController : CharaCustomFunctionController
    {
        #region private fields
        private bool _firstRandomRequest = true;
        private List<int> _tmpCoordinates;
        private int _nowRandomCoordinate;
        private ChaFileDefine.CoordinateType _nowRandomType;

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

        //protected override void OnReload(GameMode currentGameMode)
        //{
        //    base.OnReload(currentGameMode);
        //}

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (maintainState)
            {
                return;
            }

            if (KoikatuAPI.GetCurrentGameMode() != GameMode.MainGame)
            {
                return;
            }

            var heroine = ChaControl.GetHeroine();

            if (heroine != null)
            {
                // Initialize the coordinates information
                InitCoordinates();

                var coordinateType = GetCoordinateType(heroine.StatusCoordinate);
                var nowRandomCoordinate = _nowRandomCoordinateByType[coordinateType];

                if (nowRandomCoordinate != heroine.StatusCoordinate)
                {
                    // Synchronize coordinate information
                    _nowRandomCoordinateByType[coordinateType] = heroine.StatusCoordinate;
                    _nowRandomType = coordinateType;
                }


                if (heroine.fixCharaID == -13)
                {
                    SetupGuide(heroine, true);
                }

                // Sometimes cannot get ChaControl.GetHeroine() to work save
                // to a lookup table
                GirlsNames[ChaControl.name] = Utilities.GirlName(heroine);
#if DEBUG
                _Log.Warning($"[OnReload] " +
                    $"Name={heroine.Name.Trim()} chaName={heroine.chaCtrl.name} " +
                    $"heroinie.NowCoordinate={heroine.StatusCoordinate} " +
                    $"nowRandomCoordinate={nowRandomCoordinate} - " +
                    $"{_nowRandomCoordinateByType[coordinateType]} " +
                    $"LookType={coordinateType} total " +
                    $"coordinates={ChaFileControl.coordinate.Length} " +
                    $"random possible={HasMoreOutfits}");
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
        /// * Swin - can work events depend on this
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

            var _coordinate = (-1);

            _coordinate = type switch {
                ChaFileDefine.CoordinateType.Swim => (int)ChaFileDefine.CoordinateType.Swim,
                ChaFileDefine.CoordinateType.Pajamas => (int)ChaFileDefine.CoordinateType.Pajamas,
                ChaFileDefine.CoordinateType.Bathing => (int)ChaFileDefine.CoordinateType.Bathing,
                _ => RandomCoordinate(type),
            };
#if DEBUG
            _Log.Warning("[GetRandomCoordinateType] " +
                $"Name={Utilities.GirlName(ChaControl)} type={type} " +
                $"coordinate={_coordinate}");
#endif
            return _coordinate;
        }

        /// <summary>
        /// When coordinate is grater than 3 (Bathing) try and get the corresponding
        /// type. The function is needed if the type selected by the game is
        /// greater then 3.
        /// </summary>
        /// <param name="type">coordinate in the request for random coordinate</param>
        /// <returns></returns>
        public ChaFileDefine.CoordinateType GetCoordinateType(int coordinate)
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
        /// Overload
        /// </summary>
        /// <param name="type">Type of coordinate</param>
        /// <returns></returns>
        public ChaFileDefine.CoordinateType GetCoordinateType(ChaFileDefine.CoordinateType type)
        {
            return GetCoordinateType((int)type);
        }

        /// <summary>
        /// Return the current coordinate for any given type
        /// </summary>
        /// <param name="type">Type of coordinate</param>
        /// <returns></returns>
        public int NowRandomCoordinateByType(ChaFileDefine.CoordinateType type)
        {
            var coordinateNumber = (int)type;
            var lookType = GetCoordinateType(type);
            _nowRandomCoordinateByType.TryGetValue(lookType, out coordinateNumber);
#if DEBUG
            // Get calling method name
            var calllingMethod = Utils.CallingMethod();

            _Log.Warning($"[NowRandomCoordinateByType] " +
                $"Calling Method=[{calllingMethod}] " +
                $"Name={Utilities.GirlName(ChaControl)} asked type={type} " +
                $"searchType={lookType} " +
                $"coordinateNumber={coordinateNumber}");
#endif
            return coordinateNumber;
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
            var coordinateIndex = -1;
            var name = Utilities.GirlName(ChaControl);

            try
            {
                var lookType = GetCoordinateType(type);
                _tmpCoordinates = _Coordinates[lookType];

                if (_tmpCoordinates.Count > 1)
                {
                    try
                    {
                        // New random coordinate
                        coordinateIndex = RandCoordinate
                            .Next(0, _tmpCoordinates.Count);
                        newCoordinate = _tmpCoordinates[coordinateIndex];

                        // save coordinate
                        _nowRandomCoordinateByType[lookType] = newCoordinate;
                        _nowRandomCoordinate = newCoordinate;
                        _nowRandomType = lookType;
#if DEBUG
                        _Log.Warning($"[RandomCoordinate] 01 " +
                            $"Name={name} " +
                            $"Type={type} " +
                            $"NowRandomType={_nowRandomType} " +
                            $"coordinateIndex={coordinateIndex} " +
                            $"NowRandomCoordinate={_nowRandomCoordinateByType[lookType]}");
#endif
                    }
                    catch (Exception e)
                    {
                        _Log.Level(LogLevel.Error, $"[RandomCoordinate] " +
                            $"Name={name} Problem generating random " +
                            $"number lookType={lookType} " +
                            $"Error code={e.Message}");
                    }
                }
#if DEBUG
                else
                {
                    _Log.Warning($"[RandomCoordinate] 02 " +
                        $"Name={name} " +
                        $"Type={type} " +
                        $"Total Coordinates By Type={_tmpCoordinates.Count} " +
                        $"NowRandomType={_nowRandomType} " +
                        $"NowRandomCoordinate={_nowRandomCoordinate}");
                }
#endif
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
