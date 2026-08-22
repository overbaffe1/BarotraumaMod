using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001BC RID: 444
	[NetworkSerialize(28, ArrayMaxSize = 255)]
	internal readonly struct NetCharacterInfo : INetSerializableStruct, IEquatable<NetCharacterInfo>
	{
		// Token: 0x06003140 RID: 12608 RVA: 0x00203D08 File Offset: 0x00201F08
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

		// Token: 0x17000CE4 RID: 3300
		// (get) Token: 0x06003141 RID: 12609 RVA: 0x00203D62 File Offset: 0x00201F62
		// (set) Token: 0x06003142 RID: 12610 RVA: 0x00203D6A File Offset: 0x00201F6A
		public string NewName { get; set; }

		// Token: 0x17000CE5 RID: 3301
		// (get) Token: 0x06003143 RID: 12611 RVA: 0x00203D73 File Offset: 0x00201F73
		// (set) Token: 0x06003144 RID: 12612 RVA: 0x00203D7B File Offset: 0x00201F7B
		public ImmutableArray<Identifier> Tags { get; set; }

		// Token: 0x17000CE6 RID: 3302
		// (get) Token: 0x06003145 RID: 12613 RVA: 0x00203D84 File Offset: 0x00201F84
		// (set) Token: 0x06003146 RID: 12614 RVA: 0x00203D8C File Offset: 0x00201F8C
		public byte HairIndex { get; set; }

		// Token: 0x17000CE7 RID: 3303
		// (get) Token: 0x06003147 RID: 12615 RVA: 0x00203D95 File Offset: 0x00201F95
		// (set) Token: 0x06003148 RID: 12616 RVA: 0x00203D9D File Offset: 0x00201F9D
		public byte BeardIndex { get; set; }

		// Token: 0x17000CE8 RID: 3304
		// (get) Token: 0x06003149 RID: 12617 RVA: 0x00203DA6 File Offset: 0x00201FA6
		// (set) Token: 0x0600314A RID: 12618 RVA: 0x00203DAE File Offset: 0x00201FAE
		public byte MoustacheIndex { get; set; }

		// Token: 0x17000CE9 RID: 3305
		// (get) Token: 0x0600314B RID: 12619 RVA: 0x00203DB7 File Offset: 0x00201FB7
		// (set) Token: 0x0600314C RID: 12620 RVA: 0x00203DBF File Offset: 0x00201FBF
		public byte FaceAttachmentIndex { get; set; }

		// Token: 0x17000CEA RID: 3306
		// (get) Token: 0x0600314D RID: 12621 RVA: 0x00203DC8 File Offset: 0x00201FC8
		// (set) Token: 0x0600314E RID: 12622 RVA: 0x00203DD0 File Offset: 0x00201FD0
		public Color SkinColor { get; set; }

		// Token: 0x17000CEB RID: 3307
		// (get) Token: 0x0600314F RID: 12623 RVA: 0x00203DD9 File Offset: 0x00201FD9
		// (set) Token: 0x06003150 RID: 12624 RVA: 0x00203DE1 File Offset: 0x00201FE1
		public Color HairColor { get; set; }

		// Token: 0x17000CEC RID: 3308
		// (get) Token: 0x06003151 RID: 12625 RVA: 0x00203DEA File Offset: 0x00201FEA
		// (set) Token: 0x06003152 RID: 12626 RVA: 0x00203DF2 File Offset: 0x00201FF2
		public Color FacialHairColor { get; set; }

		// Token: 0x17000CED RID: 3309
		// (get) Token: 0x06003153 RID: 12627 RVA: 0x00203DFB File Offset: 0x00201FFB
		// (set) Token: 0x06003154 RID: 12628 RVA: 0x00203E03 File Offset: 0x00202003
		public ImmutableArray<NetJobVariant> JobVariants { get; set; }

		// Token: 0x06003155 RID: 12629 RVA: 0x00203E0C File Offset: 0x0020200C
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

		// Token: 0x06003156 RID: 12630 RVA: 0x00203E58 File Offset: 0x00202058
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

		// Token: 0x06003157 RID: 12631 RVA: 0x00203FDE File Offset: 0x002021DE
		[CompilerGenerated]
		public static bool operator !=(NetCharacterInfo left, NetCharacterInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06003158 RID: 12632 RVA: 0x00203FEA File Offset: 0x002021EA
		[CompilerGenerated]
		public static bool operator ==(NetCharacterInfo left, NetCharacterInfo right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003159 RID: 12633 RVA: 0x00203FF4 File Offset: 0x002021F4
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((((((((EqualityComparer<string>.Default.GetHashCode(this.<NewName>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<Identifier>>.Default.GetHashCode(this.<Tags>k__BackingField)) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<HairIndex>k__BackingField)) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<BeardIndex>k__BackingField)) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<MoustacheIndex>k__BackingField)) * -1521134295 + EqualityComparer<byte>.Default.GetHashCode(this.<FaceAttachmentIndex>k__BackingField)) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<SkinColor>k__BackingField)) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<HairColor>k__BackingField)) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<FacialHairColor>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<NetJobVariant>>.Default.GetHashCode(this.<JobVariants>k__BackingField);
		}

		// Token: 0x0600315A RID: 12634 RVA: 0x002040E0 File Offset: 0x002022E0
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is NetCharacterInfo && this.Equals((NetCharacterInfo)obj);
		}

		// Token: 0x0600315B RID: 12635 RVA: 0x002040F8 File Offset: 0x002022F8
		[CompilerGenerated]
		public bool Equals(NetCharacterInfo other)
		{
			return EqualityComparer<string>.Default.Equals(this.<NewName>k__BackingField, other.<NewName>k__BackingField) && EqualityComparer<ImmutableArray<Identifier>>.Default.Equals(this.<Tags>k__BackingField, other.<Tags>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<HairIndex>k__BackingField, other.<HairIndex>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<BeardIndex>k__BackingField, other.<BeardIndex>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<MoustacheIndex>k__BackingField, other.<MoustacheIndex>k__BackingField) && EqualityComparer<byte>.Default.Equals(this.<FaceAttachmentIndex>k__BackingField, other.<FaceAttachmentIndex>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<SkinColor>k__BackingField, other.<SkinColor>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<HairColor>k__BackingField, other.<HairColor>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<FacialHairColor>k__BackingField, other.<FacialHairColor>k__BackingField) && EqualityComparer<ImmutableArray<NetJobVariant>>.Default.Equals(this.<JobVariants>k__BackingField, other.<JobVariants>k__BackingField);
		}

		// Token: 0x0600315C RID: 12636 RVA: 0x00204204 File Offset: 0x00202404
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
