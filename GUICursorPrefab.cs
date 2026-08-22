using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000099 RID: 153
	[NullableContext(1)]
	[Nullable(0)]
	public class GUICursorPrefab : GUIPrefab
	{
		// Token: 0x06001410 RID: 5136 RVA: 0x000BF068 File Offset: 0x000BD268
		public GUICursorPrefab(ContentXElement element, UIStyleFile file) : base(element, file)
		{
			this.Sprites = new Sprite[Enum.GetValues(typeof(CursorState)).Length];
			foreach (ContentXElement subElement in element.Elements())
			{
				ContentXElement contentXElement = subElement;
				string key = "state";
				CursorState cursorState = CursorState.Default;
				CursorState state = contentXElement.GetAttributeEnum<CursorState>(key, cursorState);
				this.Sprites[(int)state] = new Sprite(subElement, "", "", false, 1f);
			}
		}

		// Token: 0x06001411 RID: 5137 RVA: 0x000BF104 File Offset: 0x000BD304
		public override void Dispose()
		{
			foreach (Sprite sprite in this.Sprites)
			{
				if (sprite != null)
				{
					sprite.Remove();
				}
			}
		}

		// Token: 0x040009E7 RID: 2535
		public readonly Sprite[] Sprites;
	}
}
