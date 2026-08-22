using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003FA RID: 1018
	internal class CharacterAbilityGiveAffliction : CharacterAbility
	{
		// Token: 0x060046B6 RID: 18102 RVA: 0x0026D980 File Offset: 0x0026BB80
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

		// Token: 0x060046B7 RID: 18103 RVA: 0x0026DA29 File Offset: 0x0026BC29
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched)
			{
				this.ApplyEffect();
			}
		}

		// Token: 0x060046B8 RID: 18104 RVA: 0x0026DA34 File Offset: 0x0026BC34
		protected override void ApplyEffect()
		{
			this.ApplyAfflictionToCharacter(base.Character);
		}

		// Token: 0x060046B9 RID: 18105 RVA: 0x0026DA44 File Offset: 0x0026BC44
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityCharacter character = abilityObject as IAbilityCharacter;
			if (character != null)
			{
				this.ApplyAfflictionToCharacter(character.Character);
			}
		}

		// Token: 0x060046BA RID: 18106 RVA: 0x0026DA68 File Offset: 0x0026BC68
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

		// Token: 0x0400249F RID: 9375
		private readonly Identifier afflictionId;

		// Token: 0x040024A0 RID: 9376
		private readonly float strength;

		// Token: 0x040024A1 RID: 9377
		private readonly Identifier multiplyStrengthBySkill;

		// Token: 0x040024A2 RID: 9378
		private readonly bool setValue;
	}
}
