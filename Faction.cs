using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002C2 RID: 706
	[NullableContext(1)]
	[Nullable(0)]
	internal class Faction
	{
		// Token: 0x17000FE3 RID: 4067
		// (get) Token: 0x06003C7B RID: 15483 RVA: 0x0022B1B9 File Offset: 0x002293B9
		public Reputation Reputation { get; }

		// Token: 0x17000FE4 RID: 4068
		// (get) Token: 0x06003C7C RID: 15484 RVA: 0x0022B1C1 File Offset: 0x002293C1
		public FactionPrefab Prefab { get; }

		// Token: 0x06003C7D RID: 15485 RVA: 0x0022B1C9 File Offset: 0x002293C9
		public Faction([Nullable(2)] CampaignMetadata metadata, FactionPrefab prefab)
		{
			this.Prefab = prefab;
			this.Reputation = new Reputation(metadata, this, prefab.MinReputation, prefab.MaxReputation, prefab.InitialReputation);
		}

		// Token: 0x06003C7E RID: 15486 RVA: 0x0022B1F8 File Offset: 0x002293F8
		public static FactionAffiliation GetPlayerAffiliationStatus(Faction faction)
		{
			GameSession gameSession = GameMain.GameSession;
			IReadOnlyList<Faction> readOnlyList;
			if (gameSession == null)
			{
				readOnlyList = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				readOnlyList = ((campaign != null) ? campaign.Factions : null);
			}
			IReadOnlyList<Faction> factions = readOnlyList;
			if (factions == null)
			{
				return FactionAffiliation.Neutral;
			}
			bool isHighest = true;
			foreach (Faction otherFaction in factions)
			{
				if (otherFaction != faction && otherFaction.Reputation.Value >= faction.Reputation.Value)
				{
					isHighest = false;
					break;
				}
			}
			if (!isHighest)
			{
				return FactionAffiliation.Negative;
			}
			return FactionAffiliation.Positive;
		}

		// Token: 0x06003C7F RID: 15487 RVA: 0x0022B284 File Offset: 0x00229484
		public override string ToString()
		{
			string str = base.ToString();
			string str2 = " (";
			FactionPrefab prefab = this.Prefab;
			return str + str2 + (((prefab != null) ? prefab.Identifier.ToString() : null) ?? "null") + ")";
		}
	}
}
