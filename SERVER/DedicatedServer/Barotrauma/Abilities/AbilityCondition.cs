using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x020002EC RID: 748
	internal abstract class AbilityCondition
	{
		// Token: 0x17000E18 RID: 3608
		// (get) Token: 0x060031B3 RID: 12723 RVA: 0x001524F0 File Offset: 0x001506F0
		public virtual bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060031B4 RID: 12724 RVA: 0x001524F3 File Offset: 0x001506F3
		public AbilityCondition(CharacterTalent characterTalent, ContentXElement conditionElement)
		{
			if (characterTalent == null)
			{
				throw new ArgumentNullException("characterTalent");
			}
			this.characterTalent = characterTalent;
			this.character = characterTalent.Character;
			this.invert = conditionElement.GetAttributeBool("invert", false);
		}

		// Token: 0x060031B5 RID: 12725
		public abstract bool MatchesCondition(AbilityObject abilityObject);

		// Token: 0x060031B6 RID: 12726
		public abstract bool MatchesCondition();

		// Token: 0x060031B7 RID: 12727 RVA: 0x00152530 File Offset: 0x00150730
		protected List<AbilityCondition.TargetType> ParseTargetTypes(string[] targetTypeStrings)
		{
			List<AbilityCondition.TargetType> targetTypes = new List<AbilityCondition.TargetType>();
			foreach (string targetTypeString in targetTypeStrings)
			{
				AbilityCondition.TargetType targetType;
				if (!Enum.TryParse<AbilityCondition.TargetType>(targetTypeString, true, out targetType))
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Invalid target type type \"",
						targetTypeString,
						"\" in CharacterTalent (",
						this.characterTalent.DebugIdentifier,
						")"
					}), null, this.characterTalent.Prefab.ContentPackage, false, false);
				}
				targetTypes.Add(targetType);
			}
			return targetTypes;
		}

		// Token: 0x060031B8 RID: 12728 RVA: 0x001525B8 File Offset: 0x001507B8
		protected bool IsViableTarget(IEnumerable<AbilityCondition.TargetType> targetTypes, Character targetCharacter)
		{
			if (targetCharacter == null)
			{
				return false;
			}
			bool isViable = true;
			foreach (AbilityCondition.TargetType targetType in targetTypes)
			{
				if (!this.IsViableTarget(targetType, targetCharacter))
				{
					isViable = false;
					break;
				}
			}
			return isViable;
		}

		// Token: 0x060031B9 RID: 12729 RVA: 0x00152610 File Offset: 0x00150810
		private bool IsViableTarget(AbilityCondition.TargetType targetType, Character targetCharacter)
		{
			switch (targetType)
			{
			case AbilityCondition.TargetType.Enemy:
				return !HumanAIController.IsFriendly(this.character, targetCharacter, false, false);
			case AbilityCondition.TargetType.Ally:
				return HumanAIController.IsFriendly(this.character, targetCharacter, false, false);
			case AbilityCondition.TargetType.NotSelf:
				return targetCharacter != this.character;
			case AbilityCondition.TargetType.Alive:
				return !targetCharacter.IsDead;
			case AbilityCondition.TargetType.Monster:
				return !targetCharacter.IsHuman && !targetCharacter.IsPet;
			case AbilityCondition.TargetType.InFriendlySubmarine:
				return targetCharacter.Submarine != null && targetCharacter.Submarine.TeamID == this.character.TeamID;
			default:
				return true;
			}
		}

		// Token: 0x0400187E RID: 6270
		protected CharacterTalent characterTalent;

		// Token: 0x0400187F RID: 6271
		protected Character character;

		// Token: 0x04001880 RID: 6272
		protected bool invert;

		// Token: 0x02000B8C RID: 2956
		protected enum TargetType
		{
			// Token: 0x040039BE RID: 14782
			Any,
			// Token: 0x040039BF RID: 14783
			Enemy,
			// Token: 0x040039C0 RID: 14784
			Ally,
			// Token: 0x040039C1 RID: 14785
			NotSelf,
			// Token: 0x040039C2 RID: 14786
			Alive,
			// Token: 0x040039C3 RID: 14787
			Monster,
			// Token: 0x040039C4 RID: 14788
			InFriendlySubmarine
		}
	}
}
