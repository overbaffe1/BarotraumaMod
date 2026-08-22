using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000415 RID: 1045
	internal sealed class CharacterAbilityReplaceAffliction : CharacterAbility
	{
		// Token: 0x0600470E RID: 18190 RVA: 0x0026F71C File Offset: 0x0026D91C
		[NullableContext(1)]
		public CharacterAbilityReplaceAffliction(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.afflictionId = abilityElement.GetAttributeIdentifier("afflictionid", abilityElement.GetAttributeIdentifier("affliction", Identifier.Empty));
			this.newAfflictionId = abilityElement.GetAttributeIdentifier("newafflictionid", abilityElement.GetAttributeIdentifier("newaffliction", Identifier.Empty));
			this.strengthMultiplier = abilityElement.GetAttributeFloat("strengthmultiplier", 1f);
			if (this.afflictionId.IsEmpty)
			{
				DebugConsole.ThrowError("Error in CharacterAbilityReplaceAffliction - affliction identifier not set.", null, null, false, false);
			}
		}

		// Token: 0x0600470F RID: 18191 RVA: 0x0026F7A4 File Offset: 0x0026D9A4
		protected override void ApplyEffect()
		{
			Affliction affliction = base.Character.CharacterHealth.GetAffliction(this.afflictionId, true);
			if (affliction != null)
			{
				float afflictionStrength = affliction.Strength;
				Limb limb = base.Character.CharacterHealth.GetAfflictionLimb(affliction);
				base.Character.CharacterHealth.ReduceAfflictionOnAllLimbs(affliction.Identifier, afflictionStrength, null, null);
				AfflictionPrefab newAfflictionPrefab;
				if (!this.newAfflictionId.IsEmpty && AfflictionPrefab.Prefabs.TryGet(this.newAfflictionId, out newAfflictionPrefab))
				{
					base.Character.CharacterHealth.ApplyAffliction(limb, newAfflictionPrefab.Instantiate(afflictionStrength * this.strengthMultiplier, null), true, false, true);
				}
			}
		}

		// Token: 0x06004710 RID: 18192 RVA: 0x0026F84B File Offset: 0x0026DA4B
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched)
			{
				this.ApplyEffect();
			}
		}

		// Token: 0x040024EC RID: 9452
		private readonly Identifier afflictionId;

		// Token: 0x040024ED RID: 9453
		private readonly Identifier newAfflictionId;

		// Token: 0x040024EE RID: 9454
		private readonly float strengthMultiplier;
	}
}
