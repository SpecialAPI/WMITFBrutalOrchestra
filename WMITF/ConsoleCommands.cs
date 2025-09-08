using BepInEx.Logging;
using BrutalAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WMITF
{
    public static class ConsoleCommands
    {
        public static void Init()
        {
            var group = new DebugCommandGroup("wmitf", "The group for debug commands added by WMITF.");
            DebugController.Commands.children.Add(group);

            group.children.Add(new DebugCommand("item", "Tells you what mod an item is from.", new()
            {
                new StringCommandArgument("itemID", new(() => PluginFinder.ModdedWearablePlugins.Keys))
            }, x =>
            {
                var id = x[0].Read<string>();

                if (id == null)
                    return;

                var itm = LoadedAssetsHandler.GetWearable(id);

                if (itm == null)
                {
                    DebugController.Instance.WriteLine($"Unknown item \"{id}\".", LogLevel.Error);
                    return;
                }

                if (PluginFinder.ModdedWearablePlugins.TryGetValue(itm.name, out var plugin))
                    DebugController.Instance.WriteLine($"{itm.GetItemLocData().text} ({itm.name}) is from {plugin.Metadata.Name ?? "[null plugin name, this should not be happening]"} ({plugin.Metadata.GUID ?? "[null plugin GUID, this should not be happening"})");
                else
                    DebugController.Instance.WriteLine($"{itm.GetItemLocData().text} ({itm.name}) is either not modded or is not recognized by WMITF.");
            }));

            group.children.Add(new DebugCommand("character", "Tells you what mod a character is from.", new()
            {
                new StringCommandArgument("characterID", new(() => PluginFinder.ModdedCharacterPlugins.Keys))
            }, x =>
            {
                var id = x[0].Read<string>();

                if (id == null)
                    return;

                var ch = LoadedAssetsHandler.GetCharacter(id);

                if (ch == null)
                {
                    DebugController.Instance.WriteLine($"Unknown character \"{id}\".", LogLevel.Error);
                    return;
                }

                if (PluginFinder.ModdedCharacterPlugins.TryGetValue(ch.name, out var plugin))
                    DebugController.Instance.WriteLine($"{ch.GetName()} ({ch.name}) is from {plugin.Metadata.Name ?? "[null plugin name, this should not be happening]"} ({plugin.Metadata.GUID ?? "[null plugin GUID, this should not be happening"})");
                else
                    DebugController.Instance.WriteLine($"{ch.GetName()} ({ch.name}) is either not modded or is not recognized by WMITF.");
            }));

            group.children.Add(new DebugCommand("enemy", "Tells you what mod an enemy is from.", new()
            {
                new StringCommandArgument("enemyID", new(() => PluginFinder.ModdedEnemyPlugins.Keys))
            }, x =>
            {
                var id = x[0].Read<string>();

                if (id == null)
                    return;

                var en = LoadedAssetsHandler.GetEnemy(id);

                if (en == null)
                {
                    DebugController.Instance.WriteLine($"Unknown enemy \"{id}\".", LogLevel.Error);
                    return;
                }

                if (PluginFinder.ModdedEnemyPlugins.TryGetValue(en.name, out var plugin))
                    DebugController.Instance.WriteLine($"{en.GetName()} ({en.name}) is from {plugin.Metadata.Name ?? "[null plugin name, this should not be happening]"} ({plugin.Metadata.GUID ?? "[null plugin GUID, this should not be happening"})");
                else
                    DebugController.Instance.WriteLine($"{en.GetName()} ({en.name}) is either not modded or is not recognized by WMITF.");
            }));

            group.children.Add(new DebugCommand("ability", "Tells you what mod an ability is from.", new()
            {
                new StringCommandArgument("abilityID", new(() => PluginFinder.ModdedAbilityPlugins.Keys))
            }, x =>
            {
                var id = x[0].Read<string>();

                if (id == null)
                    return;

                var ab = LoadedAssetsHandler.GetEnemyAbility(id);
                if(ab == null)
                    ab = LoadedAssetsHandler.GetCharacterAbility(id);

                if (ab == null)
                {
                    DebugController.Instance.WriteLine($"Unknown ability \"{id}\".", LogLevel.Error);
                    return;
                }

                if (PluginFinder.ModdedAbilityPlugins.TryGetValue(ab.name, out var plugin))
                    DebugController.Instance.WriteLine($"{ab.GetAbilityLocData().text} ({ab.name}) is from {plugin.Metadata.Name ?? "[null plugin name, this should not be happening]"} ({plugin.Metadata.GUID ?? "[null plugin GUID, this should not be happening"})");
                else
                    DebugController.Instance.WriteLine($"{ab.GetAbilityLocData().text} ({ab.name}) is either not modded or is not recognized by WMITF.");
            }));

            group.children.Add(new DebugCommand("achievement", "Tells you what mod an achievement is from.", new()
            {
                new StringCommandArgument("abilityID", new(() => PluginFinder.ModdedAchievementPlugins.Keys))
            }, x =>
            {
                var id = x[0].Read<string>();

                if (id == null)
                    return;

                var ach = LoadedDBsHandler.AchievementDB.GetModdedAchievementInfo(id);

                if (ach == null)
                {
                    DebugController.Instance.WriteLine($"Unknown achievement \"{id}\".", LogLevel.Error);
                    return;
                }

                if (ach is ModdedAchievement_t modAch && PluginFinder.ModdedAchievementPlugins.TryGetValue(modAch.m_eAchievementID, out var plugin))
                    DebugController.Instance.WriteLine($"{ach.GetAchLocData().text} ({modAch.m_eAchievementID}) is from {plugin.Metadata.Name ?? "[null plugin name, this should not be happening]"} ({plugin.Metadata.GUID ?? "[null plugin GUID, this should not be happening"})");
                else
                    DebugController.Instance.WriteLine($"{ach.GetAchLocData().text} is either not modded or is not recognized by WMITF.");
            }));

            group.children.Add(new DebugCommand("statuseffect", "Tells you what mod a status effect is from.", new()
            {
                new StringCommandArgument("statusEffectID", new(LoadWMITFStatusIDs))
            }, x =>
            {
                var id = x[0].Read<string>();

                if (id == null)
                    return;

                if (!LoadedDBsHandler.StatusFieldDB.TryGetStatusEffect(id, out var status) || status.EffectInfo == null)
                {
                    DebugController.Instance.WriteLine($"Unknown status effect \"{id}\".", LogLevel.Error);
                    return;
                }

                if (PluginFinder.ModdedStatusEffectPlugins.TryGetValue(status.EffectInfo, out var plugin))
                    DebugController.Instance.WriteLine($"{status.EffectInfo.GetStatusLocData().text} ({status.StatusID}) is from {plugin.Metadata.Name ?? "[null plugin name, this should not be happening]"} ({plugin.Metadata.GUID ?? "[null plugin GUID, this should not be happening"})");
                else
                    DebugController.Instance.WriteLine($"{status.EffectInfo.GetStatusLocData().text} ({status.StatusID}) is either not modded or is not recognized by WMITF.");
            }));

            group.children.Add(new DebugCommand("fieldeffect", "Tells you what mod a status effect is from.", new()
            {
                new StringCommandArgument("fieldEffectID", new(LoadWMITFFieldIDs))
            }, x =>
            {
                var id = x[0].Read<string>();

                if (id == null)
                    return;

                if (!LoadedDBsHandler.StatusFieldDB.TryGetFieldEffect(id, out var field) || field.EffectInfo == null)
                {
                    DebugController.Instance.WriteLine($"Unknown field effect \"{id}\".", LogLevel.Error);
                    return;
                }

                if (PluginFinder.ModdedFieldEffectPlugins.TryGetValue(field.EffectInfo, out var plugin))
                    DebugController.Instance.WriteLine($"{field.EffectInfo.GetFieldLocData().text} ({field.FieldID}) is from {plugin.Metadata.Name ?? "[null plugin name, this should not be happening]"} ({plugin.Metadata.GUID ?? "[null plugin GUID, this should not be happening"})");
                else
                    DebugController.Instance.WriteLine($"{field.EffectInfo.GetFieldLocData().text} ({field.FieldID}) is either not modded or is not recognized by WMITF.");
            }));
        }

        public static HashSet<string> LoadWMITFStatusIDs()
        {
            var output = new HashSet<string>();
            var statuses = LoadedDBsHandler.StatusFieldDB.StatusEffects;

            foreach(var kvp in statuses)
            {
                if (kvp.Value == null || kvp.Value.EffectInfo == null || !PluginFinder.ModdedStatusEffectPlugins.ContainsKey(kvp.Value.EffectInfo))
                    continue;

                output.Add(kvp.Key);
            }

            return output;
        }

        public static HashSet<string> LoadWMITFFieldIDs()
        {
            var output = new HashSet<string>();
            var fields = LoadedDBsHandler.StatusFieldDB.FieldEffects;

            foreach (var kvp in fields)
            {
                if (kvp.Value == null || kvp.Value.EffectInfo == null || !PluginFinder.ModdedFieldEffectPlugins.ContainsKey(kvp.Value.EffectInfo))
                    continue;

                output.Add(kvp.Key);
            }

            return output;
        }
    }
}
