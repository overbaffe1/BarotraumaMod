using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001A1 RID: 417
	internal class SteeringManager
	{
		// Token: 0x17000C54 RID: 3156
		// (get) Token: 0x06002FCE RID: 12238 RVA: 0x001F8232 File Offset: 0x001F6432
		// (set) Token: 0x06002FCF RID: 12239 RVA: 0x001F823A File Offset: 0x001F643A
		public Vector2 AvoidDir { get; private set; }

		// Token: 0x17000C55 RID: 3157
		// (get) Token: 0x06002FD0 RID: 12240 RVA: 0x001F8243 File Offset: 0x001F6443
		// (set) Token: 0x06002FD1 RID: 12241 RVA: 0x001F824B File Offset: 0x001F644B
		public Vector2 AvoidRayCastHitPosition { get; private set; }

		// Token: 0x17000C56 RID: 3158
		// (get) Token: 0x06002FD2 RID: 12242 RVA: 0x001F8254 File Offset: 0x001F6454
		// (set) Token: 0x06002FD3 RID: 12243 RVA: 0x001F825C File Offset: 0x001F645C
		public Vector2 AvoidLookAheadPos { get; private set; }

		// Token: 0x17000C57 RID: 3159
		// (get) Token: 0x06002FD4 RID: 12244 RVA: 0x001F8265 File Offset: 0x001F6465
		// (set) Token: 0x06002FD5 RID: 12245 RVA: 0x001F826D File Offset: 0x001F646D
		public float WanderAngle
		{
			get
			{
				return this.wanderAngle;
			}
			set
			{
				this.wanderAngle = value;
			}
		}

		// Token: 0x06002FD6 RID: 12246 RVA: 0x001F8276 File Offset: 0x001F6476
		public SteeringManager(ISteerable host)
		{
			this.host = host;
			this.wanderAngle = Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06002FD7 RID: 12247 RVA: 0x001F829B File Offset: 0x001F649B
		public void SteeringSeek(Vector2 targetSimPos, float weight = 1f)
		{
			this.steering += this.DoSteeringSeek(targetSimPos, weight);
		}

		// Token: 0x06002FD8 RID: 12248 RVA: 0x001F82B6 File Offset: 0x001F64B6
		public void SteeringWander(float weight = 1f, bool avoidWanderingOutsideLevel = false)
		{
			this.steering += this.DoSteeringWander(weight, avoidWanderingOutsideLevel);
		}

		// Token: 0x06002FD9 RID: 12249 RVA: 0x001F82D4 File Offset: 0x001F64D4
		public void SteeringAvoid(float deltaTime, float lookAheadDistance, float weight = 1f)
		{
			this.steering += this.DoSteeringAvoid(deltaTime, lookAheadDistance, weight, null);
		}

		// Token: 0x06002FDA RID: 12250 RVA: 0x001F8304 File Offset: 0x001F6504
		public void SteeringManual(float deltaTime, Vector2 velocity)
		{
			if (MathUtils.IsValid(velocity))
			{
				this.steering += velocity;
			}
		}

		// Token: 0x06002FDB RID: 12251 RVA: 0x001F8320 File Offset: 0x001F6520
		public void Reset()
		{
			this.steering = Vector2.Zero;
		}

		// Token: 0x06002FDC RID: 12252 RVA: 0x001F832D File Offset: 0x001F652D
		public void ResetX()
		{
			this.steering.X = 0f;
		}

		// Token: 0x06002FDD RID: 12253 RVA: 0x001F833F File Offset: 0x001F653F
		public void ResetY()
		{
			this.steering.Y = 0f;
		}

		// Token: 0x06002FDE RID: 12254 RVA: 0x001F8354 File Offset: 0x001F6554
		public virtual void Update(float speed)
		{
			if (this.steering == Vector2.Zero || !MathUtils.IsValid(this.steering))
			{
				this.steering = Vector2.Zero;
				this.host.Steering = Vector2.Zero;
				return;
			}
			if (this.steering.LengthSquared() > speed * speed)
			{
				this.steering = Vector2.Normalize(this.steering) * Math.Abs(speed);
			}
			AIController aiController = this.host as AIController;
			if (aiController != null && ((aiController != null) ? aiController.Character.CharacterHealth.GetAfflictionOfType("invertcontrols".ToIdentifier(), true) : null) != null)
			{
				this.steering = -this.steering;
			}
			this.host.Steering = this.steering;
		}

		// Token: 0x06002FDF RID: 12255 RVA: 0x001F841C File Offset: 0x001F661C
		protected virtual Vector2 DoSteeringSeek(Vector2 target, float weight)
		{
			Vector2 targetVel = target - this.host.SimPosition;
			if (targetVel.LengthSquared() < 1E-05f)
			{
				return Vector2.Zero;
			}
			return Vector2.Normalize(targetVel) * weight;
		}

		// Token: 0x06002FE0 RID: 12256 RVA: 0x001F8460 File Offset: 0x001F6660
		protected virtual Vector2 DoSteeringWander(float weight, bool avoidWanderingOutsideLevel)
		{
			Vector2 circleCenter = (this.host.Steering == Vector2.Zero) ? Vector2.UnitY : this.host.Steering;
			circleCenter = Vector2.Normalize(circleCenter) * 2.5f;
			Vector2 displacement = new Vector2((float)Math.Cos((double)this.wanderAngle), (float)Math.Sin((double)this.wanderAngle));
			displacement *= 0.3f;
			float angleChange = 1.5f;
			this.wanderAngle += Rand.Range(0f, 1f, Rand.RandSync.Unsynced) * angleChange - angleChange * 0.5f;
			Vector2 newSteering = circleCenter + displacement;
			if (avoidWanderingOutsideLevel && Level.Loaded != null)
			{
				float margin = 5000f;
				if (this.host.WorldPosition.X < -margin)
				{
					newSteering.X += (-margin - this.host.WorldPosition.X) * weight / margin;
				}
				else if (this.host.WorldPosition.X > (float)Level.Loaded.Size.X - margin)
				{
					newSteering.X -= (this.host.WorldPosition.X - ((float)Level.Loaded.Size.X - margin)) * weight / margin;
				}
			}
			float steeringSpeed = (newSteering + this.host.Steering).Length();
			if (steeringSpeed > weight)
			{
				newSteering = Vector2.Normalize(newSteering) * weight;
			}
			return newSteering;
		}

		// Token: 0x06002FE1 RID: 12257 RVA: 0x001F85E4 File Offset: 0x001F67E4
		protected virtual Vector2 DoSteeringAvoid(float deltaTime, float lookAheadDistance, float weight, Vector2? heading = null)
		{
			if (this.steering == Vector2.Zero || this.host.Steering == Vector2.Zero)
			{
				return Vector2.Zero;
			}
			if (Timing.TotalTime >= (double)(this.lastRayCastTime + 0.5f))
			{
				this.avoidRayCastHit = false;
				this.AvoidLookAheadPos = this.host.SimPosition + Vector2.Normalize(this.host.Steering) * lookAheadDistance;
				this.lastRayCastTime = (float)Timing.TotalTime;
				Body closestBody = Submarine.CheckVisibility(this.host.SimPosition, this.AvoidLookAheadPos, false, false, true, true, true, null);
				if (closestBody != null)
				{
					this.avoidRayCastHit = true;
					this.AvoidRayCastHitPosition = Submarine.LastPickedPosition;
					this.AvoidDir = Submarine.LastPickedNormal;
					this.AvoidDir = MathUtils.RotatePoint(this.AvoidDir, Rand.Range(-0.15f, 0.15f, Rand.RandSync.Unsynced));
					this.lastRayCastTime += 0.5f;
				}
			}
			if (this.AvoidDir.LengthSquared() < 0.0001f)
			{
				return Vector2.Zero;
			}
			if (!this.avoidRayCastHit)
			{
				this.AvoidDir -= Vector2.Normalize(this.AvoidDir) * deltaTime * 0.5f;
			}
			Vector2 diff = this.AvoidRayCastHitPosition - this.host.SimPosition;
			float dist = diff.Length();
			float dot = MathHelper.Clamp(Vector2.Dot(diff / dist, this.host.Steering), 0f, 1f);
			if (dot < 0f)
			{
				return Vector2.Zero;
			}
			return this.AvoidDir * dot * weight * MathHelper.Clamp(1f - dist / lookAheadDistance, 0f, 1f);
		}

		// Token: 0x040018E9 RID: 6377
		protected const float CircleDistance = 2.5f;

		// Token: 0x040018EA RID: 6378
		protected const float CircleRadius = 0.3f;

		// Token: 0x040018EB RID: 6379
		protected const float RayCastInterval = 0.5f;

		// Token: 0x040018EC RID: 6380
		protected ISteerable host;

		// Token: 0x040018ED RID: 6381
		protected Vector2 steering;

		// Token: 0x040018EE RID: 6382
		private float lastRayCastTime;

		// Token: 0x040018EF RID: 6383
		private bool avoidRayCastHit;

		// Token: 0x040018F3 RID: 6387
		private float wanderAngle;
	}
}
