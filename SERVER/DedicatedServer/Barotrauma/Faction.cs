using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001D5 RID: 469
	[NullableContext(1)]
	[Nullable(0)]
	internal class Faction
	{
		// Token: 0x170009DA RID: 2522
		// (get) Token: 0x06002282 RID: 8834 RVA: 0x000E83B8 File Offset: 0x000E65B8
		public Reputation Reputation { get; }

		// Token: 0x170009DB RID: 2523
		// (get) Token: 0x06002283 RID: 8835 RVA: 0x000E83C0 File Offset: 0x000E65C0
		public FactionPrefab Prefab { get; }

		// Token: 0x06002284 RID: 8836 RVA: 0x000E83C8 File Offset: 0x000E65C8
		public Faction([Nullable(2)] CampaignMetadata metadata, FactionPrefab prefab)
		{
			this.Prefab = prefab;
			this.Reputation = new Reputation(metadata, this, prefab.MinReputation, prefab.MaxReputation, prefab.InitialReputation);
		}

		// Token: 0x06002285 RID: 8837 RVA: 0x000E83F8 File Offset: 0x000E65F8
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

		// Token: 0x06002286 RID: 8838 RVA: 0x000E8484 File Offset: 0x000E6684
		public override string ToString()
		{
			string str = base.ToString();
			string str2 = " (";
			FactionPrefab prefab = this.Prefab;
			return str + str2 + (((prefab != null) ? prefab.Identifier.ToString() : null) ?? "null") + ")";
		}
	}
}
