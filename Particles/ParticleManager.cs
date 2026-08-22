using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Particles
{
	// Token: 0x02000454 RID: 1108
	internal class ParticleManager
	{
		// Token: 0x170012DC RID: 4828
		// (get) Token: 0x06004A19 RID: 18969 RVA: 0x0028E9AD File Offset: 0x0028CBAD
		public int ParticleCount
		{
			get
			{
				return this.particleCount;
			}
		}

		// Token: 0x170012DD RID: 4829
		// (get) Token: 0x06004A1A RID: 18970 RVA: 0x0028E9B5 File Offset: 0x0028CBB5
		// (set) Token: 0x06004A1B RID: 18971 RVA: 0x0028E9C0 File Offset: 0x0028CBC0
		public int MaxParticles
		{
			get
			{
				return this.maxParticles;
			}
			set
			{
				if (this.maxParticles == value || value < 4)
				{
					return;
				}
				Particle[] newParticles = new Particle[value];
				for (int i = 0; i < Math.Min(this.maxParticles, value); i++)
				{
					newParticles[i] = this.particles[i];
				}
				this.particleCount = Math.Min(this.particleCount, value);
				this.particles = newParticles;
				this.maxParticles = value;
				List<Particle> oldParticlesInCreationOrder = this.particlesInCreationOrder.ToList<Particle>();
				this.particlesInCreationOrder.Clear();
				foreach (Particle particle in oldParticlesInCreationOrder)
				{
					if (this.particles.Contains(particle))
					{
						this.particlesInCreationOrder.AddLast(particle);
					}
				}
			}
		}

		// Token: 0x170012DE RID: 4830
		// (get) Token: 0x06004A1C RID: 18972 RVA: 0x0028EA94 File Offset: 0x0028CC94
		// (set) Token: 0x06004A1D RID: 18973 RVA: 0x0028EA9C File Offset: 0x0028CC9C
		public Camera Camera
		{
			get
			{
				return this.cam;
			}
			set
			{
				this.cam = value;
			}
		}

		// Token: 0x06004A1E RID: 18974 RVA: 0x0028EAA5 File Offset: 0x0028CCA5
		public ParticleManager(Camera cam)
		{
			this.cam = cam;
			this.MaxParticles = GameSettings.CurrentConfig.Graphics.ParticleLimit;
		}

		// Token: 0x06004A1F RID: 18975 RVA: 0x0028EAD4 File Offset: 0x0028CCD4
		public Particle CreateParticle(string prefabName, Vector2 position, float angle, float speed, Hull hullGuess = null, float collisionIgnoreTimer = 0f, Tuple<Vector2, Vector2> tracerPoints = null)
		{
			return this.CreateParticle(prefabName, position, new Vector2((float)Math.Cos((double)angle), (float)(-(float)Math.Sin((double)angle))) * speed, angle, hullGuess, collisionIgnoreTimer, tracerPoints);
		}

		// Token: 0x06004A20 RID: 18976 RVA: 0x0028EB04 File Offset: 0x0028CD04
		public Particle CreateParticle(string prefabName, Vector2 position, Vector2 velocity, float rotation = 0f, Hull hullGuess = null, float collisionIgnoreTimer = 0f, Tuple<Vector2, Vector2> tracerPoints = null)
		{
			ParticlePrefab prefab = ParticleManager.FindPrefab(prefabName);
			if (prefab == null)
			{
				DebugConsole.ThrowError("Particle prefab \"" + prefabName + "\" not found!", null, null, false, false);
				return null;
			}
			return this.CreateParticle(prefab, position, velocity, rotation, hullGuess, ParticleDrawOrder.Default, collisionIgnoreTimer, 1f, tracerPoints);
		}

		// Token: 0x06004A21 RID: 18977 RVA: 0x0028EB50 File Offset: 0x0028CD50
		public Particle CreateParticle(ParticlePrefab prefab, Vector2 position, Vector2 velocity, float rotation = 0f, Hull hullGuess = null, ParticleDrawOrder drawOrder = ParticleDrawOrder.Default, float collisionIgnoreTimer = 0f, float lifeTimeMultiplier = 1f, Tuple<Vector2, Vector2> tracerPoints = null)
		{
			if (prefab == null || prefab.Sprites.Count == 0)
			{
				return null;
			}
			if (this.particleCount >= this.MaxParticles)
			{
				if (this.particleCount >= this.MaxParticles && prefab.Priority == 0 && !prefab.DrawAlways)
				{
					return null;
				}
				for (int i = 0; i < this.particleCount; i++)
				{
					if (this.particles[i].Prefab.Priority < prefab.Priority || (!this.particles[i].Prefab.DrawAlways && prefab.DrawAlways))
					{
						this.RemoveParticle(i);
						break;
					}
				}
				if (this.particleCount >= this.MaxParticles)
				{
					return null;
				}
			}
			Vector2 particleEndPos = prefab.CalculateEndPosition(position, velocity);
			Vector2 minPos = new Vector2(Math.Min(position.X, particleEndPos.X), Math.Min(position.Y, particleEndPos.Y));
			Vector2 maxPos = new Vector2(Math.Max(position.X, particleEndPos.X), Math.Max(position.Y, particleEndPos.Y));
			if (tracerPoints != null)
			{
				minPos = new Vector2(Math.Min(Math.Min(minPos.X, tracerPoints.Item1.X), tracerPoints.Item2.X), Math.Min(Math.Min(minPos.Y, tracerPoints.Item1.Y), tracerPoints.Item2.Y));
				maxPos = new Vector2(Math.Max(Math.Max(maxPos.X, tracerPoints.Item1.X), tracerPoints.Item2.X), Math.Max(Math.Max(maxPos.Y, tracerPoints.Item1.Y), tracerPoints.Item2.Y));
			}
			Rectangle expandedViewRect = MathUtils.ExpandRect(this.cam.WorldView, 500);
			if (!prefab.DrawAlways)
			{
				if (minPos.X > (float)expandedViewRect.Right || maxPos.X < (float)expandedViewRect.X)
				{
					return null;
				}
				if (minPos.Y > (float)expandedViewRect.Y || maxPos.Y < (float)(expandedViewRect.Y - expandedViewRect.Height))
				{
					return null;
				}
			}
			if (this.particles[this.particleCount] == null)
			{
				this.particles[this.particleCount] = new Particle();
			}
			Particle particle = this.particles[this.particleCount];
			particle.Init(prefab, position, velocity, rotation, hullGuess, drawOrder, collisionIgnoreTimer, lifeTimeMultiplier, tracerPoints);
			this.particleCount++;
			this.particlesInCreationOrder.AddFirst(particle);
			return particle;
		}

		// Token: 0x06004A22 RID: 18978 RVA: 0x0028EDE1 File Offset: 0x0028CFE1
		public static List<ParticlePrefab> GetPrefabList()
		{
			return ParticlePrefab.Prefabs.ToList<ParticlePrefab>();
		}

		// Token: 0x06004A23 RID: 18979 RVA: 0x0028EDF0 File Offset: 0x0028CFF0
		public static ParticlePrefab FindPrefab(string prefabName)
		{
			ParticlePrefab prefab;
			ParticlePrefab.Prefabs.TryGet(prefabName, out prefab);
			return prefab;
		}

		// Token: 0x06004A24 RID: 18980 RVA: 0x0028EE0C File Offset: 0x0028D00C
		private void RemoveParticle(int index)
		{
			this.particlesInCreationOrder.Remove(this.particles[index]);
			this.particleCount--;
			Particle[] array = this.particles;
			int num = this.particleCount;
			Particle[] array2 = this.particles;
			Particle particle = this.particles[index];
			Particle particle2 = this.particles[this.particleCount];
			array[num] = particle;
			array2[index] = particle2;
		}

		// Token: 0x06004A25 RID: 18981 RVA: 0x0028EE74 File Offset: 0x0028D074
		public void RemoveParticle(Particle particle)
		{
			for (int i = 0; i < this.particleCount; i++)
			{
				if (this.particles[i] == particle)
				{
					this.RemoveParticle(i);
					return;
				}
			}
		}

		// Token: 0x06004A26 RID: 18982 RVA: 0x0028EEA8 File Offset: 0x0028D0A8
		public void Update(float deltaTime)
		{
			this.MaxParticles = GameSettings.CurrentConfig.Graphics.ParticleLimit;
			for (int i = 0; i < this.particleCount; i++)
			{
				bool remove;
				try
				{
					remove = (this.particles[i].Update(deltaTime) == Particle.UpdateResult.Delete);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Particle update failed", e, null, false, false);
					remove = true;
				}
				if (remove)
				{
					this.RemoveParticle(i);
				}
			}
		}

		// Token: 0x06004A27 RID: 18983 RVA: 0x0028EF20 File Offset: 0x0028D120
		public void UpdateTransforms()
		{
			for (int i = 0; i < this.particleCount; i++)
			{
				this.particles[i].UpdateDrawPos();
			}
		}

		// Token: 0x06004A28 RID: 18984 RVA: 0x0028EF4C File Offset: 0x0028D14C
		public Dictionary<ParticlePrefab, int> CountActiveParticles()
		{
			Dictionary<ParticlePrefab, int> activeParticles = new Dictionary<ParticlePrefab, int>();
			for (int i = 0; i < this.particleCount; i++)
			{
				if (!activeParticles.ContainsKey(this.particles[i].Prefab))
				{
					activeParticles[this.particles[i].Prefab] = 0;
				}
				Dictionary<ParticlePrefab, int> dictionary = activeParticles;
				ParticlePrefab prefab = this.particles[i].Prefab;
				int num = dictionary[prefab];
				dictionary[prefab] = num + 1;
			}
			return activeParticles;
		}

		// Token: 0x06004A29 RID: 18985 RVA: 0x0028EFBC File Offset: 0x0028D1BC
		public void Draw(SpriteBatch spriteBatch, bool inWater, bool? inSub, ParticleBlendState blendState, bool? background = false)
		{
			ParticlePrefab.DrawTargetType drawTarget = inWater ? ParticlePrefab.DrawTargetType.Water : ParticlePrefab.DrawTargetType.Air;
			foreach (Particle particle in this.particlesInCreationOrder)
			{
				if (particle.BlendState == blendState && (particle.DrawTarget & drawTarget) != (ParticlePrefab.DrawTargetType)0)
				{
					if (inSub != null)
					{
						bool isOutside = particle.CurrentHull == null;
						if (particle.DrawOrder != ParticleDrawOrder.Foreground && isOutside == inSub.Value)
						{
							continue;
						}
					}
					if (background != null)
					{
						bool isBackgroundParticle = particle.DrawOrder == ParticleDrawOrder.Background;
						if (background.Value != isBackgroundParticle)
						{
							continue;
						}
					}
					particle.Draw(spriteBatch);
				}
			}
		}

		// Token: 0x06004A2A RID: 18986 RVA: 0x0028F070 File Offset: 0x0028D270
		public void ClearParticles()
		{
			this.particleCount = 0;
			this.particlesInCreationOrder.Clear();
		}

		// Token: 0x06004A2B RID: 18987 RVA: 0x0028F084 File Offset: 0x0028D284
		public void RemoveByPrefab(ParticlePrefab prefab)
		{
			if (this.particles == null)
			{
				return;
			}
			for (int i = this.particles.Length - 1; i >= 0; i--)
			{
				Particle particle = this.particles[i];
				if (((particle != null) ? particle.Prefab : null) == prefab)
				{
					if (i < this.particleCount)
					{
						this.particleCount--;
					}
					this.particlesInCreationOrder.Remove(this.particles[this.particleCount]);
					Particle swap = this.particles[this.particleCount];
					this.particles[this.particleCount] = null;
					this.particles[i] = swap;
				}
			}
		}

		// Token: 0x040026C3 RID: 9923
		private const int MaxOutOfViewDist = 500;

		// Token: 0x040026C4 RID: 9924
		private int particleCount;

		// Token: 0x040026C5 RID: 9925
		private int maxParticles;

		// Token: 0x040026C6 RID: 9926
		private Particle[] particles;

		// Token: 0x040026C7 RID: 9927
		private readonly LinkedList<Particle> particlesInCreationOrder = new LinkedList<Particle>();

		// Token: 0x040026C8 RID: 9928
		private Camera cam;
	}
}
