using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000093 RID: 147
	public class GUIColorPrefab : GUIPrefab
	{
		// Token: 0x060013F9 RID: 5113 RVA: 0x000BEE20 File Offset: 0x000BD020
		[NullableContext(1)]
		public GUIColorPrefab(ContentXElement element, UIStyleFile file) : base(element, file)
		{
			string key = "color";
			Color white = Color.White;
			this.Color = element.GetAttributeColor(key, white);
		}

		// Token: 0x060013FA RID: 5114 RVA: 0x000BEE4E File Offset: 0x000BD04E
		public override void Dispose()
		{
		}

		// Token: 0x040009E3 RID: 2531
		public readonly Color Color;
	}
}
