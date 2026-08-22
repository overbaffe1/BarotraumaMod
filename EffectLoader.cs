using System;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200014F RID: 335
	internal static class EffectLoader
	{
		// Token: 0x06002A0E RID: 10766 RVA: 0x001D148F File Offset: 0x001CF68F
		public static Effect Load(string path)
		{
			return GameMain.Instance.Content.Load<Effect>(path);
		}
	}
}
