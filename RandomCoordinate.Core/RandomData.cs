//
// Utilities
//
using System;
using System.Collections.Generic;
using System.Linq;

using BepInEx.Logging;


namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        public class CoordinateData
        {
            public ChaFileDefine.CoordinateType CategoryType { get; set; }
            public int CoordinateNumber { get; set; } = -1;

            public Dictionary<ChaFileDefine.CoordinateType, List<int>>
                CoordinatesByType = new()
                {
                    {ChaFileDefine.CoordinateType.Plain, new List<int> {}},
                    {ChaFileDefine.CoordinateType.Swim, new List<int> {}},
                    {ChaFileDefine.CoordinateType.Pajamas, new List<int> {}},
                    {ChaFileDefine.CoordinateType.Bathing, new List<int> {}}
                };

            public Dictionary<ChaFileDefine.CoordinateType, int>
                CoordinateByType = new()
                {
                    {ChaFileDefine.CoordinateType.Plain, 0},
                    {ChaFileDefine.CoordinateType.Swim, 1},
                    {ChaFileDefine.CoordinateType.Pajamas, 2},
                    {ChaFileDefine.CoordinateType.Bathing, 3}
                };
            public int TotalCoordinates { get; set; } = -1;

            public CoordinateData()
            {
            }

            public CoordinateData(ChaControl chaControl)
            {
                InitCoordinates(chaControl);
            }

            public void SetData(
                ChaFileDefine.CoordinateType categoryType,
                int coordinateNumber)
            {
                CategoryType = categoryType;
                CoordinateNumber = coordinateNumber;
                CoordinateByType[CategoryType] = coordinateNumber;
            }

            public bool SetData(SaveData.Heroine heroine)
            {
                var rc = false;
                if (heroine != null)
                {
                    CategoryType = GetCategoryType(heroine.StatusCoordinate);
                    CoordinateNumber = heroine.StatusCoordinate;
                    CoordinateByType[CategoryType] = heroine.StatusCoordinate;

                    rc = true;
                }
                return rc;
            }

            public void InitCoordinates(ChaControl heroine)
            {
                TotalCoordinates = heroine.chaFile.coordinate.Length;

                for (var i = 0; i < 4; i++)
                {
                    // Original 4 coordinates
                    CoordinatesByType[(ChaFileDefine.CoordinateType)i].Clear();
                    CoordinatesByType[(ChaFileDefine.CoordinateType)i].Add(i);
                }

                // Add additional outfits to Plain type for random selection
                for (var i = 4; i < TotalCoordinates; i++)
                {
                    CoordinatesByType[ChaFileDefine.CoordinateType.Plain].Add(i);
                }
            }

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
        }

        public class RandomData
        {
            public CoordinateData Current;
            public CoordinateData Previous;

            #region Properties
            public ChaFileDefine.CoordinateType CategoryType
            {
                get
                {
                    return Current.CategoryType;
                }

                set
                {
                    Current.CategoryType = value;
                }
            }
            public int CoordinateNumber
            {
                get
                {
                    return Current.CoordinateNumber;
                }
                set
                {
                    Current.CoordinateNumber = value;
                }
            }
            public Dictionary<ChaFileDefine.CoordinateType, List<int>> CoordinatesByType
            {
                get
                {
                    return Current.CoordinatesByType;
                }
                set
                {
                    Current.CoordinatesByType = value;
                }
            }
            public Dictionary<ChaFileDefine.CoordinateType, int> CoordinateByType
            {
                get
                {
                    return Current.CoordinateByType;
                }
                set
                {
                    Current.CoordinateByType = value;
                }
            }
            public string CtrlName { get; private set; }
            public bool FirstRun { get; set; } = true;
            public string Name { get; private set; } = "";
            public bool HasMoreOutfits { get; set; }
            public int TotalCoordinates
            {
                get
                {
                    return Current.TotalCoordinates;
                }
                set
                {
                    Current.TotalCoordinates = value;
                }
            }
            #endregion Properties

            public RandomData(
                ChaFileDefine.CoordinateType categoryType,
                int coordinateNumber,
                ChaControl chaCtrl)
            {
                Current = new(chaCtrl);
                Previous = new(chaCtrl);

                InitCoordinates(chaCtrl);
                HasMoreOutfits = chaCtrl.chaFile.coordinate.Length >= 4;

                Current.SetData(categoryType, coordinateNumber);
                Previous.SetData(categoryType, coordinateNumber);

                //CategoryType = categoryType;
                //CoordinateNumber = coordinateNumber;
                //CoordinateByType[CategoryType] = coordinateNumber;

                CtrlName = chaCtrl.name;
                Name = chaCtrl.chaFile.parameter.fullname.Trim();
            }

            public RandomData(SaveData.Heroine heroine)
            {
                Current = new(heroine.chaCtrl);
                Previous = new(heroine.chaCtrl);

                InitCoordinates(heroine);
                HasMoreOutfits = heroine.charFile.coordinate.Length >= 4;

                Current.SetData(heroine);
                Previous.SetData(heroine);

                //CategoryType = GetCategoryType(heroine.StatusCoordinate);
                //CoordinateNumber = heroine.StatusCoordinate;
                //CoordinateByType[CategoryType] = heroine.StatusCoordinate;

                CtrlName = heroine.chaCtrl.name;
                Name = heroine.Name.Trim();
                
            }

            public bool SetRandomData(
                ChaFileDefine.CoordinateType categoryType,
                int coordinateNumber)
            {
                SaveToPrevious();

                Current.SetData(categoryType, coordinateNumber);

                //CategoryType = categoryType;
                //CoordinateNumber = coordinateNumber;
                //CoordinateByType[CategoryType] = statusCoordinate;

                return true;
            }

            public bool SetRandomData(SaveData.Heroine heroine)
            {
                var rc = false;
                if (heroine != null)
                {
                    SaveToPrevious();

                    rc = Current.SetData(heroine);

                    //CategoryType = GetCategoryType(heroine.StatusCoordinate);
                    //CoordinateNumber = heroine.StatusCoordinate;
                    //CoordinateByType[CategoryType] = heroine.StatusCoordinate;

                    //rc = true;
                }
                return rc;
            }

            private void SaveToPrevious()
            {
                // Update Previous

                Previous.SetData(CategoryType, CoordinateNumber);

                //Previous.CategoryType = CategoryType;
                //Previous.CoordinateNumber = CoordinateNumber;
                //Previous.CoordinateByType[CategoryType] = CoordinateNumber;
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
                var rc = Current.GetCategoryType(coordinate);

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
                                CoordinateByType[categoryType] = newCoordinate;
#if DEBUG
                                _Log.Warning($"[RandomCoordinate] 0000: Name={Name} " +
                                    $"nowRCByType[{categoryType}]=" +
                                    $"{CoordinateByType[categoryType]} " +
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
                var coordinateByType = CoordinateByType[categoryType];

                var cache = $"Category={CategoryType} Coordinate={CoordinateNumber} " +
                    $"Current CoordinateByType[{CategoryType}]={CoordinateByType[CategoryType]} " +
                    $"CoordinateByType[{categoryType}]={coordinateByType}";

                return cache;
            }
        }
    }
}
