using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020001A7 RID: 423
	internal class MoneyAction : EventAction
	{
		// Token: 0x06001F93 RID: 8083 RVA: 0x000D8D25 File Offset: 0x000D6F25
		public MoneyAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x1700090E RID: 2318
		// (get) Token: 0x06001F94 RID: 8084 RVA: 0x000D8D2F File Offset: 0x000D6F2F
		// (set) Token: 0x06001F95 RID: 8085 RVA: 0x000D8D37 File Offset: 0x000D6F37
		[Serialize(0, IsPropertySaveable.Yes, "Amount of money to give or remove.", "", false)]
		public int Amount { get; set; }

		// Token: 0x1700090F RID: 2319
		// (get) Token: 0x06001F96 RID: 8086 RVA: 0x000D8D40 File Offset: 0x000D6F40
		// (set) Token: 0x06001F97 RID: 8087 RVA: 0x000D8D48 File Offset: 0x000D6F48
		[Serialize("", IsPropertySaveable.Yes, "If set, the money is removed from character(s) with this tag.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06001F98 RID: 8088 RVA: 0x000D8D51 File Offset: 0x000D6F51
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001F99 RID: 8089 RVA: 0x000D8D59 File Offset: 0x000D6F59
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001F9A RID: 8090 RVA: 0x000D8D64 File Offset: 0x000D6F64
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			bool hasTag = !this.TargetTag.IsEmpty;
			List<Client> matchingClients = new List<Client>();
			if (hasTag)
			{
				IEnumerable targets = this.ParentEvent.GetTargets(this.TargetTag);
				using (IEnumerator enumerator = targets.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Entity entity = (Entity)enumerator.Current;
						if (entity is Character)
						{
							GameServer server = GameMain.Server;
							Client matchingCharacter = (server != null) ? server.ConnectedClients.FirstOrDefault((Client c) => c.Character == entity) : null;
							if (matchingCharacter != null)
							{
								matchingClients.Add(matchingCharacter);
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
				if (!hasTag)
				{
					campaign.Bank.Give(this.Amount);
				}
				else
				{
					foreach (Client client in matchingClients)
					{
						campaign.GetWallet(client).Give(this.Amount);
					}
				}
				GameAnalyticsManager.AddMoneyGainedEvent(this.Amount, GameAnalyticsManager.MoneySource.Event, this.ParentEvent.Prefab.Identifier.Value);
			}
			this.isFinished = true;
		}

		// Token: 0x06001F9B RID: 8091 RVA: 0x000D8EE4 File Offset: 0x000D70E4
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

		// Token: 0x04000F0A RID: 3850
		private bool isFinished;
	}
}
