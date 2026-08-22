using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000317 RID: 791
	internal class AbilityConditionNoCrewDied : AbilityConditionDataless
	{
		// Token: 0x06003221 RID: 12833 RVA: 0x00154668 File Offset: 0x00152868
		public AbilityConditionNoCrewDied(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.assistantsDontCount = conditionElement.GetAttributeBool("assistantsDontCount", true);
		}

		// Token: 0x06003222 RID: 12834 RVA: 0x00154684 File Offset: 0x00152884
		protected override bool MatchesConditionSpecific()
		{
			if (GameMain.GameSession == null)
			{
				return false;
			}
			foreach (Character deadCharacter in GameMain.GameSession.Casualties)
			{
				if (deadCharacter.TeamID == this.character.TeamID)
				{
					if (this.assistantsDontCount)
					{
						CharacterInfo info = deadCharacter.Info;
						Identifier? identifier;
						Identifier? identifier2;
						if (info == null)
						{
							identifier = null;
							identifier2 = identifier;
						}
						else
						{
							Job job = info.Job;
							if (job == null)
							{
								identifier = null;
								identifier2 = identifier;
							}
							else
							{
								identifier2 = new Identifier?(job.Prefab.Identifier);
							}
						}
						identifier = identifier2;
						if (identifier == "assistant")
						{
							continue;
						}
					}
					if (deadCharacter.CauseOfDeath != null && deadCharacter.CauseOfDeath.Type != CauseOfDeathType.Disconnected)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x040018B9 RID: 6329
		public bool assistantsDontCount;
	}
}
