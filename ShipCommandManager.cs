using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001A0 RID: 416
	internal class ShipCommandManager
	{
		// Token: 0x17000C50 RID: 3152
		// (get) Token: 0x06002FBD RID: 12221 RVA: 0x001F7759 File Offset: 0x001F5959
		// (set) Token: 0x06002FBE RID: 12222 RVA: 0x001F7761 File Offset: 0x001F5961
		public bool Active
		{
			get
			{
				return this.active;
			}
			set
			{
				this.active = (value ? this.TryInitializeShipCommandManager() : value);
			}
		}

		// Token: 0x17000C51 RID: 3153
		// (get) Token: 0x06002FBF RID: 12223 RVA: 0x001F7775 File Offset: 0x001F5975
		// (set) Token: 0x06002FC0 RID: 12224 RVA: 0x001F777D File Offset: 0x001F597D
		public Submarine EnemySubmarine { get; private set; }

		// Token: 0x17000C52 RID: 3154
		// (get) Token: 0x06002FC1 RID: 12225 RVA: 0x001F7786 File Offset: 0x001F5986
		// (set) Token: 0x06002FC2 RID: 12226 RVA: 0x001F778E File Offset: 0x001F598E
		public Submarine CommandedSubmarine { get; private set; }

		// Token: 0x17000C53 RID: 3155
		// (get) Token: 0x06002FC3 RID: 12227 RVA: 0x001F7797 File Offset: 0x001F5997
		// (set) Token: 0x06002FC4 RID: 12228 RVA: 0x001F779F File Offset: 0x001F599F
		public ShipCommandManager.NavigationStates NavigationState { get; private set; }

		// Token: 0x06002FC5 RID: 12229 RVA: 0x001F77A8 File Offset: 0x001F59A8
		public ShipCommandManager(Character character)
		{
			this.character = character;
			this.humanAIController = (character.AIController as HumanAIController);
		}

		// Token: 0x06002FC6 RID: 12230 RVA: 0x001F7844 File Offset: 0x001F5A44
		public void Update(float deltaTime)
		{
			if (!this.Active || this.character.IsHandcuffed)
			{
				return;
			}
			this.decisionTimer -= deltaTime;
			if (this.decisionTimer <= 0f)
			{
				this.UpdateCommandDecision(this.timeSinceLastCommandDecision);
				this.decisionTimer = this.decisionInterval * Rand.Range(0.8f, 1.2f, Rand.RandSync.Unsynced);
				this.timeSinceLastCommandDecision = this.decisionTimer;
			}
			this.navigationTimer -= deltaTime;
			if (this.navigationTimer <= 0f)
			{
				this.UpdateNavigation(this.timeSinceLastNavigation);
				this.navigationTimer = this.navigationInterval * Rand.Range(0.8f, 1.2f, Rand.RandSync.Unsynced);
				this.timeSinceLastNavigation = this.navigationTimer;
			}
		}

		// Token: 0x06002FC7 RID: 12231 RVA: 0x001F7908 File Offset: 0x001F5B08
		public static void ShipCommandLog(string text)
		{
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.NewMessage(text, null, false);
			}
		}

		// Token: 0x06002FC8 RID: 12232 RVA: 0x001F7931 File Offset: 0x001F5B31
		private static bool WithinRange(float range, float distanceSquared)
		{
			return range * range > distanceSquared;
		}

		// Token: 0x06002FC9 RID: 12233 RVA: 0x001F793C File Offset: 0x001F5B3C
		private void UpdateNavigation(float timeSinceLastUpdate)
		{
			if (this.steering == null || this.EnemySubmarine == null)
			{
				return;
			}
			float distanceSquaredEnemy = Vector2.DistanceSquared(this.CommandedSubmarine.WorldPosition, this.EnemySubmarine.WorldPosition);
			if (this.NavigationState != ShipCommandManager.NavigationStates.Aggressive)
			{
				if (ShipCommandManager.WithinRange(7000f, distanceSquaredEnemy))
				{
					this.NavigationState = ShipCommandManager.NavigationStates.Aggressive;
				}
				else if (ShipCommandManager.WithinRange(40000f, distanceSquaredEnemy))
				{
					this.NavigationState = ShipCommandManager.NavigationStates.Patrol;
				}
			}
			if (this.NavigationState == ShipCommandManager.NavigationStates.Aggressive)
			{
				this.steering.AITacticalTarget = this.EnemySubmarine.WorldPosition;
				if (!ShipCommandManager.WithinRange(8500f, distanceSquaredEnemy) || ShipCommandManager.WithinRange(1500f, distanceSquaredEnemy))
				{
					this.steering.AIRamTimer = 0f;
					this.timeUntilRam = 17.5f * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
					return;
				}
				if (this.steering.AIRamTimer <= 0f)
				{
					this.timeUntilRam -= timeSinceLastUpdate;
					if (this.timeUntilRam <= 0f)
					{
						this.steering.AIRamTimer = 50f;
						this.timeUntilRam = 17.5f * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
						return;
					}
				}
			}
			else if (this.patrolPositions.Any<Vector2>())
			{
				float distanceSquaredPatrol = Vector2.DistanceSquared(this.CommandedSubmarine.WorldPosition, this.patrolPositions.First<Vector2>());
				if (ShipCommandManager.WithinRange(7000f, distanceSquaredPatrol))
				{
					Vector2 lastPosition = this.patrolPositions.First<Vector2>();
					this.patrolPositions.RemoveAt(0);
					this.patrolPositions.Add(lastPosition);
				}
				this.steering.AITacticalTarget = this.patrolPositions.First<Vector2>();
			}
		}

		// Token: 0x06002FCA RID: 12234 RVA: 0x001F7AE2 File Offset: 0x001F5CE2
		public bool AbleToTakeOrder(Character character)
		{
			return !character.IsIncapacitated && !character.LockHands && character.Submarine == this.CommandedSubmarine;
		}

		// Token: 0x06002FCB RID: 12235 RVA: 0x001F7B04 File Offset: 0x001F5D04
		private void UpdateCommandDecision(float timeSinceLastUpdate)
		{
			this.shipGlobalIssues.ForEach(delegate(ShipGlobalIssue c)
			{
				c.CalculateGlobalIssue();
			});
			this.AlliedCharacters.Clear();
			this.EnemyCharacters.Clear();
			bool isEmergency = false;
			foreach (Character potentialCharacter in Character.CharacterList)
			{
				if (HumanAIController.IsActive(potentialCharacter))
				{
					if (HumanAIController.IsFriendly(this.character, potentialCharacter, true, false) && potentialCharacter.AIController is HumanAIController)
					{
						if (this.AbleToTakeOrder(potentialCharacter))
						{
							this.AlliedCharacters.Add(potentialCharacter);
						}
					}
					else
					{
						this.EnemyCharacters.Add(potentialCharacter);
						if (potentialCharacter.Submarine == this.CommandedSubmarine)
						{
							isEmergency = true;
						}
					}
				}
			}
			this.attendedIssues.Clear();
			this.availableIssues.Clear();
			foreach (ShipIssueWorker shipIssueWorker in this.ShipIssueWorkers)
			{
				float importance = shipIssueWorker.CalculateImportance(isEmergency);
				if (shipIssueWorker.OrderAttendedTo(timeSinceLastUpdate))
				{
					ShipCommandManager.<UpdateCommandDecision>g__InsertIssue|43_1(shipIssueWorker, this.attendedIssues);
				}
				else
				{
					shipIssueWorker.RemoveOrder();
					ShipCommandManager.<UpdateCommandDecision>g__InsertIssue|43_1(shipIssueWorker, this.availableIssues);
				}
			}
			ShipIssueWorker mostImportantIssue = this.availableIssues.FirstOrDefault<ShipIssueWorker>();
			float bestValue = 0f;
			Character bestCharacter = null;
			if (mostImportantIssue != null && mostImportantIssue.Importance >= 10f)
			{
				IEnumerable<Character> bestCharacters = CrewManager.GetCharactersSortedForOrder(mostImportantIssue.SuggestedOrder, this.AlliedCharacters, this.character, true, null);
				using (IEnumerator<Character> enumerator3 = bestCharacters.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Character orderedCharacter = enumerator3.Current;
						float issueApplicability = mostImportantIssue.Importance;
						issueApplicability *= (mostImportantIssue.SuggestedOrder.AppropriateJobs.Contains(orderedCharacter.Info.Job.Prefab.Identifier) ? 1f : 0.75f);
						ShipIssueWorker occupiedIssue = this.attendedIssues.FirstOrDefault((ShipIssueWorker i) => i.OrderedCharacter == orderedCharacter);
						if (occupiedIssue != null)
						{
							if (occupiedIssue.GetType() == mostImportantIssue.GetType() && mostImportantIssue is ShipIssueWorkerGlobal && occupiedIssue is ShipIssueWorkerGlobal)
							{
								continue;
							}
							if (mostImportantIssue.AllowEasySwitching && occupiedIssue.AllowEasySwitching)
							{
								issueApplicability /= mostImportantIssue.CurrentRedundancy;
							}
							issueApplicability += (occupiedIssue.SuggestedOrder.AppropriateJobs.Contains(orderedCharacter.Info.Job.Prefab.Identifier) ? 0f : 7.5f);
							issueApplicability -= 5f;
							if (issueApplicability + 5f < occupiedIssue.Importance)
							{
								continue;
							}
						}
						if (issueApplicability > bestValue)
						{
							bestValue = issueApplicability;
							bestCharacter = orderedCharacter;
						}
					}
				}
			}
			if (bestCharacter != null && mostImportantIssue != null)
			{
				mostImportantIssue.SetOrder(bestCharacter);
				return;
			}
			foreach (ShipIssueWorker shipIssueWorker2 in this.ShipIssueWorkers)
			{
				if (shipIssueWorker2.Importance <= 0f && shipIssueWorker2.OrderAttendedTo(0f))
				{
					Order order = new Order(OrderPrefab.Dismissal, null, null).WithManualPriority(3).WithOrderGiver(this.character);
					shipIssueWorker2.OrderedCharacter.SetOrder(order, true, true, false);
					shipIssueWorker2.RemoveOrder();
					break;
				}
			}
		}

		// Token: 0x06002FCC RID: 12236 RVA: 0x001F7EF0 File Offset: 0x001F60F0
		private bool TryInitializeShipCommandManager()
		{
			this.CommandedSubmarine = this.character.Submarine;
			if (this.CommandedSubmarine == null)
			{
				string str = "TryInitializeShipCommandManager failed: CommandedSubmarine was null for character ";
				Character character = this.character;
				DebugConsole.ThrowError(str + ((character != null) ? character.ToString() : null), null, null, false, false);
				return false;
			}
			this.EnemySubmarine = ((Submarine.MainSubs[0] == this.CommandedSubmarine) ? Submarine.MainSubs[1] : Submarine.MainSubs[0]);
			if (this.EnemySubmarine == null)
			{
				string str2 = "TryInitializeShipCommandManager failed: EnemySubmarine was null for character ";
				Character character2 = this.character;
				DebugConsole.ThrowError(str2 + ((character2 != null) ? character2.ToString() : null), null, null, false, false);
				return false;
			}
			this.timeUntilRam = 17.5f * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
			this.ShipIssueWorkers.Clear();
			Item item2 = this.CommandedSubmarine.GetItems(false).Find((Item i) => i.HasTag(Tags.Reactor) && !i.NonInteractable);
			Reactor reactor = (item2 != null) ? item2.GetComponent<Reactor>() : null;
			if (reactor != null)
			{
				Order order = new Order(OrderPrefab.Prefabs["operatereactor"], "powerup".ToIdentifier(), reactor.Item, reactor, null, false);
				this.ShipIssueWorkers.Add(new ShipIssueWorkerPowerUpReactor(this, order));
			}
			Item nav = this.CommandedSubmarine.GetItems(false).Find((Item i) => i.HasTag(Tags.NavTerminal) && !i.NonInteractable);
			if (nav != null)
			{
				Steering steeringComponent = nav.GetComponent<Steering>();
				if (steeringComponent != null)
				{
					this.steering = steeringComponent;
					Order order2 = new Order(OrderPrefab.Prefabs["steer"], "navigatetactical".ToIdentifier(), nav, steeringComponent, null, false);
					this.ShipIssueWorkers.Add(new ShipIssueWorkerSteer(this, order2));
				}
			}
			foreach (Item item in this.CommandedSubmarine.GetItems(true).FindAll((Item i) => i.HasTag(Tags.Turret) && !i.HasTag(Tags.Hardpoint)))
			{
				Order order3 = new Order(OrderPrefab.Prefabs["operateweapons"], item, item.GetComponent<Turret>(), null, false);
				this.ShipIssueWorkers.Add(new ShipIssueWorkerOperateWeapons(this, order3));
			}
			int crewSizeModifier = 2;
			ShipGlobalIssueFixLeaks shipGlobalIssueFixLeaks = new ShipGlobalIssueFixLeaks(this);
			for (int k = 0; k < crewSizeModifier; k++)
			{
				Order order4 = OrderPrefab.Prefabs["fixleaks"].CreateInstance(OrderPrefab.OrderTargetType.Entity, null, false);
				this.ShipIssueWorkers.Add(new ShipIssueWorkerFixLeaks(this, order4, shipGlobalIssueFixLeaks));
			}
			this.shipGlobalIssues.Add(shipGlobalIssueFixLeaks);
			ShipGlobalIssueRepairSystems shipGlobalIssueRepairSystems = new ShipGlobalIssueRepairSystems(this);
			for (int j = 0; j < crewSizeModifier; j++)
			{
				Order order5 = OrderPrefab.Prefabs["repairsystems"].CreateInstance(OrderPrefab.OrderTargetType.Entity, null, false);
				this.ShipIssueWorkers.Add(new ShipIssueWorkerRepairSystems(this, order5, shipGlobalIssueRepairSystems));
			}
			this.shipGlobalIssues.Add(shipGlobalIssueRepairSystems);
			return true;
		}

		// Token: 0x06002FCD RID: 12237 RVA: 0x001F81F8 File Offset: 0x001F63F8
		[CompilerGenerated]
		internal static void <UpdateCommandDecision>g__InsertIssue|43_1(ShipIssueWorker issue, List<ShipIssueWorker> list)
		{
			int index = 0;
			while (index < list.Count && list[index].Importance > issue.Importance)
			{
				index++;
			}
			list.Insert(index, issue);
		}

		// Token: 0x040018D1 RID: 6353
		public readonly Character character;

		// Token: 0x040018D2 RID: 6354
		public readonly HumanAIController humanAIController;

		// Token: 0x040018D3 RID: 6355
		private bool active;

		// Token: 0x040018D6 RID: 6358
		private Steering steering;

		// Token: 0x040018D7 RID: 6359
		public readonly List<Vector2> patrolPositions = new List<Vector2>();

		// Token: 0x040018D9 RID: 6361
		private float navigationTimer;

		// Token: 0x040018DA RID: 6362
		private readonly float navigationInterval = 4f;

		// Token: 0x040018DB RID: 6363
		private float timeUntilRam;

		// Token: 0x040018DC RID: 6364
		private const float RamTimerMax = 17.5f;

		// Token: 0x040018DD RID: 6365
		public readonly List<ShipIssueWorker> ShipIssueWorkers = new List<ShipIssueWorker>();

		// Token: 0x040018DE RID: 6366
		public const float MinimumIssueThreshold = 10f;

		// Token: 0x040018DF RID: 6367
		private const float IssueDevotionBuffer = 5f;

		// Token: 0x040018E0 RID: 6368
		private float decisionTimer = 6f;

		// Token: 0x040018E1 RID: 6369
		private readonly float decisionInterval = 6f;

		// Token: 0x040018E2 RID: 6370
		private float timeSinceLastCommandDecision;

		// Token: 0x040018E3 RID: 6371
		private float timeSinceLastNavigation;

		// Token: 0x040018E4 RID: 6372
		public readonly List<Character> AlliedCharacters = new List<Character>();

		// Token: 0x040018E5 RID: 6373
		public readonly List<Character> EnemyCharacters = new List<Character>();

		// Token: 0x040018E6 RID: 6374
		private readonly List<ShipIssueWorker> attendedIssues = new List<ShipIssueWorker>();

		// Token: 0x040018E7 RID: 6375
		private readonly List<ShipIssueWorker> availableIssues = new List<ShipIssueWorker>();

		// Token: 0x040018E8 RID: 6376
		private readonly List<ShipGlobalIssue> shipGlobalIssues = new List<ShipGlobalIssue>();

		// Token: 0x02000E8C RID: 3724
		public enum NavigationStates
		{
			// Token: 0x0400528B RID: 21131
			Inactive,
			// Token: 0x0400528C RID: 21132
			Patrol,
			// Token: 0x0400528D RID: 21133
			Aggressive
		}
	}
}
