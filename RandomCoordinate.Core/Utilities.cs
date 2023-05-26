//
// Utilities
//
using ActionGame.Chara;

using KKAPI.MainGame;

using static IDHIPlugIns.RandomCoordinatePlugin;


namespace IDHIPlugIns
{
    public partial class Utilities
    {
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
                    if (GirlsNames.TryGetValue(girl.chaCtrl.name, out var rc))
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
            return GirlName(girl.GetHeroine());
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
