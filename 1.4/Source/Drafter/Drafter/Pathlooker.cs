using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using HarmonyLib;

namespace Drafter
{
    public class Pathlooker : Mod
    {
        public Pathlooker(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("Drafter");
            harmony.Patch(AccessTools.Method(typeof(Pawn_RotationTracker), nameof(Pawn_RotationTracker.UpdateRotation)), null, null, new HarmonyMethod(typeof(Pawn_RotationTracker_UpdateRotation), "Transpiler"));
        }

        public static class Pawn_RotationTracker_UpdateRotation
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> instructionList = new List<CodeInstruction>(instructions);

                for (var i = 0; i < instructionList.Count; i++)
                {
                    if (instructionList[i].opcode == OpCodes.Ldarg_0)
                    {
                        if (instructionList[i + 1].opcode == OpCodes.Ldfld)
                        {
                            if (instructionList[i + 2].opcode == OpCodes.Callvirt)
                            {
                                MethodInfo draftedInfo = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Drafted));
                                MethodInfo instructionInfo = instructionList[i + 2].operand as MethodInfo;

                                if (instructionInfo != null && instructionInfo == draftedInfo)
                                {
                                    var ret = new CodeInstruction(OpCodes.Ret);
                                    ret.labels.Add(instructionList[i].labels[0]);
                                    yield return ret;
                                    yield break;
                                }
                            }
                        }
                    }
                    yield return new CodeInstruction(instructionList[i]);
                }
            }
        }
    }
}
