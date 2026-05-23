using BepInEx;
using BepInEx.Logging;
using BrutalAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace WMITF
{
    public static class ConsoleCommands
    {
        public static DebugCommandGroup Group;

        public static void Init()
        {
            Group = new DebugCommandGroup("wmitf", "The group for debug commands added by WMITF.");
            DebugController.Commands.children.Add(Group);

            new WMITFItemCommandBuilder();
            new WMITFCharacterCommandBuilder();
            new WMITFEnemyCommandBuilder();
            new WMITFAbilityCommandBuilder();
            new WMITFAchievementCommandBuilder();
            new WMITFStatusEffectCommandBuilder();
            new WMITFFieldEffectCommandBuilder();
        }

        private class WMITFItemCommandBuilder : WMITFCommandBuilderScriptObjBase<BaseWearableSO>
        {
            public override string AppliesTo => "item";
            public override string Article => "an";

            public override Dictionary<string, PluginInfo> PluginsDictionary => PluginFinder.ModdedWearablePlugins;

            public override string GetDisplayName(BaseWearableSO obj)
            {
                return obj.GetItemLocData().text;
            }

            public override BaseWearableSO LoadObject(string id)
            {
                return LoadedAssetsHandler.GetWearable(id);
            }
        }

        private class WMITFCharacterCommandBuilder : WMITFCommandBuilderScriptObjBase<CharacterSO>
        {
            public override string AppliesTo => "character";

            public override Dictionary<string, PluginInfo> PluginsDictionary => PluginFinder.ModdedCharacterPlugins;

            public override string GetDisplayName(CharacterSO obj)
            {
                return obj.GetName();
            }

            public override CharacterSO LoadObject(string id)
            {
                return LoadedAssetsHandler.GetCharacter(id);
            }
        }

        private class WMITFEnemyCommandBuilder : WMITFCommandBuilderScriptObjBase<EnemySO>
        {
            public override string AppliesTo => "enemy";
            public override string Article => "an";

            public override Dictionary<string, PluginInfo> PluginsDictionary => PluginFinder.ModdedEnemyPlugins;

            public override string GetDisplayName(EnemySO obj)
            {
                return obj.GetName();
            }

            public override EnemySO LoadObject(string id)
            {
                return LoadedAssetsHandler.GetEnemy(id);
            }
        }

        private class WMITFAbilityCommandBuilder : WMITFCommandBuilderScriptObjBase<AbilitySO>
        {
            public override string AppliesTo => "ability";
            public override string Article => "an";

            public override Dictionary<string, PluginInfo> PluginsDictionary => PluginFinder.ModdedAbilityPlugins;

            public override string GetDisplayName(AbilitySO obj)
            {
                return obj.GetAbilityLocData().text;
            }

            public override AbilitySO LoadObject(string id)
            {
                var ab = LoadedAssetsHandler.GetEnemyAbility(id);
                if (ab != null)
                    return ab;

                ab = LoadedAssetsHandler.GetCharacterAbility(id);
                if (ab != null)
                    return ab;

                return null;
            }
        }

        private class WMITFAchievementCommandBuilder : WMITFCommandBuilderBase<AchievementBase_t, string>
        {
            public override string AppliesTo => "achievement";
            public override string Article => "an";

            public override Dictionary<string, PluginInfo> PluginsDictionary => PluginFinder.ModdedAchievementPlugins;

            public override IEnumerable<string> Autocomplete() => PluginsDictionary.Keys;

            public override string GetDisplayName(AchievementBase_t obj)
            {
                return obj.GetAchLocData().text;
            }

            public override string GetID(AchievementBase_t obj)
            {
                return GetKey(obj);
            }

            public override string GetKey(AchievementBase_t obj)
            {
                if (obj is ModdedAchievement_t modAch)
                    return modAch.m_eAchievementID;

                return string.Empty;
            }

            public override AchievementBase_t LoadObject(string id)
            {
                return LoadedDBsHandler.AchievementDB.GetModdedAchievementInfo(id);
            }
        }

        private class WMITFStatusEffectCommandBuilder : WMITFCommandBuilderBase<StatusEffect_SO, StatusEffectInfoSO>
        {
            public override string AppliesTo => "status effect";

            public override Dictionary<StatusEffectInfoSO, PluginInfo> PluginsDictionary => PluginFinder.ModdedStatusEffectPlugins;

            public override IEnumerable<string> Autocomplete()
            {
                var output = new HashSet<string>();
                var statuses = LoadedDBsHandler.StatusFieldDB.StatusEffects;

                foreach (var kvp in statuses)
                {
                    if (kvp.Value == null || kvp.Value.EffectInfo == null || !PluginFinder.ModdedStatusEffectPlugins.ContainsKey(kvp.Value.EffectInfo))
                        continue;

                    output.Add(kvp.Key);
                }

                return output;
            }

            public override string GetDisplayName(StatusEffect_SO obj)
            {
                return obj.EffectInfo.GetStatusLocData().text;
            }

            public override string GetID(StatusEffect_SO obj)
            {
                return obj.StatusID;
            }

            public override StatusEffectInfoSO GetKey(StatusEffect_SO obj)
            {
                return obj.EffectInfo;
            }

            public override StatusEffect_SO LoadObject(string id)
            {
                if (LoadedDBsHandler.StatusFieldDB.TryGetStatusEffect(id, out var status) && status.EffectInfo != null)
                    return status;

                return null;
            }
        }

        private class WMITFFieldEffectCommandBuilder : WMITFCommandBuilderBase<FieldEffect_SO, SlotStatusEffectInfoSO>
        {
            public override string AppliesTo => "field effect";

            public override Dictionary<SlotStatusEffectInfoSO, PluginInfo> PluginsDictionary => PluginFinder.ModdedFieldEffectPlugins;

            public override IEnumerable<string> Autocomplete()
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

            public override string GetDisplayName(FieldEffect_SO obj)
            {
                return obj.EffectInfo.GetFieldLocData().text;
            }

            public override string GetID(FieldEffect_SO obj)
            {
                return obj.FieldID;
            }

            public override SlotStatusEffectInfoSO GetKey(FieldEffect_SO obj)
            {
                return obj.EffectInfo;
            }

            public override FieldEffect_SO LoadObject(string id)
            {
                if (LoadedDBsHandler.StatusFieldDB.TryGetFieldEffect(id, out var field) && field.EffectInfo != null)
                    return field;

                return null;
            }
        }

        private abstract class WMITFCommandBuilderScriptObjBase<TKey> : WMITFCommandBuilderBase<TKey, string> where TKey : ScriptableObject
        {
            public override string GetID(TKey obj) => obj.name;

            public override string GetKey(TKey obj) => obj.name;

            public override IEnumerable<string> Autocomplete() => PluginsDictionary.Keys;
        }

        private abstract class WMITFCommandBuilderBase<TObj, TKey>
        {
            public abstract string AppliesTo { get; }
            public virtual string Article => "a";
            public abstract Dictionary<TKey, PluginInfo> PluginsDictionary { get; }

            private string Name => AppliesTo.ToLowerInvariant().Replace(" ", "");
            private string Argument => $"{AppliesTo.ToCamelCase()}ID";

            private const string UnknownObjectError = "Unknown {0} \"{1}\".";
            private const string ModdedObjectMessage = "{0} ({1}) is from {2} ({3}).";
            private const string ModdedObjectMessageNoID = "{0} is from {2} ({3}).";
            private const string UnmoddedObjectMessage = "{0} ({1}) is either not modded or is not recognized by WMITF.";
            private const string UnmoddedObjectMessageNoID = "{0} is either not modded or is not recognized by WMITF.";

            public WMITFCommandBuilderBase()
            {
                if (Group == null)
                {
                    Debug.LogError($"Trying to create a WMITF command builder when Group is null.");
                    return;
                }

                var commandName = Name;
                if (Group.children.Any(IsCommand))
                {
                    Debug.LogError($"Command with name {commandName} already exists.");
                    return;
                }

                Group.children.Add(new DebugCommand(commandName, $"Tells you what mod {Article} {AppliesTo} is from.", new()
                {
                    new StringCommandArgument(Argument, new(Autocomplete))
                }, ExecuteCommand));
            }

            private bool IsCommand(DebugCommandGroup cmd)
            {
                if(cmd == null)
                    return false;

                return cmd.name == Name;
            }

            private void ExecuteCommand(List<FilledCommandArgument> args)
            {
                var id = args[0].Read<string>();

                if (id == null)
                    return;

                var obj = LoadObject(id);

                if (!CheckExists(obj))
                {
                    DebugController.Instance.WriteLine(string.Format(UnknownObjectError, AppliesTo, id), LogLevel.Error);
                    return;
                }

                var key = GetKey(obj);
                var objId = GetID(obj) ?? "";
                var objDisplay = GetDisplayName(obj) ?? "";

                if (KeyIsValid(key) && PluginsDictionary.TryGetValue(key, out var plugin))
                {
                    var messageFormat = ModdedObjectMessage;
                    if (string.IsNullOrEmpty(objId))
                        messageFormat = ModdedObjectMessageNoID;

                    DebugController.Instance.WriteLine(string.Format(messageFormat, objDisplay, objId, plugin.Metadata.Name, plugin.Metadata.GUID));
                }
                else
                {
                    var messageFormat = UnmoddedObjectMessage;
                    if (string.IsNullOrEmpty(objId))
                        messageFormat = UnmoddedObjectMessageNoID;

                    DebugController.Instance.WriteLine(string.Format(messageFormat, objDisplay, objId));
                }
            }

            public virtual bool KeyIsValid(TKey key)
            {
                if(key == null)
                    return false;
                
                if(key is string str && string.IsNullOrEmpty(str))
                    return false;

                return true;
            }

            public abstract IEnumerable<string> Autocomplete();

            public abstract TObj LoadObject(string id);

            public virtual bool CheckExists(TObj obj)
            {
                if (obj is UnityEngine.Object unityObj)
                    return unityObj != null;
                else
                    return obj != null;
            }

            public abstract TKey GetKey(TObj obj);

            public abstract string GetDisplayName(TObj obj);

            public abstract string GetID(TObj obj);
        }
    }
}
