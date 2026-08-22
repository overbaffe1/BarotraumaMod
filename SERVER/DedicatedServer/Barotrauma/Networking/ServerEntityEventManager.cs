using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x0200036F RID: 879
	internal class ServerEntityEventManager : NetEntityEventManager
	{
		// Token: 0x17000E7D RID: 3709
		// (get) Token: 0x0600346A RID: 13418 RVA: 0x00168BBB File Offset: 0x00166DBB
		public List<ServerEntityEvent> Events
		{
			get
			{
				return this.events;
			}
		}

		// Token: 0x17000E7E RID: 3710
		// (get) Token: 0x0600346B RID: 13419 RVA: 0x00168BC3 File Offset: 0x00166DC3
		public List<ServerEntityEvent> UniqueEvents
		{
			get
			{
				return this.uniqueEvents;
			}
		}

		// Token: 0x0600346C RID: 13420 RVA: 0x00168BCB File Offset: 0x00166DCB
		public ServerEntityEventManager(GameServer server)
		{
			this.events = new List<ServerEntityEvent>();
			this.server = server;
			this.bufferedEvents = new List<ServerEntityEventManager.BufferedEvent>();
			this.uniqueEvents = new List<ServerEntityEvent>();
			this.lastWarningTime = -10.0;
		}

		// Token: 0x0600346D RID: 13421 RVA: 0x00168C0C File Offset: 0x00166E0C
		public void CreateEvent(IServerSerializable entity, NetEntityEvent.IData extraData = null)
		{
			if (!NetEntityEventManager.ValidateEntity(entity))
			{
				return;
			}
			ServerEntityEvent newEvent = new ServerEntityEvent(entity, this.ID + 1);
			if (extraData != null)
			{
				newEvent.SetData(extraData);
			}
			bool inGameClientsPresent = this.server.ConnectedClients.Count((Client c) => c.InGame) > 0;
			if (GameMain.GameSession.RoundDuration > this.server.ServerSettings.RoundStartSyncDuration)
			{
				this.events.RemoveAll((ServerEntityEvent e) => (NetIdUtils.IdMoreRecent(this.lastSentToAll, e.ID) || !inGameClientsPresent) && e.CreateTime < Timing.TotalTime - (double)this.server.ServerSettings.EventRemovalTime);
			}
			for (int i = this.events.Count - 1; i >= 0; i--)
			{
				if (this.events[i].IsDuplicate(newEvent) && !this.events[i].Sent)
				{
					return;
				}
			}
			this.ID += 1;
			this.events.Add(newEvent);
			if (!this.uniqueEvents.Any((ServerEntityEvent e) => e.IsDuplicate(newEvent)))
			{
				ServerEntityEvent uniqueEvent = new ServerEntityEvent(entity, (ushort)(this.uniqueEvents.Count + 1));
				uniqueEvent.SetData(extraData);
				this.uniqueEvents.Add(uniqueEvent);
			}
		}

		// Token: 0x0600346E RID: 13422 RVA: 0x00168D68 File Offset: 0x00166F68
		public void Update(List<Client> clients)
		{
			foreach (ServerEntityEventManager.BufferedEvent bufferedEvent in this.bufferedEvents)
			{
				if ((bufferedEvent.Character == null || bufferedEvent.Character.IsDead) && bufferedEvent.RequireCharacter)
				{
					bufferedEvent.IsProcessed = true;
				}
				else if (bufferedEvent.Character != null && !bufferedEvent.Character.IsIncapacitated && NetIdUtils.IdMoreRecent(bufferedEvent.CharacterStateID, bufferedEvent.Character.LastProcessedID))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(152, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Delaying reading entity event sent by a client until the character state has been processed. Event's character state: ");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(bufferedEvent.CharacterStateID);
					defaultInterpolatedStringHandler.AppendLiteral(", last processed character state: ");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(bufferedEvent.Character.LastProcessedID);
					DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					try
					{
						this.ReadEvent(bufferedEvent.Data, bufferedEvent.TargetEntity, bufferedEvent.Sender);
					}
					catch (Exception e)
					{
						string entityName = (bufferedEvent.TargetEntity == null) ? "null" : bufferedEvent.TargetEntity.ToString();
						Exception e2;
						if (GameSettings.CurrentConfig.VerboseLogging)
						{
							string errorMsg = "Failed to read server event for entity \"" + entityName + "\"!";
							GameServer.Log(errorMsg + "\n" + e2.StackTrace.CleanupStackTrace(), ServerLog.MessageType.Error);
							DebugConsole.ThrowError(errorMsg, e2, null, false, false);
						}
						GameAnalyticsManager.AddErrorEventOnce("ServerEntityEventManager.Read:ReadFailed" + entityName, GameAnalyticsManager.ErrorSeverity.Error, "Failed to read server event for entity \"" + entityName + "\"!\n" + e2.StackTrace.CleanupStackTrace());
					}
					bufferedEvent.IsProcessed = true;
				}
			}
			List<Client> inGameClients = clients.FindAll((Client c) => c.InGame && !c.NeedsMidRoundSync);
			if (inGameClients.Count > 0)
			{
				this.lastSentToAnyone = inGameClients[0].LastRecvEntityEventID;
				this.lastSentToAll = inGameClients[0].LastRecvEntityEventID;
				if (this.server.OwnerConnection != null)
				{
					Client owner = clients.Find((Client c) => c.Connection == this.server.OwnerConnection);
					if (owner != null)
					{
						this.lastSentToAll = owner.LastRecvEntityEventID;
					}
				}
				inGameClients.ForEach(delegate(Client c)
				{
					if (NetIdUtils.IdMoreRecent(this.lastSentToAll, c.LastRecvEntityEventID))
					{
						this.lastSentToAll = c.LastRecvEntityEventID;
					}
					if (NetIdUtils.IdMoreRecent(c.LastRecvEntityEventID, this.lastSentToAnyone))
					{
						this.lastSentToAnyone = c.LastRecvEntityEventID;
					}
				});
				ServerEntityEvent serverEntityEvent = this.events.Find((ServerEntityEvent e) => e.ID == this.lastSentToAnyone);
				this.lastSentToAnyoneTime = ((serverEntityEvent != null) ? serverEntityEvent.CreateTime : Timing.TotalTime);
				if (Timing.TotalTime - this.lastWarningTime > 5.0 && Timing.TotalTime - this.lastSentToAnyoneTime > 10.0 && GameMain.GameSession.RoundDuration > this.server.ServerSettings.RoundStartSyncDuration)
				{
					this.lastWarningTime = Timing.TotalTime;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(87, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("WARNING: ServerEntityEventManager is lagging behind! Last sent id: ");
					defaultInterpolatedStringHandler2.AppendFormatted<ushort>(this.lastSentToAnyone);
					defaultInterpolatedStringHandler2.AppendLiteral(", latest create id: ");
					defaultInterpolatedStringHandler2.AppendFormatted<ushort>(this.ID);
					string warningMsg = defaultInterpolatedStringHandler2.ToStringAndClear();
					warningMsg = warningMsg + "\n" + this.GetHighEventCountsWarning(this.events, 3);
					GameServer.Log(warningMsg, ServerLog.MessageType.ServerMessage);
					this.events.ForEach(delegate(ServerEntityEvent e)
					{
						e.ResetCreateTime();
					});
				}
				(from c in clients
				where c.NeedsMidRoundSync
				select c).ForEach(delegate(Client c)
				{
					if (NetIdUtils.IdMoreRecent(this.lastSentToAll, c.FirstNewEventID))
					{
						this.lastSentToAll = c.FirstNewEventID - 1;
					}
				});
				ServerEntityEvent firstEventToResend = this.events.Find((ServerEntityEvent e) => e.ID == this.lastSentToAll + 1);
				if (firstEventToResend != null && GameMain.GameSession.RoundDuration > this.server.ServerSettings.RoundStartSyncDuration && (this.lastSentToAnyoneTime - firstEventToResend.CreateTime > (double)this.server.ServerSettings.OldReceivedEventKickTime || Timing.TotalTime - firstEventToResend.CreateTime > (double)this.server.ServerSettings.OldEventKickTime))
				{
					List<Client> toKick = inGameClients.FindAll((Client c) => NetIdUtils.IdMoreRecent(this.lastSentToAll + 1, c.LastRecvEntityEventID) && (!c.NeedsMidRoundSync || firstEventToResend.CreateTime > c.MidRoundSyncTimeOut || this.lastSentToAnyoneTime > c.MidRoundSyncTimeOut || Timing.TotalTime > c.MidRoundSyncTimeOut + 10.0));
					toKick.ForEach(delegate(Client c)
					{
						DebugConsole.NewMessage(c.Name + " was kicked because they were expecting a very old network event (" + ((int)(c.LastRecvEntityEventID + 1)).ToString() + ")", new Color?(Color.Red), false);
						GameServer.Log(string.Concat(new string[]
						{
							NetworkMember.ClientLogName(c, null),
							" was kicked because they were expecting a very old network event (",
							((int)(c.LastRecvEntityEventID + 1)).ToString(),
							" (created ",
							(Timing.TotalTime - firstEventToResend.CreateTime).ToString("0.##"),
							" s ago, ",
							(this.lastSentToAnyoneTime - firstEventToResend.CreateTime).ToString("0.##"),
							" s older than last event sent to anyone) Events queued: ",
							this.events.Count.ToString(),
							", last sent to all: ",
							this.lastSentToAll.ToString()
						}), ServerLog.MessageType.Error);
						this.server.DisconnectClient(c, PeerDisconnectPacket.WithReason(DisconnectReason.ExcessiveDesyncOldEvent));
					});
				}
				if (this.events.Count > 0)
				{
					List<Client> toKick2 = inGameClients.FindAll((Client c) => NetIdUtils.IdMoreRecent(this.events[0].ID, c.LastRecvEntityEventID + 1));
					toKick2.ForEach(delegate(Client c)
					{
						DebugConsole.NewMessage(string.Concat(new string[]
						{
							c.Name,
							" was kicked because they were expecting a removed network event (",
							((int)(c.LastRecvEntityEventID + 1)).ToString(),
							", last available is ",
							this.events[0].ID.ToString(),
							")"
						}), new Color?(Color.Red), false);
						GameServer.Log(string.Concat(new string[]
						{
							NetworkMember.ClientLogName(c, null),
							" was kicked because they were expecting a removed network event (",
							((int)(c.LastRecvEntityEventID + 1)).ToString(),
							", last available is ",
							this.events[0].ID.ToString(),
							")"
						}), ServerLog.MessageType.Error);
						this.server.DisconnectClient(c, PeerDisconnectPacket.WithReason(DisconnectReason.ExcessiveDesyncRemovedEvent));
					});
				}
			}
			List<Client> timedOutClients = clients.FindAll((Client c) => c.Connection != GameMain.Server.OwnerConnection && c.InGame && c.NeedsMidRoundSync && Timing.TotalTime > c.MidRoundSyncTimeOut);
			foreach (Client timedOutClient in timedOutClients)
			{
				GameServer.Log("Disconnecting client " + NetworkMember.ClientLogName(timedOutClient, null) + ". Syncing the client with the server took too long.", ServerLog.MessageType.Error);
				GameMain.Server.DisconnectClient(timedOutClient, PeerDisconnectPacket.WithReason(DisconnectReason.SyncTimeout));
			}
			this.bufferedEvents.RemoveAll((ServerEntityEventManager.BufferedEvent b) => b.IsProcessed);
		}

		// Token: 0x0600346F RID: 13423 RVA: 0x001692F8 File Offset: 0x001674F8
		private void BufferEvent(ServerEntityEventManager.BufferedEvent bufferedEvent)
		{
			if (this.bufferedEvents.Count > 512)
			{
				DebugConsole.Log("Excessive amount of events in a client's event buffer. The client may be spamming events or their event IDs might be out of sync. Dropping events...");
				this.bufferedEvents.RemoveRange(0, 256);
			}
			this.bufferedEvents.Add(bufferedEvent);
		}

		// Token: 0x06003470 RID: 13424 RVA: 0x00169334 File Offset: 0x00167534
		public void Write(in SegmentTableWriter<ServerNetSegment> segmentTable, Client client, IWriteMessage msg)
		{
			List<NetEntityEvent> list;
			this.Write(segmentTable, client, msg, out list);
		}

		// Token: 0x06003471 RID: 13425 RVA: 0x0016934C File Offset: 0x0016754C
		public void Write(in SegmentTableWriter<ServerNetSegment> segmentTable, Client client, IWriteMessage msg, out List<NetEntityEvent> sentEvents)
		{
			List<NetEntityEvent> eventsToSync = this.GetEventsToSync(client);
			if (eventsToSync.Count == 0)
			{
				sentEvents = eventsToSync;
				return;
			}
			if (eventsToSync.Count > 200 && GameMain.GameSession != null && (double)GameMain.GameSession.RoundDuration > 30.0 && eventsToSync.Count > 200 && !client.NeedsMidRoundSync && Timing.TotalTime > this.lastEventCountHighWarning + 2.0)
			{
				Color color = (eventsToSync.Count > 500) ? Color.Red : Color.Orange;
				if (eventsToSync.Count < 300)
				{
					color = Color.Yellow;
				}
				string warningMsg = "WARNING: event count very high: " + eventsToSync.Count.ToString();
				warningMsg = warningMsg + "\n" + this.GetHighEventCountsWarning(eventsToSync, 3);
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					GameServer.Log(warningMsg, ServerLog.MessageType.Error);
				}
				this.server.SendConsoleMessage(warningMsg, client, new Color?(color));
				DebugConsole.NewMessage(warningMsg, new Color?(color), false);
				this.lastEventCountHighWarning = Timing.TotalTime;
			}
			if (client.NeedsMidRoundSync)
			{
				segmentTable.StartNewSegment(ServerNetSegment.EntityEventInitial);
				msg.WriteUInt16(client.UnreceivedEntityEventCount);
				msg.WriteUInt16(client.FirstNewEventID);
				base.Write(msg, eventsToSync, out sentEvents, client);
			}
			else
			{
				segmentTable.StartNewSegment(ServerNetSegment.EntityEvent);
				base.Write(msg, eventsToSync, out sentEvents, client);
			}
			foreach (NetEntityEvent entityEvent in sentEvents)
			{
				(entityEvent as ServerEntityEvent).Sent = true;
				client.EntityEventLastSent[entityEvent.ID] = NetTime.Now;
			}
		}

		// Token: 0x06003472 RID: 13426 RVA: 0x00169518 File Offset: 0x00167718
		private string GetHighEventCountsWarning(IEnumerable<NetEntityEvent> events, int maxEventsToList)
		{
			string warningMsg = string.Empty;
			var sortedEvents = from e in events
			group e by e.Entity.ToString() into e
			select new
			{
				Value = e.First<NetEntityEvent>(),
				Count = e.Count<NetEntityEvent>()
			} into e
			orderby e.Count descending
			select e;
			int count = 1;
			foreach (var sortedEvent in sortedEvents)
			{
				Entity targetEntity = sortedEvent.Value.Entity;
				if (!warningMsg.IsNullOrEmpty())
				{
					warningMsg += "\n";
				}
				warningMsg = string.Concat(new string[]
				{
					warningMsg,
					count.ToString(),
					". ",
					((targetEntity != null) ? targetEntity.ToString() : null) ?? "null",
					" x",
					sortedEvent.Count.ToString()
				});
				if (targetEntity != null && targetEntity.ContentPackage != ContentPackageManager.VanillaCorePackage)
				{
					warningMsg = warningMsg + " (content package: " + targetEntity.ContentPackage.Name + ")";
				}
				count++;
				if (count > maxEventsToList)
				{
					break;
				}
			}
			return warningMsg;
		}

		// Token: 0x06003473 RID: 13427 RVA: 0x0016968C File Offset: 0x0016788C
		private List<NetEntityEvent> GetEventsToSync(Client client)
		{
			List<NetEntityEvent> eventsToSync = new List<NetEntityEvent>();
			List<ServerEntityEvent> eventList = client.NeedsMidRoundSync ? this.uniqueEvents : this.events;
			if (eventList.Count == 0)
			{
				return eventsToSync;
			}
			int startIndex = eventList.Count;
			while (startIndex > 0 && NetIdUtils.IdMoreRecent(eventList[startIndex - 1].ID, client.LastRecvEntityEventID))
			{
				startIndex--;
			}
			int i = startIndex;
			while (i < eventList.Count)
			{
				double lastSent;
				client.EntityEventLastSent.TryGetValue(eventList[i].ID, out lastSent);
				float avgRoundtripTime = 0.01f;
				float minInterval = Math.Max(avgRoundtripTime, (float)this.server.UpdateInterval.TotalSeconds * 2f);
				if (lastSent <= NetTime.Now - (double)Math.Min(minInterval, 0.5f))
				{
					if (!client.NeedsMidRoundSync)
					{
						eventsToSync.AddRange(eventList.GetRange(i, eventList.Count - i));
						break;
					}
					if (i <= (int)client.UnreceivedEntityEventCount)
					{
						eventsToSync.AddRange(eventList.GetRange(i, (int)client.UnreceivedEntityEventCount - i));
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			return eventsToSync;
		}

		// Token: 0x06003474 RID: 13428 RVA: 0x001697A0 File Offset: 0x001679A0
		public void InitClientMidRoundSync(Client client)
		{
			if (this.uniqueEvents.Count == 0 || (this.events.Count > 0 && this.events[0].ID == this.uniqueEvents[0].ID))
			{
				client.UnreceivedEntityEventCount = 0;
				client.FirstNewEventID = 0;
				client.NeedsMidRoundSync = false;
				return;
			}
			double midRoundSyncTimeOut = (double)(this.uniqueEvents.Count / 10) * this.server.UpdateInterval.TotalSeconds;
			midRoundSyncTimeOut = Math.Max(midRoundSyncTimeOut, (double)this.server.ServerSettings.MinimumMidRoundSyncTimeout);
			client.UnreceivedEntityEventCount = (ushort)this.uniqueEvents.Count;
			client.NeedsMidRoundSync = true;
			client.MidRoundSyncTimeOut = Timing.TotalTime + midRoundSyncTimeOut;
			client.UnreceivedEntityEventCount = (ushort)this.uniqueEvents.Count;
			client.FirstNewEventID = ((this.events.Count == 0) ? 0 : this.events[this.events.Count - 1].ID);
		}

		// Token: 0x06003475 RID: 13429 RVA: 0x001698AC File Offset: 0x00167AAC
		public void Read(IReadMessage msg, Client sender = null)
		{
			msg.ReadPadBits();
			ushort firstEventID = msg.ReadUInt16();
			int eventCount = (int)msg.ReadByte();
			for (int i = 0; i < eventCount; i++)
			{
				ushort thisEventID = firstEventID + (ushort)i;
				ushort entityID = msg.ReadUInt16();
				if (entityID == 0)
				{
					if (thisEventID == sender.LastSentEntityEventID + 1)
					{
						sender.LastSentEntityEventID += 1;
					}
				}
				else
				{
					int msgLength = (int)msg.ReadVariableUInt32();
					IClientSerializable entity = Entity.FindEntityByID(entityID) as IClientSerializable;
					if (thisEventID != sender.LastSentEntityEventID + 1)
					{
						if (GameSettings.CurrentConfig.VerboseLogging)
						{
							DebugConsole.NewMessage("Received msg " + thisEventID.ToString() + ", expecting " + sender.LastSentEntityEventID.ToString(), new Color?(Color.Red), false);
						}
						msg.BitPosition += msgLength * 8;
					}
					else if (entity == null)
					{
						if (GameSettings.CurrentConfig.VerboseLogging)
						{
							DebugConsole.NewMessage(string.Concat(new string[]
							{
								"Received msg ",
								thisEventID.ToString(),
								", entity ",
								entityID.ToString(),
								" not found"
							}), new Color?(Color.Orange), false);
						}
						sender.LastSentEntityEventID += 1;
						msg.BitPosition += msgLength * 8;
					}
					else
					{
						if (GameSettings.CurrentConfig.VerboseLogging)
						{
							DebugConsole.NewMessage("Received msg " + thisEventID.ToString(), new Color?(Color.Green), false);
						}
						ushort characterStateID = msg.ReadUInt16();
						ReadWriteMessage buffer = new ReadWriteMessage();
						byte[] temp = msg.ReadBytes(msgLength - 2);
						buffer.WriteBytes(temp, 0, msgLength - 2);
						buffer.BitPosition = 0;
						this.BufferEvent(new ServerEntityEventManager.BufferedEvent(sender, sender.Character, characterStateID, entity, buffer)
						{
							RequireCharacter = !(entity is Hull)
						});
						sender.LastSentEntityEventID += 1;
					}
				}
			}
		}

		// Token: 0x06003476 RID: 13430 RVA: 0x00169AA4 File Offset: 0x00167CA4
		protected override void WriteEvent(IWriteMessage buffer, NetEntityEvent entityEvent, Client recipient = null)
		{
			ServerEntityEvent serverEvent = entityEvent as ServerEntityEvent;
			if (serverEvent == null)
			{
				return;
			}
			serverEvent.Write(buffer, recipient);
		}

		// Token: 0x06003477 RID: 13431 RVA: 0x00169AC4 File Offset: 0x00167CC4
		protected void ReadEvent(IReadMessage buffer, INetSerializable entity, Client sender = null)
		{
			IClientSerializable clientEntity = entity as IClientSerializable;
			if (clientEntity == null)
			{
				return;
			}
			clientEntity.ServerEventRead(buffer, sender);
		}

		// Token: 0x06003478 RID: 13432 RVA: 0x00169AE4 File Offset: 0x00167CE4
		public void Clear()
		{
			this.ID = 0;
			this.events.Clear();
			this.bufferedEvents.Clear();
			this.lastSentToAll = 0;
			this.uniqueEvents.Clear();
			foreach (Client c in this.server.ConnectedClients)
			{
				c.EntityEventLastSent.Clear();
				c.LastRecvEntityEventID = 0;
				c.LastSentEntityEventID = 0;
			}
		}

		// Token: 0x04001A15 RID: 6677
		private readonly List<ServerEntityEvent> events;

		// Token: 0x04001A16 RID: 6678
		private readonly List<ServerEntityEvent> uniqueEvents;

		// Token: 0x04001A17 RID: 6679
		private ushort lastSentToAll;

		// Token: 0x04001A18 RID: 6680
		private ushort lastSentToAnyone;

		// Token: 0x04001A19 RID: 6681
		private double lastSentToAnyoneTime;

		// Token: 0x04001A1A RID: 6682
		private double lastWarningTime;

		// Token: 0x04001A1B RID: 6683
		private readonly List<ServerEntityEventManager.BufferedEvent> bufferedEvents;

		// Token: 0x04001A1C RID: 6684
		private ushort ID;

		// Token: 0x04001A1D RID: 6685
		private readonly GameServer server;

		// Token: 0x04001A1E RID: 6686
		private double lastEventCountHighWarning;

		// Token: 0x02000BFE RID: 3070
		private class BufferedEvent
		{
			// Token: 0x060062A4 RID: 25252 RVA: 0x00210343 File Offset: 0x0020E543
			public BufferedEvent(Client sender, Character senderCharacter, ushort characterStateID, IClientSerializable targetEntity, ReadWriteMessage data)
			{
				this.Sender = sender;
				this.Character = senderCharacter;
				this.CharacterStateID = characterStateID;
				this.TargetEntity = targetEntity;
				this.Data = data;
			}

			// Token: 0x04003B18 RID: 15128
			public readonly Client Sender;

			// Token: 0x04003B19 RID: 15129
			public readonly ushort CharacterStateID;

			// Token: 0x04003B1A RID: 15130
			public readonly ReadWriteMessage Data;

			// Token: 0x04003B1B RID: 15131
			public readonly Character Character;

			// Token: 0x04003B1C RID: 15132
			public readonly IClientSerializable TargetEntity;

			// Token: 0x04003B1D RID: 15133
			public bool IsProcessed;

			// Token: 0x04003B1E RID: 15134
			public bool RequireCharacter = true;
		}
	}
}
