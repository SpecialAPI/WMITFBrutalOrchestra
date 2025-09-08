using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace WMITF
{
    [HarmonyPatch]
    public static class ModDisplayStatusEffects
    {
        public static MethodInfo listStatusEffectInfoSOGetItem = AccessTools.Method(typeof(List<StatusEffectInfoSO>), "get_Item");

        public static MethodInfo dm_cet_ssei = AccessTools.Method(typeof(ModDisplayStatusEffects), nameof(DisplayMod_CharacterEnemyTooltip_SaveStatusEffectInfo));
        public static MethodInfo dm_amn = AccessTools.Method(typeof(ModDisplayStatusEffects), nameof(DisplayMod_AddModName));

        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseCharacterStatusTooltip))]
        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseEnemyStatusTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_CharacterEnemyTooltip_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchCallOrCallvirt(listStatusEffectInfoSOGetItem)))
                return;

            var statusEffectInfoLoc = crs.DeclareLocal<StatusEffectInfoSO>();

            crs.Emit(OpCodes.Ldloca, statusEffectInfoLoc);
            crs.Emit(OpCodes.Call, dm_cet_ssei);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringPairData>(nameof(StringPairData.description))))
                return;

            crs.Emit(OpCodes.Ldloc, statusEffectInfoLoc);
            crs.Emit(OpCodes.Call, dm_amn);
        }

        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseInfoStatusTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_InfoTooltip_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringPairData>(nameof(StringPairData.description))))
                return;

            crs.Emit(OpCodes.Ldloc_0);
            crs.Emit(OpCodes.Call, dm_amn);
        }

        public static string DisplayMod_AddModName(string orig, StatusEffectInfoSO info)
        {
            //if (!ModConfig.ShowModsForItems)
            //    return orig;

            if (orig == null)
                return orig;

            if (!PluginFinder.TryGetStatusEffectModName(info, out var modName))
                return orig;

            return $"{orig}\n{modName}";
        }

        public static StatusEffectInfoSO DisplayMod_CharacterEnemyTooltip_SaveStatusEffectInfo(StatusEffectInfoSO curr, out StatusEffectInfoSO save)
        {
            return save = curr;
        }
    }
}
