using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x0200018E RID: 398
	internal class CheckOrderAction : BinaryOptionAction
	{
		// Token: 0x170008AE RID: 2222
		// (get) Token: 0x06001E57 RID: 7767 RVA: 0x000D560D File Offset: 0x000D380D
		// (set) Token: 0x06001E58 RID: 7768 RVA: 0x000D5615 File Offset: 0x000D3815
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to check.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170008AF RID: 2223
		// (get) Token: 0x06001E59 RID: 7769 RVA: 0x000D561E File Offset: 0x000D381E
		// (set) Token: 0x06001E5A RID: 7770 RVA: 0x000D5626 File Offset: 0x000D3826
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the order the target character must have.", "", false)]
		public Identifier OrderIdentifier { get; set; }

		// Token: 0x170008B0 RID: 2224
		// (get) Token: 0x06001E5B RID: 7771 RVA: 0x000D562F File Offset: 0x000D382F
		// (set) Token: 0x06001E5C RID: 7772 RVA: 0x000D5637 File Offset: 0x000D3837
		[Serialize("", IsPropertySaveable.Yes, "The option that must be selected for the order. If the order has multiple options (such as turning on or turning off a reactor).", "", false)]
		public Identifier OrderOption { get; set; }

		// Token: 0x170008B1 RID: 2225
		// (get) Token: 0x06001E5D RID: 7773 RVA: 0x000D5640 File Offset: 0x000D3840
		// (set) Token: 0x06001E5E RID: 7774 RVA: 0x000D5648 File Offset: 0x000D3848
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity the order must be targeting. Only valid for orders that can target a specific entity (such as orders to operate a specific turret).", "", false)]
		public Identifier OrderTargetTag { get; set; }

		// Token: 0x170008B2 RID: 2226
		// (get) Token: 0x06001E5F RID: 7775 RVA: 0x000D5651 File Offset: 0x000D3851
		// (set) Token: 0x06001E60 RID: 7776 RVA: 0x000D5659 File Offset: 0x000D3859
		[Serialize(CheckOrderAction.OrderPriority.Any, IsPropertySaveable.Yes, "Does the order need to have top priority, or is any priority fine?", "", false)]
		public CheckOrderAction.OrderPriority Priority { get; set; }

		// Token: 0x06001E61 RID: 7777 RVA: 0x000D5662 File Offset: 0x000D3862
		public CheckOrderAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001E62 RID: 7778 RVA: 0x000D566C File Offset: 0x000D386C
		protected override bool? DetermineSuccess()
		{
			IEnumerable<Entity> targetCharacters = this.ParentEvent.GetTargets(this.TargetTag);
			if (targetCharacters.None(null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(157, 2);
				defaultInterpolatedStringHandler.AppendLiteral("CheckConditionalAction error: ");
				defaultInterpolatedStringHandler.AppendFormatted(this.GetEventName());
				defaultInterpolatedStringHandler.AppendLiteral(" uses a CheckOrderAction but no valid target characters were found for tag \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.TargetTag);
				defaultInterpolatedStringHandler.AppendLiteral("\"! This will cause the check to automatically fail.");
				string msg = defaultInterpolatedStringHandler.ToStringAndClear();
				ContentPackage contentPackage = this.ParentEvent.Prefab.ContentPackage;
				DebugConsole.LogError(msg, null, contentPackage);
				return new bool?(false);
			}
			foreach (Entity t in targetCharacters)
			{
				Character c = t as Character;
				if (c != null)
				{
					if (this.Priority == CheckOrderAction.OrderPriority.Top)
					{
						Order topPrioOrder = c.GetCurrentOrderWithTopPriority();
						if (topPrioOrder != null && this.<DetermineSuccess>g__IsMatch|22_0(topPrioOrder))
						{
							return new bool?(true);
						}
					}
					else if (this.Priority == CheckOrderAction.OrderPriority.Any)
					{
						foreach (Order order in c.CurrentOrders)
						{
							if (this.<DetermineSuccess>g__IsMatch|22_0(order))
							{
								return new bool?(true);
							}
						}
					}
				}
			}
			return new bool?(false);
		}

		// Token: 0x06001E63 RID: 7779 RVA: 0x000D57E8 File Offset: 0x000D39E8
		private string GetEventName()
		{
			ScriptedEvent parentEvent = this.ParentEvent;
			Identifier? identifier2;
			if (parentEvent == null)
			{
				identifier2 = null;
			}
			else
			{
				EventPrefab prefab = parentEvent.Prefab;
				identifier2 = ((prefab != null) ? new Identifier?(prefab.Identifier) : null);
			}
			Identifier? identifier3 = identifier2;
			if (identifier3 != null)
			{
				Identifier identifier = identifier3.GetValueOrDefault();
				if (!identifier.IsEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler.AppendLiteral("the event \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
			}
			return "an unknown event";
		}

		// Token: 0x06001E64 RID: 7780 RVA: 0x000D587C File Offset: 0x000D3A7C
		[CompilerGenerated]
		private bool <DetermineSuccess>g__IsMatch|22_0(Order order)
		{
			Identifier? identifier;
			Identifier? identifier2;
			if (order == null)
			{
				identifier = null;
				identifier2 = identifier;
			}
			else
			{
				identifier2 = new Identifier?(order.Identifier);
			}
			identifier = identifier2;
			Identifier? identifier3 = new Identifier?(this.OrderIdentifier);
			if (identifier == identifier3)
			{
				if (!this.OrderTargetTag.IsEmpty)
				{
					Item targetItem = order.TargetEntity as Item;
					if (targetItem == null || !targetItem.HasTag(this.OrderTargetTag))
					{
						return false;
					}
				}
				if (!this.OrderOption.IsEmpty)
				{
					Identifier? identifier4;
					if (order == null)
					{
						identifier = null;
						identifier4 = identifier;
					}
					else
					{
						identifier4 = new Identifier?(order.Option);
					}
					identifier = identifier4;
					identifier3 = new Identifier?(this.OrderOption);
					if (!(identifier == identifier3))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x0200090B RID: 2315
		public enum OrderPriority
		{
			// Token: 0x040031DC RID: 12764
			Top,
			// Token: 0x040031DD RID: 12765
			Any
		}
	}
}
