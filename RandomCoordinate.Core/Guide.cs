//
// RandomCoordinatePlugin
//
using System.Collections;
using System.Text;

using UnityEngine;

using KKAPI.MainGame;

using Utils = IDHIUtils.Utilities;
using System.Linq;

namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        internal static SaveData.Heroine _guide;
        internal static int _guideMapNo;
        internal static bool getNewCoordinate = true;

        /// <summary>
        /// Save Heroine information for the Guide Character
        /// </summary>
        /// <param name="heroine"></param>
        /// <param name="setCoordinate"></param>
        internal static void SetupGuide(
            SaveData.Heroine heroine,
            bool setCoordinate = false)
        {
            if (heroine.fixCharaID == -13)
            {
                var fixChara = heroine.charaBase as ActionGame.Chara.Fix;

                if (fixChara != null)
                {
                    var ctrl = GetController(heroine.chaCtrl);
                    var guideMap = -1; // Utils.GuideMapNumber(heroine);
                    var mapMove = -1;
                    

                    mapMove = fixChara.charaData.moveData.mapNo;
                    guideMap = fixChara.mapNo;
                    var uMap = Utils.GuideMapNumber(heroine);

                    if ((guideMap <= 0)
                        && (Manager.Game.saveData.guideSetPositionMaps.Count == 1))
                    {
                        // When guideSetPositionMaps.Count == 1 this is the current map
                        // of the guide
                        guideMap = Manager.Game.saveData.guideSetPositionMaps
                            .ToList()
                            .FirstOrDefault();
                    }
                    else if ((mapMove > 0) && (guideMap <= 0))
                    {
                        guideMap = mapMove;
                    }

                    _guide = heroine;
                    _guideMapNo = guideMap;

                    if (setCoordinate)
                    {
                        // For the guide 
                        if (guideMap == 4)
                        {
                            heroine.chaCtrl.fileStatus.coordinateType = (int)ChaFileDefine.CoordinateType.Swim;
                            ctrl.SetRandomCoordinate(ChaFileDefine.CoordinateType.Swim);
                            ChangeCoordinate(heroine.chaCtrl, (int)ChaFileDefine.CoordinateType.Swim);
                        }
                        else
                        {
                            if (getNewCoordinate)
                            {
                                getNewCoordinate = false;
                                _Log.Error("Calling Random");
                                // Guide won't be in any map that have special
                                // consideration
                                var newCoordinate = ctrl.NewRandomCoordinateByType(
                                            ChaFileDefine.CoordinateType.Plain);
                                if (heroine.StatusCoordinate != newCoordinate)
                                {
                                    ChangeCoordinate(heroine.chaCtrl, newCoordinate);
                                }
                            }
                        }
                    }
#if DEBUG
                    _Log.Info($"[SetGuide] GUIDE={_guide.Name.Trim()} " +
                        $"chaName={_guide.chaCtrl.name} " +
                        $"position guideMap={guideMap} uMap={uMap} mapMove={mapMove} " +
                        $"mapFix={fixChara.mapNo} " +
                        $"move={fixChara.charaData.moveData.isAlive} getNewCoordinate={getNewCoordinate}.");
#endif
                }
            }
        }

        internal static void PeriodChange(object sendier, GameAPI.PeriodChangeEventArgs args)
        {
            var was = getNewCoordinate;
            getNewCoordinate = true;
            _Log.Info($"[PeriodChange] NewPeriod={args.NewPeriod} " +
                $"getNewCoordinate={getNewCoordinate} was={getNewCoordinate}");
            
        }
    }
    /*public partial class RandomCoordinatePlugin
    {
        internal static SaveData.Heroine _guide;
        internal static int _guideMapNo;
        internal static ActionGame.Chara.Fix _fixChara;

        /// <summary>
        /// Save Heroine information for the Guide Character
        /// </summary>
        /// <param name="heroine"></param>
        /// <param name="setCoordinate"></param>
        internal static void SetupGuide(
            SaveData.Heroine heroine,
            bool setCoordinate = false)
        {
            if (heroine.fixCharaID != -13)
            {
                return;
            }
            var hashes = new StringBuilder();
            var guideMap = -1;
            var fixChara = heroine.charaBase as ActionGame.Chara.Fix;
            var mapMove = -1;
            var mapFix = -1;
            var ctrl = GetController(heroine.chaCtrl);
            if (fixChara != null)
            {
                _fixChara = fixChara;
                mapMove = fixChara.charaData.moveData.mapNo;
                mapFix = fixChara.mapNo;
                guideMap = mapMove;
                if ((mapMove <= 0) && (mapFix != 0))
                {
                    guideMap = mapFix;
                }
            }
            _guide = heroine;
            _guideMapNo = guideMap;
            if (setCoordinate)
            {
                // For the guide 
                if (guideMap == 4)
                {
                    heroine.chaCtrl.fileStatus.coordinateType = (int)ChaFileDefine.CoordinateType.Swim;
                    ctrl.SetRandomCoordinate(ChaFileDefine.CoordinateType.Swim);
                    ChangeCoordinate(heroine.chaCtrl, (int)ChaFileDefine.CoordinateType.Swim);
                }
                else
                {
                    // Guide won't be in any map that have special consideration
                    var newCoordinate = ctrl.NewRandomCoordinateByType(
                                ChaFileDefine.CoordinateType.Plain);
                    ChangeCoordinate(heroine.chaCtrl, newCoordinate);
                }
            }
#if DEBUG
            _Log.Info($"[SetGuide] GUIDE={_guide.Name.Trim()} chaName={_guide.chaCtrl.name} " +
                $"position guideMap={guideMap} mapMove={mapMove} mapFix={mapFix}.");
#endif
        }
    }*/
}



/*
//
// RandomCoordinatePlugin
//
using System.Text;

using IDHIUtils;


namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        internal static SaveData.Heroine _guide;
        internal static int _guideMapNo;
        internal static ActionGame.Chara.Fix _fixChara;

        /// <summary>
        /// Save Heroine information for the Guide Character
        /// </summary>
        /// <param name="heroine"></param>
        /// <param name="setCoordinate"></param>
        internal static void SetupGuide(
            SaveData.Heroine heroine,
            bool setCoordinate = false)
        {
            if (heroine.fixCharaID != -13)
            {
                return;
            }
            var hashes = new StringBuilder();
            var guideMap = -1;
            var fixChara = heroine.charaBase as ActionGame.Chara.Fix;
            var mapMove = -1;
            var mapFix = -1;
            var ctrl = GetController(heroine.chaCtrl);
            if (fixChara != null)
            {
                _fixChara = fixChara;
                mapMove = fixChara.charaData.moveData.mapNo;
                mapFix = fixChara.mapNo;
                guideMap = mapMove;
                if ((mapMove <= 0) && (mapFix != 0))
                {
                    guideMap = mapFix;
                }
            }
            _guide = heroine;
            _guideMapNo = guideMap;
            if (setCoordinate)
            {
                // For the guide 
                if (guideMap == 4)
                {
                    heroine.chaCtrl.fileStatus.coordinateType = (int)ChaFileDefine.CoordinateType.Swim;
                    ctrl.SetRandomCoordinate(ChaFileDefine.CoordinateType.Swim);
                    ChangeCoordinate(heroine.chaCtrl, (int)ChaFileDefine.CoordinateType.Swim);
                }
                else
                {
                    // Guide won't be in any map that have special consideration
                    var newCoordinate = ctrl.NewRandomCoordinateByType(
                                ChaFileDefine.CoordinateType.Plain);
                    ChangeCoordinate(heroine.chaCtrl, newCoordinate);
                }
            }
#if DEBUG
            _Log.Info($"[SetGuide] GUIDE={_guide.Name.Trim()} chaName={_guide.chaCtrl.name} " +
                $"position guideMap={guideMap} mapMove={mapMove} mapFix={mapFix}.");
#endif
        }
    }
}

var hashes = new StringBuilder();
            var guideMap = -1;
            if (Manager.Game.saveData.guideSetPositionMaps != null)
            {
                hashes.Append("[ ");
                foreach (var h in Manager.Game.saveData.guideSetPositionMaps)
                {
                    if (Manager.Game.saveData.guideSetPositionMaps.Count == 1)
                    {
                        guideMap = h;
                    }
                    hashes.Append($"{h} ");
                }
                hashes.Append(']');
            }

 */
