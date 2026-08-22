using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000299 RID: 665
	internal class MoneyAction : EventAction
	{
		// Token: 0x06003A3F RID: 14911 RVA: 0x0021E955 File Offset: 0x0021CB55
		public MoneyAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x17000F54 RID: 3924
		// (get) Token: 0x06003A40 RID: 14912 RVA: 0x0021E95F File Offset: 0x0021CB5F
		// (set) Token: 0x06003A41 RID: 14913 RVA: 0x0021E967 File Offset: 0x0021CB67
		[Serialize(0, IsPropertySaveable.Yes, "Amount of money to give or remove.", "", false)]
		public int Amount { get; set; }

		// Token: 0x17000F55 RID: 3925
		// (get) Token: 0x06003A42 RID: 14914 RVA: 0x0021E970 File Offset: 0x0021CB70
		// (set) Token: 0x06003A43 RID: 14915 RVA: 0x0021E978 File Offset: 0x0021CB78
		[Serialize("", IsPropertySaveable.Yes, "If set, the money is removed from character(s) with this tag.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06003A44 RID: 14916 RVA: 0x0021E981 File Offset: 0x0021CB81
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003A45 RID: 14917 RVA: 0x0021E989 File Offset: 0x0021CB89
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003A46 RID: 14918 RVA: 0x0021E994 File Offset: 0x0021CB94
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
			if (campaign != null)
			{
				campaign.Wallet.Give(this.Amount);
				GameAnalyticsManager.AddMoneyGainedEvent(this.Amount, GameAnalyticsManager.MoneySource.Event, this.ParentEvent.Prefab.Identifier.Value);
			}
			this.isFinished = true;
		}

		// Token: 0x06003A47 RID: 14919 RVA: 0x0021EA00 File Offset: 0x0021CC00
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("SetDataAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Amount: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Amount.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001DF3 RID: 7667
		private bool isFinished;
	}
}
