//
// Utilities
//
using System.Collections.Generic;

using ActionGame.Chara;

using KKAPI.MainGame;

using static IDHIPlugins.RandomCoordinatePlugin;


namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        public class RandomData
        {
            public ChaFileDefine.CoordinateType CategoryType { get; set; }
            public int CoordinateNumber { get; set; }
            public bool FirstRun { get; set; } = true;

            public Dictionary<ChaFileDefine.CoordinateType, List<int>>
                CoordinatesByType = new()
            {
                {ChaFileDefine.CoordinateType.Plain, new List<int> {}},
                {ChaFileDefine.CoordinateType.Swim, new List<int> {}},
                {ChaFileDefine.CoordinateType.Pajamas, new List<int> {}},
                {ChaFileDefine.CoordinateType.Bathing, new List<int> {}}
            };

            public Dictionary<ChaFileDefine.CoordinateType, int>
                NowRandomCoordinateByType = new()
            {
                {ChaFileDefine.CoordinateType.Plain, 0},
                {ChaFileDefine.CoordinateType.Swim, 1},
                {ChaFileDefine.CoordinateType.Pajamas, 2},
                {ChaFileDefine.CoordinateType.Bathing, 3}
            };

            public RandomData(
                ChaFileDefine.CoordinateType categoryType,
                int coordinateNumber,
                int statusCoordinate,
                ChaControl chaCtrl)
            {
                InitCoordinates(chaCtrl);
                CategoryType = categoryType;
                CoordinateNumber = coordinateNumber;
                NowRandomCoordinateByType[CategoryType] = statusCoordinate;
            }

            public RandomData(SaveData.Heroine heroine)
            {
                InitCoordinates(heroine);
                CategoryType = GetCategoryType(heroine.StatusCoordinate);
                NowRandomCoordinateByType[CategoryType] = heroine.StatusCoordinate;
                CoordinateNumber = heroine.StatusCoordinate;
            }

            public bool SetRandomData(
                ChaFileDefine.CoordinateType categoryType,
                int coordinateNumber,
                int statusCoordinate)
            {
                CategoryType = categoryType;
                CoordinateNumber = coordinateNumber;
                NowRandomCoordinateByType[CategoryType] = statusCoordinate;
                return true;
            }

            public bool SetRandomData(SaveData.Heroine heroine)
            {
                var rc = false;
                if (heroine != null)
                {
                    CategoryType = GetCategoryType(heroine.StatusCoordinate);
                    NowRandomCoordinateByType[CategoryType] = heroine.StatusCoordinate;
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
    }
}
