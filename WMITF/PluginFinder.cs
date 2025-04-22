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

        public static readonly Dictionary<string, PluginInfo> ModdedCharacterPlugins = [];
        public static readonly Dictionary<string, PluginInfo> ModdedEnemyPlugins = [];
        public static readonly Dictionary<string, PluginInfo> ModdedWearablePlugins = [];

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

        public static void Init()
        {
            var pairs = new List<(Dictionary<string, Assembly> asmbls, Dictionary<string, PluginInfo> pinfos)>()
            {
                (ModdedCharacterAssemblies  ??= [], ModdedCharacterPlugins),
                (ModdedEnemyAssemblies      ??= [], ModdedEnemyPlugins),
                (ModdedWearableAssemblies   ??= [], ModdedWearablePlugins)
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
