using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000036 RID: 54
	[NullableContext(1)]
	[Nullable(0)]
	internal class ReadyCheck
	{
		// Token: 0x170001AD RID: 429
		// (get) Token: 0x060006BB RID: 1723 RVA: 0x00040A0E File Offset: 0x0003EC0E
		private static List<Client> ActivePlayers
		{
			get
			{
				return (from c in GameMain.Server.ConnectedClients
				where c != null && !c.Spectating && c.InGame
				select c).ToList<Client>();
			}
		}

		// Token: 0x060006BC RID: 1724 RVA: 0x00040A44 File Offset: 0x0003EC44
		public void InitializeReadyCheck(string author, [Nullable(2)] Client sender = null)
		{
			foreach (Client client in ReadyCheck.ActivePlayers)
			{
				if (client != null && !client.Spectating)
				{
					IWriteMessage msg = new WriteOnlyMessage();
					msg.WriteByte(27);
					msg.WriteByte(0);
					msg.WriteInt64(new DateTimeOffset(this.startTime).ToUnixTimeSeconds());
					msg.WriteInt64(new DateTimeOffset(this.endTime).ToUnixTimeSeconds());
					msg.WriteString(author);
					if (sender != null)
					{
						msg.WriteBoolean(true);
						msg.WriteByte(sender.SessionId);
					}
					else
					{
						msg.WriteBoolean(false);
					}
					msg.WriteUInt16((ushort)ReadyCheck.ActivePlayers.Count);
					foreach (byte clientId in this.Clients.Keys)
					{
						msg.WriteByte(clientId);
					}
					GameMain.Server.ServerPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
				}
			}
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x00040BA0 File Offset: 0x0003EDA0
		private void UpdateReadyCheck(byte otherClient, ReadyStatus state)
		{
			if (this.Clients.All((KeyValuePair<byte, ReadyStatus> pair) => pair.Value > ReadyStatus.Unanswered))
			{
				this.EndReadyCheck();
				return;
			}
			foreach (Client client in ReadyCheck.ActivePlayers)
			{
				IWriteMessage msg = new WriteOnlyMessage();
				msg.WriteByte(27);
				msg.WriteByte(1);
				msg.WriteByte((byte)state);
				msg.WriteByte(otherClient);
				GameMain.Server.ServerPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
			}
		}

		// Token: 0x060006BE RID: 1726 RVA: 0x00040C5C File Offset: 0x0003EE5C
		public static void ServerRead(IReadMessage inc, Client client)
		{
			ReadyCheckState state = (ReadyCheckState)inc.ReadByte();
			GameSession gameSession = GameMain.GameSession;
			ReadyCheck readyCheck2;
			if (gameSession == null)
			{
				readyCheck2 = null;
			}
			else
			{
				CrewManager crewManager = gameSession.CrewManager;
				readyCheck2 = ((crewManager != null) ? crewManager.ActiveReadyCheck : null);
			}
			ReadyCheck readyCheck = readyCheck2;
			ReadyCheckState readyCheckState = state;
			if (readyCheckState != ReadyCheckState.Start)
			{
				if (readyCheckState != ReadyCheckState.Update)
				{
					return;
				}
				if (readyCheck != null)
				{
					ReadyStatus status = (ReadyStatus)inc.ReadByte();
					if (!readyCheck.Clients.ContainsKey(client.SessionId))
					{
						return;
					}
					readyCheck.Clients[client.SessionId] = status;
					readyCheck.UpdateReadyCheck(client.SessionId, status);
				}
			}
			else if (readyCheck == null)
			{
				ReadyCheck.StartReadyCheck(client.Name, client);
				return;
			}
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x00040CE8 File Offset: 0x0003EEE8
		public static void StartReadyCheck(string author, [Nullable(2)] Client sender = null)
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.CrewManager : null) == null || GameMain.GameSession.CrewManager.ActiveReadyCheck != null)
			{
				return;
			}
			IReadOnlyList<Client> connectedClients = GameMain.Server.ConnectedClients;
			ReadyCheck newReadyCheck = new ReadyCheck((from c in connectedClients
			where !c.Spectating
			select c.SessionId).ToList<byte>(), 30f);
			GameMain.GameSession.CrewManager.ActiveReadyCheck = newReadyCheck;
			newReadyCheck.InitializeReadyCheck(author, sender);
		}

		// Token: 0x060006C0 RID: 1728 RVA: 0x00040D96 File Offset: 0x0003EF96
		public ReadyCheck(List<byte> clients, DateTime startTime, DateTime endTime) : this(clients)
		{
			this.startTime = startTime;
			this.endTime = endTime;
		}

		// Token: 0x060006C1 RID: 1729 RVA: 0x00040DAD File Offset: 0x0003EFAD
		public ReadyCheck(List<byte> clients, float duration) : this(clients)
		{
			this.startTime = DateTime.Now;
			this.endTime = this.startTime + new TimeSpan(0, 0, 0, 0, (int)(duration * 1000f));
		}

		// Token: 0x060006C2 RID: 1730 RVA: 0x00040DE4 File Offset: 0x0003EFE4
		private ReadyCheck(List<byte> clients)
		{
			this.Clients = new Dictionary<byte, ReadyStatus>();
			foreach (byte client in clients)
			{
				if (!this.Clients.ContainsKey(client))
				{
					this.Clients.Add(client, ReadyStatus.Unanswered);
				}
			}
		}

		// Token: 0x060006C3 RID: 1731 RVA: 0x00040E58 File Offset: 0x0003F058
		private void EndReadyCheck()
		{
			if (this.IsFinished)
			{
				return;
			}
			this.IsFinished = true;
			foreach (Client client in ReadyCheck.ActivePlayers)
			{
				if (client != null && !client.Spectating)
				{
					IWriteMessage msg = new WriteOnlyMessage();
					msg.WriteByte(27);
					msg.WriteByte(2);
					msg.WriteUInt16((ushort)this.Clients.Count);
					foreach (KeyValuePair<byte, ReadyStatus> keyValuePair in this.Clients)
					{
						byte b;
						ReadyStatus readyStatus;
						keyValuePair.Deconstruct(out b, out readyStatus);
						byte id = b;
						ReadyStatus state = readyStatus;
						msg.WriteByte(id);
						msg.WriteByte((byte)state);
					}
					GameMain.Server.ServerPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
				}
			}
		}

		// Token: 0x060006C4 RID: 1732 RVA: 0x00040F6C File Offset: 0x0003F16C
		public void Update(float deltaTime)
		{
			if (DateTime.Now < this.endTime)
			{
				return;
			}
			this.EndReadyCheck();
		}

		// Token: 0x0400033A RID: 826
		private readonly DateTime endTime;

		// Token: 0x0400033B RID: 827
		private readonly DateTime startTime;

		// Token: 0x0400033C RID: 828
		public readonly Dictionary<byte, ReadyStatus> Clients;

		// Token: 0x0400033D RID: 829
		public bool IsFinished;
	}
}
