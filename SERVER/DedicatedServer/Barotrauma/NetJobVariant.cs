using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020000BA RID: 186
	[NetworkSerialize(15)]
	internal readonly struct NetJobVariant : INetSerializableStruct, IEquatable<NetJobVariant>
	{
		// Token: 0x06001571 RID: 5489 RVA: 0x000B8D34 File Offset: 0x000B6F34
		public NetJobVariant(Identifier Identifier, byte Variant)
		{
			this.Identifier = Identifier;
			this.Variant = Variant;
		}

		// Token: 0x1700063C RID: 1596
		// (get) Token: 0x06001572 RID: 5490 RVA: 0x000B8D44 File Offset: 0x000B6F44
		// (set) Token: 0x06001573 RID: 5491 RVA: 0x000B8D4C File Offset: 0x000B6F4C
		public Identifier Identifier { get; set; }

		// Token: 0x1700063D RID: 1597
		// (get) Token: 0x06001574 RID: 5492 RVA: 0x000B8D55 File Offset: 0x000B6F55
		// (set) Token: 0x06001575 RID: 5493 RVA: 0x000B8D5D File Offset: 0x000B6F5D
		public byte Variant { get; set; }

		// Token: 0x06001576 RID: 5494 RVA: 0x000B8D68 File Offset: 0x000B6F68
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

		// Token: 0x06001577 RID: 5495 RVA: 0x000B8D9F File Offset: 0x000B6F9F
		public static NetJobVariant FromJobVariant(JobVariant jobVariant)
		{
			return new NetJobVariant(jobVariant.Prefab.Identifier, (byte)jobVariant.Variant);
		}

		// Token: 0x06001578 RID: 5496 RVA: 0x000B8DB8 File Offset: 0x000B6FB8
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

		// Token: 0x06001579 RID: 5497 RVA: 0x000B8E04 File Offset: 0x000B7004
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Identifier = ");
			builder.Append(this.Identifier.ToString());
			builder.Append(", Variant = ");
			builder.Append(this.Variant.ToString());
			return true;
		}

		// Token: 0x0600157A RID: 5498 RVA: 0x000B8E60 File Offset: 0x000B7060
		[CompilerGenerated]
		public static bool operator !=(NetJobVariant left, NetJobVariant right)
		{
			return !(left == right);
		}

		// Token: 0x0600157B RID: 5499 RVA: 0x000B8E6C File Offset: 0x000B706C
		[CompilerGenerated]
		public static bool operator ==(NetJobVariant left, NetJobVariant right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600157C RID: 5500 RVA: 0x000B8E76 File Offset: 0x000B7076
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Identifier>.Default.GetHashCode(this.<Identifier>k__BackingField) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<Variant>k__BackingField);
		}

		// Token: 0x0600157D RID: 5501 RVA: 0x000B8E9F File Offset: 0x000B709F
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetJobVariant && this.Equals((NetJobVariant)obj);
		}

		// Token: 0x0600157E RID: 5502 RVA: 0x000B8EB7 File Offset: 0x000B70B7
		[CompilerGenerated]
		public bool Equals(NetJobVariant other)
		{
			return EqualityComparer<Identifier>.Default.Equals(this.<Identifier>k__BackingField, other.<Identifier>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<Variant>k__BackingField, other.<Variant>k__BackingField);
		}

		// Token: 0x0600157F RID: 5503 RVA: 0x000B8EE9 File Offset: 0x000B70E9
		[CompilerGenerated]
		public void Deconstruct(out Identifier Identifier, out byte Variant)
		{
			Identifier = this.Identifier;
			Variant = this.Variant;
		}
	}
}
