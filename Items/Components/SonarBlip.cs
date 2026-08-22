using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005C8 RID: 1480
	internal class SonarBlip
	{
		// Token: 0x06005D90 RID: 23952 RVA: 0x0030A7E0 File Offset: 0x003089E0
		public SonarBlip(Vector2 pos, float fadeTimer, float scale, Sonar.BlipType blipType = Sonar.BlipType.Default)
		{
			this.Position = pos;
			this.FadeTimer = Math.Max(fadeTimer, 0f);
			this.Scale = scale;
			this.Size = new Vector2(0.5f, 1f);
			this.BlipType = blipType;
		}

		// Token: 0x04003027 RID: 12327
		public float FadeTimer;

		// Token: 0x04003028 RID: 12328
		public Vector2 Position;

		// Token: 0x04003029 RID: 12329
		public float Scale;

		// Token: 0x0400302A RID: 12330
		public Vector2 Velocity;

		// Token: 0x0400302B RID: 12331
		public float? Rotation;

		// Token: 0x0400302C RID: 12332
		public Vector2 Size;

		// Token: 0x0400302D RID: 12333
		public Sonar.BlipType BlipType;

		// Token: 0x0400302E RID: 12334
		public float Alpha = 1f;
	}
}
