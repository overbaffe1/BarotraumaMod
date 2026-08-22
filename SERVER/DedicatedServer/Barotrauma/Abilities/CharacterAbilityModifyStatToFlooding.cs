using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000348 RID: 840
	internal class CharacterAbilityModifyStatToFlooding : CharacterAbility
	{
		// Token: 0x17000E39 RID: 3641
		// (get) Token: 0x060032BD RID: 12989 RVA: 0x00157020 File Offset: 0x00155220
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032BE RID: 12990 RVA: 0x00157024 File Offset: 0x00155224
		public CharacterAbilityModifyStatToFlooding(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.maxValue = abilityElement.GetAttributeFloat("maxvalue", 0f);
		}

		// Token: 0x060032BF RID: 12991 RVA: 0x00157078 File Offset: 0x00155278
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			base.Character.ChangeStat(this.statType, -this.lastValue);
			if (conditionsMatched && base.Character.IsInFriendlySub)
			{
				float waterVolume = 0f;
				float totalVolume = 0f;
				foreach (Hull hull in Hull.HullList)
				{
					if (hull.Submarine == base.Character.Submarine)
					{
						waterVolume += hull.WaterVolume;
						totalVolume += hull.Volume;
					}
				}
				this.lastValue = ((totalVolume == 0f) ? 1f : (waterVolume / totalVolume)) * this.maxValue;
				base.Character.ChangeStat(this.statType, this.lastValue);
				return;
			}
			this.lastValue = 0f;
		}

		// Token: 0x04001918 RID: 6424
		private readonly StatTypes statType;

		// Token: 0x04001919 RID: 6425
		private readonly float maxValue;

		// Token: 0x0400191A RID: 6426
		private float lastValue;
	}
}
