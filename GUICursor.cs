using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200009A RID: 154
	[NullableContext(2)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	public class GUICursor : GUISelector<GUICursorPrefab>
	{
		// Token: 0x06001412 RID: 5138 RVA: 0x000BF133 File Offset: 0x000BD333
		[NullableContext(1)]
		public GUICursor(string identifier) : base(identifier)
		{
		}

		// Token: 0x17000511 RID: 1297
		public Sprite this[CursorState k]
		{
			get
			{
				GUICursorPrefab activePrefab = this.Prefabs.ActivePrefab;
				if (activePrefab == null)
				{
					return null;
				}
				return activePrefab.Sprites[(int)k];
			}
		}
	}
}
