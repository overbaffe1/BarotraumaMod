using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200004F RID: 79
	internal class BeaconMission : Mission
	{
		// Token: 0x1700031A RID: 794
		// (get) Token: 0x06000B3E RID: 2878 RVA: 0x00068D8C File Offset: 0x00066F8C
		public override bool DisplayAsCompleted
		{
			get
			{
				return this.State > 0;
			}
		}

		// Token: 0x1700031B RID: 795
		// (get) Token: 0x06000B3F RID: 2879 RVA: 0x00068D97 File Offset: 0x00066F97
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000B40 RID: 2880 RVA: 0x00068D9C File Offset: 0x00066F9C
		public BeaconMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.swarmSpawned = false;
			this.beaconTags = prefab.ConfigElement.GetAttributeIdentifierArray("beacontags", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			foreach (ContentXElement monsterElement in prefab.ConfigElement.GetChildElements("monster"))
			{
				if (!this.monsterSets.Any<BeaconMission.MonsterSet>())
				{
					this.monsterSets.Add(new BeaconMission.MonsterSet(monsterElement));
				}
				this.LoadMonsters(monsterElement, this.monsterSets[0]);
			}
			foreach (ContentXElement monsterSetElement in prefab.ConfigElement.GetChildElements("monsters"))
			{
				this.monsterSets.Add(new BeaconMission.MonsterSet(monsterSetElement));
				foreach (ContentXElement monsterElement2 in monsterSetElement.GetChildElements("monster"))
				{
					this.LoadMonsters(monsterElement2, this.monsterSets.Last<BeaconMission.MonsterSet>());
				}
			}
			this.sonarLabel = TextManager.Get("beaconstationsonarlabel");
			DebugConsole.NewMessage("Initialized beacon mission: " + prefab.Identifier.ToString(), new Color?(Color.LightSkyBlue), true);
		}

		// Token: 0x06000B41 RID: 2881 RVA: 0x00068F50 File Offset: 0x00067150
		private void LoadMonsters(XElement monsterElement, BeaconMission.MonsterSet set)
		{
			Identifier speciesName = monsterElement.GetAttributeIdentifier("character", Identifier.Empty);
			int defaultCount = monsterElement.GetAttributeInt("count", -1);
			if (defaultCount < 0)
			{
				defaultCount = monsterElement.GetAttributeInt("amount", 1);
			}
			int min = Math.Min(monsterElement.GetAttributeInt("min", defaultCount), 255);
			int max = Math.Min(Math.Max(min, monsterElement.GetAttributeInt("max", defaultCount)), 255);
			CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(speciesName);
			if (characterPrefab != null)
			{
				set.MonsterPrefabs.Add(new ValueTuple<CharacterPrefab, Point>(characterPrefab, new Point(min, max)));
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(79, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Error in beacon mission \"");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral("\". Could not find a character prefab with the name \"");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
			defaultInterpolatedStringHandler.AppendLiteral("\".");
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
		}

		// Token: 0x1700031C RID: 796
		// (get) Token: 0x06000B42 RID: 2882 RVA: 0x0006904C File Offset: 0x0006724C
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
				BeaconMission.<get_SonarLabels>d__12 <get_SonarLabels>d__ = new BeaconMission.<get_SonarLabels>d__12(-2);
				<get_SonarLabels>d__.<>4__this = this;
				return <get_SonarLabels>d__;
			}
		}

		// Token: 0x06000B43 RID: 2883 RVA: 0x0006906C File Offset: 0x0006726C
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			if (Mission.IsClient)
			{
				return;
			}
			if (!this.swarmSpawned && this.level.CheckBeaconActive())
			{
				BeaconMission.<>c__DisplayClass13_0 CS$<>8__locals1 = new BeaconMission.<>c__DisplayClass13_0();
				IEnumerable<Submarine> connectedSubs = this.level.BeaconStation.GetConnectedSubs();
				foreach (Item item in Item.ItemList)
				{
					if (connectedSubs.Contains(item.Submarine))
					{
						Submarine submarine = item.Submarine;
						SubmarineInfo submarineInfo = (submarine != null) ? submarine.Info : null;
						if (submarineInfo == null || !submarineInfo.IsPlayer)
						{
							bool isReactor = item.GetComponent<Reactor>() != null;
							if (isReactor)
							{
								GameSession gameSession = GameMain.GameSession;
								if (gameSession == null || !gameSession.TraitorsEnabled)
								{
									goto IL_C3;
								}
							}
							if (item.GetComponent<PowerTransfer>() == null && item.GetComponent<PowerContainer>() == null && item.GetComponent<Sonar>() == null)
							{
								continue;
							}
							IL_C3:
							item.InvulnerableToDamage = true;
						}
					}
				}
				this.State = 1;
				CS$<>8__locals1.spawnPos = this.level.BeaconStation.WorldPosition;
				BeaconMission.<>c__DisplayClass13_0 CS$<>8__locals2 = CS$<>8__locals1;
				CS$<>8__locals2.spawnPos.Y = CS$<>8__locals2.spawnPos.Y + (float)this.level.BeaconStation.GetDockedBorders(true).Height * 1.5f;
				List<Level.InterestingPosition> availablePositions = Level.Loaded.PositionsOfInterest.FindAll((Level.InterestingPosition p) => p.PositionType == Level.PositionType.MainPath || p.PositionType == Level.PositionType.SidePath);
				availablePositions.RemoveAll((Level.InterestingPosition p) => Level.Loaded.ExtraWalls.Any((LevelWall w) => w.IsPointInside(p.Position.ToVector2())));
				availablePositions.RemoveAll((Level.InterestingPosition p) => Submarine.FindContaining(p.Position.ToVector2(), 500f) != null);
				if (availablePositions.Any<Level.InterestingPosition>())
				{
					Level.InterestingPosition? closestPos = null;
					float closestDist = float.PositiveInfinity;
					foreach (Level.InterestingPosition pos in availablePositions)
					{
						Point position = pos.Position;
						float dist = Vector2.DistanceSquared(position.ToVector2(), this.level.BeaconStation.WorldPosition);
						if (dist < closestDist)
						{
							closestDist = dist;
							closestPos = new Level.InterestingPosition?(pos);
						}
					}
					if (closestPos != null)
					{
						CS$<>8__locals1.spawnPos = closestPos.Value.Position.ToVector2();
					}
				}
				if (this.monsterSets.Any<BeaconMission.MonsterSet>())
				{
					BeaconMission.MonsterSet monsterSet = ToolBox.SelectWeightedRandom<BeaconMission.MonsterSet>(this.monsterSets, (BeaconMission.MonsterSet m) => m.Commonness, Rand.RandSync.Unsynced);
					foreach (ValueTuple<CharacterPrefab, Point> valueTuple in monsterSet.MonsterPrefabs)
					{
						CharacterPrefab monsterSpecies = valueTuple.Item1;
						Point monsterCountRange = valueTuple.Item2;
						int amount = Rand.Range(monsterCountRange.X, monsterCountRange.Y + 1, Rand.RandSync.Unsynced);
						Action <>9__5;
						for (int i = 0; i < amount; i++)
						{
							Action action;
							if ((action = <>9__5) == null)
							{
								action = (<>9__5 = delegate()
								{
									if (GameMain.GameSession == null || Level.Loaded == null)
									{
										return;
									}
									Entity.Spawner.AddCharacterToSpawnQueue(monsterSpecies.Identifier, CS$<>8__locals1.spawnPos, null);
								});
							}
							CoroutineManager.Invoke(action, Rand.Range(0f, (float)amount, Rand.RandSync.Unsynced));
						}
					}
				}
				this.swarmSpawned = true;
			}
		}

		// Token: 0x06000B44 RID: 2884 RVA: 0x000693F4 File Offset: 0x000675F4
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.level.CheckBeaconActive();
		}

		// Token: 0x06000B45 RID: 2885 RVA: 0x00069401 File Offset: 0x00067601
		protected override void EndMissionSpecific(bool completed)
		{
			if (completed && this.level.LevelData != null)
			{
				this.level.LevelData.IsBeaconActive = true;
			}
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x00069424 File Offset: 0x00067624
		public override void AdjustLevelData(LevelData levelData)
		{
			levelData.HasBeaconStation = true;
			levelData.IsBeaconActive = false;
			if (this.beaconTags.Length > 0)
			{
				SubmarineInfo selectedBeacon = BeaconMission.GetRandomBeaconByTags(this.beaconTags, levelData);
				if (selectedBeacon != null)
				{
					levelData.ForceBeaconStation = selectedBeacon;
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(100, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Beacon mission \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" could not find a suitable beacon station with beacontags \"");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join<Identifier>(", ", this.beaconTags));
				defaultInterpolatedStringHandler.AppendLiteral("\" for level difficulty ");
				defaultInterpolatedStringHandler.AppendFormatted<float>(levelData.Difficulty, "F1");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
			}
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x000694FC File Offset: 0x000676FC
		private static SubmarineInfo GetRandomBeaconByTags(ImmutableArray<Identifier> tags, LevelData levelData)
		{
			return Mission.GetRandomSubmarineByTagsAndDifficulty(tags, levelData, (SubmarineInfo s) => s.IsBeacon, "beacon station");
		}

		// Token: 0x040005D0 RID: 1488
		private bool swarmSpawned;

		// Token: 0x040005D1 RID: 1489
		private readonly List<BeaconMission.MonsterSet> monsterSets = new List<BeaconMission.MonsterSet>();

		// Token: 0x040005D2 RID: 1490
		private readonly LocalizedString sonarLabel;

		// Token: 0x040005D3 RID: 1491
		private readonly ImmutableArray<Identifier> beaconTags;

		// Token: 0x020007C7 RID: 1991
		private class MonsterSet
		{
			// Token: 0x06006BBB RID: 27579 RVA: 0x0035DD49 File Offset: 0x0035BF49
			public MonsterSet(XElement element)
			{
				this.Commonness = element.GetAttributeFloat("commonness", 100f);
			}

			// Token: 0x04003BBD RID: 15293
			[TupleElementNames(new string[]
			{
				"character",
				"amountRange"
			})]
			public readonly HashSet<ValueTuple<CharacterPrefab, Point>> MonsterPrefabs = new HashSet<ValueTuple<CharacterPrefab, Point>>();

			// Token: 0x04003BBE RID: 15294
			public float Commonness;
		}
	}
}
