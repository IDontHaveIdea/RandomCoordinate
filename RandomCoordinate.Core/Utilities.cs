//
// Utilities
//
using ActionGame.Chara;

using KKAPI.MainGame;

using static IDHIPlugins.RandomCoordinatePlugin;


namespace IDHIPlugins
{
    public partial class Utilities
    {
        /// <summary>
        /// Get the girl name for debugging purposes. Sometimes looking in
        /// the Savedata.Heroine class fails.
        /// </summary>
        /// <param name="girl"></param>
        /// <returns></returns>
        public static string GirlName(ChaControl girl)
        {
            var name = girl.GetHeroine()?.Name.Trim();

            if (name == null)
            {
                _ = GirlsNames.TryGetValue(girl.name, out var rc);
                name = rc;
            }

            return name;
        }

        /// <summary>
        /// Overload GirlNme(ChaControl)
        /// </summary>
        /// <param name="girl"></param>
        /// <returns></returns>
        public static string GirlName(SaveData.Heroine girl)
        {
            var name = girl.Name.Trim();

            if (name == null)
            {
                _ = GirlsNames.TryGetValue(girl.chaCtrl.name, out var rc);
                name = rc;
            }

            return name;
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
    }
}
