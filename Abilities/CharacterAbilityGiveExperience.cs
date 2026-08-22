using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003FB RID: 1019
	internal sealed class CharacterAbilityGiveExperience : CharacterAbility
	{
		// Token: 0x17001221 RID: 4641
		// (get) Token: 0x060046BC RID: 18108 RVA: 0x0026DB38 File Offset: 0x0026BD38
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060046BD RID: 18109 RVA: 0x0026DB3C File Offset: 0x0026BD3C
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

		// Token: 0x060046BE RID: 18110 RVA: 0x0026DC5C File Offset: 0x0026BE5C
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

		// Token: 0x060046BF RID: 18111 RVA: 0x0026DCBC File Offset: 0x0026BEBC
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

		// Token: 0x060046C0 RID: 18112 RVA: 0x0026DD0F File Offset: 0x0026BF0F
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific(base.Character);
		}

		// Token: 0x040024A3 RID: 9379
		private readonly int amount;

		// Token: 0x040024A4 RID: 9380
		private readonly int level;
	}
}
