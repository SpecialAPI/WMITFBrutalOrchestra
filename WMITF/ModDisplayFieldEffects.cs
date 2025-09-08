using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace WMITF
{
    [HarmonyPatch]
    public static class ModDisplayFieldEffects
    {
        public static MethodInfo listSlotStatusEffectInfoSOGetItem = AccessTools.Method(typeof(List<SlotStatusEffectInfoSO>), "get_Item");

        public static MethodInfo dm_cet_ssei = AccessTools.Method(typeof(ModDisplayFieldEffects), nameof(DisplayMod_CharacterEnemyTooltip_SaveStatusEffectInfo));
        public static MethodInfo dm_amn = AccessTools.Method(typeof(ModDisplayFieldEffects), nameof(DisplayMod_AddModName));

        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseCharacterFieldTooltip))]
        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseEnemyFieldTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_CharacterEnemyTooltip_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchCallOrCallvirt(listSlotStatusEffectInfoSOGetItem)))
                return;

            var slotStatusEffectInfoLoc = crs.DeclareLocal<SlotStatusEffectInfoSO>();

            crs.Emit(OpCodes.Ldloca, slotStatusEffectInfoLoc);
            crs.Emit(OpCodes.Call, dm_cet_ssei);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringPairData>(nameof(StringPairData.description))))
                return;

            crs.Emit(OpCodes.Ldloc, slotStatusEffectInfoLoc);
            crs.Emit(OpCodes.Call, dm_amn);
        }

        public static string DisplayMod_AddModName(string orig, SlotStatusEffectInfoSO info)
        {
            //if (!ModConfig.ShowModsForItems)
            //    return orig;

            if (orig == null)
                return orig;

            if (!PluginFinder.TryGetFieldEffectModName(info, out var modName))
                return orig;

            return $"{orig}\n{modName}";
        }

        public static SlotStatusEffectInfoSO DisplayMod_CharacterEnemyTooltip_SaveStatusEffectInfo(SlotStatusEffectInfoSO curr, out SlotStatusEffectInfoSO save)
        {
            return save = curr;
        }
    }
}
