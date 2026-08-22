using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000046 RID: 70
	internal class PhysicsBody
	{
		// Token: 0x06000B56 RID: 2902 RVA: 0x0006D850 File Offset: 0x0006BA50
		public void ServerWrite(IWriteMessage msg)
		{
			float MaxVel = 64f;
			float MaxAngularVel = 16f;
			msg.WriteSingle(this.SimPosition.X);
			msg.WriteSingle(this.SimPosition.Y);
			msg.WriteBoolean(this.FarseerBody.Awake);
			msg.WriteBoolean(this.FarseerBody.FixedRotation);
			if (!this.FarseerBody.FixedRotation)
			{
				msg.WriteRangedSingle(MathUtils.WrapAngleTwoPi(this.FarseerBody.Rotation), 0f, 6.2831855f, 8);
			}
			if (this.FarseerBody.Awake)
			{
				this.FarseerBody.Enabled = true;
				this.FarseerBody.LinearVelocity = new Vector2(MathHelper.Clamp(this.FarseerBody.LinearVelocity.X, -MaxVel, MaxVel), MathHelper.Clamp(this.FarseerBody.LinearVelocity.Y, -MaxVel, MaxVel));
				msg.WriteRangedSingle(this.FarseerBody.LinearVelocity.X, -MaxVel, MaxVel, 12);
				msg.WriteRangedSingle(this.FarseerBody.LinearVelocity.Y, -MaxVel, MaxVel, 12);
				if (!this.FarseerBody.FixedRotation)
				{
					this.FarseerBody.AngularVelocity = MathHelper.Clamp(this.FarseerBody.AngularVelocity, -MaxAngularVel, MaxAngularVel);
					msg.WriteRangedSingle(this.FarseerBody.AngularVelocity, -MaxAngularVel, MaxAngularVel, 8);
				}
			}
			msg.WritePadBits();
		}

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x06000B57 RID: 2903 RVA: 0x0006D9B3 File Offset: 0x0006BBB3
		public static List<PhysicsBody> List
		{
			get
			{
				return PhysicsBody.list;
			}
		}

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x06000B58 RID: 2904 RVA: 0x0006D9BA File Offset: 0x0006BBBA
		// (set) Token: 0x06000B59 RID: 2905 RVA: 0x0006D9C2 File Offset: 0x0006BBC2
		public bool Removed { get; private set; }

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x06000B5A RID: 2906 RVA: 0x0006D9CB File Offset: 0x0006BBCB
		// (set) Token: 0x06000B5B RID: 2907 RVA: 0x0006D9D3 File Offset: 0x0006BBD3
		public Vector2 LastSentPosition { get; private set; }

		// Token: 0x1700030D RID: 781
		// (get) Token: 0x06000B5C RID: 2908 RVA: 0x0006D9DC File Offset: 0x0006BBDC
		// (set) Token: 0x06000B5D RID: 2909 RVA: 0x0006D9E4 File Offset: 0x0006BBE4
		public float Height { get; private set; }

		// Token: 0x1700030E RID: 782
		// (get) Token: 0x06000B5E RID: 2910 RVA: 0x0006D9ED File Offset: 0x0006BBED
		// (set) Token: 0x06000B5F RID: 2911 RVA: 0x0006D9F5 File Offset: 0x0006BBF5
		public float Width { get; private set; }

		// Token: 0x1700030F RID: 783
		// (get) Token: 0x06000B60 RID: 2912 RVA: 0x0006D9FE File Offset: 0x0006BBFE
		// (set) Token: 0x06000B61 RID: 2913 RVA: 0x0006DA06 File Offset: 0x0006BC06
		public float Radius { get; private set; }

		// Token: 0x17000310 RID: 784
		// (get) Token: 0x06000B62 RID: 2914 RVA: 0x0006DA0F File Offset: 0x0006BC0F
		public PhysicsBody.Shape BodyShape
		{
			get
			{
				return this.bodyShape;
			}
		}

		// Token: 0x17000311 RID: 785
		// (get) Token: 0x06000B63 RID: 2915 RVA: 0x0006DA17 File Offset: 0x0006BC17
		// (set) Token: 0x06000B64 RID: 2916 RVA: 0x0006DA20 File Offset: 0x0006BC20
		public Vector2? TargetPosition
		{
			get
			{
				return this.targetPosition;
			}
			set
			{
				if (value == null)
				{
					this.targetPosition = null;
					return;
				}
				if (!this.IsValidValue(value.Value, "target position", -100000f, 100000f))
				{
					return;
				}
				this.targetPosition = new Vector2?(new Vector2(MathHelper.Clamp(value.Value.X, -10000f, 10000f), MathHelper.Clamp(value.Value.Y, -10000f, 10000f)));
			}
		}

		// Token: 0x17000312 RID: 786
		// (get) Token: 0x06000B65 RID: 2917 RVA: 0x0006DAA8 File Offset: 0x0006BCA8
		// (set) Token: 0x06000B66 RID: 2918 RVA: 0x0006DAB0 File Offset: 0x0006BCB0
		public float? TargetRotation
		{
			get
			{
				return this.targetRotation;
			}
			set
			{
				if (value == null)
				{
					this.targetRotation = null;
					return;
				}
				if (!this.IsValidValue(value.Value, "target rotation", -3.4028235E+38f, 3.4028235E+38f))
				{
					return;
				}
				this.targetRotation = value;
			}
		}

		// Token: 0x17000313 RID: 787
		// (get) Token: 0x06000B67 RID: 2919 RVA: 0x0006DAEE File Offset: 0x0006BCEE
		public Vector2 DrawPosition
		{
			get
			{
				if (this.Submarine != null)
				{
					return this.drawPosition + this.Submarine.DrawPosition;
				}
				return this.drawPosition;
			}
		}

		// Token: 0x17000314 RID: 788
		// (get) Token: 0x06000B68 RID: 2920 RVA: 0x0006DB15 File Offset: 0x0006BD15
		public float DrawRotation
		{
			get
			{
				return this.drawRotation;
			}
		}

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x06000B69 RID: 2921 RVA: 0x0006DB1D File Offset: 0x0006BD1D
		// (set) Token: 0x06000B6A RID: 2922 RVA: 0x0006DB25 File Offset: 0x0006BD25
		public float Dir
		{
			get
			{
				return this.dir;
			}
			set
			{
				this.dir = value;
			}
		}

		// Token: 0x17000316 RID: 790
		// (get) Token: 0x06000B6B RID: 2923 RVA: 0x0006DB2E File Offset: 0x0006BD2E
		// (set) Token: 0x06000B6C RID: 2924 RVA: 0x0006DB38 File Offset: 0x0006BD38
		public bool Enabled
		{
			get
			{
				return this.isEnabled;
			}
			set
			{
				this.isEnabled = value;
				try
				{
					this.FarseerBody.Enabled = (this.isEnabled && this.isPhysEnabled);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Exception in PhysicsBody.Enabled = ",
						value.ToString(),
						" (",
						this.isPhysEnabled.ToString(),
						")"
					}), e, null, false, false);
					if (this.UserData != null)
					{
						string str = "PhysicsBody UserData: ";
						Type type = this.UserData.GetType();
						DebugConsole.NewMessage(str + ((type != null) ? type.ToString() : null), new Color?(Color.Red), false);
					}
					if (GameMain.World.ContactManager == null)
					{
						DebugConsole.NewMessage("ContactManager is null!", new Color?(Color.Red), false);
					}
					else if (GameMain.World.ContactManager.BroadPhase == null)
					{
						DebugConsole.NewMessage("Broadphase is null!", new Color?(Color.Red), false);
					}
					if (this.FarseerBody.FixtureList == null)
					{
						DebugConsole.NewMessage("FixtureList is null!", new Color?(Color.Red), false);
					}
					Entity entity = this.UserData as Entity;
					if (entity != null)
					{
						string str2 = "Entity \"";
						Entity entity2 = entity;
						DebugConsole.NewMessage(str2 + ((entity2 != null) ? entity2.ToString() : null) + "\" removed!", new Color?(Color.Red), false);
					}
				}
			}
		}

		// Token: 0x17000317 RID: 791
		// (get) Token: 0x06000B6D RID: 2925 RVA: 0x0006DCB0 File Offset: 0x0006BEB0
		// (set) Token: 0x06000B6E RID: 2926 RVA: 0x0006DCBD File Offset: 0x0006BEBD
		public bool PhysEnabled
		{
			get
			{
				return this.FarseerBody.Enabled;
			}
			set
			{
				this.isPhysEnabled = value;
				if (this.Enabled)
				{
					this.FarseerBody.Enabled = value;
				}
			}
		}

		// Token: 0x17000318 RID: 792
		// (get) Token: 0x06000B6F RID: 2927 RVA: 0x0006DCDA File Offset: 0x0006BEDA
		public Vector2 SimPosition
		{
			get
			{
				return this.FarseerBody.Position;
			}
		}

		// Token: 0x17000319 RID: 793
		// (get) Token: 0x06000B70 RID: 2928 RVA: 0x0006DCE7 File Offset: 0x0006BEE7
		public Vector2 Position
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.FarseerBody.Position);
			}
		}

		// Token: 0x1700031A RID: 794
		// (get) Token: 0x06000B71 RID: 2929 RVA: 0x0006DCF9 File Offset: 0x0006BEF9
		public Vector2 DrawPositionOffset
		{
			get
			{
				return this.DrawPosition - this.Position;
			}
		}

		// Token: 0x1700031B RID: 795
		// (get) Token: 0x06000B72 RID: 2930 RVA: 0x0006DD0C File Offset: 0x0006BF0C
		public Vector2 PrevPosition
		{
			get
			{
				return this.prevPosition;
			}
		}

		// Token: 0x1700031C RID: 796
		// (get) Token: 0x06000B73 RID: 2931 RVA: 0x0006DD14 File Offset: 0x0006BF14
		public float Rotation
		{
			get
			{
				return this.FarseerBody.Rotation;
			}
		}

		// Token: 0x1700031D RID: 797
		// (get) Token: 0x06000B74 RID: 2932 RVA: 0x0006DD21 File Offset: 0x0006BF21
		public float TransformedRotation
		{
			get
			{
				return PhysicsBody.TransformRotation(this.Rotation, this.Dir);
			}
		}

		// Token: 0x06000B75 RID: 2933 RVA: 0x0006DD34 File Offset: 0x0006BF34
		public float TransformRotation(float rotation)
		{
			return PhysicsBody.TransformRotation(rotation, this.dir);
		}

		// Token: 0x06000B76 RID: 2934 RVA: 0x0006DD42 File Offset: 0x0006BF42
		public static float TransformRotation(float rot, float dir)
		{
			if (dir >= 0f)
			{
				return rot;
			}
			return rot - 3.1415927f;
		}

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x06000B77 RID: 2935 RVA: 0x0006DD55 File Offset: 0x0006BF55
		// (set) Token: 0x06000B78 RID: 2936 RVA: 0x0006DD62 File Offset: 0x0006BF62
		public Vector2 LinearVelocity
		{
			get
			{
				return this.FarseerBody.LinearVelocity;
			}
			set
			{
				if (!this.IsValidValue(value, "velocity", -1000f, 1000f))
				{
					return;
				}
				this.FarseerBody.LinearVelocity = value;
			}
		}

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x06000B79 RID: 2937 RVA: 0x0006DD89 File Offset: 0x0006BF89
		// (set) Token: 0x06000B7A RID: 2938 RVA: 0x0006DD96 File Offset: 0x0006BF96
		public float AngularVelocity
		{
			get
			{
				return this.FarseerBody.AngularVelocity;
			}
			set
			{
				if (!this.IsValidValue(value, "angular velocity", -1000f, 1000f))
				{
					return;
				}
				this.FarseerBody.AngularVelocity = value;
			}
		}

		// Token: 0x17000320 RID: 800
		// (get) Token: 0x06000B7B RID: 2939 RVA: 0x0006DDBD File Offset: 0x0006BFBD
		public float Mass
		{
			get
			{
				return this.FarseerBody.Mass;
			}
		}

		// Token: 0x17000321 RID: 801
		// (get) Token: 0x06000B7C RID: 2940 RVA: 0x0006DDCA File Offset: 0x0006BFCA
		public float Density
		{
			get
			{
				return this.density;
			}
		}

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x06000B7D RID: 2941 RVA: 0x0006DDD2 File Offset: 0x0006BFD2
		// (set) Token: 0x06000B7E RID: 2942 RVA: 0x0006DDDA File Offset: 0x0006BFDA
		public Body FarseerBody { get; private set; }

		// Token: 0x17000323 RID: 803
		// (get) Token: 0x06000B7F RID: 2943 RVA: 0x0006DDE3 File Offset: 0x0006BFE3
		// (set) Token: 0x06000B80 RID: 2944 RVA: 0x0006DDF0 File Offset: 0x0006BFF0
		public object UserData
		{
			get
			{
				return this.FarseerBody.UserData;
			}
			set
			{
				this.FarseerBody.UserData = value;
			}
		}

		// Token: 0x17000324 RID: 804
		// (set) Token: 0x06000B81 RID: 2945 RVA: 0x0006DDFE File Offset: 0x0006BFFE
		public float Friction
		{
			set
			{
				this.FarseerBody.Friction = value;
			}
		}

		// Token: 0x17000325 RID: 805
		// (get) Token: 0x06000B82 RID: 2946 RVA: 0x0006DE0C File Offset: 0x0006C00C
		// (set) Token: 0x06000B83 RID: 2947 RVA: 0x0006DE19 File Offset: 0x0006C019
		public BodyType BodyType
		{
			get
			{
				return this.FarseerBody.BodyType;
			}
			set
			{
				this.FarseerBody.BodyType = value;
			}
		}

		// Token: 0x17000326 RID: 806
		// (get) Token: 0x06000B85 RID: 2949 RVA: 0x0006DE3C File Offset: 0x0006C03C
		// (set) Token: 0x06000B84 RID: 2948 RVA: 0x0006DE27 File Offset: 0x0006C027
		public Category CollisionCategories
		{
			get
			{
				return this._collisionCategories;
			}
			set
			{
				this._collisionCategories = value;
				this.FarseerBody.CollisionCategories = value;
			}
		}

		// Token: 0x17000327 RID: 807
		// (get) Token: 0x06000B87 RID: 2951 RVA: 0x0006DE59 File Offset: 0x0006C059
		// (set) Token: 0x06000B86 RID: 2950 RVA: 0x0006DE44 File Offset: 0x0006C044
		public Category CollidesWith
		{
			get
			{
				return this._collidesWith;
			}
			set
			{
				this._collidesWith = value;
				this.FarseerBody.CollidesWith = value;
			}
		}

		// Token: 0x17000328 RID: 808
		// (get) Token: 0x06000B88 RID: 2952 RVA: 0x0006DE61 File Offset: 0x0006C061
		// (set) Token: 0x06000B89 RID: 2953 RVA: 0x0006DE69 File Offset: 0x0006C069
		public bool SuppressSmoothRotationCalls
		{
			get
			{
				return this._suppressSmoothRotationCalls;
			}
			set
			{
				this._suppressSmoothRotationCalls = value;
				this.smoothRotationSuppressionCounter = 0;
			}
		}

		// Token: 0x06000B8A RID: 2954 RVA: 0x0006DE7C File Offset: 0x0006C07C
		public PhysicsBody(XElement element, float scale = 1f, bool findNewContacts = true) : this(element, Vector2.Zero, scale, null, Category.Cat5, Category.Cat1 | Category.Cat3 | Category.Cat8, findNewContacts)
		{
		}

		// Token: 0x06000B8B RID: 2955 RVA: 0x0006DEA9 File Offset: 0x0006C0A9
		public PhysicsBody(RagdollParams.ColliderParams cParams, bool findNewContacts = true) : this(cParams, Vector2.Zero, findNewContacts)
		{
		}

		// Token: 0x06000B8C RID: 2956 RVA: 0x0006DEB8 File Offset: 0x0006C0B8
		public PhysicsBody(RagdollParams.LimbParams lParams, bool findNewContacts = true) : this(lParams, Vector2.Zero, findNewContacts)
		{
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x0006DEC8 File Offset: 0x0006C0C8
		public PhysicsBody(float width, float height, float radius, float density, BodyType bodyType, Category collisionCategory, Category collidesWith, bool findNewContacts = true)
		{
			this.dir = 1f;
			this.isEnabled = true;
			this.isPhysEnabled = true;
			base..ctor();
			density = Math.Max(density, 0.01f);
			this.CreateBody(width, height, radius, density, bodyType, collisionCategory, collidesWith, findNewContacts);
			this.LastSentPosition = this.FarseerBody.Position;
			PhysicsBody.list.Add(this);
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x0006DF34 File Offset: 0x0006C134
		public PhysicsBody(Body farseerBody)
		{
			this.dir = 1f;
			this.isEnabled = true;
			this.isPhysEnabled = true;
			base..ctor();
			this.FarseerBody = farseerBody;
			if (this.FarseerBody.UserData == null)
			{
				this.FarseerBody.UserData = this;
			}
			this.LastSentPosition = this.FarseerBody.Position;
			PhysicsBody.list.Add(this);
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x0006DF9C File Offset: 0x0006C19C
		public PhysicsBody(RagdollParams.ColliderParams colliderParams, Vector2 position, bool findNewContacts = true)
		{
			this.dir = 1f;
			this.isEnabled = true;
			this.isPhysEnabled = true;
			base..ctor();
			float radius = ConvertUnits.ToSimUnits(colliderParams.Radius) * colliderParams.Ragdoll.LimbScale;
			float height = ConvertUnits.ToSimUnits(colliderParams.Height) * colliderParams.Ragdoll.LimbScale;
			float width = ConvertUnits.ToSimUnits(colliderParams.Width) * colliderParams.Ragdoll.LimbScale;
			this.density = 10f;
			this.CreateBody(width, height, radius, this.density, colliderParams.BodyType, Category.Cat2, Category.Cat1 | Category.Cat8, findNewContacts);
			this.FarseerBody.AngularDamping = 5f;
			this.FarseerBody.FixedRotation = true;
			this.FarseerBody.Friction = 0.05f;
			this.FarseerBody.Restitution = 0.05f;
			this.SetTransformIgnoreContacts(position, 0f, true);
			this.LastSentPosition = position;
			PhysicsBody.list.Add(this);
		}

		// Token: 0x06000B90 RID: 2960 RVA: 0x0006E094 File Offset: 0x0006C294
		public PhysicsBody(RagdollParams.LimbParams limbParams, Vector2 position, bool findNewContacts = true)
		{
			this.dir = 1f;
			this.isEnabled = true;
			this.isPhysEnabled = true;
			base..ctor();
			float radius = ConvertUnits.ToSimUnits(limbParams.Radius) * limbParams.Scale * limbParams.Ragdoll.LimbScale;
			float height = ConvertUnits.ToSimUnits(limbParams.Height) * limbParams.Scale * limbParams.Ragdoll.LimbScale;
			float width = ConvertUnits.ToSimUnits(limbParams.Width) * limbParams.Scale * limbParams.Ragdoll.LimbScale;
			this.density = Math.Max(limbParams.Density, 0.01f);
			Category collisionCategory = Category.Cat2;
			Category collidesWith = Category.Cat1 | Category.Cat3 | Category.Cat4 | Category.Cat7 | Category.Cat8 | Category.Cat9 | Category.Cat10 | Category.Cat11 | Category.Cat12 | Category.Cat13 | Category.Cat14 | Category.Cat15 | Category.Cat16 | Category.Cat17 | Category.Cat18 | Category.Cat19 | Category.Cat20 | Category.Cat21 | Category.Cat22 | Category.Cat23 | Category.Cat24 | Category.Cat25 | Category.Cat26 | Category.Cat27 | Category.Cat28 | Category.Cat29 | Category.Cat30 | Category.Cat31;
			if (limbParams.IgnoreCollisions)
			{
				collisionCategory = Category.None;
				collidesWith = Category.None;
			}
			this.CreateBody(width, height, radius, this.density, BodyType.Dynamic, collisionCategory, collidesWith, findNewContacts);
			this.FarseerBody.Friction = limbParams.Friction;
			this.FarseerBody.Restitution = limbParams.Restitution;
			this.FarseerBody.AngularDamping = limbParams.AngularDamping;
			this.FarseerBody.UserData = this;
			this._collisionCategories = collisionCategory;
			this._collidesWith = collidesWith;
			this.SetTransformIgnoreContacts(position, 0f, true);
			this.LastSentPosition = position;
			PhysicsBody.list.Add(this);
		}

		// Token: 0x06000B91 RID: 2961 RVA: 0x0006E1CC File Offset: 0x0006C3CC
		public PhysicsBody(XElement element, Vector2 position, float scale = 1f, float? forceDensity = null, Category collisionCategory = Category.Cat5, Category collidesWith = Category.Cat1 | Category.Cat3 | Category.Cat8, bool findNewContacts = true)
		{
			this.dir = 1f;
			this.isEnabled = true;
			this.isPhysEnabled = true;
			base..ctor();
			float radius = ConvertUnits.ToSimUnits(element.GetAttributeFloat("radius", 0f)) * scale;
			float height = ConvertUnits.ToSimUnits(element.GetAttributeFloat("height", 0f)) * scale;
			float width = ConvertUnits.ToSimUnits(element.GetAttributeFloat("width", 0f)) * scale;
			this.density = Math.Max(forceDensity ?? element.GetAttributeFloat("density", 10f), 0.01f);
			BodyType bodyType;
			Enum.TryParse<BodyType>(element.GetAttributeString("bodytype", "Dynamic"), out bodyType);
			if (element.GetAttributeBool("ignorecollision", false))
			{
				this._collisionCategories = Category.None;
				this._collidesWith = Category.None;
			}
			else
			{
				this._collisionCategories = collisionCategory;
				this._collidesWith = collidesWith;
			}
			this.CreateBody(width, height, radius, this.density, bodyType, this._collisionCategories, this._collidesWith, findNewContacts);
			this.FarseerBody.Friction = element.GetAttributeFloat("friction", 0.5f);
			this.FarseerBody.Restitution = element.GetAttributeFloat("restitution", 0.05f);
			this.FarseerBody.GravityScale = element.GetAttributeFloat("gravityscale", 1f);
			this.FarseerBody.UserData = this;
			this.SetTransformIgnoreContacts(position, 0f, true);
			this.LastSentPosition = position;
			PhysicsBody.list.Add(this);
		}

		// Token: 0x06000B92 RID: 2962 RVA: 0x0006E358 File Offset: 0x0006C558
		private void CreateBody(float width, float height, float radius, float density, BodyType bodyType, Category collisionCategory, Category collidesWith, bool findNewContacts = true)
		{
			if (PhysicsBody.IsValidShape(radius, height, width))
			{
				this.bodyShape = PhysicsBody.DefineBodyShape(radius, width, height);
				switch (this.bodyShape)
				{
				case PhysicsBody.Shape.Circle:
					this.FarseerBody = GameMain.World.CreateCircle(radius, density, default(Vector2), bodyType, collisionCategory, collidesWith, findNewContacts);
					break;
				case PhysicsBody.Shape.Rectangle:
					this.FarseerBody = GameMain.World.CreateRectangle(width, height, density, default(Vector2), 0f, bodyType, collisionCategory, collidesWith, findNewContacts);
					break;
				case PhysicsBody.Shape.Capsule:
					this.FarseerBody = GameMain.World.CreateCapsule(height, radius, density, default(Vector2), 0f, bodyType, collisionCategory, collidesWith, findNewContacts);
					break;
				case PhysicsBody.Shape.HorizontalCapsule:
					this.FarseerBody = GameMain.World.CreateCapsuleHorizontal(width, radius, density, default(Vector2), 0f, bodyType, collisionCategory, collidesWith, findNewContacts);
					break;
				default:
					throw new NotImplementedException(this.bodyShape.ToString());
				}
			}
			else
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Invalid physics body dimensions (width: ",
					width.ToString(),
					", height: ",
					height.ToString(),
					", radius: ",
					radius.ToString(),
					")"
				}), null, null, false, false);
			}
			this.Width = width;
			this.Height = height;
			this.Radius = radius;
			this._collisionCategories = collisionCategory;
			this._collidesWith = collidesWith;
		}

		// Token: 0x06000B93 RID: 2963 RVA: 0x0006E510 File Offset: 0x0006C710
		public Vector2 GetLocalFront(float? spritesheetRotation = null)
		{
			Vector2 pos;
			switch (this.bodyShape)
			{
			case PhysicsBody.Shape.Circle:
				pos = new Vector2(0f, this.Radius);
				break;
			case PhysicsBody.Shape.Rectangle:
				pos = ((this.Height > this.Width) ? new Vector2(0f, this.Height / 2f) : new Vector2(this.Width / 2f, 0f));
				break;
			case PhysicsBody.Shape.Capsule:
				pos = new Vector2(0f, this.Height / 2f + this.Radius);
				break;
			case PhysicsBody.Shape.HorizontalCapsule:
				pos = new Vector2(this.Width / 2f + this.Radius, 0f);
				break;
			default:
				throw new NotImplementedException();
			}
			if (spritesheetRotation == null)
			{
				return pos;
			}
			return PhysicsBody.RotateVector(pos, spritesheetRotation.Value);
		}

		// Token: 0x06000B94 RID: 2964 RVA: 0x0006E5F2 File Offset: 0x0006C7F2
		public static Vector2 RotateVector(Vector2 v, float rotation)
		{
			return Vector2.Transform(v, Matrix.CreateRotationZ(-rotation));
		}

		// Token: 0x06000B95 RID: 2965 RVA: 0x0006E604 File Offset: 0x0006C804
		public float GetMaxExtent()
		{
			switch (this.bodyShape)
			{
			case PhysicsBody.Shape.Circle:
				return this.Radius;
			case PhysicsBody.Shape.Rectangle:
				return new Vector2(this.Width * 0.5f, this.Height * 0.5f).Length();
			case PhysicsBody.Shape.Capsule:
				return this.Height / 2f + this.Radius;
			case PhysicsBody.Shape.HorizontalCapsule:
				return this.Width / 2f + this.Radius;
			default:
				throw new NotImplementedException();
			}
		}

		// Token: 0x06000B96 RID: 2966 RVA: 0x0006E68C File Offset: 0x0006C88C
		public Vector2 GetSize()
		{
			switch (this.bodyShape)
			{
			case PhysicsBody.Shape.Circle:
				return new Vector2(this.Radius * 2f);
			case PhysicsBody.Shape.Rectangle:
				return new Vector2(this.Width, this.Height);
			case PhysicsBody.Shape.Capsule:
				return new Vector2(this.Radius * 2f, this.Height + this.Radius * 2f);
			case PhysicsBody.Shape.HorizontalCapsule:
				return new Vector2(this.Width + this.Radius * 2f, this.Radius * 2f);
			default:
				throw new NotImplementedException();
			}
		}

		// Token: 0x06000B97 RID: 2967 RVA: 0x0006E72C File Offset: 0x0006C92C
		public void SetSize(Vector2 size)
		{
			switch (this.bodyShape)
			{
			case PhysicsBody.Shape.Circle:
				this.Radius = Math.Max(Math.Min(size.X, size.Y) / 2f, 0f);
				this.Width = 0f;
				this.Height = 0f;
				return;
			case PhysicsBody.Shape.Rectangle:
				this.Width = Math.Max(size.X, 0f);
				this.Height = Math.Max(size.Y, 0f);
				this.Radius = 0f;
				return;
			case PhysicsBody.Shape.Capsule:
				this.Radius = Math.Max(size.X / 2f, 0f);
				this.Height = Math.Max(size.Y - size.X, 0f);
				this.Width = 0f;
				return;
			case PhysicsBody.Shape.HorizontalCapsule:
				this.Radius = Math.Max(size.Y / 2f, 0f);
				this.Width = Math.Max(size.X - size.Y, 0f);
				this.Height = 0f;
				return;
			default:
				throw new NotImplementedException();
			}
		}

		// Token: 0x06000B98 RID: 2968 RVA: 0x0006E860 File Offset: 0x0006CA60
		public bool IsValidValue(float value, string valueName, float minValue = -3.4028235E+38f, float maxValue = 3.4028235E+38f)
		{
			if (!MathUtils.IsValid(value) || value < minValue || value > maxValue)
			{
				string userData = (this.UserData == null) ? "null" : this.UserData.ToString();
				string errorMsg = string.Concat(new string[]
				{
					"Attempted to apply invalid ",
					valueName,
					" to a physics body (userdata: ",
					userData,
					"), value: ",
					value.ToString()
				});
				if (GameMain.NetworkMember != null)
				{
					errorMsg += (GameMain.NetworkMember.IsClient ? " Playing as a client." : " Hosting a server.");
				}
				errorMsg = errorMsg + "\n" + Environment.StackTrace.CleanupStackTrace();
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
				GameAnalyticsManager.AddErrorEventOnce("PhysicsBody.SetPosition:InvalidPosition" + userData, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return false;
			}
			return true;
		}

		// Token: 0x06000B99 RID: 2969 RVA: 0x0006E93C File Offset: 0x0006CB3C
		private bool IsValidValue(Vector2 value, string valueName, float minValue = -3.4028235E+38f, float maxValue = 3.4028235E+38f)
		{
			if (!MathUtils.IsValid(value) || value.X < minValue || value.Y < minValue || value.X > maxValue || value.Y > maxValue)
			{
				string userData = (this.UserData == null) ? "null" : this.UserData.ToString();
				string[] array = new string[6];
				array[0] = "Attempted to apply invalid ";
				array[1] = valueName;
				array[2] = " to a physics body (userdata: ";
				array[3] = userData;
				array[4] = "), value: ";
				int num = 5;
				Vector2 vector = value;
				array[num] = vector.ToString();
				string errorMsg = string.Concat(array);
				if (GameMain.NetworkMember != null)
				{
					errorMsg += (GameMain.NetworkMember.IsClient ? " Playing as a client." : " Hosting a server.");
				}
				errorMsg = errorMsg + "\n" + Environment.StackTrace.CleanupStackTrace();
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
				GameAnalyticsManager.AddErrorEventOnce("PhysicsBody.SetPosition:InvalidPosition" + userData, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return false;
			}
			return true;
		}

		// Token: 0x06000B9A RID: 2970 RVA: 0x0006EA3A File Offset: 0x0006CC3A
		public void ResetDynamics()
		{
			this.FarseerBody.ResetDynamics();
		}

		// Token: 0x06000B9B RID: 2971 RVA: 0x0006EA48 File Offset: 0x0006CC48
		public void ApplyLinearImpulse(Vector2 impulse)
		{
			if (!this.IsValidValue(impulse / this.FarseerBody.Mass, "new velocity", -1000f, 1000f))
			{
				return;
			}
			if (!this.IsValidValue(impulse, "impulse", -1E+10f, 1E+10f))
			{
				return;
			}
			this.FarseerBody.ApplyLinearImpulse(impulse);
		}

		// Token: 0x06000B9C RID: 2972 RVA: 0x0006EAA4 File Offset: 0x0006CCA4
		public void ApplyLinearImpulse(Vector2 impulse, float maxVelocity)
		{
			if (!this.IsValidValue(impulse, "impulse", -1E+10f, 1E+10f))
			{
				return;
			}
			if (!this.IsValidValue(maxVelocity, "max velocity", -3.4028235E+38f, 3.4028235E+38f))
			{
				return;
			}
			Vector2 velocityAddition = impulse / this.Mass;
			Vector2 newVelocity = this.FarseerBody.LinearVelocity + velocityAddition;
			float newSpeedSqr = newVelocity.LengthSquared();
			if (newSpeedSqr > maxVelocity * maxVelocity)
			{
				newVelocity = newVelocity.ClampLength(maxVelocity);
			}
			if (!this.IsValidValue(newVelocity - this.FarseerBody.LinearVelocity, "new velocity", -1000f, 1000f))
			{
				return;
			}
			this.FarseerBody.ApplyLinearImpulse((newVelocity - this.FarseerBody.LinearVelocity) * this.Mass);
		}

		// Token: 0x06000B9D RID: 2973 RVA: 0x0006EB68 File Offset: 0x0006CD68
		public void ApplyLinearImpulse(Vector2 impulse, Vector2 point)
		{
			if (!this.IsValidValue(impulse, "impulse", -1E+10f, 1E+10f))
			{
				return;
			}
			if (!this.IsValidValue(point, "point", -3.4028235E+38f, 3.4028235E+38f))
			{
				return;
			}
			if (!this.IsValidValue(impulse / this.FarseerBody.Mass, "new velocity", -1000f, 1000f))
			{
				return;
			}
			this.FarseerBody.ApplyLinearImpulse(impulse, point);
		}

		// Token: 0x06000B9E RID: 2974 RVA: 0x0006EBE0 File Offset: 0x0006CDE0
		public void ApplyLinearImpulse(Vector2 impulse, Vector2 point, float maxVelocity)
		{
			if (!this.IsValidValue(impulse, "impulse", -1E+10f, 1E+10f))
			{
				return;
			}
			if (!this.IsValidValue(point, "point", -3.4028235E+38f, 3.4028235E+38f))
			{
				return;
			}
			if (!this.IsValidValue(maxVelocity, "max velocity", -3.4028235E+38f, 3.4028235E+38f))
			{
				return;
			}
			Vector2 velocityAddition = impulse / this.Mass;
			Vector2 newVelocity = this.FarseerBody.LinearVelocity + velocityAddition;
			float newSpeedSqr = newVelocity.LengthSquared();
			if (newSpeedSqr > maxVelocity * maxVelocity)
			{
				newVelocity = newVelocity.ClampLength(maxVelocity);
			}
			if (!this.IsValidValue(newVelocity - this.FarseerBody.LinearVelocity, "new velocity", -1000f, 1000f))
			{
				return;
			}
			this.FarseerBody.ApplyLinearImpulse((newVelocity - this.FarseerBody.LinearVelocity) * this.Mass, point);
			this.FarseerBody.AngularVelocity = MathHelper.Clamp(this.FarseerBody.AngularVelocity, -16f, 16f);
		}

		// Token: 0x06000B9F RID: 2975 RVA: 0x0006ECE4 File Offset: 0x0006CEE4
		public void ApplyForce(Vector2 force, float maxVelocity = 64f)
		{
			if (!this.IsValidValue(maxVelocity, "max velocity", -3.4028235E+38f, 3.4028235E+38f))
			{
				return;
			}
			if (force.LengthSquared() < 0.01f)
			{
				return;
			}
			Vector2 velocityAddition = force / this.Mass * 0.016666668f;
			Vector2 newVelocity = this.FarseerBody.LinearVelocity + velocityAddition;
			float newSpeedSqr = newVelocity.LengthSquared();
			if (newSpeedSqr > maxVelocity * maxVelocity)
			{
				float currSpeed = this.FarseerBody.LinearVelocity.Length();
				if (Vector2.Dot(this.FarseerBody.LinearVelocity, force) > 0f)
				{
					force = velocityAddition.ClampLength(Math.Max(maxVelocity - currSpeed, 0f)) * this.Mass / 0.016666668f;
				}
				else if (Vector2.Dot(newVelocity, force) > 0f)
				{
					force = velocityAddition.ClampLength(maxVelocity + currSpeed) * this.Mass / 0.016666668f;
				}
			}
			if (!this.IsValidValue(force, "clamped force", -1E+10f, 1E+10f))
			{
				return;
			}
			this.FarseerBody.ApplyForce(force);
		}

		// Token: 0x06000BA0 RID: 2976 RVA: 0x0006EE00 File Offset: 0x0006D000
		public void ApplyForce(Vector2 force, Vector2 point)
		{
			if (!this.IsValidValue(force, "force", -1E+10f, 1E+10f))
			{
				return;
			}
			if (!this.IsValidValue(point, "point", -3.4028235E+38f, 3.4028235E+38f))
			{
				return;
			}
			this.FarseerBody.ApplyForce(force, point);
		}

		// Token: 0x06000BA1 RID: 2977 RVA: 0x0006EE4C File Offset: 0x0006D04C
		public void ApplyTorque(float torque)
		{
			if (!this.IsValidValue(torque, "torque", -3.4028235E+38f, 3.4028235E+38f))
			{
				return;
			}
			this.FarseerBody.ApplyTorque(torque);
		}

		// Token: 0x06000BA2 RID: 2978 RVA: 0x0006EE74 File Offset: 0x0006D074
		public bool SetTransform(Vector2 simPosition, float rotation, bool setPrevTransform = true)
		{
			if (!this.IsValidValue(simPosition, "position", -1E+10f, 1E+10f))
			{
				return false;
			}
			if (!this.IsValidValue(rotation, "rotation", -3.4028235E+38f, 3.4028235E+38f))
			{
				return false;
			}
			this.FarseerBody.SetTransform(simPosition, rotation);
			if (setPrevTransform)
			{
				this.SetPrevTransform(simPosition, rotation);
			}
			return true;
		}

		// Token: 0x06000BA3 RID: 2979 RVA: 0x0006EED0 File Offset: 0x0006D0D0
		public bool SetTransformIgnoreContacts(Vector2 simPosition, float rotation, bool setPrevTransform = true)
		{
			if (!this.IsValidValue(simPosition, "position", -1E+10f, 1E+10f))
			{
				return false;
			}
			if (!this.IsValidValue(rotation, "rotation", -3.4028235E+38f, 3.4028235E+38f))
			{
				return false;
			}
			this.FarseerBody.SetTransformIgnoreContacts(ref simPosition, rotation);
			if (setPrevTransform)
			{
				this.SetPrevTransform(simPosition, rotation);
			}
			return true;
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x0006EF2B File Offset: 0x0006D12B
		public void SetPrevTransform(Vector2 simPosition, float rotation)
		{
			this.prevPosition = simPosition;
			this.prevRotation = rotation;
		}

		// Token: 0x06000BA5 RID: 2981 RVA: 0x0006EF3C File Offset: 0x0006D13C
		public void MoveToTargetPosition(bool lerp = true)
		{
			if (this.targetPosition == null)
			{
				return;
			}
			if (lerp)
			{
				if (Vector2.DistanceSquared(this.targetPosition.Value, this.FarseerBody.Position) < 100f)
				{
					this.drawOffset = -(this.targetPosition.Value - (this.FarseerBody.Position + this.drawOffset));
					this.prevPosition = this.targetPosition.Value;
				}
				else
				{
					this.drawOffset = Vector2.Zero;
				}
				if (this.targetRotation != null)
				{
					this.rotationOffset = -MathUtils.GetShortestAngle(this.FarseerBody.Rotation + this.rotationOffset, this.targetRotation.Value);
				}
			}
			this.SetTransformIgnoreContacts(this.targetPosition.Value, (this.targetRotation == null) ? this.FarseerBody.Rotation : this.targetRotation.Value, true);
			this.targetPosition = null;
			this.targetRotation = null;
		}

		// Token: 0x06000BA6 RID: 2982 RVA: 0x0006F058 File Offset: 0x0006D258
		public void MoveToPos(Vector2 simPosition, float force, Vector2? pullPos = null)
		{
			if (pullPos == null)
			{
				pullPos = new Vector2?(this.FarseerBody.Position);
			}
			if (!this.IsValidValue(simPosition, "position", -1E+10f, 1E+10f))
			{
				return;
			}
			if (!this.IsValidValue(force, "force", -3.4028235E+38f, 3.4028235E+38f))
			{
				return;
			}
			Vector2 vel = this.FarseerBody.LinearVelocity;
			Vector2 deltaPos = simPosition - pullPos.Value;
			if (deltaPos.LengthSquared() > 10000f)
			{
				return;
			}
			deltaPos *= force;
			this.ApplyLinearImpulse((deltaPos - vel * 0.5f) * this.FarseerBody.Mass, pullPos.Value);
		}

		// Token: 0x06000BA7 RID: 2983 RVA: 0x0006F114 File Offset: 0x0006D314
		public void ApplyWaterForces()
		{
			Vector2 buoyancy = new Vector2(0f, this.Mass * 9.6f);
			Vector2 dragForce = Vector2.Zero;
			float speedSqr = this.LinearVelocity.LengthSquared();
			if (speedSqr > 1E-05f)
			{
				float speed = (float)Math.Sqrt((double)speedSqr);
				Vector2 velDir = this.LinearVelocity / speed;
				float vel = speed * 2f;
				float drag = vel * vel * Math.Max(this.Height + this.Radius * 2f, this.Height);
				dragForce = Math.Min(drag, this.Mass * 500f) * -velDir;
			}
			this.ApplyForce(dragForce + buoyancy, 64f);
			this.ApplyTorque(this.FarseerBody.AngularVelocity * this.FarseerBody.Mass * -0.08f);
		}

		// Token: 0x06000BA8 RID: 2984 RVA: 0x0006F1F4 File Offset: 0x0006D3F4
		public void Update()
		{
			if (this.drawOffset.LengthSquared() < 0.01f)
			{
				this.PositionSmoothingFactor = null;
			}
			this.drawOffset = NetConfig.InterpolateSimPositionError(this.drawOffset, this.PositionSmoothingFactor);
			this.rotationOffset = NetConfig.InterpolateRotationError(this.rotationOffset);
			if (this.SuppressSmoothRotationCalls)
			{
				if (this.smoothRotationSuppressionCounter > 0)
				{
					this.SuppressSmoothRotationCalls = false;
					return;
				}
				this.smoothRotationSuppressionCounter++;
			}
		}

		// Token: 0x06000BA9 RID: 2985 RVA: 0x0006F270 File Offset: 0x0006D470
		public void UpdateDrawPosition(bool interpolate = true)
		{
			if (interpolate)
			{
				this.drawPosition = Timing.Interpolate(this.prevPosition, this.FarseerBody.Position);
				this.drawPosition = ConvertUnits.ToDisplayUnits(this.drawPosition + this.drawOffset);
				this.drawRotation = Timing.InterpolateRotation(this.prevRotation, this.FarseerBody.Rotation) + this.rotationOffset;
				return;
			}
			this.prevPosition = this.FarseerBody.Position;
			this.drawPosition = ConvertUnits.ToDisplayUnits(this.FarseerBody.Position);
			this.drawRotation = (this.prevRotation = this.FarseerBody.Rotation);
			this.drawOffset = Vector2.Zero;
			this.rotationOffset = 0f;
		}

		// Token: 0x06000BAA RID: 2986 RVA: 0x0006F334 File Offset: 0x0006D534
		public void CorrectPosition<T>(List<T> positionBuffer, out Vector2 newPosition, out Vector2 newVelocity, out float newRotation, out float newAngularVelocity) where T : PosInfo
		{
			newVelocity = this.LinearVelocity;
			newPosition = this.SimPosition;
			newRotation = this.Rotation;
			newAngularVelocity = this.AngularVelocity;
			while (positionBuffer.Count > 0 && positionBuffer[0].Timestamp < this.lastProcessedNetworkState)
			{
				positionBuffer.RemoveAt(0);
			}
			if (positionBuffer.Count == 0)
			{
				return;
			}
			this.lastProcessedNetworkState = positionBuffer[0].Timestamp;
			newVelocity = positionBuffer[0].LinearVelocity;
			newPosition = positionBuffer[0].Position;
			newRotation = (positionBuffer[0].Rotation ?? this.Rotation);
			newAngularVelocity = (positionBuffer[0].AngularVelocity ?? this.AngularVelocity);
			positionBuffer.RemoveAt(0);
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x0006F448 File Offset: 0x0006D648
		public void SmoothRotate(float targetRotation, float force = 10f, bool wrapAngle = true)
		{
			if (this.SuppressSmoothRotationCalls)
			{
				return;
			}
			float nextAngle = this.FarseerBody.Rotation + this.FarseerBody.AngularVelocity * 0.016666668f;
			float angle = wrapAngle ? MathUtils.GetShortestAngle(nextAngle, targetRotation) : MathHelper.Clamp(targetRotation - nextAngle, -3.1415927f, 3.1415927f);
			float torque = angle * 60f * (force / 100f);
			if (this.FarseerBody.BodyType != BodyType.Kinematic)
			{
				this.ApplyTorque(this.FarseerBody.Mass * torque);
				return;
			}
			if (!this.IsValidValue(torque, "torque", -3.4028235E+38f, 3.4028235E+38f))
			{
				return;
			}
			this.FarseerBody.AngularVelocity = torque;
		}

		// Token: 0x06000BAC RID: 2988 RVA: 0x0006F4F2 File Offset: 0x0006D6F2
		public float WrapAngleToSameNumberOfRevolutions(float angle)
		{
			if (float.IsInfinity(angle))
			{
				return angle;
			}
			while (this.Rotation - angle > 6.2831855f)
			{
				angle += 6.2831855f;
			}
			while (this.Rotation - angle < -6.2831855f)
			{
				angle -= 6.2831855f;
			}
			return angle;
		}

		// Token: 0x06000BAD RID: 2989 RVA: 0x0006F531 File Offset: 0x0006D731
		public void Remove()
		{
			PhysicsBody.list.Remove(this);
			GameMain.World.Remove(this.FarseerBody);
			this.Removed = true;
		}

		// Token: 0x06000BAE RID: 2990 RVA: 0x0006F558 File Offset: 0x0006D758
		public static void RemoveAll()
		{
			for (int i = PhysicsBody.list.Count - 1; i >= 0; i--)
			{
				PhysicsBody.list[i].Remove();
			}
		}

		// Token: 0x06000BAF RID: 2991 RVA: 0x0006F58C File Offset: 0x0006D78C
		public static bool IsValidShape(float radius, float height, float width)
		{
			return radius > 0f || (height > 0f && width > 0f);
		}

		// Token: 0x06000BB0 RID: 2992 RVA: 0x0006F5AC File Offset: 0x0006D7AC
		public static PhysicsBody.Shape DefineBodyShape(float radius, float width, float height)
		{
			PhysicsBody.Shape bodyShape;
			if (width <= 0f && height <= 0f && radius > 0f)
			{
				bodyShape = PhysicsBody.Shape.Circle;
			}
			else if (radius > 0f)
			{
				if (width > height)
				{
					bodyShape = PhysicsBody.Shape.HorizontalCapsule;
				}
				else
				{
					bodyShape = PhysicsBody.Shape.Capsule;
				}
			}
			else
			{
				bodyShape = PhysicsBody.Shape.Rectangle;
			}
			return bodyShape;
		}

		// Token: 0x040004E9 RID: 1257
		public const float MinDensity = 0.01f;

		// Token: 0x040004EA RID: 1258
		public const float DefaultAngularDamping = 5f;

		// Token: 0x040004EB RID: 1259
		private static readonly List<PhysicsBody> list = new List<PhysicsBody>();

		// Token: 0x040004EC RID: 1260
		protected Vector2 prevPosition;

		// Token: 0x040004ED RID: 1261
		protected float prevRotation;

		// Token: 0x040004EE RID: 1262
		protected Vector2? targetPosition;

		// Token: 0x040004EF RID: 1263
		protected float? targetRotation;

		// Token: 0x040004F0 RID: 1264
		private Vector2 drawPosition;

		// Token: 0x040004F1 RID: 1265
		private float drawRotation;

		// Token: 0x040004F4 RID: 1268
		private PhysicsBody.Shape bodyShape;

		// Token: 0x040004F8 RID: 1272
		private readonly float density;

		// Token: 0x040004F9 RID: 1273
		private float dir;

		// Token: 0x040004FA RID: 1274
		private Vector2 drawOffset;

		// Token: 0x040004FB RID: 1275
		private float rotationOffset;

		// Token: 0x040004FC RID: 1276
		private float lastProcessedNetworkState;

		// Token: 0x040004FD RID: 1277
		public float? PositionSmoothingFactor;

		// Token: 0x040004FE RID: 1278
		public Submarine Submarine;

		// Token: 0x040004FF RID: 1279
		private bool isEnabled;

		// Token: 0x04000500 RID: 1280
		private bool isPhysEnabled;

		// Token: 0x04000502 RID: 1282
		private Category _collisionCategories;

		// Token: 0x04000503 RID: 1283
		private Category _collidesWith;

		// Token: 0x04000504 RID: 1284
		private bool _suppressSmoothRotationCalls;

		// Token: 0x04000505 RID: 1285
		private int smoothRotationSuppressionCounter;

		// Token: 0x02000749 RID: 1865
		public enum Shape
		{
			// Token: 0x04002C8B RID: 11403
			Circle,
			// Token: 0x04002C8C RID: 11404
			Rectangle,
			// Token: 0x04002C8D RID: 11405
			Capsule,
			// Token: 0x04002C8E RID: 11406
			HorizontalCapsule
		}
	}
}
