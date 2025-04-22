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

            var methods = new Dictionary<MethodDefinition, string>()
            {
                [loadedAssetsHandler.FindMethod("AddExternalCharacter")]    = "WMITF_ModdedCharacters",
                [loadedAssetsHandler.FindMethod("AddExternalEnemy")]        = "WMITF_ModdedEnemies",
                [loadedAssetsHandler.FindMethod("TryAddExternalWearable")]  = "WMITF_ModdedWearables"
            };

            foreach (var kvp in methods)
            {
                var mthd = kvp.Key;
                var registerDictName = kvp.Value;

                var dictField = new FieldDefinition(registerDictName, FieldAttributes.Public | FieldAttributes.Static, module.ImportReference(typeof(Dictionary<string, Assembly>)));
                loadedAssetsHandler.Fields.Add(dictField);

                var crs = new ILCursor(new ILContext(mthd));
                while(crs.TryGotoNext(MoveType.After, x => x.MatchRet())) { }
                crs.Goto(crs.Prev, MoveType.Before);
                crs.Emit(OpCodes.Ldarg_0);
                crs.Emit(OpCodes.Ldsflda, dictField);
                crs.Emit(OpCodes.Call, AccessTools.Method(typeof(Patcher), nameof(RegisterID)));
            }
        }

        public static void RegisterID(string id, ref Dictionary<string, Assembly> dict)
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
