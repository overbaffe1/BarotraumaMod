using System;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Particles
{
	// Token: 0x02000450 RID: 1104
	[NullableContext(1)]
	[Nullable(0)]
	internal class ParticleEmitter
	{
		// Token: 0x06004A10 RID: 18960 RVA: 0x0028E1DB File Offset: 0x0028C3DB
		public ParticleEmitter(ContentXElement element)
		{
			this.Prefab = new ParticleEmitterPrefab(element);
		}

		// Token: 0x06004A11 RID: 18961 RVA: 0x0028E1EF File Offset: 0x0028C3EF
		public ParticleEmitter(ParticleEmitterPrefab prefab)
		{
			this.Prefab = prefab;
		}

		// Token: 0x06004A12 RID: 18962 RVA: 0x0028E200 File Offset: 0x0028C400
		[NullableContext(2)]
		public void Emit(float deltaTime, Vector2 position, Hull hullGuess = null, float angle = 0f, float particleRotation = 0f, float velocityMultiplier = 1f, float sizeMultiplier = 1f, float amountMultiplier = 1f, Color? colorMultiplier = null, ParticlePrefab overrideParticle = null, bool mirrorAngle = false, Tuple<Vector2, Vector2> tracerPoints = null)
		{
			GameClient client = GameMain.Client;
			if (client != null && client.MidRoundSyncing)
			{
				return;
			}
			if (this.initialDelay < this.Prefab.Properties.InitialDelay)
			{
				this.initialDelay += deltaTime;
				return;
			}
			this.emitTimer += deltaTime * amountMultiplier;
			this.burstEmitTimer -= deltaTime;
			if (this.Prefab.Properties.EmitAcrossRayInterval > 0f && tracerPoints != null)
			{
				Vector2 dir = tracerPoints.Item2 - tracerPoints.Item1;
				if (dir.LengthSquared() > 0.001f)
				{
					float dist = dir.Length();
					dir /= dist;
					for (float z = 0f; z < dist; z += this.Prefab.Properties.EmitAcrossRayInterval)
					{
						Vector2 pos = tracerPoints.Item1 + dir * z;
						this.Emit(pos, hullGuess, angle, particleRotation, velocityMultiplier, sizeMultiplier, colorMultiplier, overrideParticle, mirrorAngle, null);
					}
				}
			}
			if (this.Prefab.Properties.ParticlesPerSecond > 0f)
			{
				float emitInterval = 1f / this.Prefab.Properties.ParticlesPerSecond;
				while (this.emitTimer > emitInterval)
				{
					this.Emit(position, hullGuess, angle, particleRotation, velocityMultiplier, sizeMultiplier, colorMultiplier, overrideParticle, mirrorAngle, tracerPoints);
					this.emitTimer -= emitInterval;
				}
			}
			if (this.burstEmitTimer > 0f)
			{
				return;
			}
			this.burstEmitTimer = this.Prefab.Properties.EmitInterval;
			int i = 0;
			while ((float)i < (float)this.Prefab.Properties.ParticleAmount * amountMultiplier)
			{
				this.Emit(position, hullGuess, angle, particleRotation, velocityMultiplier, sizeMultiplier, colorMultiplier, overrideParticle, mirrorAngle, tracerPoints);
				i++;
			}
		}

		// Token: 0x06004A13 RID: 18963 RVA: 0x0028E3C4 File Offset: 0x0028C5C4
		[NullableContext(2)]
		private void Emit(Vector2 position, Hull hullGuess, float angle, float particleRotation, float velocityMultiplier, float sizeMultiplier, Color? colorMultiplier = null, ParticlePrefab overrideParticle = null, bool mirrorAngle = false, Tuple<Vector2, Vector2> tracerPoints = null)
		{
			ParticlePrefab particlePrefab = overrideParticle ?? this.Prefab.ParticlePrefab;
			if (particlePrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find the particle prefab \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.ParticlePrefabName);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.Prefab.ContentPackage);
				return;
			}
			Vector2 velocity = Vector2.Zero;
			if (!MathUtils.NearlyEqual(this.Prefab.Properties.VelocityMax * velocityMultiplier, 0f, 0.0001f) || !MathUtils.NearlyEqual(this.Prefab.Properties.DistanceMax, 0f, 0.0001f))
			{
				angle += Rand.Range(this.Prefab.Properties.AngleMinRad, this.Prefab.Properties.AngleMaxRad, Rand.RandSync.Unsynced) * (float)(mirrorAngle ? -1 : 1);
				Vector2 dir = new Vector2((float)Math.Cos((double)angle), (float)Math.Sin((double)angle));
				velocity = dir * Rand.Range(this.Prefab.Properties.VelocityMin, this.Prefab.Properties.VelocityMax, Rand.RandSync.Unsynced) * velocityMultiplier;
				position += dir * Rand.Range(this.Prefab.Properties.DistanceMin, this.Prefab.Properties.DistanceMax, Rand.RandSync.Unsynced);
			}
			Particle particle = GameMain.ParticleManager.CreateParticle(particlePrefab, position, velocity, particleRotation, hullGuess, (particlePrefab.DrawOrder != ParticleDrawOrder.Default) ? particlePrefab.DrawOrder : this.Prefab.DrawOrder, 0f, this.Prefab.Properties.LifeTimeMultiplier, this.Prefab.Properties.UseTracerPoints ? tracerPoints : null);
			if (particle != null)
			{
				particle.Size *= Rand.Range(this.Prefab.Properties.ScaleMin, this.Prefab.Properties.ScaleMax, Rand.RandSync.Unsynced) * sizeMultiplier;
				particle.Size *= this.Prefab.Properties.ScaleMultiplier;
				particle.HighQualityCollisionDetection = this.Prefab.Properties.HighQualityCollisionDetection;
				if (colorMultiplier != null)
				{
					particle.ColorMultiplier = colorMultiplier.Value.ToVector4();
					return;
				}
				if (this.Prefab.Properties.ColorMultiplier != Color.White)
				{
					particle.ColorMultiplier = this.Prefab.Properties.ColorMultiplier.ToVector4();
				}
			}
		}

		// Token: 0x06004A14 RID: 18964 RVA: 0x0028E660 File Offset: 0x0028C860
		public Rectangle CalculateParticleBounds(Vector2 startPosition)
		{
			Rectangle bounds = new Rectangle((int)startPosition.X, (int)startPosition.Y, (int)startPosition.X, (int)startPosition.Y);
			if (this.Prefab.ParticlePrefab == null)
			{
				return bounds;
			}
			for (float angle = this.Prefab.Properties.AngleMinRad; angle <= this.Prefab.Properties.AngleMaxRad; angle += 0.1f)
			{
				Vector2 velocity = new Vector2((float)Math.Cos((double)angle), (float)Math.Sin((double)angle)) * this.Prefab.Properties.VelocityMax;
				Vector2 endPosition = this.Prefab.ParticlePrefab.CalculateEndPosition(startPosition, velocity);
				Vector2 endSize = this.Prefab.ParticlePrefab.CalculateEndSize();
				float spriteExtent = 0f;
				foreach (Sprite sprite in this.Prefab.ParticlePrefab.Sprites)
				{
					SpriteSheet spriteSheet = sprite as SpriteSheet;
					if (spriteSheet != null)
					{
						spriteExtent = Math.Max(spriteExtent, Math.Max((float)spriteSheet.FrameSize.X * endSize.X, (float)spriteSheet.FrameSize.Y * endSize.Y));
					}
					else
					{
						spriteExtent = Math.Max(spriteExtent, Math.Max(sprite.size.X * endSize.X, sprite.size.Y * endSize.Y));
					}
				}
				bounds = new Rectangle((int)Math.Min((float)bounds.X, endPosition.X - this.Prefab.Properties.DistanceMax - spriteExtent / 2f), (int)Math.Min((float)bounds.Y, endPosition.Y - this.Prefab.Properties.DistanceMax - spriteExtent / 2f), (int)Math.Max((float)bounds.X, endPosition.X + this.Prefab.Properties.DistanceMax + spriteExtent / 2f), (int)Math.Max((float)bounds.Y, endPosition.Y + this.Prefab.Properties.DistanceMax + spriteExtent / 2f));
			}
			bounds = new Rectangle(bounds.X, bounds.Y, bounds.Width - bounds.X, bounds.Height - bounds.Y);
			return bounds;
		}

		// Token: 0x040026B5 RID: 9909
		private float emitTimer;

		// Token: 0x040026B6 RID: 9910
		private float burstEmitTimer;

		// Token: 0x040026B7 RID: 9911
		private float initialDelay;

		// Token: 0x040026B8 RID: 9912
		public readonly ParticleEmitterPrefab Prefab;
	}
}
