using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003DD RID: 989
	internal class AbilityConditionNoCrewDied : AbilityConditionDataless
	{
		// Token: 0x0600465B RID: 18011 RVA: 0x0026C4E8 File Offset: 0x0026A6E8
		public AbilityConditionNoCrewDied(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.assistantsDontCount = conditionElement.GetAttributeBool("assistantsDontCount", true);
		}

		// Token: 0x0600465C RID: 18012 RVA: 0x0026C504 File Offset: 0x0026A704
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

		// Token: 0x0400247A RID: 9338
		public bool assistantsDontCount;
	}
}
