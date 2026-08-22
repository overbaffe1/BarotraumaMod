using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003C1 RID: 961
	internal class AbilityConditionMission : AbilityConditionData
	{
		// Token: 0x06004619 RID: 17945 RVA: 0x0026B465 File Offset: 0x00269665
		public AbilityConditionMission(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.missionType = conditionElement.GetAttributeIdentifierImmutableHashSet("missiontype", ImmutableHashSet<Identifier>.Empty, true);
			this.isAffiliated = conditionElement.GetAttributeBool("isaffiliated", false);
		}

		// Token: 0x0600461A RID: 17946 RVA: 0x0026B498 File Offset: 0x00269698
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			AbilityConditionMission.<>c__DisplayClass3_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			IAbilityMission abilityMission = abilityObject as IAbilityMission;
			if (abilityMission != null)
			{
				CS$<>8__locals1.mission = abilityMission.Mission;
				if (CS$<>8__locals1.mission != null)
				{
					if (!this.isAffiliated)
					{
						return this.<MatchesConditionSpecific>g__CheckMissionType|3_1(ref CS$<>8__locals1);
					}
					GameSession gameSession = GameMain.GameSession;
					IReadOnlyList<Faction> factions;
					if (gameSession == null)
					{
						factions = null;
					}
					else
					{
						CampaignMode campaign = gameSession.Campaign;
						factions = ((campaign != null) ? campaign.Factions : null);
					}
					AbilityConditionMission.<>c__DisplayClass3_1 CS$<>8__locals2;
					CS$<>8__locals2.factions = factions;
					if (CS$<>8__locals2.factions == null)
					{
						return false;
					}
					foreach (MissionPrefab.ReputationReward reputationReward in CS$<>8__locals1.mission.ReputationRewards)
					{
						if (reputationReward.Amount > 0f)
						{
							Faction faction = this.<MatchesConditionSpecific>g__GetMatchingFaction|3_0(reputationReward.FactionIdentifier, ref CS$<>8__locals1, ref CS$<>8__locals2);
							if (faction != null && Faction.GetPlayerAffiliationStatus(faction) == FactionAffiliation.Positive)
							{
								return this.<MatchesConditionSpecific>g__CheckMissionType|3_1(ref CS$<>8__locals1);
							}
						}
					}
					return false;
				}
			}
			base.LogAbilityConditionError(abilityObject, typeof(IAbilityMission));
			return false;
		}

		// Token: 0x0600461B RID: 17947 RVA: 0x0026B5A8 File Offset: 0x002697A8
		[CompilerGenerated]
		private Faction <MatchesConditionSpecific>g__GetMatchingFaction|3_0(Identifier factionIdentifier, ref AbilityConditionMission.<>c__DisplayClass3_0 A_2, ref AbilityConditionMission.<>c__DisplayClass3_1 A_3)
		{
			if (!(factionIdentifier == "location"))
			{
				return A_3.factions.FirstOrDefault((Faction f) => factionIdentifier == f.Prefab.Identifier);
			}
			Location originLocation = A_2.mission.OriginLocation;
			if (originLocation == null)
			{
				return null;
			}
			return originLocation.Faction;
		}

		// Token: 0x0600461C RID: 17948 RVA: 0x0026B602 File Offset: 0x00269802
		[CompilerGenerated]
		private bool <MatchesConditionSpecific>g__CheckMissionType|3_1(ref AbilityConditionMission.<>c__DisplayClass3_0 A_1)
		{
			return this.missionType.IsEmpty || this.missionType.Contains(A_1.mission.Prefab.Type);
		}

		// Token: 0x04002458 RID: 9304
		private readonly ImmutableHashSet<Identifier> missionType;

		// Token: 0x04002459 RID: 9305
		private readonly bool isAffiliated;
	}
}
