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
    public static class IsExtraAbilityPatch
    {
        public static MethodInfo siea_aea_s = AccessTools.Method(typeof(IsExtraAbilityPatch), nameof(SetIsExtraAbility_AddExtraAbility_Set));

        public static FieldInfo isExtraAbilityField = AccessTools.Field(typeof(CombatAbility), "WMITF_isExtraAbility");

        [HarmonyPatch(typeof(CharacterCombat), nameof(CharacterCombat.AddExtraAbility))]
        [HarmonyPatch(typeof(EnemyCombat), nameof(EnemyCombat.AddExtraAbility))]
        [HarmonyILManipulator]
        public static void SetIsExtraAbility_AddExtraAbility_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchStloc(0)))
                return;

            crs.Emit(OpCodes.Ldloc_0);
            crs.Emit(OpCodes.Call, siea_aea_s);
        }

        [HarmonyPatch(typeof(CombatAbility), MethodType.Constructor, typeof(CombatAbility))]
        [HarmonyPostfix]
        public static void SetIsExtraAbility_Constructor_Postfix(CombatAbility __instance, CombatAbility toCopyAbility)
        {
            if (isExtraAbilityField == null)
            {
                Debug.LogError("WMITF_isExtraAbility field is null (SetIsExtraAbility_Constructor_Postfix). This should not be happening.");
                return;
            }

            var origIsExtra = isExtraAbilityField.GetValue(toCopyAbility);
            isExtraAbilityField.SetValue(__instance, origIsExtra);
        }

        public static void SetIsExtraAbility_AddExtraAbility_Set(CombatAbility ab)
        {
            if(isExtraAbilityField == null)
            {
                Debug.LogError("WMITF_isExtraAbility field is null (SetIsExtraAbility_AddExtraAbility_Set). This should not be happening.");
                return;
            }

            isExtraAbilityField.SetValue(ab, true);
        }

        public static bool IsFromExtraAbility(this CombatAbility ab)
        {
            if (isExtraAbilityField == null)
            {
                Debug.LogError("WMITF_isExtraAbility field is null (IsFromExtraAbility). This should not be happening.");
                return false;
            }

            return (bool)isExtraAbilityField.GetValue(ab);
        }
    }
}
