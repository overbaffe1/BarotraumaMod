using System;

namespace Barotrauma
{
	// Token: 0x020001BB RID: 443
	internal class UIHighlightAction : EventAction
	{
		// Token: 0x17000971 RID: 2417
		// (get) Token: 0x060020FE RID: 8446 RVA: 0x000DE569 File Offset: 0x000DC769
		// (set) Token: 0x060020FF RID: 8447 RVA: 0x000DE571 File Offset: 0x000DC771
		[Serialize(UIHighlightAction.ElementId.None, IsPropertySaveable.Yes, "An arbitrary identifier that must match the userdata of the UI element. The userdatas of the element are hard-coded, so this option is generally intended for the developers' use.", "", false)]
		public UIHighlightAction.ElementId Id { get; set; }

		// Token: 0x17000972 RID: 2418
		// (get) Token: 0x06002100 RID: 8448 RVA: 0x000DE57A File Offset: 0x000DC77A
		// (set) Token: 0x06002101 RID: 8449 RVA: 0x000DE582 File Offset: 0x000DC782
		[Serialize("", IsPropertySaveable.Yes, "If the element's userdata is an entity or an entity prefab, it's identifier must match this value.", "", false)]
		public Identifier EntityIdentifier { get; set; }

		// Token: 0x17000973 RID: 2419
		// (get) Token: 0x06002102 RID: 8450 RVA: 0x000DE58B File Offset: 0x000DC78B
		// (set) Token: 0x06002103 RID: 8451 RVA: 0x000DE593 File Offset: 0x000DC793
		[Serialize(OrderCategory.Emergency, IsPropertySaveable.Yes, "If the element's userdata is an order category, it must match this.", "", false)]
		public OrderCategory OrderCategory { get; set; }

		// Token: 0x17000974 RID: 2420
		// (get) Token: 0x06002104 RID: 8452 RVA: 0x000DE59C File Offset: 0x000DC79C
		// (set) Token: 0x06002105 RID: 8453 RVA: 0x000DE5A4 File Offset: 0x000DC7A4
		[Serialize("", IsPropertySaveable.Yes, "If the element's userdata is an order, it must match this identifier.", "", false)]
		public Identifier OrderIdentifier { get; set; }

		// Token: 0x17000975 RID: 2421
		// (get) Token: 0x06002106 RID: 8454 RVA: 0x000DE5AD File Offset: 0x000DC7AD
		// (set) Token: 0x06002107 RID: 8455 RVA: 0x000DE5B5 File Offset: 0x000DC7B5
		[Serialize("", IsPropertySaveable.Yes, "If the element's userdata is an order with options, it must match this.", "", false)]
		public Identifier OrderOption { get; set; }

		// Token: 0x17000976 RID: 2422
		// (get) Token: 0x06002108 RID: 8456 RVA: 0x000DE5BE File Offset: 0x000DC7BE
		// (set) Token: 0x06002109 RID: 8457 RVA: 0x000DE5C6 File Offset: 0x000DC7C6
		[Serialize("", IsPropertySaveable.Yes, "If the element's userdata is an order, the order must target an entity with this tag.", "", false)]
		public Identifier OrderTargetTag { get; set; }

		// Token: 0x17000977 RID: 2423
		// (get) Token: 0x0600210A RID: 8458 RVA: 0x000DE5CF File Offset: 0x000DC7CF
		// (set) Token: 0x0600210B RID: 8459 RVA: 0x000DE5D7 File Offset: 0x000DC7D7
		[Serialize(true, IsPropertySaveable.Yes, "Should the element bounce up an down in addition to being highlighted.", "", false)]
		public bool Bounce { get; set; }

		// Token: 0x17000978 RID: 2424
		// (get) Token: 0x0600210C RID: 8460 RVA: 0x000DE5E0 File Offset: 0x000DC7E0
		// (set) Token: 0x0600210D RID: 8461 RVA: 0x000DE5E8 File Offset: 0x000DC7E8
		[Serialize(false, IsPropertySaveable.Yes, "Should the action highlight the first matching element it finds, or all of them?", "", false)]
		public bool HighlightMultiple { get; set; }

		// Token: 0x0600210E RID: 8462 RVA: 0x000DE5F1 File Offset: 0x000DC7F1
		public UIHighlightAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x0600210F RID: 8463 RVA: 0x000DE5FB File Offset: 0x000DC7FB
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.isFinished = true;
		}

		// Token: 0x06002110 RID: 8464 RVA: 0x000DE60D File Offset: 0x000DC80D
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06002111 RID: 8465 RVA: 0x000DE615 File Offset: 0x000DC815
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04000F99 RID: 3993
		private bool isFinished;

		// Token: 0x02000940 RID: 2368
		public enum ElementId
		{
			// Token: 0x04003284 RID: 12932
			None,
			// Token: 0x04003285 RID: 12933
			RepairButton,
			// Token: 0x04003286 RID: 12934
			PumpSpeedSlider,
			// Token: 0x04003287 RID: 12935
			PassiveSonarIndicator,
			// Token: 0x04003288 RID: 12936
			ActiveSonarIndicator,
			// Token: 0x04003289 RID: 12937
			SonarModeSwitch,
			// Token: 0x0400328A RID: 12938
			DirectionalSonarFrame,
			// Token: 0x0400328B RID: 12939
			SteeringModeSwitch,
			// Token: 0x0400328C RID: 12940
			MaintainPosTickBox,
			// Token: 0x0400328D RID: 12941
			AutoTempSwitch,
			// Token: 0x0400328E RID: 12942
			PowerButton,
			// Token: 0x0400328F RID: 12943
			FissionRateSlider,
			// Token: 0x04003290 RID: 12944
			TurbineOutputSlider,
			// Token: 0x04003291 RID: 12945
			DeconstructButton,
			// Token: 0x04003292 RID: 12946
			RechargeSpeedSlider,
			// Token: 0x04003293 RID: 12947
			CPRButton,
			// Token: 0x04003294 RID: 12948
			CloseButton,
			// Token: 0x04003295 RID: 12949
			MessageBoxCloseButton
		}
	}
}
