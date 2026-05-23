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
            crs.EmitStaticDelegate(DisplayMod_CharacterEnemyTooltip_SaveStatusEffectInfo);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringPairData>(nameof(StringPairData.description))))
                return;

            crs.Emit(OpCodes.Ldloc, slotStatusEffectInfoLoc);
            crs.EmitStaticDelegate(DisplayMod_Combat_AddModName);
        }

        public static SlotStatusEffectInfoSO DisplayMod_CharacterEnemyTooltip_SaveStatusEffectInfo(SlotStatusEffectInfoSO curr, out SlotStatusEffectInfoSO save)
        {
            return save = curr;
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

        [HarmonyPatch(typeof(ExtraInformationUIHandler), nameof(ExtraInformationUIHandler.SetGlossaryInformation), typeof(SlotStatusEffectInfoSO))]
        [HarmonyILManipulator]
        public static void DisplayMod_Glossary_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchLdfld<StringPairData>(nameof(StringPairData.description))))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.EmitStaticDelegate(DisplayMod_Glossary_AddModName);
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
    }
}
