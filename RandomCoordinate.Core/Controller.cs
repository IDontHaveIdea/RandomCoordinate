//
// RandomCoordinateController
//
using System.Collections.Generic;
using System;

using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;

namespace IDHIPlugins
{
    public partial class RandomCoordinateController : CharaCustomFunctionController
    {
        #region private fields
        private bool _firstRun = true;
        private int _coordinateType;
        private ChaFileDefine.CoordinateType _nowType;
        private List<int> _tmpCoordinates;
        private ChaFileDefine.CoordinateType _nowRandomCoordinateType;

        private Dictionary<ChaFileDefine.CoordinateType, List<int>>
            _Coordinates = new()
        {
            {ChaFileDefine.CoordinateType.Plain, new List<int> {}},
            {ChaFileDefine.CoordinateType.Swim, new List<int> {}},
            {ChaFileDefine.CoordinateType.Pajamas, new List<int> {}},
            {ChaFileDefine.CoordinateType.Bathing, new List<int> {}}
        };

        private Dictionary<ChaFileDefine.CoordinateType, int>
            _nowRandomCoordinateByType = new()
        {
            {ChaFileDefine.CoordinateType.Plain, 0},
            {ChaFileDefine.CoordinateType.Swim, 1},
            {ChaFileDefine.CoordinateType.Pajamas, 2},
            {ChaFileDefine.CoordinateType.Bathing, 3}
        };
        #endregion

        #region properties
        public ChaFileDefine.CoordinateType NowType => _nowType;
        public ChaFileDefine.CoordinateType NowRandomCoordinateType
            => _nowRandomCoordinateType;
        public bool FirstRun => _firstRun;
        #endregion

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
        }

        protected override void OnReload(GameMode currentGameMode)
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.MainGame)
            {
                return;
            }

            var heroine = ChaControl.GetHeroine();

            if (heroine != null) {
                var totalCoordinates = ChaControl.chaFile.coordinate.Length;

                // For now just work with the Plain type
                if ((totalCoordinates > 4)
                    && (_Coordinates[ChaFileDefine.CoordinateType.Plain].Count == 0))
                {
                    // Add standard coordinates to respective types
                    _Coordinates[ChaFileDefine.CoordinateType.Plain].Add(0);
                    _Coordinates[ChaFileDefine.CoordinateType.Swim].Add(1);
                    _Coordinates[ChaFileDefine.CoordinateType.Pajamas].Add(2);
                    _Coordinates[ChaFileDefine.CoordinateType.Bathing].Add(3);

                    // Add additional outfits to Plain type for random selection
                    for (var i = 4; i < totalCoordinates; i++)
                    {
                        // TODO: Interface to classify coordinates grater than 3
                        _Coordinates[ChaFileDefine.CoordinateType.Plain].Add(i);
                    }
                }

                var lookType = LookType(heroine.NowCoordinate);
                if (lookType == ChaFileDefine.CoordinateType.Plain)
                {
                    _nowRandomCoordinateByType[lookType] = heroine.NowCoordinate;
                    _nowRandomCoordinateType = lookType;
                }
#if DEBUG
                RandomCoordinatePlugin._Log.Warning($"[OnReload] " +
                    $"Name={heroine.Name.Trim()} " +
                    $"girl={ChaControl.name} " +
                    $"Loading nowCoordinate={heroine.NowCoordinate} " +
                    $"LookType={lookType} total " +
                    $"coordinates={ChaControl.chaFile.coordinate.Length}");
#endif
            }
        }

        public int GetRandomCoordinateType(ChaFileDefine.CoordinateType type)
        {
            _nowType = type;
            if (_firstRun)
            {
                _firstRun = false;
            }
#if DEBUG
            RandomCoordinatePlugin._Log.Warning("[GetRandomCoordinateType] " +
                $"Name={ChaControl.GetHeroine().Name.Trim()}");
#endif
            if (ChaControl.chaFile.coordinate.Length <= 4)
            {
                return (int)type;
            }

            _coordinateType = (-1);

            // For now only for Plain coordinates
            _coordinateType = type switch {
                ChaFileDefine.CoordinateType.Swim => (int)ChaFileDefine.CoordinateType.Swim,
                ChaFileDefine.CoordinateType.Pajamas => (int)ChaFileDefine.CoordinateType.Pajamas,
                ChaFileDefine.CoordinateType.Bathing => (int)ChaFileDefine.CoordinateType.Bathing,
                _ => RadomCoordinate(type),
            };
            return _coordinateType;
        }

        public int NowRandomCoordinateMethod(ChaFileDefine.CoordinateType type)
        {
            var coord = (int)type;

            if (_nowRandomCoordinateByType.ContainsKey(type))
            {
                coord = _nowRandomCoordinateByType[type];
            }
            return coord;
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
        private int RadomCoordinate(ChaFileDefine.CoordinateType type)
        {
            var newCoordinate = (int)type;
            var coordinateIndex = -1;
            var name = ChaControl.GetHeroine().Name.Trim();

            try
            {
                var lookType = LookType(type);
                _nowType = lookType;
                _tmpCoordinates = _Coordinates[lookType];
                _nowRandomCoordinateType = lookType;

                if (_tmpCoordinates.Count > 1)
                {
                    try
                    {
                        coordinateIndex = RandomCoordinatePlugin
                            .RandCoordinate.Next(0, _tmpCoordinates.Count);
                        newCoordinate = _tmpCoordinates[coordinateIndex];
                        _nowRandomCoordinateByType[type] = newCoordinate;
#if DEBUG
                        RandomCoordinatePlugin._Log.Warning($"[RandomCoordinate] " +
                            $"Name={name} " +
                            $"Type={type} " +
                            $"NowRandomCoordinateType={_nowRandomCoordinateType} " +
                            $"coordinateIndex={coordinateIndex} " +
                            $"NowRandomCoordinate={_nowRandomCoordinateByType[type]}");
#endif
                    }
                    catch (Exception e)
                    {
                        RandomCoordinatePlugin._Log.Error($"[RandomCoordinate] " +
                            $"Name={name} Problem generating random " +
                            $"number lookType={lookType} " +
                            $"Error code={e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                RandomCoordinatePlugin._Log.Error($"[RandomCoordinate] " +
                    $"Name={name} Problem " +
                    $"in random coordinate search type={type} code={e.Message}");
            }
            return newCoordinate;
        }

        /// <summary>
        /// When coordinate is grater than 3 (Bathing) try and get the corresponding
        /// type. The function is needed if the type selected by the game is
        /// greater then 3.
        /// </summary>
        /// <param name="type">coordinate type in the request for random coordinate</param>
        /// <returns></returns>
        private ChaFileDefine.CoordinateType LookType(ChaFileDefine.CoordinateType type)
        {
#if DEBUG
            RandomCoordinatePlugin._Log.Warning("[LookType] 01 " +
                $"Name={ChaControl.GetHeroine().Name.Trim()} " +
                $"type={type}");
#endif
            var lookType = (int)type;
            var rc = ChaFileDefine.CoordinateType.Plain;

            if (MathfEx.RangeEqualOn(0, lookType, 3))
            {
                rc = (ChaFileDefine.CoordinateType)lookType;
            }
            else
            {
                if (_Coordinates[ChaFileDefine.CoordinateType.Plain].Contains(lookType))
                {
                    //return ChaFileDefine.CoordinateType.Plain;
                    rc = ChaFileDefine.CoordinateType.Plain;
                }
                if (_Coordinates[ChaFileDefine.CoordinateType.Swim].Contains(lookType))
                {
                    //return ChaFileDefine.CoordinateType.Swim;
                    rc = ChaFileDefine.CoordinateType.Swim;
                }
                if (_Coordinates[ChaFileDefine.CoordinateType.Pajamas].Contains(lookType))
                {
                    //return ChaFileDefine.CoordinateType.Pajamas;
                    rc = ChaFileDefine.CoordinateType.Pajamas;
                }
            }
#if DEBUG
            RandomCoordinatePlugin._Log.Warning("[LookType] " +
                $"Name={ChaControl.GetHeroine().Name.Trim()} rc={rc}");
#endif
            return rc;
        }

        /// <summary>
        /// Overload for LookType with integer parameter
        /// </summary>
        /// <param name="type">type parameter as integer</param>
        /// <returns></returns>
        private ChaFileDefine.CoordinateType LookType(int type)
        {
            return LookType((ChaFileDefine.CoordinateType)type);
        }

    }
}
