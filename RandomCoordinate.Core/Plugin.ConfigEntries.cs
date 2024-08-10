//
// Configuration entries RandomCoordinate
//

using BepInEx.Configuration;
using BepInEx.Logging;

using KKAPI.Utilities;


namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugIn
    {
        internal static ConfigEntry<bool> DebugInfo { get; set; }
        internal static ConfigEntry<bool> DebugToConsole { get; set; }
        internal static ConfigEntry<bool> OnlyChangingRoom { get; set; }
        internal static ConfigEntry<bool> PajamasInRoom { get; set; }

        internal void ConfigEntries()
        {
            var sectionDescription = "Debug";

            DebugInfo = Config.Bind(
                section: sectionDescription,
                key: "Debug Information",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Show debug information",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes
                        { Order = 40, IsAdvanced = true }));
            DebugInfo.SettingChanged += (_sender, _args) =>
            {
                Log.Enabled = DebugInfo.Value;
#if DEBUG
                Log.Level(LogLevel.Info, $"[Configuration] Log.Enabled set " +
                    $"to {Log.Enabled}.");
#endif
            };

            DebugToConsole = Config.Bind(
                section: sectionDescription,
                key: "Debug information to Console",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Show debug information in Console",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes {
                        Order = 39,
                        IsAdvanced = true
                    }));
            DebugToConsole.SettingChanged += (_sender, _args) =>
            {
                Log.DebugToConsole = DebugToConsole.Value;
#if DEBUG
                Log.Level(LogLevel.Info, $"[ConfigEntries] Log.DebugToConsole set to " +
                    $"{Log.DebugToConsole}.");
#endif

            };

            sectionDescription = "Options";

            OnlyChangingRoom = Config.Bind(
                section: sectionDescription,
                key: "Random Coordinates Change Room Only",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Random coordinate selection will be " +
                        "effective only when the character is in a " +
                        "changing room.",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 30 }));
            OnlyChangingRoom.SettingChanged += (_sender, _args) =>
            {
#if DEBUG
                Log.Level(LogLevel.Info, $"[ConfigEntries] Random Coordinates Change " +
                    $"Room Only set to={OnlyChangingRoom.Value}.");
#endif
            };

            PajamasInRoom = Config.Bind(
                section: sectionDescription,
                key: "Use pajamas in Room",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Always us pajamas for H scenes in the Player Room.",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 29 }));
            OnlyChangingRoom.SettingChanged += (_sender, _args) =>
            {
#if DEBUG
                Log.Level(LogLevel.Info, $"[ConfigEntries] Always use pajamas in " +
                    $"Player Room set to={PajamasInRoom.Value}.");
#endif
            };
        }
    }
}
