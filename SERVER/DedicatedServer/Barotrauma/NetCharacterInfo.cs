using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000BB RID: 187
	[NetworkSerialize(28, ArrayMaxSize = 255)]
	internal readonly struct NetCharacterInfo : INetSerializableStruct, IEquatable<NetCharacterInfo>
	{
		// Token: 0x06001580 RID: 5504 RVA: 0x000B8F00 File Offset: 0x000B7100
		public NetCharacterInfo(string NewName, ImmutableArray<Identifier> Tags, byte HairIndex, byte BeardIndex, byte MoustacheIndex, byte FaceAttachmentIndex, Color SkinColor, Color HairColor, Color FacialHairColor, ImmutableArray<NetJobVariant> JobVariants)
		{
			this.NewName = NewName;
			this.Tags = Tags;
			this.HairIndex = HairIndex;
			this.BeardIndex = BeardIndex;
			this.MoustacheIndex = MoustacheIndex;
			this.FaceAttachmentIndex = FaceAttachmentIndex;
			this.SkinColor = SkinColor;
			this.HairColor = HairColor;
			this.FacialHairColor = FacialHairColor;
			this.JobVariants = JobVariants;
		}

		// Token: 0x1700063E RID: 1598
		// (get) Token: 0x06001581 RID: 5505 RVA: 0x000B8F5A File Offset: 0x000B715A
		// (set) Token: 0x06001582 RID: 5506 RVA: 0x000B8F62 File Offset: 0x000B7162
		public string NewName { get; set; }

		// Token: 0x1700063F RID: 1599
		// (get) Token: 0x06001583 RID: 5507 RVA: 0x000B8F6B File Offset: 0x000B716B
		// (set) Token: 0x06001584 RID: 5508 RVA: 0x000B8F73 File Offset: 0x000B7173
		public ImmutableArray<Identifier> Tags { get; set; }

		// Token: 0x17000640 RID: 1600
		// (get) Token: 0x06001585 RID: 5509 RVA: 0x000B8F7C File Offset: 0x000B717C
		// (set) Token: 0x06001586 RID: 5510 RVA: 0x000B8F84 File Offset: 0x000B7184
		public byte HairIndex { get; set; }

		// Token: 0x17000641 RID: 1601
		// (get) Token: 0x06001587 RID: 5511 RVA: 0x000B8F8D File Offset: 0x000B718D
		// (set) Token: 0x06001588 RID: 5512 RVA: 0x000B8F95 File Offset: 0x000B7195
		public byte BeardIndex { get; set; }

		// Token: 0x17000642 RID: 1602
		// (get) Token: 0x06001589 RID: 5513 RVA: 0x000B8F9E File Offset: 0x000B719E
		// (set) Token: 0x0600158A RID: 5514 RVA: 0x000B8FA6 File Offset: 0x000B71A6
		public byte MoustacheIndex { get; set; }

		// Token: 0x17000643 RID: 1603
		// (get) Token: 0x0600158B RID: 5515 RVA: 0x000B8FAF File Offset: 0x000B71AF
		// (set) Token: 0x0600158C RID: 5516 RVA: 0x000B8FB7 File Offset: 0x000B71B7
		public byte FaceAttachmentIndex { get; set; }

		// Token: 0x17000644 RID: 1604
		// (get) Token: 0x0600158D RID: 5517 RVA: 0x000B8FC0 File Offset: 0x000B71C0
		// (set) Token: 0x0600158E RID: 5518 RVA: 0x000B8FC8 File Offset: 0x000B71C8
		public Color SkinColor { get; set; }

		// Token: 0x17000645 RID: 1605
		// (get) Token: 0x0600158F RID: 5519 RVA: 0x000B8FD1 File Offset: 0x000B71D1
		// (set) Token: 0x06001590 RID: 5520 RVA: 0x000B8FD9 File Offset: 0x000B71D9
		public Color HairColor { get; set; }

		// Token: 0x17000646 RID: 1606
		// (get) Token: 0x06001591 RID: 5521 RVA: 0x000B8FE2 File Offset: 0x000B71E2
		// (set) Token: 0x06001592 RID: 5522 RVA: 0x000B8FEA File Offset: 0x000B71EA
		public Color FacialHairColor { get; set; }

		// Token: 0x17000647 RID: 1607
		// (get) Token: 0x06001593 RID: 5523 RVA: 0x000B8FF3 File Offset: 0x000B71F3
		// (set) Token: 0x06001594 RID: 5524 RVA: 0x000B8FFB File Offset: 0x000B71FB
		public ImmutableArray<NetJobVariant> JobVariants { get; set; }

		// Token: 0x06001595 RID: 5525 RVA: 0x000B9004 File Offset: 0x000B7204
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("NetCharacterInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001596 RID: 5526 RVA: 0x000B9050 File Offset: 0x000B7250
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("NewName = ");
			builder.Append(this.NewName);
			builder.Append(", Tags = ");
			builder.Append(this.Tags.ToString());
			builder.Append(", HairIndex = ");
			builder.Append(this.HairIndex.ToString());
			builder.Append(", BeardIndex = ");
			builder.Append(this.BeardIndex.ToString());
			builder.Append(", MoustacheIndex = ");
			builder.Append(this.MoustacheIndex.ToString());
			builder.Append(", FaceAttachmentIndex = ");
			builder.Append(this.FaceAttachmentIndex.ToString());
			builder.Append(", SkinColor = ");
			builder.Append(this.SkinColor.ToString());
			builder.Append(", HairColor = ");
			builder.Append(this.HairColor.ToString());
			builder.Append(", FacialHairColor = ");
			builder.Append(this.FacialHairColor.ToString());
			builder.Append(", JobVariants = ");
			builder.Append(this.JobVariants.ToString());
			return true;
		}

		// Token: 0x06001597 RID: 5527 RVA: 0x000B91D6 File Offset: 0x000B73D6
		[CompilerGenerated]
		public static bool operator !=(NetCharacterInfo left, NetCharacterInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06001598 RID: 5528 RVA: 0x000B91E2 File Offset: 0x000B73E2
		[CompilerGenerated]
		public static bool operator ==(NetCharacterInfo left, NetCharacterInfo right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001599 RID: 5529 RVA: 0x000B91EC File Offset: 0x000B73EC
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((((((((EqualityComparer<string>.Default.GetHashCode(this.<NewName>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<Tags>k__BackingField)) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<HairIndex>k__BackingField)) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<BeardIndex>k__BackingField)) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<MoustacheIndex>k__BackingField)) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<FaceAttachmentIndex>k__BackingField)) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<SkinColor>k__BackingField)) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<HairColor>k__BackingField)) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<FacialHairColor>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<NetJobVariant>>.Default.GetHashCode(this.<JobVariants>k__BackingField);
		}

		// Token: 0x0600159A RID: 5530 RVA: 0x000B92D8 File Offset: 0x000B74D8
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetCharacterInfo && this.Equals((NetCharacterInfo)obj);
		}

		// Token: 0x0600159B RID: 5531 RVA: 0x000B92F0 File Offset: 0x000B74F0
		[CompilerGenerated]
		public bool Equals(NetCharacterInfo other)
		{
			return EqualityComparer<string>.Default.Equals(this.<NewName>k__BackingField, other.<NewName>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<Tags>k__BackingField, other.<Tags>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<HairIndex>k__BackingField, other.<HairIndex>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<BeardIndex>k__BackingField, other.<BeardIndex>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<MoustacheIndex>k__BackingField, other.<MoustacheIndex>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<FaceAttachmentIndex>k__BackingField, other.<FaceAttachmentIndex>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<SkinColor>k__BackingField, other.<SkinColor>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<HairColor>k__BackingField, other.<HairColor>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<FacialHairColor>k__BackingField, other.<FacialHairColor>k__BackingField) && EqualityComparer<ImmutableArray<NetJobVariant>>.Default.Equals(this.<JobVariants>k__BackingField, other.<JobVariants>k__BackingField);
		}

		// Token: 0x0600159C RID: 5532 RVA: 0x000B93FC File Offset: 0x000B75FC
		[CompilerGenerated]
		public void Deconstruct(out string NewName, out ImmutableArray<Identifier> Tags, out byte HairIndex, out byte BeardIndex, out byte MoustacheIndex, out byte FaceAttachmentIndex, out Color SkinColor, out Color HairColor, out Color FacialHairColor, out ImmutableArray<NetJobVariant> JobVariants)
		{
			NewName = this.NewName;
			Tags = this.Tags;
			HairIndex = this.HairIndex;
			BeardIndex = this.BeardIndex;
			MoustacheIndex = this.MoustacheIndex;
			FaceAttachmentIndex = this.FaceAttachmentIndex;
			SkinColor = this.SkinColor;
			HairColor = this.HairColor;
			FacialHairColor = this.FacialHairColor;
			JobVariants = this.JobVariants;
		}
	}
}
