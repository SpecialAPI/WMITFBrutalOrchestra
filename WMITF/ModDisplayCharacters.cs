using System;
using System.Collections;
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
    public static class ModDisplayCharacters
    {
        public static readonly MethodInfo dm_ct_amn = AccessTools.Method(typeof(ModDisplayCharacters), nameof(DisplayMod_CombatTooltip_AddModName));
        public static readonly MethodInfo dm_ow_amn = AccessTools.Method(typeof(ModDisplayCharacters), nameof(DisplayMod_Overworld_AddModName));
        public static readonly MethodInfo dm_mc_amn = AccessTools.Method(typeof(ModDisplayCharacters), nameof(DisplayMod_MinimalCharacter_AddModName));

        [HarmonyPatch(typeof(CombatVisualizationController), nameof(CombatVisualizationController.ShowcaseCharacterTooltip))]
        [HarmonyILManipulator]
        public static void DisplayMod_CombatTooltip_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpBeforeNext(x => x.MatchStloc(2)))
                return;

            crs.Emit(OpCodes.Ldloc_0);
            crs.Emit(OpCodes.Call, dm_ct_amn);
        }

        [HarmonyPatch(typeof(SelectableCharacterInformationLayout), nameof(SelectableCharacterInformationLayout.SetInformation))]
        [HarmonyPatch(typeof(CharacterUILayout), nameof(CharacterUILayout.SetInformation))]
        [HarmonyILManipulator]
        public static void DisplayMod_OverworldSetInfoCharacterSO_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchCallOrCallvirt<CharacterSO>(nameof(CharacterSO.GetName))))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.Emit(OpCodes.Call, dm_ow_amn);
        }

        //[HarmonyPatch(typeof(InteractablePartyCharacterUILayout), nameof(InteractablePartyCharacterUILayout.SetInformation))]
        [HarmonyPatch(typeof(PartyCharacterUILayout), nameof(PartyCharacterUILayout.SetInformation), typeof(IMinimalCharacterInfo), typeof(bool), typeof(bool), typeof(bool))]
        [HarmonyILManipulator]
        public static void DisplayMod_OverworldSetInfoMinimalCharacter_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchCallOrCallvirt<CharacterSO>(nameof(CharacterSO.GetName))))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.Emit(OpCodes.Call, dm_mc_amn);
        }

        [HarmonyPatch(typeof(SelectableCharacterInformationLayout), nameof(SelectableCharacterInformationLayout.SetInformation))]
        [HarmonyPostfix]
        public static void ExpandMenuName(SelectableCharacterInformationLayout __instance, CharacterSO character)
        {
            var txt = __instance._name;

            if (txt == null || txt.rectTransform == null)
                return;

            var parent = txt.rectTransform.parent;

            if (parent == null || parent is not RectTransform rectParent)
                return;

            rectParent.sizeDelta = Vector2.zero;

            if (character == null)
                return;

            var id = character.name;

            if (string.IsNullOrEmpty(id) || !PluginFinder.ModdedCharacterPlugins.ContainsKey(id))
                return;

            rectParent.sizeDelta = Vector2.up * 20f;
        }

        public static string DisplayMod_MinimalCharacter_AddModName(string orig, IMinimalCharacterInfo inf)
        {
            return DisplayMod_Overworld_AddModName(orig, inf.Character);
        }

        public static string DisplayMod_Overworld_AddModName(string orig, CharacterSO ch)
        {
            if (!ModConfig.ShowModsForCharacters)
                return orig;

            if (orig == null)
                return orig;

            if(!PluginFinder.TryGetCharacterModName(ch, out var modName))
                return orig;

            return $"{orig}\n{modName}";
        }

        public static string DisplayMod_CombatTooltip_AddModName(string orig, CharacterCombatUIInfo inf)
        {
            if (!ModConfig.ShowModsForCharacters)
                return orig;

            if (orig == null || inf == null)
                return orig;

            if (!PluginFinder.TryGetCharacterModName(inf.CharacterBase, out var modName))
                return orig;

            return $"{orig}<size=34px>{modName}</size>";
        }
    }
}
