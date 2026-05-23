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

        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseInfoAttackTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            var combatAbLocal = crs.DeclareLocal<CombatAbility>();

            foreach(var m in crs.MatchAfter(x => x.MatchCallOrCallvirt(listCombatAbilityGetItem)))
            {
                crs.Emit(OpCodes.Ldloca, combatAbLocal);
                crs.EmitStaticDelegate(DisplayMod_SaveCombatAbility);
            }

            if (!crs.JumpBeforeNext(x => x.MatchCallOrCallvirt<TooltipLayout>(nameof(TooltipLayout.DelayShow))))
                return;

            crs.Emit(OpCodes.Ldloc_0);
            crs.Emit(OpCodes.Ldloc, combatAbLocal);
            crs.EmitStaticDelegate(DisplayMod_AddModName);
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
