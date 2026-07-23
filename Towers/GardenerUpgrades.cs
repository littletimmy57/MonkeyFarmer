using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using BTD_Mod_Helper;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Models.Towers.Filters;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;
using Il2CppAssets.Scripts.Unity;
using System;

namespace Gardener.Towers;

public class SharperSeeds : ModUpgrade<GardenerMonkey>
{
    public override int Path => TOP;
    public override int Tier => 1;
    public override int Cost => 250;
    public override string DisplayName => "Seeds";
    public override string Description => "Throws seeds.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var dartMonkey = Game.instance.model.GetTowerFromId(TowerType.DartMonkey + "-001");
        towerModel.range = dartMonkey.range;

        var attack = towerModel.GetAttackModel();
        attack.range = towerModel.range;
        attack.weapons[0].projectile.pierce += 2;
        attack.weapons[0].projectile.GetDamageModel().damage += 1;
        TopPathEffects.ApplySeedProjectileDisplay(attack.weapons[0].projectile);
    }
}

public class ThornPatch : ModUpgrade<GardenerMonkey>
{
    public override int Path => TOP;
    public override int Tier => 2;
    public override int Cost => 550;
    public override string DisplayName => "Seedspread";
    public override string Description => "Spreads the seeds.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        towerModel.range *= 1.3f;
        towerModel.GetAttackModel().range = towerModel.range;
        var projectile = towerModel.GetAttackModel().weapons[0].projectile;
        projectile.pierce += 3;
        projectile.GetDamageModel().damage += 1;
        TopPathEffects.ApplySeedProjectileDisplay(projectile);
    }
}

public class BrambleBurst : ModUpgrade<GardenerMonkey>
{
    public override int Path => TOP;
    public override int Tier => 3;
    public override int Cost => 1600;
    public override string DisplayName => "Exploding Seeds";
    public override string Description => "Seeds for exploding pineapples.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        towerModel.range *= 1.1f;
        towerModel.GetAttackModel().range = towerModel.range;
        var weapon = towerModel.GetAttackModel().weapons[0];
        weapon.Rate *= 0.9f;
        weapon.projectile.pierce += 7;
        weapon.projectile.GetDamageModel().damage += 2;
        TopPathEffects.ApplyExplosiveSeedProjectileDisplay(weapon.projectile);
        TopPathEffects.TryAddExplosionOnContact(weapon.projectile, TowerType.BombShooter);
    }
}

internal static class TopPathEffects
{
    public static void ApplySeedProjectileDisplay(ProjectileModel projectile)
    {
        ApplyProjectileDisplay(projectile, TowerType.DartMonkey, 0.8f);
    }

    public static void ApplyExplosiveSeedProjectileDisplay(ProjectileModel projectile)
    {
        ApplyProjectileDisplay(projectile, TowerType.BombShooter, 0.55f);
    }

    private static void ApplyProjectileDisplay(ProjectileModel projectile, string sourceTowerId, float scale)
    {
        if (projectile == null)
        {
            return;
        }

        try
        {
            var sourceProjectile = Game.instance.model.GetTowerFromId(sourceTowerId).GetAttackModel().weapons[0].projectile;
            projectile.display = sourceProjectile.display;

            var sourceDisplay = sourceProjectile.GetBehavior<DisplayModel>();
            if (sourceDisplay != null)
            {
                var display = sourceDisplay.Duplicate();
                display.scale = scale;
                projectile.displayModel = display;
            }

            projectile.scale = scale;
        }
        catch
        {
            // Keep the existing projectile display if the source display is unavailable.
        }
    }

    public static void TryAddExplosionOnContact(ProjectileModel projectile, string sourceTowerId)
    {
        if (projectile == null)
        {
            return;
        }

        projectile.GetDamageModel().immuneBloonProperties = 0;

        try
        {
            var bombProjectile = Game.instance.model.GetTowerFromId(sourceTowerId).GetAttackModel().weapons[0].projectile;
            foreach (var behavior in bombProjectile.behaviors)
            {
                var typeName = behavior?.GetType().Name ?? "";
                var behaviorName = behavior?.name ?? "";
                if (!typeName.Contains("CreateProjectileOnContact") &&
                    !typeName.Contains("CreateEffectOnContact") &&
                    !behaviorName.Contains("Explosion"))
                {
                    continue;
                }

                projectile.AddBehavior(behavior.Duplicate());
            }
        }
        catch
        {
            // Keep the normal seed attack if the Bomb Shooter explosion behavior is unavailable.
        }
    }
}

public class IronwoodThorns : ModUpgrade<GardenerMonkey>
{
    public override int Path => TOP;
    public override int Tier => 4;
    public override int Cost => 4800;
    public override string DisplayName => "Seed Packets";
    public override string Description => "Fires packets of five recursive explosive seeds at once.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var weapon = towerModel.GetAttackModel().weapons[0];
        weapon.emission = new ParallelEmissionModel("SeedPacketsEmission_", 5, 28, 0, false, null);
        weapon.projectile.pierce += 8;
        var damage = weapon.projectile.GetDamageModel();
        damage.damage += 3;
        damage.immuneBloonProperties = 0;
        TopPathEffects.ApplyExplosiveSeedProjectileDisplay(weapon.projectile);
        TopPathEffects.TryAddExplosionOnContact(weapon.projectile, TowerType.BombShooter + "-004");
    }
}

public class AncientGrove : ModUpgrade<GardenerMonkey>
{
    public override int Path => TOP;
    public override int Tier => 5;
    public override int Cost => 37500;
    public override string DisplayName => "Seed Storm";
    public override string Description => "Becomes a gatling bomber, rapidly firing recursive explosive seed packets at bloons.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        GardenerMonkey.ApplyMonkeyFarmerProDisplay(towerModel);
        towerModel.range = 999;
        towerModel.GetAttackModel().range = towerModel.range;
        var weapon = towerModel.GetAttackModel().weapons[0];
        weapon.Rate *= 0.25f;
        weapon.projectile.pierce += 15;
        weapon.projectile.RemoveBehaviors<AgeModel>();
        weapon.projectile.AddBehavior(new AgeModel("SeedStormLongRangeLifetime_", 6f, 0, false, null));
        var damage = weapon.projectile.GetDamageModel();
        damage.damage += 10;
        damage.immuneBloonProperties = 0;
        TopPathEffects.ApplyExplosiveSeedProjectileDisplay(weapon.projectile);
    }
}

public class QuickSprout : ModUpgrade<GardenerMonkey>
{
    public override int Path => MIDDLE;
    public override int Tier => 1;
    public override int Cost => 300;
    public override string DisplayName => "Quicker Swing";
    public override string Description => "The Monkey Farmer swings the pitchfork faster.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        towerModel.GetAttackModel().weapons[0].Rate *= 0.82f;
    }
}

public class Fertilizer : ModUpgrade<GardenerMonkey>
{
    public override int Path => MIDDLE;
    public override int Tier => 2;
    public override int Cost => 300;
    public override string DisplayName => "Sharper Swing";
    public override string Description => "Sharper pitchfork swings improve range, pierce, and damage.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        towerModel.range += 4;
        var attack = towerModel.GetAttackModel();
        attack.range = towerModel.range;
        attack.weapons[0].Rate *= 0.95f;
        attack.weapons[0].projectile.pierce += 2;
        attack.weapons[0].projectile.GetDamageModel().damage += 2;
    }
}

public class VineSnare : ModUpgrade<GardenerMonkey>
{
    public override int Path => MIDDLE;
    public override int Tier => 3;
    public override int Cost => 2100;
    public override string DisplayName => "Flaming Fork";
    public override string Description => "A heated pitchfork pierces more bloons, burns targets, and pops leads.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var projectile = towerModel.GetAttackModel().weapons[0].projectile;
        projectile.pierce += 5;
        var damage = projectile.GetDamageModel();
        damage.damage += 1;
        damage.immuneBloonProperties = 0;
        MiddlePathEffects.TryAddBurnEffect(projectile);
    }
}

internal static class MiddlePathEffects
{
    public static void TryAddBurnEffect(ProjectileModel projectile)
    {
        if (projectile == null)
        {
            return;
        }

        try
        {
            foreach (var towerId in new[] { TowerType.Gwendolin + " 3", TowerType.Gwendolin + " 4", TowerType.WizardMonkey + "-030" })
            {
                var sourceTower = Game.instance.model.GetTowerFromId(towerId);
                var sourceProjectile = sourceTower?.GetAttackModel()?.weapons[0]?.projectile;
                if (sourceProjectile?.behaviors == null)
                {
                    continue;
                }

                foreach (var behavior in sourceProjectile.behaviors)
                {
                    var behaviorName = behavior?.name ?? "";
                    var typeName = behavior?.GetType().Name ?? "";
                    if (!behaviorName.Contains("Burn") && !behaviorName.Contains("burn") &&
                        !typeName.Contains("DamageOverTime") && !typeName.Contains("AddBehaviorToBloon"))
                    {
                        continue;
                    }

                    projectile.AddBehavior(behavior.Duplicate());
                    return;
                }
            }
        }
        catch
        {
            // Lead popping still works even if a source burn behavior is unavailable.
        }
    }
}

public class Greenhouse : ModUpgrade<GardenerMonkey>
{
    public override int Path => MIDDLE;
    public override int Tier => 4;
    public override int Cost => 5200;
    public override string DisplayName => "Burning Harvest";
    public override string Description => "A blazing pitchfork harvests bloons with much stronger swings, more pierce, and frozen popping.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var weapon = towerModel.GetAttackModel().weapons[0];
        weapon.Rate *= 0.55f;
        weapon.projectile.pierce += 10;
        var damage = weapon.projectile.GetDamageModel();
        damage.damage += 5;
        damage.immuneBloonProperties = 0;
    }
}

public class Overgrowth : ModUpgrade<GardenerMonkey>
{
    public override int Path => MIDDLE;
    public override int Tier => 5;
    public override int Cost => 65000;
    public override string DisplayName => "Pitchfork Inferno";
    public override string Description => "An infernal pitchfork attacks very fast, burns harder, and pops every bloon type except camo.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        GardenerMonkey.ApplyMonkeyFarmerProDisplay(towerModel);
        towerModel.range += 14;
        towerModel.GetAttackModel().range = towerModel.range;
        var weapon = towerModel.GetAttackModel().weapons[0];
        weapon.Rate *= 0.18f;
        weapon.projectile.pierce += 5;
        var damage = weapon.projectile.GetDamageModel();
        damage.damage += 10;
        damage.immuneBloonProperties = 0;
        MiddlePathEffects.TryAddBurnEffect(weapon.projectile);
    }
}

public class CarrotFarmer : ModUpgrade<GardenerMonkey>
{
    public override int Path => BOTTOM;
    public override int Tier => 1;
    public override int Cost => 350;
    public override string Description => "Carrot farming improves the Monkey Farmer's eyesight, increasing range and revealing camo bloons.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        towerModel.range += 5;
        var attack = towerModel.GetAttackModel();
        attack.range = towerModel.range;
        attack.RemoveFilter<FilterInvisibleModel>();
        attack.weapons[0].projectile.SetHitCamo(true);
    }
}

public class FruitVariety : ModUpgrade<GardenerMonkey>
{
    public override int Path => BOTTOM;
    public override int Tier => 2;
    public override int Cost => 1115;
    public override string Description => "More crop options reduce Crop Stand cooldown and increase its cash per bloon.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var ability = towerModel.GetAbility();
        if (ability == null)
        {
            return;
        }

        ability.cooldown = Math.Max(1, ability.cooldown - 10);
        ability.Cooldown = ability.cooldown;
        CropStandUpgradeTools.SetCropStandProjectileId(ability, "CropStandFruitVariety");
    }
}

public class FarmersMarket : ModUpgrade<GardenerMonkey>
{
    public override int Path => BOTTOM;
    public override int Tier => 3;
    public override int Cost => 2800;
    public override string Description => "Crop Stand becomes a Farmers Market, earns more cash, and can be placed in two spots.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var ability = towerModel.GetAbility();
        if (ability == null)
        {
            return;
        }

        ability.cooldown = Math.Max(1, ability.cooldown - 5);
        ability.Cooldown = ability.cooldown;
        CropStandUpgradeTools.SetCropStandProjectileId(ability, "CropStandFarmersMarket");
        GardenerMonkey.AddSecondCropStandAbility(towerModel);
    }
}

public class HarvestMarket : ModUpgrade<GardenerMonkey>
{
    public override int Path => BOTTOM;
    public override int Tier => 4;
    public override int Cost => 8500;
    public override string Description => "A larger harvest market reaches farther, earns more cash, and throws stronger seeds.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        towerModel.range += 14;
        towerModel.GetAttackModel().range = towerModel.range;

        var projectile = towerModel.GetAttackModel().weapons[0].projectile;
        projectile.pierce += 10;
        projectile.GetDamageModel().damage += 2;

        foreach (var ability in towerModel.GetBehaviors<AbilityModel>())
        {
            if (ability == null || !ability.displayName.Contains("Crop Stand"))
            {
                continue;
            }

            ability.cooldown = Math.Max(1, ability.cooldown - 10);
            ability.Cooldown = ability.cooldown;

            var projectileId = ability.displayName.Contains("2")
                ? "CropStandHarvestMarketSecond"
                : "CropStandHarvestMarket";
            CropStandUpgradeTools.SetCropStandProjectileId(ability, projectileId);
        }
    }
}

public class GoldenOrchard : ModUpgrade<GardenerMonkey>
{
    public override int Path => BOTTOM;
    public override int Tier => 5;
    public override int Cost => 45000;
    public override string DisplayName => "World Crop Center";
    public override string Description => "A world crop center reaches far, rapidly refreshes markets, and earns huge cash from passing bloons.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        GardenerMonkey.ApplyMonkeyFarmerProDisplay(towerModel);
        towerModel.range += 15;
        towerModel.GetAttackModel().range = towerModel.range;

        var weapon = towerModel.GetAttackModel().weapons[0];
        weapon.Rate *= 0.45f;
        weapon.projectile.pierce += 10;
        weapon.projectile.GetDamageModel().damage += 7;

        foreach (var ability in towerModel.GetBehaviors<AbilityModel>())
        {
            if (ability == null || !ability.displayName.Contains("Crop Stand"))
            {
                continue;
            }

            ability.cooldown = Math.Max(1, ability.cooldown - 15);
            ability.Cooldown = ability.cooldown;

            var projectileId = ability.displayName.Contains("2")
                ? "CropStandGoldenOrchardSecond"
                : "CropStandGoldenOrchard";
            CropStandUpgradeTools.SetCropStandProjectileId(ability, projectileId);
        }

        MoneyPathIncome.CopyIncomeFrom(towerModel, TowerType.BananaFarm + "-005");
    }
}

internal static class MoneyPathIncome
{
    public static void CopyIncomeFrom(TowerModel towerModel, string towerId)
    {
        var sourceTower = Game.instance.model.GetTowerFromId(towerId);
        var incomeModel = sourceTower?.GetBehavior<PerRoundCashBonusTowerModel>();

        if (incomeModel == null)
        {
            return;
        }

        var income = incomeModel.Duplicate();

        towerModel.RemoveBehaviors<PerRoundCashBonusTowerModel>();
        towerModel.AddBehavior(income);
    }
}

internal static class CropStandUpgradeTools
{
    private static readonly string[] CropStandProjectileIds =
    {
        "CropStand",
        "CropStandFruitVariety",
        "CropStandFarmersMarket",
        "CropStandFarmersMarketSecond",
        "CropStandHarvestMarket",
        "CropStandHarvestMarketSecond",
        "CropStandGoldenOrchard",
        "CropStandGoldenOrchardSecond"
    };

    public static void SetCropStandProjectileId(AbilityModel ability, string projectileId)
    {
        var placement = ability.GetBehavior<ActivateAttackCreateTowerPlacementModel>();
        SetProjectileId(placement?.attacks, projectileId);

        var activatedAttack = ability.GetBehavior<ActivateAttackModel>();
        SetProjectileId(activatedAttack?.attacks, projectileId);

        var createdProjectile = ability.GetBehavior<CreateProjectileOnAbilityActivateModel>();
        if (createdProjectile?.projectile != null)
        {
            createdProjectile.projectile.id = projectileId;
            ApplyCropStandDisplay(createdProjectile.projectile, projectileId);
        }
    }

    public static void SetCropStandLimit(AbilityModel ability, string projectileId, int limit)
    {
        var placement = ability.GetBehavior<ActivateAttackCreateTowerPlacementModel>();
        SetProjectileLimit(placement?.attacks, projectileId, limit);

        var activatedAttack = ability.GetBehavior<ActivateAttackModel>();
        SetProjectileLimit(activatedAttack?.attacks, projectileId, limit);
    }

    private static void SetProjectileId(Il2CppReferenceArray<AttackModel>? attacks, string projectileId)
    {
        if (attacks == null)
        {
            return;
        }

        foreach (var attack in attacks)
        {
            if (attack?.weapons == null)
            {
                continue;
            }

            foreach (var weapon in attack.weapons)
            {
                ProjectileModel projectile = weapon.projectile;
                if (projectile != null && IsCropStandProjectileId(projectile.id))
                {
                    projectile.id = projectileId;
                    ApplyCropStandDisplay(projectile, projectileId);
                }

                var limitModel = weapon.GetBehavior<LimitProjectileModel>();
                if (limitModel != null && IsCropStandProjectileId(limitModel.projectileId))
                {
                    limitModel.projectileId = projectileId;
                }
            }
        }
    }

    private static void SetProjectileLimit(Il2CppReferenceArray<AttackModel>? attacks, string projectileId, int limit)
    {
        if (attacks == null)
        {
            return;
        }

        foreach (var attack in attacks)
        {
            if (attack?.weapons == null)
            {
                continue;
            }

            foreach (WeaponModel weapon in attack.weapons)
            {
                var limitModel = weapon.GetBehavior<LimitProjectileModel>();
                if (limitModel == null)
                {
                    weapon.AddBehavior(new LimitProjectileModel("CropStandLimit_", projectileId, limit, 0, false, false));
                    continue;
                }

                limitModel.projectileId = projectileId;
                limitModel.limit = limit;
            }
        }
    }

    private static bool IsCropStandProjectileId(string projectileId)
    {
        foreach (var cropStandProjectileId in CropStandProjectileIds)
        {
            if (projectileId == cropStandProjectileId)
            {
                return true;
            }
        }

        return false;
    }

    private static void ApplyCropStandDisplay(ProjectileModel projectile, string projectileId)
    {
        if (!IsWorldCropCenterProjectileId(projectileId))
        {
            return;
        }

        try
        {
            var worldCropCenter = Game.instance.model.GetTowerFromId(TowerType.BananaFarm + "-005");
            var display = worldCropCenter?.GetBehavior<DisplayModel>();
            if (display == null)
            {
                return;
            }

            projectile.display = display.display;
        }
        catch
        {
            // Keep the normal crop stand display if the Banana Farm display is unavailable.
        }
    }

    private static bool IsWorldCropCenterProjectileId(string projectileId)
    {
        return projectileId == "CropStandGoldenOrchard" || projectileId == "CropStandGoldenOrchardSecond";
    }

}
