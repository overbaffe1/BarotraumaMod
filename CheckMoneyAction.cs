using System;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000280 RID: 640
	internal class CheckMoneyAction : BinaryOptionAction
	{
		// Token: 0x17000F00 RID: 3840
		// (get) Token: 0x0600391A RID: 14618 RVA: 0x0021A964 File Offset: 0x00218B64
		// (set) Token: 0x0600391B RID: 14619 RVA: 0x0021A96C File Offset: 0x00218B6C
		[Serialize(0, IsPropertySaveable.Yes, "Minimum amount of money the crew or the player must have.", "", false)]
		public int Amount { get; set; }

		// Token: 0x17000F01 RID: 3841
		// (get) Token: 0x0600391C RID: 14620 RVA: 0x0021A975 File Offset: 0x00218B75
		// (set) Token: 0x0600391D RID: 14621 RVA: 0x0021A97D File Offset: 0x00218B7D
		[Serialize("", IsPropertySaveable.Yes, "Tag of the player to check. If omitted, the crew's shared wallet is checked instead.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x0600391E RID: 14622 RVA: 0x0021A986 File Offset: 0x00218B86
		public CheckMoneyAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x0600391F RID: 14623 RVA: 0x0021A990 File Offset: 0x00218B90
		protected override bool? DetermineSuccess()
		{
			Client matchingClient = null;
			bool hasTag = !this.TargetTag.IsEmpty;
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
			if (campaign != null)
			{
				return new bool?((!hasTag) ? campaign.Bank.CanAfford(this.Amount) : campaign.GetWallet(matchingClient).CanAfford(this.Amount));
			}
			return new bool?(false);
		}

		// Token: 0x06003920 RID: 14624 RVA: 0x0021AA00 File Offset: 0x00218C00
		public override string ToDebugString()
		{
			string subActionStr = "";
			if (this.succeeded != null)
			{
				string str = "\n            Sub action: ";
				EventAction.SubactionGroup subactionGroup = this.succeeded.Value ? this.Success : this.Failure;
				subActionStr = str + ((subactionGroup != null) ? subactionGroup.CurrentSubAction.ColorizeObject() : null);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(base.DetermineFinished(), false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CheckMoneyAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Amount: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Amount.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(" Succeeded: ");
			defaultInterpolatedStringHandler.AppendFormatted(((this.succeeded != null) ? this.succeeded.Value.ToString() : "not determined").ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear() + subActionStr;
		}
	}
}
