using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000E2 RID: 226
	internal struct WaterVertexData
	{
		// Token: 0x06001F3C RID: 7996 RVA: 0x0013431F File Offset: 0x0013251F
		public WaterVertexData(float distortStrengthX, float distortStrengthY, float waterColorStrength, float waterAlpha)
		{
			this.DistortStrengthX = distortStrengthX;
			this.DistortStrengthY = distortStrengthY;
			this.WaterColorStrength = waterColorStrength;
			this.WaterAlpha = waterAlpha;
		}

		// Token: 0x06001F3D RID: 7997 RVA: 0x0013433E File Offset: 0x0013253E
		public static implicit operator Color(WaterVertexData wd)
		{
			return new Color(wd.DistortStrengthX, wd.DistortStrengthY, wd.WaterColorStrength, wd.WaterAlpha);
		}

		// Token: 0x04000FFD RID: 4093
		private float DistortStrengthX;

		// Token: 0x04000FFE RID: 4094
		private float DistortStrengthY;

		// Token: 0x04000FFF RID: 4095
		private float WaterColorStrength;

		// Token: 0x04001000 RID: 4096
		private float WaterAlpha;
	}
}
