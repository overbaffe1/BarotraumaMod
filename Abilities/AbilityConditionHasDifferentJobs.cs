using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x020003CE RID: 974
	internal class AbilityConditionHasDifferentJobs : AbilityConditionDataless
	{
		// Token: 0x06004639 RID: 17977 RVA: 0x0026BBC3 File Offset: 0x00269DC3
		public AbilityConditionHasDifferentJobs(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.amount = conditionElement.GetAttributeInt("amount", 0);
		}

		// Token: 0x0600463A RID: 17978 RVA: 0x0026BBE0 File Offset: 0x00269DE0
		protected override bool MatchesConditionSpecific()
		{
			IEnumerable<Character> crewMembers = Character.GetFriendlyCrew(this.character);
			int differentCrewAmount = crewMembers.Select(delegate(Character c)
			{
				Job job = c.Info.Job;
				if (job == null)
				{
					return null;
				}
				return new Identifier?(job.Prefab.Identifier);
			}).Distinct<Identifier?>().Count<Identifier?>();
			return differentCrewAmount >= this.amount;
		}

		// Token: 0x04002466 RID: 9318
		private readonly int amount;
	}
}
