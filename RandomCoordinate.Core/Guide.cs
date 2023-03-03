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

        /// <summary>
        /// Save Heroine information for the Guide Character
        /// </summary>
        /// <param name="heroine"></param>
        /// <param name="setCoordinate"></param>
        internal static void SetupGuide(
            SaveData.Heroine heroine,
            bool setCoordinate = false)
        {
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

            _guide = heroine;
#if DEBUG
            _Log.Info($"[SetGuide] GUIDE={_guide.Name.Trim()} chaName={_guide.chaCtrl.name} " +
                $"position maps={hashes} mapNo={guideMap}");
#endif
            if (setCoordinate)
            {
                // For the guide 
                if (guideMap == 4)
                {
                    heroine.chaCtrl.fileStatus.coordinateType = (int)CoordinateType.Swim;
                    ChangeCoordinate(heroine.chaCtrl, (int)CoordinateType.Swim);
                }
                else
                {
                    // Guide won't be in any map that have special consideration
                    var ctrl = GetController(heroine.chaCtrl);
                    var newCoordinate = ctrl.NewRandomCoordinateByType(
                                ChaFileDefine.CoordinateType.Plain);
                    ChangeCoordinate(heroine.chaCtrl, newCoordinate);
                }
            }
        }
    }
}
