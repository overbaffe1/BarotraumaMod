using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Particles;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x020000D7 RID: 215
	internal class DestructibleLevelWall : LevelWall, IDamageable
	{
		// Token: 0x17000764 RID: 1892
		// (get) Token: 0x06001C8C RID: 7308 RVA: 0x0011CF54 File Offset: 0x0011B154
		public override float Alpha
		{
			get
			{
				if (this.FadeOutDuration <= 0f || this.FadeOutTimer < this.FadeOutDuration - 1f)
				{
					return 1f;
				}
				return MathHelper.Clamp(this.FadeOutDuration - this.FadeOutTimer, 0f, 1f);
			}
		}

		// Token: 0x06001C8D RID: 7309 RVA: 0x0011CFA4 File Offset: 0x0011B1A4
		public void SetDamage(float damage)
		{
			this.Damage = damage;
			if (this.Damage >= this.MaxHealth && !this.Destroyed)
			{
				this.CreateFragments();
				this.Destroy();
			}
		}

		// Token: 0x17000765 RID: 1893
		// (get) Token: 0x06001C8E RID: 7310 RVA: 0x0011CFCF File Offset: 0x0011B1CF
		// (set) Token: 0x06001C8F RID: 7311 RVA: 0x0011CFD7 File Offset: 0x0011B1D7
		public float Damage { get; private set; }

		// Token: 0x17000766 RID: 1894
		// (get) Token: 0x06001C90 RID: 7312 RVA: 0x0011CFE0 File Offset: 0x0011B1E0
		// (set) Token: 0x06001C91 RID: 7313 RVA: 0x0011CFE8 File Offset: 0x0011B1E8
		public float MaxHealth { get; private set; } = 1000f;

		// Token: 0x17000767 RID: 1895
		// (get) Token: 0x06001C92 RID: 7314 RVA: 0x0011CFF1 File Offset: 0x0011B1F1
		// (set) Token: 0x06001C93 RID: 7315 RVA: 0x0011CFF9 File Offset: 0x0011B1F9
		public bool Destroyed { get; private set; }

		// Token: 0x17000768 RID: 1896
		// (get) Token: 0x06001C94 RID: 7316 RVA: 0x0011D002 File Offset: 0x0011B202
		// (set) Token: 0x06001C95 RID: 7317 RVA: 0x0011D00A File Offset: 0x0011B20A
		public float FadeOutDuration { get; private set; }

		// Token: 0x17000769 RID: 1897
		// (get) Token: 0x06001C96 RID: 7318 RVA: 0x0011D013 File Offset: 0x0011B213
		// (set) Token: 0x06001C97 RID: 7319 RVA: 0x0011D01B File Offset: 0x0011B21B
		public float FadeOutTimer { get; private set; }

		// Token: 0x1700076A RID: 1898
		// (get) Token: 0x06001C98 RID: 7320 RVA: 0x0011D024 File Offset: 0x0011B224
		public Vector2 SimPosition
		{
			get
			{
				return base.Body.Position;
			}
		}

		// Token: 0x1700076B RID: 1899
		// (get) Token: 0x06001C99 RID: 7321 RVA: 0x0011D031 File Offset: 0x0011B231
		public Vector2 WorldPosition
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(base.Body.Position);
			}
		}

		// Token: 0x1700076C RID: 1900
		// (get) Token: 0x06001C9A RID: 7322 RVA: 0x0011D043 File Offset: 0x0011B243
		public float Health
		{
			get
			{
				return this.MaxHealth - this.Damage;
			}
		}

		// Token: 0x06001C9B RID: 7323 RVA: 0x0011D054 File Offset: 0x0011B254
		public DestructibleLevelWall(List<Vector2> vertices, Color color, Level level, float? health = null, bool giftWrap = false) : base(vertices, color, level, giftWrap, true)
		{
			this.MaxHealth = (health ?? MathHelper.Clamp(base.Body.Mass * 0.5f, 50f, 1000f));
			base.Cells.ForEach(delegate(VoronoiCell c)
			{
				c.IsDestructible = true;
			});
		}

		// Token: 0x06001C9C RID: 7324 RVA: 0x0011D0E0 File Offset: 0x0011B2E0
		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			if (this.FadeOutDuration > 0f)
			{
				this.FadeOutTimer += deltaTime;
				if (this.FadeOutTimer > this.FadeOutDuration && (GameMain.NetworkMember == null || GameMain.NetworkMember.IsClient))
				{
					this.Destroy();
				}
			}
		}

		// Token: 0x06001C9D RID: 7325 RVA: 0x0011D138 File Offset: 0x0011B338
		public void AddDamage(float damage, Vector2 worldPosition)
		{
			this.AddDamageProjSpecific(damage, worldPosition);
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.Destroyed)
			{
				return;
			}
			if (!MathUtils.NearlyEqual(damage, 0f, 0.0001f))
			{
				this.NetworkUpdatePending = true;
			}
			this.Damage += damage;
			if (this.Damage >= this.MaxHealth)
			{
				this.CreateFragments();
				this.Destroy();
			}
		}

		// Token: 0x06001C9E RID: 7326 RVA: 0x0011D1AC File Offset: 0x0011B3AC
		private void AddDamageProjSpecific(float damage, Vector2 worldPosition)
		{
			if (damage <= 0f)
			{
				return;
			}
			Vector2 particlePos = worldPosition;
			Vector2 particleDir = particlePos - this.WorldPosition;
			if (particleDir.LengthSquared() > 0.0001f)
			{
				particleDir = Vector2.Normalize(particleDir);
			}
			if (!base.Cells.Any((VoronoiCell c) => c.IsPointInside(particlePos)))
			{
				bool intersectionFound = false;
				foreach (VoronoiCell cell in base.Cells)
				{
					foreach (GraphEdge edge in cell.Edges)
					{
						Vector2 intersection;
						if (MathUtils.GetLineSegmentIntersection(worldPosition, cell.Center, edge.Point1 + cell.Translation, edge.Point2 + cell.Translation, out intersection))
						{
							intersectionFound = true;
							particlePos = intersection;
							particleDir = edge.GetNormal(cell);
							break;
						}
					}
					if (intersectionFound)
					{
						break;
					}
				}
			}
			int particleAmount = MathHelper.Clamp((int)damage, 1, 10);
			for (int i = 0; i < particleAmount; i++)
			{
				Particle particle = GameMain.ParticleManager.CreateParticle("iceexplosionsmall", particlePos + Rand.Vector(5f, Rand.RandSync.Unsynced), particleDir * Rand.Range(30f, 500f, Rand.RandSync.Unsynced) + Rand.Vector(20f, Rand.RandSync.Unsynced), 0f, null, 0f, null);
				GameMain.ParticleManager.CreateParticle("iceshards", particlePos + Rand.Vector(5f, Rand.RandSync.Unsynced), particleDir * Rand.Range(100f, 500f, Rand.RandSync.Unsynced) + Rand.Vector(100f, Rand.RandSync.Unsynced), 0f, null, 0f, null);
			}
		}

		// Token: 0x06001C9F RID: 7327 RVA: 0x0011D3C0 File Offset: 0x0011B5C0
		public AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = true)
		{
			this.AddDamage(attack.LevelWallDamage, worldPosition);
			return new AttackResult(attack.StructureDamage, null);
		}

		// Token: 0x06001CA0 RID: 7328 RVA: 0x0011D3DC File Offset: 0x0011B5DC
		private void CreateFragments()
		{
			SoundPlayer.PlaySound("icebreak", this.WorldPosition, null, null, null);
			Vector2 center = Vector2.Zero;
			List<List<Vector2>> triangles = new List<List<Vector2>>();
			foreach (VoronoiCell cell in base.Cells)
			{
				foreach (GraphEdge edge in cell.Edges)
				{
					List<Vector2> triangleVerts = new List<Vector2>
					{
						edge.Point1 + cell.Translation,
						edge.Point2 + cell.Translation,
						cell.Center
					};
					triangles.Add(triangleVerts);
				}
				center += cell.Center;
			}
			if (base.Cells.Any<VoronoiCell>())
			{
				center /= (float)base.Cells.Count;
			}
			Pair<int, int> longestEdge = new Pair<int, int>(-1, -1);
			do
			{
				longestEdge.First = -1;
				longestEdge.Second = -1;
				float longestEdgeLength = 0f;
				for (int i = 0; i < triangles.Count; i++)
				{
					for (int edge2 = 0; edge2 < 3; edge2++)
					{
						float edgeLength = Vector2.Distance(triangles[i][edge2], triangles[i][(edge2 + 1) % 3]);
						if (edgeLength > longestEdgeLength)
						{
							longestEdge.First = i;
							longestEdge.Second = edge2;
							longestEdgeLength = edgeLength;
						}
					}
				}
				if (longestEdgeLength < 1000f)
				{
					break;
				}
				Vector2 p0 = triangles[longestEdge.First][longestEdge.Second];
				Vector2 p = triangles[longestEdge.First][(longestEdge.Second + 1) % 3];
				Vector2 p2 = triangles[longestEdge.First][(longestEdge.Second + 2) % 3];
				triangles[longestEdge.First] = new List<Vector2>
				{
					p0,
					(p0 + p) / 2f,
					p2
				};
				triangles.Add(new List<Vector2>
				{
					(p0 + p) / 2f,
					p,
					p2
				});
			}
			while (triangles.Count < 32);
			foreach (List<Vector2> triangle in triangles)
			{
				Vector2 triangleCenter = (triangle[0] + triangle[1] + triangle[2]) / 3f;
				List<Vector2> list = triangle;
				list[0] = list[0] - triangleCenter;
				list = triangle;
				list[1] = list[1] - triangleCenter;
				list = triangle;
				list[2] = list[2] - triangleCenter;
				Vector2 simTriangleCenter = ConvertUnits.ToSimUnits(triangleCenter);
				DestructibleLevelWall fragment = new DestructibleLevelWall(triangle, Color.White, Level.Loaded, null, true);
				fragment.Damage = fragment.MaxHealth;
				fragment.Body.Position = simTriangleCenter;
				fragment.Body.BodyType = BodyType.Dynamic;
				fragment.Body.FixedRotation = false;
				fragment.Body.LinearDamping = Rand.Range(0.2f, 0.3f, Rand.RandSync.Unsynced);
				fragment.Body.AngularDamping = Rand.Range(0.1f, 0.2f, Rand.RandSync.Unsynced);
				fragment.Body.GravityScale = 0.1f;
				fragment.Body.Mass *= 10f;
				fragment.Body.CollisionCategories = Category.None;
				fragment.Body.CollidesWith = Category.Cat1;
				fragment.FadeOutDuration = 20f;
				Vector2 bodyDiff = simTriangleCenter - base.Body.Position;
				fragment.Body.LinearVelocity = (bodyDiff + Rand.Vector(0.5f, Rand.RandSync.Unsynced)).ClampLength(15f);
				fragment.Body.AngularVelocity = Rand.Range(-0.5f, 0.5f, Rand.RandSync.Unsynced);
				Level.Loaded.UnsyncedExtraWalls.Add(fragment);
				for (int j = 0; j < 5; j++)
				{
					int startEdgeIndex = Rand.Int(3, Rand.RandSync.Unsynced);
					Vector2 pos = triangle[startEdgeIndex];
					Vector2 pos2 = triangle[(startEdgeIndex + 1) % 3];
					Particle particle = GameMain.ParticleManager.CreateParticle("iceexplosion", triangleCenter + Vector2.Lerp(pos, pos2, Rand.Range(0f, 1f, Rand.RandSync.Unsynced)), Rand.Vector(Rand.Range(50f, 1000f, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced) + fragment.Body.LinearVelocity * 100f, 0f, null, 0f, null);
					if (particle != null)
					{
						particle.Size *= Rand.Range(1f, 5f, Rand.RandSync.Unsynced);
						particle.ColorMultiplier *= Rand.Range(0.7f, 1f, Rand.RandSync.Unsynced);
					}
				}
			}
		}

		// Token: 0x06001CA1 RID: 7329 RVA: 0x0011D99C File Offset: 0x0011BB9C
		public void Destroy()
		{
			if (this.Destroyed)
			{
				return;
			}
			this.Destroyed = true;
			Level level = this.level;
			if (level != null)
			{
				List<LevelWall> unsyncedExtraWalls = level.UnsyncedExtraWalls;
				if (unsyncedExtraWalls != null)
				{
					unsyncedExtraWalls.Remove(this);
				}
			}
			foreach (VoronoiCell cell in base.Cells)
			{
				cell.CellType = CellType.Removed;
				Action onDestroyed = cell.OnDestroyed;
				if (onDestroyed != null)
				{
					onDestroyed();
				}
				cell.OnDestroyed = null;
			}
			GameMain.World.Remove(base.Body);
			base.Dispose();
		}

		// Token: 0x04000EAC RID: 3756
		public bool NetworkUpdatePending;
	}
}
