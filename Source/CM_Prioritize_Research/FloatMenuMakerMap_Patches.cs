using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Prioritize_Research
{
    [StaticConstructorOnStartup]
    public static class FloatMenuMakerMap_Patches
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap))]
        [HarmonyPatch("AddJobGiverWorkOrders", MethodType.Normal)]
        public static class FloatMenuMakerMap_AddJobGiverWorkOrders
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                FieldInfo jobDefFieldInfo = typeof(Verse.AI.Job).GetField("def");
                FieldInfo jobDefOfResearchFieldInfo = typeof(JobDefOf).GetField("Research");

                List<CodeInstruction> instructionList = instructions.ToList();

                // Find where we check if the job is research. Only happens in one place if it hasn't been changed
                int index = instructionList.FindIndex(instruction => instruction.LoadsField(jobDefOfResearchFieldInfo));
                if (index >= 2 && index < instructionList.Count - 5)
                {
                    // Verify everything we are replacing to make sure this hasn't already been tampered with
                    if (instructionList[index - 2].IsLdloc() && 
                        instructionList[index - 1].LoadsField(jobDefFieldInfo) &&
                        instructionList[index + 2].IsLdloc() &&
                        instructionList[index + 3].operand is Type && (Type)instructionList[index + 3].operand == typeof(RimWorld.Building_ResearchBench) &&
                        instructionList[index + 4].Branches(out Label? branchLabel2))
                    {
                        CodeInstruction jump = instructionList[index + 1];

                        if (jump.Branches(out Label? branchLabel))
                        {
                            Log.Message("[CM_Prioritize_Research] - patching to allow research prioritizing.");
                            jump.opcode = OpCodes.Br;
                        }
                    }
                }


                foreach (CodeInstruction instruction in instructionList)
                {
                    yield return instruction;
                }
            }
        }
    }
}
