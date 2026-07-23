using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Models.Effects;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Unity;
using Il2CppNinjaKiwi.Common.ResourceUtils;

namespace Gardener.Towers;

public class GardenerMonkey : ModTower
{
    public override TowerSet TowerSet => TowerSet.Primary;
    public override string BaseTower => TowerType.DartMonkey;
    public override int Cost => 425;
    public override int TopPathUpgrades => 5;
    public override int MiddlePathUpgrades => 5;
    public override int BottomPathUpgrades => 5;
    public override ParagonMode ParagonMode => ParagonMode.None;
    public override string DisplayName => "Monkey Farmer";
    public override SpriteReference IconReference => FarmerIcon();
    public override SpriteReference PortraitReference => FarmerIcon();
    public override string Description => "Plants sharp seeds and places a crop stand on the track that turns passing bloons into extra cash.";

    private static SpriteReference FarmerIcon()
    {
        return Game.instance.model.GetPowerWithId(PowerType.BananaFarmer).icon;
    }

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
        ApplyMonkeyFarmerDisplay(towerModel);
        towerModel.range = 26;

        AttackModel attack = towerModel.GetAttackModel();
        attack.range = towerModel.range;

        WeaponModel weapon = attack.weapons[0];
        weapon.Rate = 0.95f;

        var projectile = weapon.projectile;
        projectile.pierce = 3;
        projectile.radius = 14;
        projectile.scale = 0.01f;
        projectile.GetDamageModel().damage = 1;
        projectile.AddBehavior(new AgeModel("PitchforkSwingExpiresQuickly_", 0.08f, 0, false, null));

        AddCropStandAbility(towerModel);
    }

    public override void ModifyTowerModelForMatch(TowerModel towerModel, GameModel gameModel)
    {
        if (HasAnyTierFive(towerModel))
        {
            ApplyMonkeyFarmerProDisplay(towerModel);
            return;
        }

        ApplyMonkeyFarmerDisplay(towerModel);
    }

    public static void ApplyMonkeyFarmerProDisplay(TowerModel towerModel)
    {
        if (TryApplyPowerDisplay(towerModel, "BananaFarmerPro"))
        {
            return;
        }

        ApplyMonkeyFarmerDisplay(towerModel);
    }

    private static bool HasAnyTierFive(TowerModel towerModel)
    {
        return towerModel.tiers != null &&
               towerModel.tiers.Count >= 3 &&
               (towerModel.tiers[0] >= 5 || towerModel.tiers[1] >= 5 || towerModel.tiers[2] >= 5);
    }

    private static void ApplyMonkeyFarmerDisplay(TowerModel towerModel)
    {
        TryApplyPowerDisplay(towerModel, PowerType.BananaFarmer);
    }

    private static bool TryApplyPowerDisplay(TowerModel towerModel, string powerId)
    {
        var farmerTower = Game.instance.model.GetPowerWithId(powerId).tower;
        if (farmerTower == null)
        {
            return false;
        }

        var farmerDisplay = farmerTower?.GetBehavior<DisplayModel>();

        if (farmerDisplay == null)
        {
            return false;
        }

        towerModel.SetDisplay(farmerDisplay.display);
        return true;
    }

    public static void AddCropStandAbility(TowerModel towerModel)
    {
        towerModel.AddBehavior(CreateCropStandAbility("Crop Stand", 40, "CropStand"));
    }

    public static void AddSecondCropStandAbility(TowerModel towerModel)
    {
        towerModel.AddBehavior(CreateCropStandAbility("Crop Stand 2", 40, "CropStandFarmersMarketSecond"));
    }

    private static AbilityModel CreateCropStandAbility(string displayName, float cooldown, string projectileId)
    {
        var sourceTower = TryGetTowerFromIds(TowerType.Gwendolin + " 3", TowerType.Gwendolin + " 4", TowerType.BoomerangMonkey + "-040");
        var ability = sourceTower.GetAbility().Duplicate();

        ability.name = displayName.Replace(" ", "") + "AbilityModel_";
        ability.displayName = displayName;
        ability.description = "Places a crop market on the track. Bloons that pass through it give extra cash.";
        ability.cooldown = cooldown;
        ability.Cooldown = cooldown;
        ability.canActivateBetweenRounds = false;
        ability.startOffCooldown = true;
        ApplyCropStandAbilityIcon(ability);
        TryUseCropStandProjectile(ability, projectileId);
        CropStandUpgradeTools.SetCropStandLimit(ability, projectileId, 1);

        return ability;
    }

    private static void ApplyCropStandAbilityIcon(AbilityModel ability)
    {
        try
        {
            var bankTower = Game.instance.model.GetTowerFromId(TowerType.BananaFarm + "-040");
            var bankAbility = bankTower?.GetAbility();
            if (bankAbility is null)
            {
                return;
            }

            ability.icon = bankAbility.icon;
        }
        catch
        {
            // Keep the copied placement ability icon if the Banana Farm icon is unavailable during registration.
        }
    }

    private static void AddOldCropStandAbility(TowerModel towerModel)
    {
        var spikeFactory = Game.instance.model.GetTowerFromId(TowerType.SpikeFactory);
        var cropStand = spikeFactory.GetAttackModel().weapons[0].projectile.Duplicate();
        cropStand.name = "CropStandProjectileModel_";
        cropStand.id = "CropStand";
        cropStand.pierce = 9999;
        cropStand.maxPierce = 9999;
        cropStand.radius = 28;
        cropStand.ignorePierceExhaustion = true;

        var damage = cropStand.GetDamageModel();
        if (damage != null)
        {
            damage.damage = 0;
            damage.maxDamage = 0;
            damage.createPopEffect = false;
        }

        cropStand.AddBehavior(new ClearHitBloonsWhenNoLongerCollidingModel("CropStandRefreshHits_", 0.1f));
        cropStand.AddBehavior(new IncreaseBloonWorthWithTierModel("CropStandLayerCash_", "CropStandLayerCashMutator", 15, "", null));
        cropStand.AddBehavior(new DestroyProjectileIfTowerDestroyedModel("CropStandRemoveWithGardener_"));

        var ability = Game.instance.model.GetTowerFromId(TowerType.BoomerangMonkey + "-040").GetAbility().Duplicate();
        ability.name = "CropStandAbilityModel_";
        ability.displayName = "Crop Stand";
        ability.description = "Places a crop stand on the selected track spot. Bloons that pass through it are worth extra cash.";
        ability.cooldown = 0.1f;
        ability.Cooldown = 0.1f;
        ability.canActivateBetweenRounds = true;
        ability.startOffCooldown = true;

        var trackAreaTypes = new Il2CppStructArray<AreaType>(1);
        trackAreaTypes[0] = AreaType.track;

        var abilityBehaviors = new Il2CppReferenceArray<Model>(1);
        abilityBehaviors[0] = new PlaceProjectileAtModel(
            "CropStandPlaceProjectile_",
            cropStand,
            new SingleEmissionModel("CropStandSingleEmission_", new Il2CppReferenceArray<EmissionBehaviorModel>(0)),
            trackAreaTypes,
            18,
            towerModel.range,
            0,
            null,
            null,
            null,
            0,
            0,
            0,
            false,
            new AssetPathModel("CropStandPlacementDisplay_", cropStand.display));
        ability.behaviors = abilityBehaviors;

        towerModel.AddBehavior(ability);
    }

    private static void AddTestAbility(TowerModel towerModel)
    {
        var sourceTower = TryGetTowerFromIds(TowerType.Gwendolin + " 3", TowerType.Gwendolin + " 4", TowerType.BoomerangMonkey + "-040");
        var ability = sourceTower.GetAbility().Duplicate();

        ability.name = "GardenerPlacementTestAbilityModel_";
        ability.displayName = "Crop Stand";
        ability.description = "Places a crop stand on the track. Bloons that pass through it become worth extra cash.";
        ability.cooldown = 40;
        ability.Cooldown = 40;
        ability.canActivateBetweenRounds = false;
        ability.startOffCooldown = true;
        TryUseCropStandProjectile(ability, "CropStand");

        towerModel.AddBehavior(ability);
    }

    private static void TryUseCropStandProjectile(AbilityModel ability, string projectileId)
    {
        var cropStand = CreateCropStandProjectile();
        cropStand.id = projectileId;
        var placement = ability.GetBehavior<ActivateAttackCreateTowerPlacementModel>();

        if (TryReplacePlacementProjectile(placement?.attacks, cropStand))
        {
            ability.RemoveBehaviors<ActivateAttackModel>();
            ability.RemoveBehaviors<CreateProjectileOnAbilityActivateModel>();
            RemoveCopiedFireEffects(ability);
            return;
        }

        var activatedAttack = ability.GetBehavior<ActivateAttackModel>();
        if (TryReplacePlacementProjectile(activatedAttack?.attacks, cropStand))
        {
            RemoveCopiedFireEffects(ability);
            return;
        }

        var createdProjectile = ability.GetBehavior<CreateProjectileOnAbilityActivateModel>();
        if (createdProjectile != null)
        {
            createdProjectile.projectile = cropStand;
        }

        RemoveCopiedFireEffects(ability);
    }

    private static void RemoveCopiedFireEffects(AbilityModel ability)
    {
        ability.RemoveBehaviors<CreateEffectOnAbilityModel>();
        ability.RemoveBehaviors<CreateEffectOnAbilityEndModel>();
        ability.RemoveBehaviors<CreateSoundOnAbilityModel>();
        ability.RemoveBehaviors<CreateRandomSoundOnAbilityModel>();
        ability.RemoveBehaviors<ChangeProjectileDisplayModel>();
    }

    private static bool TryReplacePlacementProjectile(Il2CppReferenceArray<AttackModel>? attacks, ProjectileModel cropStand)
    {
        if (attacks == null || attacks.Count == 0)
        {
            return false;
        }

        foreach (var attack in attacks)
        {
            if (attack?.weapons == null || attack.weapons.Count == 0)
            {
                continue;
            }

            attack.weapons[0].projectile = cropStand;
            return true;
        }

        return false;
    }

    private static ProjectileModel CreateCropStandProjectile()
    {
        var spikeFactory = Game.instance.model.GetTowerFromId(TowerType.SpikeFactory);
        var marketFarm = Game.instance.model.GetTowerFromId(TowerType.BananaFarm + "-003");
        var marketDisplay = marketFarm?.GetBehavior<DisplayModel>();
        var cropStand = spikeFactory.GetAttackModel().weapons[0].projectile.Duplicate();

        cropStand.name = "CropStandProjectileModel_";
        cropStand.id = "CropStand";
        cropStand.pierce = 9999;
        cropStand.maxPierce = 9999;
        cropStand.radius = 28;
        cropStand.ignorePierceExhaustion = true;
        cropStand.scale = 0.6f;

        if (marketDisplay != null)
        {
            var cropStandDisplay = marketDisplay.Duplicate();
            cropStandDisplay.name = "CropStandMarketDisplay_";
            cropStandDisplay.scale = 0.25f;
            cropStand.display = marketDisplay.display;
            cropStand.displayModel = cropStandDisplay;
        }

        var damage = cropStand.GetDamageModel();
        if (damage != null)
        {
            damage.damage = 0;
            damage.maxDamage = 0;
            damage.createPopEffect = false;
        }

        cropStand.AddBehavior(new DestroyProjectileIfTowerDestroyedModel("CropStandRemoveWithGardener_"));

        return cropStand;
    }

    private static TowerModel TryGetTowerFromIds(params string[] towerIds)
    {
        foreach (var towerId in towerIds)
        {
            try
            {
                var towerModel = Game.instance.model.GetTowerFromId(towerId);

                if (towerModel?.GetAbility() != null)
                {
                    return towerModel;
                }
            }
            catch
            {
                // Try the next known tower id.
            }
        }

        return Game.instance.model.GetTowerFromId(TowerType.BoomerangMonkey + "-040");
    }

    private static void AddAbilityBehavior(AbilityModel ability, Model behavior)
    {
        var oldBehaviors = ability.behaviors;
        var newBehaviors = new Il2CppReferenceArray<Model>(oldBehaviors.Count + 1);

        for (var i = 0; i < oldBehaviors.Count; i++)
        {
            newBehaviors[i] = oldBehaviors[i];
        }

        newBehaviors[oldBehaviors.Count] = behavior;
        ability.behaviors = newBehaviors;
    }
}
