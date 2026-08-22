using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000F9 RID: 249
	internal class PhysicsBody
	{
		// Token: 0x1700099C RID: 2460
		// (get) Token: 0x06002355 RID: 9045 RVA: 0x00164F66 File Offset: 0x00163166
		public Vector2 NetworkPositionErrorOffset
		{
			get
			{
				return this.drawOffset;
			}
		}

		// Token: 0x06002356 RID: 9046 RVA: 0x00164F70 File Offset: 0x00163170
		public void Draw(DeformableSprite deformSprite, Camera cam, Vector2 scale, Color color, bool invert = false)
		{
			if (!this.Enabled)
			{
				return;
			}
			this.UpdateDrawPosition(true);
			if (deformSprite != null)
			{
				deformSprite.Draw(cam, new Vector3(this.DrawPosition, MathHelper.Clamp(deformSprite.Sprite.Depth, 0f, 1f)), deformSprite.Origin, -this.DrawRotation, scale, color, this.Dir < 0f, invert);
			}
		}

		// Token: 0x06002357 RID: 9047 RVA: 0x00164FDC File Offset: 0x001631DC
		public void Draw(SpriteBatch spriteBatch, Sprite sprite, Color color, float? depth = null, float scale = 1f, bool mirrorX = false, bool mirrorY = false, Vector2? origin = null)
		{
			if (!this.Enabled)
			{
				return;
			}
			this.UpdateDrawPosition(true);
			if (sprite == null)
			{
				return;
			}
			SpriteEffects spriteEffect = (this.Dir == 1f) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			if (mirrorX)
			{
				spriteEffect = ((spriteEffect == SpriteEffects.None) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
			}
			if (mirrorY)
			{
				spriteEffect |= SpriteEffects.FlipVertically;
			}
			sprite.Draw(spriteBatch, new Vector2(this.DrawPosition.X, -this.DrawPosition.Y), color, origin ?? sprite.Origin, -this.drawRotation, scale, spriteEffect, depth);
		}

		// Token: 0x06002358 RID: 9048 RVA: 0x00165070 File Offset: 0x00163270
		public void DebugDraw(SpriteBatch spriteBatch, Color color, bool forceColor = false)
		{
			PhysicsBody.<>c__DisplayClass5_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			if (!forceColor)
			{
				if (!this.FarseerBody.Enabled)
				{
					color = Color.Black;
				}
				else if (!this.FarseerBody.Awake)
				{
					color = Color.Blue;
				}
			}
			if (this.targetPosition != null)
			{
				Vector2 pos = ConvertUnits.ToDisplayUnits(this.targetPosition.Value);
				if (this.Submarine != null)
				{
					pos += this.Submarine.DrawPosition;
				}
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Vector2(pos.X - 5f, -(pos.Y + 5f)), Vector2.One * 10f, GUIStyle.Red, false, 0f, 3f);
			}
			if (this.drawOffset != Vector2.Zero)
			{
				Vector2 pos2 = ConvertUnits.ToDisplayUnits(this.FarseerBody.Position);
				if (this.Submarine != null)
				{
					pos2 += this.Submarine.DrawPosition;
				}
				GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(pos2.X, -pos2.Y), new Vector2(this.DrawPosition.X, -this.DrawPosition.Y), Color.Purple * 0.5f, 0f, 5f);
			}
			if (PhysicsBody.IsValidShape(this.Radius, this.Height, this.Width))
			{
				this.<DebugDraw>g__DrawShape|5_0(this.DrawPosition, this.DrawRotation, color, ref CS$<>8__locals1);
			}
			if (this.LastServerState != null)
			{
				Vector2 drawPos = ConvertUnits.ToDisplayUnits(this.LastServerState.Position);
				if (this.Submarine != null)
				{
					drawPos += this.Submarine.DrawPosition;
				}
				float rotation = this.LastServerState.Rotation.GetValueOrDefault();
				this.<DebugDraw>g__DrawShape|5_0(drawPos, rotation, Color.Purple * 0.75f, ref CS$<>8__locals1);
			}
		}

		// Token: 0x06002359 RID: 9049 RVA: 0x00165260 File Offset: 0x00163460
		public PosInfo ClientRead(IReadMessage msg, float sendingTime, string parentDebugName)
		{
			float MaxVel = 64f;
			float MaxAngularVel = 16f;
			Vector2 newPosition = this.SimPosition;
			float? newRotation = null;
			bool awake = this.FarseerBody.Awake;
			Vector2 newVelocity = this.LinearVelocity;
			float? newAngularVelocity = null;
			newPosition = new Vector2(msg.ReadSingle(), msg.ReadSingle());
			awake = msg.ReadBoolean();
			bool fixedRotation = msg.ReadBoolean();
			if (!fixedRotation)
			{
				newRotation = new float?(msg.ReadRangedSingle(0f, 6.2831855f, 8));
			}
			if (awake)
			{
				newVelocity = new Vector2(msg.ReadRangedSingle(-MaxVel, MaxVel, 12), msg.ReadRangedSingle(-MaxVel, MaxVel, 12));
				newVelocity = NetConfig.Quantize(newVelocity, -MaxVel, MaxVel, 12);
				if (!fixedRotation)
				{
					newAngularVelocity = new float?(msg.ReadRangedSingle(-MaxAngularVel, MaxAngularVel, 8));
					newAngularVelocity = new float?(NetConfig.Quantize(newAngularVelocity.Value, -MaxAngularVel, MaxAngularVel, 8));
				}
			}
			msg.ReadPadBits();
			if (!MathUtils.IsValid(newPosition) || !MathUtils.IsValid(newVelocity) || (newRotation != null && !MathUtils.IsValid(newRotation.Value)) || (newAngularVelocity != null && !MathUtils.IsValid(newAngularVelocity.Value)))
			{
				string[] array = new string[11];
				array[0] = "Received invalid position data for \"";
				array[1] = parentDebugName;
				array[2] = "\" (position: ";
				int num = 3;
				Vector2 vector = newPosition;
				array[num] = vector.ToString();
				array[4] = ", rotation: ";
				array[5] = newRotation.GetValueOrDefault().ToString();
				array[6] = ", velocity: ";
				int num2 = 7;
				vector = newVelocity;
				array[num2] = vector.ToString();
				array[8] = ", angular velocity: ";
				array[9] = newAngularVelocity.GetValueOrDefault().ToString();
				array[10] = ")";
				string errorMsg = string.Concat(array);
				GameAnalyticsManager.AddErrorEventOnce("PhysicsBody.ClientRead:InvalidData" + parentDebugName, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return null;
			}
			if (this.lastProcessedNetworkState > sendingTime)
			{
				return null;
			}
			this.LastServerState = new PosInfo(newPosition, newRotation, newVelocity, newAngularVelocity, sendingTime);
			return this.LastServerState;
		}

		// Token: 0x1700099D RID: 2461
		// (get) Token: 0x0600235A RID: 9050 RVA: 0x00165453 File Offset: 0x00163653
		public static List<PhysicsBody> List
		{
			get
			{
				return PhysicsBody.list;
			}
		}

		// Token: 0x1700099E RID: 2462
		// (get) Token: 0x0600235B RID: 9051 RVA: 0x0016545A File Offset: 0x0016365A
		// (set) Token: 0x0600235C RID: 9052 RVA: 0x00165462 File Offset: 0x00163662
		public bool Removed { get; private set; }

		// Token: 0x1700099F RID: 2463
		// (get) Token: 0x0600235D RID: 9053 RVA: 0x0016546B File Offset: 0x0016366B
		// (set) Token: 0x0600235E RID: 9054 RVA: 0x00165473 File Offset: 0x00163673
		public Vector2 LastSentPosition { get; private set; }

		// Token: 0x170009A0 RID: 2464
		// (get) Token: 0x0600235F RID: 9055 RVA: 0x0016547C File Offset: 0x0016367C
		// (set) Token: 0x06002360 RID: 9056 RVA: 0x00165484 File Offset: 0x00163684
		public float Height { get; private set; }

		// Token: 0x170009A1 RID: 2465
		// (get) Token: 0x06002361 RID: 9057 RVA: 0x0016548D File Offset: 0x0016368D
		// (set) Token: 0x06002362 RID: 9058 RVA: 0x00165495 File Offset: 0x00163695
		public float Width { get; private set; }

		// Token: 0x170009A2 RID: 2466
		// (get) Token: 0x06002363 RID: 9059 RVA: 0x0016549E File Offset: 0x0016369E
		// (set) Token: 0x06002364 RID: 9060 RVA: 0x001654A6 File Offset: 0x001636A6
		public float Radius { get; private set; }

		// Token: 0x170009A3 RID: 2467
		// (get) Token: 0x06002365 RID: 9061 RVA: 0x001654AF File Offset: 0x001636AF
		public PhysicsBody.Shape BodyShape
		{
			get
			{
				return this.bodyShape;
			}
		}

		// Token: 0x170009A4 RID: 2468
		// (get) Token: 0x06002366 RID: 9062 RVA: 0x001654B7 File Offset: 0x001636B7
		// (set) Token: 0x06002367 RID: 9063 RVA: 0x001654C0 File Offset: 0x001636C0
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

		// Token: 0x170009A5 RID: 2469
		// (get) Token: 0x06002368 RID: 9064 RVA: 0x00165548 File Offset: 0x00163748
		// (set) Token: 0x06002369 RID: 9065 RVA: 0x00165550 File Offset: 0x00163750
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

		// Token: 0x170009A6 RID: 2470
		// (get) Token: 0x0600236A RID: 9066 RVA: 0x0016558E File Offset: 0x0016378E
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

		// Token: 0x170009A7 RID: 2471
		// (get) Token: 0x0600236B RID: 9067 RVA: 0x001655B5 File Offset: 0x001637B5
		public float DrawRotation
		{
			get
			{
				return this.drawRotation;
			}
		}

		// Token: 0x170009A8 RID: 2472
		// (get) Token: 0x0600236C RID: 9068 RVA: 0x001655BD File Offset: 0x001637BD
		// (set) Token: 0x0600236D RID: 9069 RVA: 0x001655C5 File Offset: 0x001637C5
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

		// Token: 0x170009A9 RID: 2473
		// (get) Token: 0x0600236E RID: 9070 RVA: 0x001655CE File Offset: 0x001637CE
		// (set) Token: 0x0600236F RID: 9071 RVA: 0x001655D8 File Offset: 0x001637D8
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

		// Token: 0x170009AA RID: 2474
		// (get) Token: 0x06002370 RID: 9072 RVA: 0x00165750 File Offset: 0x00163950
		// (set) Token: 0x06002371 RID: 9073 RVA: 0x0016575D File Offset: 0x0016395D
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

		// Token: 0x170009AB RID: 2475
		// (get) Token: 0x06002372 RID: 9074 RVA: 0x0016577A File Offset: 0x0016397A
		public Vector2 SimPosition
		{
			get
			{
				return this.FarseerBody.Position;
			}
		}

		// Token: 0x170009AC RID: 2476
		// (get) Token: 0x06002373 RID: 9075 RVA: 0x00165787 File Offset: 0x00163987
		public Vector2 Position
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.FarseerBody.Position);
			}
		}

		// Token: 0x170009AD RID: 2477
		// (get) Token: 0x06002374 RID: 9076 RVA: 0x00165799 File Offset: 0x00163999
		public Vector2 DrawPositionOffset
		{
			get
			{
				return this.DrawPosition - this.Position;
			}
		}

		// Token: 0x170009AE RID: 2478
		// (get) Token: 0x06002375 RID: 9077 RVA: 0x001657AC File Offset: 0x001639AC
		public Vector2 PrevPosition
		{
			get
			{
				return this.prevPosition;
			}
		}

		// Token: 0x170009AF RID: 2479
		// (get) Token: 0x06002376 RID: 9078 RVA: 0x001657B4 File Offset: 0x001639B4
		public float Rotation
		{
			get
			{
				return this.FarseerBody.Rotation;
			}
		}

		// Token: 0x170009B0 RID: 2480
		// (get) Token: 0x06002377 RID: 9079 RVA: 0x001657C1 File Offset: 0x001639C1
		public float TransformedRotation
		{
			get
			{
				return PhysicsBody.TransformRotation(this.Rotation, this.Dir);
			}
		}

		// Token: 0x06002378 RID: 9080 RVA: 0x001657D4 File Offset: 0x001639D4
		public float TransformRotation(float rotation)
		{
			return PhysicsBody.TransformRotation(rotation, this.dir);
		}

		// Token: 0x06002379 RID: 9081 RVA: 0x001657E2 File Offset: 0x001639E2
		public static float TransformRotation(float rot, float dir)
		{
			if (dir >= 0f)
			{
				return rot;
			}
			return rot - 3.1415927f;
		}

		// Token: 0x170009B1 RID: 2481
		// (get) Token: 0x0600237A RID: 9082 RVA: 0x001657F5 File Offset: 0x001639F5
		// (set) Token: 0x0600237B RID: 9083 RVA: 0x00165802 File Offset: 0x00163A02
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

		// Token: 0x170009B2 RID: 2482
		// (get) Token: 0x0600237C RID: 9084 RVA: 0x00165829 File Offset: 0x00163A29
		// (set) Token: 0x0600237D RID: 9085 RVA: 0x00165836 File Offset: 0x00163A36
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

		// Token: 0x170009B3 RID: 2483
		// (get) Token: 0x0600237E RID: 9086 RVA: 0x0016585D File Offset: 0x00163A5D
		public float Mass
		{
			get
			{
				return this.FarseerBody.Mass;
			}
		}

		// Token: 0x170009B4 RID: 2484
		// (get) Token: 0x0600237F RID: 9087 RVA: 0x0016586A File Offset: 0x00163A6A
		public float Density
		{
			get
			{
				return this.density;
			}
		}

		// Token: 0x170009B5 RID: 2485
		// (get) Token: 0x06002380 RID: 9088 RVA: 0x00165872 File Offset: 0x00163A72
		// (set) Token: 0x06002381 RID: 9089 RVA: 0x0016587A File Offset: 0x00163A7A
		public Body FarseerBody { get; private set; }

		// Token: 0x170009B6 RID: 2486
		// (get) Token: 0x06002382 RID: 9090 RVA: 0x00165883 File Offset: 0x00163A83
		// (set) Token: 0x06002383 RID: 9091 RVA: 0x00165890 File Offset: 0x00163A90
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

		// Token: 0x170009B7 RID: 2487
		// (set) Token: 0x06002384 RID: 9092 RVA: 0x0016589E File Offset: 0x00163A9E
		public float Friction
		{
			set
			{
				this.FarseerBody.Friction = value;
			}
		}

		// Token: 0x170009B8 RID: 2488
		// (get) Token: 0x06002385 RID: 9093 RVA: 0x001658AC File Offset: 0x00163AAC
		// (set) Token: 0x06002386 RID: 9094 RVA: 0x001658B9 File Offset: 0x00163AB9
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

		// Token: 0x170009B9 RID: 2489
		// (get) Token: 0x06002388 RID: 9096 RVA: 0x001658DC File Offset: 0x00163ADC
		// (set) Token: 0x06002387 RID: 9095 RVA: 0x001658C7 File Offset: 0x00163AC7
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

		// Token: 0x170009BA RID: 2490
		// (get) Token: 0x0600238A RID: 9098 RVA: 0x001658F9 File Offset: 0x00163AF9
		// (set) Token: 0x06002389 RID: 9097 RVA: 0x001658E4 File Offset: 0x00163AE4
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

		// Token: 0x170009BB RID: 2491
		// (get) Token: 0x0600238B RID: 9099 RVA: 0x00165901 File Offset: 0x00163B01
		// (set) Token: 0x0600238C RID: 9100 RVA: 0x00165909 File Offset: 0x00163B09
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

		// Token: 0x0600238D RID: 9101 RVA: 0x0016591C File Offset: 0x00163B1C
		public PhysicsBody(XElement element, float scale = 1f, bool findNewContacts = true) : this(element, Vector2.Zero, scale, null, Category.Cat5, Category.Cat1 | Category.Cat3 | Category.Cat8, findNewContacts)
		{
		}

		// Token: 0x0600238E RID: 9102 RVA: 0x00165949 File Offset: 0x00163B49
		public PhysicsBody(RagdollParams.ColliderParams cParams, bool findNewContacts = true) : this(cParams, Vector2.Zero, findNewContacts)
		{
		}

		// Token: 0x0600238F RID: 9103 RVA: 0x00165958 File Offset: 0x00163B58
		public PhysicsBody(RagdollParams.LimbParams lParams, bool findNewContacts = true) : this(lParams, Vector2.Zero, findNewContacts)
		{
		}

		// Token: 0x06002390 RID: 9104 RVA: 0x00165968 File Offset: 0x00163B68
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

		// Token: 0x06002391 RID: 9105 RVA: 0x001659D4 File Offset: 0x00163BD4
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

		// Token: 0x06002392 RID: 9106 RVA: 0x00165A3C File Offset: 0x00163C3C
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

		// Token: 0x06002393 RID: 9107 RVA: 0x00165B34 File Offset: 0x00163D34
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

		// Token: 0x06002394 RID: 9108 RVA: 0x00165C6C File Offset: 0x00163E6C
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

		// Token: 0x06002395 RID: 9109 RVA: 0x00165DF8 File Offset: 0x00163FF8
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

		// Token: 0x06002396 RID: 9110 RVA: 0x00165FB0 File Offset: 0x001641B0
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

		// Token: 0x06002397 RID: 9111 RVA: 0x00166092 File Offset: 0x00164292
		public static Vector2 RotateVector(Vector2 v, float rotation)
		{
			return Vector2.Transform(v, Matrix.CreateRotationZ(-rotation));
		}

		// Token: 0x06002398 RID: 9112 RVA: 0x001660A4 File Offset: 0x001642A4
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

		// Token: 0x06002399 RID: 9113 RVA: 0x0016612C File Offset: 0x0016432C
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

		// Token: 0x0600239A RID: 9114 RVA: 0x001661CC File Offset: 0x001643CC
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

		// Token: 0x0600239B RID: 9115 RVA: 0x00166300 File Offset: 0x00164500
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

		// Token: 0x0600239C RID: 9116 RVA: 0x001663DC File Offset: 0x001645DC
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

		// Token: 0x0600239D RID: 9117 RVA: 0x001664DA File Offset: 0x001646DA
		public void ResetDynamics()
		{
			this.FarseerBody.ResetDynamics();
		}

		// Token: 0x0600239E RID: 9118 RVA: 0x001664E8 File Offset: 0x001646E8
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

		// Token: 0x0600239F RID: 9119 RVA: 0x00166544 File Offset: 0x00164744
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

		// Token: 0x060023A0 RID: 9120 RVA: 0x00166608 File Offset: 0x00164808
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

		// Token: 0x060023A1 RID: 9121 RVA: 0x00166680 File Offset: 0x00164880
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

		// Token: 0x060023A2 RID: 9122 RVA: 0x00166784 File Offset: 0x00164984
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

		// Token: 0x060023A3 RID: 9123 RVA: 0x001668A0 File Offset: 0x00164AA0
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

		// Token: 0x060023A4 RID: 9124 RVA: 0x001668EC File Offset: 0x00164AEC
		public void ApplyTorque(float torque)
		{
			if (!this.IsValidValue(torque, "torque", -3.4028235E+38f, 3.4028235E+38f))
			{
				return;
			}
			this.FarseerBody.ApplyTorque(torque);
		}

		// Token: 0x060023A5 RID: 9125 RVA: 0x00166914 File Offset: 0x00164B14
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

		// Token: 0x060023A6 RID: 9126 RVA: 0x00166970 File Offset: 0x00164B70
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

		// Token: 0x060023A7 RID: 9127 RVA: 0x001669CB File Offset: 0x00164BCB
		public void SetPrevTransform(Vector2 simPosition, float rotation)
		{
			this.prevPosition = simPosition;
			this.prevRotation = rotation;
		}

		// Token: 0x060023A8 RID: 9128 RVA: 0x001669DC File Offset: 0x00164BDC
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

		// Token: 0x060023A9 RID: 9129 RVA: 0x00166AF8 File Offset: 0x00164CF8
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

		// Token: 0x060023AA RID: 9130 RVA: 0x00166BB4 File Offset: 0x00164DB4
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

		// Token: 0x060023AB RID: 9131 RVA: 0x00166C94 File Offset: 0x00164E94
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

		// Token: 0x060023AC RID: 9132 RVA: 0x00166D10 File Offset: 0x00164F10
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

		// Token: 0x060023AD RID: 9133 RVA: 0x00166DD4 File Offset: 0x00164FD4
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

		// Token: 0x060023AE RID: 9134 RVA: 0x00166EE8 File Offset: 0x001650E8
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

		// Token: 0x060023AF RID: 9135 RVA: 0x00166F92 File Offset: 0x00165192
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

		// Token: 0x060023B0 RID: 9136 RVA: 0x00166FD1 File Offset: 0x001651D1
		public void Remove()
		{
			PhysicsBody.list.Remove(this);
			GameMain.World.Remove(this.FarseerBody);
			this.Removed = true;
		}

		// Token: 0x060023B1 RID: 9137 RVA: 0x00166FF8 File Offset: 0x001651F8
		public static void RemoveAll()
		{
			for (int i = PhysicsBody.list.Count - 1; i >= 0; i--)
			{
				PhysicsBody.list[i].Remove();
			}
		}

		// Token: 0x060023B2 RID: 9138 RVA: 0x0016702C File Offset: 0x0016522C
		public static bool IsValidShape(float radius, float height, float width)
		{
			return radius > 0f || (height > 0f && width > 0f);
		}

		// Token: 0x060023B3 RID: 9139 RVA: 0x0016704C File Offset: 0x0016524C
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

		// Token: 0x060023B5 RID: 9141 RVA: 0x00167098 File Offset: 0x00165298
		[CompilerGenerated]
		private void <DebugDraw>g__DrawShape|5_0(Vector2 position, float rotation, Color color, ref PhysicsBody.<>c__DisplayClass5_0 A_4)
		{
			float radius = ConvertUnits.ToDisplayUnits(this.Radius);
			float height = ConvertUnits.ToDisplayUnits(this.Height);
			float width = ConvertUnits.ToDisplayUnits(this.Width);
			switch (this.BodyShape)
			{
			case PhysicsBody.Shape.Circle:
				GUI.DrawDonutSection(A_4.spriteBatch, position.FlipY(), new Range<float>(radius - 0.5f, radius + 0.5f), 6.2831855f, color, 0f, -rotation);
				return;
			case PhysicsBody.Shape.Rectangle:
				GUI.DrawRectangle(A_4.spriteBatch, position.FlipY(), new Vector2(width, height), new Vector2(width, height) / 2f, -rotation, color, 0f, 1f, GUI.OutlinePosition.Centered);
				return;
			case PhysicsBody.Shape.Capsule:
				GUI.DrawCapsule(A_4.spriteBatch, position.FlipY(), height, radius, -rotation - 1.5707964f, color, 0f, 1f);
				return;
			case PhysicsBody.Shape.HorizontalCapsule:
				GUI.DrawCapsule(A_4.spriteBatch, position.FlipY(), width, radius, -rotation, color, 0f, 1f);
				return;
			default:
				throw new NotImplementedException();
			}
		}

		// Token: 0x040011B4 RID: 4532
		public PosInfo LastServerState;

		// Token: 0x040011B5 RID: 4533
		public const float MinDensity = 0.01f;

		// Token: 0x040011B6 RID: 4534
		public const float DefaultAngularDamping = 5f;

		// Token: 0x040011B7 RID: 4535
		private static readonly List<PhysicsBody> list = new List<PhysicsBody>();

		// Token: 0x040011B8 RID: 4536
		protected Vector2 prevPosition;

		// Token: 0x040011B9 RID: 4537
		protected float prevRotation;

		// Token: 0x040011BA RID: 4538
		protected Vector2? targetPosition;

		// Token: 0x040011BB RID: 4539
		protected float? targetRotation;

		// Token: 0x040011BC RID: 4540
		private Vector2 drawPosition;

		// Token: 0x040011BD RID: 4541
		private float drawRotation;

		// Token: 0x040011C0 RID: 4544
		private PhysicsBody.Shape bodyShape;

		// Token: 0x040011C4 RID: 4548
		private readonly float density;

		// Token: 0x040011C5 RID: 4549
		private float dir;

		// Token: 0x040011C6 RID: 4550
		private Vector2 drawOffset;

		// Token: 0x040011C7 RID: 4551
		private float rotationOffset;

		// Token: 0x040011C8 RID: 4552
		private float lastProcessedNetworkState;

		// Token: 0x040011C9 RID: 4553
		public float? PositionSmoothingFactor;

		// Token: 0x040011CA RID: 4554
		public Submarine Submarine;

		// Token: 0x040011CB RID: 4555
		private bool isEnabled;

		// Token: 0x040011CC RID: 4556
		private bool isPhysEnabled;

		// Token: 0x040011CE RID: 4558
		private Category _collisionCategories;

		// Token: 0x040011CF RID: 4559
		private Category _collidesWith;

		// Token: 0x040011D0 RID: 4560
		private bool _suppressSmoothRotationCalls;

		// Token: 0x040011D1 RID: 4561
		private int smoothRotationSuppressionCounter;

		// Token: 0x02000BF1 RID: 3057
		public enum Shape
		{
			// Token: 0x04004957 RID: 18775
			Circle,
			// Token: 0x04004958 RID: 18776
			Rectangle,
			// Token: 0x04004959 RID: 18777
			Capsule,
			// Token: 0x0400495A RID: 18778
			HorizontalCapsule
		}
	}
}
