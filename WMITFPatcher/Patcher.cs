using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using BepInEx.Logging;
using Mono.Cecil.Rocks;
using System.Collections;
using System.IO;

namespace WMITFPatcher
{
    public static class Patcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = ["Assembly-CSharp.dll"];

        public static ManualLogSource WLogger = Logger.CreateLogSource("WMITF Patcher");

        public static bool TryLoadWMITFPluginModule(out ModuleDefinition mod)
        {
            var patcherDllPath = typeof(Patcher).Assembly.Location;
            var patchersPath = Path.GetDirectoryName(patcherDllPath);
            var wmitfRootPath = Path.GetDirectoryName(patchersPath);
            var pluginsPath = Path.Combine(wmitfRootPath, "plugins");
            var pluginDllPath = Path.Combine(pluginsPath, "WMITF.dll");

            try
            {
                mod = ModuleDefinition.ReadModule(pluginDllPath);
                return true;
            }
            catch(Exception ex)
            {
                mod = null;
                WLogger.LogError($"Failed to read WMITF plugin dll: {ex}");
                return false;
            }
        }

        public static void Patch(AssemblyDefinition assembly)
        {
            if (!TryLoadWMITFPluginModule(out var wmitfModule))
                return;

            var module = assembly.MainModule;
            var loadedAssetsHandler = module.GetType("LoadedAssetsHandler");
            var achievementManager = module.GetType("AchievementsManagerData");
            var statusFieldDB = module.GetType("StatusFieldDataBase");
            var abilitySO = module.GetType("AbilitySO");

            var assemblyStorage = wmitfModule.GetType("WMITF.ModAssemblyStorage");
            var pluginFinder = wmitfModule.GetType("WMITF.PluginFinder");

            var methods = new Dictionary<MethodDefinition, (string asmbDictName, string pluginDictName, string mthd)>()
            {
                [loadedAssetsHandler.FindMethod("AddExternalCharacter")]        = ("ModdedCharacterAssemblies",     "ModdedCharacterPlugins",       "RegisterID"),
                [loadedAssetsHandler.FindMethod("AddExternalEnemy")]            = ("ModdedEnemyAssemblies",         "ModdedEnemyPlugins",           "RegisterID"),
                [loadedAssetsHandler.FindMethod("TryAddExternalWearable")]      = ("ModdedWearableAssemblies",      "ModdedWearablePlugins",        "RegisterID"),
                [achievementManager.FindMethod("TryAddModdedAchievement")]      = ("ModdedAchievementAssemblies",   "ModdedAchievementPlugins",     "RegisterID_Achievement"),
                [loadedAssetsHandler.FindMethod("AddExternalEnemyAbility")]     = ("ModdedAbilityAssemblies",       "ModdedAbilityPlugins",         "RegisterID"),
                [loadedAssetsHandler.FindMethod("AddExternalCharacterAbility")] = ("ModdedAbilityAssemblies",       "ModdedAbilityPlugins",         "RegisterID"),
                [abilitySO.FindMethod(".ctor")]                                 = ("ModdedAbilitySOAssemblies",     null,                           "RegisterAbilitySO"),
                [statusFieldDB.FindMethod("AddNewStatusEffect")]                = ("ModdedStatusEffectAssemblies",  "ModdedStatusEffectPlugins",    "RegisterID_StatusEffect"),
                [statusFieldDB.FindMethod("AddNewFieldEffect")]                 = ("ModdedFieldEffectAssemblies",   "ModdedFieldEffectPlugins",     "RegisterID_FieldEffect"),
            };

            foreach (var kvp in methods)
            {
                var mthd = kvp.Key;
                var (asmbDictName, pluginDictName, registerMethodName) = kvp.Value;

                var asmbDictField   = string.IsNullOrEmpty(asmbDictName)    ? null : assemblyStorage.FindField(asmbDictName);
                var pluginDictField = string.IsNullOrEmpty(pluginDictName)  ? null : pluginFinder.FindField(pluginDictName);
                var registerMethod  = assemblyStorage.FindMethod(registerMethodName);

                var crs = new ILCursor(new ILContext(mthd));
                while(crs.TryGotoNext(MoveType.After, x => x.MatchRet())) { }
                crs.Goto(crs.Prev, MoveType.Before);

                if(asmbDictField != null)
                    crs.Emit(OpCodes.Ldsfld, module.ImportReference(asmbDictField));
                if(pluginDictField != null)
                    crs.Emit(OpCodes.Ldsfld, module.ImportReference(pluginDictField));
                crs.Emit((mthd.IsStatic || mthd.Name == ".ctor") ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
                crs.Emit(OpCodes.Call, module.ImportReference(registerMethod));
            }

            var combatAbility = module.GetType("CombatAbility");
            var isExtraField = new FieldDefinition("WMITF_isExtraAbility", FieldAttributes.Public, module.ImportReference(typeof(bool)));
            combatAbility.Fields.Add(isExtraField);
        }
    }
}
