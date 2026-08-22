using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x0200034F RID: 847
	internal sealed class CharacterAbilityReplaceAffliction : CharacterAbility
	{
		// Token: 0x060032D4 RID: 13012 RVA: 0x0015789C File Offset: 0x00155A9C
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

		// Token: 0x060032D5 RID: 13013 RVA: 0x00157924 File Offset: 0x00155B24
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

		// Token: 0x060032D6 RID: 13014 RVA: 0x001579CB File Offset: 0x00155BCB
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched)
			{
				this.ApplyEffect();
			}
		}

		// Token: 0x0400192B RID: 6443
		private readonly Identifier afflictionId;

		// Token: 0x0400192C RID: 6444
		private readonly Identifier newAfflictionId;

		// Token: 0x0400192D RID: 6445
		private readonly float strengthMultiplier;
	}
}
