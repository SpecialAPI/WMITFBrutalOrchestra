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
using UnityEngine;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using Debug = UnityEngine.Debug;

namespace WMITFPatcher
{
    public static class Patcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = ["Assembly-CSharp.dll"];

        public static string[] IgnoredAssemblies =
        [
            // basegame dll
            "Assembly-CSharp",

            // apis
            "BrutalAPI",
            "Pentacle",

            // wmitf itself (just in case)
            "WMITFPatcher",
            "WMITF"
        ];

        public static void Patch(AssemblyDefinition assembly)
        {
            var module = assembly.MainModule;
            var loadedAssetsHandler = module.GetType("LoadedAssetsHandler");
            var achievementManager = module.GetType("AchievementsManagerData");

            var methods = new Dictionary<MethodDefinition, string>()
            {
                [loadedAssetsHandler.FindMethod("AddExternalCharacter")]        = "WMITF_ModdedCharacters",
                [loadedAssetsHandler.FindMethod("AddExternalEnemy")]            = "WMITF_ModdedEnemies",
                [loadedAssetsHandler.FindMethod("TryAddExternalWearable")]      = "WMITF_ModdedWearables",
                [loadedAssetsHandler.FindMethod("AddExternalEnemyAbility")]     = "WMITF_ModdedAbilities",
                [loadedAssetsHandler.FindMethod("AddExternalCharacterAbility")] = "WMITF_ModdedAbilities",
                [achievementManager.FindMethod("TryAddModdedAchievement")]      = "WMITF_ModdedAchievements"
            };
            var addedFields = new Dictionary<string, FieldDefinition>();

            var registerId = AccessTools.Method(typeof(Patcher), nameof(RegisterID));

            foreach (var kvp in methods)
            {
                var mthd = kvp.Key;
                var registerDictName = kvp.Value;

                if (!addedFields.TryGetValue(registerDictName, out var dictField))
                {
                    dictField = new FieldDefinition(registerDictName, FieldAttributes.Public | FieldAttributes.Static, module.ImportReference(typeof(Dictionary<string, Assembly>)));
                    loadedAssetsHandler.Fields.Add(addedFields[registerDictName] = dictField);
                }

                var crs = new ILCursor(new ILContext(mthd));
                while(crs.TryGotoNext(MoveType.After, x => x.MatchRet())) { }
                crs.Goto(crs.Prev, MoveType.Before);
                crs.Emit(OpCodes.Ldsflda, dictField);

                if (mthd.Name == "TryAddModdedAchievement")
                {
                    crs.Emit(OpCodes.Ldarg_1);
                    crs.Emit(OpCodes.Call, AccessTools.Method(typeof(Patcher), nameof(RegisterID_Achievement)));
                }
                else
                {
                    crs.Emit(OpCodes.Ldarg_0);
                    crs.Emit(OpCodes.Call, registerId);
                }
            }
        }

        // EXTREMELY JANK SOLUTION, need to figure out how to make this better later
        public static void RegisterID_Achievement(ref Dictionary<string, Assembly> dict, object moddedAch)
        {
            if (moddedAch == null)
                return;

            var idField = AccessTools.Field(moddedAch.GetType(), "m_eAchievementID");

            if(idField == null || idField.GetValue(moddedAch) is not string id)
                return;

            RegisterID(ref dict, id);
        }

        public static void RegisterID(ref Dictionary<string, Assembly> dict, string id)
        {
            var asmbl = GetPluginInfoFromStackTrace();

            if (asmbl == null)
                return;

            dict ??= [];
            dict[id] = asmbl;
        }

        public static Assembly GetPluginInfoFromStackTrace()
        {
            var st = new StackTrace();
            var frames = st.GetFrames();

            foreach (var frame in frames)
            {
                if (frame == null)
                    continue;

                if(frame.GetMethod() is not MethodInfo mthd)
                    continue;

                if (mthd.DeclaringType is not Type decType || decType.Assembly is not Assembly asmbl)
                    continue;

                if(asmbl.GetName() is not AssemblyName asmblName || asmblName.Name is not string name)
                    continue;

                if(string.IsNullOrEmpty(name) || Array.IndexOf(IgnoredAssemblies, name) >= 0)
                    continue;

                return asmbl;
            }

            return null;
        }
    }
}
