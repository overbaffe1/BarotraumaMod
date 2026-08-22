using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000335 RID: 821
	internal sealed class CharacterAbilityGiveExperience : CharacterAbility
	{
		// Token: 0x17000E31 RID: 3633
		// (get) Token: 0x06003282 RID: 12930 RVA: 0x00155CB8 File Offset: 0x00153EB8
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06003283 RID: 12931 RVA: 0x00155CBC File Offset: 0x00153EBC
		public CharacterAbilityGiveExperience(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.amount = abilityElement.GetAttributeInt("amount", 0);
			this.level = abilityElement.GetAttributeInt("level", 0);
			if (this.amount == 0 && this.level == 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent ");
				defaultInterpolatedStringHandler.AppendFormatted(base.CharacterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(" - no exp amount or level defined in ");
				defaultInterpolatedStringHandler.AppendFormatted("CharacterAbilityGiveExperience");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
			}
			if (this.amount > 0 && this.level > 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(59, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in talent ");
				defaultInterpolatedStringHandler2.AppendFormatted(base.CharacterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler2.AppendLiteral(" - ");
				defaultInterpolatedStringHandler2.AppendFormatted("CharacterAbilityGiveExperience");
				defaultInterpolatedStringHandler2.AppendLiteral(" defines both an exp amount and a level.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x06003284 RID: 12932 RVA: 0x00155DDC File Offset: 0x00153FDC
		private void ApplyEffectSpecific(Character targetCharacter)
		{
			if (this.level <= 0)
			{
				if (this.amount != 0)
				{
					CharacterInfo info = targetCharacter.Info;
					if (info == null)
					{
						return;
					}
					info.GiveExperience(this.amount);
				}
				return;
			}
			CharacterInfo info2 = targetCharacter.Info;
			if (info2 == null)
			{
				return;
			}
			info2.GiveExperience(targetCharacter.Info.GetExperienceRequiredForLevel(this.level) + this.amount);
		}

		// Token: 0x06003285 RID: 12933 RVA: 0x00155E3C File Offset: 0x0015403C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			AbilityCharacterKill abilityCharacterKill = abilityObject as AbilityCharacterKill;
			if (abilityCharacterKill != null)
			{
				Character killer = abilityCharacterKill.Killer;
				if (killer != null)
				{
					this.ApplyEffectSpecific(killer);
					return;
				}
			}
			IAbilityCharacter abilityCharacter = abilityObject as IAbilityCharacter;
			Character targetCharacter = (abilityCharacter != null) ? abilityCharacter.Character : null;
			if (targetCharacter != null)
			{
				this.ApplyEffectSpecific(targetCharacter);
				return;
			}
			this.ApplyEffectSpecific(base.Character);
		}

		// Token: 0x06003286 RID: 12934 RVA: 0x00155E8F File Offset: 0x0015408F
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific(base.Character);
		}

		// Token: 0x040018E2 RID: 6370
		private readonly int amount;

		// Token: 0x040018E3 RID: 6371
		private readonly int level;
	}
}
