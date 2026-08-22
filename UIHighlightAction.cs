using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200004B RID: 75
	internal class UIHighlightAction : EventAction
	{
		// Token: 0x17000305 RID: 773
		// (get) Token: 0x06000AD0 RID: 2768 RVA: 0x00062B11 File Offset: 0x00060D11
		// (set) Token: 0x06000AD1 RID: 2769 RVA: 0x00062B19 File Offset: 0x00060D19
		[Serialize(UIHighlightAction.ElementId.None, IsPropertySaveable.Yes, "An arbitrary identifier that must match the userdata of the UI element. The userdatas of the element are hard-coded, so this option is generally intended for the developers' use.", "", false)]
		public UIHighlightAction.ElementId Id { get; set; }

		// Token: 0x17000306 RID: 774
		// (get) Token: 0x06000AD2 RID: 2770 RVA: 0x00062B22 File Offset: 0x00060D22
		// (set) Token: 0x06000AD3 RID: 2771 RVA: 0x00062B2A File Offset: 0x00060D2A
		[Serialize("", IsPropertySaveable.Yes, "If the element's userdata is an entity or an entity prefab, it's identifier must match this value.", "", false)]
		public Identifier EntityIdentifier { get; set; }

		// Token: 0x17000307 RID: 775
		// (get) Token: 0x06000AD4 RID: 2772 RVA: 0x00062B33 File Offset: 0x00060D33
		// (set) Token: 0x06000AD5 RID: 2773 RVA: 0x00062B3B File Offset: 0x00060D3B
		[Serialize(OrderCategory.Emergency, IsPropertySaveable.Yes, "If the element's userdata is an order category, it must match this.", "", false)]
		public OrderCategory OrderCategory { get; set; }

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06000AD6 RID: 2774 RVA: 0x00062B44 File Offset: 0x00060D44
		// (set) Token: 0x06000AD7 RID: 2775 RVA: 0x00062B4C File Offset: 0x00060D4C
		[Serialize("", IsPropertySaveable.Yes, "If the element's userdata is an order, it must match this identifier.", "", false)]
		public Identifier OrderIdentifier { get; set; }

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x06000AD8 RID: 2776 RVA: 0x00062B55 File Offset: 0x00060D55
		// (set) Token: 0x06000AD9 RID: 2777 RVA: 0x00062B5D File Offset: 0x00060D5D
		[Serialize("", IsPropertySaveable.Yes, "If the element's userdata is an order with options, it must match this.", "", false)]
		public Identifier OrderOption { get; set; }

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x06000ADA RID: 2778 RVA: 0x00062B66 File Offset: 0x00060D66
		// (set) Token: 0x06000ADB RID: 2779 RVA: 0x00062B6E File Offset: 0x00060D6E
		[Serialize("", IsPropertySaveable.Yes, "If the element's userdata is an order, the order must target an entity with this tag.", "", false)]
		public Identifier OrderTargetTag { get; set; }

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x06000ADC RID: 2780 RVA: 0x00062B77 File Offset: 0x00060D77
		// (set) Token: 0x06000ADD RID: 2781 RVA: 0x00062B7F File Offset: 0x00060D7F
		[Serialize(true, IsPropertySaveable.Yes, "Should the element bounce up an down in addition to being highlighted.", "", false)]
		public bool Bounce { get; set; }

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x06000ADE RID: 2782 RVA: 0x00062B88 File Offset: 0x00060D88
		// (set) Token: 0x06000ADF RID: 2783 RVA: 0x00062B90 File Offset: 0x00060D90
		[Serialize(false, IsPropertySaveable.Yes, "Should the action highlight the first matching element it finds, or all of them?", "", false)]
		public bool HighlightMultiple { get; set; }

		// Token: 0x06000AE0 RID: 2784 RVA: 0x00062B99 File Offset: 0x00060D99
		public UIHighlightAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06000AE1 RID: 2785 RVA: 0x00062BA3 File Offset: 0x00060DA3
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.UpdateProjSpecific();
			this.isFinished = true;
		}

		// Token: 0x06000AE2 RID: 2786 RVA: 0x00062BBC File Offset: 0x00060DBC
		private void UpdateProjSpecific()
		{
			UIHighlightAction.<>c__DisplayClass37_0 CS$<>8__locals1 = new UIHighlightAction.<>c__DisplayClass37_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.useCircularFlash = false;
			if (this.Id != UIHighlightAction.ElementId.None)
			{
				Func<GUIComponent, bool> predicate = (GUIComponent c) => c != null && object.Equals(CS$<>8__locals1.<>4__this.Id, c.UserData);
				if (!CS$<>8__locals1.<UpdateProjSpecific>g__FindAndFlashAddedComponents|2(new Func<GUIComponent, bool>[]
				{
					predicate
				}))
				{
					if (predicate(GUIMessageBox.VisibleBox))
					{
						CS$<>8__locals1.<UpdateProjSpecific>g__Flash|4(GUIMessageBox.VisibleBox);
						return;
					}
					CS$<>8__locals1.<UpdateProjSpecific>g__FindAndFlashMessageBoxComponents|3(new Func<GUIComponent, bool>[]
					{
						predicate
					});
					return;
				}
			}
			else
			{
				if (!this.EntityIdentifier.IsEmpty)
				{
					CS$<>8__locals1.<UpdateProjSpecific>g__FindAndFlashAddedComponents|2(new Func<GUIComponent, bool>[]
					{
						delegate(GUIComponent c)
						{
							MapEntityPrefab mep = c.UserData as MapEntityPrefab;
							if (mep != null)
							{
								Prefab prefab = mep;
								Identifier entityIdentifier = CS$<>8__locals1.<>4__this.EntityIdentifier;
								if (prefab.Identifier == entityIdentifier)
								{
									return true;
								}
							}
							MapEntity me = c.UserData as MapEntity;
							if (me != null)
							{
								Prefab prefab2 = me.Prefab;
								Identifier entityIdentifier2 = CS$<>8__locals1.<>4__this.EntityIdentifier;
								return prefab2.Identifier == entityIdentifier2;
							}
							return false;
						}
					});
					return;
				}
				if (!this.OrderIdentifier.IsEmpty)
				{
					CS$<>8__locals1.useCircularFlash = true;
					bool foundMinimapNode = false;
					if (!this.OrderTargetTag.IsEmpty)
					{
						foundMinimapNode = CS$<>8__locals1.<UpdateProjSpecific>g__FindAndFlashAddedComponents|2(new Func<GUIComponent, bool>[]
						{
							delegate(GUIComponent c)
							{
								object userData = c.UserData;
								if (userData is CrewManager.MinimapNodeData)
								{
									CrewManager.MinimapNodeData nodeData = (CrewManager.MinimapNodeData)userData;
									Order order = nodeData.Order;
									if (order != null)
									{
										Identifier identifier = order.Identifier;
										Identifier orderIdentifier = CS$<>8__locals1.<>4__this.OrderIdentifier;
										if (identifier == orderIdentifier)
										{
											Order order2 = order;
											Identifier orderOption = CS$<>8__locals1.<>4__this.OrderOption;
											if (order2.Option == orderOption)
											{
												Item item = order.TargetEntity as Item;
												if (item != null)
												{
													return item.HasTag(CS$<>8__locals1.<>4__this.OrderTargetTag);
												}
											}
										}
									}
								}
								return false;
							}
						});
					}
					if (!foundMinimapNode)
					{
						CS$<>8__locals1.<UpdateProjSpecific>g__FindAndFlashAddedComponents|2(new Func<GUIComponent, bool>[]
						{
							delegate(GUIComponent c)
							{
								Order order = c.UserData as Order;
								if (order != null)
								{
									Identifier identifier = order.Identifier;
									Identifier orderIdentifier = CS$<>8__locals1.<>4__this.OrderIdentifier;
									if (identifier == orderIdentifier)
									{
										Order order2 = order;
										Identifier orderOption = CS$<>8__locals1.<>4__this.OrderOption;
										return order2.Option == orderOption;
									}
								}
								return false;
							},
							delegate(GUIComponent c)
							{
								Order order = c.UserData as Order;
								if (order != null)
								{
									Identifier identifier = order.Identifier;
									Identifier orderIdentifier = CS$<>8__locals1.<>4__this.OrderIdentifier;
									return identifier == orderIdentifier;
								}
								return false;
							},
							(GUIComponent c) => object.Equals(CS$<>8__locals1.<>4__this.OrderCategory, c.UserData)
						});
					}
				}
			}
		}

		// Token: 0x06000AE3 RID: 2787 RVA: 0x00062CE1 File Offset: 0x00060EE1
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06000AE4 RID: 2788 RVA: 0x00062CE9 File Offset: 0x00060EE9
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04000581 RID: 1409
		private static readonly Color highlightColor = Color.Orange;

		// Token: 0x0400058A RID: 1418
		private bool isFinished;

		// Token: 0x020007A2 RID: 1954
		public enum ElementId
		{
			// Token: 0x04003B3B RID: 15163
			None,
			// Token: 0x04003B3C RID: 15164
			RepairButton,
			// Token: 0x04003B3D RID: 15165
			PumpSpeedSlider,
			// Token: 0x04003B3E RID: 15166
			PassiveSonarIndicator,
			// Token: 0x04003B3F RID: 15167
			ActiveSonarIndicator,
			// Token: 0x04003B40 RID: 15168
			SonarModeSwitch,
			// Token: 0x04003B41 RID: 15169
			DirectionalSonarFrame,
			// Token: 0x04003B42 RID: 15170
			SteeringModeSwitch,
			// Token: 0x04003B43 RID: 15171
			MaintainPosTickBox,
			// Token: 0x04003B44 RID: 15172
			AutoTempSwitch,
			// Token: 0x04003B45 RID: 15173
			PowerButton,
			// Token: 0x04003B46 RID: 15174
			FissionRateSlider,
			// Token: 0x04003B47 RID: 15175
			TurbineOutputSlider,
			// Token: 0x04003B48 RID: 15176
			DeconstructButton,
			// Token: 0x04003B49 RID: 15177
			RechargeSpeedSlider,
			// Token: 0x04003B4A RID: 15178
			CPRButton,
			// Token: 0x04003B4B RID: 15179
			CloseButton,
			// Token: 0x04003B4C RID: 15180
			MessageBoxCloseButton
		}
	}
}
