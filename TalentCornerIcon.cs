using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020000B9 RID: 185
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct TalentCornerIcon : IEquatable<TalentCornerIcon>
	{
		// Token: 0x0600171F RID: 5919 RVA: 0x000DF6CE File Offset: 0x000DD8CE
		public TalentCornerIcon(Identifier TalentTree, int Index, GUIImage IconComponent, GUIFrame BackgroundComponent, GUIFrame GlowComponent)
		{
			this.TalentTree = TalentTree;
			this.Index = Index;
			this.IconComponent = IconComponent;
			this.BackgroundComponent = BackgroundComponent;
			this.GlowComponent = GlowComponent;
		}

		// Token: 0x170005C9 RID: 1481
		// (get) Token: 0x06001720 RID: 5920 RVA: 0x000DF6F5 File Offset: 0x000DD8F5
		// (set) Token: 0x06001721 RID: 5921 RVA: 0x000DF6FD File Offset: 0x000DD8FD
		public Identifier TalentTree { get; set; }

		// Token: 0x170005CA RID: 1482
		// (get) Token: 0x06001722 RID: 5922 RVA: 0x000DF706 File Offset: 0x000DD906
		// (set) Token: 0x06001723 RID: 5923 RVA: 0x000DF70E File Offset: 0x000DD90E
		public int Index { get; set; }

		// Token: 0x170005CB RID: 1483
		// (get) Token: 0x06001724 RID: 5924 RVA: 0x000DF717 File Offset: 0x000DD917
		// (set) Token: 0x06001725 RID: 5925 RVA: 0x000DF71F File Offset: 0x000DD91F
		public GUIImage IconComponent { get; set; }

		// Token: 0x170005CC RID: 1484
		// (get) Token: 0x06001726 RID: 5926 RVA: 0x000DF728 File Offset: 0x000DD928
		// (set) Token: 0x06001727 RID: 5927 RVA: 0x000DF730 File Offset: 0x000DD930
		public GUIFrame BackgroundComponent { get; set; }

		// Token: 0x170005CD RID: 1485
		// (get) Token: 0x06001728 RID: 5928 RVA: 0x000DF739 File Offset: 0x000DD939
		// (set) Token: 0x06001729 RID: 5929 RVA: 0x000DF741 File Offset: 0x000DD941
		public GUIFrame GlowComponent { get; set; }

		// Token: 0x0600172A RID: 5930 RVA: 0x000DF74C File Offset: 0x000DD94C
		[NullableContext(0)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("TalentCornerIcon");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600172B RID: 5931 RVA: 0x000DF798 File Offset: 0x000DD998
		[NullableContext(0)]
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("TalentTree = ");
			builder.Append(this.TalentTree.ToString());
			builder.Append(", Index = ");
			builder.Append(this.Index.ToString());
			builder.Append(", IconComponent = ");
			builder.Append(this.IconComponent);
			builder.Append(", BackgroundComponent = ");
			builder.Append(this.BackgroundComponent);
			builder.Append(", GlowComponent = ");
			builder.Append(this.GlowComponent);
			return true;
		}

		// Token: 0x0600172C RID: 5932 RVA: 0x000DF83F File Offset: 0x000DDA3F
		[CompilerGenerated]
		public static bool operator !=(TalentCornerIcon left, TalentCornerIcon right)
		{
			return !(left == right);
		}

		// Token: 0x0600172D RID: 5933 RVA: 0x000DF84B File Offset: 0x000DDA4B
		[CompilerGenerated]
		public static bool operator ==(TalentCornerIcon left, TalentCornerIcon right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600172E RID: 5934 RVA: 0x000DF858 File Offset: 0x000DDA58
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((EqualityComparer<Identifier>.Default.GetHashCode(this.<TalentTree>k__BackingField) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<Index>k__BackingField)) * -1521134295 + EqualityComparer<GUIImage>.Default.GetHashCode(this.<IconComponent>k__BackingField)) * -1521134295 + EqualityComparer<GUIFrame>.Default.GetHashCode(this.<BackgroundComponent>k__BackingField)) * -1521134295 + EqualityComparer<GUIFrame>.Default.GetHashCode(this.<GlowComponent>k__BackingField);
		}

		// Token: 0x0600172F RID: 5935 RVA: 0x000DF8D1 File Offset: 0x000DDAD1
		[NullableContext(0)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is TalentCornerIcon && this.Equals((TalentCornerIcon)obj);
		}

		// Token: 0x06001730 RID: 5936 RVA: 0x000DF8EC File Offset: 0x000DDAEC
		[CompilerGenerated]
		public bool Equals(TalentCornerIcon other)
		{
			return EqualityComparer<Identifier>.Default.Equals(this.<TalentTree>k__BackingField, other.<TalentTree>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<Index>k__BackingField, other.<Index>k__BackingField) && EqualityComparer<GUIImage>.Default.Equals(this.<IconComponent>k__BackingField, other.<IconComponent>k__BackingField) && EqualityComparer<GUIFrame>.Default.Equals(this.<BackgroundComponent>k__BackingField, other.<BackgroundComponent>k__BackingField) && EqualityComparer<GUIFrame>.Default.Equals(this.<GlowComponent>k__BackingField, other.<GlowComponent>k__BackingField);
		}

		// Token: 0x06001731 RID: 5937 RVA: 0x000DF971 File Offset: 0x000DDB71
		[CompilerGenerated]
		public void Deconstruct(out Identifier TalentTree, out int Index, out GUIImage IconComponent, out GUIFrame BackgroundComponent, out GUIFrame GlowComponent)
		{
			TalentTree = this.TalentTree;
			Index = this.Index;
			IconComponent = this.IconComponent;
			BackgroundComponent = this.BackgroundComponent;
			GlowComponent = this.GlowComponent;
		}
	}
}
