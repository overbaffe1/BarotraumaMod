using System;
using System.Linq;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000155 RID: 341
	internal static class RichTextDataExtensions
	{
		// Token: 0x06002A21 RID: 10785 RVA: 0x001D19B0 File Offset: 0x001CFBB0
		public static Client ExtractClient(this RichTextData data)
		{
			ulong uintId;
			bool isInt = ulong.TryParse(data.Metadata, out uintId);
			Option<AccountId> accountId = AccountId.Parse(data.Metadata);
			Client result;
			if ((result = GameMain.Client.ConnectedClients.Find((Client c) => accountId.IsSome() && accountId == c.AccountId)) == null && (result = GameMain.Client.ConnectedClients.Find((Client c) => isInt && (ulong)c.SessionId == uintId)) == null)
			{
				result = (GameMain.Client.PreviouslyConnectedClients.FirstOrDefault((Client c) => accountId.IsSome() && accountId == c.AccountId) ?? GameMain.Client.PreviouslyConnectedClients.FirstOrDefault((Client c) => isInt && (ulong)c.SessionId == uintId));
			}
			return result;
		}
	}
}
