using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x020003B2 RID: 946
	internal abstract class AbilityCondition
	{
		// Token: 0x17001208 RID: 4616
		// (get) Token: 0x060045ED RID: 17901 RVA: 0x0026A370 File Offset: 0x00268570
		public virtual bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060045EE RID: 17902 RVA: 0x0026A373 File Offset: 0x00268573
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

		// Token: 0x060045EF RID: 17903
		public abstract bool MatchesCondition(AbilityObject abilityObject);

		// Token: 0x060045F0 RID: 17904
		public abstract bool MatchesCondition();

		// Token: 0x060045F1 RID: 17905 RVA: 0x0026A3B0 File Offset: 0x002685B0
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

		// Token: 0x060045F2 RID: 17906 RVA: 0x0026A438 File Offset: 0x00268638
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

		// Token: 0x060045F3 RID: 17907 RVA: 0x0026A490 File Offset: 0x00268690
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

		// Token: 0x0400243F RID: 9279
		protected CharacterTalent characterTalent;

		// Token: 0x04002440 RID: 9280
		protected Character character;

		// Token: 0x04002441 RID: 9281
		protected bool invert;

		// Token: 0x020010D5 RID: 4309
		protected enum TargetType
		{
			// Token: 0x040059CE RID: 22990
			Any,
			// Token: 0x040059CF RID: 22991
			Enemy,
			// Token: 0x040059D0 RID: 22992
			Ally,
			// Token: 0x040059D1 RID: 22993
			NotSelf,
			// Token: 0x040059D2 RID: 22994
			Alive,
			// Token: 0x040059D3 RID: 22995
			Monster,
			// Token: 0x040059D4 RID: 22996
			InFriendlySubmarine
		}
	}
}
