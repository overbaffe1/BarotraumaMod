using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200018C RID: 396
	internal class CheckMoneyAction : BinaryOptionAction
	{
		// Token: 0x170008AC RID: 2220
		// (get) Token: 0x06001E4E RID: 7758 RVA: 0x000D53B0 File Offset: 0x000D35B0
		// (set) Token: 0x06001E4F RID: 7759 RVA: 0x000D53B8 File Offset: 0x000D35B8
		[Serialize(0, IsPropertySaveable.Yes, "Minimum amount of money the crew or the player must have.", "", false)]
		public int Amount { get; set; }

		// Token: 0x170008AD RID: 2221
		// (get) Token: 0x06001E50 RID: 7760 RVA: 0x000D53C1 File Offset: 0x000D35C1
		// (set) Token: 0x06001E51 RID: 7761 RVA: 0x000D53C9 File Offset: 0x000D35C9
		[Serialize("", IsPropertySaveable.Yes, "Tag of the player to check. If omitted, the crew's shared wallet is checked instead.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06001E52 RID: 7762 RVA: 0x000D53D2 File Offset: 0x000D35D2
		public CheckMoneyAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001E53 RID: 7763 RVA: 0x000D53DC File Offset: 0x000D35DC
		protected override bool? DetermineSuccess()
		{
			Client matchingClient = null;
			bool hasTag = !this.TargetTag.IsEmpty;
			IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.TargetTag);
			if (hasTag)
			{
				using (IEnumerator<Entity> enumerator = targets.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Entity entity = enumerator.Current;
						if (entity is Character)
						{
							GameServer server = GameMain.Server;
							Client matchingCharacter = (server != null) ? server.ConnectedClients.FirstOrDefault((Client c) => c.Character == entity) : null;
							if (matchingCharacter != null)
							{
								matchingClient = matchingCharacter;
								break;
							}
						}
					}
				}
			}
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
			if (campaign != null)
			{
				return new bool?((!hasTag) ? campaign.Bank.CanAfford(this.Amount) : campaign.GetWallet(matchingClient).CanAfford(this.Amount));
			}
			return new bool?(false);
		}

		// Token: 0x06001E54 RID: 7764 RVA: 0x000D54E4 File Offset: 0x000D36E4
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
