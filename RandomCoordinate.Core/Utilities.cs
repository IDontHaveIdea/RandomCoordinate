//
// Utilities
//
using System.Collections.Generic;

using ActionGame.Chara;

using KKAPI.MainGame;

using static IDHIPlugins.RandomCoordinatePlugin;


namespace IDHIPlugins
{
    public partial class Utilities
    {
        private static readonly Dictionary<ChaFileDefine.CoordinateType, List<int>>
            _coordinatesByType = new()
            {
                {ChaFileDefine.CoordinateType.Plain, new List<int> {}},
                {ChaFileDefine.CoordinateType.Swim, new List<int> {}},
                {ChaFileDefine.CoordinateType.Pajamas, new List<int> {}},
                {ChaFileDefine.CoordinateType.Bathing, new List<int> {}}
            };

        /// <summary>
        /// Overload GirlNme(ChaControl)
        /// </summary>
        /// <param name="girl"></param>
        /// <returns></returns>
        public static string GirlName(SaveData.Heroine girl)
        {
            var name = "";

            if (girl != null)
            {
                name = girl.Name.Trim();

                if (name == null)
                {
                    var rc = GirlName(girl.chaCtrl);

                    if (rc != null)
                    {
                        name = rc;
                    }
                }
            }
            return name;
        }

        /// <summary>
        /// Get the girl name for debugging purposes. Sometimes looking in
        /// the Savedata.Heroine class fails.
        /// </summary>
        /// <param name="girl"></param>
        /// <returns></returns>
        public static string GirlName(ChaControl girl)
        {
            return (girl.chaFile.parameter.fullname.Trim());
        }

        /// <summary>
        /// Overload GirlNme(ChaControl)
        /// </summary>
        /// <param name="girl"></param>
        /// <returns></returns>
        public static string GirlName(NPC girl)
        {
            return GirlName(girl.chaCtrl);
        }

        public static ChaFileDefine.CoordinateType GetCoordinateType(ChaControl girl, int coordinate)
        {
            //Dictionary<ChaFileDefine.CoordinateType, List<int>>
            //    coordinatesByType = new()
            //    {
            //        {ChaFileDefine.CoordinateType.Plain, new List<int> {}},
            //        {ChaFileDefine.CoordinateType.Swim, new List<int> {}},
            //        {ChaFileDefine.CoordinateType.Pajamas, new List<int> {}},
            //        {ChaFileDefine.CoordinateType.Bathing, new List<int> {}}
            //    };

            var rc = ChaFileDefine.CoordinateType.Plain;

            var totalCoordinates = girl.chaFile.coordinate.Length;

            for (var i = 0; i < 4; i++)
            {
                // Original 4 coordinates
                _coordinatesByType[(ChaFileDefine.CoordinateType)i].Clear();
                _coordinatesByType[(ChaFileDefine.CoordinateType)i].Add(i);
            }

            // Add additional outfits to Plain type for random selection
            for (var i = 4; i < totalCoordinates; i++)
            {
                // TODO: Interface to classify coordinates grater than 3
                _coordinatesByType[ChaFileDefine.CoordinateType.Plain].Add(i);
            }

            if (MathfEx.RangeEqualOn(0, coordinate, 3))
            {
                rc = (ChaFileDefine.CoordinateType)coordinate;
            }
            else
            {
                if (_coordinatesByType[ChaFileDefine.CoordinateType.Plain].Contains(coordinate))
                {
                    return ChaFileDefine.CoordinateType.Plain;
                }
                if (_coordinatesByType[ChaFileDefine.CoordinateType.Swim].Contains(coordinate))
                {
                    return ChaFileDefine.CoordinateType.Swim;
                }
                if (_coordinatesByType[ChaFileDefine.CoordinateType.Pajamas].Contains(coordinate))
                {
                    return ChaFileDefine.CoordinateType.Pajamas;
                }
            }

            return rc;
        }
    }

    public class RandomInfo
    {
        public ChaFileDefine.CoordinateType CoordinateType { get; set; }
        public int CoordinateNumber { get; set; }
        public Dictionary<ChaFileDefine.CoordinateType, int>
            NowRandomCoordinateByType = new()
                {
                    {ChaFileDefine.CoordinateType.Plain, 0},
                    {ChaFileDefine.CoordinateType.Swim, 1},
                    {ChaFileDefine.CoordinateType.Pajamas, 2},
                    {ChaFileDefine.CoordinateType.Bathing, 3}
                };

        public RandomInfo(
            ChaFileDefine.CoordinateType coordinateType,
            int coordinateNumber,
            int statusCoordinate)
        {
            CoordinateType = coordinateType;
            CoordinateNumber = coordinateNumber;
            NowRandomCoordinateByType[CoordinateType] = statusCoordinate;
        }
    }
}
