using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000D4 RID: 212
	internal class BackgroundCreatureManager
	{
		// Token: 0x1700074A RID: 1866
		// (get) Token: 0x06001C44 RID: 7236 RVA: 0x0011A418 File Offset: 0x00118618
		public IEnumerable<BackgroundCreature> VisibleCreatures
		{
			get
			{
				return this.visibleCreatures;
			}
		}

		// Token: 0x06001C45 RID: 7237 RVA: 0x0011A420 File Offset: 0x00118620
		public void SpawnCreatures(Level level, int count, Vector2? position = null)
		{
			this.creatures.Clear();
			List<BackgroundCreaturePrefab> availablePrefabs = new List<BackgroundCreaturePrefab>(from p in BackgroundCreaturePrefab.Prefabs
			orderby p.Identifier.Value
			select p);
			if (availablePrefabs.Count == 0)
			{
				return;
			}
			count = Math.Min(count, 100);
			Func<BackgroundCreaturePrefab, float> <>9__1;
			for (int i = 0; i < count; i++)
			{
				BackgroundCreatureManager.<>c__DisplayClass7_1 CS$<>8__locals2 = new BackgroundCreatureManager.<>c__DisplayClass7_1();
				Vector2 pos = Vector2.Zero;
				if (position == null)
				{
					List<WayPoint> wayPoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.Submarine == null);
					if (wayPoints.Any<WayPoint>())
					{
						WayPoint wp2 = wayPoints[Rand.Int(wayPoints.Count, Rand.RandSync.ClientOnly)];
						pos = new Vector2((float)wp2.Rect.X, (float)wp2.Rect.Y);
						pos += Rand.Vector(200f, Rand.RandSync.ClientOnly);
					}
					else
					{
						pos = Rand.Vector(2000f, Rand.RandSync.ClientOnly);
					}
				}
				else
				{
					pos = position.Value;
				}
				BackgroundCreatureManager.<>c__DisplayClass7_1 CS$<>8__locals3 = CS$<>8__locals2;
				IList<BackgroundCreaturePrefab> objects = availablePrefabs;
				IEnumerable<BackgroundCreaturePrefab> source = availablePrefabs;
				Func<BackgroundCreaturePrefab, float> selector;
				if ((selector = <>9__1) == null)
				{
					selector = (<>9__1 = delegate(BackgroundCreaturePrefab p)
					{
						Level level2 = level;
						return p.GetCommonness((level2 != null) ? level2.LevelData : null);
					});
				}
				CS$<>8__locals3.prefab = ToolBox.SelectWeightedRandom<BackgroundCreaturePrefab>(objects, source.Select(selector).ToList<float>(), Rand.RandSync.ClientOnly);
				if (CS$<>8__locals2.prefab == null)
				{
					break;
				}
				int amount = Rand.Range(CS$<>8__locals2.prefab.SwarmMin, CS$<>8__locals2.prefab.SwarmMax + 1, Rand.RandSync.ClientOnly);
				List<BackgroundCreature> swarmMembers = new List<BackgroundCreature>();
				for (int j = 0; j < amount; j++)
				{
					BackgroundCreature creature = new BackgroundCreature(CS$<>8__locals2.prefab, pos + Rand.Vector(Rand.Range(0f, CS$<>8__locals2.prefab.SwarmRadius, Rand.RandSync.ClientOnly), Rand.RandSync.ClientOnly));
					this.creatures.Add(creature);
					swarmMembers.Add(creature);
				}
				if (amount > 1)
				{
					new Swarm(swarmMembers, CS$<>8__locals2.prefab.SwarmRadius, CS$<>8__locals2.prefab.SwarmCohesion);
				}
				if (this.creatures.Count((BackgroundCreature c) => c.Prefab == CS$<>8__locals2.prefab) > CS$<>8__locals2.prefab.MaxCount)
				{
					availablePrefabs.Remove(CS$<>8__locals2.prefab);
					if (availablePrefabs.Count <= 0)
					{
						break;
					}
				}
			}
		}

		// Token: 0x06001C46 RID: 7238 RVA: 0x0011A66A File Offset: 0x0011886A
		public void Clear()
		{
			this.creatures.Clear();
		}

		// Token: 0x06001C47 RID: 7239 RVA: 0x0011A678 File Offset: 0x00118878
		public void Update(float deltaTime, Camera cam)
		{
			if (this.checkVisibleTimer < 0f)
			{
				this.visibleCreatures.Clear();
				int margin = 500;
				foreach (BackgroundCreature creature in this.creatures)
				{
					Rectangle extents = creature.GetExtents(cam);
					creature.Visible = (extents.Right >= cam.WorldView.X - margin && extents.X <= cam.WorldView.Right + margin && extents.Bottom >= cam.WorldView.Y - cam.WorldView.Height - margin && extents.Y <= cam.WorldView.Y + margin);
					if (creature.Visible)
					{
						int i = 0;
						while (i < this.visibleCreatures.Count && this.visibleCreatures[i].Depth >= creature.Depth)
						{
							i++;
						}
						this.visibleCreatures.Insert(i, creature);
					}
				}
				this.checkVisibleTimer = 1f;
			}
			else
			{
				this.checkVisibleTimer -= deltaTime;
			}
			foreach (BackgroundCreature creature2 in this.visibleCreatures)
			{
				creature2.Update(deltaTime);
			}
		}

		// Token: 0x06001C48 RID: 7240 RVA: 0x0011A810 File Offset: 0x00118A10
		public void DrawLights(SpriteBatch spriteBatch, Camera cam)
		{
			foreach (BackgroundCreature creature in this.visibleCreatures)
			{
				creature.DrawLightSprite(spriteBatch, cam);
			}
		}

		// Token: 0x04000E8B RID: 3723
		private const int MaxCreatures = 100;

		// Token: 0x04000E8C RID: 3724
		private const float VisibilityCheckInterval = 1f;

		// Token: 0x04000E8D RID: 3725
		private float checkVisibleTimer;

		// Token: 0x04000E8E RID: 3726
		private readonly List<BackgroundCreature> creatures = new List<BackgroundCreature>();

		// Token: 0x04000E8F RID: 3727
		private readonly List<BackgroundCreature> visibleCreatures = new List<BackgroundCreature>();
	}
}
