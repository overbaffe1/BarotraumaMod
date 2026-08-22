using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020003A0 RID: 928
	[NetworkSerialize(5, ArrayMaxSize = 1100)]
	internal readonly struct MessageFragment : INetSerializableStruct, IEquatable<MessageFragment>
	{
		// Token: 0x0600368C RID: 13964 RVA: 0x00174497 File Offset: 0x00172697
		public MessageFragment(MessageFragment.Id FragmentId, ImmutableArray<byte> Data)
		{
			this.FragmentId = FragmentId;
			this.Data = Data;
		}

		// Token: 0x17000F19 RID: 3865
		// (get) Token: 0x0600368D RID: 13965 RVA: 0x001744A7 File Offset: 0x001726A7
		// (set) Token: 0x0600368E RID: 13966 RVA: 0x001744AF File Offset: 0x001726AF
		public MessageFragment.Id FragmentId { get; set; }

		// Token: 0x17000F1A RID: 3866
		// (get) Token: 0x0600368F RID: 13967 RVA: 0x001744B8 File Offset: 0x001726B8
		// (set) Token: 0x06003690 RID: 13968 RVA: 0x001744C0 File Offset: 0x001726C0
		public ImmutableArray<byte> Data { get; set; }

		// Token: 0x06003691 RID: 13969 RVA: 0x001744CC File Offset: 0x001726CC
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

		// Token: 0x06003692 RID: 13970 RVA: 0x00174518 File Offset: 0x00172718
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("FragmentId = ");
			builder.Append(this.FragmentId.ToString());
			builder.Append(", Data = ");
			builder.Append(this.Data.ToString());
			return true;
		}

		// Token: 0x06003693 RID: 13971 RVA: 0x00174574 File Offset: 0x00172774
		[CompilerGenerated]
		public static bool operator !=(MessageFragment left, MessageFragment right)
		{
			return !(left == right);
		}

		// Token: 0x06003694 RID: 13972 RVA: 0x00174580 File Offset: 0x00172780
		[CompilerGenerated]
		public static bool operator ==(MessageFragment left, MessageFragment right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003695 RID: 13973 RVA: 0x0017458A File Offset: 0x0017278A
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<MessageFragment.Id>.Default.GetHashCode(this.<FragmentId>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<byte>>.Default.GetHashCode(this.<Data>k__BackingField);
		}

		// Token: 0x06003696 RID: 13974 RVA: 0x001745B3 File Offset: 0x001727B3
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is MessageFragment && this.Equals((MessageFragment)obj);
		}

		// Token: 0x06003697 RID: 13975 RVA: 0x001745CB File Offset: 0x001727CB
		[CompilerGenerated]
		public bool Equals(MessageFragment other)
		{
			return EqualityComparer<MessageFragment.Id>.Default.Equals(this.<FragmentId>k__BackingField, other.<FragmentId>k__BackingField) && EqualityComparer<ImmutableArray<byte>>.Default.Equals(this.<Data>k__BackingField, other.<Data>k__BackingField);
		}

		// Token: 0x06003698 RID: 13976 RVA: 0x001745FD File Offset: 0x001727FD
		[CompilerGenerated]
		public void Deconstruct(out MessageFragment.Id FragmentId, out ImmutableArray<byte> Data)
		{
			FragmentId = this.FragmentId;
			Data = this.Data;
		}

		// Token: 0x04001BC0 RID: 7104
		public const int MaxSize = 1100;

		// Token: 0x02000C4C RID: 3148
		[NetworkSerialize(12)]
		public readonly struct Id : INetSerializableStruct, IEquatable<MessageFragment.Id>
		{
			// Token: 0x060063A7 RID: 25511 RVA: 0x00212EA4 File Offset: 0x002110A4
			public Id(ushort FragmentIndex, ushort FragmentCount, ushort MessageId)
			{
				this.FragmentIndex = FragmentIndex;
				this.FragmentCount = FragmentCount;
				this.MessageId = MessageId;
			}

			// Token: 0x17001625 RID: 5669
			// (get) Token: 0x060063A8 RID: 25512 RVA: 0x00212EBB File Offset: 0x002110BB
			// (set) Token: 0x060063A9 RID: 25513 RVA: 0x00212EC3 File Offset: 0x002110C3
			public ushort FragmentIndex { get; set; }

			// Token: 0x17001626 RID: 5670
			// (get) Token: 0x060063AA RID: 25514 RVA: 0x00212ECC File Offset: 0x002110CC
			// (set) Token: 0x060063AB RID: 25515 RVA: 0x00212ED4 File Offset: 0x002110D4
			public ushort FragmentCount { get; set; }

			// Token: 0x17001627 RID: 5671
			// (get) Token: 0x060063AC RID: 25516 RVA: 0x00212EDD File Offset: 0x002110DD
			// (set) Token: 0x060063AD RID: 25517 RVA: 0x00212EE5 File Offset: 0x002110E5
			public ushort MessageId { get; set; }

			// Token: 0x060063AE RID: 25518 RVA: 0x00212EF0 File Offset: 0x002110F0
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

			// Token: 0x060063AF RID: 25519 RVA: 0x00212F3C File Offset: 0x0021113C
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

			// Token: 0x060063B0 RID: 25520 RVA: 0x00212FBF File Offset: 0x002111BF
			[CompilerGenerated]
			public static bool operator !=(MessageFragment.Id left, MessageFragment.Id right)
			{
				return !(left == right);
			}

			// Token: 0x060063B1 RID: 25521 RVA: 0x00212FCB File Offset: 0x002111CB
			[CompilerGenerated]
			public static bool operator ==(MessageFragment.Id left, MessageFragment.Id right)
			{
				return left.Equals(right);
			}

			// Token: 0x060063B2 RID: 25522 RVA: 0x00212FD5 File Offset: 0x002111D5
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<ushort>.Default.GetHashCode(this.<FragmentIndex>k__BackingField) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<FragmentCount>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<MessageId>k__BackingField);
			}

			// Token: 0x060063B3 RID: 25523 RVA: 0x00213015 File Offset: 0x00211215
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MessageFragment.Id && this.Equals((MessageFragment.Id)obj);
			}

			// Token: 0x060063B4 RID: 25524 RVA: 0x00213030 File Offset: 0x00211230
			[CompilerGenerated]
			public bool Equals(MessageFragment.Id other)
			{
				return EqualityComparer<ushort>.Default.Equals(this.<FragmentIndex>k__BackingField, other.<FragmentIndex>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<FragmentCount>k__BackingField, other.<FragmentCount>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<MessageId>k__BackingField, other.<MessageId>k__BackingField);
			}

			// Token: 0x060063B5 RID: 25525 RVA: 0x00213085 File Offset: 0x00211285
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
