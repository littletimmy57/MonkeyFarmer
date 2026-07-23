using HarmonyLib;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using System.Collections.Generic;

namespace Gardener.Patches;

[HarmonyPatch(typeof(Projectile), nameof(Projectile.CollideBloon))]
internal static class CropStandCashPatch
{
    private const string CropStandProjectileId = "CropStand";
    private const string FruitVarietyCropStandProjectileId = "CropStandFruitVariety";
    private const string FarmersMarketCropStandProjectileId = "CropStandFarmersMarket";
    private const string SecondFarmersMarketCropStandProjectileId = "CropStandFarmersMarketSecond";
    private const string HarvestMarketCropStandProjectileId = "CropStandHarvestMarket";
    private const string SecondHarvestMarketCropStandProjectileId = "CropStandHarvestMarketSecond";
    private const string GoldenOrchardCropStandProjectileId = "CropStandGoldenOrchard";
    private const string SecondGoldenOrchardCropStandProjectileId = "CropStandGoldenOrchardSecond";
    private const double CashPerBloonTouch = 10;
    private const double FruitVarietyCashPerBloonTouch = 20;
    private const double FarmersMarketCashPerBloonTouch = 30;
    private const double HarvestMarketCashPerBloonTouch = 45;
    private const double GoldenOrchardCashPerBloonLayer = 100;
    private static readonly HashSet<string> PaidTouches = new();

    [HarmonyPostfix]
    private static void Postfix(Projectile __instance, Bloon bloon, bool __result)
    {
        if (!__result || __instance?.projectileModel == null)
        {
            return;
        }

        var cash = __instance.projectileModel.id switch
        {
            CropStandProjectileId => CashPerBloonTouch,
            FruitVarietyCropStandProjectileId => FruitVarietyCashPerBloonTouch,
            FarmersMarketCropStandProjectileId => FarmersMarketCashPerBloonTouch,
            SecondFarmersMarketCropStandProjectileId => FarmersMarketCashPerBloonTouch,
            HarvestMarketCropStandProjectileId => HarvestMarketCashPerBloonTouch,
            SecondHarvestMarketCropStandProjectileId => HarvestMarketCashPerBloonTouch,
            GoldenOrchardCropStandProjectileId => GetGoldenOrchardCash(bloon),
            SecondGoldenOrchardCropStandProjectileId => GetGoldenOrchardCash(bloon),
            _ => 0
        };

        if (cash <= 0)
        {
            return;
        }

        var touchId = $"{__instance.Id}_{bloon.Id}";
        if (!PaidTouches.Add(touchId))
        {
            return;
        }

        __instance.Sim.AddCash(
            cash,
            Simulation.CashType.Ability,
            __instance.owner,
            Simulation.CashSource.Normal,
            __instance.EmittedBy,
            false);
    }

    public static void Reset()
    {
        PaidTouches.Clear();
    }

    private static double GetGoldenOrchardCash(Bloon bloon)
    {
        var layers = 1.0;

        try
        {
            if (bloon?.bloonModel != null && bloon.bloonModel.leakDamage > 0)
            {
                layers = bloon.bloonModel.leakDamage;
            }
        }
        catch
        {
            layers = 1.0;
        }

        return layers * GoldenOrchardCashPerBloonLayer;
    }
}
