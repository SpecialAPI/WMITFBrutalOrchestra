using BepInEx.Logging;
using BrutalAPI;
using System;
using System.Collections.Generic;
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
        }
    }
}
