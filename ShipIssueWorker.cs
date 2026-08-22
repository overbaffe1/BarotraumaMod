using System;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000195 RID: 405
	internal abstract class ShipIssueWorker
	{
		// Token: 0x17000C3E RID: 3134
		// (get) Token: 0x06002F89 RID: 12169 RVA: 0x001F6DC0 File Offset: 0x001F4FC0
		public Order SuggestedOrder { get; }

		// Token: 0x17000C3F RID: 3135
		// (get) Token: 0x06002F8A RID: 12170 RVA: 0x001F6DC8 File Offset: 0x001F4FC8
		// (set) Token: 0x06002F8B RID: 12171 RVA: 0x001F6DD0 File Offset: 0x001F4FD0
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

		// Token: 0x17000C40 RID: 3136
		// (get) Token: 0x06002F8C RID: 12172 RVA: 0x001F6DE8 File Offset: 0x001F4FE8
		// (set) Token: 0x06002F8D RID: 12173 RVA: 0x001F6DF0 File Offset: 0x001F4FF0
		public float CurrentRedundancy { get; set; }

		// Token: 0x17000C41 RID: 3137
		// (get) Token: 0x06002F8E RID: 12174 RVA: 0x001F6DF9 File Offset: 0x001F4FF9
		public Identifier Option
		{
			get
			{
				return this.SuggestedOrder.Option;
			}
		}

		// Token: 0x17000C42 RID: 3138
		// (get) Token: 0x06002F8F RID: 12175 RVA: 0x001F6E06 File Offset: 0x001F5006
		// (set) Token: 0x06002F90 RID: 12176 RVA: 0x001F6E0E File Offset: 0x001F500E
		public Character OrderedCharacter { get; set; }

		// Token: 0x17000C43 RID: 3139
		// (get) Token: 0x06002F91 RID: 12177 RVA: 0x001F6E17 File Offset: 0x001F5017
		// (set) Token: 0x06002F92 RID: 12178 RVA: 0x001F6E1F File Offset: 0x001F501F
		public Order CurrentOrder { get; private set; }

		// Token: 0x17000C44 RID: 3140
		// (get) Token: 0x06002F93 RID: 12179 RVA: 0x001F6E28 File Offset: 0x001F5028
		public ItemComponent TargetItemComponent
		{
			get
			{
				return this.SuggestedOrder.TargetItemComponent;
			}
		}

		// Token: 0x17000C45 RID: 3141
		// (get) Token: 0x06002F94 RID: 12180 RVA: 0x001F6E35 File Offset: 0x001F5035
		public Item TargetItem
		{
			get
			{
				return this.SuggestedOrder.TargetEntity as Item;
			}
		}

		// Token: 0x17000C46 RID: 3142
		// (get) Token: 0x06002F95 RID: 12181 RVA: 0x001F6E47 File Offset: 0x001F5047
		// (set) Token: 0x06002F96 RID: 12182 RVA: 0x001F6E4F File Offset: 0x001F504F
		public bool Active { get; protected set; } = true;

		// Token: 0x17000C47 RID: 3143
		// (get) Token: 0x06002F97 RID: 12183 RVA: 0x001F6E58 File Offset: 0x001F5058
		protected virtual Character CommandingCharacter
		{
			get
			{
				return this.shipCommandManager.character;
			}
		}

		// Token: 0x17000C48 RID: 3144
		// (get) Token: 0x06002F98 RID: 12184 RVA: 0x001F6E65 File Offset: 0x001F5065
		// (set) Token: 0x06002F99 RID: 12185 RVA: 0x001F6E6D File Offset: 0x001F506D
		public virtual float TimeSinceLastAttempt { get; set; }

		// Token: 0x17000C49 RID: 3145
		// (get) Token: 0x06002F9A RID: 12186 RVA: 0x001F6E76 File Offset: 0x001F5076
		public virtual float RedundantIssueModifier
		{
			get
			{
				return 0.5f;
			}
		}

		// Token: 0x17000C4A RID: 3146
		// (get) Token: 0x06002F9B RID: 12187 RVA: 0x001F6E7D File Offset: 0x001F507D
		public virtual bool StopDuringEmergency
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000C4B RID: 3147
		// (get) Token: 0x06002F9C RID: 12188 RVA: 0x001F6E80 File Offset: 0x001F5080
		public virtual bool AllowEasySwitching
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002F9D RID: 12189 RVA: 0x001F6E83 File Offset: 0x001F5083
		public ShipIssueWorker(ShipCommandManager shipCommandManager, Order suggestedOrder)
		{
			this.shipCommandManager = shipCommandManager;
			this.SuggestedOrder = suggestedOrder;
		}

		// Token: 0x06002F9E RID: 12190 RVA: 0x001F6EA0 File Offset: 0x001F50A0
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

		// Token: 0x06002F9F RID: 12191 RVA: 0x001F7036 File Offset: 0x001F5236
		public void RemoveOrder()
		{
			this.OrderedCharacter = null;
			this.CurrentOrder = null;
		}

		// Token: 0x06002FA0 RID: 12192 RVA: 0x001F7046 File Offset: 0x001F5246
		protected virtual bool IsIssueViable()
		{
			return true;
		}

		// Token: 0x06002FA1 RID: 12193 RVA: 0x001F704C File Offset: 0x001F524C
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

		// Token: 0x06002FA2 RID: 12194 RVA: 0x001F7138 File Offset: 0x001F5338
		public bool OrderAttendedTo(float timeSinceLastCheck = 0f)
		{
			return HumanAIController.IsActive(this.OrderedCharacter) && this.CurrentOrder != null && this.OrderedCharacter.GetCurrentOrderWithTopPriority() == this.CurrentOrder && this.shipCommandManager.AbleToTakeOrder(this.OrderedCharacter);
		}

		// Token: 0x06002FA3 RID: 12195
		public abstract void CalculateImportanceSpecific();

		// Token: 0x040018C1 RID: 6337
		public const float MaxImportance = 100f;

		// Token: 0x040018C2 RID: 6338
		public const float MinImportance = 0f;

		// Token: 0x040018C4 RID: 6340
		private float importance;

		// Token: 0x040018C6 RID: 6342
		public readonly ShipCommandManager shipCommandManager;
	}
}
