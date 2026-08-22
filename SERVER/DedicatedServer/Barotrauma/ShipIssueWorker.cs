using System;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200008F RID: 143
	internal abstract class ShipIssueWorker
	{
		// Token: 0x17000524 RID: 1316
		// (get) Token: 0x06001281 RID: 4737 RVA: 0x000A3EE0 File Offset: 0x000A20E0
		public Order SuggestedOrder { get; }

		// Token: 0x17000525 RID: 1317
		// (get) Token: 0x06001282 RID: 4738 RVA: 0x000A3EE8 File Offset: 0x000A20E8
		// (set) Token: 0x06001283 RID: 4739 RVA: 0x000A3EF0 File Offset: 0x000A20F0
		public float Importance
		{
			get
			{
				return this.importance;
			}
			set
			{
				this.importance = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000526 RID: 1318
		// (get) Token: 0x06001284 RID: 4740 RVA: 0x000A3F08 File Offset: 0x000A2108
		// (set) Token: 0x06001285 RID: 4741 RVA: 0x000A3F10 File Offset: 0x000A2110
		public float CurrentRedundancy { get; set; }

		// Token: 0x17000527 RID: 1319
		// (get) Token: 0x06001286 RID: 4742 RVA: 0x000A3F19 File Offset: 0x000A2119
		public Identifier Option
		{
			get
			{
				return this.SuggestedOrder.Option;
			}
		}

		// Token: 0x17000528 RID: 1320
		// (get) Token: 0x06001287 RID: 4743 RVA: 0x000A3F26 File Offset: 0x000A2126
		// (set) Token: 0x06001288 RID: 4744 RVA: 0x000A3F2E File Offset: 0x000A212E
		public Character OrderedCharacter { get; set; }

		// Token: 0x17000529 RID: 1321
		// (get) Token: 0x06001289 RID: 4745 RVA: 0x000A3F37 File Offset: 0x000A2137
		// (set) Token: 0x0600128A RID: 4746 RVA: 0x000A3F3F File Offset: 0x000A213F
		public Order CurrentOrder { get; private set; }

		// Token: 0x1700052A RID: 1322
		// (get) Token: 0x0600128B RID: 4747 RVA: 0x000A3F48 File Offset: 0x000A2148
		public ItemComponent TargetItemComponent
		{
			get
			{
				return this.SuggestedOrder.TargetItemComponent;
			}
		}

		// Token: 0x1700052B RID: 1323
		// (get) Token: 0x0600128C RID: 4748 RVA: 0x000A3F55 File Offset: 0x000A2155
		public Item TargetItem
		{
			get
			{
				return this.SuggestedOrder.TargetEntity as Item;
			}
		}

		// Token: 0x1700052C RID: 1324
		// (get) Token: 0x0600128D RID: 4749 RVA: 0x000A3F67 File Offset: 0x000A2167
		// (set) Token: 0x0600128E RID: 4750 RVA: 0x000A3F6F File Offset: 0x000A216F
		public bool Active { get; protected set; } = true;

		// Token: 0x1700052D RID: 1325
		// (get) Token: 0x0600128F RID: 4751 RVA: 0x000A3F78 File Offset: 0x000A2178
		protected virtual Character CommandingCharacter
		{
			get
			{
				return this.shipCommandManager.character;
			}
		}

		// Token: 0x1700052E RID: 1326
		// (get) Token: 0x06001290 RID: 4752 RVA: 0x000A3F85 File Offset: 0x000A2185
		// (set) Token: 0x06001291 RID: 4753 RVA: 0x000A3F8D File Offset: 0x000A218D
		public virtual float TimeSinceLastAttempt { get; set; }

		// Token: 0x1700052F RID: 1327
		// (get) Token: 0x06001292 RID: 4754 RVA: 0x000A3F96 File Offset: 0x000A2196
		public virtual float RedundantIssueModifier
		{
			get
			{
				return 0.5f;
			}
		}

		// Token: 0x17000530 RID: 1328
		// (get) Token: 0x06001293 RID: 4755 RVA: 0x000A3F9D File Offset: 0x000A219D
		public virtual bool StopDuringEmergency
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000531 RID: 1329
		// (get) Token: 0x06001294 RID: 4756 RVA: 0x000A3FA0 File Offset: 0x000A21A0
		public virtual bool AllowEasySwitching
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001295 RID: 4757 RVA: 0x000A3FA3 File Offset: 0x000A21A3
		public ShipIssueWorker(ShipCommandManager shipCommandManager, Order suggestedOrder)
		{
			this.shipCommandManager = shipCommandManager;
			this.SuggestedOrder = suggestedOrder;
		}

		// Token: 0x06001296 RID: 4758 RVA: 0x000A3FC0 File Offset: 0x000A21C0
		public void SetOrder(Character orderedCharacter)
		{
			this.OrderedCharacter = orderedCharacter;
			HumanAIController humanAI = this.OrderedCharacter.AIController as HumanAIController;
			if (humanAI != null && humanAI.ObjectiveManager.CurrentOrders.None((Order o) => o.MatchesOrder(this.SuggestedOrder.Identifier, this.Option) && o.TargetEntity == this.TargetItem))
			{
				bool orderGivenByDifferentCharacter = orderedCharacter != this.CommandingCharacter;
				if (orderGivenByDifferentCharacter)
				{
					Character commandingCharacter = this.CommandingCharacter;
					string chatMessage = this.SuggestedOrder.GetChatMessage(this.OrderedCharacter.Name, "", false, default(Identifier), true);
					Identifier identifier = ("GiveOrder." + this.SuggestedOrder.Prefab.Identifier.ToString()).ToIdentifier();
					commandingCharacter.Speak(chatMessage, null, 0f, identifier, 5f);
				}
				this.CurrentOrder = this.SuggestedOrder.WithOption(this.Option).WithItemComponent(this.TargetItem, this.TargetItemComponent).WithOrderGiver(this.CommandingCharacter).WithManualPriority(CharacterInfo.HighestManualOrderPriority);
				this.OrderedCharacter.SetOrder(this.CurrentOrder, this.CommandingCharacter != this.OrderedCharacter, true, false);
				if (orderGivenByDifferentCharacter)
				{
					Character orderedCharacter2 = this.OrderedCharacter;
					string value = TextManager.Get("DialogAffirmative").Value;
					Identifier identifier = ("ReceiveOrder." + this.SuggestedOrder.Prefab.Identifier.ToString()).ToIdentifier();
					orderedCharacter2.Speak(value, null, 1f, identifier, 5f);
				}
			}
			this.TimeSinceLastAttempt = 0f;
		}

		// Token: 0x06001297 RID: 4759 RVA: 0x000A4156 File Offset: 0x000A2356
		public void RemoveOrder()
		{
			this.OrderedCharacter = null;
			this.CurrentOrder = null;
		}

		// Token: 0x06001298 RID: 4760 RVA: 0x000A4166 File Offset: 0x000A2366
		protected virtual bool IsIssueViable()
		{
			return true;
		}

		// Token: 0x06001299 RID: 4761 RVA: 0x000A416C File Offset: 0x000A236C
		public float CalculateImportance(bool isEmergency)
		{
			this.Importance = 0f;
			if (!this.Active)
			{
				return this.Importance;
			}
			this.Active = this.IsIssueViable();
			if (isEmergency && this.StopDuringEmergency)
			{
				return this.Importance;
			}
			this.CalculateImportanceSpecific();
			this.CurrentRedundancy = 1f;
			foreach (ShipIssueWorker shipIssueWorker in this.shipCommandManager.ShipIssueWorkers)
			{
				if (shipIssueWorker.GetType() == base.GetType() && shipIssueWorker != this && shipIssueWorker.OrderAttendedTo(0f))
				{
					this.CurrentRedundancy *= this.RedundantIssueModifier;
				}
			}
			this.Importance *= this.CurrentRedundancy;
			return this.Importance;
		}

		// Token: 0x0600129A RID: 4762 RVA: 0x000A4258 File Offset: 0x000A2458
		public bool OrderAttendedTo(float timeSinceLastCheck = 0f)
		{
			return HumanAIController.IsActive(this.OrderedCharacter) && this.CurrentOrder != null && this.OrderedCharacter.GetCurrentOrderWithTopPriority() == this.CurrentOrder && this.shipCommandManager.AbleToTakeOrder(this.OrderedCharacter);
		}

		// Token: 0x0600129B RID: 4763
		public abstract void CalculateImportanceSpecific();

		// Token: 0x040008C7 RID: 2247
		public const float MaxImportance = 100f;

		// Token: 0x040008C8 RID: 2248
		public const float MinImportance = 0f;

		// Token: 0x040008CA RID: 2250
		private float importance;

		// Token: 0x040008CC RID: 2252
		public readonly ShipCommandManager shipCommandManager;
	}
}
