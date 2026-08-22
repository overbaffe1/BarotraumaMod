using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200015B RID: 347
	[NetworkSerialize(14)]
	internal readonly struct NetIncrementedStat : INetSerializableStruct, IEquatable<NetIncrementedStat>
	{
		// Token: 0x06002A97 RID: 10903 RVA: 0x001D6574 File Offset: 0x001D4774
		public NetIncrementedStat(AchievementStat Stat, float Amount)
		{
			this.Stat = Stat;
			this.Amount = Amount;
		}

		// Token: 0x17000ABD RID: 2749
		// (get) Token: 0x06002A98 RID: 10904 RVA: 0x001D6584 File Offset: 0x001D4784
		// (set) Token: 0x06002A99 RID: 10905 RVA: 0x001D658C File Offset: 0x001D478C
		public AchievementStat Stat { get; set; }

		// Token: 0x17000ABE RID: 2750
		// (get) Token: 0x06002A9A RID: 10906 RVA: 0x001D6595 File Offset: 0x001D4795
		// (set) Token: 0x06002A9B RID: 10907 RVA: 0x001D659D File Offset: 0x001D479D
		public float Amount { get; set; }

		// Token: 0x06002A9C RID: 10908 RVA: 0x001D65A8 File Offset: 0x001D47A8
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("NetIncrementedStat");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06002A9D RID: 10909 RVA: 0x001D65F4 File Offset: 0x001D47F4
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Stat = ");
			builder.Append(this.Stat.ToString());
			builder.Append(", Amount = ");
			builder.Append(this.Amount.ToString());
			return true;
		}

		// Token: 0x06002A9E RID: 10910 RVA: 0x001D6650 File Offset: 0x001D4850
		[CompilerGenerated]
		public static bool operator !=(NetIncrementedStat left, NetIncrementedStat right)
		{
			return !(left == right);
		}

		// Token: 0x06002A9F RID: 10911 RVA: 0x001D665C File Offset: 0x001D485C
		[CompilerGenerated]
		public static bool operator ==(NetIncrementedStat left, NetIncrementedStat right)
		{
			return left.Equals(right);
		}

		// Token: 0x06002AA0 RID: 10912 RVA: 0x001D6666 File Offset: 0x001D4866
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<AchievementStat>.Default.GetHashCode(this.<Stat>k__BackingField) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<Amount>k__BackingField);
		}

		// Token: 0x06002AA1 RID: 10913 RVA: 0x001D668F File Offset: 0x001D488F
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetIncrementedStat && this.Equals((NetIncrementedStat)obj);
		}

		// Token: 0x06002AA2 RID: 10914 RVA: 0x001D66A7 File Offset: 0x001D48A7
		[CompilerGenerated]
		public bool Equals(NetIncrementedStat other)
		{
			return EqualityComparer<AchievementStat>.Default.Equals(this.<Stat>k__BackingField, other.<Stat>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<Amount>k__BackingField, other.<Amount>k__BackingField);
		}

		// Token: 0x06002AA3 RID: 10915 RVA: 0x001D66D9 File Offset: 0x001D48D9
		[CompilerGenerated]
		public void Deconstruct(out AchievementStat Stat, out float Amount)
		{
			Stat = this.Stat;
			Amount = this.Amount;
		}
	}
}
