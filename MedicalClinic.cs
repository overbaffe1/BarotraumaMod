using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200006F RID: 111
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class MedicalClinic
	{
		// Token: 0x17000425 RID: 1061
		// (get) Token: 0x06001035 RID: 4149 RVA: 0x0009AD6E File Offset: 0x00098F6E
		[Nullable(2)]
		private MedicalClinicUI ui
		{
			[NullableContext(2)]
			get
			{
				CampaignMode campaignMode = this.campaign;
				if (campaignMode == null)
				{
					return null;
				}
				CampaignUI campaignUI = campaignMode.CampaignUI;
				if (campaignUI == null)
				{
					return null;
				}
				return campaignUI.MedicalClinic;
			}
		}

		// Token: 0x06001036 RID: 4150 RVA: 0x0009AD8C File Offset: 0x00098F8C
		public bool RequestAfflictions(CharacterInfo info, Action<MedicalClinic.AfflictionRequest> onReceived)
		{
			if (GameMain.IsSingleplayer)
			{
				if (info != null)
				{
					Character character = info.Character;
					if (character != null)
					{
						CharacterHealth health = character.CharacterHealth;
						if (health != null)
						{
							ImmutableArray<MedicalClinic.NetAffliction> pendingAfflictions = this.GetAllAfflictions(health);
							onReceived(new MedicalClinic.AfflictionRequest(MedicalClinic.RequestResult.Success, pendingAfflictions));
							return true;
						}
					}
				}
				onReceived(new MedicalClinic.AfflictionRequest(MedicalClinic.RequestResult.CharacterInfoMissing, ImmutableArray<MedicalClinic.NetAffliction>.Empty));
				return true;
			}
			return MedicalClinic.requestBucket.TryEnqueue(delegate
			{
				this.afflictionRequests.Add(new MedicalClinic.RequestAction<MedicalClinic.AfflictionRequest>(onReceived, MedicalClinic.GetTimeout()));
				MedicalClinic.SendAfflictionRequest(info);
			});
		}

		// Token: 0x06001037 RID: 4151 RVA: 0x0009AE28 File Offset: 0x00099028
		public void RequestLatestPending(Action<MedicalClinic.PendingRequest> onReceived)
		{
			if (GameMain.IsSingleplayer)
			{
				return;
			}
			MedicalClinic.requestBucket.TryEnqueue(delegate
			{
				this.pendingHealRequests.Add(new MedicalClinic.RequestAction<MedicalClinic.PendingRequest>(onReceived, MedicalClinic.GetTimeout()));
				MedicalClinic.SendPendingRequest();
			});
		}

		// Token: 0x06001038 RID: 4152 RVA: 0x0009AE68 File Offset: 0x00099068
		public void Update(float deltaTime)
		{
			this.processAfflictionChangesTimer -= deltaTime;
			if (this.processAfflictionChangesTimer <= 0f)
			{
				foreach (Character character in this.charactersWithAfflictionChanges)
				{
					if (GameMain.NetworkMember == null)
					{
						ImmutableArray<MedicalClinic.NetAffliction> afflictions = this.GetAllAfflictions(character.CharacterHealth);
						MedicalClinicUI ui = this.ui;
						if (ui != null)
						{
							ui.UpdateAfflictions(new MedicalClinic.NetCrewMember(character.Info, afflictions));
						}
					}
					MedicalClinicUI ui2 = this.ui;
					if (ui2 != null)
					{
						ui2.UpdateCrewPanel();
					}
				}
				this.charactersWithAfflictionChanges.Clear();
				this.processAfflictionChangesTimer = 1f;
			}
			DateTimeOffset now = DateTimeOffset.Now;
			MedicalClinic.UpdateQueue<MedicalClinic.AfflictionRequest>(this.afflictionRequests, now, delegate(Action<MedicalClinic.AfflictionRequest> callback)
			{
				callback(new MedicalClinic.AfflictionRequest(MedicalClinic.RequestResult.Timeout, ImmutableArray<MedicalClinic.NetAffliction>.Empty));
			});
			MedicalClinic.UpdateQueue<MedicalClinic.PendingRequest>(this.pendingHealRequests, now, delegate(Action<MedicalClinic.PendingRequest> callback)
			{
				callback(new MedicalClinic.PendingRequest(MedicalClinic.RequestResult.Timeout, NetCollection<MedicalClinic.NetCrewMember>.Empty));
			});
			MedicalClinic.UpdateQueue<MedicalClinic.HealRequest>(this.healAllRequests, now, delegate(Action<MedicalClinic.HealRequest> callback)
			{
				callback(new MedicalClinic.HealRequest(MedicalClinic.RequestResult.Timeout, MedicalClinic.HealRequestResult.Unknown));
			});
			List<MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>> requestQueue = this.clearAllRequests;
			DateTimeOffset now2 = now;
			Action<Action<MedicalClinic.CallbackOnlyRequest>> onTimeout;
			if ((onTimeout = MedicalClinic.<>O.<0>__CallbackOnlyTimeout) == null)
			{
				onTimeout = (MedicalClinic.<>O.<0>__CallbackOnlyTimeout = new Action<Action<MedicalClinic.CallbackOnlyRequest>>(MedicalClinic.<Update>g__CallbackOnlyTimeout|17_3));
			}
			MedicalClinic.UpdateQueue<MedicalClinic.CallbackOnlyRequest>(requestQueue, now2, onTimeout);
			List<MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>> requestQueue2 = this.addRequests;
			DateTimeOffset now3 = now;
			Action<Action<MedicalClinic.CallbackOnlyRequest>> onTimeout2;
			if ((onTimeout2 = MedicalClinic.<>O.<0>__CallbackOnlyTimeout) == null)
			{
				onTimeout2 = (MedicalClinic.<>O.<0>__CallbackOnlyTimeout = new Action<Action<MedicalClinic.CallbackOnlyRequest>>(MedicalClinic.<Update>g__CallbackOnlyTimeout|17_3));
			}
			MedicalClinic.UpdateQueue<MedicalClinic.CallbackOnlyRequest>(requestQueue2, now3, onTimeout2);
			List<MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>> requestQueue3 = this.removeRequests;
			DateTimeOffset now4 = now;
			Action<Action<MedicalClinic.CallbackOnlyRequest>> onTimeout3;
			if ((onTimeout3 = MedicalClinic.<>O.<0>__CallbackOnlyTimeout) == null)
			{
				onTimeout3 = (MedicalClinic.<>O.<0>__CallbackOnlyTimeout = new Action<Action<MedicalClinic.CallbackOnlyRequest>>(MedicalClinic.<Update>g__CallbackOnlyTimeout|17_3));
			}
			MedicalClinic.UpdateQueue<MedicalClinic.CallbackOnlyRequest>(requestQueue3, now4, onTimeout3);
			MedicalClinic.requestBucket.Update(deltaTime);
		}

		// Token: 0x06001039 RID: 4153 RVA: 0x0009B034 File Offset: 0x00099234
		public bool IsAfflictionPending(MedicalClinic.NetCrewMember character, MedicalClinic.NetAffliction affliction)
		{
			Func<MedicalClinic.NetAffliction, bool> <>9__0;
			foreach (MedicalClinic.NetCrewMember crewMember in this.PendingHeals)
			{
				if (crewMember.CharacterEquals(character))
				{
					ImmutableArray<MedicalClinic.NetAffliction> afflictions = crewMember.Afflictions;
					Func<MedicalClinic.NetAffliction, bool> predicate;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = ((MedicalClinic.NetAffliction a) => a.AfflictionEquals(affliction)));
					}
					return afflictions.Any(predicate);
				}
			}
			return false;
		}

		// Token: 0x0600103A RID: 4154 RVA: 0x0009B0CC File Offset: 0x000992CC
		private static bool TryDequeue<[Nullable(2)] T>([Nullable(new byte[]
		{
			1,
			0,
			1
		})] List<MedicalClinic.RequestAction<T>> requestQueue, out Action<T> result)
		{
			MedicalClinic.RequestAction<T>? first = requestQueue.FirstOrNull<MedicalClinic.RequestAction<T>>();
			if (first != null)
			{
				MedicalClinic.RequestAction<T> action = first.GetValueOrDefault();
				requestQueue.Remove(action);
				result = action.Callback;
				return true;
			}
			Action<T> action2;
			if ((action2 = MedicalClinic.<>c__19<T>.<>9__19_0) == null)
			{
				Action<T> action3 = MedicalClinic.<>c__19<T>.<>9__19_0 = delegate(T _)
				{
				};
				action2 = action3;
			}
			result = action2;
			return false;
		}

		// Token: 0x0600103B RID: 4155 RVA: 0x0009B12C File Offset: 0x0009932C
		private static void UpdateQueue<[Nullable(2)] T>([Nullable(new byte[]
		{
			1,
			0,
			1
		})] List<MedicalClinic.RequestAction<T>> requestQueue, DateTimeOffset now, Action<Action<T>> onTimeout)
		{
			HashSet<MedicalClinic.RequestAction<T>> removals = null;
			foreach (MedicalClinic.RequestAction<T> action in requestQueue)
			{
				if (action.Timeout < now)
				{
					onTimeout(action.Callback);
					if (removals == null)
					{
						removals = new HashSet<MedicalClinic.RequestAction<T>>();
					}
					removals.Add(action);
				}
			}
			if (removals == null)
			{
				return;
			}
			foreach (MedicalClinic.RequestAction<T> action2 in removals)
			{
				requestQueue.Remove(action2);
			}
		}

		// Token: 0x0600103C RID: 4156 RVA: 0x0009B1E8 File Offset: 0x000993E8
		private void OnMoneyChanged(WalletChangedEvent e)
		{
			if (e.Wallet.IsOwnWallet)
			{
				Action onUpdate = this.OnUpdate;
				if (onUpdate == null)
				{
					return;
				}
				onUpdate();
			}
		}

		// Token: 0x0600103D RID: 4157 RVA: 0x0009B208 File Offset: 0x00099408
		private static DateTimeOffset GetTimeout()
		{
			return DateTimeOffset.Now.AddSeconds(5.0).AddMilliseconds((double)MedicalClinic.GetPing());
		}

		// Token: 0x0600103E RID: 4158 RVA: 0x0009B23C File Offset: 0x0009943C
		private static int GetPing()
		{
			MedicalClinic.<>c__DisplayClass23_0 CS$<>8__locals1 = new MedicalClinic.<>c__DisplayClass23_0();
			if (!GameMain.IsSingleplayer)
			{
				MedicalClinic.<>c__DisplayClass23_0 CS$<>8__locals2 = CS$<>8__locals1;
				GameClient client2 = GameMain.Client;
				CS$<>8__locals2.ownName = ((client2 != null) ? client2.Name : null);
				if (CS$<>8__locals1.ownName != null)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					IReadOnlyList<Client> clients = (networkMember != null) ? networkMember.ConnectedClients : null;
					if (clients != null)
					{
						return (int)(from client in clients
						where client.Name == CS$<>8__locals1.ownName
						select client.Ping).FirstOrDefault<ushort>();
					}
				}
			}
			return 0;
		}

		// Token: 0x0600103F RID: 4159 RVA: 0x0009B2C8 File Offset: 0x000994C8
		public bool TreatAllButtonAction(Action<MedicalClinic.CallbackOnlyRequest> onReceived)
		{
			if (GameMain.IsSingleplayer)
			{
				this.AddEverythingToPending();
				onReceived(new MedicalClinic.CallbackOnlyRequest(MedicalClinic.RequestResult.Success));
				Action onUpdate = this.OnUpdate;
				if (onUpdate != null)
				{
					onUpdate();
				}
				return true;
			}
			return MedicalClinic.requestBucket.TryEnqueue(delegate
			{
				this.addRequests.Add(new MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>(onReceived, MedicalClinic.GetTimeout()));
				MedicalClinic.ClientSend(null, MedicalClinic.NetworkHeader.ADD_EVERYTHING_TO_PENDING, DeliveryMethod.Reliable);
			});
		}

		// Token: 0x06001040 RID: 4160 RVA: 0x0009B330 File Offset: 0x00099530
		public bool HealAllButtonAction(Action<MedicalClinic.HealRequest> onReceived)
		{
			if (GameMain.IsSingleplayer)
			{
				MedicalClinic.HealRequestResult result = this.HealAllPending(false, null);
				onReceived(new MedicalClinic.HealRequest(MedicalClinic.RequestResult.Success, this.HealAllPending(false, null)));
				if (result == MedicalClinic.HealRequestResult.Success)
				{
					Action onUpdate = this.OnUpdate;
					if (onUpdate != null)
					{
						onUpdate();
					}
				}
				return true;
			}
			CampaignMode campaignMode = this.campaign;
			MedicalClinicUI medicalClinicUI;
			if (campaignMode == null)
			{
				medicalClinicUI = null;
			}
			else
			{
				CampaignUI campaignUI = campaignMode.CampaignUI;
				medicalClinicUI = ((campaignUI != null) ? campaignUI.MedicalClinic : null);
			}
			MedicalClinicUI openedUi = medicalClinicUI;
			if (openedUi != null)
			{
				openedUi.ClosePopup();
			}
			return MedicalClinic.requestBucket.TryEnqueue(delegate
			{
				this.healAllRequests.Add(new MedicalClinic.RequestAction<MedicalClinic.HealRequest>(onReceived, MedicalClinic.GetTimeout()));
				MedicalClinic.ClientSend(null, MedicalClinic.NetworkHeader.HEAL_PENDING, DeliveryMethod.Reliable);
			});
		}

		// Token: 0x06001041 RID: 4161 RVA: 0x0009B3D0 File Offset: 0x000995D0
		public bool ClearAllButtonAction(Action<MedicalClinic.CallbackOnlyRequest> onReceived)
		{
			if (GameMain.IsSingleplayer)
			{
				this.ClearPendingHeals();
				onReceived(new MedicalClinic.CallbackOnlyRequest(MedicalClinic.RequestResult.Success));
				Action onUpdate = this.OnUpdate;
				if (onUpdate != null)
				{
					onUpdate();
				}
				return true;
			}
			return MedicalClinic.requestBucket.TryEnqueue(delegate
			{
				this.clearAllRequests.Add(new MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>(onReceived, MedicalClinic.GetTimeout()));
				MedicalClinic.ClientSend(null, MedicalClinic.NetworkHeader.CLEAR_PENDING, DeliveryMethod.Reliable);
			});
		}

		// Token: 0x06001042 RID: 4162 RVA: 0x0009B438 File Offset: 0x00099638
		private void ClearRequestReceived()
		{
			this.ClearPendingHeals();
			Action<MedicalClinic.CallbackOnlyRequest> callback;
			if (MedicalClinic.TryDequeue<MedicalClinic.CallbackOnlyRequest>(this.clearAllRequests, out callback))
			{
				callback(new MedicalClinic.CallbackOnlyRequest(MedicalClinic.RequestResult.Success));
			}
			Action onUpdate = this.OnUpdate;
			if (onUpdate == null)
			{
				return;
			}
			onUpdate();
		}

		// Token: 0x06001043 RID: 4163 RVA: 0x0009B478 File Offset: 0x00099678
		private void HealRequestReceived(IReadMessage inc)
		{
			MedicalClinic.NetHealRequest request = INetSerializableStruct.Read<MedicalClinic.NetHealRequest>(inc);
			if (request.Result == MedicalClinic.HealRequestResult.Success)
			{
				this.HealAllPending(true, null);
			}
			Action<MedicalClinic.HealRequest> callback;
			if (MedicalClinic.TryDequeue<MedicalClinic.HealRequest>(this.healAllRequests, out callback))
			{
				callback(new MedicalClinic.HealRequest(MedicalClinic.RequestResult.Success, request.Result));
			}
			Action onUpdate = this.OnUpdate;
			if (onUpdate == null)
			{
				return;
			}
			onUpdate();
		}

		// Token: 0x06001044 RID: 4164 RVA: 0x0009B4D4 File Offset: 0x000996D4
		public bool AddPendingButtonAction(MedicalClinic.NetCrewMember crewMember, Action<MedicalClinic.CallbackOnlyRequest> onReceived)
		{
			if (GameMain.IsSingleplayer)
			{
				this.InsertPendingCrewMember(crewMember);
				onReceived(new MedicalClinic.CallbackOnlyRequest(MedicalClinic.RequestResult.Success));
				Action onUpdate = this.OnUpdate;
				if (onUpdate != null)
				{
					onUpdate();
				}
				return true;
			}
			return MedicalClinic.requestBucket.TryEnqueue(delegate
			{
				this.addRequests.Add(new MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>(onReceived, MedicalClinic.GetTimeout()));
				MedicalClinic.ClientSend(crewMember, MedicalClinic.NetworkHeader.ADD_PENDING, DeliveryMethod.Reliable);
			});
		}

		// Token: 0x06001045 RID: 4165 RVA: 0x0009B54C File Offset: 0x0009974C
		public bool RemovePendingButtonAction(MedicalClinic.NetCrewMember crewMember, MedicalClinic.NetAffliction affliction, Action<MedicalClinic.CallbackOnlyRequest> onReceived)
		{
			if (GameMain.IsSingleplayer)
			{
				this.RemovePendingAffliction(crewMember, affliction);
				onReceived(new MedicalClinic.CallbackOnlyRequest(MedicalClinic.RequestResult.Success));
				Action onUpdate = this.OnUpdate;
				if (onUpdate != null)
				{
					onUpdate();
				}
				return true;
			}
			INetSerializableStruct removedAffliction = new MedicalClinic.NetRemovedAffliction
			{
				CrewMember = crewMember,
				Affliction = affliction
			};
			return MedicalClinic.requestBucket.TryEnqueue(delegate
			{
				this.removeRequests.Add(new MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>(onReceived, MedicalClinic.GetTimeout()));
				MedicalClinic.ClientSend(removedAffliction, MedicalClinic.NetworkHeader.REMOVE_PENDING, DeliveryMethod.Reliable);
			});
		}

		// Token: 0x06001046 RID: 4166 RVA: 0x0009B5DC File Offset: 0x000997DC
		private void NewAdditionReceived(IReadMessage inc, MedicalClinic.MessageFlag flag)
		{
			foreach (MedicalClinic.NetCrewMember crewMember in ((IEnumerable<MedicalClinic.NetCrewMember>)INetSerializableStruct.Read<NetCollection<MedicalClinic.NetCrewMember>>(inc)))
			{
				this.InsertPendingCrewMember(crewMember);
			}
			Action<MedicalClinic.CallbackOnlyRequest> callback;
			if (flag == MedicalClinic.MessageFlag.Response && MedicalClinic.TryDequeue<MedicalClinic.CallbackOnlyRequest>(this.addRequests, out callback))
			{
				callback(new MedicalClinic.CallbackOnlyRequest(MedicalClinic.RequestResult.Success));
			}
			Action onUpdate = this.OnUpdate;
			if (onUpdate == null)
			{
				return;
			}
			onUpdate();
		}

		// Token: 0x06001047 RID: 4167 RVA: 0x0009B660 File Offset: 0x00099860
		private void NewRemovalReceived(IReadMessage inc, MedicalClinic.MessageFlag flag)
		{
			MedicalClinic.NetRemovedAffliction removed = INetSerializableStruct.Read<MedicalClinic.NetRemovedAffliction>(inc);
			this.RemovePendingAffliction(removed.CrewMember, removed.Affliction);
			Action<MedicalClinic.CallbackOnlyRequest> callback;
			if (flag == MedicalClinic.MessageFlag.Response && MedicalClinic.TryDequeue<MedicalClinic.CallbackOnlyRequest>(this.removeRequests, out callback))
			{
				callback(new MedicalClinic.CallbackOnlyRequest(MedicalClinic.RequestResult.Success));
			}
			Action onUpdate = this.OnUpdate;
			if (onUpdate == null)
			{
				return;
			}
			onUpdate();
		}

		// Token: 0x06001048 RID: 4168 RVA: 0x0009B6B8 File Offset: 0x000998B8
		private static void SendAfflictionRequest(CharacterInfo info)
		{
			INetSerializableStruct crewMember = new MedicalClinic.NetCrewMember(info);
			MedicalClinic.ClientSend(crewMember, MedicalClinic.NetworkHeader.REQUEST_AFFLICTIONS, DeliveryMethod.Unreliable);
		}

		// Token: 0x06001049 RID: 4169 RVA: 0x0009B6D9 File Offset: 0x000998D9
		private static void SendPendingRequest()
		{
			MedicalClinic.ClientSend(null, MedicalClinic.NetworkHeader.REQUEST_PENDING, DeliveryMethod.Reliable);
		}

		// Token: 0x0600104A RID: 4170 RVA: 0x0009B6E4 File Offset: 0x000998E4
		private void AfflictionRequestReceived(IReadMessage inc)
		{
			MedicalClinic.NetCrewMember crewMember = INetSerializableStruct.Read<MedicalClinic.NetCrewMember>(inc);
			Action<MedicalClinic.AfflictionRequest> callback;
			if (MedicalClinic.TryDequeue<MedicalClinic.AfflictionRequest>(this.afflictionRequests, out callback))
			{
				MedicalClinic.RequestResult result = (crewMember.CharacterInfoID == 0) ? MedicalClinic.RequestResult.CharacterNotFound : MedicalClinic.RequestResult.Success;
				callback(new MedicalClinic.AfflictionRequest(result, crewMember.Afflictions.ToImmutableArray<MedicalClinic.NetAffliction>()));
			}
		}

		// Token: 0x0600104B RID: 4171 RVA: 0x0009B730 File Offset: 0x00099930
		private void AfflictionUpdateReceived(IReadMessage inc)
		{
			MedicalClinic.NetCrewMember crewMember = INetSerializableStruct.Read<MedicalClinic.NetCrewMember>(inc);
			MedicalClinicUI ui = this.ui;
			if (ui == null)
			{
				return;
			}
			ui.UpdateAfflictions(crewMember);
		}

		// Token: 0x0600104C RID: 4172 RVA: 0x0009B758 File Offset: 0x00099958
		private void PendingRequestReceived(IReadMessage inc)
		{
			NetCollection<MedicalClinic.NetCrewMember> pendingCrew = INetSerializableStruct.Read<NetCollection<MedicalClinic.NetCrewMember>>(inc);
			Action<MedicalClinic.PendingRequest> callback;
			if (MedicalClinic.TryDequeue<MedicalClinic.PendingRequest>(this.pendingHealRequests, out callback))
			{
				callback(new MedicalClinic.PendingRequest(MedicalClinic.RequestResult.Success, pendingCrew));
			}
		}

		// Token: 0x0600104D RID: 4173 RVA: 0x0009B788 File Offset: 0x00099988
		public static void SendUnsubscribeRequest()
		{
			MedicalClinic.ClientSend(null, MedicalClinic.NetworkHeader.UNSUBSCRIBE_ME, DeliveryMethod.Reliable);
		}

		// Token: 0x0600104E RID: 4174 RVA: 0x0009B794 File Offset: 0x00099994
		private static IWriteMessage StartSending()
		{
			IWriteMessage writeMessage = new WriteOnlyMessage();
			writeMessage.WriteByte(17);
			return writeMessage;
		}

		// Token: 0x0600104F RID: 4175 RVA: 0x0009B7B0 File Offset: 0x000999B0
		[NullableContext(2)]
		private static void ClientSend(INetSerializableStruct netStruct, MedicalClinic.NetworkHeader header, DeliveryMethod deliveryMethod)
		{
			IWriteMessage msg = MedicalClinic.StartSending();
			msg.WriteByte((byte)header);
			if (netStruct != null)
			{
				netStruct.Write(msg);
			}
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			ClientPeer clientPeer = client.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, deliveryMethod, true);
		}

		// Token: 0x06001050 RID: 4176 RVA: 0x0009B7F4 File Offset: 0x000999F4
		public void ClientRead(IReadMessage inc)
		{
			MedicalClinic.NetworkHeader header = (MedicalClinic.NetworkHeader)inc.ReadByte();
			MedicalClinic.MessageFlag flag = (MedicalClinic.MessageFlag)inc.ReadByte();
			switch (header)
			{
			case MedicalClinic.NetworkHeader.REQUEST_AFFLICTIONS:
				this.AfflictionRequestReceived(inc);
				return;
			case MedicalClinic.NetworkHeader.AFFLICTION_UPDATE:
				this.AfflictionUpdateReceived(inc);
				return;
			case MedicalClinic.NetworkHeader.UNSUBSCRIBE_ME:
				break;
			case MedicalClinic.NetworkHeader.REQUEST_PENDING:
				this.PendingRequestReceived(inc);
				return;
			case MedicalClinic.NetworkHeader.ADD_PENDING:
				this.NewAdditionReceived(inc, flag);
				return;
			case MedicalClinic.NetworkHeader.REMOVE_PENDING:
				this.NewRemovalReceived(inc, flag);
				return;
			case MedicalClinic.NetworkHeader.CLEAR_PENDING:
				this.ClearRequestReceived();
				break;
			case MedicalClinic.NetworkHeader.HEAL_PENDING:
				this.HealRequestReceived(inc);
				return;
			default:
				return;
			}
		}

		// Token: 0x06001051 RID: 4177 RVA: 0x0009B870 File Offset: 0x00099A70
		public MedicalClinic(CampaignMode campaign)
		{
			this.campaign = campaign;
			campaign.OnMoneyChanged.RegisterOverwriteExisting("MedicalClinic".ToIdentifier(), new Action<WalletChangedEvent>(this.OnMoneyChanged));
		}

		// Token: 0x06001052 RID: 4178 RVA: 0x0009B904 File Offset: 0x00099B04
		private static bool IsOutpostInCombat()
		{
			Level loaded = Level.Loaded;
			if (loaded == null || loaded.Type != LevelData.LevelType.Outpost)
			{
				return false;
			}
			IEnumerable<Character> crew = (from c in MedicalClinic.GetCrewCharacters()
			where c.Character != null
			select c.Character).ToImmutableHashSet<Character>();
			foreach (Character npc in from c in Character.CharacterList
			where c.TeamID == CharacterTeamType.FriendlyNPC
			select c)
			{
				if (npc.IsInstigator)
				{
					goto IL_E6;
				}
				HumanAIController humanAIController = npc.AIController as HumanAIController;
				if (humanAIController == null)
				{
					goto IL_E6;
				}
				AIObjectiveManager objectiveManager = humanAIController.ObjectiveManager;
				if (objectiveManager == null)
				{
					goto IL_E6;
				}
				AIObjectiveCombat combatObjective = objectiveManager.CurrentObjective as AIObjectiveCombat;
				if (combatObjective == null)
				{
					goto IL_E6;
				}
				bool flag = crew.Contains(combatObjective.Enemy);
				IL_E7:
				bool isInCombatWithCrew = flag;
				if (isInCombatWithCrew)
				{
					return true;
				}
				continue;
				IL_E6:
				flag = false;
				goto IL_E7;
			}
			return false;
		}

		// Token: 0x06001053 RID: 4179 RVA: 0x0009BA2C File Offset: 0x00099C2C
		[NullableContext(2)]
		private MedicalClinic.HealRequestResult HealAllPending(bool force = false, Client client = null)
		{
			int totalCost = this.GetTotalCost();
			if (!force)
			{
				if (MedicalClinic.IsOutpostInCombat())
				{
					return MedicalClinic.HealRequestResult.Refused;
				}
				CampaignMode campaignMode = this.campaign;
				if (campaignMode == null || !campaignMode.TryPurchase(client, totalCost))
				{
					return MedicalClinic.HealRequestResult.InsufficientFunds;
				}
			}
			ImmutableArray<CharacterInfo> crew = MedicalClinic.GetCrewCharacters();
			foreach (MedicalClinic.NetCrewMember crewMember in this.PendingHeals)
			{
				CharacterInfo targetCharacter = crewMember.FindCharacterInfo(crew);
				Character character = (targetCharacter != null) ? targetCharacter.Character : null;
				if (character != null)
				{
					CharacterHealth health = character.CharacterHealth;
					if (health != null)
					{
						foreach (MedicalClinic.NetAffliction affliction in crewMember.Afflictions)
						{
							CharacterHealth characterHealth = health;
							Identifier identifier = affliction.Identifier;
							AfflictionPrefab prefab = affliction.Prefab;
							characterHealth.ReduceAfflictionOnAllLimbs(identifier, (prefab != null) ? prefab.MaxStrength : ((float)affliction.Strength), null, null);
						}
					}
				}
			}
			this.ClearPendingHeals();
			return MedicalClinic.HealRequestResult.Success;
		}

		// Token: 0x06001054 RID: 4180 RVA: 0x0009BB38 File Offset: 0x00099D38
		private void ClearPendingHeals()
		{
			this.PendingHeals.Clear();
		}

		// Token: 0x06001055 RID: 4181 RVA: 0x0009BB48 File Offset: 0x00099D48
		private void AddEverythingToPending()
		{
			foreach (CharacterInfo info in MedicalClinic.GetCrewCharacters())
			{
				Character character = info.Character;
				CharacterHealth health = (character != null) ? character.CharacterHealth : null;
				if (health != null)
				{
					ImmutableArray<MedicalClinic.NetAffliction> afflictions = this.GetAllAfflictions(health);
					if (afflictions.Length != 0)
					{
						this.InsertPendingCrewMember(new MedicalClinic.NetCrewMember(info, afflictions));
					}
				}
			}
		}

		// Token: 0x06001056 RID: 4182 RVA: 0x0009BBAC File Offset: 0x00099DAC
		private void RemovePendingAffliction(MedicalClinic.NetCrewMember crewMember, MedicalClinic.NetAffliction affliction)
		{
			foreach (MedicalClinic.NetCrewMember listMember in this.PendingHeals.ToList<MedicalClinic.NetCrewMember>())
			{
				this.PendingHeals.Remove(listMember);
				MedicalClinic.NetCrewMember pendingMember = listMember;
				if (pendingMember.CharacterEquals(crewMember))
				{
					List<MedicalClinic.NetAffliction> newAfflictions = new List<MedicalClinic.NetAffliction>();
					foreach (MedicalClinic.NetAffliction pendingAffliction in pendingMember.Afflictions)
					{
						if (!pendingAffliction.AfflictionEquals(affliction))
						{
							newAfflictions.Add(pendingAffliction);
						}
					}
					pendingMember.Afflictions = newAfflictions.ToImmutableArray<MedicalClinic.NetAffliction>();
				}
				if (pendingMember.Afflictions.Any<MedicalClinic.NetAffliction>())
				{
					this.PendingHeals.Add(pendingMember);
				}
			}
		}

		// Token: 0x06001057 RID: 4183 RVA: 0x0009BC7C File Offset: 0x00099E7C
		private void InsertPendingCrewMember(MedicalClinic.NetCrewMember crewMember)
		{
			MedicalClinic.NetCrewMember? netCrewMember = this.PendingHeals.FirstOrNull((MedicalClinic.NetCrewMember m) => m.CharacterEquals(crewMember));
			if (netCrewMember != null)
			{
				MedicalClinic.NetCrewMember foundHeal = netCrewMember.GetValueOrDefault();
				this.PendingHeals.Remove(foundHeal);
			}
			this.PendingHeals.Add(crewMember);
		}

		// Token: 0x06001058 RID: 4184 RVA: 0x0009BCDD File Offset: 0x00099EDD
		public static bool IsHealable(Affliction affliction)
		{
			return affliction.Prefab.HealableInMedicalClinic && affliction.Strength > MedicalClinic.<IsHealable>g__GetShowTreshold|65_0(affliction);
		}

		// Token: 0x06001059 RID: 4185 RVA: 0x0009BCFC File Offset: 0x00099EFC
		[NullableContext(0)]
		private ImmutableArray<MedicalClinic.NetAffliction> GetAllAfflictions([Nullable(1)] CharacterHealth health)
		{
			IEnumerable<Affliction> allAfflictions = health.GetAllAfflictions();
			Func<Affliction, bool> predicate;
			if ((predicate = MedicalClinic.<>O.<1>__IsHealable) == null)
			{
				predicate = (MedicalClinic.<>O.<1>__IsHealable = new Func<Affliction, bool>(MedicalClinic.IsHealable));
			}
			IEnumerable<Affliction> rawAfflictions = allAfflictions.Where(predicate);
			List<MedicalClinic.NetAffliction> afflictions = new List<MedicalClinic.NetAffliction>();
			using (IEnumerator<Affliction> enumerator = rawAfflictions.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Affliction affliction = enumerator.Current;
					MedicalClinic.NetAffliction? netAffliction2 = afflictions.FirstOrNull((MedicalClinic.NetAffliction netAffliction) => netAffliction.AfflictionEquals(affliction.Prefab));
					MedicalClinic.NetAffliction newAffliction;
					if (netAffliction2 != null)
					{
						MedicalClinic.NetAffliction foundAffliction = netAffliction2.GetValueOrDefault();
						afflictions.Remove(foundAffliction);
						foundAffliction.Strength += (ushort)affliction.Strength;
						foundAffliction.Price += (ushort)this.GetAdjustedPrice(MedicalClinic.<GetAllAfflictions>g__GetHealPrice|66_0(affliction));
						newAffliction = foundAffliction;
					}
					else
					{
						newAffliction = default(MedicalClinic.NetAffliction);
						newAffliction.SetAffliction(affliction, health);
						newAffliction.Price = (ushort)this.GetAdjustedPrice((int)newAffliction.Price);
					}
					afflictions.Add(newAffliction);
				}
			}
			return afflictions.ToImmutableArray<MedicalClinic.NetAffliction>();
		}

		// Token: 0x0600105A RID: 4186 RVA: 0x0009BE20 File Offset: 0x0009A020
		public static void OnAfflictionCountChanged(Character character)
		{
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null)
			{
				return;
			}
			CampaignMode campaignMode = gameSession.Campaign;
			if (campaignMode == null)
			{
				return;
			}
			MedicalClinic medicalClinic = campaignMode.MedicalClinic;
			if (medicalClinic == null)
			{
				return;
			}
			medicalClinic.OnAfflictionCountChangedPrivate(character);
		}

		// Token: 0x0600105B RID: 4187 RVA: 0x0009BE46 File Offset: 0x0009A046
		private void OnAfflictionCountChangedPrivate(Character character)
		{
			if (((character != null) ? character.Info : null) == null)
			{
				return;
			}
			this.charactersWithAfflictionChanges.Add(character);
		}

		// Token: 0x0600105C RID: 4188 RVA: 0x0009BE64 File Offset: 0x0009A064
		public int GetTotalCost()
		{
			return this.PendingHeals.SelectMany((MedicalClinic.NetCrewMember h) => h.Afflictions).Aggregate(0, (int current, MedicalClinic.NetAffliction affliction) => current + (int)affliction.Price);
		}

		// Token: 0x0600105D RID: 4189 RVA: 0x0009BEC0 File Offset: 0x0009A0C0
		private int GetAdjustedPrice(int price)
		{
			CampaignMode campaignMode = this.campaign;
			Location location;
			if (campaignMode == null)
			{
				location = null;
			}
			else
			{
				Map map = campaignMode.Map;
				location = ((map != null) ? map.CurrentLocation : null);
			}
			Location currentLocation = location;
			if (currentLocation != null)
			{
				LocationType type = currentLocation.Type;
				if (type != null && type.HasOutpost)
				{
					return currentLocation.GetAdjustedHealCost(price);
				}
			}
			return int.MaxValue;
		}

		// Token: 0x0600105E RID: 4190 RVA: 0x0009BF0E File Offset: 0x0009A10E
		public int GetBalance()
		{
			CampaignMode campaignMode = this.campaign;
			if (campaignMode == null)
			{
				return 0;
			}
			return campaignMode.GetBalance(null);
		}

		// Token: 0x0600105F RID: 4191 RVA: 0x0009BF24 File Offset: 0x0009A124
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static ImmutableArray<CharacterInfo> GetCrewCharacters()
		{
			return (from c in Character.CharacterList
			where c.Info != null && c.TeamID == CharacterTeamType.Team1
			select c.Info).ToImmutableArray<CharacterInfo>();
		}

		// Token: 0x06001061 RID: 4193 RVA: 0x0009BF96 File Offset: 0x0009A196
		[CompilerGenerated]
		internal static void <Update>g__CallbackOnlyTimeout|17_3(Action<MedicalClinic.CallbackOnlyRequest> callback)
		{
			callback(new MedicalClinic.CallbackOnlyRequest(MedicalClinic.RequestResult.Timeout));
		}

		// Token: 0x06001062 RID: 4194 RVA: 0x0009BFA4 File Offset: 0x0009A1A4
		[CompilerGenerated]
		internal static float <IsHealable>g__GetShowTreshold|65_0(Affliction affliction)
		{
			return Math.Max(0f, Math.Min(affliction.Prefab.ShowIconToOthersThreshold, affliction.Prefab.ShowInHealthScannerThreshold));
		}

		// Token: 0x06001063 RID: 4195 RVA: 0x0009BFCB File Offset: 0x0009A1CB
		[CompilerGenerated]
		internal static int <GetAllAfflictions>g__GetHealPrice|66_0(Affliction affliction)
		{
			return (int)((float)affliction.Prefab.BaseHealCost + affliction.Prefab.HealCostMultiplier * affliction.Strength);
		}

		// Token: 0x04000800 RID: 2048
		[Nullable(new byte[]
		{
			1,
			0
		})]
		private readonly List<MedicalClinic.RequestAction<MedicalClinic.AfflictionRequest>> afflictionRequests = new List<MedicalClinic.RequestAction<MedicalClinic.AfflictionRequest>>();

		// Token: 0x04000801 RID: 2049
		[Nullable(new byte[]
		{
			1,
			0
		})]
		private readonly List<MedicalClinic.RequestAction<MedicalClinic.PendingRequest>> pendingHealRequests = new List<MedicalClinic.RequestAction<MedicalClinic.PendingRequest>>();

		// Token: 0x04000802 RID: 2050
		[Nullable(new byte[]
		{
			1,
			0
		})]
		private readonly List<MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>> clearAllRequests = new List<MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>>();

		// Token: 0x04000803 RID: 2051
		[Nullable(new byte[]
		{
			1,
			0
		})]
		private readonly List<MedicalClinic.RequestAction<MedicalClinic.HealRequest>> healAllRequests = new List<MedicalClinic.RequestAction<MedicalClinic.HealRequest>>();

		// Token: 0x04000804 RID: 2052
		[Nullable(new byte[]
		{
			1,
			0
		})]
		private readonly List<MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>> addRequests = new List<MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>>();

		// Token: 0x04000805 RID: 2053
		[Nullable(new byte[]
		{
			1,
			0
		})]
		private readonly List<MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>> removeRequests = new List<MedicalClinic.RequestAction<MedicalClinic.CallbackOnlyRequest>>();

		// Token: 0x04000806 RID: 2054
		private static readonly LeakyBucket requestBucket = new LeakyBucket(0.25f, 10);

		// Token: 0x04000807 RID: 2055
		private const int RateLimitMaxRequests = 20;

		// Token: 0x04000808 RID: 2056
		private const int RateLimitExpiry = 5;

		// Token: 0x04000809 RID: 2057
		public readonly List<MedicalClinic.NetCrewMember> PendingHeals = new List<MedicalClinic.NetCrewMember>();

		// Token: 0x0400080A RID: 2058
		[Nullable(2)]
		public Action OnUpdate;

		// Token: 0x0400080B RID: 2059
		[Nullable(2)]
		private readonly CampaignMode campaign;

		// Token: 0x0400080C RID: 2060
		private readonly HashSet<Character> charactersWithAfflictionChanges = new HashSet<Character>();

		// Token: 0x0400080D RID: 2061
		private float processAfflictionChangesTimer;

		// Token: 0x0400080E RID: 2062
		private const float ProcessAfflictionChangesInterval = 1f;

		// Token: 0x020008CF RID: 2255
		[NullableContext(0)]
		public enum RequestResult
		{
			// Token: 0x04003F71 RID: 16241
			Undecided,
			// Token: 0x04003F72 RID: 16242
			Success,
			// Token: 0x04003F73 RID: 16243
			CharacterInfoMissing,
			// Token: 0x04003F74 RID: 16244
			CharacterNotFound,
			// Token: 0x04003F75 RID: 16245
			Timeout
		}

		// Token: 0x020008D0 RID: 2256
		[NullableContext(0)]
		public readonly struct RequestAction<[Nullable(2)] T> : IEquatable<MedicalClinic.RequestAction<T>>
		{
			// Token: 0x06006F94 RID: 28564 RVA: 0x00367CC1 File Offset: 0x00365EC1
			[NullableContext(1)]
			public RequestAction(Action<T> Callback, DateTimeOffset Timeout)
			{
				this.Callback = Callback;
				this.Timeout = Timeout;
			}

			// Token: 0x17001A38 RID: 6712
			// (get) Token: 0x06006F95 RID: 28565 RVA: 0x00367CD1 File Offset: 0x00365ED1
			// (set) Token: 0x06006F96 RID: 28566 RVA: 0x00367CD9 File Offset: 0x00365ED9
			[Nullable(1)]
			public Action<T> Callback { [NullableContext(1)] get; [NullableContext(1)] set; }

			// Token: 0x17001A39 RID: 6713
			// (get) Token: 0x06006F97 RID: 28567 RVA: 0x00367CE2 File Offset: 0x00365EE2
			// (set) Token: 0x06006F98 RID: 28568 RVA: 0x00367CEA File Offset: 0x00365EEA
			public DateTimeOffset Timeout { get; set; }

			// Token: 0x06006F99 RID: 28569 RVA: 0x00367CF4 File Offset: 0x00365EF4
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("RequestAction");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006F9A RID: 28570 RVA: 0x00367D40 File Offset: 0x00365F40
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Callback = ");
				builder.Append(this.Callback);
				builder.Append(", Timeout = ");
				builder.Append(this.Timeout.ToString());
				return true;
			}

			// Token: 0x06006F9B RID: 28571 RVA: 0x00367D8E File Offset: 0x00365F8E
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.RequestAction<T> left, MedicalClinic.RequestAction<T> right)
			{
				return !(left == right);
			}

			// Token: 0x06006F9C RID: 28572 RVA: 0x00367D9A File Offset: 0x00365F9A
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.RequestAction<T> left, MedicalClinic.RequestAction<T> right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006F9D RID: 28573 RVA: 0x00367DA4 File Offset: 0x00365FA4
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<Action<T>>.Default.GetHashCode(this.<Callback>k__BackingField) * -1521134295 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(this.<Timeout>k__BackingField);
			}

			// Token: 0x06006F9E RID: 28574 RVA: 0x00367DCD File Offset: 0x00365FCD
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinic.RequestAction<T> && this.Equals((MedicalClinic.RequestAction<T>)obj);
			}

			// Token: 0x06006F9F RID: 28575 RVA: 0x00367DE5 File Offset: 0x00365FE5
			[CompilerGenerated]
			public bool Equals(MedicalClinic.RequestAction<T> other)
			{
				return EqualityComparer<Action<T>>.Default.Equals(this.<Callback>k__BackingField, other.<Callback>k__BackingField) && EqualityComparer<DateTimeOffset>.Default.Equals(this.<Timeout>k__BackingField, other.<Timeout>k__BackingField);
			}

			// Token: 0x06006FA0 RID: 28576 RVA: 0x00367E17 File Offset: 0x00366017
			[NullableContext(1)]
			[CompilerGenerated]
			public void Deconstruct(out Action<T> Callback, out DateTimeOffset Timeout)
			{
				Callback = this.Callback;
				Timeout = this.Timeout;
			}
		}

		// Token: 0x020008D1 RID: 2257
		[NullableContext(0)]
		public readonly struct AfflictionRequest : IEquatable<MedicalClinic.AfflictionRequest>
		{
			// Token: 0x06006FA1 RID: 28577 RVA: 0x00367E2D File Offset: 0x0036602D
			public AfflictionRequest(MedicalClinic.RequestResult Result, ImmutableArray<MedicalClinic.NetAffliction> Afflictions)
			{
				this.Result = Result;
				this.Afflictions = Afflictions;
			}

			// Token: 0x17001A3A RID: 6714
			// (get) Token: 0x06006FA2 RID: 28578 RVA: 0x00367E3D File Offset: 0x0036603D
			// (set) Token: 0x06006FA3 RID: 28579 RVA: 0x00367E45 File Offset: 0x00366045
			public MedicalClinic.RequestResult Result { get; set; }

			// Token: 0x17001A3B RID: 6715
			// (get) Token: 0x06006FA4 RID: 28580 RVA: 0x00367E4E File Offset: 0x0036604E
			// (set) Token: 0x06006FA5 RID: 28581 RVA: 0x00367E56 File Offset: 0x00366056
			public ImmutableArray<MedicalClinic.NetAffliction> Afflictions { get; set; }

			// Token: 0x06006FA6 RID: 28582 RVA: 0x00367E60 File Offset: 0x00366060
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("AfflictionRequest");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006FA7 RID: 28583 RVA: 0x00367EAC File Offset: 0x003660AC
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Result = ");
				builder.Append(this.Result.ToString());
				builder.Append(", Afflictions = ");
				builder.Append(this.Afflictions.ToString());
				return true;
			}

			// Token: 0x06006FA8 RID: 28584 RVA: 0x00367F08 File Offset: 0x00366108
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.AfflictionRequest left, MedicalClinic.AfflictionRequest right)
			{
				return !(left == right);
			}

			// Token: 0x06006FA9 RID: 28585 RVA: 0x00367F14 File Offset: 0x00366114
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.AfflictionRequest left, MedicalClinic.AfflictionRequest right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006FAA RID: 28586 RVA: 0x00367F1E File Offset: 0x0036611E
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<MedicalClinic.RequestResult>.Default.GetHashCode(this.<Result>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<MedicalClinic.NetAffliction>>.Default.GetHashCode(this.<Afflictions>k__BackingField);
			}

			// Token: 0x06006FAB RID: 28587 RVA: 0x00367F47 File Offset: 0x00366147
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinic.AfflictionRequest && this.Equals((MedicalClinic.AfflictionRequest)obj);
			}

			// Token: 0x06006FAC RID: 28588 RVA: 0x00367F5F File Offset: 0x0036615F
			[CompilerGenerated]
			public bool Equals(MedicalClinic.AfflictionRequest other)
			{
				return EqualityComparer<MedicalClinic.RequestResult>.Default.Equals(this.<Result>k__BackingField, other.<Result>k__BackingField) && EqualityComparer<ImmutableArray<MedicalClinic.NetAffliction>>.Default.Equals(this.<Afflictions>k__BackingField, other.<Afflictions>k__BackingField);
			}

			// Token: 0x06006FAD RID: 28589 RVA: 0x00367F91 File Offset: 0x00366191
			[CompilerGenerated]
			public void Deconstruct(out MedicalClinic.RequestResult Result, out ImmutableArray<MedicalClinic.NetAffliction> Afflictions)
			{
				Result = this.Result;
				Afflictions = this.Afflictions;
			}
		}

		// Token: 0x020008D2 RID: 2258
		[NullableContext(0)]
		public readonly struct PendingRequest : IEquatable<MedicalClinic.PendingRequest>
		{
			// Token: 0x06006FAE RID: 28590 RVA: 0x00367FA7 File Offset: 0x003661A7
			public PendingRequest(MedicalClinic.RequestResult Result, NetCollection<MedicalClinic.NetCrewMember> CrewMembers)
			{
				this.Result = Result;
				this.CrewMembers = CrewMembers;
			}

			// Token: 0x17001A3C RID: 6716
			// (get) Token: 0x06006FAF RID: 28591 RVA: 0x00367FB7 File Offset: 0x003661B7
			// (set) Token: 0x06006FB0 RID: 28592 RVA: 0x00367FBF File Offset: 0x003661BF
			public MedicalClinic.RequestResult Result { get; set; }

			// Token: 0x17001A3D RID: 6717
			// (get) Token: 0x06006FB1 RID: 28593 RVA: 0x00367FC8 File Offset: 0x003661C8
			// (set) Token: 0x06006FB2 RID: 28594 RVA: 0x00367FD0 File Offset: 0x003661D0
			public NetCollection<MedicalClinic.NetCrewMember> CrewMembers { get; set; }

			// Token: 0x06006FB3 RID: 28595 RVA: 0x00367FDC File Offset: 0x003661DC
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("PendingRequest");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006FB4 RID: 28596 RVA: 0x00368028 File Offset: 0x00366228
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Result = ");
				builder.Append(this.Result.ToString());
				builder.Append(", CrewMembers = ");
				builder.Append(this.CrewMembers.ToString());
				return true;
			}

			// Token: 0x06006FB5 RID: 28597 RVA: 0x00368084 File Offset: 0x00366284
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.PendingRequest left, MedicalClinic.PendingRequest right)
			{
				return !(left == right);
			}

			// Token: 0x06006FB6 RID: 28598 RVA: 0x00368090 File Offset: 0x00366290
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.PendingRequest left, MedicalClinic.PendingRequest right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006FB7 RID: 28599 RVA: 0x0036809A File Offset: 0x0036629A
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<MedicalClinic.RequestResult>.Default.GetHashCode(this.<Result>k__BackingField) * -1521134295 + EqualityComparer<NetCollection<MedicalClinic.NetCrewMember>>.Default.GetHashCode(this.<CrewMembers>k__BackingField);
			}

			// Token: 0x06006FB8 RID: 28600 RVA: 0x003680C3 File Offset: 0x003662C3
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinic.PendingRequest && this.Equals((MedicalClinic.PendingRequest)obj);
			}

			// Token: 0x06006FB9 RID: 28601 RVA: 0x003680DB File Offset: 0x003662DB
			[CompilerGenerated]
			public bool Equals(MedicalClinic.PendingRequest other)
			{
				return EqualityComparer<MedicalClinic.RequestResult>.Default.Equals(this.<Result>k__BackingField, other.<Result>k__BackingField) && EqualityComparer<NetCollection<MedicalClinic.NetCrewMember>>.Default.Equals(this.<CrewMembers>k__BackingField, other.<CrewMembers>k__BackingField);
			}

			// Token: 0x06006FBA RID: 28602 RVA: 0x0036810D File Offset: 0x0036630D
			[CompilerGenerated]
			public void Deconstruct(out MedicalClinic.RequestResult Result, out NetCollection<MedicalClinic.NetCrewMember> CrewMembers)
			{
				Result = this.Result;
				CrewMembers = this.CrewMembers;
			}
		}

		// Token: 0x020008D3 RID: 2259
		[NullableContext(0)]
		public readonly struct CallbackOnlyRequest : IEquatable<MedicalClinic.CallbackOnlyRequest>
		{
			// Token: 0x06006FBB RID: 28603 RVA: 0x00368123 File Offset: 0x00366323
			public CallbackOnlyRequest(MedicalClinic.RequestResult Result)
			{
				this.Result = Result;
			}

			// Token: 0x17001A3E RID: 6718
			// (get) Token: 0x06006FBC RID: 28604 RVA: 0x0036812C File Offset: 0x0036632C
			// (set) Token: 0x06006FBD RID: 28605 RVA: 0x00368134 File Offset: 0x00366334
			public MedicalClinic.RequestResult Result { get; set; }

			// Token: 0x06006FBE RID: 28606 RVA: 0x00368140 File Offset: 0x00366340
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("CallbackOnlyRequest");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006FBF RID: 28607 RVA: 0x0036818C File Offset: 0x0036638C
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Result = ");
				builder.Append(this.Result.ToString());
				return true;
			}

			// Token: 0x06006FC0 RID: 28608 RVA: 0x003681C1 File Offset: 0x003663C1
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.CallbackOnlyRequest left, MedicalClinic.CallbackOnlyRequest right)
			{
				return !(left == right);
			}

			// Token: 0x06006FC1 RID: 28609 RVA: 0x003681CD File Offset: 0x003663CD
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.CallbackOnlyRequest left, MedicalClinic.CallbackOnlyRequest right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006FC2 RID: 28610 RVA: 0x003681D7 File Offset: 0x003663D7
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<MedicalClinic.RequestResult>.Default.GetHashCode(this.<Result>k__BackingField);
			}

			// Token: 0x06006FC3 RID: 28611 RVA: 0x003681E9 File Offset: 0x003663E9
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinic.CallbackOnlyRequest && this.Equals((MedicalClinic.CallbackOnlyRequest)obj);
			}

			// Token: 0x06006FC4 RID: 28612 RVA: 0x00368201 File Offset: 0x00366401
			[CompilerGenerated]
			public bool Equals(MedicalClinic.CallbackOnlyRequest other)
			{
				return EqualityComparer<MedicalClinic.RequestResult>.Default.Equals(this.<Result>k__BackingField, other.<Result>k__BackingField);
			}

			// Token: 0x06006FC5 RID: 28613 RVA: 0x00368219 File Offset: 0x00366419
			[CompilerGenerated]
			public void Deconstruct(out MedicalClinic.RequestResult Result)
			{
				Result = this.Result;
			}
		}

		// Token: 0x020008D4 RID: 2260
		[NullableContext(0)]
		public readonly struct HealRequest : IEquatable<MedicalClinic.HealRequest>
		{
			// Token: 0x06006FC6 RID: 28614 RVA: 0x00368223 File Offset: 0x00366423
			public HealRequest(MedicalClinic.RequestResult Result, MedicalClinic.HealRequestResult HealResult)
			{
				this.Result = Result;
				this.HealResult = HealResult;
			}

			// Token: 0x17001A3F RID: 6719
			// (get) Token: 0x06006FC7 RID: 28615 RVA: 0x00368233 File Offset: 0x00366433
			// (set) Token: 0x06006FC8 RID: 28616 RVA: 0x0036823B File Offset: 0x0036643B
			public MedicalClinic.RequestResult Result { get; set; }

			// Token: 0x17001A40 RID: 6720
			// (get) Token: 0x06006FC9 RID: 28617 RVA: 0x00368244 File Offset: 0x00366444
			// (set) Token: 0x06006FCA RID: 28618 RVA: 0x0036824C File Offset: 0x0036644C
			public MedicalClinic.HealRequestResult HealResult { get; set; }

			// Token: 0x06006FCB RID: 28619 RVA: 0x00368258 File Offset: 0x00366458
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("HealRequest");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006FCC RID: 28620 RVA: 0x003682A4 File Offset: 0x003664A4
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Result = ");
				builder.Append(this.Result.ToString());
				builder.Append(", HealResult = ");
				builder.Append(this.HealResult.ToString());
				return true;
			}

			// Token: 0x06006FCD RID: 28621 RVA: 0x00368300 File Offset: 0x00366500
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.HealRequest left, MedicalClinic.HealRequest right)
			{
				return !(left == right);
			}

			// Token: 0x06006FCE RID: 28622 RVA: 0x0036830C File Offset: 0x0036650C
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.HealRequest left, MedicalClinic.HealRequest right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006FCF RID: 28623 RVA: 0x00368316 File Offset: 0x00366516
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<MedicalClinic.RequestResult>.Default.GetHashCode(this.<Result>k__BackingField) * -1521134295 + EqualityComparer<MedicalClinic.HealRequestResult>.Default.GetHashCode(this.<HealResult>k__BackingField);
			}

			// Token: 0x06006FD0 RID: 28624 RVA: 0x0036833F File Offset: 0x0036653F
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinic.HealRequest && this.Equals((MedicalClinic.HealRequest)obj);
			}

			// Token: 0x06006FD1 RID: 28625 RVA: 0x00368357 File Offset: 0x00366557
			[CompilerGenerated]
			public bool Equals(MedicalClinic.HealRequest other)
			{
				return EqualityComparer<MedicalClinic.RequestResult>.Default.Equals(this.<Result>k__BackingField, other.<Result>k__BackingField) && EqualityComparer<MedicalClinic.HealRequestResult>.Default.Equals(this.<HealResult>k__BackingField, other.<HealResult>k__BackingField);
			}

			// Token: 0x06006FD2 RID: 28626 RVA: 0x00368389 File Offset: 0x00366589
			[CompilerGenerated]
			public void Deconstruct(out MedicalClinic.RequestResult Result, out MedicalClinic.HealRequestResult HealResult)
			{
				Result = this.Result;
				HealResult = this.HealResult;
			}
		}

		// Token: 0x020008D5 RID: 2261
		[NullableContext(0)]
		public enum NetworkHeader
		{
			// Token: 0x04003F80 RID: 16256
			REQUEST_AFFLICTIONS,
			// Token: 0x04003F81 RID: 16257
			AFFLICTION_UPDATE,
			// Token: 0x04003F82 RID: 16258
			UNSUBSCRIBE_ME,
			// Token: 0x04003F83 RID: 16259
			REQUEST_PENDING,
			// Token: 0x04003F84 RID: 16260
			ADD_PENDING,
			// Token: 0x04003F85 RID: 16261
			REMOVE_PENDING,
			// Token: 0x04003F86 RID: 16262
			CLEAR_PENDING,
			// Token: 0x04003F87 RID: 16263
			HEAL_PENDING,
			// Token: 0x04003F88 RID: 16264
			ADD_EVERYTHING_TO_PENDING
		}

		// Token: 0x020008D6 RID: 2262
		[NullableContext(0)]
		public enum AfflictionSeverity
		{
			// Token: 0x04003F8A RID: 16266
			Low,
			// Token: 0x04003F8B RID: 16267
			Medium,
			// Token: 0x04003F8C RID: 16268
			High
		}

		// Token: 0x020008D7 RID: 2263
		[NullableContext(0)]
		public enum MessageFlag
		{
			// Token: 0x04003F8E RID: 16270
			Response,
			// Token: 0x04003F8F RID: 16271
			Announce
		}

		// Token: 0x020008D8 RID: 2264
		[NullableContext(0)]
		public enum HealRequestResult
		{
			// Token: 0x04003F91 RID: 16273
			Unknown,
			// Token: 0x04003F92 RID: 16274
			Success,
			// Token: 0x04003F93 RID: 16275
			InsufficientFunds,
			// Token: 0x04003F94 RID: 16276
			Refused
		}

		// Token: 0x020008D9 RID: 2265
		[NullableContext(0)]
		[NetworkSerialize(51)]
		public readonly struct NetHealRequest : INetSerializableStruct, IEquatable<MedicalClinic.NetHealRequest>
		{
			// Token: 0x06006FD3 RID: 28627 RVA: 0x0036839B File Offset: 0x0036659B
			public NetHealRequest(MedicalClinic.HealRequestResult Result)
			{
				this.Result = Result;
			}

			// Token: 0x17001A41 RID: 6721
			// (get) Token: 0x06006FD4 RID: 28628 RVA: 0x003683A4 File Offset: 0x003665A4
			// (set) Token: 0x06006FD5 RID: 28629 RVA: 0x003683AC File Offset: 0x003665AC
			public MedicalClinic.HealRequestResult Result { get; set; }

			// Token: 0x06006FD6 RID: 28630 RVA: 0x003683B8 File Offset: 0x003665B8
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetHealRequest");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006FD7 RID: 28631 RVA: 0x00368404 File Offset: 0x00366604
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Result = ");
				builder.Append(this.Result.ToString());
				return true;
			}

			// Token: 0x06006FD8 RID: 28632 RVA: 0x00368439 File Offset: 0x00366639
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.NetHealRequest left, MedicalClinic.NetHealRequest right)
			{
				return !(left == right);
			}

			// Token: 0x06006FD9 RID: 28633 RVA: 0x00368445 File Offset: 0x00366645
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.NetHealRequest left, MedicalClinic.NetHealRequest right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006FDA RID: 28634 RVA: 0x0036844F File Offset: 0x0036664F
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<MedicalClinic.HealRequestResult>.Default.GetHashCode(this.<Result>k__BackingField);
			}

			// Token: 0x06006FDB RID: 28635 RVA: 0x00368461 File Offset: 0x00366661
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinic.NetHealRequest && this.Equals((MedicalClinic.NetHealRequest)obj);
			}

			// Token: 0x06006FDC RID: 28636 RVA: 0x00368479 File Offset: 0x00366679
			[CompilerGenerated]
			public bool Equals(MedicalClinic.NetHealRequest other)
			{
				return EqualityComparer<MedicalClinic.HealRequestResult>.Default.Equals(this.<Result>k__BackingField, other.<Result>k__BackingField);
			}

			// Token: 0x06006FDD RID: 28637 RVA: 0x00368491 File Offset: 0x00366691
			[CompilerGenerated]
			public void Deconstruct(out MedicalClinic.HealRequestResult Result)
			{
				Result = this.Result;
			}
		}

		// Token: 0x020008DA RID: 2266
		[NullableContext(0)]
		[NetworkSerialize(54)]
		public readonly struct NetRemovedAffliction : INetSerializableStruct, IEquatable<MedicalClinic.NetRemovedAffliction>
		{
			// Token: 0x06006FDE RID: 28638 RVA: 0x0036849B File Offset: 0x0036669B
			public NetRemovedAffliction(MedicalClinic.NetCrewMember CrewMember, MedicalClinic.NetAffliction Affliction)
			{
				this.CrewMember = CrewMember;
				this.Affliction = Affliction;
			}

			// Token: 0x17001A42 RID: 6722
			// (get) Token: 0x06006FDF RID: 28639 RVA: 0x003684AB File Offset: 0x003666AB
			// (set) Token: 0x06006FE0 RID: 28640 RVA: 0x003684B3 File Offset: 0x003666B3
			public MedicalClinic.NetCrewMember CrewMember { get; set; }

			// Token: 0x17001A43 RID: 6723
			// (get) Token: 0x06006FE1 RID: 28641 RVA: 0x003684BC File Offset: 0x003666BC
			// (set) Token: 0x06006FE2 RID: 28642 RVA: 0x003684C4 File Offset: 0x003666C4
			public MedicalClinic.NetAffliction Affliction { get; set; }

			// Token: 0x06006FE3 RID: 28643 RVA: 0x003684D0 File Offset: 0x003666D0
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetRemovedAffliction");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006FE4 RID: 28644 RVA: 0x0036851C File Offset: 0x0036671C
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("CrewMember = ");
				builder.Append(this.CrewMember.ToString());
				builder.Append(", Affliction = ");
				builder.Append(this.Affliction.ToString());
				return true;
			}

			// Token: 0x06006FE5 RID: 28645 RVA: 0x00368578 File Offset: 0x00366778
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.NetRemovedAffliction left, MedicalClinic.NetRemovedAffliction right)
			{
				return !(left == right);
			}

			// Token: 0x06006FE6 RID: 28646 RVA: 0x00368584 File Offset: 0x00366784
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.NetRemovedAffliction left, MedicalClinic.NetRemovedAffliction right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006FE7 RID: 28647 RVA: 0x0036858E File Offset: 0x0036678E
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<MedicalClinic.NetCrewMember>.Default.GetHashCode(this.<CrewMember>k__BackingField) * -1521134295 + EqualityComparer<MedicalClinic.NetAffliction>.Default.GetHashCode(this.<Affliction>k__BackingField);
			}

			// Token: 0x06006FE8 RID: 28648 RVA: 0x003685B7 File Offset: 0x003667B7
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is MedicalClinic.NetRemovedAffliction && this.Equals((MedicalClinic.NetRemovedAffliction)obj);
			}

			// Token: 0x06006FE9 RID: 28649 RVA: 0x003685CF File Offset: 0x003667CF
			[CompilerGenerated]
			public bool Equals(MedicalClinic.NetRemovedAffliction other)
			{
				return EqualityComparer<MedicalClinic.NetCrewMember>.Default.Equals(this.<CrewMember>k__BackingField, other.<CrewMember>k__BackingField) && EqualityComparer<MedicalClinic.NetAffliction>.Default.Equals(this.<Affliction>k__BackingField, other.<Affliction>k__BackingField);
			}

			// Token: 0x06006FEA RID: 28650 RVA: 0x00368601 File Offset: 0x00366801
			[CompilerGenerated]
			public void Deconstruct(out MedicalClinic.NetCrewMember CrewMember, out MedicalClinic.NetAffliction Affliction)
			{
				CrewMember = this.CrewMember;
				Affliction = this.Affliction;
			}
		}

		// Token: 0x020008DB RID: 2267
		[NullableContext(2)]
		[Nullable(0)]
		public struct NetAffliction : INetSerializableStruct
		{
			// Token: 0x06006FEB RID: 28651 RVA: 0x0036861C File Offset: 0x0036681C
			[NullableContext(1)]
			public void SetAffliction(Affliction affliction, CharacterHealth characterHealth)
			{
				this.Identifier = affliction.Identifier;
				this.Strength = (ushort)Math.Ceiling((double)affliction.Strength);
				this.Price = (ushort)((float)affliction.Prefab.BaseHealCost + (float)this.Strength * affliction.Prefab.HealCostMultiplier);
				this.VitalityDecrease = (int)affliction.GetVitalityDecrease(characterHealth);
			}

			// Token: 0x17001A44 RID: 6724
			// (get) Token: 0x06006FEC RID: 28652 RVA: 0x00368680 File Offset: 0x00366880
			// (set) Token: 0x06006FED RID: 28653 RVA: 0x003686F4 File Offset: 0x003668F4
			public AfflictionPrefab Prefab
			{
				get
				{
					AfflictionPrefab cached = this.cachedPrefab;
					if (cached != null)
					{
						return cached;
					}
					foreach (AfflictionPrefab prefab in AfflictionPrefab.List)
					{
						if (prefab.Identifier == this.Identifier)
						{
							this.cachedPrefab = prefab;
							return prefab;
						}
					}
					return null;
				}
				set
				{
					this.cachedPrefab = value;
					this.Identifier = ((value != null) ? value.Identifier : Identifier.Empty);
					this.Strength = 0;
					this.Price = 0;
				}
			}

			// Token: 0x06006FEE RID: 28654 RVA: 0x00368721 File Offset: 0x00366921
			[NullableContext(1)]
			public readonly bool AfflictionEquals(AfflictionPrefab prefab)
			{
				return prefab.Identifier == this.Identifier;
			}

			// Token: 0x06006FEF RID: 28655 RVA: 0x00368734 File Offset: 0x00366934
			public readonly bool AfflictionEquals(MedicalClinic.NetAffliction affliction)
			{
				return affliction.Identifier == this.Identifier;
			}

			// Token: 0x04003F98 RID: 16280
			[NetworkSerialize(59)]
			public Identifier Identifier;

			// Token: 0x04003F99 RID: 16281
			[NetworkSerialize(62)]
			public ushort Strength;

			// Token: 0x04003F9A RID: 16282
			[NetworkSerialize(65)]
			public int VitalityDecrease;

			// Token: 0x04003F9B RID: 16283
			[NetworkSerialize(68)]
			public ushort Price;

			// Token: 0x04003F9C RID: 16284
			private AfflictionPrefab cachedPrefab;
		}

		// Token: 0x020008DC RID: 2268
		[NullableContext(0)]
		public struct NetCrewMember : INetSerializableStruct, IEquatable<MedicalClinic.NetCrewMember>
		{
			// Token: 0x06006FF0 RID: 28656 RVA: 0x00368748 File Offset: 0x00366948
			[NullableContext(1)]
			public NetCrewMember(CharacterInfo info)
			{
				this.CharacterInfoID = (int)info.ID;
				this.Afflictions = ImmutableArray<MedicalClinic.NetAffliction>.Empty;
			}

			// Token: 0x06006FF1 RID: 28657 RVA: 0x00368761 File Offset: 0x00366961
			public NetCrewMember([Nullable(1)] CharacterInfo info, ImmutableArray<MedicalClinic.NetAffliction> afflictions)
			{
				this = new MedicalClinic.NetCrewMember(info);
				this.Afflictions = afflictions;
			}

			// Token: 0x06006FF2 RID: 28658 RVA: 0x00368774 File Offset: 0x00366974
			[NullableContext(2)]
			public readonly CharacterInfo FindCharacterInfo([Nullable(new byte[]
			{
				0,
				1
			})] ImmutableArray<CharacterInfo> crew)
			{
				foreach (CharacterInfo info in crew)
				{
					if ((int)info.ID == this.CharacterInfoID)
					{
						return info;
					}
				}
				return null;
			}

			// Token: 0x06006FF3 RID: 28659 RVA: 0x003687AD File Offset: 0x003669AD
			public readonly bool CharacterEquals(MedicalClinic.NetCrewMember crewMember)
			{
				return crewMember.CharacterInfoID == this.CharacterInfoID;
			}

			// Token: 0x06006FF4 RID: 28660 RVA: 0x003687C0 File Offset: 0x003669C0
			[CompilerGenerated]
			public override readonly string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetCrewMember");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006FF5 RID: 28661 RVA: 0x0036880C File Offset: 0x00366A0C
			[CompilerGenerated]
			private readonly bool PrintMembers(StringBuilder builder)
			{
				builder.Append("CharacterInfoID = ");
				builder.Append(this.CharacterInfoID.ToString());
				builder.Append(", Afflictions = ");
				builder.Append(this.Afflictions.ToString());
				return true;
			}

			// Token: 0x06006FF6 RID: 28662 RVA: 0x00368862 File Offset: 0x00366A62
			[CompilerGenerated]
			public static bool operator !=(MedicalClinic.NetCrewMember left, MedicalClinic.NetCrewMember right)
			{
				return !(left == right);
			}

			// Token: 0x06006FF7 RID: 28663 RVA: 0x0036886E File Offset: 0x00366A6E
			[CompilerGenerated]
			public static bool operator ==(MedicalClinic.NetCrewMember left, MedicalClinic.NetCrewMember right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006FF8 RID: 28664 RVA: 0x00368878 File Offset: 0x00366A78
			[CompilerGenerated]
			public override readonly int GetHashCode()
			{
				return EqualityComparer<int>.Default.GetHashCode(this.CharacterInfoID) * -1521134295 + EqualityComparer<ImmutableArray<MedicalClinic.NetAffliction>>.Default.GetHashCode(this.Afflictions);
			}

			// Token: 0x06006FF9 RID: 28665 RVA: 0x003688A1 File Offset: 0x00366AA1
			[CompilerGenerated]
			public override readonly bool Equals(object obj)
			{
				return obj is MedicalClinic.NetCrewMember && this.Equals((MedicalClinic.NetCrewMember)obj);
			}

			// Token: 0x06006FFA RID: 28666 RVA: 0x003688B9 File Offset: 0x00366AB9
			[CompilerGenerated]
			public readonly bool Equals(MedicalClinic.NetCrewMember other)
			{
				return EqualityComparer<int>.Default.Equals(this.CharacterInfoID, other.CharacterInfoID) && EqualityComparer<ImmutableArray<MedicalClinic.NetAffliction>>.Default.Equals(this.Afflictions, other.Afflictions);
			}

			// Token: 0x04003F9D RID: 16285
			[NetworkSerialize(120)]
			public int CharacterInfoID;

			// Token: 0x04003F9E RID: 16286
			[NetworkSerialize(123)]
			public ImmutableArray<MedicalClinic.NetAffliction> Afflictions;
		}

		// Token: 0x020008DD RID: 2269
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003F9F RID: 16287
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Action<Action<MedicalClinic.CallbackOnlyRequest>> <0>__CallbackOnlyTimeout;

			// Token: 0x04003FA0 RID: 16288
			[Nullable(0)]
			public static Func<Affliction, bool> <1>__IsHealable;
		}
	}
}
