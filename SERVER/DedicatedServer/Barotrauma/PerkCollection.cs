using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.PerkBehaviors;

namespace Barotrauma
{
	// Token: 0x020001E9 RID: 489
	internal readonly struct PerkCollection : IEquatable<PerkCollection>
	{
		// Token: 0x06002308 RID: 8968 RVA: 0x000E9D3E File Offset: 0x000E7F3E
		public PerkCollection([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<DisembarkPerkPrefab> Team1Perks, [Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<DisembarkPerkPrefab> Team2Perks)
		{
			this.Team1Perks = Team1Perks;
			this.Team2Perks = Team2Perks;
		}

		// Token: 0x17000A13 RID: 2579
		// (get) Token: 0x06002309 RID: 8969 RVA: 0x000E9D4E File Offset: 0x000E7F4E
		// (set) Token: 0x0600230A RID: 8970 RVA: 0x000E9D56 File Offset: 0x000E7F56
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<DisembarkPerkPrefab> Team1Perks { [return: Nullable(new byte[]
		{
			0,
			1
		})] get; [param: Nullable(new byte[]
		{
			0,
			1
		})] set; }

		// Token: 0x17000A14 RID: 2580
		// (get) Token: 0x0600230B RID: 8971 RVA: 0x000E9D5F File Offset: 0x000E7F5F
		// (set) Token: 0x0600230C RID: 8972 RVA: 0x000E9D67 File Offset: 0x000E7F67
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<DisembarkPerkPrefab> Team2Perks { [return: Nullable(new byte[]
		{
			0,
			1
		})] get; [param: Nullable(new byte[]
		{
			0,
			1
		})] set; }

		// Token: 0x0600230D RID: 8973 RVA: 0x000E9D70 File Offset: 0x000E7F70
		[NullableContext(1)]
		public void ApplyAll(IReadOnlyCollection<Character> team1Characters, IReadOnlyCollection<Character> team2Characters)
		{
			bool anyMissionDoesNotLoadSubs = GameMain.GameSession.Missions.Any((Mission m) => !m.Prefab.LoadSubmarines);
			foreach (DisembarkPerkPrefab team1Perk in this.Team1Perks)
			{
				GameAnalyticsManager.AddDesignEvent("DisembarkPerk:" + team1Perk.Identifier.ToString());
				foreach (PerkBase behavior in team1Perk.PerkBehaviors)
				{
					if (!anyMissionDoesNotLoadSubs || behavior.CanApplyWithoutSubmarine())
					{
						behavior.ApplyOnRoundStart(team1Characters, Submarine.MainSubs[0]);
					}
				}
			}
			if (Submarine.MainSubs[1] != null && GameMain.GameSession.GameMode is PvPMode)
			{
				foreach (DisembarkPerkPrefab team2Perk in this.Team2Perks)
				{
					GameAnalyticsManager.AddDesignEvent("DisembarkPerk:" + team2Perk.Identifier.ToString());
					foreach (PerkBase behavior2 in team2Perk.PerkBehaviors)
					{
						if (!anyMissionDoesNotLoadSubs || behavior2.CanApplyWithoutSubmarine())
						{
							behavior2.ApplyOnRoundStart(team2Characters, Submarine.MainSubs[1]);
						}
					}
				}
			}
		}

		// Token: 0x0600230E RID: 8974 RVA: 0x000E9ED4 File Offset: 0x000E80D4
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("PerkCollection");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600230F RID: 8975 RVA: 0x000E9F20 File Offset: 0x000E8120
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Team1Perks = ");
			builder.Append(this.Team1Perks.ToString());
			builder.Append(", Team2Perks = ");
			builder.Append(this.Team2Perks.ToString());
			return true;
		}

		// Token: 0x06002310 RID: 8976 RVA: 0x000E9F7C File Offset: 0x000E817C
		[CompilerGenerated]
		public static bool operator !=(PerkCollection left, PerkCollection right)
		{
			return !(left == right);
		}

		// Token: 0x06002311 RID: 8977 RVA: 0x000E9F88 File Offset: 0x000E8188
		[CompilerGenerated]
		public static bool operator ==(PerkCollection left, PerkCollection right)
		{
			return left.Equals(right);
		}

		// Token: 0x06002312 RID: 8978 RVA: 0x000E9F92 File Offset: 0x000E8192
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<DisembarkPerkPrefab>>.Default.GetHashCode(this.<Team1Perks>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<DisembarkPerkPrefab>>.Default.GetHashCode(this.<Team2Perks>k__BackingField);
		}

		// Token: 0x06002313 RID: 8979 RVA: 0x000E9FBB File Offset: 0x000E81BB
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is PerkCollection && this.Equals((PerkCollection)obj);
		}

		// Token: 0x06002314 RID: 8980 RVA: 0x000E9FD3 File Offset: 0x000E81D3
		[CompilerGenerated]
		public bool Equals(PerkCollection other)
		{
			return EqualityComparer<ImmutableArray<DisembarkPerkPrefab>>.Default.Equals(this.<Team1Perks>k__BackingField, other.<Team1Perks>k__BackingField) && EqualityComparer<ImmutableArray<DisembarkPerkPrefab>>.Default.Equals(this.<Team2Perks>k__BackingField, other.<Team2Perks>k__BackingField);
		}

		// Token: 0x06002315 RID: 8981 RVA: 0x000EA005 File Offset: 0x000E8205
		[CompilerGenerated]
		public void Deconstruct([Nullable(new byte[]
		{
			0,
			1
		})] out ImmutableArray<DisembarkPerkPrefab> Team1Perks, [Nullable(new byte[]
		{
			0,
			1
		})] out ImmutableArray<DisembarkPerkPrefab> Team2Perks)
		{
			Team1Perks = this.Team1Perks;
			Team2Perks = this.Team2Perks;
		}

		// Token: 0x040010FD RID: 4349
		public static readonly PerkCollection Empty = new PerkCollection(ImmutableArray<DisembarkPerkPrefab>.Empty, ImmutableArray<DisembarkPerkPrefab>.Empty);
	}
}
