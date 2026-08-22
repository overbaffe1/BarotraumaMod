using System;
using System.Runtime.CompilerServices;
using Barotrauma.SpriteDeformations;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000032 RID: 50
	internal class LimbJoint
	{
		// Token: 0x06000812 RID: 2066 RVA: 0x00049724 File Offset: 0x00047924
		public void UpdateDeformations(float deltaTime)
		{
			float diff = Math.Abs(this.UpperLimit - this.LowerLimit);
			float strength = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, 3.1415927f, diff));
			float jointAngle = this.JointAngle * strength;
			JointBendDeformation limbADeformation = this.LimbA.ActiveDeformations.Find((SpriteDeformation d) => d is JointBendDeformation) as JointBendDeformation;
			JointBendDeformation limbBDeformation = this.LimbB.ActiveDeformations.Find((SpriteDeformation d) => d is JointBendDeformation) as JointBendDeformation;
			if (limbADeformation != null && limbBDeformation != null)
			{
				LimbJoint.<UpdateDeformations>g__UpdateBend|0_2(this.LimbA, limbADeformation, this.LocalAnchorA, -jointAngle);
				LimbJoint.<UpdateDeformations>g__UpdateBend|0_2(this.LimbB, limbBDeformation, this.LocalAnchorB, jointAngle);
			}
		}

		// Token: 0x06000813 RID: 2067 RVA: 0x00049807 File Offset: 0x00047A07
		public void Draw(SpriteBatch spriteBatch)
		{
		}

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x06000814 RID: 2068 RVA: 0x00049809 File Offset: 0x00047A09
		public bool CanBeSevered
		{
			get
			{
				return this.Params.CanBeSevered;
			}
		}

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x06000815 RID: 2069 RVA: 0x00049816 File Offset: 0x00047A16
		public float Scale
		{
			get
			{
				return this.Params.Scale * this.ragdoll.RagdollParams.JointScale;
			}
		}

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x06000816 RID: 2070 RVA: 0x00049834 File Offset: 0x00047A34
		public Joint Joint
		{
			get
			{
				return this.revoluteJoint ?? this.weldJoint;
			}
		}

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06000817 RID: 2071 RVA: 0x00049846 File Offset: 0x00047A46
		// (set) Token: 0x06000818 RID: 2072 RVA: 0x00049853 File Offset: 0x00047A53
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

		// Token: 0x17000240 RID: 576
		// (get) Token: 0x06000819 RID: 2073 RVA: 0x00049861 File Offset: 0x00047A61
		public Body BodyA
		{
			get
			{
				return this.Joint.BodyA;
			}
		}

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x0600081A RID: 2074 RVA: 0x0004986E File Offset: 0x00047A6E
		public Body BodyB
		{
			get
			{
				return this.Joint.BodyB;
			}
		}

		// Token: 0x17000242 RID: 578
		// (get) Token: 0x0600081B RID: 2075 RVA: 0x0004987B File Offset: 0x00047A7B
		// (set) Token: 0x0600081C RID: 2076 RVA: 0x00049888 File Offset: 0x00047A88
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

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x0600081D RID: 2077 RVA: 0x00049896 File Offset: 0x00047A96
		// (set) Token: 0x0600081E RID: 2078 RVA: 0x000498A3 File Offset: 0x00047AA3
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

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x0600081F RID: 2079 RVA: 0x000498B1 File Offset: 0x00047AB1
		// (set) Token: 0x06000820 RID: 2080 RVA: 0x000498D2 File Offset: 0x00047AD2
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

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x06000821 RID: 2081 RVA: 0x000498F5 File Offset: 0x00047AF5
		// (set) Token: 0x06000822 RID: 2082 RVA: 0x00049916 File Offset: 0x00047B16
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

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x06000823 RID: 2083 RVA: 0x00049939 File Offset: 0x00047B39
		// (set) Token: 0x06000824 RID: 2084 RVA: 0x00049950 File Offset: 0x00047B50
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

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x06000825 RID: 2085 RVA: 0x00049966 File Offset: 0x00047B66
		// (set) Token: 0x06000826 RID: 2086 RVA: 0x00049981 File Offset: 0x00047B81
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

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x06000827 RID: 2087 RVA: 0x00049997 File Offset: 0x00047B97
		// (set) Token: 0x06000828 RID: 2088 RVA: 0x000499B2 File Offset: 0x00047BB2
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

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x06000829 RID: 2089 RVA: 0x000499C8 File Offset: 0x00047BC8
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

		// Token: 0x0600082A RID: 2090 RVA: 0x000499E9 File Offset: 0x00047BE9
		public LimbJoint(Limb limbA, Limb limbB, RagdollParams.JointParams jointParams, Ragdoll ragdoll) : this(limbA, limbB, Vector2.One, Vector2.One, jointParams.WeldJoint)
		{
			this.Params = jointParams;
			this.ragdoll = ragdoll;
			this.LoadParams();
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x00049A18 File Offset: 0x00047C18
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

		// Token: 0x0600082C RID: 2092 RVA: 0x00049AA8 File Offset: 0x00047CA8
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

		// Token: 0x0600082D RID: 2093 RVA: 0x00049D60 File Offset: 0x00047F60
		[CompilerGenerated]
		internal static void <UpdateDeformations>g__UpdateBend|0_2(Limb limb, JointBendDeformation deformation, Vector2 localAnchor, float angle)
		{
			deformation.Scale = limb.DeformSprite.Size;
			Vector2 displayAnchor = ConvertUnits.ToDisplayUnits(localAnchor);
			displayAnchor.Y = -displayAnchor.Y;
			Vector2 refPos = displayAnchor + limb.DeformSprite.Origin;
			refPos.X /= limb.DeformSprite.Size.X;
			refPos.Y /= limb.DeformSprite.Size.Y;
			if (Math.Abs(localAnchor.X) > Math.Abs(localAnchor.Y))
			{
				if (localAnchor.X > 0f)
				{
					deformation.BendRightRefPos = refPos;
					deformation.BendRight = angle;
					return;
				}
				deformation.BendLeftRefPos = refPos;
				deformation.BendLeft = angle;
				return;
			}
			else
			{
				if (localAnchor.Y > 0f)
				{
					deformation.BendUpRefPos = refPos;
					deformation.BendUp = angle;
					return;
				}
				deformation.BendDownRefPos = refPos;
				deformation.BendDown = angle;
				return;
			}
		}

		// Token: 0x0400043A RID: 1082
		public bool IsSevered;

		// Token: 0x0400043B RID: 1083
		public readonly RagdollParams.JointParams Params;

		// Token: 0x0400043C RID: 1084
		public readonly Ragdoll ragdoll;

		// Token: 0x0400043D RID: 1085
		public readonly Limb LimbA;

		// Token: 0x0400043E RID: 1086
		public readonly Limb LimbB;

		// Token: 0x0400043F RID: 1087
		public readonly RevoluteJoint revoluteJoint;

		// Token: 0x04000440 RID: 1088
		public readonly WeldJoint weldJoint;
	}
}
