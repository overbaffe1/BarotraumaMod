using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.PerkBehaviors;

namespace Barotrauma
{
	// Token: 0x020002D4 RID: 724
	internal readonly struct PerkCollection : IEquatable<PerkCollection>
	{
		// Token: 0x06003CFA RID: 15610 RVA: 0x0022CCBA File Offset: 0x0022AEBA
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

		// Token: 0x1700101A RID: 4122
		// (get) Token: 0x06003CFB RID: 15611 RVA: 0x0022CCCA File Offset: 0x0022AECA
		// (set) Token: 0x06003CFC RID: 15612 RVA: 0x0022CCD2 File Offset: 0x0022AED2
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

		// Token: 0x1700101B RID: 4123
		// (get) Token: 0x06003CFD RID: 15613 RVA: 0x0022CCDB File Offset: 0x0022AEDB
		// (set) Token: 0x06003CFE RID: 15614 RVA: 0x0022CCE3 File Offset: 0x0022AEE3
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

		// Token: 0x06003CFF RID: 15615 RVA: 0x0022CCEC File Offset: 0x0022AEEC
		[NullableContext(1)]
		public void ApplyAll(IReadOnlyCollection<Character> team1Characters, IReadOnlyCollection<Character> team2Characters)
		{
			bool anyMissionDoesNotLoadSubs = GameMain.GameSession.Missions.Any((Mission m) => !m.Prefab.LoadSubmarines);
			foreach (DisembarkPerkPrefab team1Perk in this.Team1Perks)
			{
				GameAnalyticsManager.AddDesignEvent("DisembarkPerk:" + team1Perk.Identifier.ToString());
				foreach (PerkBase behavior in team1Perk.PerkBehaviors)
				{
					if ((!anyMissionDoesNotLoadSubs || behavior.CanApplyWithoutSubmarine()) && behavior.Simulation != PerkSimulation.ServerOnly)
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
						if ((!anyMissionDoesNotLoadSubs || behavior2.CanApplyWithoutSubmarine()) && behavior2.Simulation != PerkSimulation.ServerOnly)
						{
							behavior2.ApplyOnRoundStart(team2Characters, Submarine.MainSubs[1]);
						}
					}
				}
			}
		}

		// Token: 0x06003D00 RID: 15616 RVA: 0x0022CE64 File Offset: 0x0022B064
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

		// Token: 0x06003D01 RID: 15617 RVA: 0x0022CEB0 File Offset: 0x0022B0B0
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Team1Perks = ");
			builder.Append(this.Team1Perks.ToString());
			builder.Append(", Team2Perks = ");
			builder.Append(this.Team2Perks.ToString());
			return true;
		}

		// Token: 0x06003D02 RID: 15618 RVA: 0x0022CF0C File Offset: 0x0022B10C
		[CompilerGenerated]
		public static bool operator !=(PerkCollection left, PerkCollection right)
		{
			return !(left == right);
		}

		// Token: 0x06003D03 RID: 15619 RVA: 0x0022CF18 File Offset: 0x0022B118
		[CompilerGenerated]
		public static bool operator ==(PerkCollection left, PerkCollection right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003D04 RID: 15620 RVA: 0x0022CF22 File Offset: 0x0022B122
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<DisembarkPerkPrefab>>.Default.GetHashCode(this.<Team1Perks>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<DisembarkPerkPrefab>>.Default.GetHashCode(this.<Team2Perks>k__BackingField);
		}

		// Token: 0x06003D05 RID: 15621 RVA: 0x0022CF4B File Offset: 0x0022B14B
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is PerkCollection && this.Equals((PerkCollection)obj);
		}

		// Token: 0x06003D06 RID: 15622 RVA: 0x0022CF63 File Offset: 0x0022B163
		[CompilerGenerated]
		public bool Equals(PerkCollection other)
		{
			return EqualityComparer<ImmutableArray<DisembarkPerkPrefab>>.Default.Equals(this.<Team1Perks>k__BackingField, other.<Team1Perks>k__BackingField) && EqualityComparer<ImmutableArray<DisembarkPerkPrefab>>.Default.Equals(this.<Team2Perks>k__BackingField, other.<Team2Perks>k__BackingField);
		}

		// Token: 0x06003D07 RID: 15623 RVA: 0x0022CF95 File Offset: 0x0022B195
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

		// Token: 0x04001F9F RID: 8095
		public static readonly PerkCollection Empty = new PerkCollection(ImmutableArray<DisembarkPerkPrefab>.Empty, ImmutableArray<DisembarkPerkPrefab>.Empty);
	}
}
