using BepInEx.Configuration;
using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BepInEx.Bootstrap;

namespace WMITF
{
    public static class PluginFinder
    {
        private static readonly FieldInfo _moddedCH = AccessTools.Field(typeof(LoadedAssetsHandler), "WMITF_ModdedCharacters");
        private static readonly FieldInfo _moddedEN = AccessTools.Field(typeof(LoadedAssetsHandler), "WMITF_ModdedEnemies");
        private static readonly FieldInfo _moddedW = AccessTools.Field(typeof(LoadedAssetsHandler), "WMITF_ModdedWearables");
        private static readonly FieldInfo _moddedACH = AccessTools.Field(typeof(LoadedAssetsHandler), "WMITF_ModdedAchievements");
        private static readonly FieldInfo _moddedA = AccessTools.Field(typeof(LoadedAssetsHandler), "WMITF_ModdedAbilities");

        public static readonly Dictionary<string, PluginInfo> ModdedCharacterPlugins = [];
        public static readonly Dictionary<string, PluginInfo> ModdedEnemyPlugins = [];
        public static readonly Dictionary<string, PluginInfo> ModdedWearablePlugins = [];
        public static readonly Dictionary<string, PluginInfo> ModdedAchievementPlugins = [];
        public static readonly Dictionary<string, PluginInfo> ModdedAbilityPlugins = [];

        public static Dictionary<string, Assembly> ModdedCharacterAssemblies
        {
            get => _moddedCH.GetValue(null) as Dictionary<string, Assembly>;
            set => _moddedCH.SetValue(null, value);
        }

        public static Dictionary<string, Assembly> ModdedEnemyAssemblies
        {
            get => _moddedEN.GetValue(null) as Dictionary<string, Assembly>;
            set => _moddedEN.SetValue(null, value);
        }

        public static Dictionary<string, Assembly> ModdedWearableAssemblies
        {
            get => _moddedW.GetValue(null) as Dictionary<string, Assembly>;
            set => _moddedW.SetValue(null, value);
        }

        public static Dictionary<string, Assembly> ModdedAchievementAssemblies
        {
            get => _moddedACH.GetValue(null) as Dictionary<string, Assembly>;
            set => _moddedACH.SetValue(null, value);
        }

        public static Dictionary<string, Assembly> ModdedAbilityAssemblies
        {
            get => _moddedA.GetValue(null) as Dictionary<string, Assembly>;
            set => _moddedA.SetValue(null, value);
        }

        public static void Init()
        {
            var pairs = new List<(Dictionary<string, Assembly> asmbls, Dictionary<string, PluginInfo> pinfos)>()
            {
                (ModdedCharacterAssemblies      ??= [], ModdedCharacterPlugins),
                (ModdedEnemyAssemblies          ??= [], ModdedEnemyPlugins),
                (ModdedWearableAssemblies       ??= [], ModdedWearablePlugins),
                (ModdedAchievementAssemblies    ??= [], ModdedAchievementPlugins),
                (ModdedAbilityAssemblies        ??= [], ModdedAbilityPlugins)
            };

            foreach (var (asmbls, pinfos) in pairs)
            {
                foreach (var kvp in asmbls)
                {
                    var plugin = FindPluginWithAssembly(kvp.Value);

                    if (plugin == null)
                        continue;

                    pinfos[kvp.Key] = plugin;
                }
            }
        }

        public static bool TryGetCharacterModName(CharacterSO character, out string modName)
        {
            modName = string.Empty;

            if(character == null)
                return false;

            var id = character.name;

            if (string.IsNullOrEmpty(id) || !ModdedCharacterPlugins.TryGetValue(id, out var plugin))
                return false;

            modName = ModConfig.FormatModDisplay(plugin.Metadata.Name);
            return true;
        }

        public static bool TryGetEnemyModName(EnemySO enemy, out string modName)
        {
            modName = string.Empty;

            if (enemy == null)
                return false;

            var id = enemy.name;

            if (string.IsNullOrEmpty(id) || !ModdedEnemyPlugins.TryGetValue(id, out var plugin))
                return false;

            modName = ModConfig.FormatModDisplay(plugin.Metadata.Name);
            return true;
        }

        public static bool TryGetWearableModName(BaseWearableSO wearable, out string modName)
        {
            modName = string.Empty;

            if (wearable == null)
                return false;

            var id = wearable.name;

            if (string.IsNullOrEmpty(id) || !ModdedWearablePlugins.TryGetValue(id, out var plugin))
                return false;

            modName = ModConfig.FormatModDisplay(plugin.Metadata.Name);
            return true;
        }

        public static bool TryGetAchievementModName(ModdedAchievement_t achievement, out string modName)
        {
            modName = string.Empty;

            if (achievement == null)
                return false;

            var id = achievement.m_eAchievementID;

            if (string.IsNullOrEmpty(id) || !ModdedAchievementPlugins.TryGetValue(id, out var plugin))
                return false;

            modName = ModConfig.FormatModDisplay(plugin.Metadata.Name);
            return true;
        }

        public static bool TryGetAbilityModName(AbilitySO ability, out string modName)
        {
            modName = string.Empty;

            if (ability == null)
                return false;

            var id = ability.name;

            if (string.IsNullOrEmpty(id) || !ModdedAbilityPlugins.TryGetValue(id, out var plugin))
                return false;

            modName = ModConfig.FormatModDisplay(plugin.Metadata.Name);
            return true;
        }

        public static PluginInfo FindPluginWithAssembly(Assembly asmbl)
        {
            if (asmbl == null)
                return null;

            foreach (var pinfo in Chainloader.PluginInfos.Values)
            {
                if (pinfo == null || pinfo.Instance == null)
                    continue;

                if (pinfo.Instance.GetType().Assembly != asmbl)
                    continue;

                return pinfo;
            }

            return null;
        }
    }
}
