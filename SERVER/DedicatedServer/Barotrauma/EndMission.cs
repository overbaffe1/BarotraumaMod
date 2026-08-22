using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000024 RID: 36
	internal class EndMission : Mission
	{
		// Token: 0x06000486 RID: 1158 RVA: 0x00027B84 File Offset: 0x00025D84
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			this.boss.WriteSpawnData(msg, this.boss.ID, false);
			msg.WriteByte((byte)this.minions.Length);
			foreach (Character minion in this.minions)
			{
				minion.WriteSpawnData(msg, minion.ID, false);
			}
		}

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000487 RID: 1159 RVA: 0x00027BF0 File Offset: 0x00025DF0
		[TupleElementNames(new string[]
		{
			"Label",
			"Position"
		})]
		public override IEnumerable<ValueTuple<LocalizedString, Vector2>> SonarLabels
		{
			[return: TupleElementNames(new string[]
			{
				"Label",
				"Position"
			})]
			get
			{
				return from it in this.destructibleItems
				where it.Condition > 0f
				select new ValueTuple<LocalizedString, Vector2>(this.Prefab.SonarLabel, it.WorldPosition);
			}
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x06000488 RID: 1160 RVA: 0x00027C2D File Offset: 0x00025E2D
		// (set) Token: 0x06000489 RID: 1161 RVA: 0x00027C35 File Offset: 0x00025E35
		public override int State
		{
			get
			{
				return base.State;
			}
			set
			{
				if (this.state != value)
				{
					base.State = value;
					if (this.Phase == EndMission.MissionPhase.AllItemsDestroyed)
					{
						CoroutineManager.Invoke(delegate
						{
							if (this.boss != null && !this.boss.Removed)
							{
								Vector2 prevPos = this.boss.AnimController.Collider.SimPosition;
								this.boss.AnimController.ColliderIndex = 1;
								if (this.bossSpawnPoint != null)
								{
									this.boss.AnimController.Collider.SetTransform(prevPos, 0f, true);
								}
							}
						}, this.wakeUpCinematicDelay + this.bossWakeUpDelay + 2f);
					}
				}
			}
		}

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x0600048A RID: 1162 RVA: 0x00027C78 File Offset: 0x00025E78
		private EndMission.MissionPhase Phase
		{
			get
			{
				if (this.state == 0)
				{
					return EndMission.MissionPhase.Initial;
				}
				if (this.state == 1)
				{
					return EndMission.MissionPhase.NoItemsDestroyed;
				}
				if (this.state < this.destructibleItems.Count + 1)
				{
					return EndMission.MissionPhase.SomeItemsDestroyed;
				}
				if (this.state < this.destructibleItems.Count + 2)
				{
					return EndMission.MissionPhase.AllItemsDestroyed;
				}
				return EndMission.MissionPhase.BossKilled;
			}
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x00027CCC File Offset: 0x00025ECC
		public EndMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			Identifier speciesName = prefab.ConfigElement.GetAttributeIdentifier("bossfile", Identifier.Empty);
			if (!speciesName.IsEmpty)
			{
				this.bossPrefab = CharacterPrefab.FindBySpeciesName(speciesName);
				if (this.bossPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in end mission \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\". Could not find a character prefab with the name \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				}
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(46, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in end mission \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\". Monster file not set.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
			}
			Identifier minionName = prefab.ConfigElement.GetAttributeIdentifier("minionfile", Identifier.Empty);
			if (!minionName.IsEmpty)
			{
				this.minionPrefab = CharacterPrefab.FindBySpeciesName(minionName);
				if (this.minionPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(76, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("Error in end mission \"");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(prefab.Identifier);
					defaultInterpolatedStringHandler3.AppendLiteral("\". Could not find a character prefab with the name \"");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(speciesName);
					defaultInterpolatedStringHandler3.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				}
			}
			this.minionCount = Math.Min(prefab.ConfigElement.GetAttributeInt("minionCount", 0), 255);
			this.minionScatter = Math.Min(prefab.ConfigElement.GetAttributeFloat("minionScatter", 0f), 10000f);
			Identifier projectileId = prefab.ConfigElement.GetAttributeIdentifier("projectile", Identifier.Empty);
			if (!projectileId.IsEmpty)
			{
				this.projectilePrefab = (MapEntityPrefab.FindByIdentifier(projectileId) as ItemPrefab);
				if (this.projectilePrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(72, 2);
					defaultInterpolatedStringHandler4.AppendLiteral("Error in end mission \"");
					defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(prefab.Identifier);
					defaultInterpolatedStringHandler4.AppendLiteral("\". Could not find an item prefab with the name \"");
					defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(projectileId);
					defaultInterpolatedStringHandler4.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				}
			}
			this.spawnPointTag = prefab.ConfigElement.GetAttributeIdentifier("spawnPointTag", Identifier.Empty);
			this.destructibleItemTag = prefab.ConfigElement.GetAttributeIdentifier("destructibleItemTag", Identifier.Empty);
			this.endCinematicSound = prefab.ConfigElement.GetAttributeString("endCinematicSound", string.Empty);
			this.startCinematicDistance = prefab.ConfigElement.GetAttributeFloat("startCinematicDistance", 0f);
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x00027FDC File Offset: 0x000261DC
		protected override void StartMissionSpecific(Level level)
		{
			this.bossSpawnPoint = WayPoint.WayPointList.FirstOrDefault((WayPoint wp) => wp.Tags.Contains(this.spawnPointTag));
			if (this.bossSpawnPoint == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in end mission \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". Could not find a spawn point \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.spawnPointTag);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return;
			}
			if (!Mission.IsClient)
			{
				this.boss = Character.Create(this.bossPrefab.Identifier, this.bossSpawnPoint.WorldPosition, ToolBox.RandomSeed(8), null, 0, false, true, false, null, true, true);
				List<Character> minionList = new List<Character>();
				float angle = 0f;
				float angleStep = 6.2831855f / (float)Math.Max(this.minionCount, 1);
				for (int i = 0; i < this.minionCount; i++)
				{
					minionList.Add(Character.Create(this.minionPrefab.Identifier, MathUtils.GetPointOnCircumference(this.bossSpawnPoint.WorldPosition, this.minionScatter, angle), ToolBox.RandomSeed(8), null, 0, false, true, false, null, true, true));
					angle += angleStep;
				}
				SwarmBehavior.CreateSwarm(minionList.Cast<AICharacter>());
				this.minions = minionList.ToImmutableArray<Character>();
			}
			if (this.destructibleItemTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(55, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in end mission \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\". Destructible item tag not set.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return;
			}
			this.destructibleItems.Clear();
			this.destructibleItems.AddRange(Item.ItemList.FindAll((Item it) => it.HasTag(this.destructibleItemTag)));
			if (this.destructibleItems.None(null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(79, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("Error in end mission \"");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler3.AppendLiteral("\". Could not find any destructible items with the tag \"");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.spawnPointTag);
				defaultInterpolatedStringHandler3.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return;
			}
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x00028238 File Offset: 0x00026438
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			if (this.state == 0)
			{
				if (this.startCinematicDistance <= 0f || this.boss == null || Submarine.MainSub == null || Vector2.DistanceSquared(Submarine.MainSub.WorldPosition, this.boss.WorldPosition) <= this.startCinematicDistance * this.startCinematicDistance)
				{
					this.State = 1;
				}
				return;
			}
			if (!Mission.IsClient && this.State > 0)
			{
				this.State = Math.Max(this.State, this.destructibleItems.Count((Item it) => it.Condition <= 0f) + 1);
			}
			if (this.Phase == EndMission.MissionPhase.AllItemsDestroyed)
			{
				if (this.projectilePrefab == null || this.boss == null || this.boss.IsDead || this.boss.Removed)
				{
					this.State = Math.Max(this.destructibleItems.Count + 2, this.State);
					return;
				}
				this.projectileTimer -= deltaTime;
				if (this.projectileTimer <= 0f)
				{
					float dist = Vector2.Distance(Submarine.MainSub.WorldPosition, this.boss.WorldPosition);
					float distanceFactor = Math.Min(dist / 10000f, 1f);
					int projectileAmount = Rand.Range(3, 6, Rand.RandSync.Unsynced);
					float spread = MathHelper.ToRadians(Rand.Range(20f, 180f, Rand.RandSync.Unsynced)) * Math.Max(1f - distanceFactor, 0.2f);
					for (int i = 0; i < projectileAmount; i++)
					{
						int index = i;
						Entity.Spawner.AddItemToSpawnQueue(this.projectilePrefab, this.boss.WorldPosition, null, null, delegate(Item it)
						{
							Projectile projectile = it.GetComponent<Projectile>();
							float angle = MathUtils.VectorToAngle(Submarine.MainSub.WorldPosition - this.boss.WorldPosition);
							if (projectileAmount > 1)
							{
								angle += ((float)index / (float)(projectileAmount - 1) - 0.5f) * spread;
							}
							it.body.SetTransform(it.SimPosition, angle, true);
							it.UpdateTransform();
							projectile.Use(null, MathHelper.Lerp(0f, 5f, distanceFactor));
						});
					}
					float shortIntervalProbability = MathHelper.Lerp(0.9f, 0.05f, distanceFactor);
					if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < shortIntervalProbability)
					{
						this.projectileTimer = Rand.Range(3f, 5f, Rand.RandSync.Unsynced);
						return;
					}
					this.projectileTimer = Rand.Range(15f, 30f, Rand.RandSync.Unsynced);
					return;
				}
			}
			else if (this.Phase == EndMission.MissionPhase.BossKilled)
			{
				this.endCinematicTimer += deltaTime;
				if (this.endCinematicTimer > 5f && !Mission.IsClient)
				{
					foreach (Character c in Character.CharacterList)
					{
						EnemyAIController enemyAI = c.AIController as EnemyAIController;
						if (enemyAI != null && enemyAI.PetBehavior == null)
						{
							c.SetAllDamage(200f, 0f, 0f);
						}
					}
				}
				if (this.endCinematicTimer > 20f && !Mission.IsClient)
				{
					CampaignMode campaign = GameMain.GameSession.Campaign;
					if (campaign == null)
					{
						return;
					}
					campaign.LoadNewLevel();
				}
			}
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x00028568 File Offset: 0x00026768
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.Phase == EndMission.MissionPhase.BossKilled;
		}

		// Token: 0x0400023D RID: 573
		private readonly CharacterPrefab bossPrefab;

		// Token: 0x0400023E RID: 574
		private readonly CharacterPrefab minionPrefab;

		// Token: 0x0400023F RID: 575
		private readonly Identifier spawnPointTag;

		// Token: 0x04000240 RID: 576
		private WayPoint bossSpawnPoint;

		// Token: 0x04000241 RID: 577
		private readonly Identifier destructibleItemTag;

		// Token: 0x04000242 RID: 578
		private readonly string endCinematicSound;

		// Token: 0x04000243 RID: 579
		private ImmutableArray<Character> minions;

		// Token: 0x04000244 RID: 580
		private readonly int minionCount;

		// Token: 0x04000245 RID: 581
		private readonly float minionScatter;

		// Token: 0x04000246 RID: 582
		private Character boss;

		// Token: 0x04000247 RID: 583
		private readonly ItemPrefab projectilePrefab;

		// Token: 0x04000248 RID: 584
		private float projectileTimer = 30f;

		// Token: 0x04000249 RID: 585
		private readonly float startCinematicDistance = 30f;

		// Token: 0x0400024A RID: 586
		private float endCinematicTimer;

		// Token: 0x0400024B RID: 587
		private readonly List<Item> destructibleItems = new List<Item>();

		// Token: 0x0400024C RID: 588
		protected readonly float wakeUpCinematicDelay = 5f;

		// Token: 0x0400024D RID: 589
		protected readonly float bossWakeUpDelay = 7f;

		// Token: 0x0400024E RID: 590
		protected readonly float cameraWaitDuration = 7f;

		// Token: 0x020005D8 RID: 1496
		private enum MissionPhase
		{
			// Token: 0x040027C5 RID: 10181
			Initial,
			// Token: 0x040027C6 RID: 10182
			NoItemsDestroyed,
			// Token: 0x040027C7 RID: 10183
			SomeItemsDestroyed,
			// Token: 0x040027C8 RID: 10184
			AllItemsDestroyed,
			// Token: 0x040027C9 RID: 10185
			BossKilled
		}
	}
}
