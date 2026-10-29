using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
using MelonLoader;
using ModComponent.Utils;
using UnityEngine;
using System.Collections.Generic;

namespace CanneryManufacturing
{
    internal static class Patches
    {
        private static readonly Dictionary<string, System.Action<GearItem>> GearRuinedActions = new Dictionary<string, System.Action<GearItem>>()
        {
            { "GEAR_FlareGun", GearItem_Awake.ForceWornOutForFirearms },
            { "GEAR_Revolver", GearItem_Awake.ForceWornOutForFirearms },
            { "GEAR_RevolverFancy", GearItem_Awake.ForceWornOutForFirearms },
            { "GEAR_RevolverGreen", GearItem_Awake.ForceWornOutForFirearms },
            { "GEAR_RevolverStubNosed", GearItem_Awake.ForceWornOutForFirearms },
            { "GEAR_Rifle", GearItem_Awake.ForceWornOutForFirearms },
            { "GEAR_Rifle_Vaughns", GearItem_Awake.ForceWornOutForFirearms },
            { "GEAR_Rifle_Barbs", GearItem_Awake.ForceWornOutForFirearms },
            { "GEAR_Rifle_Curators", GearItem_Awake.ForceWornOutForFirearms }
        };

        [HarmonyPatch(typeof(Panel_Crafting), "Enable", new Type[] { typeof(bool), typeof(bool) })]
        private static class WorkbenchLocationPatch
        {
            internal static void Postfix(ref Panel_Crafting __instance, bool enable, bool fromPanel)
            {
                foreach (BlueprintData singlePrint in __instance.m_Blueprints)
                {
                    if (singlePrint.name == "BLUEPRINT_GEAR_GunpowderCan_A")
                    {
                        singlePrint.m_RequiredCraftingLocation = (CraftingLocation)Settings.options.gunpowderLocationIndex; // [("Anywhere", "Workbench", "Forge", "Ammo Workbench")]
                    }
                }

                __instance.RefreshAvailableBlueprints();
            }
        }

        //This patch handles ruining firearms on start
        [HarmonyPatch(typeof(GearItem), "Awake")]
        private static class GearItem_Awake
        {
            private const string SCRAP_METAL_NAME = "GEAR_ScrapMetal";

            internal static void Postfix(GearItem __instance)
            {
                string normalizedName = Utils.NormalizeName(__instance.name);

                // Handle special items (e.g., crampons, flaregun)
                if (normalizedName == "GEAR_Crampons")
                {
                    SetupMillable(__instance, 210, 30, 1, 4);
                }
                else if (normalizedName == "GEAR_Flaregun")
                {
                    SetupMillable(__instance, 180, 30, 1, 3);
                }
                else if (__instance.m_BeenInspected)
                {
                    return;
                }
                else if (GearRuinedActions.ContainsKey(normalizedName))
                {
                    if (ShouldForceWornOut(normalizedName))
                    {
                        GearRuinedActions[normalizedName].Invoke(__instance);
                    }
                }
            }

            private static void SetupMillable(GearItem gearItem, int recoveryDuration, int repairDuration, int repairUnits, int restoreUnits)
            {
                gearItem.m_Millable = Utils.GetOrCreateComponent<Millable>(gearItem.gameObject);
                gearItem.m_Millable.m_CanRestoreFromWornOut = true;
                gearItem.m_Millable.m_RecoveryDurationMinutes = recoveryDuration;
                gearItem.m_Millable.m_RepairDurationMinutes = repairDuration;
                gearItem.m_Millable.m_RepairRequiredGear = new GearItem[] { GetGearItemPrefab(SCRAP_METAL_NAME) };
                gearItem.m_Millable.m_RepairRequiredGearUnits = new int[] { repairUnits };
                gearItem.m_Millable.m_RestoreRequiredGear = new GearItem[] { GetGearItemPrefab(SCRAP_METAL_NAME) };
                gearItem.m_Millable.m_RestoreRequiredGearUnits = new int[] { restoreUnits };
                gearItem.m_Millable.m_Skill = SkillType.None;
            }

            private static bool ShouldForceWornOut(string normalizedName)
            {
                return (Settings.options.flareGunsStartRuined && normalizedName == "GEAR_FlareGun") ||
                       (Settings.options.revolversStartRuined && normalizedName.StartsWith("GEAR_Revolver")) ||
                       (Settings.options.riflesStartRuined && normalizedName.StartsWith("GEAR_Rifle"));
            }

            internal static void ForceWornOutForFirearms(GearItem gearItem)
            {
                gearItem.ForceWornOut();
            }

            private static GearItem GetGearItemPrefab(string name) =>
                Utils.LoadGearItemPrefab(name) ?? throw new InvalidOperationException($"Unable to load gear item prefab '{name}'.");
        }
    }
}