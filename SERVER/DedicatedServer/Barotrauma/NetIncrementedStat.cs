using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000050 RID: 80
	[NetworkSerialize(14)]
	internal readonly struct NetIncrementedStat : INetSerializableStruct, IEquatable<NetIncrementedStat>
	{
		// Token: 0x06000C03 RID: 3075 RVA: 0x000720D0 File Offset: 0x000702D0
		public NetIncrementedStat(AchievementStat Stat, float Amount)
		{
			this.Stat = Stat;
			this.Amount = Amount;
		}

		// Token: 0x17000334 RID: 820
		// (get) Token: 0x06000C04 RID: 3076 RVA: 0x000720E0 File Offset: 0x000702E0
		// (set) Token: 0x06000C05 RID: 3077 RVA: 0x000720E8 File Offset: 0x000702E8
		public AchievementStat Stat { get; set; }

		// Token: 0x17000335 RID: 821
		// (get) Token: 0x06000C06 RID: 3078 RVA: 0x000720F1 File Offset: 0x000702F1
		// (set) Token: 0x06000C07 RID: 3079 RVA: 0x000720F9 File Offset: 0x000702F9
		public float Amount { get; set; }

		// Token: 0x06000C08 RID: 3080 RVA: 0x00072104 File Offset: 0x00070304
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

		// Token: 0x06000C09 RID: 3081 RVA: 0x00072150 File Offset: 0x00070350
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Stat = ");
			builder.Append(this.Stat.ToString());
			builder.Append(", Amount = ");
			builder.Append(this.Amount.ToString());
			return true;
		}

		// Token: 0x06000C0A RID: 3082 RVA: 0x000721AC File Offset: 0x000703AC
		[CompilerGenerated]
		public static bool operator !=(NetIncrementedStat left, NetIncrementedStat right)
		{
			return !(left == right);
		}

		// Token: 0x06000C0B RID: 3083 RVA: 0x000721B8 File Offset: 0x000703B8
		[CompilerGenerated]
		public static bool operator ==(NetIncrementedStat left, NetIncrementedStat right)
		{
			return left.Equals(right);
		}

		// Token: 0x06000C0C RID: 3084 RVA: 0x000721C2 File Offset: 0x000703C2
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<AchievementStat>.Default.GetHashCode(this.<Stat>k__BackingField) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<Amount>k__BackingField);
		}

		// Token: 0x06000C0D RID: 3085 RVA: 0x000721EB File Offset: 0x000703EB
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetIncrementedStat && this.Equals((NetIncrementedStat)obj);
		}

		// Token: 0x06000C0E RID: 3086 RVA: 0x00072203 File Offset: 0x00070403
		[CompilerGenerated]
		public bool Equals(NetIncrementedStat other)
		{
			return EqualityComparer<AchievementStat>.Default.Equals(this.<Stat>k__BackingField, other.<Stat>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<Amount>k__BackingField, other.<Amount>k__BackingField);
		}

		// Token: 0x06000C0F RID: 3087 RVA: 0x00072235 File Offset: 0x00070435
		[CompilerGenerated]
		public void Deconstruct(out AchievementStat Stat, out float Amount)
		{
			Stat = this.Stat;
			Amount = this.Amount;
		}
	}
}
