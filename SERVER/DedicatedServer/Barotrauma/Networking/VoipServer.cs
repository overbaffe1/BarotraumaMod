using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000378 RID: 888
	internal class VoipServer
	{
		// Token: 0x06003607 RID: 13831 RVA: 0x001727E7 File Offset: 0x001709E7
		public VoipServer(ServerPeer server)
		{
			this.netServer = server;
			this.queues = new List<VoipQueue>();
			this.lastSendTime = new Dictionary<VoipQueue, DateTime>();
		}

		// Token: 0x06003608 RID: 13832 RVA: 0x0017280C File Offset: 0x00170A0C
		public void RegisterQueue(VoipQueue queue)
		{
			if (!this.queues.Contains(queue))
			{
				this.queues.Add(queue);
			}
		}

		// Token: 0x06003609 RID: 13833 RVA: 0x00172828 File Offset: 0x00170A28
		public void UnregisterQueue(VoipQueue queue)
		{
			if (this.queues.Contains(queue))
			{
				this.queues.Remove(queue);
			}
		}

		// Token: 0x0600360A RID: 13834 RVA: 0x00172848 File Offset: 0x00170A48
		public void SendToClients(List<Client> clients)
		{
			using (List<VoipQueue>.Enumerator enumerator = this.queues.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VoipQueue queue = enumerator.Current;
					if (!(queue.LastReadTime < DateTime.Now - VoipConfig.SEND_INTERVAL))
					{
						if (this.lastSendTime.ContainsKey(queue))
						{
							if (this.lastSendTime[queue] + VoipConfig.SEND_INTERVAL > DateTime.Now)
							{
								continue;
							}
							this.lastSendTime[queue] = DateTime.Now;
						}
						else
						{
							this.lastSendTime.Add(queue, DateTime.Now);
						}
						Client sender = clients.Find((Client c) => c.VoipQueue == queue);
						if (sender == null)
						{
							break;
						}
						foreach (Client recipient in clients)
						{
							float distanceFactor;
							bool isRadio;
							if (recipient != sender && VoipServer.CanReceive(sender, recipient, out distanceFactor, out isRadio))
							{
								IWriteMessage msg = new WriteOnlyMessage();
								msg.WriteByte(10);
								msg.WriteByte(queue.QueueID);
								msg.WriteRangedSingle(distanceFactor, 0f, 1f, 8);
								msg.WriteBoolean(isRadio);
								queue.Write(msg);
								this.netServer.Send(msg, recipient.Connection, DeliveryMethod.Unreliable, true);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600360B RID: 13835 RVA: 0x00172A18 File Offset: 0x00170C18
		private static bool CanReceive(Client sender, Client recipient, out float distanceFactor, out bool isRadio)
		{
			if (Screen.Selected != GameMain.GameScreen)
			{
				distanceFactor = 0f;
				isRadio = false;
				return true;
			}
			distanceFactor = 0f;
			isRadio = false;
			if (sender.Muted)
			{
				return false;
			}
			bool recipientSpectating = recipient.Character == null || recipient.Character.IsDead;
			bool senderSpectating = sender.Character == null || sender.Character.IsDead;
			if (senderSpectating)
			{
				return recipientSpectating;
			}
			if (sender.Character != null && sender.Character.SpeechImpediment >= 100f)
			{
				return false;
			}
			WifiComponent recipientRadio = null;
			WifiComponent senderRadio;
			if (!sender.VoipQueue.ForceLocal && ChatMessage.CanUseRadio(sender.Character, out senderRadio, false) && (recipientSpectating || ChatMessage.CanUseRadio(recipient.Character, out recipientRadio, false)))
			{
				bool? canUse = null;
				LuaCsSetup.Instance.EventService.PublishEvent<IEventCanUseVoiceRadio>(delegate(IEventCanUseVoiceRadio x)
				{
					bool? flag = x.OnCanUseVoiceRadio(sender, recipient);
					canUse = ((flag != null) ? flag : canUse);
				});
				if (canUse != null)
				{
					isRadio = canUse.Value;
					return canUse.Value;
				}
				if (recipientSpectating)
				{
					isRadio = true;
					if (recipient.SpectatePos == null)
					{
						return true;
					}
					distanceFactor = MathHelper.Clamp(Vector2.Distance(sender.Character.WorldPosition, recipient.SpectatePos.Value) / senderRadio.Range, 0f, 1f);
					return distanceFactor < 1f;
				}
				else if (recipientRadio != null && recipientRadio.CanReceive(senderRadio))
				{
					isRadio = true;
					distanceFactor = MathHelper.Clamp(Vector2.Distance(sender.Character.WorldPosition, recipient.Character.WorldPosition) / senderRadio.Range, 0f, 1f);
					return true;
				}
			}
			float range = 1f;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventChangeLocalVoiceRange>(delegate(IEventChangeLocalVoiceRange x)
			{
				range = x.OnChangeLocalVoiceRange(sender, recipient).GetValueOrDefault(range);
			});
			if (!recipientSpectating)
			{
				float garbleAmount = ChatMessage.GetGarbleAmount(recipient.Character, sender.Character, 1000f, 2f);
				distanceFactor = garbleAmount;
				return garbleAmount < range;
			}
			if (recipient.SpectatePos == null)
			{
				return true;
			}
			distanceFactor = MathHelper.Clamp(Vector2.Distance(sender.Character.WorldPosition, recipient.SpectatePos.Value) / 1000f, 0f, 1f);
			return distanceFactor < 1f;
		}

		// Token: 0x0600360C RID: 13836 RVA: 0x00172D1C File Offset: 0x00170F1C
		public static void Read(IReadMessage inc, Client connectedClient)
		{
			VoipQueue queue = connectedClient.VoipQueue;
			if (queue.Read(inc, false))
			{
				connectedClient.VoipServerDecoder.OnNewVoiceReceived();
			}
		}

		// Token: 0x04001AC5 RID: 6853
		private readonly ServerPeer netServer;

		// Token: 0x04001AC6 RID: 6854
		private readonly List<VoipQueue> queues;

		// Token: 0x04001AC7 RID: 6855
		private readonly Dictionary<VoipQueue, DateTime> lastSendTime;
	}
}
