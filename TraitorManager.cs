using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200014D RID: 333
	internal sealed class TraitorManager
	{
		// Token: 0x060029F0 RID: 10736 RVA: 0x001D08D0 File Offset: 0x001CEAD0
		[NullableContext(1)]
		public static void ClientRead(IReadMessage msg)
		{
			TraitorEvent.State state = (TraitorEvent.State)msg.ReadByte();
			Identifier eventIdentifier = msg.ReadIdentifier();
			GameClient client = GameMain.Client;
			if (((client != null) ? client.Character : null) == null)
			{
				DebugConsole.AddSafeError("Received a traitor update when not controlling a character.");
				return;
			}
			GameMain.Client.Character.IsTraitor = true;
		}

		// Token: 0x02000DB7 RID: 3511
		public struct TraitorResults : INetSerializableStruct
		{
			// Token: 0x0600821E RID: 33310 RVA: 0x0039A100 File Offset: 0x00398300
			[NullableContext(1)]
			public TraitorResults([Nullable(2)] Client votedAsTraitor, TraitorEvent traitorEvent)
			{
				this.VotedAsTraitorClientSessionId = ((votedAsTraitor != null) ? votedAsTraitor.SessionId : 0);
				this.VotedCorrectTraitor = (votedAsTraitor == traitorEvent.Traitor);
				if (traitorEvent.Prefab.AllowAccusingSecondaryTraitor && !this.VotedCorrectTraitor)
				{
					this.VotedCorrectTraitor = traitorEvent.SecondaryTraitors.Contains(votedAsTraitor);
				}
				this.ObjectiveSuccessful = (traitorEvent.CurrentState == TraitorEvent.State.Completed);
				this.MoneyPenalty = ((votedAsTraitor != null && !this.VotedCorrectTraitor) ? traitorEvent.Prefab.MoneyPenaltyForUnfoundedTraitorAccusation : 0);
				this.TraitorEventIdentifier = traitorEvent.Prefab.Identifier;
			}

			// Token: 0x0600821F RID: 33311 RVA: 0x0039A194 File Offset: 0x00398394
			[NullableContext(2)]
			public Client GetTraitorClient()
			{
				int sessionId = (int)this.VotedAsTraitorClientSessionId;
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember == null)
				{
					return null;
				}
				IReadOnlyList<Client> connectedClients = networkMember.ConnectedClients;
				if (connectedClients == null)
				{
					return null;
				}
				return connectedClients.FirstOrDefault((Client c) => (int)c.SessionId == sessionId);
			}

			// Token: 0x04005057 RID: 20567
			[NetworkSerialize(12)]
			public byte VotedAsTraitorClientSessionId;

			// Token: 0x04005058 RID: 20568
			[NetworkSerialize(15)]
			public bool VotedCorrectTraitor;

			// Token: 0x04005059 RID: 20569
			[NetworkSerialize(18)]
			public bool ObjectiveSuccessful;

			// Token: 0x0400505A RID: 20570
			[NetworkSerialize(21)]
			public int MoneyPenalty;

			// Token: 0x0400505B RID: 20571
			[NetworkSerialize(24)]
			public Identifier TraitorEventIdentifier;
		}
	}
}
