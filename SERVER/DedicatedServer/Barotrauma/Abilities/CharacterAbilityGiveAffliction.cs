using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000334 RID: 820
	internal class CharacterAbilityGiveAffliction : CharacterAbility
	{
		// Token: 0x0600327C RID: 12924 RVA: 0x00155B00 File Offset: 0x00153D00
		public CharacterAbilityGiveAffliction(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.afflictionId = abilityElement.GetAttributeIdentifier("afflictionid", abilityElement.GetAttributeIdentifier("affliction", Identifier.Empty));
			this.strength = abilityElement.GetAttributeFloat("strength", 0f);
			this.multiplyStrengthBySkill = abilityElement.GetAttributeIdentifier("multiplystrengthbyskill", Identifier.Empty);
			this.setValue = abilityElement.GetAttributeBool("setvalue", false);
			if (this.afflictionId.IsEmpty)
			{
				DebugConsole.ThrowError("Error in talent " + base.CharacterTalent.DebugIdentifier + ", CharacterAbilityGiveAffliction - affliction identifier not set.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x0600327D RID: 12925 RVA: 0x00155BA9 File Offset: 0x00153DA9
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched)
			{
				this.ApplyEffect();
			}
		}

		// Token: 0x0600327E RID: 12926 RVA: 0x00155BB4 File Offset: 0x00153DB4
		protected override void ApplyEffect()
		{
			this.ApplyAfflictionToCharacter(base.Character);
		}

		// Token: 0x0600327F RID: 12927 RVA: 0x00155BC4 File Offset: 0x00153DC4
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityCharacter character = abilityObject as IAbilityCharacter;
			if (character != null)
			{
				this.ApplyAfflictionToCharacter(character.Character);
			}
		}

		// Token: 0x06003280 RID: 12928 RVA: 0x00155BE8 File Offset: 0x00153DE8
		private void ApplyAfflictionToCharacter(Character character)
		{
			AfflictionPrefab afflictionPrefab = AfflictionPrefab.Prefabs.Find((AfflictionPrefab a) => a.Identifier == this.afflictionId);
			if (afflictionPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(94, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in CharacterAbilityGiveAffliction - could not find an affliction with the identifier \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.afflictionId);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, base.CharacterTalent.Prefab.ContentPackage, false, false);
				return;
			}
			float strength = this.strength;
			if (!this.multiplyStrengthBySkill.IsEmpty)
			{
				strength *= base.Character.GetSkillLevel(this.multiplyStrengthBySkill);
			}
			character.CharacterHealth.ApplyAffliction(null, afflictionPrefab.Instantiate(strength, null), !this.setValue, false, true);
		}

		// Token: 0x040018DE RID: 6366
		private readonly Identifier afflictionId;

		// Token: 0x040018DF RID: 6367
		private readonly float strength;

		// Token: 0x040018E0 RID: 6368
		private readonly Identifier multiplyStrengthBySkill;

		// Token: 0x040018E1 RID: 6369
		private readonly bool setValue;
	}
}
