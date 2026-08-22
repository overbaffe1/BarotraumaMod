using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000D7 RID: 215
	internal class LimbJoint
	{
		// Token: 0x170006D8 RID: 1752
		// (get) Token: 0x0600176B RID: 5995 RVA: 0x000C2F3F File Offset: 0x000C113F
		public bool CanBeSevered
		{
			get
			{
				return this.Params.CanBeSevered;
			}
		}

		// Token: 0x170006D9 RID: 1753
		// (get) Token: 0x0600176C RID: 5996 RVA: 0x000C2F4C File Offset: 0x000C114C
		public float Scale
		{
			get
			{
				return this.Params.Scale * this.ragdoll.RagdollParams.JointScale;
			}
		}

		// Token: 0x170006DA RID: 1754
		// (get) Token: 0x0600176D RID: 5997 RVA: 0x000C2F6A File Offset: 0x000C116A
		public Joint Joint
		{
			get
			{
				return this.revoluteJoint ?? this.weldJoint;
			}
		}

		// Token: 0x170006DB RID: 1755
		// (get) Token: 0x0600176E RID: 5998 RVA: 0x000C2F7C File Offset: 0x000C117C
		// (set) Token: 0x0600176F RID: 5999 RVA: 0x000C2F89 File Offset: 0x000C1189
		public bool Enabled
		{
			get
			{
				return this.Joint.Enabled;
			}
			set
			{
				this.Joint.Enabled = value;
			}
		}

		// Token: 0x170006DC RID: 1756
		// (get) Token: 0x06001770 RID: 6000 RVA: 0x000C2F97 File Offset: 0x000C1197
		public Body BodyA
		{
			get
			{
				return this.Joint.BodyA;
			}
		}

		// Token: 0x170006DD RID: 1757
		// (get) Token: 0x06001771 RID: 6001 RVA: 0x000C2FA4 File Offset: 0x000C11A4
		public Body BodyB
		{
			get
			{
				return this.Joint.BodyB;
			}
		}

		// Token: 0x170006DE RID: 1758
		// (get) Token: 0x06001772 RID: 6002 RVA: 0x000C2FB1 File Offset: 0x000C11B1
		// (set) Token: 0x06001773 RID: 6003 RVA: 0x000C2FBE File Offset: 0x000C11BE
		public Vector2 WorldAnchorA
		{
			get
			{
				return this.Joint.WorldAnchorA;
			}
			set
			{
				this.Joint.WorldAnchorA = value;
			}
		}

		// Token: 0x170006DF RID: 1759
		// (get) Token: 0x06001774 RID: 6004 RVA: 0x000C2FCC File Offset: 0x000C11CC
		// (set) Token: 0x06001775 RID: 6005 RVA: 0x000C2FD9 File Offset: 0x000C11D9
		public Vector2 WorldAnchorB
		{
			get
			{
				return this.Joint.WorldAnchorB;
			}
			set
			{
				this.Joint.WorldAnchorB = value;
			}
		}

		// Token: 0x170006E0 RID: 1760
		// (get) Token: 0x06001776 RID: 6006 RVA: 0x000C2FE7 File Offset: 0x000C11E7
		// (set) Token: 0x06001777 RID: 6007 RVA: 0x000C3008 File Offset: 0x000C1208
		public Vector2 LocalAnchorA
		{
			get
			{
				if (this.revoluteJoint == null)
				{
					return this.weldJoint.LocalAnchorA;
				}
				return this.revoluteJoint.LocalAnchorA;
			}
			set
			{
				if (this.weldJoint != null)
				{
					this.weldJoint.LocalAnchorA = value;
					return;
				}
				this.revoluteJoint.LocalAnchorA = value;
			}
		}

		// Token: 0x170006E1 RID: 1761
		// (get) Token: 0x06001778 RID: 6008 RVA: 0x000C302B File Offset: 0x000C122B
		// (set) Token: 0x06001779 RID: 6009 RVA: 0x000C304C File Offset: 0x000C124C
		public Vector2 LocalAnchorB
		{
			get
			{
				if (this.revoluteJoint == null)
				{
					return this.weldJoint.LocalAnchorB;
				}
				return this.revoluteJoint.LocalAnchorB;
			}
			set
			{
				if (this.weldJoint != null)
				{
					this.weldJoint.LocalAnchorB = value;
					return;
				}
				this.revoluteJoint.LocalAnchorB = value;
			}
		}

		// Token: 0x170006E2 RID: 1762
		// (get) Token: 0x0600177A RID: 6010 RVA: 0x000C306F File Offset: 0x000C126F
		// (set) Token: 0x0600177B RID: 6011 RVA: 0x000C3086 File Offset: 0x000C1286
		public bool LimitEnabled
		{
			get
			{
				return this.revoluteJoint != null && this.revoluteJoint.LimitEnabled;
			}
			set
			{
				if (this.revoluteJoint != null)
				{
					this.revoluteJoint.LimitEnabled = value;
				}
			}
		}

		// Token: 0x170006E3 RID: 1763
		// (get) Token: 0x0600177C RID: 6012 RVA: 0x000C309C File Offset: 0x000C129C
		// (set) Token: 0x0600177D RID: 6013 RVA: 0x000C30B7 File Offset: 0x000C12B7
		public float LowerLimit
		{
			get
			{
				if (this.revoluteJoint == null)
				{
					return 0f;
				}
				return this.revoluteJoint.LowerLimit;
			}
			set
			{
				if (this.revoluteJoint != null)
				{
					this.revoluteJoint.LowerLimit = value;
				}
			}
		}

		// Token: 0x170006E4 RID: 1764
		// (get) Token: 0x0600177E RID: 6014 RVA: 0x000C30CD File Offset: 0x000C12CD
		// (set) Token: 0x0600177F RID: 6015 RVA: 0x000C30E8 File Offset: 0x000C12E8
		public float UpperLimit
		{
			get
			{
				if (this.revoluteJoint == null)
				{
					return 0f;
				}
				return this.revoluteJoint.UpperLimit;
			}
			set
			{
				if (this.revoluteJoint != null)
				{
					this.revoluteJoint.UpperLimit = value;
				}
			}
		}

		// Token: 0x170006E5 RID: 1765
		// (get) Token: 0x06001780 RID: 6016 RVA: 0x000C30FE File Offset: 0x000C12FE
		public float JointAngle
		{
			get
			{
				if (this.revoluteJoint == null)
				{
					return this.weldJoint.ReferenceAngle;
				}
				return this.revoluteJoint.JointAngle;
			}
		}

		// Token: 0x06001781 RID: 6017 RVA: 0x000C311F File Offset: 0x000C131F
		public LimbJoint(Limb limbA, Limb limbB, RagdollParams.JointParams jointParams, Ragdoll ragdoll) : this(limbA, limbB, Vector2.One, Vector2.One, jointParams.WeldJoint)
		{
			this.Params = jointParams;
			this.ragdoll = ragdoll;
			this.LoadParams();
		}

		// Token: 0x06001782 RID: 6018 RVA: 0x000C3150 File Offset: 0x000C1350
		public LimbJoint(Limb limbA, Limb limbB, Vector2 anchor1, Vector2 anchor2, bool weld = false)
		{
			if (weld)
			{
				this.weldJoint = new WeldJoint(limbA.body.FarseerBody, limbB.body.FarseerBody, anchor1, anchor2, false);
			}
			else
			{
				this.revoluteJoint = new RevoluteJoint(limbA.body.FarseerBody, limbB.body.FarseerBody, anchor1, anchor2, false)
				{
					MotorEnabled = true,
					MaxMotorTorque = 0.25f
				};
			}
			this.Joint.CollideConnected = false;
			this.LimbA = limbA;
			this.LimbB = limbB;
		}

		// Token: 0x06001783 RID: 6019 RVA: 0x000C31E0 File Offset: 0x000C13E0
		public void LoadParams()
		{
			if (this.revoluteJoint != null)
			{
				this.revoluteJoint.MaxMotorTorque = this.Params.Stiffness;
				this.revoluteJoint.LimitEnabled = this.Params.LimitEnabled;
			}
			if (float.IsNaN(this.Params.LowerLimit))
			{
				this.Params.LowerLimit = 0f;
			}
			if (float.IsNaN(this.Params.UpperLimit))
			{
				this.Params.UpperLimit = 0f;
			}
			if (this.ragdoll.IsFlipped)
			{
				if (this.weldJoint != null)
				{
					this.weldJoint.LocalAnchorA = ConvertUnits.ToSimUnits(new Vector2(-this.Params.Limb1Anchor.X, this.Params.Limb1Anchor.Y) * this.Scale);
					this.weldJoint.LocalAnchorB = ConvertUnits.ToSimUnits(new Vector2(-this.Params.Limb2Anchor.X, this.Params.Limb2Anchor.Y) * this.Scale);
					return;
				}
				this.revoluteJoint.LocalAnchorA = ConvertUnits.ToSimUnits(new Vector2(-this.Params.Limb1Anchor.X, this.Params.Limb1Anchor.Y) * this.Scale);
				this.revoluteJoint.LocalAnchorB = ConvertUnits.ToSimUnits(new Vector2(-this.Params.Limb2Anchor.X, this.Params.Limb2Anchor.Y) * this.Scale);
				this.revoluteJoint.UpperLimit = MathHelper.ToRadians(-this.Params.LowerLimit);
				this.revoluteJoint.LowerLimit = MathHelper.ToRadians(-this.Params.UpperLimit);
				return;
			}
			else
			{
				if (this.weldJoint != null)
				{
					this.weldJoint.LocalAnchorA = ConvertUnits.ToSimUnits(this.Params.Limb1Anchor * this.Scale);
					this.weldJoint.LocalAnchorB = ConvertUnits.ToSimUnits(this.Params.Limb2Anchor * this.Scale);
					return;
				}
				this.revoluteJoint.LocalAnchorA = ConvertUnits.ToSimUnits(this.Params.Limb1Anchor * this.Scale);
				this.revoluteJoint.LocalAnchorB = ConvertUnits.ToSimUnits(this.Params.Limb2Anchor * this.Scale);
				this.revoluteJoint.UpperLimit = MathHelper.ToRadians(this.Params.UpperLimit);
				this.revoluteJoint.LowerLimit = MathHelper.ToRadians(this.Params.LowerLimit);
				return;
			}
		}

		// Token: 0x04000B6C RID: 2924
		public bool IsSevered;

		// Token: 0x04000B6D RID: 2925
		public readonly RagdollParams.JointParams Params;

		// Token: 0x04000B6E RID: 2926
		public readonly Ragdoll ragdoll;

		// Token: 0x04000B6F RID: 2927
		public readonly Limb LimbA;

		// Token: 0x04000B70 RID: 2928
		public readonly Limb LimbB;

		// Token: 0x04000B71 RID: 2929
		public readonly RevoluteJoint revoluteJoint;

		// Token: 0x04000B72 RID: 2930
		public readonly WeldJoint weldJoint;
	}
}
