using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x0200049D RID: 1181
	[NetworkSerialize(5, ArrayMaxSize = 1100)]
	internal readonly struct MessageFragment : INetSerializableStruct, IEquatable<MessageFragment>
	{
		// Token: 0x06004E61 RID: 20065 RVA: 0x002ACFD3 File Offset: 0x002AB1D3
		public MessageFragment(MessageFragment.Id FragmentId, ImmutableArray<byte> Data)
		{
			this.FragmentId = FragmentId;
			this.Data = Data;
		}

		// Token: 0x17001414 RID: 5140
		// (get) Token: 0x06004E62 RID: 20066 RVA: 0x002ACFE3 File Offset: 0x002AB1E3
		// (set) Token: 0x06004E63 RID: 20067 RVA: 0x002ACFEB File Offset: 0x002AB1EB
		public MessageFragment.Id FragmentId { get; set; }

		// Token: 0x17001415 RID: 5141
		// (get) Token: 0x06004E64 RID: 20068 RVA: 0x002ACFF4 File Offset: 0x002AB1F4
		// (set) Token: 0x06004E65 RID: 20069 RVA: 0x002ACFFC File Offset: 0x002AB1FC
		public ImmutableArray<byte> Data { get; set; }

		// Token: 0x06004E66 RID: 20070 RVA: 0x002AD008 File Offset: 0x002AB208
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("MessageFragment");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06004E67 RID: 20071 RVA: 0x002AD054 File Offset: 0x002AB254
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("FragmentId = ");
			builder.Append(this.FragmentId.ToString());
			builder.Append(", Data = ");
			builder.Append(this.Data.ToString());
			return true;
		}

		// Token: 0x06004E68 RID: 20072 RVA: 0x002AD0B0 File Offset: 0x002AB2B0
		[CompilerGenerated]
		public static bool operator !=(MessageFragment left, MessageFragment right)
		{
			return !(left == right);
		}

		// Token: 0x06004E69 RID: 20073 RVA: 0x002AD0BC File Offset: 0x002AB2BC
		[CompilerGenerated]
		public static bool operator ==(MessageFragment left, MessageFragment right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004E6A RID: 20074 RVA: 0x002AD0C6 File Offset: 0x002AB2C6
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<MessageFragment.Id>.Default.GetHashCode(this.<FragmentId>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<byte>>.Default.GetHashCode(this.<Data>k__BackingField);
		}

		// Token: 0x06004E6B RID: 20075 RVA: 0x002AD0EF File Offset: 0x002AB2EF
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is MessageFragment && this.Equals((MessageFragment)obj);
		}

		// Token: 0x06004E6C RID: 20076 RVA: 0x002AD107 File Offset: 0x002AB307
		[CompilerGenerated]
		public bool Equals(MessageFragment other)
		{
			return EqualityComparer<MessageFragment.Id>.Default.Equals(this.<FragmentId>k__BackingField, other.<FragmentId>k__BackingField) && EqualityComparer<ImmutableArray<byte>>.Default.Equals(this.<Data>k__BackingField, other.<Data>k__BackingField);
		}

		// Token: 0x06004E6D RID: 20077 RVA: 0x002AD139 File Offset: 0x002AB339
		[CompilerGenerated]
		public void Deconstruct(out MessageFragment.Id FragmentId, out ImmutableArray<byte> Data)
		{
			FragmentId = this.FragmentId;
			Data = this.Data;
		}

		// Token: 0x040029B7 RID: 10679
		public const int MaxSize = 1100;

		// Token: 0x0200124C RID: 4684
		[NetworkSerialize(12)]
		public readonly struct Id : INetSerializableStruct, IEquatable<MessageFragment.Id>
		{
			// Token: 0x060093C1 RID: 37825 RVA: 0x003CDDE0 File Offset: 0x003CBFE0
			public Id(ushort FragmentIndex, ushort FragmentCount, ushort MessageId)
			{
				this.FragmentIndex = FragmentIndex;
				this.FragmentCount = FragmentCount;
				this.MessageId = MessageId;
			}

			// Token: 0x17001CE4 RID: 7396
			// (get) Token: 0x060093C2 RID: 37826 RVA: 0x003CDDF7 File Offset: 0x003CBFF7
			// (set) Token: 0x060093C3 RID: 37827 RVA: 0x003CDDFF File Offset: 0x003CBFFF
			public ushort FragmentIndex { get; set; }

			// Token: 0x17001CE5 RID: 7397
			// (get) Token: 0x060093C4 RID: 37828 RVA: 0x003CDE08 File Offset: 0x003CC008
			// (set) Token: 0x060093C5 RID: 37829 RVA: 0x003CDE10 File Offset: 0x003CC010
			public ushort FragmentCount { get; set; }

			// Token: 0x17001CE6 RID: 7398
			// (get) Token: 0x060093C6 RID: 37830 RVA: 0x003CDE19 File Offset: 0x003CC019
			// (set) Token: 0x060093C7 RID: 37831 RVA: 0x003CDE21 File Offset: 0x003CC021
			public ushort MessageId { get; set; }

			// Token: 0x060093C8 RID: 37832 RVA: 0x003CDE2C File Offset: 0x003CC02C
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Id");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060093C9 RID: 37833 RVA: 0x003CDE78 File Offset: 0x003CC078
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("FragmentIndex = ");
				builder.Append(this.FragmentIndex.ToString());
				builder.Append(", FragmentCount = ");
				builder.Append(this.FragmentCount.ToString());
				builder.Append(", MessageId = ");
				builder.Append(this.MessageId.ToString());
				return true;
			}

			// Token: 0x060093CA RID: 37834 RVA: 0x003CDEFB File Offset: 0x003CC0FB
			[CompilerGenerated]
			public static bool operator !=(MessageFragment.Id left, MessageFragment.Id right)
			{
				return !(left == right);
			}

			// Token: 0x060093CB RID: 37835 RVA: 0x003CDF07 File Offset: 0x003CC107
			[CompilerGenerated]
			public static bool operator ==(MessageFragment.Id left, MessageFragment.Id right)
			{
				return left.Equals(right);
			}

			// Token: 0x060093CC RID: 37836 RVA: 0x003CDF11 File Offset: 0x003CC111
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<ushort>.Default.GetHashCode(this.<FragmentIndex>k__BackingField) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<FragmentCount>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<MessageId>k__BackingField);
			}

			// Token: 0x060093CD RID: 37837 RVA: 0x003CDF51 File Offset: 0x003CC151
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MessageFragment.Id && this.Equals((MessageFragment.Id)obj);
			}

			// Token: 0x060093CE RID: 37838 RVA: 0x003CDF6C File Offset: 0x003CC16C
			[CompilerGenerated]
			public bool Equals(MessageFragment.Id other)
			{
				return EqualityComparer<ushort>.Default.Equals(this.<FragmentIndex>k__BackingField, other.<FragmentIndex>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<FragmentCount>k__BackingField, other.<FragmentCount>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<MessageId>k__BackingField, other.<MessageId>k__BackingField);
			}

			// Token: 0x060093CF RID: 37839 RVA: 0x003CDFC1 File Offset: 0x003CC1C1
			[CompilerGenerated]
			public void Deconstruct(out ushort FragmentIndex, out ushort FragmentCount, out ushort MessageId)
			{
				FragmentIndex = this.FragmentIndex;
				FragmentCount = this.FragmentCount;
				MessageId = this.MessageId;
			}
		}
	}
}
