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
        public static readonly MethodInfo listCombatAbilityGetItem = AccessTools.Method(typeof(List<CombatAbility>), "get_Item");

        public static readonly MethodInfo dm_sca = AccessTools.Method(typeof(ModDisplayAbilities), nameof(DisplayMod_SaveCombatAbility));
        public static readonly MethodInfo dm_amn = AccessTools.Method(typeof(ModDisplayAbilities), nameof(DisplayMod_AddModName));

        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseInfoAttackTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            var combatAbLocal = crs.DeclareLocal<CombatAbility>();

            foreach(var m in crs.MatchAfter(x => x.MatchCallOrCallvirt(listCombatAbilityGetItem)))
            {
                crs.Emit(OpCodes.Ldloca, combatAbLocal);
                crs.Emit(OpCodes.Call, dm_sca);
            }

            if (!crs.JumpBeforeNext(x => x.MatchCallOrCallvirt<TooltipLayout>(nameof(TooltipLayout.DelayShow))))
                return;

            crs.Emit(OpCodes.Ldloc_0);
            crs.Emit(OpCodes.Ldloc, combatAbLocal);
            crs.Emit(OpCodes.Call, dm_amn);
        }

        public static CombatAbility DisplayMod_SaveCombatAbility(CombatAbility curr, out CombatAbility s)
        {
            return s = curr;
        }

        public static string DisplayMod_AddModName(string orig, AbilitySO ab, CombatAbility combatAb)
        {
            if (!(ModConfig.ShowModsForAbilities switch
            {
                AbilityModDisplayCondition.Off => false,
                AbilityModDisplayCondition.OnlyForExtraAbilities => combatAb != null && combatAb.IsFromExtraAbility(),
                AbilityModDisplayCondition.On => true,

                _ => false
            }))
                return orig;

            if (orig == null || ab == null)
                return orig;

            if (!PluginFinder.TryGetAbilityModName(ab, out var modName))
                return orig;

            return $"{orig}<size=34px>{modName}</size>";
        }
    }
}
