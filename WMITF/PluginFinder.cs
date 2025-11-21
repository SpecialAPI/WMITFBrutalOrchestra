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
        public static readonly Dictionary<string, PluginInfo> ModdedCharacterPlugins = [];
        public static readonly Dictionary<string, PluginInfo> ModdedEnemyPlugins = [];
        public static readonly Dictionary<string, PluginInfo> ModdedWearablePlugins = [];
        public static readonly Dictionary<string, PluginInfo> ModdedAchievementPlugins = [];
        public static readonly Dictionary<string, PluginInfo> ModdedAbilityPlugins = [];

        public static readonly Dictionary<StatusEffectInfoSO, PluginInfo> ModdedStatusEffectPlugins = [];
        public static readonly Dictionary<SlotStatusEffectInfoSO, PluginInfo> ModdedFieldEffectPlugins = [];

        public static void Init()
        {
            var pairs = new List<(Dictionary<string, Assembly> asmbls, Dictionary<string, PluginInfo> pinfos)>()
            {
                (ModAssemblyStorage.ModdedCharacterAssemblies,     ModdedCharacterPlugins),
                (ModAssemblyStorage.ModdedEnemyAssemblies,         ModdedEnemyPlugins),
                (ModAssemblyStorage.ModdedWearableAssemblies,      ModdedWearablePlugins),
                (ModAssemblyStorage.ModdedAchievementAssemblies,   ModdedAchievementPlugins),
                (ModAssemblyStorage.ModdedAbilityAssemblies,       ModdedAbilityPlugins)
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

            foreach (var kvp in ModAssemblyStorage.ModdedStatusEffectAssemblies)
            {
                var plugin = FindPluginWithAssembly(kvp.Value);

                if (plugin == null)
                    continue;

                ModdedStatusEffectPlugins[kvp.Key] = plugin;
            }

            foreach (var kvp in ModAssemblyStorage.ModdedFieldEffectAssemblies)
            {
                var plugin = FindPluginWithAssembly(kvp.Value);

                if (plugin == null)
                    continue;

                ModdedFieldEffectPlugins[kvp.Key] = plugin;
            }

            foreach (var kvp in ModAssemblyStorage.ModdedAbilitySOAssemblies)
            {
                var ab = kvp.Key;

                if (ab == null)
                    continue;

                var name = ab.name;

                if (string.IsNullOrEmpty(name) || ModdedAbilityPlugins.ContainsKey(name))
                    continue;

                var plugin = FindPluginWithAssembly(kvp.Value);

                if (plugin == null)
                    continue;

                ModdedAbilityPlugins[name] = plugin;
            }
        }

        public static bool PluginIsIgnored(PluginInfo plugin)
        {
            if (ModConfig.IgnoredMods == null)
                return false;

            if(Array.IndexOf(ModConfig.IgnoredMods, plugin.Metadata.GUID) < 0)
                return false;

            return true;
        }

        public static bool TryGetCharacterModName(CharacterSO character, out string modName)
        {
            modName = string.Empty;

            if(character == null)
                return false;

            var id = character.name;

            if (string.IsNullOrEmpty(id) || !ModdedCharacterPlugins.TryGetValue(id, out var plugin))
                return false;

            if (PluginIsIgnored(plugin))
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

            if (PluginIsIgnored(plugin))
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

            if (PluginIsIgnored(plugin))
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

            if (PluginIsIgnored(plugin))
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

            if (PluginIsIgnored(plugin))
                return false;

            modName = ModConfig.FormatModDisplay(plugin.Metadata.Name);
            return true;
        }

        public static bool TryGetStatusEffectModName(StatusEffectInfoSO se, out string modName)
        {
            modName = string.Empty;

            if (se == null)
                return false;

            if (!ModdedStatusEffectPlugins.TryGetValue(se, out var plugin))
                return false;

            if (PluginIsIgnored(plugin))
                return false;

            modName = ModConfig.FormatModDisplay(plugin.Metadata.Name);
            return true;
        }

        public static bool TryGetFieldEffectModName(SlotStatusEffectInfoSO fe, out string modName)
        {
            modName = string.Empty;

            if (fe == null)
                return false;

            if (!ModdedFieldEffectPlugins.TryGetValue(fe, out var plugin))
                return false;

            if (PluginIsIgnored(plugin))
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
