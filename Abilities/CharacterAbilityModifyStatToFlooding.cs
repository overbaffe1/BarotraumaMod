using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200040E RID: 1038
	internal class CharacterAbilityModifyStatToFlooding : CharacterAbility
	{
		// Token: 0x17001229 RID: 4649
		// (get) Token: 0x060046F7 RID: 18167 RVA: 0x0026EEA0 File Offset: 0x0026D0A0
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060046F8 RID: 18168 RVA: 0x0026EEA4 File Offset: 0x0026D0A4
		public CharacterAbilityModifyStatToFlooding(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.maxValue = abilityElement.GetAttributeFloat("maxvalue", 0f);
		}

		// Token: 0x060046F9 RID: 18169 RVA: 0x0026EEF8 File Offset: 0x0026D0F8
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

		// Token: 0x040024D9 RID: 9433
		private readonly StatTypes statType;

		// Token: 0x040024DA RID: 9434
		private readonly float maxValue;

		// Token: 0x040024DB RID: 9435
		private float lastValue;
	}
}
