using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200035B RID: 859
	internal class CharacterAbilityPsychoClown : CharacterAbility
	{
		// Token: 0x17000E42 RID: 3650
		// (get) Token: 0x060032FA RID: 13050 RVA: 0x0015845D File Offset: 0x0015665D
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032FB RID: 13051 RVA: 0x00158460 File Offset: 0x00156660
		public CharacterAbilityPsychoClown(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.maxValue = abilityElement.GetAttributeFloat("maxValue", 0f);
			this.minValue = abilityElement.GetAttributeFloat("minValue", 0f);
			this.afflictionIdentifier = abilityElement.GetAttributeString("afflictionIdentifier", "");
		}

		// Token: 0x060032FC RID: 13052 RVA: 0x001584E0 File Offset: 0x001566E0
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			base.Character.ChangeStat(this.statType, -this.lastValue);
			if (conditionsMatched)
			{
				Affliction affliction = base.Character.CharacterHealth.GetAffliction(this.afflictionIdentifier, true);
				float afflictionStrength = 0f;
				if (affliction != null)
				{
					afflictionStrength = affliction.Strength / affliction.Prefab.MaxStrength;
				}
				this.lastValue = this.minValue + afflictionStrength * (this.maxValue - this.minValue);
				base.Character.ChangeStat(this.statType, this.lastValue);
				return;
			}
			this.lastValue = 0f;
		}

		// Token: 0x04001946 RID: 6470
		private readonly StatTypes statType;

		// Token: 0x04001947 RID: 6471
		private readonly float minValue;

		// Token: 0x04001948 RID: 6472
		private readonly float maxValue;

		// Token: 0x04001949 RID: 6473
		private readonly string afflictionIdentifier;

		// Token: 0x0400194A RID: 6474
		private float lastValue;
	}
}
