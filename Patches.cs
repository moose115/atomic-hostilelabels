using HarmonyLib;
using RimWorld;
using Verse;

namespace AtomicHostileLabels
{
    [HarmonyPatch]
    public class PawnUIOverlay_DrawPawnGUIOverlay_Patches
    {
        [HarmonyPatch(typeof(PawnUIOverlay), nameof(PawnUIOverlay.DrawPawnGUIOverlay))]
        [HarmonyPrefix]
        [HarmonyPriority(20000)]
        public static bool Prefix(ref Pawn ___pawn)
        {
            if (___pawn.RaceProps.Humanlike || !___pawn.HostileTo(Faction.OfPlayer))
                return true;
            GenMapUI2.DrawPawnLabel(___pawn, GenMapUI.LabelDrawPosFor((Thing) ___pawn, -0.6f));
            return false;
        }
    }
}