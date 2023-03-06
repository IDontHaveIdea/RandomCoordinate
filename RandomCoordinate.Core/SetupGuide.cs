//
// RandomCoordinatePlugin
//
using System.Collections;

using KKAPI.MainGame;

using UnityEngine;

using Utils = IDHIUtils.Utilities;


namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        internal static SaveData.Heroine _guide;
        internal static int _guideMapNo;
        internal static ActionGame.Chara.Fix _fixChara;
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
                    var mapFix = -1;

                    _fixChara = fixChara;
                    mapMove = fixChara.charaData.moveData.mapNo;
                    mapFix = fixChara.mapNo;
                    guideMap = mapMove;
                    var uMap = Utils.GuideMapNumber(heroine);
                    if ((mapMove <= 0) && (mapFix != 0))
                    {
                        guideMap = mapFix;
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
                                _Log.Error("CARAJO GUIDE");
                                // Guide won't be in any map that have special consideration
                                var newCoordinate = ctrl.NewRandomCoordinateByType(
                                            ChaFileDefine.CoordinateType.Plain);
                                ChangeCoordinate(heroine.chaCtrl, newCoordinate);
                                getNewCoordinate = false;
                            }
                        }
                    }
#if DEBUG
                    _Log.Info($"[SetGuide] GUIDE={_guide.Name.Trim()} chaName={_guide.chaCtrl.name} " +
                        $"position guideMap={guideMap} uMap={uMap} mapMove={mapMove} mapFix={mapFix} " +
                        $"move={fixChara.charaData.moveData.isAlive}.");
#endif
                }
            }
        }
        internal static void PeriodChange(object sendier, GameAPI.PeriodChangeEventArgs args)
        {
            _Log.Info($"[PeriodChange] NewPeriod={args.NewPeriod}");
            getNewCoordinate = true;
        }
    }
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

 */
