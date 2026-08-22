using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000342 RID: 834
	internal class PosInfo
	{
		// Token: 0x1700116E RID: 4462
		// (get) Token: 0x060041BE RID: 16830 RVA: 0x002471D1 File Offset: 0x002453D1
		// (set) Token: 0x060041BF RID: 16831 RVA: 0x002471D9 File Offset: 0x002453D9
		public Vector2 Position { get; private set; }

		// Token: 0x1700116F RID: 4463
		// (get) Token: 0x060041C0 RID: 16832 RVA: 0x002471E2 File Offset: 0x002453E2
		// (set) Token: 0x060041C1 RID: 16833 RVA: 0x002471EA File Offset: 0x002453EA
		public float? Rotation { get; private set; }

		// Token: 0x17001170 RID: 4464
		// (get) Token: 0x060041C2 RID: 16834 RVA: 0x002471F3 File Offset: 0x002453F3
		// (set) Token: 0x060041C3 RID: 16835 RVA: 0x002471FB File Offset: 0x002453FB
		public Vector2 LinearVelocity { get; private set; }

		// Token: 0x17001171 RID: 4465
		// (get) Token: 0x060041C4 RID: 16836 RVA: 0x00247204 File Offset: 0x00245404
		// (set) Token: 0x060041C5 RID: 16837 RVA: 0x0024720C File Offset: 0x0024540C
		public float? AngularVelocity { get; private set; }

		// Token: 0x060041C6 RID: 16838 RVA: 0x00247215 File Offset: 0x00245415
		public PosInfo(Vector2 pos, float? rotation, Vector2 linearVelocity, float? angularVelocity, float time) : this(pos, rotation, linearVelocity, angularVelocity, 0, time)
		{
		}

		// Token: 0x060041C7 RID: 16839 RVA: 0x00247225 File Offset: 0x00245425
		public PosInfo(Vector2 pos, float? rotation, Vector2 linearVelocity, float? angularVelocity, ushort ID) : this(pos, rotation, linearVelocity, angularVelocity, ID, 0f)
		{
		}

		// Token: 0x060041C8 RID: 16840 RVA: 0x00247239 File Offset: 0x00245439
		protected PosInfo(Vector2 pos, float? rotation, Vector2 linearVelocity, float? angularVelocity, ushort ID, float time)
		{
			this.Position = pos;
			this.Rotation = rotation;
			this.LinearVelocity = linearVelocity;
			this.AngularVelocity = angularVelocity;
			this.ID = ID;
			this.Timestamp = time;
		}

		// Token: 0x060041C9 RID: 16841 RVA: 0x0024726E File Offset: 0x0024546E
		public void TransformOutToInside(Submarine submarine)
		{
			this.Position -= ConvertUnits.ToSimUnits(submarine.Position);
		}

		// Token: 0x060041CA RID: 16842 RVA: 0x0024728C File Offset: 0x0024548C
		public void TransformInToOutside()
		{
			Submarine sub = Submarine.FindContainingInLocalCoordinates(ConvertUnits.ToDisplayUnits(this.Position), 500f);
			if (sub != null)
			{
				this.Position += ConvertUnits.ToSimUnits(sub.Position);
			}
		}

		// Token: 0x060041CB RID: 16843 RVA: 0x002472D0 File Offset: 0x002454D0
		public void Translate(Vector2 posAmount, float rotationAmount)
		{
			this.Position += posAmount;
			this.Rotation += rotationAmount;
		}

		// Token: 0x0400224F RID: 8783
		public readonly float Timestamp;

		// Token: 0x04002250 RID: 8784
		public readonly ushort ID;
	}
}
