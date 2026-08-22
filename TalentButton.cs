using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020000B8 RID: 184
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct TalentButton : IEquatable<TalentButton>
	{
		// Token: 0x06001711 RID: 5905 RVA: 0x000DF540 File Offset: 0x000DD740
		public TalentButton(GUIComponent IconComponent, TalentPrefab Prefab)
		{
			this.IconComponent = IconComponent;
			this.Prefab = Prefab;
		}

		// Token: 0x170005C6 RID: 1478
		// (get) Token: 0x06001712 RID: 5906 RVA: 0x000DF550 File Offset: 0x000DD750
		// (set) Token: 0x06001713 RID: 5907 RVA: 0x000DF558 File Offset: 0x000DD758
		public GUIComponent IconComponent { get; set; }

		// Token: 0x170005C7 RID: 1479
		// (get) Token: 0x06001714 RID: 5908 RVA: 0x000DF561 File Offset: 0x000DD761
		// (set) Token: 0x06001715 RID: 5909 RVA: 0x000DF569 File Offset: 0x000DD769
		public TalentPrefab Prefab { get; set; }

		// Token: 0x170005C8 RID: 1480
		// (get) Token: 0x06001716 RID: 5910 RVA: 0x000DF572 File Offset: 0x000DD772
		public Identifier Identifier
		{
			get
			{
				return this.Prefab.Identifier;
			}
		}

		// Token: 0x06001717 RID: 5911 RVA: 0x000DF580 File Offset: 0x000DD780
		[NullableContext(0)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("TalentButton");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001718 RID: 5912 RVA: 0x000DF5CC File Offset: 0x000DD7CC
		[NullableContext(0)]
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("IconComponent = ");
			builder.Append(this.IconComponent);
			builder.Append(", Prefab = ");
			builder.Append(this.Prefab);
			builder.Append(", Identifier = ");
			builder.Append(this.Identifier.ToString());
			return true;
		}

		// Token: 0x06001719 RID: 5913 RVA: 0x000DF633 File Offset: 0x000DD833
		[CompilerGenerated]
		public static bool operator !=(TalentButton left, TalentButton right)
		{
			return !(left == right);
		}

		// Token: 0x0600171A RID: 5914 RVA: 0x000DF63F File Offset: 0x000DD83F
		[CompilerGenerated]
		public static bool operator ==(TalentButton left, TalentButton right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600171B RID: 5915 RVA: 0x000DF649 File Offset: 0x000DD849
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<GUIComponent>.Default.GetHashCode(this.<IconComponent>k__BackingField) * -1521134295 + EqualityComparer<TalentPrefab>.Default.GetHashCode(this.<Prefab>k__BackingField);
		}

		// Token: 0x0600171C RID: 5916 RVA: 0x000DF672 File Offset: 0x000DD872
		[NullableContext(0)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is TalentButton && this.Equals((TalentButton)obj);
		}

		// Token: 0x0600171D RID: 5917 RVA: 0x000DF68A File Offset: 0x000DD88A
		[CompilerGenerated]
		public bool Equals(TalentButton other)
		{
			return EqualityComparer<GUIComponent>.Default.Equals(this.<IconComponent>k__BackingField, other.<IconComponent>k__BackingField) && EqualityComparer<TalentPrefab>.Default.Equals(this.<Prefab>k__BackingField, other.<Prefab>k__BackingField);
		}

		// Token: 0x0600171E RID: 5918 RVA: 0x000DF6BC File Offset: 0x000DD8BC
		[CompilerGenerated]
		public void Deconstruct(out GUIComponent IconComponent, out TalentPrefab Prefab)
		{
			IconComponent = this.IconComponent;
			Prefab = this.Prefab;
		}
	}
}
