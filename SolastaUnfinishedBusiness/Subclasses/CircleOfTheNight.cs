﻿using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MonsterDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MonsterAttackDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class CircleOfTheNight : AbstractSubclass
{
    internal const string Name = "CircleOfTheNight";

    private static readonly ValidatorsPowerUse CanUseCombatHealing = new(
        ValidatorsCharacter.HasAnyOfConditions(ConditionDefinitions.ConditionWildShapeSubstituteForm.name));

    internal CircleOfTheNight()
    {
        // 2nd level

        var combatWildshape = BuildWildShapePower();

        // remove regular WS action

        // kept this name for compatibility reasons
        var actionAffinityWildshape = FeatureDefinitionActionAffinityBuilder
            .Create("OnAfterActionWildShape")
            .SetGuiPresentationNoContent(true)
            .SetForbiddenActions(ActionDefinitions.Id.WildShape)
            .AddToDB();

        // Combat Wild Shape Healing

        var powerCircleOfTheNightWildShapeHealing = FeatureDefinitionPowerBuilder
            .Create("PowerCircleOfTheNightWildShapeHealing")
            .SetGuiPresentation(Category.Feature, PowerPaladinCureDisease)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .SetEffectDescription(CombatHealing())
            .SetCustomSubFeatures(CanUseCombatHealing)
            .AddToDB();

        // 6th Level

        // Primal Strike

        var powerCircleOfTheNightPrimalStrike = FeatureDefinitionAttackModifierBuilder
            .Create("PowerCircleOfTheNightPrimalStrike")
            .SetGuiPresentation(Category.Feature)
            .SetMagicalWeapon()
            .SetCustomSubFeatures(
                new RestrictedContextValidator((_, _, character, _, _, _, _) =>
                    (OperationType.Set,
                        ValidatorsCharacter.HasAnyOfConditions(ConditionDefinitions.ConditionRaging.Name)(character))))
            .AddToDB();

        // Improved Combat Healing
        // At 6th level, your combat healing improves to 2d8
        var powerCircleOfTheNightWildShapeImprovedHealing = FeatureDefinitionPowerBuilder
            .Create("PowerCircleOfTheNightWildShapeImprovedHealing")
            .SetGuiPresentation(Category.Feature, PowerPaladinCureDisease)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .SetEffectDescription(CombatHealing(2))
            .SetCustomSubFeatures(CanUseCombatHealing)
            .SetOverriddenPower(powerCircleOfTheNightWildShapeHealing)
            .AddToDB();

        // 10th Level

        // Superior Combat Healing
        // At 10th level, your combat healing improves to 3d8
        var powerCircleOfTheNightWildShapeSuperiorHealing = FeatureDefinitionPowerBuilder
            .Create("PowerCircleOfTheNightWildShapeSuperiorHealing")
            .SetGuiPresentation(Category.Feature, PowerPaladinCureDisease)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .SetEffectDescription(CombatHealing(3))
            .SetCustomSubFeatures(CanUseCombatHealing)
            .SetOverriddenPower(powerCircleOfTheNightWildShapeImprovedHealing)
            .AddToDB();

        // Elemental Forms
        var featureSetCircleOfTheNightElementalForms = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetCircleOfTheNightElementalForms")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        // 14th level

        // Superior Combat Healing
        // At 14th level, your combat healing improves to 4d8
        var powerCircleOfTheNightWildShapeMasterfulHealing = FeatureDefinitionPowerBuilder
            .Create("PowerCircleOfTheNightWildShapeMasterfulHealing")
            .SetGuiPresentation(Category.Feature, PowerPaladinCureDisease)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .SetEffectDescription(CombatHealing(4))
            .SetCustomSubFeatures(CanUseCombatHealing)
            .SetOverriddenPower(powerCircleOfTheNightWildShapeSuperiorHealing)
            .AddToDB();

        // Monstrous Forms
        var featureSetCircleOfTheNightMonstrousForms = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetCircleOfTheNightMonstrousForms")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass,
                Sprites.GetSprite("CircleOfTheNight", Resources.CircleOfTheNight, 256))
            .AddFeaturesAtLevel(2,
                combatWildshape,
                actionAffinityWildshape,
                powerCircleOfTheNightWildShapeHealing)
            .AddFeaturesAtLevel(6,
                powerCircleOfTheNightPrimalStrike,
                powerCircleOfTheNightWildShapeImprovedHealing)
            .AddFeaturesAtLevel(10,
                featureSetCircleOfTheNightElementalForms,
                powerCircleOfTheNightWildShapeSuperiorHealing)
            .AddFeaturesAtLevel(14,
                featureSetCircleOfTheNightMonstrousForms,
                powerCircleOfTheNightWildShapeMasterfulHealing)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceDruidCircle;

    internal override DeityDefinition DeityDefinition => null;

    private static FeatureDefinitionPower BuildWildShapePower()
    {
        const string NAME = "PowerCircleOfTheNightWildShapeCombat";

        var baseAction = GetDefinition<ActionDefinition>("WildShape");
        var shapeOptions = new List<ShapeOptionDescription>
        {
            ShapeBuilder(2, WildShapeBadlandsSpider),
            ShapeBuilder(2, WildshapeDirewolf),
            ShapeBuilder(2, WildShapeBrownBear),
            ShapeBuilder(4, WildshapeDeepSpider),
            ShapeBuilder(4, HbWildShapeDireBear()),
            ShapeBuilder(6, WildShapeApe),
            ShapeBuilder(8, WildshapeTiger_Drake),
            ShapeBuilder(8, WildShapeGiant_Eagle),
            ShapeBuilder(10, WildShapeTundraTiger),
            ShapeBuilder(10, HbWildShapeAirElemental()),
            ShapeBuilder(10, HbWildShapeFireElemental()),
            ShapeBuilder(10, HbWildShapeEarthElemental()),
            ShapeBuilder(10, HbWildShapeWaterElemental()),
            ShapeBuilder(14, HbWildShapeCrimsonSpider()),
            ShapeBuilder(14, HbWildShapeMinotaurElite())
        };

        var power = FeatureDefinitionPowerSharedPoolBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feature)
            .SetOverriddenPower(PowerDruidWildShape)
            .SetSharedPool(ActivationTime.BonusAction, PowerDruidWildShape)
            .DelegatedToAction()
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.HalfClassLevelHours)
                .SetParticleEffectParameters(PowerDruidWildShape)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetShapeChangeForm(ShapeChangeForm.Type.ClassLevelListSelection, true,
                        ConditionDefinitions.ConditionWildShapeSubstituteForm, shapeOptions)
                    .Build())
                .Build())
            .AddToDB();

        power.SetCustomSubFeatures(new OnAfterActionWildShape(power));

        ActionDefinitionBuilder
            .Create(baseAction, "CombatWildShape")
            .SetGuiPresentation(NAME, Category.Feature, baseAction, baseAction.GuiPresentation.SortOrder)
            .OverrideClassName("WildShape")
            .SetActionId(ExtraActionId.CombatWildShape)
            .SetActionType(ActionDefinitions.ActionType.Bonus)
            .SetActivatedPower(power)
            .AddToDB();

        return power;
    }

    // custom wild shapes

    private static MonsterDefinition HbWildShapeDireBear()
    {
        var biteAttack = new MonsterAttackIteration
        {
            monsterAttackDefinition = MonsterAttackDefinitionBuilder
                .Create(Attack_Wildshape_BrownBear_Bite, "Attack_Wildshape_DireBear_Bite")
                .SetToHitBonus(7)
                .AddToDB()
        };

        var clawAttack = new MonsterAttackIteration
        {
            monsterAttackDefinition = MonsterAttackDefinitionBuilder
                .Create(Attack_Wildshape_BrownBear_Claw, "Attack_Wildshape_DireBear_Claw")
                .SetToHitBonus(7)
                .AddToDB()
        };

        var shape = MonsterDefinitionBuilder
            .Create(WildshapeBlackBear, "WildShapeDireBear")
            .SetCreatureTags(TagsDefinitions.CreatureTagWildShape)
            // STR, DEX, CON, INT, WIS, CHA
            .SetAbilityScores(20, 10, 16, 2, 13, 7)
            .SetArmorClass(14)
            .SetStandardHitPoints(42)
            .SetHitDice(DieType.D10, 5)
            .SetChallengeRating(2)
            .SetOrUpdateGuiPresentation(Category.Monster, WildshapeBlackBear)
            .SetAttackIterations(biteAttack, clawAttack)
            .AddToDB();

        return shape;
    }

    private static MonsterDefinition HbWildShapeAirElemental()
    {
        var shape = MonsterDefinitionBuilder
            .Create(Air_Elemental, "WildShapeAirElemental")
            .SetCreatureTags(TagsDefinitions.CreatureTagWildShape, Name)
            .SetAbilityScores(14, 20, 14, 6, 10, 6) // STR, DEX, CON, INT, WIS, CHA
            .SetArmorClass(15)
            .SetStandardHitPoints(90)
            .SetHitDice(DieType.D10, 12)
            .AddToDB();

        return shape;
    }

    private static MonsterDefinition HbWildShapeFireElemental()
    {
        var shape = MonsterDefinitionBuilder
            .Create(Fire_Elemental, "WildShapeFireElemental")
            .SetCreatureTags(TagsDefinitions.CreatureTagWildShape, Name)
            .AddToDB();

        return shape;
    }

    private static MonsterDefinition HbWildShapeEarthElemental()
    {
        var shape = MonsterDefinitionBuilder
            .Create(Earth_Elemental, "WildShapeEarthElemental")
            .SetCreatureTags(TagsDefinitions.CreatureTagWildShape, Name)
            .AddToDB();

        return shape;
    }

    private static MonsterDefinition HbWildShapeWaterElemental()
    {
        var shape = MonsterDefinitionBuilder
            .Create(Ice_Elemental, "WildShapeWaterElemental")
            .SetCreatureTags(TagsDefinitions.CreatureTagWildShape, Name)
            .AddToDB();

        return shape;
    }

    private static MonsterDefinition HbWildShapeCrimsonSpider()
    {
        var attackCrimsonBite = MonsterAttackDefinitionBuilder
            .Create(Attack_Spiderling_Crimson_Bite, "Attack_Spiderling_WildShape_Crimson_Bite")
            .AddToDB();

        attackCrimsonBite.EffectDescription.EffectForms[1].savingThrowAffinity = EffectSavingThrowType.Negates;

        var shape = MonsterDefinitionBuilder
            .Create(CrimsonSpider, "WildShapeCrimsonSpider")
            .SetCreatureTags(TagsDefinitions.CreatureTagWildShape, Name)
            .AddToDB();

        shape.AttackIterations[1].monsterAttackDefinition = attackCrimsonBite;

        return shape;
    }

    private static MonsterDefinition HbWildShapeMinotaurElite()
    {
        var attackGreataxe = MonsterAttackDefinitionBuilder
            .Create(Attack_Minotaur_Elite_Greataxe, "Attack_Minotaur_Wildshape_Elite_Greataxe")
            .SetToHitBonus(9)
            .AddToDB();

        var attackChargedGore = MonsterAttackDefinitionBuilder
            .Create(Attack_Minotaur_Elite_Charged_Gore, "Attack_Minotaur_Wildshape_Elite_Charged_Gore")
            .SetToHitBonus(9)
            .AddToDB();

        attackChargedGore.EffectDescription.fixedSavingThrowDifficultyClass = 16;

        var attackGore = MonsterAttackDefinitionBuilder
            .Create(Attack_Minotaur_Elite_Gore, "Attack_Minotaur__Wildshape_Elite_Gore")
            .SetToHitBonus(9)
            .AddToDB();

        var shape = MonsterDefinitionBuilder
            .Create(MinotaurElite, "WildShapeMinotaurElite")
            .SetOrUpdateGuiPresentation(Category.Monster)
            .SetCreatureTags(TagsDefinitions.CreatureTagWildShape, Name)
            .SetArmorClass(16)
            .SetStandardHitPoints(126)
            .SetAbilityScores(20, 11, 20, 6, 16, 9)
            .SetSavingThrowScores((AttributeDefinitions.Strength, 9), (AttributeDefinitions.Constitution, 9))
            .SetSkillScores((SkillDefinitions.Perception, 6), (SkillDefinitions.Athletics, 9))
            .SetAttackIterations((2, attackGreataxe), (1, attackChargedGore), (1, attackGore))
            .AddFeatures(
                FeatureDefinitionDamageAffinitys.DamageAffinityBludgeoningResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityPiercingResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinitySlashingResistance)
            .AddToDB();

        return shape;
    }

    private static ShapeOptionDescription ShapeBuilder(int level, MonsterDefinition monster)
    {
        return new ShapeOptionDescription { requiredLevel = level, substituteMonster = monster };
    }

    private static EffectDescription CombatHealing(
        int diceNumber = 1,
        DieType dieType = DieType.D8,
        int bonusHealing = 0)
    {
        var healingForm = EffectFormBuilder
            .Create()
            .SetHealingForm(
                HealingComputation.Dice,
                bonusHealing,
                dieType,
                diceNumber,
                false,
                HealingCap.MaximumHitPoints)
            .Build();

        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetRequiredCondition(ConditionDefinitions.ConditionWildShapeSubstituteForm)
            .SetDurationData(DurationType.Instantaneous)
            .SetEffectForms(healingForm)
            .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
            .Build();

        return effectDescription;
    }

    private sealed class OnAfterActionWildShape : IOnAfterActionFeature
    {
        private readonly FeatureDefinitionPower _featureDefinitionPower;

        public OnAfterActionWildShape(FeatureDefinitionPower featureDefinitionPower)
        {
            _featureDefinitionPower = featureDefinitionPower;
        }

        public void OnAfterAction(CharacterAction action)
        {
            if (action is not CharacterActionUsePower characterActionUsePower ||
                characterActionUsePower.activePower.PowerDefinition != _featureDefinitionPower ||
                !action.ActionParams.TargetSubstitute.CreatureTags.Contains(Name))
            {
                return;
            }

            var rulesetCharacter = action.ActingCharacter.RulesetCharacter;

            rulesetCharacter.UsePower(UsablePowersProvider.Get(_featureDefinitionPower, rulesetCharacter));
        }
    }
}
