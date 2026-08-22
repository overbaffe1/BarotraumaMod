using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000421 RID: 1057
	internal class CharacterAbilityPsychoClown : CharacterAbility
	{
		// Token: 0x17001232 RID: 4658
		// (get) Token: 0x06004734 RID: 18228 RVA: 0x002702DD File Offset: 0x0026E4DD
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06004735 RID: 18229 RVA: 0x002702E0 File Offset: 0x0026E4E0
		public CharacterAbilityPsychoClown(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.maxValue = abilityElement.GetAttributeFloat("maxValue", 0f);
			this.minValue = abilityElement.GetAttributeFloat("minValue", 0f);
			this.afflictionIdentifier = abilityElement.GetAttributeString("afflictionIdentifier", "");
		}

		// Token: 0x06004736 RID: 18230 RVA: 0x00270360 File Offset: 0x0026E560
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

		// Token: 0x04002507 RID: 9479
		private readonly StatTypes statType;

		// Token: 0x04002508 RID: 9480
		private readonly float minValue;

		// Token: 0x04002509 RID: 9481
		private readonly float maxValue;

		// Token: 0x0400250A RID: 9482
		private readonly string afflictionIdentifier;

		// Token: 0x0400250B RID: 9483
		private float lastValue;
	}
}
