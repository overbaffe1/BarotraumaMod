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
	// Token: 0x02000023 RID: 35
	[TypePreviouslyKnownAs("AlienRuinMission")]
	internal class EliminateTargetsMission : Mission
	{
		// Token: 0x06000478 RID: 1144 RVA: 0x000272B8 File Offset: 0x000254B8
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			msg.WriteUInt16((ushort)this.existingTargets.Count);
			foreach (Entity t in this.existingTargets)
			{
				msg.WriteUInt16((t != null) ? t.ID : 0);
			}
			msg.WriteUInt16((ushort)this.spawnedTargets.Count);
			foreach (Character t2 in this.spawnedTargets)
			{
				t2.WriteSpawnData(msg, t2.ID, false);
			}
		}

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000479 RID: 1145 RVA: 0x0002738C File Offset: 0x0002558C
		// (set) Token: 0x0600047A RID: 1146 RVA: 0x00027394 File Offset: 0x00025594
		private Submarine TargetSub { get; set; }

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x0600047B RID: 1147 RVA: 0x000273A0 File Offset: 0x000255A0
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

		// Token: 0x0600047C RID: 1148 RVA: 0x000273F8 File Offset: 0x000255F8
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

		// Token: 0x0600047D RID: 1149 RVA: 0x000274A4 File Offset: 0x000256A4
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
				targetSub = this.<StartMissionSpecific>g__FindWreck|16_0();
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

		// Token: 0x0600047E RID: 1150 RVA: 0x000279A8 File Offset: 0x00025BA8
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

		// Token: 0x0600047F RID: 1151 RVA: 0x000279D8 File Offset: 0x00025BD8
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

		// Token: 0x06000480 RID: 1152 RVA: 0x00027A58 File Offset: 0x00025C58
		private static bool IsItemDestroyed(Item item)
		{
			return item == null || item.Removed || item.Condition <= 0f;
		}

		// Token: 0x06000481 RID: 1153 RVA: 0x00027A77 File Offset: 0x00025C77
		private static bool IsEnemyDefeated(Character enemy)
		{
			return enemy == null || enemy.Removed || enemy.IsDead;
		}

		// Token: 0x06000482 RID: 1154 RVA: 0x00027A8C File Offset: 0x00025C8C
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

		// Token: 0x06000483 RID: 1155 RVA: 0x00027ADC File Offset: 0x00025CDC
		protected override void EndMissionSpecific(bool completed)
		{
			this.failed = (!completed && this.State > 0);
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x00027B0C File Offset: 0x00025D0C
		[CompilerGenerated]
		private Submarine <StartMissionSpecific>g__FindWreck|16_0()
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

		// Token: 0x04000234 RID: 564
		private readonly Identifier[] targetItemIdentifiers;

		// Token: 0x04000235 RID: 565
		private readonly Identifier[] targetEnemyIdentifiers;

		// Token: 0x04000236 RID: 566
		private readonly int minEnemyCount;

		// Token: 0x04000237 RID: 567
		private readonly HashSet<Entity> existingTargets = new HashSet<Entity>();

		// Token: 0x04000238 RID: 568
		private readonly HashSet<Character> spawnedTargets = new HashSet<Character>();

		// Token: 0x04000239 RID: 569
		private readonly HashSet<Entity> allTargets = new HashSet<Entity>();

		// Token: 0x0400023A RID: 570
		public readonly SubmarineType TargetSubType;

		// Token: 0x0400023B RID: 571
		public readonly bool PrioritizeThalamus;
	}
}
