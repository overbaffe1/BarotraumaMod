using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200034B RID: 843
	[NetworkSerialize(8)]
	public readonly struct RoundStartWarningData : INetSerializableStruct, IEquatable<RoundStartWarningData>
	{
		// Token: 0x0600422A RID: 16938 RVA: 0x00249452 File Offset: 0x00247652
		public RoundStartWarningData(float RoundStartsAnywaysTimeInSeconds, string Team1Sub, ImmutableArray<uint> Team1IncompatiblePerks, string Team2Sub, ImmutableArray<uint> Team2IncompatiblePerks)
		{
			this.RoundStartsAnywaysTimeInSeconds = RoundStartsAnywaysTimeInSeconds;
			this.Team1Sub = Team1Sub;
			this.Team1IncompatiblePerks = Team1IncompatiblePerks;
			this.Team2Sub = Team2Sub;
			this.Team2IncompatiblePerks = Team2IncompatiblePerks;
		}

		// Token: 0x17001186 RID: 4486
		// (get) Token: 0x0600422B RID: 16939 RVA: 0x00249479 File Offset: 0x00247679
		// (set) Token: 0x0600422C RID: 16940 RVA: 0x00249481 File Offset: 0x00247681
		public float RoundStartsAnywaysTimeInSeconds { get; set; }

		// Token: 0x17001187 RID: 4487
		// (get) Token: 0x0600422D RID: 16941 RVA: 0x0024948A File Offset: 0x0024768A
		// (set) Token: 0x0600422E RID: 16942 RVA: 0x00249492 File Offset: 0x00247692
		public string Team1Sub { get; set; }

		// Token: 0x17001188 RID: 4488
		// (get) Token: 0x0600422F RID: 16943 RVA: 0x0024949B File Offset: 0x0024769B
		// (set) Token: 0x06004230 RID: 16944 RVA: 0x002494A3 File Offset: 0x002476A3
		public ImmutableArray<uint> Team1IncompatiblePerks { get; set; }

		// Token: 0x17001189 RID: 4489
		// (get) Token: 0x06004231 RID: 16945 RVA: 0x002494AC File Offset: 0x002476AC
		// (set) Token: 0x06004232 RID: 16946 RVA: 0x002494B4 File Offset: 0x002476B4
		public string Team2Sub { get; set; }

		// Token: 0x1700118A RID: 4490
		// (get) Token: 0x06004233 RID: 16947 RVA: 0x002494BD File Offset: 0x002476BD
		// (set) Token: 0x06004234 RID: 16948 RVA: 0x002494C5 File Offset: 0x002476C5
		public ImmutableArray<uint> Team2IncompatiblePerks { get; set; }

		// Token: 0x06004235 RID: 16949 RVA: 0x002494D0 File Offset: 0x002476D0
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

		// Token: 0x06004236 RID: 16950 RVA: 0x0024951C File Offset: 0x0024771C
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

		// Token: 0x06004237 RID: 16951 RVA: 0x002495D1 File Offset: 0x002477D1
		[CompilerGenerated]
		public static bool operator !=(RoundStartWarningData left, RoundStartWarningData right)
		{
			return !(left == right);
		}

		// Token: 0x06004238 RID: 16952 RVA: 0x002495DD File Offset: 0x002477DD
		[CompilerGenerated]
		public static bool operator ==(RoundStartWarningData left, RoundStartWarningData right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004239 RID: 16953 RVA: 0x002495E8 File Offset: 0x002477E8
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((EqualityComparer<float>.Default.GetHashCode(this.<RoundStartsAnywaysTimeInSeconds>k__BackingField) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Team1Sub>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<uint>>.Default.GetHashCode(this.<Team1IncompatiblePerks>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Team2Sub>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<uint>>.Default.GetHashCode(this.<Team2IncompatiblePerks>k__BackingField);
		}

		// Token: 0x0600423A RID: 16954 RVA: 0x00249661 File Offset: 0x00247861
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is RoundStartWarningData && this.Equals((RoundStartWarningData)obj);
		}

		// Token: 0x0600423B RID: 16955 RVA: 0x0024967C File Offset: 0x0024787C
		[CompilerGenerated]
		public bool Equals(RoundStartWarningData other)
		{
			return EqualityComparer<float>.Default.Equals(this.<RoundStartsAnywaysTimeInSeconds>k__BackingField, other.<RoundStartsAnywaysTimeInSeconds>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Team1Sub>k__BackingField, other.<Team1Sub>k__BackingField) && EqualityComparer<ImmutableArray<uint>>.Default.Equals(this.<Team1IncompatiblePerks>k__BackingField, other.<Team1IncompatiblePerks>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Team2Sub>k__BackingField, other.<Team2Sub>k__BackingField) && EqualityComparer<ImmutableArray<uint>>.Default.Equals(this.<Team2IncompatiblePerks>k__BackingField, other.<Team2IncompatiblePerks>k__BackingField);
		}

		// Token: 0x0600423C RID: 16956 RVA: 0x00249701 File Offset: 0x00247901
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
