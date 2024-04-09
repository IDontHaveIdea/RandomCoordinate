//
// RandomCoordinatePlugin
//
using System.Linq;

using KKAPI.MainGame;

using Utils = IDHIUtils.Utilities;


namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        internal static SaveData.Heroine _guide;
        internal static int _guideMapNo;
        internal static bool _guideNewCoordinate = true;

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
                    var guideMap = -1;
                    var mapMove = -1;
                    var statusCoordinate = heroine.StatusCoordinate;
                    var nowRandomCoordinate = -1;
                    var newCoordinate = -1;

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
                            heroine.chaCtrl.fileStatus.coordinateType =
                                (int)ChaFileDefine.CoordinateType.Swim;
                            ctrl.SetRandomCoordinate(ChaFileDefine.CoordinateType.Swim);
                            ChangeCoordinate(
                                heroine.chaCtrl,
                                (int)ChaFileDefine.CoordinateType.Swim);
                        }
                        else
                        {
                            if (_guideNewCoordinate)
                            {
                                _guideNewCoordinate = false;
#if DEBUG
                                _Log.Warning("[SetGuide] Calling NewRandomCoordinateByType.");
#endif
                                // Guide won't be in any map that have special
                                // consideration
                                newCoordinate = ctrl.NewRandomCoordinateByType(
                                            ChaFileDefine.CoordinateType.Plain);
                                nowRandomCoordinate = ctrl.GetRandomCoordinate();
                                if (heroine.StatusCoordinate != newCoordinate)
                                {
                                    ChangeCoordinate(heroine.chaCtrl, newCoordinate);
                                }
                            }
                        }
                    }
#if DEBUG
                    var nowName = "";
                    var newName = "";
                    var mapName = Utils.MapName(guideMap);
                    if (statusCoordinate > 3)
                    {
                        nowName = $"({_MoreOutfits
                            .GetCoordinateName(heroine.chaCtrl, statusCoordinate)}) ";
                    }
                    if (ctrl.GetRandomCoordinate() > 3)
                    {
                        newName = $" ({_MoreOutfits
                            .GetCoordinateName(
                            heroine.chaCtrl, ctrl.GetRandomCoordinate())}).";
                    }
                    _Log.Debug($"[SetGuide] GUIDE={_guide.Name.Trim()} ({_guide.chaCtrl.name})" +
                        $"mapFix={fixChara.mapNo} mapNo={guideMap} ({mapName}) " +
                        $"statusCoordinate={statusCoordinate}{nowName} " +
                        $"NowRandomCoordinate={ctrl.GetRandomCoordinate()}{newName}.");
#endif
                }
            }
        }
    }
}
