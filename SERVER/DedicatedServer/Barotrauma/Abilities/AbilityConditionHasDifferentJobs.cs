using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x02000308 RID: 776
	internal class AbilityConditionHasDifferentJobs : AbilityConditionDataless
	{
		// Token: 0x060031FF RID: 12799 RVA: 0x00153D43 File Offset: 0x00151F43
		public AbilityConditionHasDifferentJobs(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.amount = conditionElement.GetAttributeInt("amount", 0);
		}

		// Token: 0x06003200 RID: 12800 RVA: 0x00153D60 File Offset: 0x00151F60
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

		// Token: 0x040018A5 RID: 6309
		private readonly int amount;
	}
}
