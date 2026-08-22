using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x02000238 RID: 568
	internal class DestructibleLevelWall : LevelWall, IDamageable
	{
		// Token: 0x17000B2A RID: 2858
		// (get) Token: 0x060026D2 RID: 9938 RVA: 0x000FEF04 File Offset: 0x000FD104
		// (set) Token: 0x060026D3 RID: 9939 RVA: 0x000FEF0C File Offset: 0x000FD10C
		public float Damage { get; private set; }

		// Token: 0x17000B2B RID: 2859
		// (get) Token: 0x060026D4 RID: 9940 RVA: 0x000FEF15 File Offset: 0x000FD115
		// (set) Token: 0x060026D5 RID: 9941 RVA: 0x000FEF1D File Offset: 0x000FD11D
		public float MaxHealth { get; private set; } = 1000f;

		// Token: 0x17000B2C RID: 2860
		// (get) Token: 0x060026D6 RID: 9942 RVA: 0x000FEF26 File Offset: 0x000FD126
		// (set) Token: 0x060026D7 RID: 9943 RVA: 0x000FEF2E File Offset: 0x000FD12E
		public bool Destroyed { get; private set; }

		// Token: 0x17000B2D RID: 2861
		// (get) Token: 0x060026D8 RID: 9944 RVA: 0x000FEF37 File Offset: 0x000FD137
		// (set) Token: 0x060026D9 RID: 9945 RVA: 0x000FEF3F File Offset: 0x000FD13F
		public float FadeOutDuration { get; private set; }

		// Token: 0x17000B2E RID: 2862
		// (get) Token: 0x060026DA RID: 9946 RVA: 0x000FEF48 File Offset: 0x000FD148
		// (set) Token: 0x060026DB RID: 9947 RVA: 0x000FEF50 File Offset: 0x000FD150
		public float FadeOutTimer { get; private set; }

		// Token: 0x17000B2F RID: 2863
		// (get) Token: 0x060026DC RID: 9948 RVA: 0x000FEF59 File Offset: 0x000FD159
		public Vector2 SimPosition
		{
			get
			{
				return base.Body.Position;
			}
		}

		// Token: 0x17000B30 RID: 2864
		// (get) Token: 0x060026DD RID: 9949 RVA: 0x000FEF66 File Offset: 0x000FD166
		public Vector2 WorldPosition
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(base.Body.Position);
			}
		}

		// Token: 0x17000B31 RID: 2865
		// (get) Token: 0x060026DE RID: 9950 RVA: 0x000FEF78 File Offset: 0x000FD178
		public float Health
		{
			get
			{
				return this.MaxHealth - this.Damage;
			}
		}

		// Token: 0x060026DF RID: 9951 RVA: 0x000FEF88 File Offset: 0x000FD188
		public DestructibleLevelWall(List<Vector2> vertices, Color color, Level level, float? health = null, bool giftWrap = false) : base(vertices, color, level, giftWrap, true)
		{
			this.MaxHealth = (health ?? MathHelper.Clamp(base.Body.Mass * 0.5f, 50f, 1000f));
			base.Cells.ForEach(delegate(VoronoiCell c)
			{
				c.IsDestructible = true;
			});
		}

		// Token: 0x060026E0 RID: 9952 RVA: 0x000FF014 File Offset: 0x000FD214
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

		// Token: 0x060026E1 RID: 9953 RVA: 0x000FF06C File Offset: 0x000FD26C
		public void AddDamage(float damage, Vector2 worldPosition)
		{
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

		// Token: 0x060026E2 RID: 9954 RVA: 0x000FF0D7 File Offset: 0x000FD2D7
		public AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = true)
		{
			this.AddDamage(attack.LevelWallDamage, worldPosition);
			return new AttackResult(attack.StructureDamage, null);
		}

		// Token: 0x060026E3 RID: 9955 RVA: 0x000FF0F4 File Offset: 0x000FD2F4
		private void CreateFragments()
		{
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
			}
		}

		// Token: 0x060026E4 RID: 9956 RVA: 0x000FF5A0 File Offset: 0x000FD7A0
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

		// Token: 0x040012FF RID: 4863
		public bool NetworkUpdatePending;
	}
}
