using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WMITF
{
    [HarmonyPatch]
    public static class ModDisplayAbilities
    {
        public static readonly MethodInfo dm_amn = AccessTools.Method(typeof(ModDisplayAbilities), nameof(DisplayMod_AddModName));

        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseInfoAttackTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpBeforeNext(x => x.MatchCallOrCallvirt<TooltipLayout>(nameof(TooltipLayout.DelayShow))))
                return;

            crs.Emit(OpCodes.Ldloc_0);
            crs.Emit(OpCodes.Call, dm_amn);
        }

        public static string DisplayMod_AddModName(string orig, AbilitySO ab)
        {
            if (!ModConfig.ShowModsForAbilities)
                return orig;

            if (orig == null || ab == null)
                return orig;

            if (!PluginFinder.TryGetAbilityModName(ab, out var modName))
                return orig;

            return $"{orig}<size=34px>{modName}</size>";
        }
    }
}
