using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200009B RID: 155
	internal class SteeringManager
	{
		// Token: 0x1700053A RID: 1338
		// (get) Token: 0x060012C6 RID: 4806 RVA: 0x000A5352 File Offset: 0x000A3552
		// (set) Token: 0x060012C7 RID: 4807 RVA: 0x000A535A File Offset: 0x000A355A
		public Vector2 AvoidDir { get; private set; }

		// Token: 0x1700053B RID: 1339
		// (get) Token: 0x060012C8 RID: 4808 RVA: 0x000A5363 File Offset: 0x000A3563
		// (set) Token: 0x060012C9 RID: 4809 RVA: 0x000A536B File Offset: 0x000A356B
		public Vector2 AvoidRayCastHitPosition { get; private set; }

		// Token: 0x1700053C RID: 1340
		// (get) Token: 0x060012CA RID: 4810 RVA: 0x000A5374 File Offset: 0x000A3574
		// (set) Token: 0x060012CB RID: 4811 RVA: 0x000A537C File Offset: 0x000A357C
		public Vector2 AvoidLookAheadPos { get; private set; }

		// Token: 0x1700053D RID: 1341
		// (get) Token: 0x060012CC RID: 4812 RVA: 0x000A5385 File Offset: 0x000A3585
		// (set) Token: 0x060012CD RID: 4813 RVA: 0x000A538D File Offset: 0x000A358D
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

		// Token: 0x060012CE RID: 4814 RVA: 0x000A5396 File Offset: 0x000A3596
		public SteeringManager(ISteerable host)
		{
			this.host = host;
			this.wanderAngle = Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced);
		}

		// Token: 0x060012CF RID: 4815 RVA: 0x000A53BB File Offset: 0x000A35BB
		public void SteeringSeek(Vector2 targetSimPos, float weight = 1f)
		{
			this.steering += this.DoSteeringSeek(targetSimPos, weight);
		}

		// Token: 0x060012D0 RID: 4816 RVA: 0x000A53D6 File Offset: 0x000A35D6
		public void SteeringWander(float weight = 1f, bool avoidWanderingOutsideLevel = false)
		{
			this.steering += this.DoSteeringWander(weight, avoidWanderingOutsideLevel);
		}

		// Token: 0x060012D1 RID: 4817 RVA: 0x000A53F4 File Offset: 0x000A35F4
		public void SteeringAvoid(float deltaTime, float lookAheadDistance, float weight = 1f)
		{
			this.steering += this.DoSteeringAvoid(deltaTime, lookAheadDistance, weight, null);
		}

		// Token: 0x060012D2 RID: 4818 RVA: 0x000A5424 File Offset: 0x000A3624
		public void SteeringManual(float deltaTime, Vector2 velocity)
		{
			if (MathUtils.IsValid(velocity))
			{
				this.steering += velocity;
			}
		}

		// Token: 0x060012D3 RID: 4819 RVA: 0x000A5440 File Offset: 0x000A3640
		public void Reset()
		{
			this.steering = Vector2.Zero;
		}

		// Token: 0x060012D4 RID: 4820 RVA: 0x000A544D File Offset: 0x000A364D
		public void ResetX()
		{
			this.steering.X = 0f;
		}

		// Token: 0x060012D5 RID: 4821 RVA: 0x000A545F File Offset: 0x000A365F
		public void ResetY()
		{
			this.steering.Y = 0f;
		}

		// Token: 0x060012D6 RID: 4822 RVA: 0x000A5474 File Offset: 0x000A3674
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

		// Token: 0x060012D7 RID: 4823 RVA: 0x000A553C File Offset: 0x000A373C
		protected virtual Vector2 DoSteeringSeek(Vector2 target, float weight)
		{
			Vector2 targetVel = target - this.host.SimPosition;
			if (targetVel.LengthSquared() < 1E-05f)
			{
				return Vector2.Zero;
			}
			return Vector2.Normalize(targetVel) * weight;
		}

		// Token: 0x060012D8 RID: 4824 RVA: 0x000A5580 File Offset: 0x000A3780
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

		// Token: 0x060012D9 RID: 4825 RVA: 0x000A5704 File Offset: 0x000A3904
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

		// Token: 0x040008EF RID: 2287
		protected const float CircleDistance = 2.5f;

		// Token: 0x040008F0 RID: 2288
		protected const float CircleRadius = 0.3f;

		// Token: 0x040008F1 RID: 2289
		protected const float RayCastInterval = 0.5f;

		// Token: 0x040008F2 RID: 2290
		protected ISteerable host;

		// Token: 0x040008F3 RID: 2291
		protected Vector2 steering;

		// Token: 0x040008F4 RID: 2292
		private float lastRayCastTime;

		// Token: 0x040008F5 RID: 2293
		private bool avoidRayCastHit;

		// Token: 0x040008F9 RID: 2297
		private float wanderAngle;
	}
}
