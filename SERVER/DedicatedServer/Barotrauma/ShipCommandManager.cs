using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200009A RID: 154
	internal class ShipCommandManager
	{
		// Token: 0x17000536 RID: 1334
		// (get) Token: 0x060012B5 RID: 4789 RVA: 0x000A4879 File Offset: 0x000A2A79
		// (set) Token: 0x060012B6 RID: 4790 RVA: 0x000A4881 File Offset: 0x000A2A81
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

		// Token: 0x17000537 RID: 1335
		// (get) Token: 0x060012B7 RID: 4791 RVA: 0x000A4895 File Offset: 0x000A2A95
		// (set) Token: 0x060012B8 RID: 4792 RVA: 0x000A489D File Offset: 0x000A2A9D
		public Submarine EnemySubmarine { get; private set; }

		// Token: 0x17000538 RID: 1336
		// (get) Token: 0x060012B9 RID: 4793 RVA: 0x000A48A6 File Offset: 0x000A2AA6
		// (set) Token: 0x060012BA RID: 4794 RVA: 0x000A48AE File Offset: 0x000A2AAE
		public Submarine CommandedSubmarine { get; private set; }

		// Token: 0x17000539 RID: 1337
		// (get) Token: 0x060012BB RID: 4795 RVA: 0x000A48B7 File Offset: 0x000A2AB7
		// (set) Token: 0x060012BC RID: 4796 RVA: 0x000A48BF File Offset: 0x000A2ABF
		public ShipCommandManager.NavigationStates NavigationState { get; private set; }

		// Token: 0x060012BD RID: 4797 RVA: 0x000A48C8 File Offset: 0x000A2AC8
		public ShipCommandManager(Character character)
		{
			this.character = character;
			this.humanAIController = (character.AIController as HumanAIController);
		}

		// Token: 0x060012BE RID: 4798 RVA: 0x000A4964 File Offset: 0x000A2B64
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

		// Token: 0x060012BF RID: 4799 RVA: 0x000A4A28 File Offset: 0x000A2C28
		public static void ShipCommandLog(string text)
		{
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.NewMessage(text, null, false);
			}
		}

		// Token: 0x060012C0 RID: 4800 RVA: 0x000A4A51 File Offset: 0x000A2C51
		private static bool WithinRange(float range, float distanceSquared)
		{
			return range * range > distanceSquared;
		}

		// Token: 0x060012C1 RID: 4801 RVA: 0x000A4A5C File Offset: 0x000A2C5C
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

		// Token: 0x060012C2 RID: 4802 RVA: 0x000A4C02 File Offset: 0x000A2E02
		public bool AbleToTakeOrder(Character character)
		{
			return !character.IsIncapacitated && !character.LockHands && character.Submarine == this.CommandedSubmarine;
		}

		// Token: 0x060012C3 RID: 4803 RVA: 0x000A4C24 File Offset: 0x000A2E24
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

		// Token: 0x060012C4 RID: 4804 RVA: 0x000A5010 File Offset: 0x000A3210
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

		// Token: 0x060012C5 RID: 4805 RVA: 0x000A5318 File Offset: 0x000A3518
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

		// Token: 0x040008D7 RID: 2263
		public readonly Character character;

		// Token: 0x040008D8 RID: 2264
		public readonly HumanAIController humanAIController;

		// Token: 0x040008D9 RID: 2265
		private bool active;

		// Token: 0x040008DC RID: 2268
		private Steering steering;

		// Token: 0x040008DD RID: 2269
		public readonly List<Vector2> patrolPositions = new List<Vector2>();

		// Token: 0x040008DF RID: 2271
		private float navigationTimer;

		// Token: 0x040008E0 RID: 2272
		private readonly float navigationInterval = 4f;

		// Token: 0x040008E1 RID: 2273
		private float timeUntilRam;

		// Token: 0x040008E2 RID: 2274
		private const float RamTimerMax = 17.5f;

		// Token: 0x040008E3 RID: 2275
		public readonly List<ShipIssueWorker> ShipIssueWorkers = new List<ShipIssueWorker>();

		// Token: 0x040008E4 RID: 2276
		public const float MinimumIssueThreshold = 10f;

		// Token: 0x040008E5 RID: 2277
		private const float IssueDevotionBuffer = 5f;

		// Token: 0x040008E6 RID: 2278
		private float decisionTimer = 6f;

		// Token: 0x040008E7 RID: 2279
		private readonly float decisionInterval = 6f;

		// Token: 0x040008E8 RID: 2280
		private float timeSinceLastCommandDecision;

		// Token: 0x040008E9 RID: 2281
		private float timeSinceLastNavigation;

		// Token: 0x040008EA RID: 2282
		public readonly List<Character> AlliedCharacters = new List<Character>();

		// Token: 0x040008EB RID: 2283
		public readonly List<Character> EnemyCharacters = new List<Character>();

		// Token: 0x040008EC RID: 2284
		private readonly List<ShipIssueWorker> attendedIssues = new List<ShipIssueWorker>();

		// Token: 0x040008ED RID: 2285
		private readonly List<ShipIssueWorker> availableIssues = new List<ShipIssueWorker>();

		// Token: 0x040008EE RID: 2286
		private readonly List<ShipGlobalIssue> shipGlobalIssues = new List<ShipGlobalIssue>();

		// Token: 0x02000844 RID: 2116
		public enum NavigationStates
		{
			// Token: 0x04002F2B RID: 12075
			Inactive,
			// Token: 0x04002F2C RID: 12076
			Patrol,
			// Token: 0x04002F2D RID: 12077
			Aggressive
		}
	}
}
