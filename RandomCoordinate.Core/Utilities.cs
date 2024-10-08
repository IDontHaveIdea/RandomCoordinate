﻿//
// Utilities
//

using System;
using System.Collections.Generic;

using ActionGame.Chara;

using KKAPI.MainGame;

using static IDHIPlugins.RandomCoordinatePlugIn;


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
        /// the SaveData.Heroine class fails.
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

        /// <summary>
        /// Need this because the controller sometimes is not defined. The controller
        /// method is a little more efficient.
        /// TODO: This and the controller one are done this way for when I decide to
        /// implement categories for the MoreOutfits coordinates
        /// </summary>
        /// <param name="girl"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public static ChaFileDefine.CoordinateType GetCoordinateType(
            ChaControl girl, int coordinate)
        {
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
                if (_coordinatesByType[ChaFileDefine.CoordinateType.Plain]
                    .Contains(coordinate))
                {
                    return ChaFileDefine.CoordinateType.Plain;
                }
                if (_coordinatesByType[ChaFileDefine.CoordinateType.Swim]
                    .Contains(coordinate))
                {
                    return ChaFileDefine.CoordinateType.Swim;
                }
                if (_coordinatesByType[ChaFileDefine.CoordinateType.Pajamas]
                    .Contains(coordinate))
                {
                    return ChaFileDefine.CoordinateType.Pajamas;
                }
            }
            return rc;
        }

        public static string PseudoKey(ChaControl chaControl)
        {
            var name = chaControl.chaFile.parameter.fullname.Trim();
            var personality = chaControl.chaFile.parameter.personality.ToString();
            var height = (int)Math.Round(chaControl.chaFile.custom.body
                .shapeValueBody[(int)ChaFileDefine.BodyShapeIdx.Height] * 100);

            var rc = $"{name}.{personality}.{height}";

            return rc;
        }

        /// <summary>
        /// Reset some variables to force a new random selection to occur.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal static void PeriodChange(
            object sender,
            GameAPI.PeriodChangeEventArgs args)
        {
            var was = _guideNewCoordinate;
            _guideNewCoordinate = true;
            if (GirlsRandomData.Count > 0)
            {
                foreach (var item in GirlsRandomData)
                {
                    item.Value.FirstRun = true;
                }
            }
#if DEBUG
            Log.Warning($"[PeriodChange] NewPeriod={args.NewPeriod} " +
                $"getNewCoordinate={_guideNewCoordinate} was={was}.");
#endif
        }

        /// <summary>
        /// Reset some variables to force a new random coordinate selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal static void DayChange( object sender, GameAPI.DayChangeEventArgs args )
        {
            var was = _guideNewCoordinate;
            _guideNewCoordinate = true;
            if (GirlsRandomData.Count > 0)
            {
                foreach (var item in GirlsRandomData)
                {
                    item.Value.FirstRun = true;
                }
            }
#if DEBUG
            Log.Warning($"[Daychange] NewDay={args.NewDay} " +
                $"getNewCoordinate={_guideNewCoordinate} was={was}.");
#endif
        }

    }
}
