//
// Utilities
//
using System;
using System.Collections.Generic;

using BepInEx.Logging;


namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        public class RandomData
        {
            public ChaFileDefine.CoordinateType CategoryType { get; set; }
            public int CoordinateNumber { get; set; }
            public string CtrlName { get; private set; }
            public bool FirstRun { get; set; } = true;
            public string Name { get; private set; } = "";
            public bool HasMoreOutfits { get; set; }

            public Dictionary<ChaFileDefine.CoordinateType, List<int>>
                CoordinatesByType = new()
            {
                {ChaFileDefine.CoordinateType.Plain, new List<int> {}},
                {ChaFileDefine.CoordinateType.Swim, new List<int> {}},
                {ChaFileDefine.CoordinateType.Pajamas, new List<int> {}},
                {ChaFileDefine.CoordinateType.Bathing, new List<int> {}}
            };

            public Dictionary<ChaFileDefine.CoordinateType, int>
                RandomCoordinateByType = new()
            {
                {ChaFileDefine.CoordinateType.Plain, 0},
                {ChaFileDefine.CoordinateType.Swim, 1},
                {ChaFileDefine.CoordinateType.Pajamas, 2},
                {ChaFileDefine.CoordinateType.Bathing, 3}
            };

            private int _totalCoordinates = 0;

            public RandomData(
                ChaFileDefine.CoordinateType categoryType,
                int coordinateNumber,
                int statusCoordinate,
                ChaControl chaCtrl)
            {
                InitCoordinates(chaCtrl);
                _totalCoordinates = chaCtrl.chaFile.coordinate.Length;
                HasMoreOutfits = _totalCoordinates >= 4;
                CategoryType = categoryType;
                CoordinateNumber = coordinateNumber;
                CtrlName = chaCtrl.name;
                Name = chaCtrl.chaFile.parameter.fullname.Trim();
                RandomCoordinateByType[CategoryType] = statusCoordinate;
            }

            public RandomData(SaveData.Heroine heroine)
            {
                InitCoordinates(heroine);
                _totalCoordinates = heroine.charFile.coordinate.Length;
                HasMoreOutfits = _totalCoordinates >= 4;
                CategoryType = GetCategoryType(heroine.StatusCoordinate);
                CoordinateNumber = heroine.StatusCoordinate;
                CtrlName = heroine.chaCtrl.name;
                Name = heroine.Name.Trim();
                RandomCoordinateByType[CategoryType] = heroine.StatusCoordinate;
            }

            public bool SetRandomData(
                ChaFileDefine.CoordinateType categoryType,
                int coordinateNumber,
                int statusCoordinate)
            {
                CategoryType = categoryType;
                CoordinateNumber = coordinateNumber;
                RandomCoordinateByType[CategoryType] = statusCoordinate;
                return true;
            }

            public bool SetRandomData(SaveData.Heroine heroine)
            {
                var rc = false;
                if (heroine != null)
                {
                    CategoryType = GetCategoryType(heroine.StatusCoordinate);
                    RandomCoordinateByType[CategoryType] = heroine.StatusCoordinate;
                    CoordinateNumber = heroine.StatusCoordinate;
                    rc = true;
                }
                return rc;
            }

            /// <summary>
            /// Clears and fill the _Coordinates dictionary
            /// </summary>
            private void InitCoordinates(SaveData.Heroine heroine)
            {
                var totalCoordinates = heroine.chaCtrl.chaFile.coordinate.Length;

                for (var i = 0; i < 4; i++)
                {
                    // Original 4 coordinates
                    CoordinatesByType[(ChaFileDefine.CoordinateType)i].Clear();
                    CoordinatesByType[(ChaFileDefine.CoordinateType)i].Add(i);
                }

                // Add additional outfits to Plain type for random selection
                for (var i = 4; i < totalCoordinates; i++)
                {
                    // TODO: Interface to classify coordinates grater than 3
                    CoordinatesByType[ChaFileDefine.CoordinateType.Plain].Add(i);
                }

                var tmpCoordinates = CoordinatesByType[ChaFileDefine.CoordinateType.Plain];
                _Log.Warning($"[InitCoordinates.heroine] Name={heroine.Name.Trim()} " +
                        $"tmpCoordinates[{ChaFileDefine.CoordinateType.Plain}].Count={tmpCoordinates.Count}.");
            }

            /// <summary>
            /// Clears and fill the _Coordinates dictionary
            /// </summary>
            private void InitCoordinates(ChaControl heroine)
            {
                var totalCoordinates = heroine.chaFile.coordinate.Length;

                for (var i = 0; i < 4; i++)
                {
                    // Original 4 coordinates
                    CoordinatesByType[(ChaFileDefine.CoordinateType)i].Clear();
                    CoordinatesByType[(ChaFileDefine.CoordinateType)i].Add(i);
                }

                // Add additional outfits to Plain type for random selection
                for (var i = 4; i < totalCoordinates; i++)
                {
                    // TODO: Interface to classify coordinates grater than 3
                    CoordinatesByType[ChaFileDefine.CoordinateType.Plain].Add(i);
                }
                var tmpCoordinates = CoordinatesByType[ChaFileDefine.CoordinateType.Plain];
                _Log.Warning($"[InitCoordinates.ChaControl] Name={heroine.chaFile.parameter.fullname.Trim()} " +
                        $"tmpCoordinates[{ChaFileDefine.CoordinateType.Plain}].Count={tmpCoordinates.Count}.");
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
                    if (CoordinatesByType[ChaFileDefine.CoordinateType.Plain].Contains(coordinate))
                    {
                        return ChaFileDefine.CoordinateType.Plain;
                    }
                    if (CoordinatesByType[ChaFileDefine.CoordinateType.Swim].Contains(coordinate))
                    {
                        return ChaFileDefine.CoordinateType.Swim;
                    }
                    if (CoordinatesByType[ChaFileDefine.CoordinateType.Pajamas].Contains(coordinate))
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
            public ChaFileDefine.CoordinateType GetCategoryType(ChaFileDefine.CoordinateType type)
            {
                return GetCategoryType((int)type);
            }

            public int NewRandomCoordinateByType(ChaFileDefine.CoordinateType type)
            {
                if (FirstRun)
                {
                    FirstRun = false;
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
            private int RandomCoordinate(ChaFileDefine.CoordinateType type, bool test = false)
            {
                var newCoordinate = (int)type;

                try
                {
                    var categoryType = GetCategoryType(type);
                    var tmpCoordinates = CoordinatesByType[categoryType];

                    if (tmpCoordinates.Count > 1)
                    {
                        try
                        {
                            // New random coordinate
                            var coordinateIndex = RandCoordinate.Next(0, tmpCoordinates.Count);
                            newCoordinate = tmpCoordinates[coordinateIndex];

                            // save coordinate
                            if (!test)
                            {
                                CategoryType = categoryType;
                                CoordinateNumber = newCoordinate;
                                RandomCoordinateByType[categoryType] = newCoordinate;
#if DEBUG
                                _Log.Warning($"[RandomCoordinate] Name={Name} " +
                                    $"nowRCByType[{categoryType}]=" +
                                    $"{RandomCoordinateByType[categoryType]} " +
                                    $"nowRT={CategoryType} " +
                                    $"nowRC={CoordinateNumber}.");
#endif
                            }

                        }
                        catch (Exception e)
                        {
                            _Log.Level(LogLevel.Error, $"[RandomCoordinate] " +
                                $"Name={Name} Problem generating random " +
                                $"number categoryType={categoryType} " +
                                $"Error code={e.Message}");
                        }
                    }
                }
                catch (Exception e)
                {
                    _Log.Level(LogLevel.Error, $"[RandomCoordinate] " +
                        $"Name={Name} Problem " +
                        $"in random coordinate search type={type} " +
                        $"code={e.Message}");
                }
                return newCoordinate;
            }


            public string ToString(ChaFileDefine.CoordinateType type)
            {
                var categoryType = GetCategoryType((int)type);
                var coordinateByType = RandomCoordinateByType[categoryType];

                var cache = $"CoordinateByType[{categoryType}]={coordinateByType} " +
                    $"Category={CategoryType} Coordinate{CoordinateNumber}";

                return cache;
            }
        }
    }
}
