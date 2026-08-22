using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x0200034A RID: 842
	internal class CharacterAbilityModifyStatToSkill : CharacterAbility
	{
		// Token: 0x17000E3B RID: 3643
		// (get) Token: 0x060032C3 RID: 12995 RVA: 0x0015724D File Offset: 0x0015544D
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032C4 RID: 12996 RVA: 0x00157250 File Offset: 0x00155450
		public CharacterAbilityModifyStatToSkill(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.statType = CharacterAbilityGroup.ParseStatType(abilityElement.GetAttributeString("stattype", ""), base.CharacterTalent.DebugIdentifier);
			this.maxValue = abilityElement.GetAttributeFloat("maxvalue", 0f);
			this.skillIdentifier = abilityElement.GetAttributeIdentifier("skillidentifier", Identifier.Empty);
			this.useAll = (this.skillIdentifier == "all");
		}

		// Token: 0x060032C5 RID: 12997 RVA: 0x001572D0 File Offset: 0x001554D0
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

		// Token: 0x0400191F RID: 6431
		private readonly StatTypes statType;

		// Token: 0x04001920 RID: 6432
		private readonly float maxValue;

		// Token: 0x04001921 RID: 6433
		private readonly Identifier skillIdentifier;

		// Token: 0x04001922 RID: 6434
		private readonly bool useAll;

		// Token: 0x04001923 RID: 6435
		private float lastValue;
	}
}
