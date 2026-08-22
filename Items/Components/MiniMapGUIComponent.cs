using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005BD RID: 1469
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct MiniMapGUIComponent : IEquatable<MiniMapGUIComponent>
	{
		// Token: 0x06005C45 RID: 23621 RVA: 0x002F757A File Offset: 0x002F577A
		public MiniMapGUIComponent(GUIComponent RectComponent, GUIComponent BorderComponent)
		{
			this.RectComponent = RectComponent;
			this.BorderComponent = BorderComponent;
		}

		// Token: 0x17001740 RID: 5952
		// (get) Token: 0x06005C46 RID: 23622 RVA: 0x002F758A File Offset: 0x002F578A
		// (set) Token: 0x06005C47 RID: 23623 RVA: 0x002F7592 File Offset: 0x002F5792
		public GUIComponent RectComponent { get; set; }

		// Token: 0x17001741 RID: 5953
		// (get) Token: 0x06005C48 RID: 23624 RVA: 0x002F759B File Offset: 0x002F579B
		// (set) Token: 0x06005C49 RID: 23625 RVA: 0x002F75A3 File Offset: 0x002F57A3
		public GUIComponent BorderComponent { get; set; }

		// Token: 0x06005C4A RID: 23626 RVA: 0x002F75AC File Offset: 0x002F57AC
		public MiniMapGUIComponent(GUIComponent rectComponent)
		{
			this = new MiniMapGUIComponent(rectComponent, rectComponent);
		}

		// Token: 0x06005C4B RID: 23627 RVA: 0x002F75B6 File Offset: 0x002F57B6
		public void Deconstruct(out GUIComponent component, out GUIComponent borderComponent)
		{
			component = this.RectComponent;
			borderComponent = this.BorderComponent;
		}

		// Token: 0x06005C4C RID: 23628 RVA: 0x002F75C8 File Offset: 0x002F57C8
		[NullableContext(0)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("MiniMapGUIComponent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06005C4D RID: 23629 RVA: 0x002F7614 File Offset: 0x002F5814
		[NullableContext(0)]
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("RectComponent = ");
			builder.Append(this.RectComponent);
			builder.Append(", BorderComponent = ");
			builder.Append(this.BorderComponent);
			return true;
		}

		// Token: 0x06005C4E RID: 23630 RVA: 0x002F7649 File Offset: 0x002F5849
		[CompilerGenerated]
		public static bool operator !=(MiniMapGUIComponent left, MiniMapGUIComponent right)
		{
			return !(left == right);
		}

		// Token: 0x06005C4F RID: 23631 RVA: 0x002F7655 File Offset: 0x002F5855
		[CompilerGenerated]
		public static bool operator ==(MiniMapGUIComponent left, MiniMapGUIComponent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06005C50 RID: 23632 RVA: 0x002F765F File Offset: 0x002F585F
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<GUIComponent>.Default.GetHashCode(this.<RectComponent>k__BackingField) * -1521134295 + EqualityComparer<GUIComponent>.Default.GetHashCode(this.<BorderComponent>k__BackingField);
		}

		// Token: 0x06005C51 RID: 23633 RVA: 0x002F7688 File Offset: 0x002F5888
		[NullableContext(0)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is MiniMapGUIComponent && this.Equals((MiniMapGUIComponent)obj);
		}

		// Token: 0x06005C52 RID: 23634 RVA: 0x002F76A0 File Offset: 0x002F58A0
		[CompilerGenerated]
		public bool Equals(MiniMapGUIComponent other)
		{
			return EqualityComparer<GUIComponent>.Default.Equals(this.<RectComponent>k__BackingField, other.<RectComponent>k__BackingField) && EqualityComparer<GUIComponent>.Default.Equals(this.<BorderComponent>k__BackingField, other.<BorderComponent>k__BackingField);
		}
	}
}
