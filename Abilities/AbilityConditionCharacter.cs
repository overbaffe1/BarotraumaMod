using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003B6 RID: 950
	internal class AbilityConditionCharacter : AbilityConditionData
	{
		// Token: 0x060045FB RID: 17915 RVA: 0x0026AA8C File Offset: 0x00268C8C
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

		// Token: 0x060045FC RID: 17916 RVA: 0x0026ABC4 File Offset: 0x00268DC4
		public sealed override bool MatchesCondition()
		{
			return this.MatchesCondition(null);
		}

		// Token: 0x060045FD RID: 17917 RVA: 0x0026ABCD File Offset: 0x00268DCD
		public sealed override bool MatchesCondition(AbilityObject abilityObject)
		{
			if (!this.invert)
			{
				return this.MatchesConditionSpecific(abilityObject);
			}
			return !this.MatchesConditionSpecific(abilityObject);
		}

		// Token: 0x060045FE RID: 17918 RVA: 0x0026ABEC File Offset: 0x00268DEC
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

		// Token: 0x060045FF RID: 17919 RVA: 0x0026AC94 File Offset: 0x00268E94
		protected virtual bool MatchesCharacter(Character character)
		{
			return true;
		}

		// Token: 0x0400244B RID: 9291
		private readonly List<AbilityCondition.TargetType> targetTypes;

		// Token: 0x0400244C RID: 9292
		private readonly List<PropertyConditional> conditionals = new List<PropertyConditional>();

		// Token: 0x0400244D RID: 9293
		private readonly bool targetAbilityTarget;
	}
}
