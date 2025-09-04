using BepInEx.Logging;
using BrutalAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace WMITF
{
    public static class ConsoleCommands
    {
        public static AutocompletionGroup AbilityAutocomplete = new(GetAbilityIDs);

        public static void Init()
        {
            var group = new DebugCommandGroup("wmitf", "The group for debug commands added by WMITF.");
            DebugController.Commands.children.Add(group);

            group.children.Add(new DebugCommand("item", "Tells you what mod an item is from.", new()
            {
                new StringCommandArgument("itemID", DebugController.ItemAutocomplete)
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
                new StringCommandArgument("characterID", DebugController.CharacterAutocomplete)
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
                new StringCommandArgument("enemyID", DebugController.EnemyAutocomplete)
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
                new StringCommandArgument("abilityID", AbilityAutocomplete)
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
        }

        public static HashSet<string> GetAbilityIDs()
        {
            var hs = new HashSet<string>();

            foreach (var a in LoadedDBsHandler.AbilityDB._characterAbilityPool)
                hs.Add(a);
            foreach (var a in LoadedDBsHandler.AbilityDB._enemyAbilityPool)
                hs.Add(a);

            foreach (var a in LoadedAssetsHandler.LoadedEnemyAbilities.Keys)
                hs.Add(a);
            foreach (var a in LoadedAssetsHandler.LoadedCharacterAbilities.Keys)
                hs.Add(a);

            return hs;
        }
    }
}
