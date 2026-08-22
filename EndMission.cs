using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000054 RID: 84
	internal class EndMission : Mission
	{
		// Token: 0x17000330 RID: 816
		// (get) Token: 0x06000B82 RID: 2946 RVA: 0x0006B9CB File Offset: 0x00069BCB
		public override bool DisplayAsCompleted
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000331 RID: 817
		// (get) Token: 0x06000B83 RID: 2947 RVA: 0x0006B9CE File Offset: 0x00069BCE
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000B84 RID: 2948 RVA: 0x0006B9D4 File Offset: 0x00069BD4
		public override void ClientReadInitial(IReadMessage msg)
		{
			base.ClientReadInitial(msg);
			this.boss = Character.ReadSpawnData(msg);
			byte minionCount = msg.ReadByte();
			List<Character> minionList = new List<Character>();
			for (int i = 0; i < (int)minionCount; i++)
			{
				Character minion = Character.ReadSpawnData(msg);
				if (minion == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in EndMission.ClientReadInitial: failed to create a minion (mission: ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(", index: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(i);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				minionList.Add(minion);
			}
			this.minions = minionList.ToImmutableArray<Character>();
			if (this.minions.Length != (int)minionCount)
			{
				throw new Exception(string.Concat(new string[]
				{
					"Error in EndMission.ClientReadInitial: minion count does not match the server count (",
					minionCount.ToString(),
					" != ",
					this.minions.Length.ToString(),
					"mission: ",
					this.Prefab.Identifier.ToString(),
					")"
				}));
			}
		}

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x06000B85 RID: 2949 RVA: 0x0006BAF8 File Offset: 0x00069CF8
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

		// Token: 0x17000333 RID: 819
		// (get) Token: 0x06000B86 RID: 2950 RVA: 0x0006BB35 File Offset: 0x00069D35
		// (set) Token: 0x06000B87 RID: 2951 RVA: 0x0006BB40 File Offset: 0x00069D40
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
					this.OnStateChangedProjSpecific();
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

		// Token: 0x17000334 RID: 820
		// (get) Token: 0x06000B88 RID: 2952 RVA: 0x0006BB94 File Offset: 0x00069D94
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

		// Token: 0x06000B89 RID: 2953 RVA: 0x0006BBE8 File Offset: 0x00069DE8
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

		// Token: 0x06000B8A RID: 2954 RVA: 0x0006BEF8 File Offset: 0x0006A0F8
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

		// Token: 0x06000B8B RID: 2955 RVA: 0x0006C154 File Offset: 0x0006A354
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			this.UpdateProjSpecific();
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
				Screen.Selected.Cam.Shake = MathHelper.Clamp(MathF.Pow(this.endCinematicTimer, 3f), 5f, 200f);
				Screen.Selected.Cam.Rotation = Math.Max((this.endCinematicTimer - 5f) * 0.05f, 0f) + (PerlinNoise.GetPerlin(this.endCinematicTimer * 0.1f, this.endCinematicTimer * 0.05f) - 0.5f) * 0.5f * (this.endCinematicTimer / 20f);
				if (Rand.Range(0f, 100f, Rand.RandSync.Unsynced) < this.endCinematicTimer)
				{
					Level.Loaded.Renderer.Flash();
				}
				Level.Loaded.Renderer.ChromaticAberrationStrength = this.endCinematicTimer * 5f;
				Level.Loaded.Renderer.CollapseEffectOrigin = this.boss.WorldPosition;
				Level.Loaded.Renderer.CollapseEffectStrength = this.endCinematicTimer / 20f;
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

		// Token: 0x06000B8C RID: 2956 RVA: 0x0006C590 File Offset: 0x0006A790
		private void UpdateProjSpecific()
		{
			if (this.boss == null || this.boss.Removed)
			{
				return;
			}
			EndMission.MissionPhase phase = this.Phase;
			bool flag = phase <= EndMission.MissionPhase.SomeItemsDestroyed;
			if (flag)
			{
				foreach (Limb limb in this.boss.AnimController.Limbs)
				{
					if (limb.Params.BlinkFrequency > 0f)
					{
						limb.FreezeBlinkState = true;
						limb.BlinkPhase = -limb.Params.BlinkHoldTime;
						LightSource light = limb.LightSource;
						if (light != null)
						{
							light.Enabled = false;
						}
					}
				}
			}
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x0006C630 File Offset: 0x0006A830
		private void OnStateChangedProjSpecific()
		{
			SoundPlayer.ForceMusicUpdate();
			if (this.Phase == EndMission.MissionPhase.NoItemsDestroyed)
			{
				CoroutineManager.Invoke(delegate
				{
					if (this.boss != null && !this.boss.Removed)
					{
						CameraTransition cameraTransition = new CameraTransition(this.boss, GameMain.GameScreen.Cam, null, new Alignment?(Alignment.Center), false, false, 0f, 8f, new float?(1f), new float?(0.3f * GUI.yScale));
						cameraTransition.RunWhilePaused = false;
						cameraTransition.EndWaitDuration = 3f;
					}
				}, 3f);
				return;
			}
			if (this.Phase == EndMission.MissionPhase.AllItemsDestroyed)
			{
				CoroutineManager.StartCoroutine(this.<OnStateChangedProjSpecific>g__wakeUpCoroutine|35_2(), "EndMission.wakeUpCoroutine");
				return;
			}
			if (this.Phase == EndMission.MissionPhase.BossKilled)
			{
				if (!string.IsNullOrEmpty(this.endCinematicSound))
				{
					SoundPlayer.PlaySound(this.endCinematicSound, 1f);
				}
				CoroutineManager.Invoke(delegate
				{
					ISpatialEntity targetEntity = this.boss;
					Camera cam = GameMain.GameScreen.Cam;
					Alignment? cameraStartPos = null;
					Alignment? cameraEndPos = new Alignment?(Alignment.Center);
					bool fadeOut = false;
					bool losFadeIn = false;
					float waitDuration = 0f;
					float panDuration = 3f;
					float? endZoom = new float?(0.1f * GUI.yScale);
					CameraTransition cameraTransition = new CameraTransition(targetEntity, cam, cameraStartPos, cameraEndPos, fadeOut, losFadeIn, waitDuration, panDuration, null, endZoom);
					cameraTransition.RunWhilePaused = false;
					cameraTransition.EndWaitDuration = float.PositiveInfinity;
				}, 3f);
			}
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x0006C6BC File Offset: 0x0006A8BC
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.Phase == EndMission.MissionPhase.BossKilled;
		}

		// Token: 0x06000B95 RID: 2965 RVA: 0x0006C85C File Offset: 0x0006AA5C
		[CompilerGenerated]
		private IEnumerable<CoroutineStatus> <OnStateChangedProjSpecific>g__wakeUpCoroutine|35_2()
		{
			EndMission.<<OnStateChangedProjSpecific>g__wakeUpCoroutine|35_2>d <<OnStateChangedProjSpecific>g__wakeUpCoroutine|35_2>d = new EndMission.<<OnStateChangedProjSpecific>g__wakeUpCoroutine|35_2>d(-2);
			<<OnStateChangedProjSpecific>g__wakeUpCoroutine|35_2>d.<>4__this = this;
			return <<OnStateChangedProjSpecific>g__wakeUpCoroutine|35_2>d;
		}

		// Token: 0x040005FB RID: 1531
		private readonly CharacterPrefab bossPrefab;

		// Token: 0x040005FC RID: 1532
		private readonly CharacterPrefab minionPrefab;

		// Token: 0x040005FD RID: 1533
		private readonly Identifier spawnPointTag;

		// Token: 0x040005FE RID: 1534
		private WayPoint bossSpawnPoint;

		// Token: 0x040005FF RID: 1535
		private readonly Identifier destructibleItemTag;

		// Token: 0x04000600 RID: 1536
		private readonly string endCinematicSound;

		// Token: 0x04000601 RID: 1537
		private ImmutableArray<Character> minions;

		// Token: 0x04000602 RID: 1538
		private readonly int minionCount;

		// Token: 0x04000603 RID: 1539
		private readonly float minionScatter;

		// Token: 0x04000604 RID: 1540
		private Character boss;

		// Token: 0x04000605 RID: 1541
		private readonly ItemPrefab projectilePrefab;

		// Token: 0x04000606 RID: 1542
		private float projectileTimer = 30f;

		// Token: 0x04000607 RID: 1543
		private readonly float startCinematicDistance = 30f;

		// Token: 0x04000608 RID: 1544
		private float endCinematicTimer;

		// Token: 0x04000609 RID: 1545
		private readonly List<Item> destructibleItems = new List<Item>();

		// Token: 0x0400060A RID: 1546
		protected readonly float wakeUpCinematicDelay = 5f;

		// Token: 0x0400060B RID: 1547
		protected readonly float bossWakeUpDelay = 7f;

		// Token: 0x0400060C RID: 1548
		protected readonly float cameraWaitDuration = 7f;

		// Token: 0x020007D4 RID: 2004
		private enum MissionPhase
		{
			// Token: 0x04003BE6 RID: 15334
			Initial,
			// Token: 0x04003BE7 RID: 15335
			NoItemsDestroyed,
			// Token: 0x04003BE8 RID: 15336
			SomeItemsDestroyed,
			// Token: 0x04003BE9 RID: 15337
			AllItemsDestroyed,
			// Token: 0x04003BEA RID: 15338
			BossKilled
		}
	}
}
