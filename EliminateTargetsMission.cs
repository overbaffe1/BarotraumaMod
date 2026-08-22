using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Barotrauma.RuinGeneration;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000053 RID: 83
	[TypePreviouslyKnownAs("AlienRuinMission")]
	internal class EliminateTargetsMission : Mission
	{
		// Token: 0x1700032C RID: 812
		// (get) Token: 0x06000B72 RID: 2930 RVA: 0x0006B12E File Offset: 0x0006932E
		public override bool DisplayAsCompleted
		{
			get
			{
				return this.State > 0;
			}
		}

		// Token: 0x1700032D RID: 813
		// (get) Token: 0x06000B73 RID: 2931 RVA: 0x0006B139 File Offset: 0x00069339
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000B74 RID: 2932 RVA: 0x0006B13C File Offset: 0x0006933C
		public override void ClientReadInitial(IReadMessage msg)
		{
			base.ClientReadInitial(msg);
			this.existingTargets.Clear();
			this.spawnedTargets.Clear();
			this.allTargets.Clear();
			ushort existingTargetsCount = msg.ReadUInt16();
			for (int i = 0; i < (int)existingTargetsCount; i++)
			{
				ushort targetId = msg.ReadUInt16();
				if (targetId != 0)
				{
					Entity target = Entity.FindEntityByID(targetId);
					if (target != null)
					{
						this.allTargets.Add(target);
					}
				}
			}
			ushort spawnedTargetsCount = msg.ReadUInt16();
			for (int j = 0; j < (int)spawnedTargetsCount; j++)
			{
				Character enemy = Character.ReadSpawnData(msg);
				this.allTargets.Add(enemy);
			}
		}

		// Token: 0x1700032E RID: 814
		// (get) Token: 0x06000B75 RID: 2933 RVA: 0x0006B1D5 File Offset: 0x000693D5
		// (set) Token: 0x06000B76 RID: 2934 RVA: 0x0006B1DD File Offset: 0x000693DD
		private Submarine TargetSub { get; set; }

		// Token: 0x1700032F RID: 815
		// (get) Token: 0x06000B77 RID: 2935 RVA: 0x0006B1E8 File Offset: 0x000693E8
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
				if (this.State == 0)
				{
					return from t in this.allTargets.Where(delegate(Entity t)
					{
						Item i = t as Item;
						if (i == null || EliminateTargetsMission.IsItemDestroyed(i))
						{
							Character c = t as Character;
							return c != null && !EliminateTargetsMission.IsEnemyDefeated(c);
						}
						return true;
					})
					select new ValueTuple<LocalizedString, Vector2>(this.Prefab.SonarLabel, t.WorldPosition);
				}
				return Enumerable.Empty<ValueTuple<LocalizedString, Vector2>>();
			}
		}

		// Token: 0x06000B78 RID: 2936 RVA: 0x0006B240 File Offset: 0x00069440
		public EliminateTargetsMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.targetItemIdentifiers = prefab.ConfigElement.GetAttributeIdentifierArray("targetitems", Array.Empty<Identifier>(), true);
			this.targetEnemyIdentifiers = prefab.ConfigElement.GetAttributeIdentifierArray("targetenemies", Array.Empty<Identifier>(), true);
			this.minEnemyCount = prefab.ConfigElement.GetAttributeInt("minenemycount", 0);
			ContentXElement configElement = prefab.ConfigElement;
			string key = "targetsub";
			SubmarineType submarineType = SubmarineType.Ruin;
			this.TargetSubType = configElement.GetAttributeEnum<SubmarineType>(key, submarineType);
			this.PrioritizeThalamus = prefab.RequireThalamusWreck;
		}

		// Token: 0x06000B79 RID: 2937 RVA: 0x0006B2EC File Offset: 0x000694EC
		protected override void StartMissionSpecific(Level level)
		{
			this.existingTargets.Clear();
			this.spawnedTargets.Clear();
			this.allTargets.Clear();
			if (Mission.IsClient)
			{
				return;
			}
			Submarine targetSub;
			switch (this.TargetSubType)
			{
			case SubmarineType.Wreck:
				targetSub = this.<StartMissionSpecific>g__FindWreck|20_0();
				goto IL_8F;
			case SubmarineType.BeaconStation:
			{
				Level loaded = Level.Loaded;
				targetSub = ((loaded != null) ? loaded.BeaconStation : null);
				goto IL_8F;
			}
			case SubmarineType.Ruin:
			{
				Level loaded2 = Level.Loaded;
				Submarine submarine;
				if (loaded2 == null)
				{
					submarine = null;
				}
				else
				{
					List<Ruin> ruins = loaded2.Ruins;
					submarine = ((ruins != null) ? ruins.GetRandom(Rand.RandSync.ServerAndClient).Submarine : null);
				}
				targetSub = submarine;
				goto IL_8F;
			}
			}
			targetSub = null;
			IL_8F:
			this.TargetSub = targetSub;
			if (this.TargetSub == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to initialize an ");
				defaultInterpolatedStringHandler.AppendFormatted("EliminateTargetsMission");
				defaultInterpolatedStringHandler.AppendLiteral(" mission (\"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\"): level contains no submarines");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return;
			}
			if (this.targetItemIdentifiers.Length < 1 && this.targetEnemyIdentifiers.Length < 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(90, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Failed to initialize an ");
				defaultInterpolatedStringHandler2.AppendFormatted("EliminateTargetsMission");
				defaultInterpolatedStringHandler2.AppendLiteral(" mission (\"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\"): no target identifiers set in the mission definition");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return;
			}
			foreach (Item item in Item.ItemList)
			{
				if (this.targetItemIdentifiers.Contains(item.Prefab.Identifier) && item.Submarine == this.TargetSub)
				{
					this.existingTargets.Add(item);
					this.allTargets.Add(item);
				}
			}
			int existingEnemyCount = 0;
			foreach (Character character in Character.CharacterList)
			{
				if (!character.SpeciesName.IsEmpty && this.targetEnemyIdentifiers.Contains(character.SpeciesName) && character.Submarine == this.TargetSub)
				{
					this.existingTargets.Add(character);
					this.allTargets.Add(character);
					existingEnemyCount++;
				}
			}
			if (existingEnemyCount < this.minEnemyCount)
			{
				HashSet<CharacterPrefab> enemyPrefabs = new HashSet<CharacterPrefab>();
				foreach (Identifier identifier in this.targetEnemyIdentifiers)
				{
					CharacterPrefab prefab = CharacterPrefab.FindBySpeciesName(identifier);
					if (prefab != null)
					{
						enemyPrefabs.Add(prefab);
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(90, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Error in an Alien Ruin mission (\"");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Prefab.Identifier);
						defaultInterpolatedStringHandler3.AppendLiteral("\"): could not find a character prefab with the species \"");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(identifier);
						defaultInterpolatedStringHandler3.AppendLiteral("\"");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					}
				}
				if (enemyPrefabs.None(null))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(105, 2);
					defaultInterpolatedStringHandler4.AppendLiteral("Error in an Alien Ruin mission (\"");
					defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler4.AppendLiteral("\"): no enemy species defined that could be used to spawn more (");
					defaultInterpolatedStringHandler4.AppendFormatted<int>(this.minEnemyCount - existingEnemyCount);
					defaultInterpolatedStringHandler4.AppendLiteral(") enemies");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					return;
				}
				for (int i = 0; i < this.minEnemyCount - existingEnemyCount; i++)
				{
					CharacterPrefab prefab2 = enemyPrefabs.GetRandomUnsynced<CharacterPrefab>();
					WayPoint randomUnsynced = this.TargetSub.GetWaypoints(false).GetRandomUnsynced((WayPoint w) => w.CurrentHull != null);
					Vector2? spawnPos = (randomUnsynced != null) ? new Vector2?(randomUnsynced.WorldPosition) : null;
					if (spawnPos == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(120, 2);
						defaultInterpolatedStringHandler5.AppendLiteral("Error in an Alien Ruin mission (\"");
						defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(this.Prefab.Identifier);
						defaultInterpolatedStringHandler5.AppendLiteral("\"): no valid spawn positions could be found for the additional (");
						defaultInterpolatedStringHandler5.AppendFormatted<int>(this.minEnemyCount - existingEnemyCount);
						defaultInterpolatedStringHandler5.AppendLiteral(") enemies to be spawned");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
						return;
					}
					Character newEnemy = Character.Create(prefab2.Identifier, spawnPos.Value, ToolBox.RandomSeed(8), null, 0, false, true, false, null, true, true);
					this.spawnedTargets.Add(newEnemy);
					this.allTargets.Add(newEnemy);
				}
			}
		}

		// Token: 0x06000B7A RID: 2938 RVA: 0x0006B7F0 File Offset: 0x000699F0
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			if (Mission.IsClient)
			{
				return;
			}
			if (this.State == 0)
			{
				if (!this.AllTargetsEliminated())
				{
					return;
				}
				this.State = 1;
			}
		}

		// Token: 0x06000B7B RID: 2939 RVA: 0x0006B820 File Offset: 0x00069A20
		private bool AllTargetsEliminated()
		{
			foreach (Entity target in this.allTargets)
			{
				Item targetItem = target as Item;
				if (targetItem != null)
				{
					if (!EliminateTargetsMission.IsItemDestroyed(targetItem))
					{
						return false;
					}
				}
				else
				{
					Character targetEnemy = target as Character;
					if (targetEnemy != null && !EliminateTargetsMission.IsEnemyDefeated(targetEnemy))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000B7C RID: 2940 RVA: 0x0006B8A0 File Offset: 0x00069AA0
		private static bool IsItemDestroyed(Item item)
		{
			return item == null || item.Removed || item.Condition <= 0f;
		}

		// Token: 0x06000B7D RID: 2941 RVA: 0x0006B8BF File Offset: 0x00069ABF
		private static bool IsEnemyDefeated(Character enemy)
		{
			return enemy == null || enemy.Removed || enemy.IsDead;
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x0006B8D4 File Offset: 0x00069AD4
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
			bool flag;
			if (campaign == null)
			{
				Submarine sub = Submarine.MainSub;
				flag = (sub != null && sub.AtEitherExit);
			}
			else
			{
				flag = (campaign.GetAvailableTransition() > CampaignMode.TransitionType.None);
			}
			bool exitingLevel = flag;
			return this.State > 0 && exitingLevel;
		}

		// Token: 0x06000B7F RID: 2943 RVA: 0x0006B924 File Offset: 0x00069B24
		protected override void EndMissionSpecific(bool completed)
		{
			this.failed = (!completed && this.State > 0);
		}

		// Token: 0x06000B81 RID: 2945 RVA: 0x0006B954 File Offset: 0x00069B54
		[CompilerGenerated]
		private Submarine <StartMissionSpecific>g__FindWreck|20_0()
		{
			Level loaded = Level.Loaded;
			List<Submarine> wrecks = (loaded != null) ? loaded.Wrecks : null;
			if (wrecks == null || wrecks.None(null))
			{
				return null;
			}
			if (this.PrioritizeThalamus)
			{
				Submarine[] thalamusWrecks = (from w in wrecks
				where w.WreckAI != null
				select w).ToArray<Submarine>();
				if (thalamusWrecks.Any<Submarine>())
				{
					return thalamusWrecks.GetRandom(Rand.RandSync.ServerAndClient);
				}
			}
			return wrecks.GetRandom(Rand.RandSync.ServerAndClient);
		}

		// Token: 0x040005F2 RID: 1522
		private readonly Identifier[] targetItemIdentifiers;

		// Token: 0x040005F3 RID: 1523
		private readonly Identifier[] targetEnemyIdentifiers;

		// Token: 0x040005F4 RID: 1524
		private readonly int minEnemyCount;

		// Token: 0x040005F5 RID: 1525
		private readonly HashSet<Entity> existingTargets = new HashSet<Entity>();

		// Token: 0x040005F6 RID: 1526
		private readonly HashSet<Character> spawnedTargets = new HashSet<Character>();

		// Token: 0x040005F7 RID: 1527
		private readonly HashSet<Entity> allTargets = new HashSet<Entity>();

		// Token: 0x040005F8 RID: 1528
		public readonly SubmarineType TargetSubType;

		// Token: 0x040005F9 RID: 1529
		public readonly bool PrioritizeThalamus;
	}
}
