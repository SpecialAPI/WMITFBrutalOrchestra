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

            var methods = new Dictionary<MethodDefinition, (string, string)>()
            {
                [loadedAssetsHandler.FindMethod("AddExternalCharacter")]        = ("ModdedCharacterAssemblies",     "RegisterID"),
                [loadedAssetsHandler.FindMethod("AddExternalEnemy")]            = ("ModdedEnemyAssemblies",         "RegisterID"),
                [loadedAssetsHandler.FindMethod("TryAddExternalWearable")]      = ("ModdedWearableAssemblies",      "RegisterID"),
                [achievementManager.FindMethod("TryAddModdedAchievement")]      = ("ModdedAchievementAssemblies",   "RegisterID_Achievement"),
                [loadedAssetsHandler.FindMethod("AddExternalEnemyAbility")]     = ("ModdedAbilityAssemblies",       "RegisterID"),
                [loadedAssetsHandler.FindMethod("AddExternalCharacterAbility")] = ("ModdedAbilityAssemblies",       "RegisterID"),
                [abilitySO.FindMethod(".ctor")]                                 = ("ModdedAbilitySOAssemblies",     "RegisterAbilitySO"),
                [statusFieldDB.FindMethod("AddNewStatusEffect")]                = ("ModdedStatusEffectAssemblies",  "RegisterID_StatusEffect"),
                [statusFieldDB.FindMethod("AddNewFieldEffect")]                 = ("ModdedFieldEffectAssemblies",   "RegisterID_FieldEffect"),
            };

            foreach (var kvp in methods)
            {
                var mthd = kvp.Key;
                var (dictName, registerMethodName) = kvp.Value;

                var dictField = assemblyStorage.FindField(dictName);
                var registerMethod = assemblyStorage.FindMethod(registerMethodName);

                var crs = new ILCursor(new ILContext(mthd));
                while(crs.TryGotoNext(MoveType.After, x => x.MatchRet())) { }
                crs.Goto(crs.Prev, MoveType.Before);

                crs.Emit(OpCodes.Ldsfld, module.ImportReference(dictField));
                crs.Emit((mthd.IsStatic || mthd.Name == ".ctor") ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
                crs.Emit(OpCodes.Call, module.ImportReference(registerMethod));
            }

            var combatAbility = module.GetType("CombatAbility");
            var isExtraField = new FieldDefinition("WMITF_isExtraAbility", FieldAttributes.Public, module.ImportReference(typeof(bool)));
            combatAbility.Fields.Add(isExtraField);
        }
    }
}
