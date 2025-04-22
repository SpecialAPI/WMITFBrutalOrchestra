using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace WMITF
{
    [HarmonyPatch]
    public static class ModDisplayEnemies
    {
        public static readonly MethodInfo dm_amn = AccessTools.Method(typeof(ModDisplayEnemies), nameof(DisplayMod_AddModName));

        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseEnemyTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpBeforeNext(x => x.MatchStloc(2)))
                return;

            crs.Emit(OpCodes.Ldloc_0);
            crs.Emit(OpCodes.Call, dm_amn);
        }

        public static string DisplayMod_AddModName(string orig, EnemyCombatUIInfo inf)
        {
            if (!ModConfig.ShowModsForEnemies)
                return orig;

            if (orig == null || inf == null)
                return orig;

            if(!PluginFinder.TryGetEnemyModName(inf.EnemyBase, out var modName))
                return orig;

            return $"{orig}<size=34px>{modName}</size>";
        }
    }
}
