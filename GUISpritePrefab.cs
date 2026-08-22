using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000095 RID: 149
	[NullableContext(1)]
	[Nullable(0)]
	public class GUISpritePrefab : GUIPrefab
	{
		// Token: 0x060013FF RID: 5119 RVA: 0x000BEECE File Offset: 0x000BD0CE
		public GUISpritePrefab(ContentXElement element, UIStyleFile file) : base(element, file)
		{
			this.Sprite = new UISprite(element);
		}

		// Token: 0x06001400 RID: 5120 RVA: 0x000BEEE4 File Offset: 0x000BD0E4
		public override void Dispose()
		{
			this.Sprite.Sprite.Remove();
		}

		// Token: 0x040009E5 RID: 2533
		public readonly UISprite Sprite;
	}
}
