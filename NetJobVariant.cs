using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020001BB RID: 443
	[NetworkSerialize(15)]
	internal readonly struct NetJobVariant : INetSerializableStruct, IEquatable<NetJobVariant>
	{
		// Token: 0x06003131 RID: 12593 RVA: 0x00203B3C File Offset: 0x00201D3C
		public NetJobVariant(Identifier Identifier, byte Variant)
		{
			this.Identifier = Identifier;
			this.Variant = Variant;
		}

		// Token: 0x17000CE2 RID: 3298
		// (get) Token: 0x06003132 RID: 12594 RVA: 0x00203B4C File Offset: 0x00201D4C
		// (set) Token: 0x06003133 RID: 12595 RVA: 0x00203B54 File Offset: 0x00201D54
		public Identifier Identifier { get; set; }

		// Token: 0x17000CE3 RID: 3299
		// (get) Token: 0x06003134 RID: 12596 RVA: 0x00203B5D File Offset: 0x00201D5D
		// (set) Token: 0x06003135 RID: 12597 RVA: 0x00203B65 File Offset: 0x00201D65
		public byte Variant { get; set; }

		// Token: 0x06003136 RID: 12598 RVA: 0x00203B70 File Offset: 0x00201D70
		[return: MaybeNull]
		public JobVariant ToJobVariant()
		{
			JobPrefab jobPrefab;
			if (!JobPrefab.Prefabs.TryGet(this.Identifier, out jobPrefab) || jobPrefab.HiddenJob)
			{
				return null;
			}
			return new JobVariant(jobPrefab, (int)this.Variant);
		}

		// Token: 0x06003137 RID: 12599 RVA: 0x00203BA7 File Offset: 0x00201DA7
		public static NetJobVariant FromJobVariant(JobVariant jobVariant)
		{
			return new NetJobVariant(jobVariant.Prefab.Identifier, (byte)jobVariant.Variant);
		}

		// Token: 0x06003138 RID: 12600 RVA: 0x00203BC0 File Offset: 0x00201DC0
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("NetJobVariant");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003139 RID: 12601 RVA: 0x00203C0C File Offset: 0x00201E0C
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Identifier = ");
			builder.Append(this.Identifier.ToString());
			builder.Append(", Variant = ");
			builder.Append(this.Variant.ToString());
			return true;
		}

		// Token: 0x0600313A RID: 12602 RVA: 0x00203C68 File Offset: 0x00201E68
		[CompilerGenerated]
		public static bool operator !=(NetJobVariant left, NetJobVariant right)
		{
			return !(left == right);
		}

		// Token: 0x0600313B RID: 12603 RVA: 0x00203C74 File Offset: 0x00201E74
		[CompilerGenerated]
		public static bool operator ==(NetJobVariant left, NetJobVariant right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600313C RID: 12604 RVA: 0x00203C7E File Offset: 0x00201E7E
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Identifier>.Default.GetHashCode(this.<Identifier>k__BackingField) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<Variant>k__BackingField);
		}

		// Token: 0x0600313D RID: 12605 RVA: 0x00203CA7 File Offset: 0x00201EA7
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetJobVariant && this.Equals((NetJobVariant)obj);
		}

		// Token: 0x0600313E RID: 12606 RVA: 0x00203CBF File Offset: 0x00201EBF
		[CompilerGenerated]
		public bool Equals(NetJobVariant other)
		{
			return EqualityComparer<Identifier>.Default.Equals(this.<Identifier>k__BackingField, other.<Identifier>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<Variant>k__BackingField, other.<Variant>k__BackingField);
		}

		// Token: 0x0600313F RID: 12607 RVA: 0x00203CF1 File Offset: 0x00201EF1
		[CompilerGenerated]
		public void Deconstruct(out Identifier Identifier, out byte Variant)
		{
			Identifier = this.Identifier;
			Variant = this.Variant;
		}
	}
}
