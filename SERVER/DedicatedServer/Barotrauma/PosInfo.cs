using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200026F RID: 623
	internal class PosInfo
	{
		// Token: 0x17000D49 RID: 3401
		// (get) Token: 0x06002C9F RID: 11423 RVA: 0x00126E6D File Offset: 0x0012506D
		// (set) Token: 0x06002CA0 RID: 11424 RVA: 0x00126E75 File Offset: 0x00125075
		public Vector2 Position { get; private set; }

		// Token: 0x17000D4A RID: 3402
		// (get) Token: 0x06002CA1 RID: 11425 RVA: 0x00126E7E File Offset: 0x0012507E
		// (set) Token: 0x06002CA2 RID: 11426 RVA: 0x00126E86 File Offset: 0x00125086
		public float? Rotation { get; private set; }

		// Token: 0x17000D4B RID: 3403
		// (get) Token: 0x06002CA3 RID: 11427 RVA: 0x00126E8F File Offset: 0x0012508F
		// (set) Token: 0x06002CA4 RID: 11428 RVA: 0x00126E97 File Offset: 0x00125097
		public Vector2 LinearVelocity { get; private set; }

		// Token: 0x17000D4C RID: 3404
		// (get) Token: 0x06002CA5 RID: 11429 RVA: 0x00126EA0 File Offset: 0x001250A0
		// (set) Token: 0x06002CA6 RID: 11430 RVA: 0x00126EA8 File Offset: 0x001250A8
		public float? AngularVelocity { get; private set; }

		// Token: 0x06002CA7 RID: 11431 RVA: 0x00126EB1 File Offset: 0x001250B1
		public PosInfo(Vector2 pos, float? rotation, Vector2 linearVelocity, float? angularVelocity, float time) : this(pos, rotation, linearVelocity, angularVelocity, 0, time)
		{
		}

		// Token: 0x06002CA8 RID: 11432 RVA: 0x00126EC1 File Offset: 0x001250C1
		public PosInfo(Vector2 pos, float? rotation, Vector2 linearVelocity, float? angularVelocity, ushort ID) : this(pos, rotation, linearVelocity, angularVelocity, ID, 0f)
		{
		}

		// Token: 0x06002CA9 RID: 11433 RVA: 0x00126ED5 File Offset: 0x001250D5
		protected PosInfo(Vector2 pos, float? rotation, Vector2 linearVelocity, float? angularVelocity, ushort ID, float time)
		{
			this.Position = pos;
			this.Rotation = rotation;
			this.LinearVelocity = linearVelocity;
			this.AngularVelocity = angularVelocity;
			this.ID = ID;
			this.Timestamp = time;
		}

		// Token: 0x06002CAA RID: 11434 RVA: 0x00126F0A File Offset: 0x0012510A
		public void TransformOutToInside(Submarine submarine)
		{
			this.Position -= ConvertUnits.ToSimUnits(submarine.Position);
		}

		// Token: 0x06002CAB RID: 11435 RVA: 0x00126F28 File Offset: 0x00125128
		public void TransformInToOutside()
		{
			Submarine sub = Submarine.FindContainingInLocalCoordinates(ConvertUnits.ToDisplayUnits(this.Position), 500f);
			if (sub != null)
			{
				this.Position += ConvertUnits.ToSimUnits(sub.Position);
			}
		}

		// Token: 0x06002CAC RID: 11436 RVA: 0x00126F6C File Offset: 0x0012516C
		public void Translate(Vector2 posAmount, float rotationAmount)
		{
			this.Position += posAmount;
			this.Rotation += rotationAmount;
		}

		// Token: 0x04001607 RID: 5639
		public readonly float Timestamp;

		// Token: 0x04001608 RID: 5640
		public readonly ushort ID;
	}
}
