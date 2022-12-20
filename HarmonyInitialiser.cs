using HarmonyLib;
using Verse;

namespace AtomicHostileLabels
{
    [StaticConstructorOnStartup]
    public class HarmonyInitialiser
    {
        static HarmonyInitialiser()
        {
            Harmony harmony = new Harmony("momo.atomic.hostilelabels");
            harmony.PatchAll();
        }
    }
}