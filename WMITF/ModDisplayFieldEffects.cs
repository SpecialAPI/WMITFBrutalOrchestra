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
        public static MethodInfo dm_c_amn = AccessTools.Method(typeof(ModDisplayFieldEffects), nameof(DisplayMod_Combat_AddModName));
        public static MethodInfo dm_g_amn = AccessTools.Method(typeof(ModDisplayFieldEffects), nameof(DisplayMod_Glossary_AddModName));

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
            crs.Emit(OpCodes.Call, dm_c_amn);
        }

        [HarmonyPatch(typeof(ExtraInformationUIHandler), nameof(ExtraInformationUIHandler.SetGlossaryInformation), typeof(SlotStatusEffectInfoSO))]
        [HarmonyILManipulator]
        public static void DisplayMod_Glossary_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringPairData>(nameof(StringPairData.description))))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.Emit(OpCodes.Call, dm_g_amn);
        }

        public static string DisplayMod_Glossary_AddModName(string orig, SlotStatusEffectInfoSO info)
        {
            if (!(ModConfig.ShowModsForFieldEffects is StatusFieldDisplayCondition.On or StatusFieldDisplayCondition.OnlyInGlossary))
                return orig;

            if (orig == null)
                return orig;

            if (!PluginFinder.TryGetFieldEffectModName(info, out var modName))
                return orig;

            return $"{orig}\n\n{modName}";
        }

        public static string DisplayMod_Combat_AddModName(string orig, SlotStatusEffectInfoSO info)
        {
            if (ModConfig.ShowModsForFieldEffects != StatusFieldDisplayCondition.On)
                return orig;

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
