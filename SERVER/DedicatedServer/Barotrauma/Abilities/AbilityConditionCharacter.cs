using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020002F0 RID: 752
	internal class AbilityConditionCharacter : AbilityConditionData
	{
		// Token: 0x060031C1 RID: 12737 RVA: 0x00152C0C File Offset: 0x00150E0C
		public AbilityConditionCharacter(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.targetTypes = base.ParseTargetTypes(conditionElement.GetAttributeStringArray("targettypes", conditionElement.GetAttributeStringArray("targettype", Array.Empty<string>(), false), false));
			foreach (ContentXElement subElement in conditionElement.Elements())
			{
				Identifier identifier = subElement.NameAsIdentifier();
				if (identifier == "conditional")
				{
					this.conditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
				}
			}
			if (!this.targetTypes.Any<AbilityCondition.TargetType>() && !this.conditionals.Any<PropertyConditional>() && base.GetType() == typeof(AbilityConditionCharacter))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(101, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent \"");
				defaultInterpolatedStringHandler.AppendFormatted<CharacterTalent>(characterTalent);
				defaultInterpolatedStringHandler.AppendLiteral("\". No target types or conditionals defined - the condition will match any character.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, conditionElement.ContentPackage, false, false);
			}
			this.targetAbilityTarget = conditionElement.GetAttributeBool("targetAbilityTarget", !(this is AbilityConditionHasPermanentStat));
		}

		// Token: 0x060031C2 RID: 12738 RVA: 0x00152D44 File Offset: 0x00150F44
		public sealed override bool MatchesCondition()
		{
			return this.MatchesCondition(null);
		}

		// Token: 0x060031C3 RID: 12739 RVA: 0x00152D4D File Offset: 0x00150F4D
		public sealed override bool MatchesCondition(AbilityObject abilityObject)
		{
			if (!this.invert)
			{
				return this.MatchesConditionSpecific(abilityObject);
			}
			return !this.MatchesConditionSpecific(abilityObject);
		}

		// Token: 0x060031C4 RID: 12740 RVA: 0x00152D6C File Offset: 0x00150F6C
		protected sealed override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			Character character;
			if (!this.targetAbilityTarget)
			{
				character = this.character;
			}
			else
			{
				IAbilityCharacter abilityCharacter = abilityObject as IAbilityCharacter;
				character = (((abilityCharacter != null) ? abilityCharacter.Character : null) ?? this.character);
			}
			Character targetCharacter = character;
			if (targetCharacter == null)
			{
				return false;
			}
			if (!base.IsViableTarget(this.targetTypes, targetCharacter))
			{
				return false;
			}
			foreach (PropertyConditional conditional in this.conditionals)
			{
				if (!conditional.Matches(targetCharacter))
				{
					return false;
				}
			}
			return this.MatchesCharacter(targetCharacter);
		}

		// Token: 0x060031C5 RID: 12741 RVA: 0x00152E14 File Offset: 0x00151014
		protected virtual bool MatchesCharacter(Character character)
		{
			return true;
		}

		// Token: 0x0400188A RID: 6282
		private readonly List<AbilityCondition.TargetType> targetTypes;

		// Token: 0x0400188B RID: 6283
		private readonly List<PropertyConditional> conditionals = new List<PropertyConditional>();

		// Token: 0x0400188C RID: 6284
		private readonly bool targetAbilityTarget;
	}
}
