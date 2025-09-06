using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WMITF
{
    public static class ModAssemblyStorage
    {
        public static readonly string[] IgnoredAssemblies =
        [
            // basegame dll
            "Assembly-CSharp",

            // apis
            "BrutalAPI",
            "Pentacle",

            // wmitf itself (just in case)
            "WMITFPatcher",
            "WMITF",

            // thanks unity
            "UnityEngine.CoreModule"
        ];
        public static readonly MethodInfo ScriptableCreateInstanceGeneric = AccessTools.Method(typeof(ScriptableObject), nameof(ScriptableObject.CreateInstance));

        public static readonly Dictionary<string, Assembly> ModdedCharacterAssemblies = [];
        public static readonly Dictionary<string, Assembly> ModdedEnemyAssemblies = [];
        public static readonly Dictionary<string, Assembly> ModdedWearableAssemblies = [];
        public static readonly Dictionary<string, Assembly> ModdedAchievementAssemblies = [];
        public static readonly Dictionary<string, Assembly> ModdedAbilityAssemblies = [];

        public static readonly Dictionary<AbilitySO, Assembly> ModdedAbilitySOAssemblies = [];

        public static void RegisterAbilitySO(Dictionary<AbilitySO, Assembly> dict, AbilitySO ab)
        {
            var asmbl = GetModAssemblyFromStackTraceOnlyCreateScriptableInstance();

            if (asmbl == null || dict == null)
                return;

            dict[ab] = asmbl;
        }

        public static void RegisterID_Achievement(Dictionary<string, Assembly> dict, ModdedAchievement_t ach)
        {
            RegisterID(dict, ach.m_eAchievementID);
        }

        public static void RegisterID(Dictionary<string, Assembly> dict, string id)
        {
            var asmbl = GetModAssemblyFromStackTrace();

            if (asmbl == null || dict == null)
                return;

            dict[id] = asmbl;
        }

        public static Assembly GetModAssemblyFromStackTraceOnlyCreateScriptableInstance()
        {
            var st = new StackTrace();
            var frames = st.GetFrames();
            var foundCreateScriptable = false;

            foreach (var frame in frames)
            {
                if (frame == null)
                    continue;

                if (frame.GetMethod() is not MethodInfo mthd)
                    continue;

                if (!foundCreateScriptable)
                {
                    if(mthd.IsGenericMethod && mthd.GetGenericMethodDefinition() == ScriptableCreateInstanceGeneric)
                        foundCreateScriptable = true;

                    continue;
                }

                if (mthd.DeclaringType is not Type decType || decType.Assembly is not Assembly asmbl)
                    continue;

                if (asmbl.GetName() is not AssemblyName asmblName || asmblName.Name is not string name)
                    continue;

                if (string.IsNullOrEmpty(name) || Array.IndexOf(IgnoredAssemblies, name) >= 0)
                    continue;

                return asmbl;
            }

            return null;
        }

        public static Assembly GetModAssemblyFromStackTrace()
        {
            var st = new StackTrace();
            var frames = st.GetFrames();

            foreach (var frame in frames)
            {
                if (frame == null)
                    continue;

                if (frame.GetMethod() is not MethodInfo mthd)
                    continue;

                if (mthd.DeclaringType is not Type decType || decType.Assembly is not Assembly asmbl)
                    continue;

                if (asmbl.GetName() is not AssemblyName asmblName || asmblName.Name is not string name)
                    continue;

                if (string.IsNullOrEmpty(name) || Array.IndexOf(IgnoredAssemblies, name) >= 0)
                    continue;

                return asmbl;
            }

            return null;
        }
    }
}
