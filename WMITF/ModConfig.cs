using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WMITF
{
    public static class ModConfig
    {
        public static ConfigFile File;

        public static ConfigEntry<Color> ModLabelColorConfig;
        public static ConfigEntry<bool> ShowModsForCharactersConfig;
        public static ConfigEntry<bool> ShowModsForEnemiesConfig;
        public static ConfigEntry<bool> ShowModsForItemsConfig;
        public static ConfigEntry<bool> ShowModsForAchievementsConfig;
        public static ConfigEntry<AbilityModDisplayCondition> ShowModsForAbilitiesConfig;
        public static ConfigEntry<StatusFieldDisplayCondition> ShowModsForStatusEffectsConfig;
        public static ConfigEntry<StatusFieldDisplayCondition> ShowModsForFieldEffectsConfig;

        public static Color ModLabelColor => ModLabelColorConfig.Value;
        public static bool ShowModsForCharacters => ShowModsForCharactersConfig.Value;
        public static bool ShowModsForEnemies => ShowModsForCharactersConfig.Value;
        public static bool ShowModsForItems => ShowModsForCharactersConfig.Value;
        public static bool ShowModsForAchievements => ShowModsForAchievementsConfig.Value;
        public static AbilityModDisplayCondition ShowModsForAbilities => ShowModsForAbilitiesConfig.Value;
        public static StatusFieldDisplayCondition ShowModsForStatusEffects => ShowModsForStatusEffectsConfig.Value;
        public static StatusFieldDisplayCondition ShowModsForFieldEffects => ShowModsForFieldEffectsConfig.Value;

        public static void Init(ConfigFile file)
        {
            File = file;

            ModLabelColorConfig = file.Bind("ModDisplay", "ModDisplayColor", new Color(0f, 0.5961f, 0.8667f), "The color of the mod display.");
            ShowModsForCharactersConfig = file.Bind("ModDisplay", "DisplayModsForCharacters", true, "Whether or not WMITF displays mods for characters.");
            ShowModsForEnemiesConfig = file.Bind("ModDisplay", "DisplayModsForEnemies", true, "Whether or not WMITF displays mods for enemies.");
            ShowModsForItemsConfig = file.Bind("ModDisplay", "DisplayModsForItems", true, "Whether or not WMITF displays mods for items.");
            ShowModsForAchievementsConfig = file.Bind("ModDisplay", "DisplayModsForAchievements", true, "Whether or not WMITF displays mods for achievements.");
            ShowModsForAbilitiesConfig = file.Bind("ModDisplay", "DisplayModsForAbilities", AbilityModDisplayCondition.OnlyForExtraAbilities, "Whether or not WMITF displays mods for abilities.");
            ShowModsForStatusEffectsConfig = file.Bind("ModDisplay", "DisplayModsForStatusEffects", StatusFieldDisplayCondition.On, "Whether or not WMITF displays mods for status effects.");
            ShowModsForFieldEffectsConfig = file.Bind("ModDisplay", "DisplayModsForFieldEffects", StatusFieldDisplayCondition.On, "Whether or not WMITF displays mods for field effects.");
        }

        public static string FormatModDisplay(string modName)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(ModLabelColor)}>[{modName}]</color>";
        }
    }

    public enum AbilityModDisplayCondition
    {
        Off,
        OnlyForExtraAbilities,
        On
    }

    public enum StatusFieldDisplayCondition
    {
        Off,
        OnlyInGlossary,
        On
    }
}
