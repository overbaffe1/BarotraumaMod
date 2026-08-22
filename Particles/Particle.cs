using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Particles
{
	// Token: 0x0200044E RID: 1102
	internal class Particle
	{
		// Token: 0x170012B5 RID: 4789
		// (get) Token: 0x060049BF RID: 18879 RVA: 0x0028C9EF File Offset: 0x0028ABEF
		// (set) Token: 0x060049C0 RID: 18880 RVA: 0x0028C9F7 File Offset: 0x0028ABF7
		public ParticleDrawOrder DrawOrder { get; private set; }

		// Token: 0x170012B6 RID: 4790
		// (get) Token: 0x060049C1 RID: 18881 RVA: 0x0028CA00 File Offset: 0x0028AC00
		public ParticlePrefab.DrawTargetType DrawTarget
		{
			get
			{
				return this.prefab.DrawTarget;
			}
		}

		// Token: 0x170012B7 RID: 4791
		// (get) Token: 0x060049C2 RID: 18882 RVA: 0x0028CA0D File Offset: 0x0028AC0D
		public ParticleBlendState BlendState
		{
			get
			{
				return this.prefab.BlendState;
			}
		}

		// Token: 0x170012B8 RID: 4792
		// (get) Token: 0x060049C3 RID: 18883 RVA: 0x0028CA1A File Offset: 0x0028AC1A
		// (set) Token: 0x060049C4 RID: 18884 RVA: 0x0028CA22 File Offset: 0x0028AC22
		public float StartDelay
		{
			get
			{
				return this.startDelay;
			}
			set
			{
				this.startDelay = Math.Max(value, 0f);
			}
		}

		// Token: 0x170012B9 RID: 4793
		// (get) Token: 0x060049C5 RID: 18885 RVA: 0x0028CA35 File Offset: 0x0028AC35
		// (set) Token: 0x060049C6 RID: 18886 RVA: 0x0028CA3D File Offset: 0x0028AC3D
		public Vector2 Size
		{
			get
			{
				return this.size;
			}
			set
			{
				this.size = value;
			}
		}

		// Token: 0x170012BA RID: 4794
		// (get) Token: 0x060049C7 RID: 18887 RVA: 0x0028CA46 File Offset: 0x0028AC46
		public Hull CurrentHull
		{
			get
			{
				return this.currentHull;
			}
		}

		// Token: 0x170012BB RID: 4795
		// (get) Token: 0x060049C8 RID: 18888 RVA: 0x0028CA4E File Offset: 0x0028AC4E
		public ParticlePrefab Prefab
		{
			get
			{
				return this.prefab;
			}
		}

		// Token: 0x060049C9 RID: 18889 RVA: 0x0028CA56 File Offset: 0x0028AC56
		public override string ToString()
		{
			return this.debugName;
		}

		// Token: 0x060049CA RID: 18890 RVA: 0x0028CA60 File Offset: 0x0028AC60
		public void Init(ParticlePrefab prefab, Vector2 spawnPosition, Vector2 speed, float spawnRotation, Hull hullGuess = null, ParticleDrawOrder drawOrder = ParticleDrawOrder.Default, float collisionIgnoreTimer = 0f, float lifeTimeMultiplier = 1f, Tuple<Vector2, Vector2> tracerPoints = null)
		{
			this.prefab = prefab;
			this.debugName = prefab.Name;
			this.spriteIndex = Rand.Int(prefab.Sprites.Count, Rand.RandSync.Unsynced);
			this.animState = 0f;
			this.animFrame = 0;
			this.currentHull = (prefab.CanEnterSubs ? Hull.FindHull(spawnPosition, hullGuess, true, true) : null);
			this.size = prefab.StartSizeMin + (prefab.StartSizeMax - prefab.StartSizeMin) * Rand.Range(0f, 1f, Rand.RandSync.Unsynced);
			if (tracerPoints != null)
			{
				this.size = new Vector2(Vector2.Distance(tracerPoints.Item1, tracerPoints.Item2), this.size.Y);
				spawnPosition = (tracerPoints.Item1 + tracerPoints.Item2) / 2f;
			}
			this.RefreshColliderSize();
			this.sizeChange = prefab.SizeChangeMin + (prefab.SizeChangeMax - prefab.SizeChangeMin) * Rand.Range(0f, 1f, Rand.RandSync.Unsynced);
			this.changesSize = !this.sizeChange.NearlyEquals(Vector2.Zero);
			Hull hull = this.currentHull;
			if (((hull != null) ? hull.Submarine : null) != null)
			{
				spawnPosition -= this.currentHull.Submarine.Position;
			}
			this.position = (this.prevPosition = (this.drawPosition = spawnPosition));
			this.velocity = (MathUtils.IsValid(speed) ? speed : Vector2.Zero);
			this.rotation = spawnRotation + Rand.Range(prefab.StartRotationMinRad, prefab.StartRotationMaxRad, Rand.RandSync.Unsynced);
			this.prevRotation = spawnRotation;
			this.angularVelocity = Rand.Range(prefab.AngularVelocityMinRad, prefab.AngularVelocityMaxRad, Rand.RandSync.Unsynced);
			if (prefab.LifeTimeMin <= 0f)
			{
				this.totalLifeTime = prefab.LifeTime * lifeTimeMultiplier;
				this.lifeTime = prefab.LifeTime * lifeTimeMultiplier;
			}
			else
			{
				this.totalLifeTime = Rand.Range(prefab.LifeTimeMin, prefab.LifeTime, Rand.RandSync.Unsynced) * lifeTimeMultiplier;
				this.lifeTime = this.totalLifeTime * lifeTimeMultiplier;
			}
			this.startDelay = Rand.Range(prefab.StartDelayMin, prefab.StartDelayMax, Rand.RandSync.Unsynced);
			this.UseMiddleColor = prefab.UseMiddleColor;
			this.color = prefab.StartColor;
			this.changeColor = (prefab.StartColor != prefab.EndColor);
			this.ColorMultiplier = Vector4.One;
			this.velocityChange = prefab.VelocityChangeDisplay;
			this.velocityChangeWater = prefab.VelocityChangeWaterDisplay;
			this.HighQualityCollisionDetection = false;
			this.VelocityChangeMultiplier = 1f;
			this.OnChangeHull = null;
			this.OnCollision = null;
			this.subEmitters.Clear();
			this.hasSubEmitters = false;
			foreach (ParticleEmitterPrefab emitterPrefab in prefab.SubEmitters)
			{
				this.subEmitters.Add(new ParticleEmitter(emitterPrefab));
				this.hasSubEmitters = true;
			}
			if (prefab.UseCollision)
			{
				this.hullGaps = ((this.currentHull == null) ? new List<Gap>() : this.currentHull.ConnectedGaps);
			}
			if (prefab.RotateToDirection)
			{
				this.rotation = MathUtils.VectorToAngle(new Vector2(this.velocity.X, -this.velocity.Y));
				this.prevRotation = spawnRotation;
			}
			this.DrawOrder = drawOrder;
			this.collisionIgnoreTimer = collisionIgnoreTimer;
		}

		// Token: 0x060049CB RID: 18891 RVA: 0x0028CDEC File Offset: 0x0028AFEC
		public Particle.UpdateResult Update(float deltaTime)
		{
			if (this.startDelay > 0f)
			{
				this.startDelay -= deltaTime;
				return Particle.UpdateResult.Normal;
			}
			this.prevPosition = this.position;
			this.prevRotation = this.rotation;
			this.position.X = this.position.X + this.velocity.X * deltaTime;
			this.position.Y = this.position.Y + this.velocity.Y * deltaTime;
			if (this.prefab.RotateToDirection)
			{
				if (this.velocityChange != Vector2.Zero || this.angularVelocity != 0f)
				{
					Vector2 relativeVel = this.velocity;
					Hull hull = this.currentHull;
					if (((hull != null) ? hull.Submarine : null) != null)
					{
						relativeVel -= ConvertUnits.ToDisplayUnits(this.currentHull.Submarine.Velocity);
					}
					this.rotation = MathUtils.VectorToAngle(new Vector2(relativeVel.X, -relativeVel.Y));
				}
			}
			else
			{
				this.rotation += this.angularVelocity * deltaTime;
			}
			bool inWater = this.currentHull == null || (this.currentHull.Submarine != null && this.position.Y < this.currentHull.Surface);
			if (inWater)
			{
				this.velocity.X = this.velocity.X + this.velocityChangeWater.X * this.VelocityChangeMultiplier * deltaTime;
				this.velocity.Y = this.velocity.Y + this.velocityChangeWater.Y * this.VelocityChangeMultiplier * deltaTime;
				if (this.prefab.WaterDrag > 0f)
				{
					this.ApplyDrag(this.prefab.WaterDrag, deltaTime);
				}
			}
			else
			{
				this.velocity.X = this.velocity.X + this.velocityChange.X * this.VelocityChangeMultiplier * deltaTime;
				this.velocity.Y = this.velocity.Y + this.velocityChange.Y * this.VelocityChangeMultiplier * deltaTime;
				if (this.prefab.Drag > 0f)
				{
					this.ApplyDrag(this.prefab.Drag, deltaTime);
				}
			}
			if (this.changesSize)
			{
				this.size.X = this.size.X + this.sizeChange.X * deltaTime;
				this.size.Y = this.size.Y + this.sizeChange.Y * deltaTime;
				this.RefreshColliderSize();
			}
			if (this.UseMiddleColor)
			{
				if (this.lifeTime > this.totalLifeTime * 0.5f)
				{
					this.color = Color.Lerp(this.prefab.MiddleColor, this.prefab.StartColor, (this.lifeTime / this.totalLifeTime - 0.5f) * 2f);
				}
				else
				{
					this.color = Color.Lerp(this.prefab.EndColor, this.prefab.MiddleColor, this.lifeTime / this.totalLifeTime * 2f);
				}
			}
			else if (this.changeColor)
			{
				this.color = Color.Lerp(this.prefab.EndColor, this.prefab.StartColor, this.lifeTime / this.totalLifeTime);
			}
			if (this.prefab.Sprites[this.spriteIndex] is SpriteSheet)
			{
				this.animState += deltaTime;
				int frameCount = ((SpriteSheet)this.prefab.Sprites[this.spriteIndex]).FrameCount;
				if (this.prefab.LoopAnim)
				{
					this.animFrame = (int)(Math.Floor((double)(this.animState / this.prefab.AnimDuration * (float)frameCount)) % (double)frameCount);
				}
				else
				{
					this.animFrame = (int)Math.Min(Math.Floor((double)(this.animState / this.prefab.AnimDuration * (float)frameCount)), (double)(frameCount - 1));
				}
			}
			this.lifeTime -= deltaTime;
			if (this.lifeTime <= 0f || this.color.A <= 0 || this.size.X <= 0f || this.size.Y <= 0f)
			{
				return Particle.UpdateResult.Delete;
			}
			if (this.hasSubEmitters)
			{
				foreach (ParticleEmitter emitter in this.subEmitters)
				{
					emitter.Emit(deltaTime, this.position, this.currentHull, 0f, this.rotation, 1f, emitter.Prefab.Properties.CopyParentParticleScale ? Math.Max(this.size.X, this.size.Y) : 1f, 1f, null, null, false, null);
				}
			}
			if (this.collisionIgnoreTimer > 0f)
			{
				this.collisionIgnoreTimer -= deltaTime;
				if (this.collisionIgnoreTimer <= 0f && this.currentHull == null)
				{
					this.currentHull = Hull.FindHull(this.position, this.currentHull, false, true);
				}
				return Particle.UpdateResult.Normal;
			}
			if (!this.prefab.UseCollision)
			{
				return Particle.UpdateResult.Normal;
			}
			if (this.HighQualityCollisionDetection)
			{
				return this.CollisionUpdate();
			}
			this.collisionUpdateTimer -= deltaTime;
			if (this.collisionUpdateTimer <= 0f)
			{
				this.collisionUpdateTimer = 0.5f - Math.Min((Math.Abs(this.velocity.X) + Math.Abs(this.velocity.Y)) * 0.01f, 0.45f);
				return this.CollisionUpdate();
			}
			return Particle.UpdateResult.Normal;
		}

		// Token: 0x060049CC RID: 18892 RVA: 0x0028D394 File Offset: 0x0028B594
		private Particle.UpdateResult CollisionUpdate()
		{
			if (this.currentHull == null)
			{
				Hull collidedHull = Hull.FindHull(this.position, null, true, true);
				if (collidedHull != null)
				{
					if (this.prefab.DeleteOnCollision)
					{
						return Particle.UpdateResult.Delete;
					}
					this.OnWallCollisionOutside(collidedHull);
				}
			}
			else
			{
				Rectangle hullRect = this.currentHull.Rect;
				Vector2 collisionNormal = Vector2.Zero;
				if (this.velocity.Y < 0f && this.position.Y - this.colliderRadius.Y < (float)(hullRect.Y - hullRect.Height))
				{
					collisionNormal = new Vector2(0f, 1f);
				}
				else if (this.velocity.Y > 0f && this.position.Y + this.colliderRadius.Y > (float)hullRect.Y)
				{
					collisionNormal = new Vector2(0f, -1f);
				}
				if (collisionNormal != Vector2.Zero)
				{
					bool gapFound = false;
					foreach (Gap gap in this.hullGaps)
					{
						if (gap.Open > 0.9f && !gap.IsHorizontal && (float)gap.Rect.X <= this.position.X && (float)gap.Rect.Right >= this.position.X)
						{
							float hullCenterY = (float)(this.currentHull.Rect.Y - this.currentHull.Rect.Height / 2);
							int gapDir = Math.Sign((float)gap.Rect.Y - hullCenterY);
							if (Math.Sign(this.velocity.Y) == gapDir && Math.Sign(this.position.Y - hullCenterY) == gapDir)
							{
								gapFound = true;
								break;
							}
						}
					}
					if (this.prefab.DeleteOnCollision && !gapFound)
					{
						Particle.OnChangeHullHandler onCollision = this.OnCollision;
						if (onCollision != null)
						{
							onCollision(this.position, this.currentHull);
						}
						return Particle.UpdateResult.Delete;
					}
					this.<CollisionUpdate>g__handleCollision|59_0(gapFound, collisionNormal);
				}
				collisionNormal = Vector2.Zero;
				if (this.velocity.X < 0f && this.position.X - this.colliderRadius.X < (float)hullRect.X)
				{
					collisionNormal = new Vector2(1f, 0f);
				}
				else if (this.velocity.X > 0f && this.position.X + this.colliderRadius.X > (float)hullRect.Right)
				{
					collisionNormal = new Vector2(-1f, 0f);
				}
				if (collisionNormal != Vector2.Zero)
				{
					bool gapFound2 = false;
					foreach (Gap gap2 in this.hullGaps)
					{
						if (gap2.Open > 0.9f && gap2.IsHorizontal && (float)gap2.Rect.Y >= this.position.Y && (float)(gap2.WorldRect.Y - gap2.Rect.Height) <= this.position.Y)
						{
							int gapDir2 = Math.Sign(gap2.Rect.Center.X - this.currentHull.Rect.Center.X);
							if (Math.Sign(this.velocity.X) == gapDir2 && Math.Sign(this.position.X - (float)this.currentHull.Rect.Center.X) == gapDir2)
							{
								gapFound2 = true;
								break;
							}
						}
					}
					if (this.prefab.DeleteOnCollision && !gapFound2)
					{
						Particle.OnChangeHullHandler onCollision2 = this.OnCollision;
						if (onCollision2 != null)
						{
							onCollision2(this.position, this.currentHull);
						}
						return Particle.UpdateResult.Delete;
					}
					this.<CollisionUpdate>g__handleCollision|59_0(gapFound2, collisionNormal);
				}
			}
			return Particle.UpdateResult.Normal;
		}

		// Token: 0x060049CD RID: 18893 RVA: 0x0028D7CC File Offset: 0x0028B9CC
		private void RefreshColliderSize()
		{
			if (!this.prefab.UseCollision)
			{
				return;
			}
			this.colliderRadius = new Vector2(this.prefab.CollisionRadius);
			if (!this.prefab.InvariantCollisionSize)
			{
				this.colliderRadius *= this.size;
			}
		}

		// Token: 0x060049CE RID: 18894 RVA: 0x0028D824 File Offset: 0x0028BA24
		private void ApplyDrag(float dragCoefficient, float deltaTime)
		{
			Vector2 newVel = this.velocity;
			float speed = this.velocity.Length();
			if (speed < 0.01f)
			{
				return;
			}
			newVel /= speed;
			float drag = speed * speed * dragCoefficient * 0.01f * deltaTime;
			if (drag > speed)
			{
				newVel = Vector2.Zero;
			}
			else
			{
				speed -= drag;
				newVel *= speed;
			}
			this.velocity = newVel;
		}

		// Token: 0x060049CF RID: 18895 RVA: 0x0028D884 File Offset: 0x0028BA84
		private void OnWallCollisionInside(Hull prevHull, Vector2 collisionNormal)
		{
			if (prevHull == null)
			{
				return;
			}
			Rectangle prevHullRect = prevHull.Rect;
			if (Math.Abs(collisionNormal.X) > Math.Abs(collisionNormal.Y))
			{
				if (collisionNormal.X > 0f)
				{
					this.position.X = Math.Max(this.position.X, (float)prevHullRect.X + this.colliderRadius.X);
				}
				else
				{
					this.position.X = Math.Min(this.position.X, (float)prevHullRect.Right - this.colliderRadius.X);
				}
				this.velocity.X = (float)Math.Sign(collisionNormal.X) * Math.Abs(this.velocity.X) * this.prefab.Restitution;
				this.velocity.Y = this.velocity.Y * (1f - this.prefab.Friction);
			}
			else
			{
				if (collisionNormal.Y > 0f)
				{
					this.position.Y = Math.Max(this.position.Y, (float)(prevHullRect.Y - prevHullRect.Height) + this.colliderRadius.Y);
				}
				else
				{
					this.position.Y = Math.Min(this.position.Y, (float)prevHullRect.Y - this.colliderRadius.Y);
				}
				this.velocity.X = this.velocity.X * (1f - this.prefab.Friction);
				this.velocity.Y = (float)Math.Sign(collisionNormal.Y) * Math.Abs(this.velocity.Y) * this.prefab.Restitution;
			}
			Particle.OnChangeHullHandler onCollision = this.OnCollision;
			if (onCollision == null)
			{
				return;
			}
			onCollision(this.position, this.currentHull);
		}

		// Token: 0x060049D0 RID: 18896 RVA: 0x0028DA60 File Offset: 0x0028BC60
		private void OnWallCollisionOutside(Hull collisionHull)
		{
			Rectangle hullRect = collisionHull.Rect;
			Vector2 center = new Vector2((float)(hullRect.X + hullRect.Width / 2), (float)(hullRect.Y - hullRect.Height / 2));
			if (this.position.Y < center.Y)
			{
				this.position.Y = (float)(hullRect.Y - hullRect.Height) - this.colliderRadius.Y;
				this.velocity.X = this.velocity.X * (1f - this.prefab.Friction);
				this.velocity.Y = -this.velocity.Y * this.prefab.Restitution;
			}
			else if (this.position.Y > center.Y)
			{
				this.position.Y = (float)hullRect.Y + this.colliderRadius.Y;
				this.velocity.X = this.velocity.X * (1f - this.prefab.Friction);
				this.velocity.Y = -this.velocity.Y * this.prefab.Restitution;
			}
			if (this.position.X < center.X)
			{
				this.position.X = (float)hullRect.X - this.colliderRadius.X;
				this.velocity.X = -this.velocity.X * this.prefab.Restitution;
				this.velocity.Y = this.velocity.Y * (1f - this.prefab.Friction);
			}
			else if (this.position.X > center.X)
			{
				this.position.X = (float)(hullRect.X + hullRect.Width) + this.colliderRadius.X;
				this.velocity.X = -this.velocity.X * this.prefab.Restitution;
				this.velocity.Y = this.velocity.Y * (1f - this.prefab.Friction);
			}
			Particle.OnChangeHullHandler onCollision = this.OnCollision;
			if (onCollision != null)
			{
				onCollision(this.position, this.currentHull);
			}
			this.velocity *= this.prefab.Restitution;
		}

		// Token: 0x060049D1 RID: 18897 RVA: 0x0028DCB6 File Offset: 0x0028BEB6
		public void UpdateDrawPos()
		{
			this.drawPosition = Timing.Interpolate(this.prevPosition, this.position);
			this.drawRotation = Timing.Interpolate(this.prevRotation, this.rotation);
		}

		// Token: 0x060049D2 RID: 18898 RVA: 0x0028DCE8 File Offset: 0x0028BEE8
		public void Draw(SpriteBatch spriteBatch)
		{
			if (this.startDelay > 0f)
			{
				return;
			}
			Vector2 drawSize = this.size;
			if (this.prefab.GrowTime > 0f && this.totalLifeTime - this.lifeTime < this.prefab.GrowTime)
			{
				drawSize *= MathUtils.SmoothStep((this.totalLifeTime - this.lifeTime) / this.prefab.GrowTime);
			}
			Color currColor = new Color(this.color.ToVector4() * this.ColorMultiplier);
			Vector2 drawPos = this.drawPosition;
			Hull hull = this.currentHull;
			Submarine sub = (hull != null) ? hull.Submarine : null;
			if (sub != null)
			{
				drawPos += sub.DrawPosition;
			}
			drawPos = new Vector2(drawPos.X, -drawPos.Y);
			SpriteSheet sheet = this.prefab.Sprites[this.spriteIndex] as SpriteSheet;
			if (sheet != null)
			{
				sheet.Draw(spriteBatch, this.animFrame, drawPos, currColor * ((float)currColor.A / 255f), this.prefab.Sprites[this.spriteIndex].Origin, this.drawRotation, drawSize, SpriteEffects.None, new float?(this.prefab.Sprites[this.spriteIndex].Depth));
				return;
			}
			this.prefab.Sprites[this.spriteIndex].Draw(spriteBatch, drawPos, currColor * ((float)currColor.A / 255f), this.prefab.Sprites[this.spriteIndex].Origin, this.drawRotation, drawSize, SpriteEffects.None, new float?(this.prefab.Sprites[this.spriteIndex].Depth));
		}

		// Token: 0x060049D4 RID: 18900 RVA: 0x0028DED0 File Offset: 0x0028C0D0
		[CompilerGenerated]
		private void <CollisionUpdate>g__handleCollision|59_0(bool gapFound, Vector2 collisionNormal)
		{
			if (!gapFound)
			{
				this.OnWallCollisionInside(this.currentHull, collisionNormal);
				return;
			}
			Hull newHull = Hull.FindHull(this.position, this.currentHull, false, true);
			if (newHull != this.currentHull)
			{
				this.currentHull = newHull;
				this.hullGaps = ((this.currentHull == null) ? new List<Gap>() : this.currentHull.ConnectedGaps);
				Particle.OnChangeHullHandler onChangeHull = this.OnChangeHull;
				if (onChangeHull == null)
				{
					return;
				}
				onChangeHull(this.position, this.currentHull);
			}
		}

		// Token: 0x04002674 RID: 9844
		private ParticlePrefab prefab;

		// Token: 0x04002675 RID: 9845
		private string debugName = "Particle (uninitialized)";

		// Token: 0x04002676 RID: 9846
		public Particle.OnChangeHullHandler OnChangeHull;

		// Token: 0x04002677 RID: 9847
		public Particle.OnChangeHullHandler OnCollision;

		// Token: 0x04002678 RID: 9848
		private Vector2 position;

		// Token: 0x04002679 RID: 9849
		private Vector2 prevPosition;

		// Token: 0x0400267A RID: 9850
		private Vector2 velocity;

		// Token: 0x0400267B RID: 9851
		private float rotation;

		// Token: 0x0400267C RID: 9852
		private float prevRotation;

		// Token: 0x0400267D RID: 9853
		private float angularVelocity;

		// Token: 0x0400267E RID: 9854
		private float collisionIgnoreTimer;

		// Token: 0x0400267F RID: 9855
		private Vector2 size;

		// Token: 0x04002680 RID: 9856
		private Vector2 sizeChange;

		// Token: 0x04002681 RID: 9857
		private Color color;

		// Token: 0x04002682 RID: 9858
		private bool changeColor;

		// Token: 0x04002683 RID: 9859
		private bool UseMiddleColor;

		// Token: 0x04002684 RID: 9860
		private int spriteIndex;

		// Token: 0x04002685 RID: 9861
		private float totalLifeTime;

		// Token: 0x04002686 RID: 9862
		private float lifeTime;

		// Token: 0x04002687 RID: 9863
		private float startDelay;

		// Token: 0x04002688 RID: 9864
		private Vector2 velocityChange;

		// Token: 0x04002689 RID: 9865
		private Vector2 velocityChangeWater;

		// Token: 0x0400268A RID: 9866
		private Vector2 drawPosition;

		// Token: 0x0400268B RID: 9867
		private float drawRotation;

		// Token: 0x0400268C RID: 9868
		private Vector2 colliderRadius;

		// Token: 0x0400268D RID: 9869
		private Hull currentHull;

		// Token: 0x0400268E RID: 9870
		private List<Gap> hullGaps;

		// Token: 0x0400268F RID: 9871
		private bool hasSubEmitters;

		// Token: 0x04002690 RID: 9872
		private readonly List<ParticleEmitter> subEmitters = new List<ParticleEmitter>();

		// Token: 0x04002691 RID: 9873
		private float animState;

		// Token: 0x04002692 RID: 9874
		private int animFrame;

		// Token: 0x04002693 RID: 9875
		private float collisionUpdateTimer;

		// Token: 0x04002694 RID: 9876
		private bool changesSize;

		// Token: 0x04002695 RID: 9877
		public bool HighQualityCollisionDetection;

		// Token: 0x04002696 RID: 9878
		public Vector4 ColorMultiplier;

		// Token: 0x04002697 RID: 9879
		public float VelocityChangeMultiplier;

		// Token: 0x020011B4 RID: 4532
		// (Invoke) Token: 0x06009186 RID: 37254
		public delegate void OnChangeHullHandler(Vector2 position, Hull currentHull);

		// Token: 0x020011B5 RID: 4533
		public enum UpdateResult
		{
			// Token: 0x04005CBF RID: 23743
			Normal,
			// Token: 0x04005CC0 RID: 23744
			Delete
		}
	}
}
