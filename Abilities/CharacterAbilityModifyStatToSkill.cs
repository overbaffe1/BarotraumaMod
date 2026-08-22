using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x02000410 RID: 1040
	internal class CharacterAbilityModifyStatToSkill : CharacterAbility
	{
		// Token: 0x1700122B RID: 4651
		// (get) Token: 0x060046FD RID: 18173 RVA: 0x0026F0CD File Offset: 0x0026D2CD
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060046FE RID: 18174 RVA: 0x0026F0D0 File Offset: 0x0026D2D0
		public CharacterAbilityModifyStatToSkill(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.maxValue = abilityElement.GetAttributeFloat("maxvalue", 0f);
			this.skillIdentifier = abilityElement.GetAttributeIdentifier("skillidentifier", Identifier.Empty);
			this.useAll = (this.skillIdentifier == "all");
		}

		// Token: 0x060046FF RID: 18175 RVA: 0x0026F150 File Offset: 0x0026D350
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			base.Character.ChangeStat(this.statType, -this.lastValue);
			if (conditionsMatched)
			{
				float skillTotal = 0f;
				if (this.useAll)
				{
					CharacterInfo info = base.Character.Info;
					if (((info != null) ? info.Job : null) != null)
					{
						IEnumerable<Skill> skills = base.Character.Info.Job.GetSkills();
						foreach (Skill skill in skills)
						{
							skillTotal += base.Character.GetSkillLevel(skill.Identifier);
						}
						skillTotal /= (float)skills.Count<Skill>();
						goto IL_B1;
					}
				}
				skillTotal = base.Character.GetSkillLevel(this.skillIdentifier);
				IL_B1:
				this.lastValue = skillTotal / 100f * this.maxValue;
				base.Character.ChangeStat(this.statType, this.lastValue);
				return;
			}
			this.lastValue = 0f;
		}

		// Token: 0x040024E0 RID: 9440
		private readonly StatTypes statType;

		// Token: 0x040024E1 RID: 9441
		private readonly float maxValue;

		// Token: 0x040024E2 RID: 9442
		private readonly Identifier skillIdentifier;

		// Token: 0x040024E3 RID: 9443
		private readonly bool useAll;

		// Token: 0x040024E4 RID: 9444
		private float lastValue;
	}
}
