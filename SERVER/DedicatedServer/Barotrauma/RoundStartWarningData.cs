using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000279 RID: 633
	[NetworkSerialize(8)]
	public readonly struct RoundStartWarningData : INetSerializableStruct, IEquatable<RoundStartWarningData>
	{
		// Token: 0x06002D0F RID: 11535 RVA: 0x00129318 File Offset: 0x00127518
		public RoundStartWarningData(float RoundStartsAnywaysTimeInSeconds, string Team1Sub, ImmutableArray<uint> Team1IncompatiblePerks, string Team2Sub, ImmutableArray<uint> Team2IncompatiblePerks)
		{
			this.RoundStartsAnywaysTimeInSeconds = RoundStartsAnywaysTimeInSeconds;
			this.Team1Sub = Team1Sub;
			this.Team1IncompatiblePerks = Team1IncompatiblePerks;
			this.Team2Sub = Team2Sub;
			this.Team2IncompatiblePerks = Team2IncompatiblePerks;
		}

		// Token: 0x17000D61 RID: 3425
		// (get) Token: 0x06002D10 RID: 11536 RVA: 0x0012933F File Offset: 0x0012753F
		// (set) Token: 0x06002D11 RID: 11537 RVA: 0x00129347 File Offset: 0x00127547
		public float RoundStartsAnywaysTimeInSeconds { get; set; }

		// Token: 0x17000D62 RID: 3426
		// (get) Token: 0x06002D12 RID: 11538 RVA: 0x00129350 File Offset: 0x00127550
		// (set) Token: 0x06002D13 RID: 11539 RVA: 0x00129358 File Offset: 0x00127558
		public string Team1Sub { get; set; }

		// Token: 0x17000D63 RID: 3427
		// (get) Token: 0x06002D14 RID: 11540 RVA: 0x00129361 File Offset: 0x00127561
		// (set) Token: 0x06002D15 RID: 11541 RVA: 0x00129369 File Offset: 0x00127569
		public ImmutableArray<uint> Team1IncompatiblePerks { get; set; }

		// Token: 0x17000D64 RID: 3428
		// (get) Token: 0x06002D16 RID: 11542 RVA: 0x00129372 File Offset: 0x00127572
		// (set) Token: 0x06002D17 RID: 11543 RVA: 0x0012937A File Offset: 0x0012757A
		public string Team2Sub { get; set; }

		// Token: 0x17000D65 RID: 3429
		// (get) Token: 0x06002D18 RID: 11544 RVA: 0x00129383 File Offset: 0x00127583
		// (set) Token: 0x06002D19 RID: 11545 RVA: 0x0012938B File Offset: 0x0012758B
		public ImmutableArray<uint> Team2IncompatiblePerks { get; set; }

		// Token: 0x06002D1A RID: 11546 RVA: 0x00129394 File Offset: 0x00127594
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("RoundStartWarningData");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06002D1B RID: 11547 RVA: 0x001293E0 File Offset: 0x001275E0
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("RoundStartsAnywaysTimeInSeconds = ");
			builder.Append(this.RoundStartsAnywaysTimeInSeconds.ToString());
			builder.Append(", Team1Sub = ");
			builder.Append(this.Team1Sub);
			builder.Append(", Team1IncompatiblePerks = ");
			builder.Append(this.Team1IncompatiblePerks.ToString());
			builder.Append(", Team2Sub = ");
			builder.Append(this.Team2Sub);
			builder.Append(", Team2IncompatiblePerks = ");
			builder.Append(this.Team2IncompatiblePerks.ToString());
			return true;
		}

		// Token: 0x06002D1C RID: 11548 RVA: 0x00129495 File Offset: 0x00127695
		[CompilerGenerated]
		public static bool operator !=(RoundStartWarningData left, RoundStartWarningData right)
		{
			return !(left == right);
		}

		// Token: 0x06002D1D RID: 11549 RVA: 0x001294A1 File Offset: 0x001276A1
		[CompilerGenerated]
		public static bool operator ==(RoundStartWarningData left, RoundStartWarningData right)
		{
			return left.Equals(right);
		}

		// Token: 0x06002D1E RID: 11550 RVA: 0x001294AC File Offset: 0x001276AC
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((EqualityComparer<float>.Default.GetHashCode(this.<RoundStartsAnywaysTimeInSeconds>k__BackingField) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Team1Sub>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<uint>>.Default.GetHashCode(this.<Team1IncompatiblePerks>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Team2Sub>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<uint>>.Default.GetHashCode(this.<Team2IncompatiblePerks>k__BackingField);
		}

		// Token: 0x06002D1F RID: 11551 RVA: 0x00129525 File Offset: 0x00127725
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is RoundStartWarningData && this.Equals((RoundStartWarningData)obj);
		}

		// Token: 0x06002D20 RID: 11552 RVA: 0x00129540 File Offset: 0x00127740
		[CompilerGenerated]
		public bool Equals(RoundStartWarningData other)
		{
			return EqualityComparer<float>.Default.Equals(this.<RoundStartsAnywaysTimeInSeconds>k__BackingField, other.<RoundStartsAnywaysTimeInSeconds>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Team1Sub>k__BackingField, other.<Team1Sub>k__BackingField) && EqualityComparer<ImmutableArray<uint>>.Default.Equals(this.<Team1IncompatiblePerks>k__BackingField, other.<Team1IncompatiblePerks>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Team2Sub>k__BackingField, other.<Team2Sub>k__BackingField) && EqualityComparer<ImmutableArray<uint>>.Default.Equals(this.<Team2IncompatiblePerks>k__BackingField, other.<Team2IncompatiblePerks>k__BackingField);
		}

		// Token: 0x06002D21 RID: 11553 RVA: 0x001295C5 File Offset: 0x001277C5
		[CompilerGenerated]
		public void Deconstruct(out float RoundStartsAnywaysTimeInSeconds, out string Team1Sub, out ImmutableArray<uint> Team1IncompatiblePerks, out string Team2Sub, out ImmutableArray<uint> Team2IncompatiblePerks)
		{
			RoundStartsAnywaysTimeInSeconds = this.RoundStartsAnywaysTimeInSeconds;
			Team1Sub = this.Team1Sub;
			Team1IncompatiblePerks = this.Team1IncompatiblePerks;
			Team2Sub = this.Team2Sub;
			Team2IncompatiblePerks = this.Team2IncompatiblePerks;
		}
	}
}
