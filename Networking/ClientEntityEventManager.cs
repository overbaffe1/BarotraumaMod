using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x0200045F RID: 1119
	internal class ClientEntityEventManager : NetEntityEventManager
	{
		// Token: 0x17001347 RID: 4935
		// (get) Token: 0x06004B9A RID: 19354 RVA: 0x0029B3F7 File Offset: 0x002995F7
		public ushort LastReceivedID
		{
			get
			{
				return this.lastReceivedID;
			}
		}

		// Token: 0x17001348 RID: 4936
		// (get) Token: 0x06004B9B RID: 19355 RVA: 0x0029B3FF File Offset: 0x002995FF
		public bool MidRoundSyncing
		{
			get
			{
				return this.firstNewID != null;
			}
		}

		// Token: 0x17001349 RID: 4937
		// (get) Token: 0x06004B9C RID: 19356 RVA: 0x0029B40C File Offset: 0x0029960C
		// (set) Token: 0x06004B9D RID: 19357 RVA: 0x0029B414 File Offset: 0x00299614
		public bool MidRoundSyncingDone { get; private set; }

		// Token: 0x06004B9E RID: 19358 RVA: 0x0029B41D File Offset: 0x0029961D
		public ClientEntityEventManager(GameClient client)
		{
			this.events = new List<ClientEntityEvent>();
			this.eventLastSent = new Dictionary<ushort, float>();
			this.thisClient = client;
		}

		// Token: 0x06004B9F RID: 19359 RVA: 0x0029B450 File Offset: 0x00299650
		public void CreateEvent(IClientSerializable entity, NetEntityEvent.IData extraData = null, bool requireControlledCharacter = true)
		{
			if (GameMain.Client == null)
			{
				return;
			}
			if (requireControlledCharacter && GameMain.Client.Character == null)
			{
				return;
			}
			if (!NetEntityEventManager.ValidateEntity(entity))
			{
				return;
			}
			ushort eventId = this.ID + 1;
			Character character = GameMain.Client.Character;
			ClientEntityEvent newEvent = new ClientEntityEvent(entity, eventId, (character != null) ? character.LastNetworkUpdateID : 0);
			if (extraData != null)
			{
				newEvent.SetData(extraData);
			}
			for (int i = this.events.Count - 1; i >= 0; i--)
			{
				if (!this.events[i].Sent && this.events[i].IsDuplicate(newEvent))
				{
					return;
				}
			}
			this.ID += 1;
			this.events.Add(newEvent);
		}

		// Token: 0x06004BA0 RID: 19360 RVA: 0x0029B50C File Offset: 0x0029970C
		public void Write(in SegmentTableWriter<ClientNetSegment> segmentTable, IWriteMessage msg, NetworkConnection serverConnection)
		{
			if (this.events.Count == 0 || serverConnection == null)
			{
				return;
			}
			List<NetEntityEvent> eventsToSync = new List<NetEntityEvent>();
			int startIndex = this.events.Count;
			while (startIndex > 0 && NetIdUtils.IdMoreRecent(this.events[startIndex - 1].ID, this.thisClient.LastSentEntityEventID))
			{
				startIndex--;
			}
			this.events.RemoveRange(0, startIndex);
			for (int i = 0; i < this.events.Count; i++)
			{
				float lastSent;
				this.eventLastSent.TryGetValue(this.events[i].ID, out lastSent);
				if ((double)lastSent <= NetTime.Now - 0.2)
				{
					eventsToSync.AddRange(this.events.GetRange(i, this.events.Count - i));
					break;
				}
			}
			if (eventsToSync.Count == 0)
			{
				return;
			}
			foreach (NetEntityEvent entityEvent in eventsToSync)
			{
				this.eventLastSent[entityEvent.ID] = (float)NetTime.Now;
			}
			segmentTable.StartNewSegment(ClientNetSegment.EntityState);
			List<NetEntityEvent> list;
			base.Write(msg, eventsToSync, out list, null);
		}

		// Token: 0x06004BA1 RID: 19361 RVA: 0x0029B650 File Offset: 0x00299850
		public bool Read(ServerNetSegment type, IReadMessage msg, float sendingTime)
		{
			if (type == ServerNetSegment.EntityEventInitial)
			{
				ushort unreceivedEntityEventCount = msg.ReadUInt16();
				this.firstNewID = new ushort?(msg.ReadUInt16());
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					string str = "received midround syncing msg, unreceived: ";
					string str2 = unreceivedEntityEventCount.ToString();
					string str3 = ", first new ID: ";
					ushort? num = this.firstNewID;
					DebugConsole.NewMessage(str + str2 + str3 + num.ToString(), new Color?(Color.Yellow), false);
				}
			}
			else
			{
				this.MidRoundSyncingDone = true;
				if (this.firstNewID != null)
				{
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						DebugConsole.NewMessage("midround syncing complete, switching to ID " + ((ushort)((int)(this.firstNewID - 1)).Value).ToString(), new Color?(Color.Yellow), false);
					}
					this.lastReceivedID = (ushort)((int)(this.firstNewID - 1)).Value;
					this.firstNewID = null;
				}
			}
			this.tempEntityList.Clear();
			msg.ReadPadBits();
			ushort firstEventID = msg.ReadUInt16();
			int eventCount = (int)msg.ReadByte();
			for (int i = 0; i < eventCount; i++)
			{
				if (msg.BitPosition + 16 + 8 > msg.LengthBits)
				{
					ushort potentialEntityId = 0;
					try
					{
						potentialEntityId = msg.ReadUInt16();
					}
					catch
					{
					}
					Entity targetEntity = Entity.FindEntityByID(potentialEntityId);
					string errorMsg = "Error while reading a message from the server (entity: " + (((targetEntity != null) ? targetEntity.ToString() : null) ?? "unknown") + ").";
					string str4 = errorMsg;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 2);
					defaultInterpolatedStringHandler.AppendLiteral(" Entity event data exceeds the size of the buffer (current position: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(msg.BitPosition);
					defaultInterpolatedStringHandler.AppendLiteral(", length: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(msg.LengthBits);
					defaultInterpolatedStringHandler.AppendLiteral(").");
					errorMsg = str4 + defaultInterpolatedStringHandler.ToStringAndClear();
					errorMsg += "\nPrevious entities:";
					for (int j = this.tempEntityList.Count - 1; j >= 0; j--)
					{
						errorMsg = errorMsg + "\n" + ((this.tempEntityList[j] == null) ? "NULL" : this.tempEntityList[j].ToString());
					}
					DebugConsole.ThrowError(errorMsg, null, (targetEntity != null) ? targetEntity.ContentPackage : null, false, false);
					return false;
				}
				ushort thisEventID = firstEventID + (ushort)i;
				ushort entityID = msg.ReadUInt16();
				if (entityID == 0)
				{
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						DebugConsole.NewMessage("received msg " + thisEventID.ToString() + " (null entity)", new Color?(Color.Orange), false);
					}
					this.tempEntityList.Add(null);
					if (thisEventID == this.lastReceivedID + 1)
					{
						this.lastReceivedID += 1;
					}
				}
				else
				{
					int msgLength = (int)msg.ReadVariableUInt32();
					IServerSerializable entity = Entity.FindEntityByID(entityID) as IServerSerializable;
					this.tempEntityList.Add(entity);
					if (thisEventID != this.lastReceivedID + 1 || entity == null)
					{
						if (thisEventID != this.lastReceivedID + 1)
						{
							if (GameSettings.CurrentConfig.VerboseLogging)
							{
								DebugConsole.NewMessage(string.Concat(new string[]
								{
									"Received msg ",
									thisEventID.ToString(),
									" (waiting for ",
									((int)(this.lastReceivedID + 1)).ToString(),
									")"
								}), new Color?(NetIdUtils.IdMoreRecent(thisEventID, this.lastReceivedID + 1) ? GUIStyle.Red : Color.Yellow), false);
							}
						}
						else if (entity == null)
						{
							DebugConsole.NewMessage(string.Concat(new string[]
							{
								"Received msg ",
								thisEventID.ToString(),
								", entity ",
								entityID.ToString(),
								" not found"
							}), new Color?(GUIStyle.Red), false);
							GameMain.Client.ReportError(ClientNetError.MISSING_ENTITY, 0, thisEventID, entityID);
							return false;
						}
						msg.BitPosition += msgLength * 8;
					}
					else
					{
						int msgPosition = msg.BitPosition;
						if (GameSettings.CurrentConfig.VerboseLogging)
						{
							DebugConsole.NewMessage(string.Concat(new string[]
							{
								"received msg ",
								thisEventID.ToString(),
								" (",
								entity.ToString(),
								")"
							}), new Color?(Color.Green), false);
						}
						this.lastReceivedID += 1;
						try
						{
							this.ReadEvent(msg, entity, sendingTime);
							msg.ReadPadBits();
						}
						catch (Exception exception)
						{
							throw new EntityEventException("Failed to read event.", entity as Entity, exception);
						}
						if (msg.BitPosition != msgPosition + msgLength * 8)
						{
							IServerSerializable prevEntity = (this.tempEntityList.Count >= 2) ? this.tempEntityList[this.tempEntityList.Count - 2] : null;
							Entity p = prevEntity as Entity;
							ushort prevId = (p != null) ? p.ID : 0;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(159, 6);
							defaultInterpolatedStringHandler2.AppendLiteral("Message byte position incorrect after reading an event for the entity \"");
							defaultInterpolatedStringHandler2.AppendFormatted<IServerSerializable>(entity);
							defaultInterpolatedStringHandler2.AppendLiteral("\" (ID ");
							Entity e = entity as Entity;
							defaultInterpolatedStringHandler2.AppendFormatted<int>((int)((e != null) ? e.ID : 0));
							defaultInterpolatedStringHandler2.AppendLiteral("). ");
							defaultInterpolatedStringHandler2.AppendLiteral("The previous entity was \"");
							defaultInterpolatedStringHandler2.AppendFormatted<IServerSerializable>(prevEntity);
							defaultInterpolatedStringHandler2.AppendLiteral("\" (ID ");
							defaultInterpolatedStringHandler2.AppendFormatted<ushort>(prevId);
							defaultInterpolatedStringHandler2.AppendLiteral(") ");
							defaultInterpolatedStringHandler2.AppendLiteral("Read ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(msg.BitPosition - msgPosition);
							defaultInterpolatedStringHandler2.AppendLiteral(" bits, expected message length was ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(msgLength * 8);
							defaultInterpolatedStringHandler2.AppendLiteral(" bits.");
							string errorMsg2 = defaultInterpolatedStringHandler2.ToStringAndClear();
							GameAnalyticsManager.AddErrorEventOnce("ClientEntityEventManager.Read:BitPosMismatch", GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
							throw new EntityEventException(errorMsg2, entity as Entity, null);
						}
					}
				}
			}
			return true;
		}

		// Token: 0x06004BA2 RID: 19362 RVA: 0x0029BC94 File Offset: 0x00299E94
		protected override void WriteEvent(IWriteMessage buffer, NetEntityEvent entityEvent, Client recipient = null)
		{
			ClientEntityEvent clientEvent = entityEvent as ClientEntityEvent;
			if (clientEvent == null)
			{
				return;
			}
			clientEvent.Write(buffer);
			clientEvent.Sent = true;
		}

		// Token: 0x06004BA3 RID: 19363 RVA: 0x0029BCBA File Offset: 0x00299EBA
		protected void ReadEvent(IReadMessage buffer, IServerSerializable entity, float sendingTime)
		{
			entity.ClientEventRead(buffer, sendingTime);
		}

		// Token: 0x06004BA4 RID: 19364 RVA: 0x0029BCC4 File Offset: 0x00299EC4
		public void Clear()
		{
			this.lastReceivedID = 0;
			this.firstNewID = null;
			this.eventLastSent.Clear();
			this.MidRoundSyncingDone = false;
			this.ClearSelf();
		}

		// Token: 0x06004BA5 RID: 19365 RVA: 0x0029BCF1 File Offset: 0x00299EF1
		public void ClearSelf()
		{
			this.ID = 0;
			this.events.Clear();
			if (this.thisClient != null)
			{
				this.thisClient.LastSentEntityEventID = 0;
			}
		}

		// Token: 0x0400278D RID: 10125
		private List<ClientEntityEvent> events;

		// Token: 0x0400278E RID: 10126
		private ushort ID;

		// Token: 0x0400278F RID: 10127
		private GameClient thisClient;

		// Token: 0x04002790 RID: 10128
		public Dictionary<ushort, float> eventLastSent;

		// Token: 0x04002791 RID: 10129
		private ushort lastReceivedID;

		// Token: 0x04002793 RID: 10131
		private ushort? firstNewID;

		// Token: 0x04002794 RID: 10132
		private readonly List<IServerSerializable> tempEntityList = new List<IServerSerializable>();
	}
}
