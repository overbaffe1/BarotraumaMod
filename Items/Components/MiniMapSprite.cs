using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005BE RID: 1470
	[NullableContext(2)]
	[Nullable(0)]
	internal readonly struct MiniMapSprite : IEquatable<MiniMapSprite>
	{
		// Token: 0x06005C53 RID: 23635 RVA: 0x002F76D2 File Offset: 0x002F58D2
		public MiniMapSprite(Sprite Sprite, Color Color)
		{
			this.Sprite = Sprite;
			this.Color = Color;
		}

		// Token: 0x17001742 RID: 5954
		// (get) Token: 0x06005C54 RID: 23636 RVA: 0x002F76E2 File Offset: 0x002F58E2
		// (set) Token: 0x06005C55 RID: 23637 RVA: 0x002F76EA File Offset: 0x002F58EA
		public Sprite Sprite { get; set; }

		// Token: 0x17001743 RID: 5955
		// (get) Token: 0x06005C56 RID: 23638 RVA: 0x002F76F3 File Offset: 0x002F58F3
		// (set) Token: 0x06005C57 RID: 23639 RVA: 0x002F76FB File Offset: 0x002F58FB
		public Color Color { get; set; }

		// Token: 0x06005C58 RID: 23640 RVA: 0x002F7704 File Offset: 0x002F5904
		[NullableContext(1)]
		public MiniMapSprite(JobPrefab prefab)
		{
			this = new MiniMapSprite(prefab.IconSmall, prefab.UIColor);
		}

		// Token: 0x06005C59 RID: 23641 RVA: 0x002F7718 File Offset: 0x002F5918
		[NullableContext(1)]
		public MiniMapSprite(Order order)
		{
			this = new MiniMapSprite(order.SymbolSprite, order.Color);
		}

		// Token: 0x06005C5A RID: 23642 RVA: 0x002F772C File Offset: 0x002F592C
		[NullableContext(0)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("MiniMapSprite");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06005C5B RID: 23643 RVA: 0x002F7778 File Offset: 0x002F5978
		[NullableContext(0)]
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Sprite = ");
			builder.Append(this.Sprite);
			builder.Append(", Color = ");
			builder.Append(this.Color.ToString());
			return true;
		}

		// Token: 0x06005C5C RID: 23644 RVA: 0x002F77C6 File Offset: 0x002F59C6
		[CompilerGenerated]
		public static bool operator !=(MiniMapSprite left, MiniMapSprite right)
		{
			return !(left == right);
		}

		// Token: 0x06005C5D RID: 23645 RVA: 0x002F77D2 File Offset: 0x002F59D2
		[CompilerGenerated]
		public static bool operator ==(MiniMapSprite left, MiniMapSprite right)
		{
			return left.Equals(right);
		}

		// Token: 0x06005C5E RID: 23646 RVA: 0x002F77DC File Offset: 0x002F59DC
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Sprite>.Default.GetHashCode(this.<Sprite>k__BackingField) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<Color>k__BackingField);
		}

		// Token: 0x06005C5F RID: 23647 RVA: 0x002F7805 File Offset: 0x002F5A05
		[NullableContext(0)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is MiniMapSprite && this.Equals((MiniMapSprite)obj);
		}

		// Token: 0x06005C60 RID: 23648 RVA: 0x002F781D File Offset: 0x002F5A1D
		[CompilerGenerated]
		public bool Equals(MiniMapSprite other)
		{
			return EqualityComparer<Sprite>.Default.Equals(this.<Sprite>k__BackingField, other.<Sprite>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField);
		}

		// Token: 0x06005C61 RID: 23649 RVA: 0x002F784F File Offset: 0x002F5A4F
		[CompilerGenerated]
		public void Deconstruct(out Sprite Sprite, out Color Color)
		{
			Sprite = this.Sprite;
			Color = this.Color;
		}
	}
}
