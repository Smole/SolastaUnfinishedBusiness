﻿using System.Linq;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.CustomInterfaces;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

internal sealed class ReactionAttackModeRestriction : IReactionAttackModeRestriction
{
    internal static (GameLocationCharacter, GameLocationCharacter, RulesetAttackMode) ReactionContext =
        (null, null, null);

    private readonly ValidReactionModeHandler[] validators;

    internal ReactionAttackModeRestriction(params ValidReactionModeHandler[] validators)
    {
        this.validators = validators;
    }

    public bool ValidReactionMode(
        RulesetAttackMode attackMode,
        GameLocationCharacter character,
        GameLocationCharacter target)
    {
        return validators.All(v => v(attackMode, character, target));
    }

#if false
    internal static ValidReactionModeHandler TargetHasNoCondition(ConditionDefinition condition)
    {
        return (_, _, target) =>
        {
            var rulesetCharacter = target.RulesetCharacter;

            return rulesetCharacter != null && !rulesetCharacter.HasConditionOfType(condition.Name);
        };
    }
#endif

    internal static bool CanCharacterReactWithPower(GameLocationBattleManager _, RulesetUsablePower usablePower)
    {
        var (attacker, defender, attackMode) = ReactionContext;

        if (attacker == null || defender == null || attackMode == null)
        {
            return true;
        }

        var validator = usablePower.PowerDefinition.GetFirstSubFeatureOfType<IReactionAttackModeRestriction>();

        return validator == null || validator.ValidReactionMode(attackMode, attacker, defender);
    }
}
