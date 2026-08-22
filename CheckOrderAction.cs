using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000281 RID: 641
	internal class CheckOrderAction : BinaryOptionAction
	{
		// Token: 0x17000F02 RID: 3842
		// (get) Token: 0x06003921 RID: 14625 RVA: 0x0021AB07 File Offset: 0x00218D07
		// (set) Token: 0x06003922 RID: 14626 RVA: 0x0021AB0F File Offset: 0x00218D0F
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to check.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000F03 RID: 3843
		// (get) Token: 0x06003923 RID: 14627 RVA: 0x0021AB18 File Offset: 0x00218D18
		// (set) Token: 0x06003924 RID: 14628 RVA: 0x0021AB20 File Offset: 0x00218D20
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the order the target character must have.", "", false)]
		public Identifier OrderIdentifier { get; set; }

		// Token: 0x17000F04 RID: 3844
		// (get) Token: 0x06003925 RID: 14629 RVA: 0x0021AB29 File Offset: 0x00218D29
		// (set) Token: 0x06003926 RID: 14630 RVA: 0x0021AB31 File Offset: 0x00218D31
		[Serialize("", IsPropertySaveable.Yes, "The option that must be selected for the order. If the order has multiple options (such as turning on or turning off a reactor).", "", false)]
		public Identifier OrderOption { get; set; }

		// Token: 0x17000F05 RID: 3845
		// (get) Token: 0x06003927 RID: 14631 RVA: 0x0021AB3A File Offset: 0x00218D3A
		// (set) Token: 0x06003928 RID: 14632 RVA: 0x0021AB42 File Offset: 0x00218D42
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity the order must be targeting. Only valid for orders that can target a specific entity (such as orders to operate a specific turret).", "", false)]
		public Identifier OrderTargetTag { get; set; }

		// Token: 0x17000F06 RID: 3846
		// (get) Token: 0x06003929 RID: 14633 RVA: 0x0021AB4B File Offset: 0x00218D4B
		// (set) Token: 0x0600392A RID: 14634 RVA: 0x0021AB53 File Offset: 0x00218D53
		[Serialize(CheckOrderAction.OrderPriority.Any, IsPropertySaveable.Yes, "Does the order need to have top priority, or is any priority fine?", "", false)]
		public CheckOrderAction.OrderPriority Priority { get; set; }

		// Token: 0x0600392B RID: 14635 RVA: 0x0021AB5C File Offset: 0x00218D5C
		public CheckOrderAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x0600392C RID: 14636 RVA: 0x0021AB68 File Offset: 0x00218D68
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

		// Token: 0x0600392D RID: 14637 RVA: 0x0021ACE4 File Offset: 0x00218EE4
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

		// Token: 0x0600392E RID: 14638 RVA: 0x0021AD78 File Offset: 0x00218F78
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

		// Token: 0x02000F1C RID: 3868
		public enum OrderPriority
		{
			// Token: 0x040054A9 RID: 21673
			Top,
			// Token: 0x040054AA RID: 21674
			Any
		}
	}
}
