using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using Barotrauma.Eos;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.PerkBehaviors;
using Barotrauma.Steam;
using FarseerPhysics;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x0200036C RID: 876
	internal sealed class GameServer : NetworkMember
	{
		// Token: 0x17000E69 RID: 3689
		// (get) Token: 0x060033D8 RID: 13272 RVA: 0x0015E68C File Offset: 0x0015C88C
		public override bool IsServer
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000E6A RID: 3690
		// (get) Token: 0x060033D9 RID: 13273 RVA: 0x0015E68F File Offset: 0x0015C88F
		public override bool IsClient
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000E6B RID: 3691
		// (get) Token: 0x060033DA RID: 13274 RVA: 0x0015E692 File Offset: 0x0015C892
		public override Voting Voting { get; }

		// Token: 0x17000E6C RID: 3692
		// (get) Token: 0x060033DB RID: 13275 RVA: 0x0015E69A File Offset: 0x0015C89A
		// (set) Token: 0x060033DC RID: 13276 RVA: 0x0015E6A7 File Offset: 0x0015C8A7
		public string ServerName
		{
			get
			{
				return base.ServerSettings.ServerName;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					return;
				}
				base.ServerSettings.ServerName = value;
			}
		}

		// Token: 0x17000E6D RID: 3693
		// (get) Token: 0x060033DD RID: 13277 RVA: 0x0015E6BE File Offset: 0x0015C8BE
		public ServerPeer ServerPeer
		{
			get
			{
				return this.serverPeer;
			}
		}

		// Token: 0x17000E6E RID: 3694
		// (get) Token: 0x060033DE RID: 13278 RVA: 0x0015E6C6 File Offset: 0x0015C8C6
		// (set) Token: 0x060033DF RID: 13279 RVA: 0x0015E6CE File Offset: 0x0015C8CE
		public float EndRoundTimer { get; private set; }

		// Token: 0x17000E6F RID: 3695
		// (get) Token: 0x060033E0 RID: 13280 RVA: 0x0015E6D7 File Offset: 0x0015C8D7
		// (set) Token: 0x060033E1 RID: 13281 RVA: 0x0015E6DF File Offset: 0x0015C8DF
		public float EndRoundDelay { get; private set; }

		// Token: 0x17000E70 RID: 3696
		// (get) Token: 0x060033E2 RID: 13282 RVA: 0x0015E6E8 File Offset: 0x0015C8E8
		public float EndRoundTimeRemaining
		{
			get
			{
				if (this.EndRoundTimer <= 0f)
				{
					return 0f;
				}
				return this.EndRoundDelay - this.EndRoundTimer;
			}
		}

		// Token: 0x17000E71 RID: 3697
		// (get) Token: 0x060033E3 RID: 13283 RVA: 0x0015E70A File Offset: 0x0015C90A
		private int Team1Count
		{
			get
			{
				return this.GetPlayingClients().Count((Client c) => c.TeamID == CharacterTeamType.Team1);
			}
		}

		// Token: 0x17000E72 RID: 3698
		// (get) Token: 0x060033E4 RID: 13284 RVA: 0x0015E736 File Offset: 0x0015C936
		private int Team2Count
		{
			get
			{
				return this.GetPlayingClients().Count((Client c) => c.TeamID == CharacterTeamType.Team2);
			}
		}

		// Token: 0x17000E73 RID: 3699
		// (get) Token: 0x060033E5 RID: 13285 RVA: 0x0015E762 File Offset: 0x0015C962
		// (set) Token: 0x060033E6 RID: 13286 RVA: 0x0015E76A File Offset: 0x0015C96A
		public VoipServer VoipServer { get; private set; }

		// Token: 0x17000E74 RID: 3700
		// (get) Token: 0x060033E7 RID: 13287 RVA: 0x0015E773 File Offset: 0x0015C973
		// (set) Token: 0x060033E8 RID: 13288 RVA: 0x0015E77B File Offset: 0x0015C97B
		public FileSender FileSender { get; private set; }

		// Token: 0x17000E75 RID: 3701
		// (get) Token: 0x060033E9 RID: 13289 RVA: 0x0015E784 File Offset: 0x0015C984
		// (set) Token: 0x060033EA RID: 13290 RVA: 0x0015E78C File Offset: 0x0015C98C
		public ModSender ModSender { get; private set; }

		// Token: 0x17000E76 RID: 3702
		// (get) Token: 0x060033EB RID: 13291 RVA: 0x0015E795 File Offset: 0x0015C995
		public TraitorManager TraitorManager
		{
			get
			{
				if (this.traitorManager == null)
				{
					this.traitorManager = new TraitorManager(this);
				}
				return this.traitorManager;
			}
		}

		// Token: 0x17000E77 RID: 3703
		// (get) Token: 0x060033EC RID: 13292 RVA: 0x0015E7B1 File Offset: 0x0015C9B1
		public override IReadOnlyList<Client> ConnectedClients
		{
			get
			{
				return this.connectedClients;
			}
		}

		// Token: 0x17000E78 RID: 3704
		// (get) Token: 0x060033ED RID: 13293 RVA: 0x0015E7B9 File Offset: 0x0015C9B9
		public ServerEntityEventManager EntityEventManager
		{
			get
			{
				return this.entityEventManager;
			}
		}

		// Token: 0x17000E79 RID: 3705
		// (get) Token: 0x060033EE RID: 13294 RVA: 0x0015E7C1 File Offset: 0x0015C9C1
		public int Port
		{
			get
			{
				ServerSettings serverSettings = base.ServerSettings;
				if (serverSettings == null)
				{
					return 0;
				}
				return serverSettings.Port;
			}
		}

		// Token: 0x17000E7A RID: 3706
		// (get) Token: 0x060033EF RID: 13295 RVA: 0x0015E7D4 File Offset: 0x0015C9D4
		public int QueryPort
		{
			get
			{
				ServerSettings serverSettings = base.ServerSettings;
				if (serverSettings == null)
				{
					return 0;
				}
				return serverSettings.QueryPort;
			}
		}

		// Token: 0x17000E7B RID: 3707
		// (get) Token: 0x060033F0 RID: 13296 RVA: 0x0015E7E7 File Offset: 0x0015C9E7
		// (set) Token: 0x060033F1 RID: 13297 RVA: 0x0015E7EF File Offset: 0x0015C9EF
		public NetworkConnection OwnerConnection { get; private set; }

		// Token: 0x060033F2 RID: 13298 RVA: 0x0015E7F8 File Offset: 0x0015C9F8
		public void ClearRecentlyDisconnectedClients()
		{
			List<Client> obj = this.clientsAttemptingToReconnectSoon;
			lock (obj)
			{
				this.clientsAttemptingToReconnectSoon.Clear();
			}
		}

		// Token: 0x060033F3 RID: 13299 RVA: 0x0015E840 File Offset: 0x0015CA40
		public bool FindAndRemoveRecentlyDisconnectedConnection(NetworkConnection conn)
		{
			List<Client> obj = this.clientsAttemptingToReconnectSoon;
			lock (obj)
			{
				Client found = null;
				foreach (Client client in this.clientsAttemptingToReconnectSoon)
				{
					if (conn.AddressMatches(client.Connection))
					{
						found = client;
						break;
					}
				}
				if (found != null)
				{
					this.clientsAttemptingToReconnectSoon.Remove(found);
					return true;
				}
			}
			return false;
		}

		// Token: 0x060033F4 RID: 13300 RVA: 0x0015E8E8 File Offset: 0x0015CAE8
		public GameServer(string name, IPAddress listenIp, int port, int queryPort, bool isPublic, string password, bool attemptUPnP, int maxPlayers, Option<int> ownerKey, Option<P2PEndpoint> ownerEndpoint)
		{
			if (name.Length > NetConfig.ServerNameMaxLength)
			{
				name = name.Substring(0, NetConfig.ServerNameMaxLength);
			}
			base.LastClientListUpdateID = 0;
			base.ServerSettings = new ServerSettings(this, name, port, queryPort, maxPlayers, isPublic, attemptUPnP, listenIp);
			base.KarmaManager.SelectPreset(base.ServerSettings.KarmaPreset);
			base.ServerSettings.SetPassword(password);
			base.ServerSettings.SaveSettings();
			this.Voting = new Voting();
			this.ownerKey = ownerKey;
			this.ownerEndpoint = ownerEndpoint;
			this.entityEventManager = new ServerEntityEventManager(this);
		}

		// Token: 0x060033F5 RID: 13301 RVA: 0x0015EA00 File Offset: 0x0015CC00
		public void StartServer(bool registerToServerList)
		{
			GameServer.Log("Starting the server...", ServerLog.MessageType.ServerMessage);
			ServerPeer.Callbacks callbacks = new ServerPeer.Callbacks(new ServerPeer.Callbacks.MessageCallback(this.ReadDataMessage), new ServerPeer.Callbacks.DisconnectCallback(this.OnClientDisconnect), new ServerPeer.Callbacks.InitializationCompleteCallback(this.OnInitializationComplete), new ServerPeer.Callbacks.ShutdownCallback(GameMain.Instance.CloseServer), new ServerPeer.Callbacks.OwnerDeterminedCallback(this.OnOwnerDetermined));
			P2PEndpoint endpoint;
			if (this.ownerEndpoint.TryUnwrap(out endpoint))
			{
				GameServer.Log("Using P2P networking.", ServerLog.MessageType.ServerMessage);
				this.serverPeer = new P2PServerPeer(endpoint, this.ownerKey.Fallback(0), base.ServerSettings, callbacks);
			}
			else
			{
				GameServer.Log("Using Lidgren networking. Manual port forwarding may be required. If players cannot connect to the server, you may want to use the in-game hosting menu (which uses Steamworks and EOS networking and does not require port forwarding).", ServerLog.MessageType.ServerMessage);
				this.serverPeer = new LidgrenServerPeer(this.ownerKey, base.ServerSettings, callbacks);
				if (registerToServerList)
				{
					try
					{
						this.registeredToSteamMaster = SteamManager.CreateServer(this, base.ServerSettings.IsPublic);
					}
					catch (Exception e)
					{
						DebugConsole.NewMessage("Steam registering skipped due to error (and probably more of it was printed above): " + e.Message, null, false);
					}
					Option.UnspecifiedNone none = Option.None;
					EosSessionManager.UpdateOwnedSession(none, base.ServerSettings);
				}
			}
			this.FileSender = new FileSender(this.serverPeer, 1170);
			FileSender fileSender = this.FileSender;
			fileSender.OnEnded = (FileSender.FileTransferDelegate)Delegate.Combine(fileSender.OnEnded, new FileSender.FileTransferDelegate(this.FileTransferChanged));
			FileSender fileSender2 = this.FileSender;
			fileSender2.OnStarted = (FileSender.FileTransferDelegate)Delegate.Combine(fileSender2.OnStarted, new FileSender.FileTransferDelegate(this.FileTransferChanged));
			if (base.ServerSettings.AllowModDownloads)
			{
				this.ModSender = new ModSender();
			}
			this.serverPeer.Start();
			this.VoipServer = new VoipServer(this.serverPeer);
			GameServer.Log("Server started", ServerLog.MessageType.ServerMessage);
			GameMain.NetLobbyScreen.Select();
			GameMain.NetLobbyScreen.RandomizeSettings();
			if (!string.IsNullOrEmpty(base.ServerSettings.SelectedSubmarine))
			{
				SubmarineInfo sub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == base.ServerSettings.SelectedSubmarine);
				if (sub != null)
				{
					GameMain.NetLobbyScreen.SelectedSub = sub;
				}
			}
			if (!string.IsNullOrEmpty(base.ServerSettings.SelectedShuttle))
			{
				SubmarineInfo shuttle = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == base.ServerSettings.SelectedShuttle);
				if (shuttle != null)
				{
					GameMain.NetLobbyScreen.SelectedShuttle = shuttle;
				}
			}
			this.started = true;
			GameAnalyticsManager.AddDesignEvent("GameServer:Start");
		}

		// Token: 0x060033F6 RID: 13302 RVA: 0x0015EC60 File Offset: 0x0015CE60
		public static void AddPendingMessageToOwner(string message, ChatMessageType messageType)
		{
			GameServer.pendingMessagesToOwner.Enqueue(ChatMessage.Create(string.Empty, message, messageType, null, null, PlayerConnectionChangeType.None, null));
		}

		// Token: 0x060033F7 RID: 13303 RVA: 0x0015EC90 File Offset: 0x0015CE90
		private void OnOwnerDetermined(NetworkConnection connection)
		{
			this.OwnerConnection = connection;
			Client ownerClient = this.ConnectedClients.Find((Client c) => c.Connection == connection);
			if (ownerClient == null)
			{
				DebugConsole.ThrowError("Owner client not found! Can't set permissions", null, null, false, false);
				return;
			}
			ownerClient.SetPermissions(ClientPermissions.All, DebugConsole.Commands);
			this.UpdateClientPermissions(ownerClient);
		}

		// Token: 0x060033F8 RID: 13304 RVA: 0x0015ECF8 File Offset: 0x0015CEF8
		public void NotifyCrash()
		{
			List<Client> tempList = (from c in this.ConnectedClients
			where c.Connection != this.OwnerConnection
			select c).ToList<Client>();
			foreach (Client c2 in tempList)
			{
				this.DisconnectClient(c2.Connection, PeerDisconnectPacket.WithReason(DisconnectReason.ServerCrashed));
			}
			if (this.OwnerConnection != null)
			{
				NetworkConnection conn = this.OwnerConnection;
				this.OwnerConnection = null;
				this.DisconnectClient(conn, PeerDisconnectPacket.WithReason(DisconnectReason.ServerCrashed));
			}
			Thread.Sleep(500);
		}

		// Token: 0x060033F9 RID: 13305 RVA: 0x0015ED9C File Offset: 0x0015CF9C
		private void OnInitializationComplete(NetworkConnection connection, string clientName)
		{
			clientName = Client.SanitizeName(clientName, 32);
			Client newClient = new Client(clientName, this.GetNewClientSessionId());
			newClient.InitClientSync();
			newClient.Connection = connection;
			newClient.Connection.Status = NetworkConnectionStatus.Connected;
			newClient.AccountInfo = connection.AccountInfo;
			newClient.Language = connection.Language;
			this.connectedClients.Add(newClient);
			PreviousPlayer previousPlayer = this.previousPlayers.Find((PreviousPlayer p) => p.MatchesClient(newClient));
			if (previousPlayer != null)
			{
				newClient.Karma = previousPlayer.Karma;
				newClient.KarmaKickCount = previousPlayer.KarmaKickCount;
				foreach (Client c in previousPlayer.KickVoters)
				{
					if (this.connectedClients.Contains(c))
					{
						newClient.AddKickVote(c);
					}
				}
			}
			ushort lastClientListUpdateID = base.LastClientListUpdateID;
			base.LastClientListUpdateID = lastClientListUpdateID + 1;
			if (newClient.Connection == this.OwnerConnection && this.OwnerConnection != null)
			{
				newClient.GivePermission(ClientPermissions.All);
				foreach (DebugConsole.Command command in DebugConsole.Commands)
				{
					newClient.PermittedConsoleCommands.Add(command);
				}
				this.SendConsoleMessage("Granted all permissions to " + newClient.Name + ".", newClient, null);
			}
			this.SendChatMessage("ServerMessage.JoinedServer~[client]=" + NetworkMember.ClientLogName(newClient, null), new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.Joined, ChatMode.None);
			base.ServerSettings.ServerDetailsChanged = true;
			if (previousPlayer != null && previousPlayer.Name != newClient.Name)
			{
				string prevNameSanitized = previousPlayer.Name.Replace("‖", "");
				this.SendChatMessage("ServerMessage.PreviousClientName~[client]=" + NetworkMember.ClientLogName(newClient, null) + "~[previousname]=" + prevNameSanitized, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
				previousPlayer.Name = newClient.Name;
			}
			if (!base.ServerSettings.ServerMessageText.IsNullOrEmpty())
			{
				this.SendDirectChatMessage((TextManager.Get("servermotd") + '\n' + base.ServerSettings.ServerMessageText).Value, newClient, ChatMessageType.Server);
			}
			ServerSettings.SavedClientPermission savedPermissions = base.ServerSettings.ClientPermissions.Find(delegate(ServerSettings.SavedClientPermission scp)
			{
				AccountId accountId;
				if (!scp.AddressOrAccountId.TryGet(out accountId))
				{
					return newClient.Connection.Endpoint.Address == scp.AddressOrAccountId;
				}
				return newClient.AccountId.ValueEquals(accountId);
			});
			if (savedPermissions != null)
			{
				newClient.SetPermissions(savedPermissions.Permissions, savedPermissions.PermittedCommands);
			}
			else
			{
				PermissionPreset defaultPerms = PermissionPreset.List.Find((PermissionPreset p) => p.Identifier == "None");
				if (defaultPerms != null)
				{
					newClient.SetPermissions(defaultPerms.Permissions, defaultPerms.PermittedCommands);
				}
				else
				{
					newClient.SetPermissions(ClientPermissions.None, Enumerable.Empty<DebugConsole.Command>());
				}
			}
			this.UpdateClientPermissions(newClient);
			foreach (Client otherClient in this.connectedClients)
			{
				if (otherClient != newClient)
				{
					CoroutineManager.StartCoroutine(this.SendClientPermissionsAfterClientListSynced(newClient, otherClient), "");
				}
			}
		}

		// Token: 0x060033FA RID: 13306 RVA: 0x0015F174 File Offset: 0x0015D374
		private void OnClientDisconnect(NetworkConnection connection, PeerDisconnectPacket peerDisconnectPacket)
		{
			Client connectedClient = this.connectedClients.Find((Client c) => c.Connection == connection);
			this.DisconnectClient(connectedClient, peerDisconnectPacket);
		}

		// Token: 0x060033FB RID: 13307 RVA: 0x0015F1B0 File Offset: 0x0015D3B0
		public void Update(float deltaTime)
		{
			this.dosProtection.Update(deltaTime);
			if (!this.started)
			{
				return;
			}
			if (ChildServerRelay.HasShutDown)
			{
				GameMain.Instance.CloseServer();
				return;
			}
			this.FileSender.Update(deltaTime);
			base.KarmaManager.UpdateClients(this.ConnectedClients, deltaTime);
			this.UpdatePing();
			if (base.ServerSettings.VoiceChatEnabled)
			{
				this.VoipServer.SendToClients(this.connectedClients);
				foreach (Client c5 in this.connectedClients)
				{
					c5.VoipServerDecoder.DebugUpdate(deltaTime);
				}
			}
			if (base.GameStarted)
			{
				RespawnManager respawnManager = base.RespawnManager;
				if (respawnManager != null)
				{
					respawnManager.Update(deltaTime);
				}
				this.entityEventManager.Update(this.connectedClients);
				bool permadeathMode = base.ServerSettings.RespawnMode == RespawnMode.Permadeath;
				for (int i = Character.CharacterList.Count - 1; i >= 0; i--)
				{
					Character character = Character.CharacterList[i];
					Client owner = this.connectedClients.Find((Client c) => (c.Character == null || c.Character == character) && character.IsClientOwner(c));
					bool spectating = owner != null && owner.SpectateOnly && base.ServerSettings.AllowSpectating;
					if (character.ClientDisconnected || spectating)
					{
						bool flag;
						if (owner != null && owner.InGame && !owner.NeedsMidRoundSync)
						{
							if (spectating)
							{
								if (permadeathMode)
								{
									if (character.IsDead)
									{
										CauseOfDeath causeOfDeath = character.CauseOfDeath;
										flag = (causeOfDeath != null && causeOfDeath.Type == CauseOfDeathType.Disconnected);
									}
									else
									{
										flag = true;
									}
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								flag = true;
							}
						}
						else
						{
							flag = false;
						}
						bool canOwnerTakeControl = flag;
						if (!character.IsDead)
						{
							if (!LuaCsSetup.Instance.Game.disableDisconnectCharacter)
							{
								character.KillDisconnectedTimer += deltaTime;
								character.SetStun(1f, false, false);
							}
							float killTime = permadeathMode ? base.ServerSettings.DespawnDisconnectedPermadeathTime : base.ServerSettings.KillDisconnectedTime;
							if (spectating)
							{
								killTime = 0f;
							}
							if ((this.OwnerConnection == null || ((owner != null) ? owner.Connection : null) != this.OwnerConnection) && character.KillDisconnectedTimer > killTime)
							{
								character.Kill(CauseOfDeathType.Disconnected, null, false, true);
							}
							else if (canOwnerTakeControl)
							{
								this.SetClientCharacter(owner, character);
							}
						}
						else if (canOwnerTakeControl)
						{
							CauseOfDeath causeOfDeath2 = character.CauseOfDeath;
							if (causeOfDeath2 != null && causeOfDeath2.Type == CauseOfDeathType.Disconnected && character.CharacterHealth.VitalityDisregardingDeath > 0f)
							{
								character.Revive(false, true);
								this.SetClientCharacter(owner, character);
							}
						}
					}
				}
				TraitorManager traitorManager = this.TraitorManager;
				if (traitorManager != null)
				{
					traitorManager.Update(deltaTime);
				}
				this.Voting.Update(deltaTime);
				bool isCrewDown = this.connectedClients.All((Client c) => !c.UsingFreeCam && (c.Character == null || c.Character.IsDead || c.Character.IsIncapacitated));
				bool isSomeoneIncapacitatedNotDead = this.connectedClients.Any(delegate(Client c)
				{
					if (!c.UsingFreeCam)
					{
						Character character3 = c.Character;
						return character3 != null && !character3.IsDead && character3.IsIncapacitated;
					}
					return false;
				});
				bool subAtLevelEnd = false;
				if (Submarine.MainSub != null && !(GameMain.GameSession.GameMode is PvPMode))
				{
					Level loaded = Level.Loaded;
					if (((loaded != null) ? loaded.EndOutpost : null) != null)
					{
						int charactersInsideOutpost = this.connectedClients.Count((Client c) => c.Character != null && !c.Character.IsDead && !c.Character.IsUnconscious && c.Character.Submarine == Level.Loaded.EndOutpost);
						int charactersOutsideOutpost = this.connectedClients.Count((Client c) => c.Character != null && !c.Character.IsDead && !c.Character.IsUnconscious && c.Character.Submarine != Level.Loaded.EndOutpost);
						subAtLevelEnd = (Submarine.MainSub.DockedTo.Contains(Level.Loaded.EndOutpost) || (Submarine.MainSub.AtEndExit && charactersInsideOutpost > 0) || charactersInsideOutpost > charactersOutsideOutpost);
					}
					else
					{
						subAtLevelEnd = Submarine.MainSub.AtEndExit;
					}
				}
				this.EndRoundDelay = 1f;
				if (permadeathMode && isCrewDown)
				{
					if (this.EndRoundTimer <= 0f)
					{
						this.CreateEntityEvent(base.RespawnManager, null);
					}
					this.EndRoundDelay = 120f;
					this.EndRoundTimer += deltaTime;
				}
				else if (base.ServerSettings.AutoRestart && isCrewDown)
				{
					this.EndRoundDelay = (isSomeoneIncapacitatedNotDead ? 120f : 5f);
					this.EndRoundTimer += deltaTime;
				}
				else
				{
					if (subAtLevelEnd)
					{
						GameSession gameSession = GameMain.GameSession;
						if (!(((gameSession != null) ? gameSession.GameMode : null) is CampaignMode))
						{
							this.EndRoundDelay = 5f;
							this.EndRoundTimer += deltaTime;
							goto IL_5A5;
						}
					}
					if (isCrewDown && (base.RespawnManager == null || (!base.RespawnManager.CanRespawnAgain(CharacterTeamType.Team1) && !base.RespawnManager.CanRespawnAgain(CharacterTeamType.Team2))))
					{
						if (this.EndRoundTimer <= 0f)
						{
							this.SendChatMessage(TextManager.GetWithVariable("CrewDeadNoRespawns", "[time]", "120", FormatCapitals.No).Value, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
						}
						this.EndRoundDelay = 120f;
						this.EndRoundTimer += deltaTime;
					}
					else
					{
						if (isCrewDown)
						{
							GameSession gameSession2 = GameMain.GameSession;
							if (((gameSession2 != null) ? gameSession2.GameMode : null) is CampaignMode)
							{
								this.EndRoundDelay = (isSomeoneIncapacitatedNotDead ? 120f : 2f);
								this.EndRoundTimer += deltaTime;
								goto IL_5A5;
							}
						}
						this.EndRoundTimer = 0f;
					}
				}
				IL_5A5:
				if (this.EndRoundTimer >= this.EndRoundDelay)
				{
					if (permadeathMode && isCrewDown)
					{
						GameServer.Log("Ending round (entire crew dead or down and did not acquire new characters in time)", ServerLog.MessageType.ServerMessage);
					}
					else if (base.ServerSettings.AutoRestart && isCrewDown)
					{
						GameServer.Log("Ending round (entire crew down)", ServerLog.MessageType.ServerMessage);
					}
					else if (subAtLevelEnd)
					{
						GameServer.Log("Ending round (submarine reached the end of the level)", ServerLog.MessageType.ServerMessage);
					}
					else if (base.RespawnManager == null)
					{
						GameServer.Log("Ending round (no players left standing and respawning is not enabled during this round)", ServerLog.MessageType.ServerMessage);
					}
					else
					{
						GameServer.Log("Ending round (no players left standing)", ServerLog.MessageType.ServerMessage);
					}
					this.EndGame(CampaignMode.TransitionType.None, false, null);
					return;
				}
			}
			else if (this.initiatedStartGame)
			{
				if (this.startGameCoroutine != null && !CoroutineManager.IsCoroutineRunning(this.startGameCoroutine))
				{
					if (base.ServerSettings.AutoRestart)
					{
						base.ServerSettings.AutoRestartTimer = Math.Max(base.ServerSettings.AutoRestartInterval, 5f);
					}
					if (this.startGameCoroutine.Exception != null && this.OwnerConnection != null)
					{
						string message = this.startGameCoroutine.Exception.Message;
						string str = "\n";
						string stackTrace = this.startGameCoroutine.Exception.StackTrace;
						this.SendConsoleMessage(message + str + (((stackTrace != null) ? stackTrace.CleanupStackTrace() : null) ?? "null"), this.connectedClients.Find((Client c) => c.Connection == this.OwnerConnection), new Color?(Color.Red));
					}
					this.EndGame(CampaignMode.TransitionType.None, false, null);
					NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
					ushort lastUpdateID = netLobbyScreen.LastUpdateID;
					netLobbyScreen.LastUpdateID = lastUpdateID + 1;
					this.startGameCoroutine = null;
					this.initiatedStartGame = false;
				}
			}
			else if (Screen.Selected == GameMain.NetLobbyScreen && !base.GameStarted && !this.initiatedStartGame)
			{
				if (base.ServerSettings.AutoRestart)
				{
					bool shouldAutoRestart = this.connectedClients.Any((Client c) => c.Connection != this.OwnerConnection && (!c.SpectateOnly || !base.ServerSettings.AllowSpectating));
					if (shouldAutoRestart != this.autoRestartTimerRunning)
					{
						this.autoRestartTimerRunning = shouldAutoRestart;
						NetLobbyScreen netLobbyScreen2 = GameMain.NetLobbyScreen;
						ushort lastUpdateID = netLobbyScreen2.LastUpdateID;
						netLobbyScreen2.LastUpdateID = lastUpdateID + 1;
					}
					if (this.autoRestartTimerRunning)
					{
						base.ServerSettings.AutoRestartTimer -= deltaTime;
					}
				}
				bool readyToStartAutomatically = false;
				if (base.ServerSettings.AutoRestart && this.autoRestartTimerRunning && base.ServerSettings.AutoRestartTimer < 0f)
				{
					readyToStartAutomatically = true;
				}
				else if (base.ServerSettings.StartWhenClientsReady)
				{
					IEnumerable<Client> startVoteEligibleClients = from c in this.connectedClients
					where this.Voting.CanVoteToStartRound(c)
					select c;
					int clientsReady = startVoteEligibleClients.Count((Client c) => c.GetVote<bool>(VoteType.StartRound));
					if ((float)clientsReady / (float)startVoteEligibleClients.Count<Client>() >= base.ServerSettings.StartWhenClientsReadyRatio)
					{
						readyToStartAutomatically = true;
					}
				}
				if (readyToStartAutomatically && !this.isRoundStartWarningActive)
				{
					if (!this.wasReadyToStartAutomatically)
					{
						NetLobbyScreen netLobbyScreen3 = GameMain.NetLobbyScreen;
						ushort lastUpdateID = netLobbyScreen3.LastUpdateID;
						netLobbyScreen3.LastUpdateID = lastUpdateID + 1;
					}
					this.TryStartGame();
				}
				this.wasReadyToStartAutomatically = readyToStartAutomatically;
			}
			List<Client> obj = this.clientsAttemptingToReconnectSoon;
			lock (obj)
			{
				foreach (Client client in this.clientsAttemptingToReconnectSoon)
				{
					client.DeleteDisconnectedTimer -= deltaTime;
				}
				this.clientsAttemptingToReconnectSoon.RemoveAll((Client c) => c.DeleteDisconnectedTimer < 0f);
			}
			foreach (Client c2 in this.connectedClients)
			{
				c2.ChatSpamTimer = Math.Max(0f, c2.ChatSpamTimer - deltaTime);
				c2.ChatSpamSpeed = Math.Max(0f, c2.ChatSpamSpeed - deltaTime);
				if (base.GameStarted && c2.Character != null && !c2.Character.IsDead && !c2.Character.IsIncapacitated && (!c2.AFK || !base.ServerSettings.AllowAFK) && c2.Connection != this.OwnerConnection && c2.Permissions != ClientPermissions.All)
				{
					c2.KickAFKTimer += deltaTime;
				}
			}
			if (GameServer.pvpAutoBalanceCountdownRemaining > 0f)
			{
				if (base.GameStarted || this.initiatedStartGame || Screen.Selected != GameMain.NetLobbyScreen || base.ServerSettings.PvpTeamSelectionMode == PvpTeamSelectionMode.PlayerPreference || base.ServerSettings.PvpAutoBalanceThreshold == 0)
				{
					this.StopAutoBalanceCountdown();
				}
				else
				{
					float prevTimeRemaining = GameServer.pvpAutoBalanceCountdownRemaining;
					GameServer.pvpAutoBalanceCountdownRemaining -= deltaTime;
					if (GameServer.pvpAutoBalanceCountdownRemaining <= 0f)
					{
						GameServer.pvpAutoBalanceCountdownRemaining = -1f;
						this.RefreshPvpTeamAssignments(false, true);
					}
					else
					{
						int currentTimeRemainingInteger = (int)Math.Ceiling((double)GameServer.pvpAutoBalanceCountdownRemaining);
						if (Math.Ceiling((double)prevTimeRemaining) > (double)currentTimeRemainingInteger && currentTimeRemainingInteger % 5 == 0)
						{
							this.SendChatMessage(TextManager.GetWithVariable("AutoBalance.CountdownRemaining", "[number]", currentTimeRemainingInteger.ToString(), FormatCapitals.No).Value, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
						}
					}
				}
			}
			if (this.connectedClients.Any((Client c) => c.KickAFKTimer >= base.ServerSettings.KickAFKTime))
			{
				IEnumerable<Client> kickAFK = this.connectedClients.FindAll((Client c) => c.KickAFKTimer >= base.ServerSettings.KickAFKTime && (this.OwnerConnection == null || c.Connection != this.OwnerConnection));
				foreach (Client c3 in kickAFK)
				{
					this.KickClient(c3, "DisconnectMessage.AFK", false);
				}
			}
			this.serverPeer.Update(deltaTime);
			if (!this.started)
			{
				return;
			}
			if (this.updateTimer < DateTime.Now)
			{
				if (this.ConnectedClients.Count > 0)
				{
					foreach (Client c4 in this.ConnectedClients)
					{
						try
						{
							this.ClientWrite(c4);
						}
						catch (Exception e)
						{
							DebugConsole.ThrowError("Failed to write a network message for the client \"" + c4.Name + "\"!", e, null, false, false);
							string errorMsg = string.Concat(new string[]
							{
								"Failed to write a network message for a client! (MidRoundSyncing: ",
								c4.NeedsMidRoundSync.ToString(),
								")\n",
								e.Message,
								"\n",
								e.StackTrace.CleanupStackTrace()
							});
							if (e.InnerException != null)
							{
								errorMsg = string.Concat(new string[]
								{
									errorMsg,
									"\nInner exception: ",
									e.InnerException.Message,
									"\n",
									e.InnerException.StackTrace.CleanupStackTrace()
								});
							}
							GameAnalyticsManager.AddErrorEventOnce("GameServer.Update:ClientWriteFailed" + e.StackTrace.CleanupStackTrace(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
						}
					}
					foreach (Character character2 in Character.CharacterList)
					{
						if (character2.healthUpdateTimer <= 0f)
						{
							if (!character2.HealthUpdatePending)
							{
								character2.healthUpdateTimer = character2.HealthUpdateInterval;
							}
							character2.HealthUpdatePending = true;
						}
						else
						{
							character2.healthUpdateTimer -= (float)base.UpdateInterval.TotalSeconds;
						}
						character2.HealthUpdateInterval += (float)base.UpdateInterval.TotalSeconds;
					}
				}
				this.updateTimer = DateTime.Now + base.UpdateInterval;
			}
			if (DateTime.Now > this.refreshMasterTimer || base.ServerSettings.ServerDetailsChanged)
			{
				if (this.registeredToSteamMaster)
				{
					bool refreshSuccessful = SteamManager.RefreshServerDetails(this);
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						GameServer.Log(refreshSuccessful ? "Refreshed server info on the Steam server list." : "Refreshing server info on the Steam server list failed.", ServerLog.MessageType.ServerMessage);
					}
				}
				Option.UnspecifiedNone none = Option.None;
				EosSessionManager.UpdateOwnedSession(none, base.ServerSettings);
				base.ServerSettings.ServerDetailsChanged = false;
				this.refreshMasterTimer = DateTime.Now + this.refreshMasterInterval;
			}
		}

		// Token: 0x060033FC RID: 13308 RVA: 0x00160044 File Offset: 0x0015E244
		private void UpdatePing()
		{
			if (Timing.TotalTime > this.lastPingTime + 1.0)
			{
				if (this.lastPingData == null)
				{
					this.lastPingData = new byte[64];
				}
				for (int i = 0; i < this.lastPingData.Length; i++)
				{
					this.lastPingData[i] = (byte)Rand.Range(33, 126, Rand.RandSync.Unsynced);
				}
				this.lastPingTime = Timing.TotalTime;
				this.ConnectedClients.ForEach(delegate(Client c)
				{
					IWriteMessage pingReq = new WriteOnlyMessage();
					pingReq.WriteByte(12);
					pingReq.WriteByte((byte)this.lastPingData.Length);
					pingReq.WriteBytes(this.lastPingData, 0, this.lastPingData.Length);
					this.serverPeer.Send(pingReq, c.Connection, DeliveryMethod.Unreliable, true);
					IWriteMessage pingInf = new WriteOnlyMessage();
					pingInf.WriteByte(13);
					pingInf.WriteByte((byte)this.ConnectedClients.Count);
					this.ConnectedClients.ForEach(delegate(Client c2)
					{
						pingInf.WriteByte(c2.SessionId);
						pingInf.WriteUInt16(c2.Ping);
					});
					this.serverPeer.Send(pingInf, c.Connection, DeliveryMethod.Unreliable, true);
				});
			}
		}

		// Token: 0x060033FD RID: 13309 RVA: 0x001600C8 File Offset: 0x0015E2C8
		private void ReadDataMessage(NetworkConnection sender, IReadMessage inc)
		{
			Client connectedClient = this.connectedClients.Find((Client c) => c.Connection == sender);
			using (this.dosProtection.Start(connectedClient))
			{
				switch (inc.ReadByte())
				{
				case 0:
					this.ClientReadLobby(inc);
					return;
				case 1:
					if (!base.GameStarted)
					{
						return;
					}
					this.ClientReadIngame(inc);
					return;
				case 2:
					base.ServerSettings.ServerRead(inc, connectedClient);
					return;
				case 3:
					base.ServerSettings.ReadPerks(inc, connectedClient);
					return;
				case 4:
				{
					bool isNew = inc.ReadBoolean();
					inc.ReadPadBits();
					if (isNew)
					{
						string saveName = inc.ReadString();
						string seed = inc.ReadString();
						string subName = inc.ReadString();
						string subHash = inc.ReadString();
						CampaignSettings settings = INetSerializableStruct.Read<CampaignSettings>(inc);
						SubmarineInfo matchingSub = base.ServerSettings.AllowSubVoting ? Voting.HighestVoted<SubmarineInfo>(VoteType.Sub, this.connectedClients) : SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == subName && s.MD5Hash.StringRepresentation == subHash);
						if (base.GameStarted)
						{
							this.SendDirectChatMessage(TextManager.Get("CampaignStartFailedRoundRunning").Value, connectedClient, ChatMessageType.MessageBox);
							return;
						}
						if (matchingSub == null)
						{
							this.SendDirectChatMessage(TextManager.GetWithVariable("CampaignStartFailedSubNotFound", "[subname]", subName, FormatCapitals.No).Value, connectedClient, ChatMessageType.MessageBox);
							return;
						}
						string localSavePath = SaveUtil.CreateSavePath(SaveUtil.SaveType.Multiplayer, saveName);
						if (!CampaignMode.AllowedToManageCampaign(connectedClient, ClientPermissions.ManageRound))
						{
							goto IL_564;
						}
						using (this.dosProtection.Pause(connectedClient))
						{
							base.ServerSettings.CampaignSettings = settings;
							base.ServerSettings.SaveSettings();
							MultiPlayerCampaign.StartNewCampaign(localSavePath, matchingSub.FilePath, seed, settings);
							return;
						}
					}
					string savePath = inc.ReadString();
					bool isBackup = inc.ReadBoolean();
					inc.ReadPadBits();
					uint backupIndex = isBackup ? inc.ReadUInt32() : 0U;
					if (base.GameStarted)
					{
						this.SendDirectChatMessage(TextManager.Get("CampaignStartFailedRoundRunning").Value, connectedClient, ChatMessageType.MessageBox);
						return;
					}
					if (!CampaignMode.AllowedToManageCampaign(connectedClient, ClientPermissions.ManageRound))
					{
						goto IL_564;
					}
					using (this.dosProtection.Pause(connectedClient))
					{
						CampaignDataPath dataPath;
						if (isBackup)
						{
							string backupPath = SaveUtil.GetBackupPath(savePath, backupIndex);
							dataPath = new CampaignDataPath(backupPath, savePath);
						}
						else
						{
							dataPath = CampaignDataPath.CreateRegular(savePath);
						}
						MultiPlayerCampaign.LoadCampaign(dataPath, connectedClient);
						return;
					}
					break;
				}
				case 5:
					if (base.ServerSettings.AllowFileTransfers)
					{
						this.FileSender.ReadFileRequest(inc, connectedClient);
						return;
					}
					goto IL_564;
				case 6:
					break;
				case 7:
				{
					byte responseLen = inc.ReadByte();
					if ((int)responseLen != this.lastPingData.Length)
					{
						return;
					}
					for (int i = 0; i < (int)responseLen; i++)
					{
						byte b = inc.ReadByte();
						if (b != this.lastPingData[i])
						{
							return;
						}
					}
					connectedClient.Ping = (ushort)((Timing.TotalTime - this.lastPingTime) * 1000.0);
					return;
				}
				case 8:
					if (this.isRoundStartWarningActive)
					{
						foreach (Client c2 in this.connectedClients)
						{
							IWriteMessage msg = new WriteOnlyMessage().WithHeader(ServerPacketHeader.CANCEL_STARTGAME);
							this.serverPeer.Send(msg, c2.Connection, DeliveryMethod.Reliable, true);
						}
						this.AbortStartGameIfWarningActive();
						return;
					}
					goto IL_564;
				case 9:
					if (connectedClient == null)
					{
						goto IL_564;
					}
					connectedClient.ReadyToStart = inc.ReadBoolean();
					connectedClient.AFK = inc.ReadBoolean();
					this.UpdateCharacterInfo(inc, connectedClient);
					if (base.GameStarted)
					{
						this.SendStartMessage(this.roundStartSeed, GameMain.GameSession.Level.Seed, GameMain.GameSession, connectedClient, true);
						return;
					}
					goto IL_564;
				case 10:
					this.ClientReadServerCommand(inc);
					return;
				case 11:
					connectedClient.InGame = false;
					connectedClient.ResetSync();
					return;
				case 12:
				{
					GameSession gameSession = GameMain.GameSession;
					if (gameSession == null)
					{
						return;
					}
					gameSession.EventManager.ServerRead(inc, connectedClient);
					return;
				}
				case 13:
					if (connectedClient == null)
					{
						DebugConsole.AddWarning("Received a REQUEST_STARTGAMEFINALIZE message. Client not connected, ignoring the message.", null);
						return;
					}
					if (!base.GameStarted)
					{
						DebugConsole.AddWarning("Received a REQUEST_STARTGAMEFINALIZE message. Game not started, ignoring the message.", null);
						return;
					}
					this.SendRoundStartFinalize(connectedClient);
					return;
				case 14:
					this.UpdateCharacterInfo(inc, connectedClient);
					return;
				case 15:
					this.HandleClientError(inc, connectedClient);
					goto IL_564;
				case 16:
					this.ReadCrewMessage(inc, connectedClient);
					return;
				case 17:
					this.ReadMedicalMessage(inc, connectedClient);
					return;
				case 18:
					this.ReadMoneyMessage(inc, connectedClient);
					return;
				case 19:
					this.ReadRewardDistributionMessage(inc, connectedClient);
					return;
				case 20:
					this.ResetRewardDistribution(connectedClient);
					return;
				case 21:
					GameServer.ReadCircuitBoxMessage(inc, connectedClient);
					return;
				case 22:
					ReadyCheck.ServerRead(inc, connectedClient);
					return;
				case 23:
					this.ReadReadyToSpawnMessage(inc, connectedClient);
					return;
				case 24:
					this.ReadTakeOverBotMessage(inc, connectedClient);
					return;
				case 25:
				{
					GameSession gameSession2 = GameMain.GameSession;
					if (gameSession2 == null)
					{
						return;
					}
					CrewManager crewManager = gameSession2.CrewManager;
					if (crewManager == null)
					{
						return;
					}
					crewManager.ReadToggleReserveBenchMessage(inc, connectedClient);
					return;
				}
				case 26:
					this.SendBackupIndices(inc, connectedClient);
					return;
				default:
					return;
				}
				if (base.ServerSettings.VoiceChatEnabled && !connectedClient.Muted)
				{
					byte id = inc.ReadByte();
					if (connectedClient.SessionId == id)
					{
						VoipServer.Read(inc, connectedClient);
					}
				}
				IL_564:;
			}
		}

		// Token: 0x060033FE RID: 13310 RVA: 0x001606A8 File Offset: 0x0015E8A8
		private void SendBackupIndices(IReadMessage inc, Client connectedClient)
		{
			string savePath = inc.ReadString();
			ImmutableArray<SaveUtil.BackupIndexData> indexData = SaveUtil.GetIndexData(savePath);
			IWriteMessage msg = new WriteOnlyMessage().WithHeader(ServerPacketHeader.SEND_BACKUP_INDICES);
			msg.WriteString(savePath);
			msg.WriteNetSerializableStruct(indexData.ToNetCollection<SaveUtil.BackupIndexData>());
			ServerPeer serverPeer = this.serverPeer;
			if (serverPeer == null)
			{
				return;
			}
			serverPeer.Send(msg, connectedClient.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x060033FF RID: 13311 RVA: 0x00160704 File Offset: 0x0015E904
		private void HandleClientError(IReadMessage inc, Client c)
		{
			string errorStr = "Unhandled error report";
			string errorStrNoName = errorStr;
			bool malformedData = false;
			try
			{
				ClientNetError error = (ClientNetError)inc.ReadByte();
				if (error != ClientNetError.MISSING_EVENT)
				{
					if (error == ClientNetError.MISSING_ENTITY)
					{
						ushort eventID = inc.ReadUInt16();
						ushort entityID = inc.ReadUInt16();
						int subCount = (int)inc.ReadByte();
						List<string> subNames = new List<string>();
						for (int i = 0; i < Math.Min(subCount, 5); i++)
						{
							string subName = inc.ReadString();
							if (subName == null || subName.Length > 16)
							{
								malformedData = true;
							}
							else
							{
								subNames.Add(subName);
							}
						}
						Entity entity = Entity.FindEntityByID(entityID);
						if (entity == null)
						{
							errorStrNoName = (errorStr = string.Concat(new string[]
							{
								"Received an update for an entity that doesn't exist (event id ",
								eventID.ToString(),
								", entity id ",
								entityID.ToString(),
								")."
							}));
						}
						else
						{
							Character character = entity as Character;
							if (character != null)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 3);
								defaultInterpolatedStringHandler.AppendLiteral("Missing character ");
								defaultInterpolatedStringHandler.AppendFormatted(character.Name);
								defaultInterpolatedStringHandler.AppendLiteral(" (event id ");
								defaultInterpolatedStringHandler.AppendFormatted<ushort>(eventID);
								defaultInterpolatedStringHandler.AppendLiteral(", entity id ");
								defaultInterpolatedStringHandler.AppendFormatted<ushort>(entityID);
								defaultInterpolatedStringHandler.AppendLiteral(").");
								errorStr = defaultInterpolatedStringHandler.ToStringAndClear();
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(44, 3);
								defaultInterpolatedStringHandler2.AppendLiteral("Missing character ");
								defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(character.SpeciesName);
								defaultInterpolatedStringHandler2.AppendLiteral("  (event id ");
								defaultInterpolatedStringHandler2.AppendFormatted<ushort>(eventID);
								defaultInterpolatedStringHandler2.AppendLiteral(", entity id ");
								defaultInterpolatedStringHandler2.AppendFormatted<ushort>(entityID);
								defaultInterpolatedStringHandler2.AppendLiteral(").");
								errorStrNoName = defaultInterpolatedStringHandler2.ToStringAndClear();
							}
							else
							{
								Item item = entity as Item;
								if (item != null)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(48, 5);
									defaultInterpolatedStringHandler3.AppendLiteral("Missing item ");
									defaultInterpolatedStringHandler3.AppendFormatted(item.Name);
									defaultInterpolatedStringHandler3.AppendLiteral(" (");
									defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(item.Prefab.Identifier);
									defaultInterpolatedStringHandler3.AppendLiteral("), sub: ");
									Submarine submarine = item.Submarine;
									string text;
									if (submarine == null)
									{
										text = null;
									}
									else
									{
										SubmarineInfo info = submarine.Info;
										text = ((info != null) ? info.Name : null);
									}
									defaultInterpolatedStringHandler3.AppendFormatted(text ?? "none");
									defaultInterpolatedStringHandler3.AppendLiteral(" (event id ");
									defaultInterpolatedStringHandler3.AppendFormatted<ushort>(eventID);
									defaultInterpolatedStringHandler3.AppendLiteral(", entity id ");
									defaultInterpolatedStringHandler3.AppendFormatted<ushort>(entityID);
									defaultInterpolatedStringHandler3.AppendLiteral(").");
									errorStrNoName = (errorStr = defaultInterpolatedStringHandler3.ToStringAndClear());
								}
								else
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(47, 4);
									defaultInterpolatedStringHandler4.AppendLiteral("Missing entity ");
									defaultInterpolatedStringHandler4.AppendFormatted<Entity>(entity);
									defaultInterpolatedStringHandler4.AppendLiteral(", sub: ");
									Submarine submarine2 = entity.Submarine;
									string text2;
									if (submarine2 == null)
									{
										text2 = null;
									}
									else
									{
										SubmarineInfo info2 = submarine2.Info;
										text2 = ((info2 != null) ? info2.Name : null);
									}
									defaultInterpolatedStringHandler4.AppendFormatted(text2 ?? "none");
									defaultInterpolatedStringHandler4.AppendLiteral(" (event id ");
									defaultInterpolatedStringHandler4.AppendFormatted<ushort>(eventID);
									defaultInterpolatedStringHandler4.AppendLiteral(", entity id ");
									defaultInterpolatedStringHandler4.AppendFormatted<ushort>(entityID);
									defaultInterpolatedStringHandler4.AppendLiteral(").");
									errorStrNoName = (errorStr = defaultInterpolatedStringHandler4.ToStringAndClear());
								}
							}
						}
						if (base.GameStarted)
						{
							IEnumerable<string> serverSubNames = Submarine.Loaded.Select(delegate(Submarine s)
							{
								if (s.Info.Name.Length <= 16)
								{
									return s.Info.Name;
								}
								return s.Info.Name.Substring(0, 16);
							});
							if (subCount != Submarine.Loaded.Count || !subNames.SequenceEqual(serverSubNames))
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(52, 2);
								defaultInterpolatedStringHandler5.AppendLiteral(" Loaded submarines don't match (client: ");
								defaultInterpolatedStringHandler5.AppendFormatted(string.Join(", ", subNames));
								defaultInterpolatedStringHandler5.AppendLiteral(", server: ");
								defaultInterpolatedStringHandler5.AppendFormatted(string.Join(", ", serverSubNames));
								defaultInterpolatedStringHandler5.AppendLiteral(").");
								string subErrorStr = defaultInterpolatedStringHandler5.ToStringAndClear();
								errorStr += subErrorStr;
								errorStrNoName += subErrorStr;
							}
						}
					}
				}
				else
				{
					ushort expectedID = inc.ReadUInt16();
					ushort receivedID = inc.ReadUInt16();
					errorStrNoName = (errorStr = "Expecting event id " + expectedID.ToString() + ", received " + receivedID.ToString());
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to read error data from the client " + NetworkMember.ClientLogName(c, null) + ".", e, null, false, false);
				malformedData = true;
			}
			if (malformedData)
			{
				this.KickClient(c, "Received malformed error data.", false);
				return;
			}
			GameServer.Log(NetworkMember.ClientLogName(c, null) + " has reported an error: " + errorStr, ServerLog.MessageType.Error);
			GameAnalyticsManager.AddErrorEventOnce("GameServer.HandleClientError:" + errorStrNoName, GameAnalyticsManager.ErrorSeverity.Error, errorStr);
			try
			{
				this.WriteEventErrorData(c, errorStr);
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Failed to write event error data", e2, null, false, false);
			}
			if (c.Connection == this.OwnerConnection)
			{
				this.SendDirectChatMessage(errorStr, c, ChatMessageType.MessageBox);
				this.EndGame(CampaignMode.TransitionType.None, false, null);
				return;
			}
			this.KickClient(c, errorStr, false);
		}

		// Token: 0x06003400 RID: 13312 RVA: 0x00160BEC File Offset: 0x0015EDEC
		private void WriteEventErrorData(Client client, string errorStr)
		{
			if (!Directory.Exists("ServerLogs"))
			{
				Directory.CreateDirectory("ServerLogs", false);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 2);
			defaultInterpolatedStringHandler.AppendLiteral("event_error_log_server_");
			defaultInterpolatedStringHandler.AppendFormatted(client.Name);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted(DateTime.UtcNow.ToShortTimeString());
			defaultInterpolatedStringHandler.AppendLiteral(".log");
			string filePath = defaultInterpolatedStringHandler.ToStringAndClear();
			filePath = Path.Combine(new string[]
			{
				"ServerLogs",
				ToolBox.RemoveInvalidFileNameChars(filePath)
			});
			if (File.Exists(filePath))
			{
				return;
			}
			List<string> errorLines = new List<string>
			{
				errorStr,
				""
			};
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) != null)
			{
				errorLines.Add("Game mode: " + GameMain.GameSession.GameMode.Name.Value);
				GameSession gameSession2 = GameMain.GameSession;
				MultiPlayerCampaign campaign = ((gameSession2 != null) ? gameSession2.GameMode : null) as MultiPlayerCampaign;
				if (campaign != null)
				{
					errorLines.Add("Campaign ID: " + campaign.CampaignID.ToString());
					errorLines.Add("Campaign save ID: " + campaign.LastSaveID.ToString());
				}
				foreach (Mission mission in GameMain.GameSession.Missions)
				{
					errorLines.Add("Mission: " + mission.Prefab.Identifier.ToString());
				}
			}
			GameSession gameSession3 = GameMain.GameSession;
			if (((gameSession3 != null) ? gameSession3.Submarine : null) != null)
			{
				errorLines.Add("Submarine: " + GameMain.GameSession.Submarine.Info.Name);
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			RespawnManager respawnManager = (networkMember != null) ? networkMember.RespawnManager : null;
			if (respawnManager != null)
			{
				errorLines.Add("Respawn shuttles: " + string.Join(", ", from s in respawnManager.RespawnShuttles
				select s.Info.Name));
			}
			if (Level.Loaded != null)
			{
				errorLines.Add("Level: " + Level.Loaded.Seed + ", " + string.Join("; ", from cv in Level.Loaded.EqualityCheckValues
				select cv.Key.ToString() + "=" + cv.Value.ToString("X")));
				errorLines.Add("Entity count before generating level: " + Level.Loaded.EntityCountBeforeGenerate.ToString());
				errorLines.Add("Entities:");
				foreach (Entity e3 in from e in Level.Loaded.EntitiesBeforeGenerate
				orderby e.CreationIndex
				select e)
				{
					errorLines.Add(e3.ErrorLine);
				}
				errorLines.Add("Entity count after generating level: " + Level.Loaded.EntityCountAfterGenerate.ToString());
			}
			errorLines.Add("Entity IDs:");
			Entity[] sortedEntities = (from e in Entity.GetEntities()
			orderby e.CreationIndex
			select e).ToArray<Entity>();
			foreach (Entity e2 in sortedEntities)
			{
				errorLines.Add(e2.ErrorLine);
			}
			errorLines.Add("");
			errorLines.Add("EntitySpawner events:");
			foreach (ServerEntityEvent entityEvent in this.entityEventManager.UniqueEvents)
			{
				if (entityEvent.Entity is EntitySpawner)
				{
					EntitySpawner.SpawnOrRemove spawnData = entityEvent.Data as EntitySpawner.SpawnOrRemove;
					errorLines.Add(string.Concat(new string[]
					{
						entityEvent.ID.ToString(),
						": ",
						(spawnData is EntitySpawner.RemoveEntity) ? "Remove " : "Create ",
						spawnData.Entity.ToString(),
						" (",
						spawnData.ID.ToString(),
						", ",
						spawnData.Entity.ID.ToString(),
						")"
					}));
				}
			}
			errorLines.Add("");
			errorLines.Add("Last debug messages:");
			int i = DebugConsole.Messages.Count - 1;
			while (i > 0 && i > DebugConsole.Messages.Count - 15)
			{
				errorLines.Add("   " + DebugConsole.Messages[i].Time + " - " + DebugConsole.Messages[i].Text);
				i--;
			}
			File.WriteAllLines(filePath, errorLines, null, true);
		}

		// Token: 0x06003401 RID: 13313 RVA: 0x0016114C File Offset: 0x0015F34C
		public override void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData = null)
		{
			IServerSerializable serverSerializable = entity as IServerSerializable;
			if (serverSerializable == null)
			{
				throw new InvalidCastException("Entity is not IServerSerializable");
			}
			this.entityEventManager.CreateEvent(serverSerializable, extraData);
		}

		// Token: 0x06003402 RID: 13314 RVA: 0x0016117C File Offset: 0x0015F37C
		private byte GetNewClientSessionId()
		{
			byte userId = 1;
			while (this.connectedClients.Any((Client c) => c.SessionId == userId))
			{
				byte userId2 = userId;
				userId = userId2 + 1;
			}
			return userId;
		}

		// Token: 0x06003403 RID: 13315 RVA: 0x001611C8 File Offset: 0x0015F3C8
		private void ClientReadLobby(IReadMessage inc)
		{
			Client c = this.ConnectedClients.Find((Client x) => x.Connection == inc2.Sender);
			if (c == null)
			{
				return;
			}
			SegmentTableReader<ClientNetSegment>.Read(inc2, delegate(ClientNetSegment segment, [Nullable(1)] IReadMessage inc)
			{
				switch (segment)
				{
				case ClientNetSegment.SyncIds:
				{
					c.LastRecvLobbyUpdate = NetIdUtils.Clamp(inc.ReadUInt16(), c.LastRecvLobbyUpdate, GameMain.NetLobbyScreen.LastUpdateID);
					if (c.HasPermission(ClientPermissions.ManageSettings) && NetIdUtils.IdMoreRecentOrMatches(c.LastRecvLobbyUpdate, c.LastSentServerSettingsUpdate))
					{
						c.LastRecvServerSettingsUpdate = c.LastSentServerSettingsUpdate;
					}
					c.LastRecvChatMsgID = NetIdUtils.Clamp(inc.ReadUInt16(), c.LastRecvChatMsgID, c.LastChatMsgQueueID);
					c.LastRecvClientListUpdate = NetIdUtils.Clamp(inc.ReadUInt16(), c.LastRecvClientListUpdate, this.LastClientListUpdateID);
					c.AFK = inc.ReadBoolean();
					this.ReadClientNameChange(c, inc);
					c.LastRecvCampaignSave = inc.ReadUInt16();
					if (c.LastRecvCampaignSave <= 0)
					{
						goto IL_270;
					}
					byte campaignID = inc.ReadByte();
					foreach (object obj in Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)))
					{
						MultiPlayerCampaign.NetFlags netFlag = (MultiPlayerCampaign.NetFlags)obj;
						c.LastRecvCampaignUpdate[netFlag] = inc.ReadUInt16();
					}
					bool characterDiscarded = inc.ReadBoolean();
					GameSession gameSession = GameMain.GameSession;
					MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
					if (campaign == null)
					{
						goto IL_270;
					}
					if (characterDiscarded)
					{
						campaign.DiscardClientCharacterData(c);
					}
					if (campaign.CampaignID == campaignID)
					{
						goto IL_270;
					}
					c.LastRecvCampaignSave = campaign.LastSaveID - 1;
					using (IEnumerator enumerator2 = Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)).GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							object obj2 = enumerator2.Current;
							MultiPlayerCampaign.NetFlags netFlag2 = (MultiPlayerCampaign.NetFlags)obj2;
							c.LastRecvCampaignUpdate[netFlag2] = campaign.GetLastUpdateIdForFlag(netFlag2) - 1;
						}
						goto IL_270;
					}
					break;
				}
				case ClientNetSegment.ChatMessage:
					break;
				case ClientNetSegment.Vote:
					this.Voting.ServerRead(inc, c, this.dosProtection);
					goto IL_270;
				default:
					return SegmentTableReader<ClientNetSegment>.BreakSegmentReading.Yes;
				}
				ChatMessage.ServerRead(inc, c);
				IL_270:
				if (!this.connectedClients.Contains(c))
				{
					return SegmentTableReader<ClientNetSegment>.BreakSegmentReading.Yes;
				}
				return SegmentTableReader<ClientNetSegment>.BreakSegmentReading.No;
			}, null);
		}

		// Token: 0x06003404 RID: 13316 RVA: 0x00161228 File Offset: 0x0015F428
		private void ClientReadIngame(IReadMessage inc)
		{
			Client c = this.ConnectedClients.Find((Client x) => x.Connection == inc2.Sender);
			if (c == null)
			{
				return;
			}
			bool midroundSyncingDone = inc2.ReadBoolean();
			inc2.ReadPadBits();
			if (base.GameStarted && !c.InGame)
			{
				if (!midroundSyncingDone)
				{
					this.entityEventManager.InitClientMidRoundSync(c);
				}
				MissionAction.NotifyMissionsUnlockedThisRound(c);
				UnlockPathAction.NotifyPathsUnlockedThisRound(c);
				if (GameMain.GameSession.GameMode is PvPMode)
				{
					if (c.TeamID == CharacterTeamType.None)
					{
						this.AssignClientToPvpTeamMidgame(c);
					}
				}
				else
				{
					MultiPlayerCampaign mpCampaign = GameMain.GameSession.Campaign as MultiPlayerCampaign;
					if (mpCampaign != null)
					{
						mpCampaign.SendCrewState(default(ValueTuple<ushort, string>), null, true);
					}
					c.TeamID = CharacterTeamType.Team1;
				}
				c.InGame = true;
				c.AFK = false;
			}
			SegmentTableReader<ClientNetSegment>.Read(inc2, delegate(ClientNetSegment segment, [Nullable(1)] IReadMessage inc)
			{
				switch (segment)
				{
				case ClientNetSegment.SyncIds:
				{
					ushort lastRecvChatMsgID = inc.ReadUInt16();
					ushort lastRecvEntityEventID = inc.ReadUInt16();
					ushort lastRecvClientListUpdate = inc.ReadUInt16();
					ushort lastEntityEventID = (this.entityEventManager.Events.Count == 0) ? 0 : this.entityEventManager.Events.Last<ServerEntityEvent>().ID;
					c.LastRecvCampaignSave = inc.ReadUInt16();
					if (c.LastRecvCampaignSave > 0)
					{
						byte campaignID = inc.ReadByte();
						foreach (object obj in Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)))
						{
							MultiPlayerCampaign.NetFlags netFlag = (MultiPlayerCampaign.NetFlags)obj;
							c.LastRecvCampaignUpdate[netFlag] = inc.ReadUInt16();
						}
						bool characterDiscarded = inc.ReadBoolean();
						GameSession gameSession = GameMain.GameSession;
						MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
						if (campaign != null)
						{
							if (characterDiscarded)
							{
								campaign.DiscardClientCharacterData(c);
							}
							if (campaign.CampaignID != campaignID)
							{
								c.LastRecvCampaignSave = campaign.LastSaveID - 1;
								foreach (object obj2 in Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)))
								{
									MultiPlayerCampaign.NetFlags netFlag2 = (MultiPlayerCampaign.NetFlags)obj2;
									c.LastRecvCampaignUpdate[netFlag2] = campaign.GetLastUpdateIdForFlag(netFlag2) - 1;
								}
							}
						}
					}
					if (c.NeedsMidRoundSync)
					{
						if (lastRecvEntityEventID >= c.UnreceivedEntityEventCount - 1 || c.UnreceivedEntityEventCount == 0)
						{
							ushort prevID = lastRecvEntityEventID;
							c.NeedsMidRoundSync = false;
							lastRecvEntityEventID = c.FirstNewEventID - 1;
							c.LastRecvEntityEventID = lastRecvEntityEventID;
							DebugConsole.Log(string.Concat(new string[]
							{
								"Finished midround syncing ",
								c.Name,
								" - switching from ID ",
								prevID.ToString(),
								" to ",
								c.LastRecvEntityEventID.ToString()
							}));
							if (this.RespawnManager != null)
							{
								this.CreateEntityEvent(this.RespawnManager, null);
							}
							GameSession gameSession2 = GameMain.GameSession;
							MultiPlayerCampaign campaign2 = ((gameSession2 != null) ? gameSession2.GameMode : null) as MultiPlayerCampaign;
							if (campaign2 != null)
							{
								campaign2.Bank.ForceUpdate();
								campaign2.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.Misc);
							}
						}
						else
						{
							lastEntityEventID = c.UnreceivedEntityEventCount - 1;
						}
					}
					if (NetIdUtils.IsValidId(lastRecvChatMsgID, c.LastRecvChatMsgID, c.LastChatMsgQueueID))
					{
						c.LastRecvChatMsgID = lastRecvChatMsgID;
					}
					else if (lastRecvChatMsgID != c.LastRecvChatMsgID && GameSettings.CurrentConfig.VerboseLogging)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Invalid lastRecvChatMsgID  ",
							lastRecvChatMsgID.ToString(),
							" (previous: ",
							c.LastChatMsgQueueID.ToString(),
							", latest: ",
							c.LastChatMsgQueueID.ToString(),
							")"
						}), null, null, false, false);
					}
					if (NetIdUtils.IsValidId(lastRecvEntityEventID, c.LastRecvEntityEventID, lastEntityEventID))
					{
						if (c.NeedsMidRoundSync)
						{
							int receivedEventCount = (int)(lastRecvEntityEventID - c.LastRecvEntityEventID);
							if (receivedEventCount < 0)
							{
								receivedEventCount += 65535;
							}
							c.MidRoundSyncTimeOut += (double)((float)receivedEventCount * 0.01f);
							DebugConsole.Log("Midround sync timeout " + c.MidRoundSyncTimeOut.ToString("0.##") + "/" + Timing.TotalTime.ToString("0.##"));
						}
						c.LastRecvEntityEventID = lastRecvEntityEventID;
					}
					else if (lastRecvEntityEventID != c.LastRecvEntityEventID && GameSettings.CurrentConfig.VerboseLogging)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Invalid lastRecvEntityEventID  ",
							lastRecvEntityEventID.ToString(),
							" (previous: ",
							c.LastRecvEntityEventID.ToString(),
							", latest: ",
							lastEntityEventID.ToString(),
							")"
						}), null, null, false, false);
					}
					if (NetIdUtils.IdMoreRecent(lastRecvClientListUpdate, c.LastRecvClientListUpdate))
					{
						c.LastRecvClientListUpdate = lastRecvClientListUpdate;
					}
					break;
				}
				case ClientNetSegment.ChatMessage:
					ChatMessage.ServerRead(inc, c);
					break;
				case ClientNetSegment.Vote:
					this.Voting.ServerRead(inc, c, this.dosProtection);
					break;
				case ClientNetSegment.CharacterInput:
					if (c.Character != null)
					{
						c.Character.ServerReadInput(inc, c);
					}
					else
					{
						DebugConsole.AddWarning("Received character inputs from a client who's not controlling a character (" + c.Name + ").", null);
					}
					break;
				case ClientNetSegment.EntityState:
					this.entityEventManager.Read(inc, c);
					break;
				case ClientNetSegment.SpectatingPos:
					c.SpectatePos = new Vector2?(new Vector2(inc.ReadSingle(), inc.ReadSingle()));
					break;
				default:
					return SegmentTableReader<ClientNetSegment>.BreakSegmentReading.Yes;
				}
				if (!this.connectedClients.Contains(c))
				{
					return SegmentTableReader<ClientNetSegment>.BreakSegmentReading.Yes;
				}
				return SegmentTableReader<ClientNetSegment>.BreakSegmentReading.No;
			}, null);
		}

		// Token: 0x06003405 RID: 13317 RVA: 0x00161358 File Offset: 0x0015F558
		private void ReadCrewMessage(IReadMessage inc, Client sender)
		{
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
			if (mpCampaign != null)
			{
				mpCampaign.ServerReadCrew(inc, sender);
			}
		}

		// Token: 0x06003406 RID: 13318 RVA: 0x00161388 File Offset: 0x0015F588
		private void ReadMoneyMessage(IReadMessage inc, Client sender)
		{
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
			if (mpCampaign != null)
			{
				mpCampaign.ServerReadMoney(inc, sender);
			}
		}

		// Token: 0x06003407 RID: 13319 RVA: 0x001613B8 File Offset: 0x0015F5B8
		private void ReadRewardDistributionMessage(IReadMessage inc, Client sender)
		{
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
			if (mpCampaign != null)
			{
				mpCampaign.ServerReadRewardDistribution(inc, sender);
			}
		}

		// Token: 0x06003408 RID: 13320 RVA: 0x001613E8 File Offset: 0x0015F5E8
		private void ResetRewardDistribution(Client client)
		{
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
			if (mpCampaign != null)
			{
				mpCampaign.ResetSalaries(client);
			}
		}

		// Token: 0x06003409 RID: 13321 RVA: 0x00161418 File Offset: 0x0015F618
		private void ReadMedicalMessage(IReadMessage inc, Client sender)
		{
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
			if (mpCampaign != null)
			{
				mpCampaign.MedicalClinic.ServerRead(inc, sender);
			}
		}

		// Token: 0x0600340A RID: 13322 RVA: 0x0016144C File Offset: 0x0015F64C
		private static void ReadCircuitBoxMessage(IReadMessage inc, Client sender)
		{
			NetCircuitBoxHeader header = INetSerializableStruct.Read<NetCircuitBoxHeader>(inc);
			CircuitBoxOpcode opcode = header.Opcode;
			if (opcode == CircuitBoxOpcode.Cursor)
			{
				NetCircuitBoxCursorInfo netCircuitBoxCursorInfo = INetSerializableStruct.Read<NetCircuitBoxCursorInfo>(inc);
				INetSerializableStruct data = netCircuitBoxCursorInfo;
				CircuitBox box;
				if (header.FindTarget().TryUnwrap(out box))
				{
					box.ServerRead(data, sender);
				}
				return;
			}
			throw new ArgumentOutOfRangeException("Opcode", header.Opcode, "This data cannot be handled using direct network messages.");
		}

		// Token: 0x0600340B RID: 13323 RVA: 0x001614B8 File Offset: 0x0015F6B8
		private void ReadReadyToSpawnMessage(IReadMessage inc, Client sender)
		{
			sender.SpectateOnly = (inc.ReadBoolean() && (base.ServerSettings.AllowSpectating || sender.Connection == this.OwnerConnection));
			sender.WaitForNextRoundRespawn = new bool?(inc.ReadBoolean());
			GameSession gameSession = GameMain.GameSession;
			if (!(((gameSession != null) ? gameSession.GameMode : null) is CampaignMode))
			{
				sender.WaitForNextRoundRespawn = null;
			}
		}

		// Token: 0x0600340C RID: 13324 RVA: 0x0016152C File Offset: 0x0015F72C
		private void ReadTakeOverBotMessage(IReadMessage inc, Client sender)
		{
			ushort botId = inc.ReadUInt16();
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
			if (campaign == null)
			{
				return;
			}
			if (base.ServerSettings.IronmanModeActive)
			{
				DebugConsole.ThrowError("Client " + sender.Name + " has requested to take over a bot in Ironman mode!", null, null, false, false);
				return;
			}
			CharacterInfo hireableCharacter = campaign.CurrentLocation.GetHireableCharacters().FirstOrDefault((CharacterInfo c) => c.ID == botId);
			if (hireableCharacter != null)
			{
				if (base.ServerSettings.ReplaceCostPercentage > 0f && !CampaignMode.AllowedToManageCampaign(sender, ClientPermissions.ManageMoney) && !CampaignMode.AllowedToManageCampaign(sender, ClientPermissions.ManageHires))
				{
					this.SendConsoleMessage("Could not hire the bot " + hireableCharacter.Name + ". No permission to manage money or hires.", sender, new Color?(Color.Red));
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Client ");
					defaultInterpolatedStringHandler.AppendFormatted(sender.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" failed to hire the bot ");
					defaultInterpolatedStringHandler.AppendFormatted(hireableCharacter.Name);
					defaultInterpolatedStringHandler.AppendLiteral(". No permission to manage money or hires.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return;
				}
				if (campaign.TryHireCharacter(campaign.CurrentLocation, hireableCharacter, true, sender, true))
				{
					campaign.CurrentLocation.RemoveHireableCharacter(hireableCharacter);
					GameServer.SpawnAndTakeOverBot(campaign, hireableCharacter, sender);
					campaign.SendCrewState(default(ValueTuple<ushort, string>), null, false);
					return;
				}
				this.SendConsoleMessage("Could not hire the bot " + hireableCharacter.Name + ".", sender, new Color?(Color.Red));
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(32, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Client ");
				defaultInterpolatedStringHandler2.AppendFormatted(sender.Name);
				defaultInterpolatedStringHandler2.AppendLiteral(" failed to hire the bot ");
				defaultInterpolatedStringHandler2.AppendFormatted(hireableCharacter.Name);
				defaultInterpolatedStringHandler2.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				return;
			}
			else
			{
				CrewManager crewManager = GameMain.GameSession.CrewManager;
				CharacterInfo characterInfo;
				if (crewManager == null)
				{
					characterInfo = null;
				}
				else
				{
					IEnumerable<CharacterInfo> characterInfos = crewManager.GetCharacterInfos(true);
					characterInfo = ((characterInfos != null) ? characterInfos.FirstOrDefault((CharacterInfo i) => i.ID == botId) : null);
				}
				CharacterInfo botInfo = characterInfo;
				if (botInfo != null && botInfo.Character == null && (botInfo.IsNewHire || botInfo.IsOnReserveBench))
				{
					if (this.IsUsingRespawnShuttle())
					{
						GameServer.SpawnAndTakeOverBotInShuttle(campaign, botInfo, sender);
						return;
					}
					GameServer.SpawnAndTakeOverBot(campaign, botInfo, sender);
					return;
				}
				else
				{
					if (((botInfo != null) ? botInfo.Character : null) == null || !botInfo.Character.IsBot)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(34, 1);
						defaultInterpolatedStringHandler3.AppendLiteral("Could not find a bot with the id ");
						defaultInterpolatedStringHandler3.AppendFormatted<ushort>(botId);
						defaultInterpolatedStringHandler3.AppendLiteral(".");
						this.SendConsoleMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), sender, new Color?(Color.Red));
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(70, 2);
						defaultInterpolatedStringHandler4.AppendLiteral("Client ");
						defaultInterpolatedStringHandler4.AppendFormatted(sender.Name);
						defaultInterpolatedStringHandler4.AppendLiteral(" failed to take over a bot (Could not find a bot with the id ");
						defaultInterpolatedStringHandler4.AppendFormatted<ushort>(botId);
						defaultInterpolatedStringHandler4.AppendLiteral(").");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, null, false, false);
						return;
					}
					if (base.ServerSettings.AllowBotTakeoverOnPermadeath)
					{
						sender.TryTakeOverBot(botInfo.Character);
						return;
					}
					this.SendConsoleMessage("Failed to take over a bot (taking control of bots is disallowed).", sender, new Color?(Color.Red));
					DebugConsole.ThrowError("Client " + sender.Name + " failed to take over a bot (taking control of bots is disallowed).", null, null, false, false);
					return;
				}
			}
		}

		// Token: 0x0600340D RID: 13325 RVA: 0x00161898 File Offset: 0x0015FA98
		private static void SpawnAndTakeOverBot(CampaignMode campaign, CharacterInfo botInfo, Client client)
		{
			WayPoint mainSubSpawnpoint = WayPoint.SelectCrewSpawnPoints(botInfo.ToEnumerable<CharacterInfo>().ToList<CharacterInfo>(), Submarine.MainSub).FirstOrDefault<WayPoint>();
			List<WayPoint> outpostSpawnpoints = campaign.CrewManager.GetOutpostSpawnpoints();
			WayPoint outpostWaypoint = (outpostSpawnpoints != null) ? outpostSpawnpoints.FirstOrDefault<WayPoint>() : null;
			GameServer.TransferPreviousSalaryToBot(campaign, botInfo, client);
			WayPoint spawnWaypoint;
			if (botInfo.IsOnReserveBench)
			{
				spawnWaypoint = (mainSubSpawnpoint ?? outpostWaypoint);
			}
			else
			{
				spawnWaypoint = (outpostWaypoint ?? mainSubSpawnpoint);
			}
			if (spawnWaypoint == null)
			{
				DebugConsole.ThrowError("SpawnAndTakeOverBot: Unable to find any spawn waypoints inside the sub", null, null, false, false);
				return;
			}
			Entity.Spawner.AddCharacterToSpawnQueue(botInfo.SpeciesName, spawnWaypoint.WorldPosition, botInfo, delegate(Character newCharacter)
			{
				if (newCharacter == null)
				{
					DebugConsole.ThrowError("SpawnAndTakeOverBot: newCharacter is null somehow", null, null, false, false);
					return;
				}
				if (botInfo.IsOnReserveBench)
				{
					campaign.CrewManager.ToggleReserveBenchStatus(botInfo, client, false, false, true);
				}
				newCharacter.TeamID = CharacterTeamType.Team1;
				campaign.CrewManager.InitializeCharacter(newCharacter, mainSubSpawnpoint, spawnWaypoint);
				client.TryTakeOverBot(newCharacter);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Client \"");
				defaultInterpolatedStringHandler.AppendFormatted(client.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" took over the bot \"");
				defaultInterpolatedStringHandler.AppendFormatted(botInfo.DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
			});
		}

		// Token: 0x0600340E RID: 13326 RVA: 0x00161994 File Offset: 0x0015FB94
		private static void SpawnAndTakeOverBotInShuttle(CampaignMode campaign, CharacterInfo botInfo, Client client)
		{
			if (botInfo.IsOnReserveBench)
			{
				MultiPlayerCampaign mpCampaign = campaign as MultiPlayerCampaign;
				if (mpCampaign != null)
				{
					GameServer.TransferPreviousSalaryToBot(campaign, botInfo, client);
					mpCampaign.CrewManager.ToggleReserveBenchStatus(botInfo, client, false, false, true);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Client \"");
					defaultInterpolatedStringHandler.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\" chose to spawn as the bot \"");
					defaultInterpolatedStringHandler.AppendFormatted(botInfo.DisplayName);
					defaultInterpolatedStringHandler.AppendLiteral("\" in the next respawn shuttle.");
					GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
					mpCampaign.DiscardClientCharacterData(client);
					client.CharacterInfo = botInfo;
					client.CharacterInfo.RenamingEnabled = true;
					client.CharacterInfo.IsNewHire = false;
					client.SpectateOnly = false;
					client.WaitForNextRoundRespawn = new bool?(false);
					CharacterCampaignData characterData = mpCampaign.SetClientCharacterData(client);
					if (characterData != null)
					{
						characterData.HasSpawned = true;
						characterData.ChosenNewBotViaShuttle = true;
					}
				}
			}
		}

		// Token: 0x0600340F RID: 13327 RVA: 0x00161A78 File Offset: 0x0015FC78
		private static void TransferPreviousSalaryToBot(CampaignMode campaign, CharacterInfo botInfo, Client client)
		{
			int? num;
			if (client == null)
			{
				num = null;
			}
			else
			{
				Character character = client.Character;
				num = ((character != null) ? new int?(character.Wallet.RewardDistribution) : null);
			}
			botInfo.LastRewardDistribution = Option<int>.Some(num ?? campaign.Bank.RewardDistribution);
		}

		// Token: 0x06003410 RID: 13328 RVA: 0x00161AE0 File Offset: 0x0015FCE0
		private void ClientReadServerCommand(IReadMessage inc)
		{
			GameServer.<>c__DisplayClass107_0 CS$<>8__locals1 = new GameServer.<>c__DisplayClass107_0();
			CS$<>8__locals1.inc = inc;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.sender = this.ConnectedClients.Find((Client x) => x.Connection == CS$<>8__locals1.inc.Sender);
			if (CS$<>8__locals1.sender == null)
			{
				return;
			}
			ClientPermissions command = ClientPermissions.None;
			try
			{
				command = (ClientPermissions)CS$<>8__locals1.inc.ReadUInt16();
			}
			catch
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
			if ((command != ClientPermissions.ManageRound || mpCampaign == null) && (command != ClientPermissions.ManageCampaign || mpCampaign == null) && !CS$<>8__locals1.sender.HasPermission(command))
			{
				GameServer.Log(string.Concat(new string[]
				{
					"Client \"",
					NetworkMember.ClientLogName(CS$<>8__locals1.sender, null),
					"\" sent a server command \"",
					command.ToString(),
					"\". Permission denied."
				}), ServerLog.MessageType.ServerMessage);
				return;
			}
			if (command <= ClientPermissions.SelectSub)
			{
				switch (command)
				{
				case ClientPermissions.ManageRound:
				{
					bool end = CS$<>8__locals1.inc.ReadBoolean();
					if (end)
					{
						if (mpCampaign != null && !CampaignMode.AllowedToManageCampaign(CS$<>8__locals1.sender, ClientPermissions.ManageRound))
						{
							goto IL_C10;
						}
						bool save = CS$<>8__locals1.inc.ReadBoolean();
						bool quitCampaign = CS$<>8__locals1.inc.ReadBoolean();
						if (base.GameStarted)
						{
							using (this.dosProtection.Pause(CS$<>8__locals1.sender))
							{
								GameServer.Log("Client \"" + NetworkMember.ClientLogName(CS$<>8__locals1.sender, null) + "\" ended the round.", ServerLog.MessageType.ServerMessage);
								if (mpCampaign != null && Level.IsLoadedFriendlyOutpost && save)
								{
									mpCampaign.SavePlayers();
									mpCampaign.HandleSaveAndQuit();
									GameMain.GameSession.SubmarineInfo = new SubmarineInfo(GameMain.GameSession.Submarine);
									SaveUtil.SaveGame(GameMain.GameSession.DataPath, false);
								}
								else
								{
									save = false;
								}
								this.EndGame(CampaignMode.TransitionType.None, save, null);
								goto IL_C10;
							}
						}
						if (mpCampaign != null)
						{
							GameServer.Log("Client \"" + NetworkMember.ClientLogName(CS$<>8__locals1.sender, null) + "\" quit the currently active campaign.", ServerLog.MessageType.ServerMessage);
							GameMain.GameSession = null;
							GameMain.NetLobbyScreen.SelectedModeIdentifier = GameModePreset.Sandbox.Identifier;
							NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
							ushort lastUpdateID = netLobbyScreen.LastUpdateID;
							netLobbyScreen.LastUpdateID = lastUpdateID + 1;
							goto IL_C10;
						}
						goto IL_C10;
					}
					else
					{
						CS$<>8__locals1.sender.AFK = false;
						bool continueCampaign = CS$<>8__locals1.inc.ReadBoolean();
						if ((mpCampaign != null && mpCampaign.GameOver) || continueCampaign)
						{
							if (base.GameStarted)
							{
								this.SendDirectChatMessage("Cannot continue the campaign from the previous save (round already running).", CS$<>8__locals1.sender, ChatMessageType.Error);
								goto IL_C10;
							}
							if (!CampaignMode.AllowedToManageCampaign(CS$<>8__locals1.sender, ClientPermissions.ManageCampaign) && !CampaignMode.AllowedToManageCampaign(CS$<>8__locals1.sender, ClientPermissions.ManageMap))
							{
								goto IL_C10;
							}
							using (this.dosProtection.Pause(CS$<>8__locals1.sender))
							{
								MultiPlayerCampaign.LoadCampaign(GameMain.GameSession.DataPath, CS$<>8__locals1.sender);
								goto IL_C10;
							}
						}
						if (!base.GameStarted && !this.initiatedStartGame)
						{
							using (this.dosProtection.Pause(CS$<>8__locals1.sender))
							{
								GameServer.Log("Client \"" + NetworkMember.ClientLogName(CS$<>8__locals1.sender, null) + "\" started the round.", ServerLog.MessageType.ServerMessage);
								GameServer.TryStartGameResult result = this.TryStartGame();
								if (result != GameServer.TryStartGameResult.Success)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
									defaultInterpolatedStringHandler.AppendLiteral("TryStartGameError.");
									defaultInterpolatedStringHandler.AppendFormatted<GameServer.TryStartGameResult>(result);
									this.SendDirectChatMessage(TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Value, CS$<>8__locals1.sender, ChatMessageType.Error);
								}
								goto IL_C10;
							}
						}
						if (mpCampaign == null || (!CampaignMode.AllowedToManageCampaign(CS$<>8__locals1.sender, ClientPermissions.ManageCampaign) && !CampaignMode.AllowedToManageCampaign(CS$<>8__locals1.sender, ClientPermissions.ManageMap)))
						{
							goto IL_C10;
						}
						using (this.dosProtection.Pause(CS$<>8__locals1.sender))
						{
							LevelData levelData;
							Submarine submarine;
							CampaignMode.TransitionType availableTransition = mpCampaign.GetAvailableTransition(out levelData, out submarine);
							bool forceLocation = !mpCampaign.Map.AllowDebugTeleport || mpCampaign.Map.CurrentLocation == Level.Loaded.StartLocation;
							if (availableTransition != CampaignMode.TransitionType.None)
							{
								if (availableTransition != CampaignMode.TransitionType.ReturnToPreviousEmptyLocation)
								{
									if (availableTransition != CampaignMode.TransitionType.ProgressToNextEmptyLocation)
									{
										GameServer.Log("Client \"" + NetworkMember.ClientLogName(CS$<>8__locals1.sender, null) + "\" ended the round.", ServerLog.MessageType.ServerMessage);
										mpCampaign.LoadNewLevel();
									}
									else
									{
										if (forceLocation)
										{
											mpCampaign.Map.SetLocation(mpCampaign.Map.Locations.IndexOf(Level.Loaded.EndLocation));
										}
										mpCampaign.LoadNewLevel();
									}
								}
								else
								{
									if (forceLocation)
									{
										mpCampaign.Map.SelectLocation(mpCampaign.Map.CurrentLocation.Connections.Find(delegate(LocationConnection c)
										{
											LevelData levelData2 = c.LevelData;
											Level loaded = Level.Loaded;
											return levelData2 == ((loaded != null) ? loaded.LevelData : null);
										}).OtherLocation(mpCampaign.Map.CurrentLocation));
									}
									mpCampaign.LoadNewLevel();
								}
							}
							goto IL_C10;
						}
					}
					break;
				}
				case ClientPermissions.Kick:
				{
					string kickedName = CS$<>8__locals1.inc.ReadString().ToLowerInvariant();
					string kickReason = CS$<>8__locals1.inc.ReadString();
					Client kickedClient = this.connectedClients.Find((Client cl) => cl != CS$<>8__locals1.sender && cl.Name.Equals(kickedName, StringComparison.OrdinalIgnoreCase) && cl.Connection != CS$<>8__locals1.<>4__this.OwnerConnection);
					if (kickedClient != null)
					{
						GameServer.Log(string.Concat(new string[]
						{
							"Client \"",
							NetworkMember.ClientLogName(CS$<>8__locals1.sender, null),
							"\" kicked \"",
							NetworkMember.ClientLogName(kickedClient, null),
							"\"."
						}), ServerLog.MessageType.ServerMessage);
						this.KickClient(kickedClient, string.IsNullOrEmpty(kickReason) ? ("ServerMessage.KickedBy~[initiator]=" + CS$<>8__locals1.sender.Name) : kickReason, false);
						goto IL_C10;
					}
					this.SendDirectChatMessage(TextManager.GetServerMessage("ServerMessage.PlayerNotFound~[player]=" + kickedName).Value, CS$<>8__locals1.sender, ChatMessageType.Console);
					goto IL_C10;
				}
				case ClientPermissions.ManageRound | ClientPermissions.Kick:
					goto IL_C10;
				case ClientPermissions.Ban:
				{
					string bannedName = CS$<>8__locals1.inc.ReadString().ToLowerInvariant();
					string banReason = CS$<>8__locals1.inc.ReadString();
					double durationSeconds = CS$<>8__locals1.inc.ReadDouble();
					TimeSpan? banDuration = null;
					if (durationSeconds > 0.0)
					{
						banDuration = new TimeSpan?(TimeSpan.FromSeconds(durationSeconds));
					}
					Client bannedClient = this.connectedClients.Find((Client cl) => cl != CS$<>8__locals1.sender && cl.Name.Equals(bannedName, StringComparison.OrdinalIgnoreCase) && cl.Connection != CS$<>8__locals1.<>4__this.OwnerConnection);
					if (bannedClient != null)
					{
						GameServer.Log(string.Concat(new string[]
						{
							"Client \"",
							NetworkMember.ClientLogName(CS$<>8__locals1.sender, null),
							"\" banned \"",
							NetworkMember.ClientLogName(bannedClient, null),
							"\"."
						}), ServerLog.MessageType.ServerMessage);
						this.BanClient(bannedClient, string.IsNullOrEmpty(banReason) ? ("ServerMessage.BannedBy~[initiator]=" + CS$<>8__locals1.sender.Name) : banReason, banDuration);
						goto IL_C10;
					}
					PreviousPlayer bannedPreviousClient = this.previousPlayers.Find((PreviousPlayer p) => p.Name.Equals(bannedName, StringComparison.OrdinalIgnoreCase));
					if (bannedPreviousClient != null)
					{
						GameServer.Log(string.Concat(new string[]
						{
							"Client \"",
							NetworkMember.ClientLogName(CS$<>8__locals1.sender, null),
							"\" banned \"",
							bannedPreviousClient.Name,
							"\"."
						}), ServerLog.MessageType.ServerMessage);
						this.BanPreviousPlayer(bannedPreviousClient, string.IsNullOrEmpty(banReason) ? ("ServerMessage.BannedBy~[initiator]=" + CS$<>8__locals1.sender.Name) : banReason, banDuration);
						goto IL_C10;
					}
					this.SendDirectChatMessage(TextManager.GetServerMessage("ServerMessage.PlayerNotFound~[player]=" + bannedName).Value, CS$<>8__locals1.sender, ChatMessageType.Console);
					goto IL_C10;
				}
				default:
					if (command != ClientPermissions.Unban)
					{
						if (command != ClientPermissions.SelectSub)
						{
							goto IL_C10;
						}
					}
					else
					{
						bool isPlayerName = CS$<>8__locals1.inc.ReadBoolean();
						CS$<>8__locals1.inc.ReadPadBits();
						string str = CS$<>8__locals1.inc.ReadString();
						if (isPlayerName)
						{
							this.UnbanPlayer(str);
							goto IL_C10;
						}
						Endpoint endpoint;
						if (Endpoint.Parse(str).TryUnwrap(out endpoint))
						{
							this.UnbanPlayer(endpoint);
							goto IL_C10;
						}
						goto IL_C10;
					}
					break;
				}
				SelectedSubType subType = (SelectedSubType)CS$<>8__locals1.inc.ReadByte();
				string subHash = CS$<>8__locals1.inc.ReadString();
				IReadOnlyList<SubmarineInfo> subList = GameMain.NetLobbyScreen.GetSubList();
				SubmarineInfo sub = subList.FirstOrDefault((SubmarineInfo s) => s.MD5Hash.StringRepresentation == subHash);
				if (sub == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(79, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Client \"");
					defaultInterpolatedStringHandler2.AppendFormatted(NetworkMember.ClientLogName(CS$<>8__locals1.sender, null));
					defaultInterpolatedStringHandler2.AppendLiteral("\" attempted to select a sub, could not find a sub with the MD5 hash \"");
					defaultInterpolatedStringHandler2.AppendFormatted(subHash);
					defaultInterpolatedStringHandler2.AppendLiteral("\".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Red), false);
				}
				else
				{
					switch (subType)
					{
					case SelectedSubType.Shuttle:
						GameMain.NetLobbyScreen.SelectedShuttle = sub;
						break;
					case SelectedSubType.Sub:
						GameMain.NetLobbyScreen.SelectedSub = sub;
						break;
					case SelectedSubType.EnemySub:
						GameMain.NetLobbyScreen.SelectedEnemySub = sub;
						break;
					}
				}
			}
			else if (command <= ClientPermissions.ManageCampaign)
			{
				if (command != ClientPermissions.SelectMode)
				{
					if (command == ClientPermissions.ManageCampaign)
					{
						if (mpCampaign != null)
						{
							mpCampaign.ServerRead(CS$<>8__locals1.inc, CS$<>8__locals1.sender);
						}
					}
				}
				else
				{
					ushort modeIndex = CS$<>8__locals1.inc.ReadUInt16();
					GameMain.NetLobbyScreen.SelectedModeIndex = (int)modeIndex;
					string str2 = "Gamemode changed to ";
					GameModePreset selectedMode = GameMain.NetLobbyScreen.SelectedMode;
					GameServer.Log(str2 + (((selectedMode != null) ? selectedMode.Name.Value : null) ?? "none"), ServerLog.MessageType.ServerMessage);
					if (GameMain.NetLobbyScreen.GameModes[(int)modeIndex] == GameModePreset.MultiPlayerCampaign)
					{
						this.TrySendCampaignSetupInfo(CS$<>8__locals1.sender);
					}
				}
			}
			else if (command != ClientPermissions.ConsoleCommands)
			{
				if (command == ClientPermissions.ManagePermissions)
				{
					byte targetClientID = CS$<>8__locals1.inc.ReadByte();
					Client targetClient = this.connectedClients.Find((Client c) => c.SessionId == targetClientID);
					if (targetClient == null || targetClient == CS$<>8__locals1.sender || targetClient.Connection == this.OwnerConnection)
					{
						return;
					}
					targetClient.ReadPermissions(CS$<>8__locals1.inc);
					List<string> permissionNames = new List<string>();
					foreach (object obj in Enum.GetValues(typeof(ClientPermissions)))
					{
						ClientPermissions permission = (ClientPermissions)obj;
						if (permission != ClientPermissions.None && permission != ClientPermissions.All && targetClient.Permissions.HasFlag(permission))
						{
							permissionNames.Add(permission.ToString());
						}
					}
					string logMsg;
					if (permissionNames.Any<string>())
					{
						logMsg = string.Concat(new string[]
						{
							"Client \"",
							NetworkMember.ClientLogName(CS$<>8__locals1.sender, null),
							"\" set the permissions of the client \"",
							NetworkMember.ClientLogName(targetClient, null),
							"\" to ",
							string.Join(", ", permissionNames)
						});
					}
					else
					{
						logMsg = string.Concat(new string[]
						{
							"Client \"",
							NetworkMember.ClientLogName(CS$<>8__locals1.sender, null),
							"\" removed all permissions from the client \"",
							NetworkMember.ClientLogName(targetClient, null),
							"."
						});
					}
					GameServer.Log(logMsg, ServerLog.MessageType.ServerMessage);
					this.UpdateClientPermissions(targetClient);
				}
			}
			else
			{
				DebugConsole.ServerRead(CS$<>8__locals1.inc, CS$<>8__locals1.sender);
			}
			IL_C10:
			CS$<>8__locals1.inc.ReadPadBits();
		}

		// Token: 0x06003411 RID: 13329 RVA: 0x0016279C File Offset: 0x0016099C
		private void ClientWrite(Client c)
		{
			if (base.GameStarted && c.InGame)
			{
				this.ClientWriteIngame(c);
			}
			else
			{
				if (base.GameStarted && c.Character != null && (float)(DateTime.Now - this.roundStartTime).Seconds > 30f)
				{
					c.Character.ClientDisconnected = true;
				}
				this.ClientWriteLobby(c);
			}
			if (c.Connection == this.OwnerConnection)
			{
				while (GameServer.pendingMessagesToOwner.Any<ChatMessage>())
				{
					this.SendDirectChatMessage(GameServer.pendingMessagesToOwner.Dequeue(), c);
				}
			}
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
			if (campaign != null && GameMain.NetLobbyScreen.SelectedMode == campaign.Preset && NetIdUtils.IdMoreRecent(campaign.LastSaveID, c.LastRecvCampaignSave))
			{
				ValueTuple<ushort, float> lastCampaignSaveSendTime = c.LastCampaignSaveSendTime;
				ushort item = lastCampaignSaveSendTime.Item1;
				float item2 = lastCampaignSaveSendTime.Item2;
				if ((item != 0 || item2 != 0f) && campaign.LastSaveID == c.LastCampaignSaveSendTime.Item1 && (double)c.LastCampaignSaveSendTime.Item2 > NetTime.Now - 5.0)
				{
					return;
				}
				if (this.FileSender.ActiveTransfers.None((FileSender.FileTransferOut t) => t.Connection == c.Connection))
				{
					this.FileSender.StartTransfer(c.Connection, FileTransferType.CampaignSave, GameMain.GameSession.DataPath.SavePath);
					c.LastCampaignSaveSendTime = new ValueTuple<ushort, float>(campaign.LastSaveID, (float)NetTime.Now);
				}
			}
		}

		// Token: 0x06003412 RID: 13330 RVA: 0x00162970 File Offset: 0x00160B70
		private void ClientWriteInitial(Client c, IWriteMessage outmsg)
		{
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.NewMessage("Sending initial lobby update to " + c.Name, new Color?(Color.Gray), false);
			}
			outmsg.WriteByte(c.SessionId);
			IReadOnlyList<SubmarineInfo> subList = GameMain.NetLobbyScreen.GetSubList();
			outmsg.WriteUInt16((ushort)subList.Count);
			for (int i = 0; i < subList.Count; i++)
			{
				SubmarineInfo sub = subList[i];
				outmsg.WriteString(sub.Name);
				outmsg.WriteString(sub.MD5Hash.ToString());
				outmsg.WriteByte((byte)sub.SubmarineClass);
				outmsg.WriteBoolean(sub.HasTag(SubmarineTag.Shuttle));
				outmsg.WriteBoolean(sub.RequiredContentPackagesInstalled);
			}
			outmsg.WriteBoolean(base.GameStarted);
			outmsg.WriteBoolean(base.ServerSettings.AllowSpectating);
			outmsg.WriteBoolean(base.ServerSettings.AllowAFK);
			outmsg.WriteBoolean(base.ServerSettings.RespawnMode == RespawnMode.Permadeath);
			outmsg.WriteBoolean(base.ServerSettings.IronmanMode);
			c.WritePermissions(outmsg);
		}

		// Token: 0x06003413 RID: 13331 RVA: 0x00162A84 File Offset: 0x00160C84
		private void ClientWriteIngame(Client c)
		{
			if (!c.NeedsMidRoundSync)
			{
				Character clientCharacter = c.Character;
				foreach (Character otherCharacter in Character.CharacterList)
				{
					if (otherCharacter.Enabled)
					{
						if (c.SpectatePos == null)
						{
							float distSqr = GameServer.<ClientWriteIngame>g__GetShortestDistance|110_0(clientCharacter.WorldPosition, otherCharacter);
							if (clientCharacter.ViewTarget != null && clientCharacter.ViewTarget != clientCharacter)
							{
								distSqr = Math.Min(distSqr, GameServer.<ClientWriteIngame>g__GetShortestDistance|110_0(clientCharacter.ViewTarget.WorldPosition, otherCharacter));
							}
							if (distSqr >= MathUtils.Pow2(otherCharacter.Params.DisableDistance))
							{
								continue;
							}
						}
						else if (otherCharacter != clientCharacter && GameServer.<ClientWriteIngame>g__GetShortestDistance|110_0(c.SpectatePos.Value, otherCharacter) >= MathUtils.Pow2(otherCharacter.Params.DisableDistance))
						{
							continue;
						}
						float updateInterval = otherCharacter.GetPositionUpdateInterval(c);
						float lastSent;
						c.PositionUpdateLastSent.TryGetValue(otherCharacter, out lastSent);
						if ((double)lastSent > NetTime.Now)
						{
							c.PositionUpdateLastSent.Remove(otherCharacter);
						}
						else if ((double)lastSent > NetTime.Now - (double)updateInterval)
						{
							continue;
						}
						if (!c.PendingPositionUpdates.Contains(otherCharacter))
						{
							c.PendingPositionUpdates.Enqueue(otherCharacter);
						}
					}
				}
				using (List<Submarine>.Enumerator enumerator2 = Submarine.Loaded.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Submarine sub = enumerator2.Current;
						if (!sub.Info.IsOutpost && !sub.DockedTo.Any((Submarine s) => s.ID < sub.ID) && sub.PhysicsBody != null && sub.PhysicsBody.BodyType != BodyType.Static && !c.PendingPositionUpdates.Contains(sub))
						{
							c.PendingPositionUpdates.Enqueue(sub);
						}
					}
				}
				foreach (Item item in Item.ItemList)
				{
					if (item.PositionUpdateInterval != float.PositiveInfinity)
					{
						float updateInterval2 = item.GetPositionUpdateInterval(c);
						float lastSent2;
						c.PositionUpdateLastSent.TryGetValue(item, out lastSent2);
						if ((double)lastSent2 > NetTime.Now)
						{
							c.PositionUpdateLastSent.Remove(item);
						}
						else if ((double)lastSent2 > NetTime.Now - (double)updateInterval2)
						{
							continue;
						}
						if (!c.PendingPositionUpdates.Contains(item))
						{
							c.PendingPositionUpdates.Enqueue(item);
						}
					}
				}
			}
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(3);
			outmsg.WriteSingle((float)NetTime.Now);
			outmsg.WriteSingle(this.EndRoundTimeRemaining);
			SegmentTableWriter<ServerNetSegment> segmentTable = SegmentTableWriter<ServerNetSegment>.StartWriting(outmsg);
			try
			{
				segmentTable.StartNewSegment(ServerNetSegment.SyncIds);
				outmsg.WriteUInt16(c.LastSentChatMsgID);
				outmsg.WriteUInt16(c.LastSentEntityEventID);
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
				if (campaign != null && campaign.Preset == GameMain.NetLobbyScreen.SelectedMode)
				{
					outmsg.WriteBoolean(true);
					outmsg.WritePadBits();
					campaign.ServerWrite(outmsg, c);
				}
				else
				{
					outmsg.WriteBoolean(false);
					outmsg.WritePadBits();
				}
				int clientListBytes = outmsg.LengthBytes;
				this.WriteClientList(segmentTable, c, outmsg);
				clientListBytes = outmsg.LengthBytes - clientListBytes;
				int chatMessageBytes = outmsg.LengthBytes;
				GameServer.WriteChatMessages(segmentTable, outmsg, c);
				chatMessageBytes = outmsg.LengthBytes - chatMessageBytes;
				int positionUpdateBytes = outmsg.LengthBytes;
				while (!c.NeedsMidRoundSync && c.PendingPositionUpdates.Count > 0)
				{
					Entity entity = c.PendingPositionUpdates.Peek();
					IServerPositionSync entityPositionSync = entity as IServerPositionSync;
					if (entityPositionSync != null && !entity.Removed)
					{
						Item item2 = entity as Item;
						if (item2 == null || !float.IsInfinity(item2.PositionUpdateInterval))
						{
							ReadWriteMessage tempBuffer = new ReadWriteMessage();
							EntityPositionHeader entityPositionHeader = EntityPositionHeader.FromEntity(entity);
							tempBuffer.WriteNetSerializableStruct(entityPositionHeader);
							entityPositionSync.ServerWritePosition(tempBuffer, c);
							if (outmsg.LengthBytes + tempBuffer.LengthBytes <= 1070)
							{
								segmentTable.StartNewSegment(ServerNetSegment.EntityPosition);
								outmsg.WritePadBits();
								outmsg.WriteVariableUInt32((uint)tempBuffer.LengthBytes);
								outmsg.WriteBytes(tempBuffer.Buffer, 0, tempBuffer.LengthBytes);
								outmsg.WritePadBits();
								c.PositionUpdateLastSent[entity] = (float)NetTime.Now;
								c.PendingPositionUpdates.Dequeue();
								continue;
							}
							break;
						}
					}
					c.PendingPositionUpdates.Dequeue();
				}
				positionUpdateBytes = outmsg.LengthBytes - positionUpdateBytes;
				if (outmsg.LengthBytes > 1170)
				{
					string errorMsg = string.Concat(new string[]
					{
						"Maximum packet size exceeded (",
						outmsg.LengthBytes.ToString(),
						" > ",
						1170.ToString(),
						")\n"
					});
					errorMsg = string.Concat(new string[]
					{
						errorMsg,
						"  Client list size: ",
						clientListBytes.ToString(),
						" bytes\n  Chat message size: ",
						chatMessageBytes.ToString(),
						" bytes\n  Position update size: ",
						positionUpdateBytes.ToString(),
						" bytes\n\n"
					});
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("GameServer.ClientWriteIngame1:PacketSizeExceeded" + outmsg.LengthBytes.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				}
			}
			finally
			{
				segmentTable.Dispose();
			}
			this.serverPeer.Send(outmsg, c.Connection, DeliveryMethod.Unreliable, true);
			for (int i = 0; i < NetConfig.MaxEventPacketsPerUpdate; i++)
			{
				outmsg = new WriteOnlyMessage();
				outmsg.WriteByte(3);
				outmsg.WriteSingle((float)NetTime.Now);
				outmsg.WriteSingle(this.EndRoundTimeRemaining);
				SegmentTableWriter<ServerNetSegment> segmentTable2 = SegmentTableWriter<ServerNetSegment>.StartWriting(outmsg);
				try
				{
					int eventManagerBytes = outmsg.LengthBytes;
					List<NetEntityEvent> sentEvents;
					this.entityEventManager.Write(segmentTable2, c, outmsg, out sentEvents);
					eventManagerBytes = outmsg.LengthBytes - eventManagerBytes;
					if (sentEvents.Count == 0)
					{
						break;
					}
					if (outmsg.LengthBytes > 1170)
					{
						string errorMsg2 = string.Concat(new string[]
						{
							"Maximum packet size exceeded (",
							outmsg.LengthBytes.ToString(),
							" > ",
							1170.ToString(),
							")\n"
						});
						errorMsg2 = errorMsg2 + "  Event size: " + eventManagerBytes.ToString() + " bytes\n";
						if (sentEvents != null && sentEvents.Count > 0)
						{
							errorMsg2 += "Sent events: \n";
							foreach (NetEntityEvent entityEvent in sentEvents)
							{
								string str = errorMsg2;
								string str2 = "  - ";
								Entity entity2 = entityEvent.Entity;
								errorMsg2 = str + str2 + (((entity2 != null) ? entity2.ToString() : null) ?? "null") + "\n";
							}
						}
						DebugConsole.ThrowError(errorMsg2, null, null, false, false);
						GameAnalyticsManager.AddErrorEventOnce("GameServer.ClientWriteIngame2:PacketSizeExceeded" + outmsg.LengthBytes.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
					}
				}
				finally
				{
					segmentTable2.Dispose();
				}
				this.serverPeer.Send(outmsg, c.Connection, DeliveryMethod.Unreliable, true);
			}
		}

		// Token: 0x06003414 RID: 13332 RVA: 0x0016323C File Offset: 0x0016143C
		private void WriteClientList(in SegmentTableWriter<ServerNetSegment> segmentTable, Client c, IWriteMessage outmsg)
		{
			if (!NetIdUtils.IdMoreRecent(base.LastClientListUpdateID, c.LastRecvClientListUpdate))
			{
				return;
			}
			segmentTable.StartNewSegment(ServerNetSegment.ClientList);
			outmsg.WriteUInt16(base.LastClientListUpdateID);
			outmsg.WriteByte((byte)this.Team1Count);
			outmsg.WriteByte((byte)this.Team2Count);
			outmsg.WriteByte((byte)this.connectedClients.Count);
			using (List<Client>.Enumerator enumerator = this.connectedClients.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Client client = enumerator.Current;
					TempClient tempClient = default(TempClient);
					tempClient.SessionId = client.SessionId;
					tempClient.AccountInfo = client.AccountInfo;
					tempClient.NameId = client.NameId;
					tempClient.Name = client.Name;
					Character character = client.Character;
					bool flag;
					if (character == null)
					{
						flag = (null != null);
					}
					else
					{
						CharacterInfo info = character.Info;
						flag = (((info != null) ? info.Job : null) != null);
					}
					tempClient.PreferredJob = ((flag && base.GameStarted) ? client.Character.Info.Job.Prefab.Identifier : client.PreferredJob);
					tempClient.PreferredTeam = client.PreferredTeam;
					tempClient.TeamID = client.TeamID;
					tempClient.CharacterId = ((client.Character == null || !base.GameStarted) ? 0 : client.Character.ID);
					tempClient.Karma = (c.HasPermission(ClientPermissions.ServerLog) ? client.Karma : 100f);
					tempClient.Muted = client.Muted;
					tempClient.InGame = client.InGame;
					tempClient.HasPermissions = (client.Permissions > ClientPermissions.None);
					tempClient.IsOwner = (client.Connection == this.OwnerConnection);
					tempClient.IsDownloading = this.FileSender.ActiveTransfers.Any((FileSender.FileTransferOut t) => t.Connection == client.Connection);
					TempClient tempClientData = tempClient;
					outmsg.WriteNetSerializableStruct(tempClientData);
					outmsg.WritePadBits();
				}
			}
		}

		// Token: 0x06003415 RID: 13333 RVA: 0x001634AC File Offset: 0x001616AC
		public void ClientWriteLobby(Client c)
		{
			bool isInitialUpdate = false;
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(2);
			SegmentTableWriter<ServerNetSegment> segmentTable = SegmentTableWriter<ServerNetSegment>.StartWriting(outmsg);
			bool messageTooLarge;
			try
			{
				segmentTable.StartNewSegment(ServerNetSegment.SyncIds);
				int settingsBytes = outmsg.LengthBytes;
				int initialUpdateBytes = 0;
				if (base.ServerSettings.UnsentFlags() != ServerSettings.NetFlags.None)
				{
					NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
					ushort lastUpdateID = netLobbyScreen.LastUpdateID;
					netLobbyScreen.LastUpdateID = lastUpdateID + 1;
				}
				IWriteMessage settingsBuf = null;
				if (NetIdUtils.IdMoreRecent(GameMain.NetLobbyScreen.LastUpdateID, c2.LastRecvLobbyUpdate))
				{
					outmsg.WriteBoolean(true);
					outmsg.WritePadBits();
					outmsg.WriteUInt16(GameMain.NetLobbyScreen.LastUpdateID);
					settingsBuf = new ReadWriteMessage();
					base.ServerSettings.ServerWrite(settingsBuf, c2);
					outmsg.WriteUInt16((ushort)settingsBuf.LengthBytes);
					outmsg.WriteBytes(settingsBuf.Buffer, 0, settingsBuf.LengthBytes);
					outmsg.WriteBoolean(!c2.InitialLobbyUpdateSent);
					if (!c2.InitialLobbyUpdateSent)
					{
						isInitialUpdate = true;
						initialUpdateBytes = outmsg.LengthBytes;
						this.ClientWriteInitial(c2, outmsg);
						c2.InitialLobbyUpdateSent = true;
						initialUpdateBytes = outmsg.LengthBytes - initialUpdateBytes;
					}
					outmsg.WriteString(GameMain.NetLobbyScreen.SelectedSub.Name);
					outmsg.WriteString(GameMain.NetLobbyScreen.SelectedSub.MD5Hash.ToString());
					SubmarineInfo enemySub = GameMain.NetLobbyScreen.SelectedEnemySub;
					if (enemySub != null)
					{
						outmsg.WriteBoolean(true);
						outmsg.WriteString(enemySub.Name);
						outmsg.WriteString(enemySub.MD5Hash.ToString());
					}
					else
					{
						outmsg.WriteBoolean(false);
					}
					outmsg.WriteBoolean(this.IsUsingRespawnShuttle());
					SubmarineInfo selectedShuttle = (base.GameStarted && base.RespawnManager != null && base.RespawnManager.UsingShuttle) ? base.RespawnManager.RespawnShuttles.First<Submarine>().Info : GameMain.NetLobbyScreen.SelectedShuttle;
					outmsg.WriteString(selectedShuttle.Name);
					outmsg.WriteString(selectedShuttle.MD5Hash.ToString());
					outmsg.WriteBoolean(base.ServerSettings.AllowSubVoting);
					outmsg.WriteBoolean(base.ServerSettings.AllowModeVoting);
					outmsg.WriteBoolean(base.ServerSettings.VoiceChatEnabled);
					outmsg.WriteBoolean(base.ServerSettings.AllowSpectating);
					outmsg.WriteBoolean(base.ServerSettings.AllowAFK);
					outmsg.WriteSingle(base.ServerSettings.TraitorProbability);
					outmsg.WriteRangedInteger(base.ServerSettings.TraitorDangerLevel, 1, 3);
					outmsg.WriteVariableUInt32((uint)GameMain.NetLobbyScreen.MissionTypes.Count<Identifier>());
					foreach (Identifier missionType in GameMain.NetLobbyScreen.MissionTypes)
					{
						outmsg.WriteIdentifier(missionType);
					}
					outmsg.WriteByte((byte)GameMain.NetLobbyScreen.SelectedModeIndex);
					outmsg.WriteString(GameMain.NetLobbyScreen.LevelSeed);
					outmsg.WriteSingle(base.ServerSettings.SelectedLevelDifficulty);
					outmsg.WriteByte((byte)base.ServerSettings.BotCount);
					outmsg.WriteBoolean(base.ServerSettings.BotSpawnMode == BotSpawnMode.Fill);
					outmsg.WriteBoolean(base.ServerSettings.AutoRestart);
					if (base.ServerSettings.AutoRestart)
					{
						outmsg.WriteSingle(this.autoRestartTimerRunning ? base.ServerSettings.AutoRestartTimer : 0f);
					}
					if (GameMain.NetLobbyScreen.SelectedMode == GameModePreset.MultiPlayerCampaign && this.connectedClients.None((Client c) => c.Connection == this.OwnerConnection || c.HasPermission(ClientPermissions.ManageRound) || c.HasPermission(ClientPermissions.ManageCampaign)))
					{
						this.TrySendCampaignSetupInfo(c2);
					}
				}
				else
				{
					outmsg.WriteBoolean(false);
					outmsg.WritePadBits();
				}
				settingsBytes = outmsg.LengthBytes - settingsBytes;
				int campaignBytes = outmsg.LengthBytes;
				bool hasSpaceForCampaignData = outmsg.LengthBytes < 670;
				if (hasSpaceForCampaignData)
				{
					GameSession gameSession = GameMain.GameSession;
					MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
					if (campaign != null && campaign.Preset == GameMain.NetLobbyScreen.SelectedMode)
					{
						outmsg.WriteBoolean(true);
						outmsg.WritePadBits();
						campaign.ServerWrite(outmsg, c2);
						goto IL_42C;
					}
				}
				outmsg.WriteBoolean(false);
				outmsg.WritePadBits();
				if (!hasSpaceForCampaignData)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Not enough space to fit campaign data in the lobby update (length ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(outmsg.LengthBytes);
					defaultInterpolatedStringHandler.AppendLiteral(" bytes), omitting...");
					DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				IL_42C:
				campaignBytes = outmsg.LengthBytes - campaignBytes;
				outmsg.WriteUInt16(c2.LastSentChatMsgID);
				int clientListBytes = outmsg.LengthBytes;
				if (outmsg.LengthBytes < 670)
				{
					this.WriteClientList(segmentTable, c2, outmsg);
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(84, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Not enough space to fit client list in the lobby update (length ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(outmsg.LengthBytes);
					defaultInterpolatedStringHandler2.AppendLiteral(" bytes), omitting...");
					DebugConsole.Log(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
				clientListBytes = outmsg.LengthBytes - clientListBytes;
				int chatMessageBytes = outmsg.LengthBytes;
				GameServer.WriteChatMessages(segmentTable, outmsg, c2);
				chatMessageBytes = outmsg.LengthBytes - chatMessageBytes;
				messageTooLarge = (outmsg.LengthBytes > 1170);
				if (messageTooLarge && !isInitialUpdate)
				{
					string warningMsg = string.Concat(new string[]
					{
						"Maximum packet size exceeded, will send using reliable mode (",
						outmsg.LengthBytes.ToString(),
						" > ",
						1170.ToString(),
						")\n"
					});
					warningMsg = string.Concat(new string[]
					{
						warningMsg,
						"  Client list size: ",
						clientListBytes.ToString(),
						" bytes\n  Chat message size: ",
						chatMessageBytes.ToString(),
						" bytes\n  Campaign size: ",
						campaignBytes.ToString(),
						" bytes\n  Settings size: ",
						settingsBytes.ToString(),
						" bytes\n"
					});
					if (initialUpdateBytes > 0)
					{
						warningMsg = warningMsg + "    Initial update size: " + initialUpdateBytes.ToString() + " bytes\n";
					}
					if (settingsBuf != null)
					{
						warningMsg = warningMsg + "    Settings buffer size: " + settingsBuf.LengthBytes.ToString() + " bytes\n";
					}
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						DebugConsole.AddWarning(warningMsg, null);
					}
					GameAnalyticsManager.AddErrorEventOnce("GameServer.ClientWriteIngame1:ClientWriteLobby" + outmsg.LengthBytes.ToString(), GameAnalyticsManager.ErrorSeverity.Warning, warningMsg);
				}
			}
			finally
			{
				segmentTable.Dispose();
			}
			if (isInitialUpdate || messageTooLarge)
			{
				this.serverPeer.Send(outmsg, c2.Connection, DeliveryMethod.Reliable, true);
				c2.LastRecvLobbyUpdate = GameMain.NetLobbyScreen.LastUpdateID;
			}
			else
			{
				this.serverPeer.Send(outmsg, c2.Connection, DeliveryMethod.Unreliable, true);
			}
			if (isInitialUpdate)
			{
				this.SendVoteStatus(new List<Client>
				{
					c2
				});
			}
		}

		// Token: 0x06003416 RID: 13334 RVA: 0x00163B54 File Offset: 0x00161D54
		private static void WriteChatMessages(in SegmentTableWriter<ServerNetSegment> segmentTable, IWriteMessage outmsg, Client c)
		{
			c.ChatMsgQueue.RemoveAll((ChatMessage cMsg) => !NetIdUtils.IdMoreRecent(cMsg.NetStateID, c.LastRecvChatMsgID));
			int i = 0;
			while (i < c.ChatMsgQueue.Count && i < 10)
			{
				if (outmsg.LengthBytes + c.ChatMsgQueue[i].EstimateLengthBytesServer(c) > 1165 && i > 0)
				{
					return;
				}
				c.ChatMsgQueue[i].ServerWrite(segmentTable, outmsg, c);
				i++;
			}
		}

		// Token: 0x06003417 RID: 13335 RVA: 0x00163BF8 File Offset: 0x00161DF8
		public GameServer.TryStartGameResult TryStartGame()
		{
			if (this.initiatedStartGame || base.GameStarted)
			{
				return GameServer.TryStartGameResult.GameAlreadyStarted;
			}
			GameModePreset selectedMode = Voting.HighestVoted<GameModePreset>(VoteType.Mode, this.connectedClients) ?? GameMain.NetLobbyScreen.SelectedMode;
			if (selectedMode == null)
			{
				return GameServer.TryStartGameResult.GameModeNotSelected;
			}
			if (selectedMode == GameModePreset.MultiPlayerCampaign)
			{
				GameSession gameSession = GameMain.GameSession;
				if (!(((gameSession != null) ? gameSession.GameMode : null) is MultiPlayerCampaign))
				{
					if (GameMain.NetLobbyScreen.SelectedMode != GameModePreset.MultiPlayerCampaign)
					{
						GameMain.NetLobbyScreen.SelectedModeIdentifier = GameModePreset.MultiPlayerCampaign.Identifier;
					}
					return GameServer.TryStartGameResult.CannotStartMultiplayerCampaign;
				}
			}
			bool applyPerks = GameSession.ShouldApplyDisembarkPoints(selectedMode);
			if (applyPerks && !GameSession.ValidatedDisembarkPoints(selectedMode, GameMain.NetLobbyScreen.MissionTypes))
			{
				return GameServer.TryStartGameResult.PerksExceedAllowance;
			}
			GameServer.Log("Starting a new round...", ServerLog.MessageType.ServerMessage);
			SubmarineInfo selectedShuttle = GameMain.NetLobbyScreen.SelectedShuttle;
			Option.UnspecifiedNone none = Option.None;
			Option<SubmarineInfo> selectedEnemySub = none;
			SubmarineInfo selectedSub;
			if (base.ServerSettings.AllowSubVoting)
			{
				if (selectedMode == GameModePreset.PvP)
				{
					IEnumerable<Client> team1Voters = from c in this.connectedClients
					where c.PreferredTeam == CharacterTeamType.Team1
					select c;
					IEnumerable<Client> team2Voters = from c in this.connectedClients
					where c.PreferredTeam == CharacterTeamType.Team2
					select c;
					int team1VoteCount;
					SubmarineInfo team1Sub = Voting.HighestVoted<SubmarineInfo>(VoteType.Sub, team1Voters, out team1VoteCount);
					int team2VoteCount;
					SubmarineInfo team2Sub = Voting.HighestVoted<SubmarineInfo>(VoteType.Sub, team2Voters, out team2VoteCount);
					if (team1VoteCount > 0)
					{
						selectedSub = team1Sub;
					}
					else
					{
						selectedSub = ((team2VoteCount > 0) ? team2Sub : GameMain.NetLobbyScreen.SelectedSub);
					}
					if (team2VoteCount > 0 && team2Sub != null)
					{
						selectedEnemySub = Option.Some<SubmarineInfo>(team2Sub);
					}
				}
				else
				{
					selectedSub = (Voting.HighestVoted<SubmarineInfo>(VoteType.Sub, this.connectedClients) ?? GameMain.NetLobbyScreen.SelectedSub);
				}
			}
			else
			{
				selectedSub = GameMain.NetLobbyScreen.SelectedSub;
				SubmarineInfo enemySub = GameMain.NetLobbyScreen.SelectedEnemySub ?? GameMain.NetLobbyScreen.SelectedSub;
				if (enemySub != null)
				{
					selectedEnemySub = Option.Some<SubmarineInfo>(enemySub);
				}
			}
			if (selectedSub == null || selectedShuttle == null)
			{
				return GameServer.TryStartGameResult.SubmarineNotFound;
			}
			PerkCollection incompatiblePerks;
			if (applyPerks && this.CheckIfAnyPerksAreIncompatible(selectedSub, selectedEnemySub.Fallback(selectedSub), selectedMode, out incompatiblePerks))
			{
				CoroutineManager.StartCoroutine(this.WarnAndDelayStartGame(incompatiblePerks, selectedSub, selectedEnemySub, selectedShuttle, selectedMode), "WarnAndDelayStartGame");
				return GameServer.TryStartGameResult.Success;
			}
			this.initiatedStartGame = true;
			this.startGameCoroutine = CoroutineManager.StartCoroutine(this.InitiateStartGame(selectedSub, selectedEnemySub, selectedShuttle, selectedMode), "InitiateStartGame");
			return GameServer.TryStartGameResult.Success;
		}

		// Token: 0x06003418 RID: 13336 RVA: 0x00163E2C File Offset: 0x0016202C
		private bool CheckIfAnyPerksAreIncompatible(SubmarineInfo team1Sub, SubmarineInfo team2Sub, GameModePreset preset, out PerkCollection incompatiblePerks)
		{
			ImmutableArray<DisembarkPerkPrefab>.Builder incompatibleTeam1Perks = ImmutableArray.CreateBuilder<DisembarkPerkPrefab>();
			ImmutableArray<DisembarkPerkPrefab>.Builder incompatibleTeam2Perks = ImmutableArray.CreateBuilder<DisembarkPerkPrefab>();
			bool hasIncompatiblePerks = false;
			PerkCollection perks = GameSession.GetPerks();
			bool ignorePerksThatCanNotApplyWithoutSubmarine = GameSession.ShouldIgnorePerksThatCanNotApplyWithoutSubmarine(preset, GameMain.NetLobbyScreen.MissionTypes);
			Func<PerkBase, bool> <>9__1;
			foreach (DisembarkPerkPrefab perk in perks.Team1Perks)
			{
				if (ignorePerksThatCanNotApplyWithoutSubmarine)
				{
					if (perk.PerkBehaviors.Any((PerkBase p) => !p.CanApplyWithoutSubmarine()))
					{
						continue;
					}
				}
				ImmutableArray<PerkBase> perkBehaviors = perk.PerkBehaviors;
				Func<PerkBase, bool> predicate;
				if ((predicate = <>9__1) == null)
				{
					predicate = (<>9__1 = ((PerkBase p) => !p.CanApply(team1Sub)));
				}
				bool anyCanNotApply = perkBehaviors.Any(predicate);
				if (anyCanNotApply)
				{
					incompatibleTeam1Perks.Add(perk);
					hasIncompatiblePerks = true;
				}
			}
			if (preset == GameModePreset.PvP)
			{
				Func<PerkBase, bool> <>9__3;
				foreach (DisembarkPerkPrefab perk2 in perks.Team2Perks)
				{
					if (ignorePerksThatCanNotApplyWithoutSubmarine)
					{
						if (perk2.PerkBehaviors.Any((PerkBase p) => !p.CanApplyWithoutSubmarine()))
						{
							continue;
						}
					}
					ImmutableArray<PerkBase> perkBehaviors2 = perk2.PerkBehaviors;
					Func<PerkBase, bool> predicate2;
					if ((predicate2 = <>9__3) == null)
					{
						predicate2 = (<>9__3 = ((PerkBase p) => !p.CanApply(team2Sub)));
					}
					bool anyCanNotApply2 = perkBehaviors2.Any(predicate2);
					if (anyCanNotApply2)
					{
						incompatibleTeam2Perks.Add(perk2);
						hasIncompatiblePerks = true;
					}
				}
			}
			incompatiblePerks = new PerkCollection(incompatibleTeam1Perks.ToImmutable(), incompatibleTeam2Perks.ToImmutable());
			return hasIncompatiblePerks;
		}

		// Token: 0x06003419 RID: 13337 RVA: 0x00163FC0 File Offset: 0x001621C0
		private void AbortStartGameIfWarningActive()
		{
			this.isRoundStartWarningActive = false;
			if (base.ServerSettings.AutoRestart)
			{
				base.ServerSettings.AutoRestartTimer = Math.Max(base.ServerSettings.AutoRestartInterval, 5f);
			}
			foreach (Client client in this.connectedClients)
			{
				client.SetVote(VoteType.StartRound, false);
			}
			int clientsReady = this.connectedClients.Count((Client c) => c.GetVote<bool>(VoteType.StartRound));
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			ushort lastUpdateID = netLobbyScreen.LastUpdateID;
			netLobbyScreen.LastUpdateID = lastUpdateID + 1;
			CoroutineManager.StopCoroutines("WarnAndDelayStartGame");
		}

		// Token: 0x0600341A RID: 13338 RVA: 0x00164098 File Offset: 0x00162298
		private IEnumerable<CoroutineStatus> WarnAndDelayStartGame(PerkCollection incompatiblePerks, SubmarineInfo selectedSub, Option<SubmarineInfo> selectedEnemySub, SubmarineInfo selectedShuttle, GameModePreset selectedMode)
		{
			GameServer.<WarnAndDelayStartGame>d__119 <WarnAndDelayStartGame>d__ = new GameServer.<WarnAndDelayStartGame>d__119(-2);
			<WarnAndDelayStartGame>d__.<>4__this = this;
			<WarnAndDelayStartGame>d__.<>3__incompatiblePerks = incompatiblePerks;
			<WarnAndDelayStartGame>d__.<>3__selectedSub = selectedSub;
			<WarnAndDelayStartGame>d__.<>3__selectedEnemySub = selectedEnemySub;
			<WarnAndDelayStartGame>d__.<>3__selectedShuttle = selectedShuttle;
			<WarnAndDelayStartGame>d__.<>3__selectedMode = selectedMode;
			return <WarnAndDelayStartGame>d__;
		}

		// Token: 0x0600341B RID: 13339 RVA: 0x001640CD File Offset: 0x001622CD
		private IEnumerable<CoroutineStatus> InitiateStartGame(SubmarineInfo selectedSub, Option<SubmarineInfo> selectedEnemySub, SubmarineInfo selectedShuttle, GameModePreset selectedMode)
		{
			GameServer.<InitiateStartGame>d__120 <InitiateStartGame>d__ = new GameServer.<InitiateStartGame>d__120(-2);
			<InitiateStartGame>d__.<>4__this = this;
			<InitiateStartGame>d__.<>3__selectedSub = selectedSub;
			<InitiateStartGame>d__.<>3__selectedEnemySub = selectedEnemySub;
			<InitiateStartGame>d__.<>3__selectedShuttle = selectedShuttle;
			<InitiateStartGame>d__.<>3__selectedMode = selectedMode;
			return <InitiateStartGame>d__;
		}

		// Token: 0x0600341C RID: 13340 RVA: 0x001640FA File Offset: 0x001622FA
		private IEnumerable<CoroutineStatus> StartGame(SubmarineInfo selectedSub, SubmarineInfo selectedShuttle, Option<SubmarineInfo> selectedEnemySub, GameModePreset selectedMode, CampaignSettings settings)
		{
			GameServer.<StartGame>d__121 <StartGame>d__ = new GameServer.<StartGame>d__121(-2);
			<StartGame>d__.<>4__this = this;
			<StartGame>d__.<>3__selectedSub = selectedSub;
			<StartGame>d__.<>3__selectedShuttle = selectedShuttle;
			<StartGame>d__.<>3__selectedEnemySub = selectedEnemySub;
			<StartGame>d__.<>3__selectedMode = selectedMode;
			<StartGame>d__.<>3__settings = settings;
			return <StartGame>d__;
		}

		// Token: 0x0600341D RID: 13341 RVA: 0x00164130 File Offset: 0x00162330
		private void SendStartMessage(int seed, string levelSeed, GameSession gameSession, List<Client> clients, bool includesFinalize)
		{
			foreach (Client client in clients)
			{
				this.SendStartMessage(seed, levelSeed, gameSession, client, includesFinalize);
			}
		}

		// Token: 0x0600341E RID: 13342 RVA: 0x00164184 File Offset: 0x00162384
		private void SendStartMessage(int seed, string levelSeed, GameSession gameSession, Client client, bool includesFinalize)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(17);
			msg.WriteInt32(seed);
			msg.WriteIdentifier(gameSession.GameMode.Preset.Identifier);
			MissionMode missionMode = GameMain.GameSession.GameMode as MissionMode;
			bool flag;
			if (missionMode != null)
			{
				flag = !missionMode.Missions.Any((Mission m) => !m.AllowRespawning);
			}
			else
			{
				flag = true;
			}
			bool missionAllowRespawn = flag;
			msg.WriteBoolean(base.ServerSettings.RespawnMode != RespawnMode.BetweenRounds && missionAllowRespawn);
			msg.WriteBoolean(base.ServerSettings.AllowDisguises);
			msg.WriteBoolean(base.ServerSettings.AllowRewiring);
			msg.WriteBoolean(base.ServerSettings.AllowImmediateItemDelivery);
			msg.WriteBoolean(base.ServerSettings.AllowFriendlyFire);
			msg.WriteBoolean(base.ServerSettings.AllowDragAndDropGive);
			msg.WriteBoolean(base.ServerSettings.LockAllDefaultWires);
			msg.WriteBoolean(base.ServerSettings.AllowLinkingWifiToChat);
			msg.WriteInt32(base.ServerSettings.MaximumMoneyTransferRequest);
			msg.WriteByte((byte)base.ServerSettings.RespawnMode);
			msg.WriteBoolean(this.IsUsingRespawnShuttle());
			msg.WriteByte((byte)base.ServerSettings.LosMode);
			msg.WriteByte((byte)base.ServerSettings.ShowEnemyHealthBars);
			msg.WriteBoolean(includesFinalize);
			msg.WritePadBits();
			base.ServerSettings.WriteMonsterEnabled(msg, null);
			GameSession gameSession2 = GameMain.GameSession;
			GameMode gameMode = (gameSession2 != null) ? gameSession2.GameMode : null;
			MultiPlayerCampaign campaign = gameMode as MultiPlayerCampaign;
			if (campaign == null)
			{
				msg.WriteString(levelSeed);
				msg.WriteSingle(base.ServerSettings.SelectedLevelDifficulty);
				IWriteMessage writeMessage = msg;
				Identifier biome = base.ServerSettings.Biome;
				Identifier identifier = "Random".ToIdentifier();
				writeMessage.WriteIdentifier((biome == identifier) ? Identifier.Empty : base.ServerSettings.Biome);
				msg.WriteString(gameSession.SubmarineInfo.Name);
				msg.WriteString(gameSession.SubmarineInfo.MD5Hash.StringRepresentation);
				SubmarineInfo selectedShuttle = (base.GameStarted && base.RespawnManager != null && base.RespawnManager.UsingShuttle) ? base.RespawnManager.RespawnShuttles.First<Submarine>().Info : GameMain.NetLobbyScreen.SelectedShuttle;
				msg.WriteString(selectedShuttle.Name);
				msg.WriteString(selectedShuttle.MD5Hash.StringRepresentation);
				SubmarineInfo enemySub = gameSession.EnemySubmarineInfo;
				if (enemySub != null)
				{
					msg.WriteBoolean(true);
					msg.WriteString(enemySub.Name);
					msg.WriteString(enemySub.MD5Hash.StringRepresentation);
				}
				else
				{
					msg.WriteBoolean(false);
				}
				msg.WriteByte((byte)GameMain.GameSession.GameMode.Missions.Count<Mission>());
				using (IEnumerator<Mission> enumerator = GameMain.GameSession.GameMode.Missions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Mission mission = enumerator.Current;
						msg.WriteUInt32(mission.Prefab.UintIdentifier);
					}
					goto IL_3D6;
				}
			}
			int nextLocationIndex = campaign.Map.Locations.FindIndex((Location l) => l.LevelData == campaign.NextLevel);
			int nextConnectionIndex = campaign.Map.Connections.FindIndex((LocationConnection c) => c.LevelData == campaign.NextLevel);
			msg.WriteByte(campaign.CampaignID);
			msg.WriteByte((campaign == null) ? 0 : campaign.RoundID);
			msg.WriteUInt16(campaign.LastSaveID);
			msg.WriteInt32(nextLocationIndex);
			msg.WriteInt32(nextConnectionIndex);
			msg.WriteInt32(campaign.Map.SelectedLocationIndex);
			msg.WriteBoolean(campaign.MirrorLevel);
			IL_3D6:
			if (includesFinalize)
			{
				this.WriteRoundStartFinalize(msg, client);
			}
			this.serverPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x0600341F RID: 13343 RVA: 0x0016459C File Offset: 0x0016279C
		private bool TrySendCampaignSetupInfo(Client client)
		{
			if (!CampaignMode.AllowedToManageCampaign(client, ClientPermissions.ManageRound))
			{
				return false;
			}
			using (this.dosProtection.Pause(client))
			{
				IReadOnlyList<CampaignMode.SaveInfo> saveInfos = SaveUtil.GetSaveFiles(SaveUtil.SaveType.Multiplayer, false, true);
				IWriteMessage msg = new WriteOnlyMessage();
				msg.WriteByte(8);
				msg.WriteByte((byte)Math.Min(saveInfos.Count, 255));
				int i = 0;
				while (i < saveInfos.Count && i < 255)
				{
					msg.WriteNetSerializableStruct(saveInfos[i]);
					i++;
				}
				this.serverPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
			}
			return true;
		}

		// Token: 0x06003420 RID: 13344 RVA: 0x00164644 File Offset: 0x00162844
		private bool IsUsingRespawnShuttle()
		{
			return base.ServerSettings.UseRespawnShuttle || (base.GameStarted && base.RespawnManager != null && base.RespawnManager.UsingShuttle);
		}

		// Token: 0x06003421 RID: 13345 RVA: 0x00164674 File Offset: 0x00162874
		private void SendRoundStartFinalize(Client client)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(18);
			this.WriteRoundStartFinalize(msg, client);
			this.serverPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06003422 RID: 13346 RVA: 0x001646AC File Offset: 0x001628AC
		private void WriteRoundStartFinalize(IWriteMessage msg, Client client)
		{
			IEnumerable<ContentFile> contentToPreload = GameMain.GameSession.EventManager.GetFilesToPreload();
			msg.WriteUInt16((ushort)contentToPreload.Count<ContentFile>());
			foreach (ContentFile contentFile in contentToPreload)
			{
				msg.WriteString(contentFile.Path.Value);
			}
			MultiPlayerCampaign multiPlayerCampaign = GameMain.GameSession.Campaign as MultiPlayerCampaign;
			msg.WriteByte((multiPlayerCampaign != null) ? multiPlayerCampaign.RoundID : 0);
			Submarine mainSub = Submarine.MainSub;
			msg.WriteInt32((mainSub != null) ? mainSub.Info.EqualityCheckVal : 0);
			msg.WriteByte((byte)GameMain.GameSession.Missions.Count<Mission>());
			foreach (Mission mission in GameMain.GameSession.Missions)
			{
				msg.WriteIdentifier(mission.Prefab.Identifier);
			}
			foreach (Level.LevelGenStage stage in from s in Enum.GetValues(typeof(Level.LevelGenStage)).OfType<Level.LevelGenStage>()
			orderby s
			select s)
			{
				msg.WriteInt32(GameMain.GameSession.Level.EqualityCheckValues[stage]);
			}
			foreach (Mission mission2 in GameMain.GameSession.Missions)
			{
				mission2.ServerWriteInitial(msg, client);
			}
			msg.WriteBoolean(GameMain.GameSession.CrewManager != null);
			CrewManager crewManager = GameMain.GameSession.CrewManager;
			if (crewManager != null)
			{
				crewManager.ServerWriteActiveOrders(msg);
			}
			GameMode gameMode = GameMain.GameSession.GameMode;
			msg.WriteBoolean(GameSession.ShouldApplyDisembarkPoints((gameMode != null) ? gameMode.Preset : null));
		}

		// Token: 0x06003423 RID: 13347 RVA: 0x001648D8 File Offset: 0x00162AD8
		public void EndGame(CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None, bool wasSaved = false, IEnumerable<Mission> missions = null)
		{
			if (base.GameStarted)
			{
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					GameServer.Log("Ending the round...\n" + Environment.StackTrace.CleanupStackTrace(), ServerLog.MessageType.ServerMessage);
				}
				else
				{
					GameServer.Log("Ending the round...", ServerLog.MessageType.ServerMessage);
				}
			}
			string endMessage = TextManager.FormatServerMessage("RoundSummaryRoundHasEnded");
			if (missions == null)
			{
				missions = GameMain.GameSession.Missions.ToList<Mission>();
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null && gameSession.IsRunning)
			{
				GameMain.GameSession.EndRound(endMessage, CampaignMode.TransitionType.None, null, true);
			}
			TraitorManager traitorManager = this.traitorManager;
			TraitorManager.TraitorResults? traitorResults2 = (traitorManager != null) ? traitorManager.GetEndResults() : null;
			TraitorManager.TraitorResults? traitorResults = (traitorResults2 != null) ? traitorResults2 : null;
			this.EndRoundTimer = 0f;
			if (base.ServerSettings.AutoRestart)
			{
				base.ServerSettings.AutoRestartTimer = base.ServerSettings.AutoRestartInterval;
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				ushort lastUpdateID = netLobbyScreen.LastUpdateID;
				netLobbyScreen.LastUpdateID = lastUpdateID + 1;
			}
			if (base.ServerSettings.SaveServerLogs)
			{
				base.ServerSettings.ServerLog.Save();
			}
			GameMain.GameScreen.Cam.TargetPos = Vector2.Zero;
			this.entityEventManager.Clear();
			foreach (Client c in this.connectedClients)
			{
				c.ResetSync();
			}
			if (base.GameStarted)
			{
				base.KarmaManager.OnRoundEnded();
			}
			base.RespawnManager = null;
			base.GameStarted = false;
			if (this.connectedClients.Count > 0)
			{
				IWriteMessage msg = new WriteOnlyMessage();
				msg.WriteByte(19);
				msg.WriteByte((byte)transitionType);
				msg.WriteBoolean(wasSaved);
				msg.WriteString(endMessage);
				msg.WriteByte((byte)missions.Count<Mission>());
				foreach (Mission mission in missions)
				{
					msg.WriteBoolean(mission.Completed);
				}
				IWriteMessage writeMessage = msg;
				GameSession gameSession2 = GameMain.GameSession;
				writeMessage.WriteByte((gameSession2 == null || gameSession2.WinningTeam == null) ? 0 : ((byte)GameMain.GameSession.WinningTeam.Value));
				msg.WriteBoolean(traitorResults != null);
				if (traitorResults != null)
				{
					msg.WriteNetSerializableStruct(traitorResults.Value);
				}
				foreach (Client client in this.connectedClients)
				{
					this.serverPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
					Character character = client.Character;
					if (character != null)
					{
						CharacterInfo info = character.Info;
						if (info != null)
						{
							info.ClearCurrentOrders();
						}
					}
					client.Character = null;
					client.HasSpawned = false;
					client.InGame = false;
					client.WaitForNextRoundRespawn = null;
				}
			}
			this.entityEventManager.Clear();
			Submarine.Unload();
			GameMain.NetLobbyScreen.Select();
			GameServer.Log("Round ended.", ServerLog.MessageType.ServerMessage);
			GameMain.NetLobbyScreen.RandomizeSettings();
		}

		// Token: 0x06003424 RID: 13348 RVA: 0x00164C30 File Offset: 0x00162E30
		public override void AddChatMessage(ChatMessage message)
		{
			if (string.IsNullOrEmpty(message.Text))
			{
				return;
			}
			string logMsg;
			if (message.SenderClient != null)
			{
				logMsg = NetworkMember.ClientLogName(message.SenderClient, null) + ": " + message.TranslatedText;
			}
			else
			{
				logMsg = message.TextWithSender;
			}
			Character sender = message.Sender as Character;
			if (sender != null)
			{
				sender.TextChatVolume = 1f;
			}
			GameServer.Log(logMsg, ServerLog.MessageType.Chat);
		}

		// Token: 0x06003425 RID: 13349 RVA: 0x00164C9C File Offset: 0x00162E9C
		private bool ReadClientNameChange(Client c, IReadMessage inc)
		{
			ushort nameId = inc.ReadUInt16();
			string newName = inc.ReadString();
			Identifier newJob = inc.ReadIdentifier();
			CharacterTeamType newTeam = (CharacterTeamType)inc.ReadByte();
			if (c == null || string.IsNullOrEmpty(newName) || !NetIdUtils.IdMoreRecent(nameId, c.NameId))
			{
				return false;
			}
			JobPrefab newJobPrefab;
			if (!newJob.IsEmpty && (!JobPrefab.Prefabs.TryGet(newJob, out newJobPrefab) || newJobPrefab.HiddenJob))
			{
				newJob = Identifier.Empty;
			}
			if (newName == c.Name && newJob == c.PreferredJob && newTeam == c.PreferredTeam)
			{
				return false;
			}
			c.NameId = nameId;
			c.PreferredJob = newJob;
			if (newTeam != c.PreferredTeam)
			{
				c.PreferredTeam = newTeam;
				this.RefreshPvpTeamAssignments(false, false);
			}
			TimeSpan timeSinceNameChange = DateTime.Now - c.LastNameChangeTime;
			if (timeSinceNameChange < Client.NameChangeCoolDown && newName != c.Name)
			{
				if (timeSinceNameChange.TotalSeconds > 1.0)
				{
					TimeSpan coolDownRemaining = Client.NameChangeCoolDown - timeSinceNameChange;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 1);
					defaultInterpolatedStringHandler.AppendLiteral("ServerMessage.NameChangeFailedCooldownActive~[seconds]=");
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)coolDownRemaining.TotalSeconds);
					this.SendDirectChatMessage(defaultInterpolatedStringHandler.ToStringAndClear(), c, ChatMessageType.Server);
					ushort lastClientListUpdateID = base.LastClientListUpdateID;
					base.LastClientListUpdateID = lastClientListUpdateID + 1;
					Client c2 = c;
					c2.NameId += 1;
				}
				c.RejectedName = newName;
				return false;
			}
			bool? result = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventTryClientChangeName>(delegate(IEventTryClientChangeName x)
			{
				bool? flag = x.OnTryClienChangeName(c, newName, newJob, newTeam);
				result = ((flag != null) ? flag : result);
			});
			if (result != null)
			{
				ushort lastClientListUpdateID = base.LastClientListUpdateID;
				base.LastClientListUpdateID = lastClientListUpdateID + 1;
				return result.Value;
			}
			return this.TryChangeClientName(c, newName, true);
		}

		// Token: 0x06003426 RID: 13350 RVA: 0x00164F08 File Offset: 0x00163108
		public bool TryChangeClientName(Client c, string newName, bool clientRenamingSelf = false)
		{
			newName = Client.SanitizeName(newName, 32);
			ushort lastClientListUpdateID;
			if (newName != c.Name && !string.IsNullOrEmpty(newName) && this.IsNameValid(c, newName, clientRenamingSelf))
			{
				c.LastNameChangeTime = DateTime.Now;
				string oldName = c.Name;
				c.Name = newName;
				c.RejectedName = string.Empty;
				this.SendChatMessage("ServerMessage.NameChangeSuccessful~[oldname]=" + oldName + "~[newname]=" + newName, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
				lastClientListUpdateID = base.LastClientListUpdateID;
				base.LastClientListUpdateID = lastClientListUpdateID + 1;
				return true;
			}
			lastClientListUpdateID = base.LastClientListUpdateID;
			base.LastClientListUpdateID = lastClientListUpdateID + 1;
			return false;
		}

		// Token: 0x06003427 RID: 13351 RVA: 0x00164FAC File Offset: 0x001631AC
		public bool IsNameValid(Client c, string newName, bool clientRenamingSelf = false)
		{
			if (c.Connection != this.OwnerConnection)
			{
				if (!Client.IsValidName(newName, base.ServerSettings))
				{
					this.SendDirectChatMessage("ServerMessage.NameChangeFailedSymbols~[newname]=" + newName, c, ChatMessageType.ServerMessageBox);
					return false;
				}
				if (Homoglyphs.Compare(newName.ToLower(), this.ServerName.ToLower()))
				{
					this.SendDirectChatMessage("ServerMessage.NameChangeFailedServerTooSimilar~[newname]=" + newName, c, ChatMessageType.ServerMessageBox);
					return false;
				}
				if (c.KickVoteCount > 0)
				{
					this.SendDirectChatMessage("ServerMessage.NameChangeFailedVoteKick~[newname]=" + newName, c, ChatMessageType.ServerMessageBox);
					return false;
				}
			}
			Client nameTakenByClient = this.ConnectedClients.Find((Client c2) => (!clientRenamingSelf || c != c2) && Homoglyphs.Compare(c2.Name.ToLower(), newName.ToLower()));
			if (nameTakenByClient != null)
			{
				this.SendDirectChatMessage("ServerMessage.NameChangeFailedClientTooSimilar~[newname]=" + newName + "~[takenname]=" + nameTakenByClient.Name, c, ChatMessageType.ServerMessageBox);
				return false;
			}
			GameSession gameSession = GameMain.GameSession;
			string text;
			if (gameSession == null)
			{
				text = null;
			}
			else
			{
				CrewManager crewManager = gameSession.CrewManager;
				if (crewManager == null)
				{
					text = null;
				}
				else
				{
					CharacterInfo characterInfo = crewManager.GetCharacterInfos(true).FirstOrDefault(delegate(CharacterInfo ci)
					{
						if (clientRenamingSelf)
						{
							int id = (int)ci.ID;
							Character character = c.Character;
							ushort? num = (character != null) ? new ushort?(character.ID) : null;
							int? num2 = (num != null) ? new int?((int)num.GetValueOrDefault()) : null;
							if (id == num2.GetValueOrDefault() & num2 != null)
							{
								return false;
							}
						}
						return Homoglyphs.Compare(ci.Name.ToLower(), newName.ToLower());
					});
					text = ((characterInfo != null) ? characterInfo.Name : null);
				}
			}
			string existingTooSimilarName = text;
			if (!existingTooSimilarName.IsNullOrEmpty())
			{
				this.SendDirectChatMessage("ServerMessage.NameChangeFailedTooSimilar~[newname]=" + newName + "~[takenname]=" + existingTooSimilarName, c, ChatMessageType.ServerMessageBox);
				return false;
			}
			return true;
		}

		// Token: 0x06003428 RID: 13352 RVA: 0x0016513C File Offset: 0x0016333C
		public override void KickPlayer(string playerName, string reason)
		{
			Client client = this.connectedClients.Find((Client c) => c.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase) || (c.Character != null && c.Character.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)));
			this.KickClient(client, reason, false);
		}

		// Token: 0x06003429 RID: 13353 RVA: 0x00165178 File Offset: 0x00163378
		public void KickClient(NetworkConnection conn, string reason)
		{
			if (conn == this.OwnerConnection)
			{
				return;
			}
			Client client = this.connectedClients.Find((Client c) => c.Connection == conn);
			this.KickClient(client, reason, false);
		}

		// Token: 0x0600342A RID: 13354 RVA: 0x001651C4 File Offset: 0x001633C4
		public void KickClient(Client client, string reason, bool resetKarma = false)
		{
			if (client == null || client.Connection == this.OwnerConnection)
			{
				return;
			}
			if (resetKarma)
			{
				PreviousPlayer previousPlayer = this.previousPlayers.Find((PreviousPlayer p) => p.MatchesClient(client));
				if (previousPlayer != null)
				{
					previousPlayer.Karma = Math.Max(previousPlayer.Karma, 50f);
				}
				client.Karma = Math.Max(client.Karma, 50f);
			}
			this.DisconnectClient(client, PeerDisconnectPacket.Kicked(reason));
		}

		// Token: 0x0600342B RID: 13355 RVA: 0x00165260 File Offset: 0x00163460
		public override void BanPlayer(string playerName, string reason, TimeSpan? duration = null)
		{
			Client client = this.connectedClients.Find((Client c) => c.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase) || (c.Character != null && c.Character.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)));
			if (client == null)
			{
				DebugConsole.ThrowError("Client \"" + playerName + "\" not found.", null, null, false, false);
				return;
			}
			this.BanClient(client, reason, duration);
		}

		// Token: 0x0600342C RID: 13356 RVA: 0x001652C0 File Offset: 0x001634C0
		public void BanClient(Client client, string reason, TimeSpan? duration = null)
		{
			if (client == null || client.Connection == this.OwnerConnection)
			{
				return;
			}
			PreviousPlayer previousPlayer = this.previousPlayers.Find((PreviousPlayer p) => p.MatchesClient(client));
			if (previousPlayer != null)
			{
				previousPlayer.Karma = Math.Max(previousPlayer.Karma, 50f);
			}
			client.Karma = Math.Max(client.Karma, 50f);
			this.DisconnectClient(client, PeerDisconnectPacket.Banned(reason));
			AccountId accountId;
			if (client.AccountInfo.AccountId.TryUnwrap(out accountId))
			{
				base.ServerSettings.BanList.BanPlayer(client.Name, accountId, reason, duration);
			}
			else
			{
				base.ServerSettings.BanList.BanPlayer(client.Name, client.Connection.Endpoint, reason, duration);
			}
			foreach (AccountId relatedId in client.AccountInfo.OtherMatchingIds)
			{
				base.ServerSettings.BanList.BanPlayer(client.Name, relatedId, reason, duration);
			}
		}

		// Token: 0x0600342D RID: 13357 RVA: 0x00165414 File Offset: 0x00163614
		public void BanPreviousPlayer(PreviousPlayer previousPlayer, string reason, TimeSpan? duration = null)
		{
			if (previousPlayer == null)
			{
				return;
			}
			previousPlayer.Karma = Math.Max(previousPlayer.Karma, 50f);
			base.ServerSettings.BanList.BanPlayer(previousPlayer.Name, previousPlayer.Address, reason, duration);
			AccountId accountId;
			if (previousPlayer.AccountInfo.AccountId.TryUnwrap(out accountId))
			{
				base.ServerSettings.BanList.BanPlayer(previousPlayer.Name, accountId, reason, duration);
			}
			foreach (AccountId relatedId in previousPlayer.AccountInfo.OtherMatchingIds)
			{
				base.ServerSettings.BanList.BanPlayer(previousPlayer.Name, relatedId, reason, duration);
			}
			string msg = "ServerMessage.BannedFromServer~[client]=" + previousPlayer.Name;
			if (!string.IsNullOrWhiteSpace(reason))
			{
				msg = msg + "/ /ServerMessage.Reason/: /" + reason;
			}
			this.SendChatMessage(msg, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.Banned, ChatMode.None);
		}

		// Token: 0x0600342E RID: 13358 RVA: 0x0016550C File Offset: 0x0016370C
		public override void UnbanPlayer(string playerName)
		{
			BannedPlayer bannedPlayer = base.ServerSettings.BanList.BannedPlayers.FirstOrDefault((BannedPlayer bp) => bp.Name == playerName);
			if (bannedPlayer == null)
			{
				return;
			}
			base.ServerSettings.BanList.UnbanPlayer(bannedPlayer.AddressOrAccountId);
		}

		// Token: 0x0600342F RID: 13359 RVA: 0x00165562 File Offset: 0x00163762
		public override void UnbanPlayer(Endpoint endpoint)
		{
			base.ServerSettings.BanList.UnbanPlayer(endpoint);
		}

		// Token: 0x06003430 RID: 13360 RVA: 0x00165578 File Offset: 0x00163778
		public void DisconnectClient(NetworkConnection senderConnection, PeerDisconnectPacket peerDisconnectPacket)
		{
			Client client = this.connectedClients.Find((Client x) => x.Connection == senderConnection);
			if (client == null)
			{
				return;
			}
			this.DisconnectClient(client, peerDisconnectPacket);
		}

		// Token: 0x06003431 RID: 13361 RVA: 0x001655B8 File Offset: 0x001637B8
		public void DisconnectClient(Client client, PeerDisconnectPacket peerDisconnectPacket)
		{
			if (client == null)
			{
				return;
			}
			if (client.Character != null)
			{
				client.Character.ClientDisconnected = true;
				client.Character.ClearInputs();
			}
			client.Character = null;
			client.HasSpawned = false;
			client.WaitForNextRoundRespawn = null;
			client.InGame = false;
			PreviousPlayer previousPlayer = this.previousPlayers.Find((PreviousPlayer p) => p.MatchesClient(client));
			if (previousPlayer == null)
			{
				previousPlayer = new PreviousPlayer(client);
				this.previousPlayers.Add(previousPlayer);
			}
			if (peerDisconnectPacket.ShouldAttemptReconnect)
			{
				List<Client> obj = this.clientsAttemptingToReconnectSoon;
				lock (obj)
				{
					client.DeleteDisconnectedTimer = base.ServerSettings.KillDisconnectedTime;
					this.clientsAttemptingToReconnectSoon.Add(client);
				}
			}
			previousPlayer.Name = client.Name;
			previousPlayer.Karma = client.Karma;
			previousPlayer.KarmaKickCount = client.KarmaKickCount;
			previousPlayer.KickVoters.Clear();
			foreach (Client c in this.connectedClients)
			{
				if (client.HasKickVoteFrom(c))
				{
					previousPlayer.KickVoters.Add(c);
				}
			}
			client.Dispose();
			this.connectedClients.Remove(client);
			this.serverPeer.Disconnect(client.Connection, peerDisconnectPacket);
			base.KarmaManager.OnClientDisconnected(client);
			if (!base.GameStarted)
			{
				this.RefreshPvpTeamAssignments(false, false);
			}
			this.UpdateVoteStatus(true);
			this.SendChatMessage(peerDisconnectPacket.ChatMessage(client.Name).Value, new ChatMessageType?(ChatMessageType.Server), null, null, peerDisconnectPacket.ConnectionChangeType, ChatMode.None);
			this.UpdateCrewFrame();
			base.ServerSettings.ServerDetailsChanged = true;
			this.refreshMasterTimer = DateTime.Now;
		}

		// Token: 0x06003432 RID: 13362 RVA: 0x0016580C File Offset: 0x00163A0C
		private void UpdateCrewFrame()
		{
			foreach (Client c in this.connectedClients)
			{
				if (c.Character != null)
				{
					bool inGame = c.InGame;
				}
			}
		}

		// Token: 0x06003433 RID: 13363 RVA: 0x00165868 File Offset: 0x00163A68
		public void SendDirectChatMessage(string txt, Client recipient, ChatMessageType messageType = ChatMessageType.Server)
		{
			ChatMessage msg = ChatMessage.Create("", txt, messageType, null, null, PlayerConnectionChangeType.None, null);
			this.SendDirectChatMessage(msg, recipient);
		}

		// Token: 0x06003434 RID: 13364 RVA: 0x00165898 File Offset: 0x00163A98
		public void SendConsoleMessage(string txt, Client recipient, Color? color = null)
		{
			ChatMessage msg = ChatMessage.Create("", txt, ChatMessageType.Console, null, null, PlayerConnectionChangeType.None, color);
			this.SendDirectChatMessage(msg, recipient);
		}

		// Token: 0x06003435 RID: 13365 RVA: 0x001658C0 File Offset: 0x00163AC0
		public void SendDirectChatMessage(ChatMessage msg, Client recipient)
		{
			if (recipient == null)
			{
				string errorMsg = "Attempted to send a chat message to a null client.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("GameServer.SendDirectChatMessage:ClientNull", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			msg.NetStateID = ((recipient.ChatMsgQueue.Count > 0) ? (recipient.ChatMsgQueue.Last<ChatMessage>().NetStateID + 1) : (recipient.LastRecvChatMsgID + 1));
			recipient.ChatMsgQueue.Add(msg);
			recipient.LastChatMsgQueueID = msg.NetStateID;
		}

		// Token: 0x06003436 RID: 13366 RVA: 0x00165948 File Offset: 0x00163B48
		public void SendChatMessage(string message, ChatMessageType? type = null, Client senderClient = null, Character senderCharacter = null, PlayerConnectionChangeType changeType = PlayerConnectionChangeType.None, ChatMode chatMode = ChatMode.None)
		{
			string senderName = "";
			Client targetClient = null;
			if (type == null)
			{
				string tempStr;
				string command = ChatMessage.GetChatMessageCommand(message, out tempStr);
				string a = command.ToLowerInvariant();
				if (!(a == "r") && !(a == "radio"))
				{
					if (!(a == "d") && !(a == "dead"))
					{
						if (command != "")
						{
							if (command.ToLower() == this.ServerName.ToLower())
							{
								if (this.OwnerConnection != null)
								{
									targetClient = this.connectedClients.Find((Client c) => c.Connection == this.OwnerConnection);
								}
							}
							else
							{
								targetClient = this.connectedClients.Find(delegate(Client c)
								{
									if (!(command.ToLower() == c.Name.ToLower()))
									{
										string a2 = command.ToLower();
										Character character = c.Character;
										string b;
										if (character == null)
										{
											b = null;
										}
										else
										{
											string name = character.Name;
											b = ((name != null) ? name.ToLower() : null);
										}
										return a2 == b;
									}
									return true;
								});
								if (targetClient == null)
								{
									if (senderClient != null)
									{
										ChatMessage chatMsg = ChatMessage.Create("", "ServerMessage.PlayerNotFound~[player]=" + command, ChatMessageType.Error, null, null, PlayerConnectionChangeType.None, null);
										this.SendDirectChatMessage(chatMsg, senderClient);
										return;
									}
									base.AddChatMessage("ServerMessage.PlayerNotFound~[player]=" + command, ChatMessageType.Error, "", null, null, PlayerConnectionChangeType.None, null);
									return;
								}
							}
							type = new ChatMessageType?(ChatMessageType.Private);
						}
						else if (chatMode == ChatMode.Radio)
						{
							type = new ChatMessageType?(ChatMessageType.Radio);
						}
						else
						{
							type = new ChatMessageType?(ChatMessageType.Default);
						}
					}
					else
					{
						type = new ChatMessageType?(ChatMessageType.Dead);
					}
				}
				else
				{
					type = new ChatMessageType?(ChatMessageType.Radio);
				}
				message = tempStr;
			}
			if (base.GameStarted)
			{
				if (senderClient == null)
				{
					if (senderCharacter == null)
					{
						senderName = this.ServerName;
					}
					else
					{
						senderName = senderCharacter.DisplayName;
					}
				}
				else
				{
					senderCharacter = senderClient.Character;
					senderName = ((senderCharacter == null) ? senderClient.Name : senderCharacter.DisplayName);
					if (type.GetValueOrDefault() == ChatMessageType.Private)
					{
						if ((senderCharacter != null && !senderCharacter.IsDead) || (targetClient.Character != null && !targetClient.Character.IsDead))
						{
							this.SendDirectChatMessage(ChatMessage.Create("", "ServerMessage.PrivateMessagesNotAllowed", ChatMessageType.Error, null, null, PlayerConnectionChangeType.None, null), senderClient);
							return;
						}
					}
					else if (senderCharacter == null || senderCharacter.IsDead || senderCharacter.SpeechImpediment >= 100f)
					{
						type = new ChatMessageType?(ChatMessageType.Dead);
					}
				}
			}
			else if (senderClient == null)
			{
				if (senderCharacter != null)
				{
					return;
				}
				senderName = this.ServerName;
			}
			else
			{
				if (type.GetValueOrDefault() != ChatMessageType.Private && type.GetValueOrDefault() != ChatMessageType.Team)
				{
					type = new ChatMessageType?(ChatMessageType.Default);
				}
				senderName = senderClient.Name;
			}
			WifiComponent senderRadio = null;
			if (type != null)
			{
				ChatMessageType valueOrDefault = type.GetValueOrDefault();
				if (valueOrDefault != ChatMessageType.Dead)
				{
					if (valueOrDefault == ChatMessageType.Radio || valueOrDefault == ChatMessageType.Order)
					{
						if (senderCharacter == null)
						{
							return;
						}
						if (!ChatMessage.CanUseRadio(senderCharacter, out senderRadio, false))
						{
							return;
						}
					}
				}
				else if (senderClient != null && senderCharacter != null && !senderCharacter.IsDead && senderCharacter.SpeechImpediment < 100f)
				{
					return;
				}
			}
			if (type.GetValueOrDefault() == ChatMessageType.Server || type.GetValueOrDefault() == ChatMessageType.Error)
			{
				senderName = null;
				senderCharacter = null;
			}
			else if (type.GetValueOrDefault() == ChatMessageType.Radio && !LuaCsSetup.Instance.Game.overrideSignalRadio)
			{
				Signal s = new Signal(message, 0, senderCharacter, senderRadio.Item, 0f, 1f);
				senderRadio.TransmitSignal(s, true);
			}
			ChatMessage hookChatMsg = ChatMessage.Create(senderName, message, type.Value, senderCharacter, senderClient, changeType, null);
			bool shouldSkip = false;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventModifyChatMessage>(delegate(IEventModifyChatMessage sub)
			{
				bool? flag = sub.OnModifyMessagePredicate(hookChatMsg, senderRadio);
				if (flag != null && flag.GetValueOrDefault())
				{
					shouldSkip = true;
				}
			});
			if (shouldSkip)
			{
				return;
			}
			foreach (Client client in this.ConnectedClients)
			{
				string modifiedMessage = message;
				if (type != null)
				{
					ChatMessageType valueOrDefault2 = type.GetValueOrDefault();
					switch (valueOrDefault2)
					{
					case ChatMessageType.Default:
					case ChatMessageType.Radio:
					case ChatMessageType.Order:
						if (senderCharacter != null && client.Character != null && !client.Character.IsDead)
						{
							if (senderCharacter != client.Character)
							{
								modifiedMessage = ChatMessage.ApplyDistanceEffect(message, type.Value, senderCharacter, client.Character);
							}
							if (string.IsNullOrWhiteSpace(modifiedMessage))
							{
								continue;
							}
						}
						break;
					case ChatMessageType.Error:
					case ChatMessageType.Server:
					case ChatMessageType.Console:
					case ChatMessageType.MessageBox:
						break;
					case ChatMessageType.Dead:
						if (client != senderClient && client.Character != null && !client.Character.IsDead)
						{
							continue;
						}
						break;
					case ChatMessageType.Private:
						if (client != targetClient && client != senderClient)
						{
							continue;
						}
						break;
					default:
						if (valueOrDefault2 == ChatMessageType.Team)
						{
							if (client.TeamID == CharacterTeamType.None || client.TeamID != senderClient.TeamID)
							{
								continue;
							}
						}
						break;
					}
				}
				ChatMessage chatMsg2 = ChatMessage.Create(senderName, modifiedMessage, type.Value, senderCharacter, senderClient, changeType, null);
				this.SendDirectChatMessage(chatMsg2, client);
			}
			if (type.Value != ChatMessageType.MessageBox)
			{
				string myReceivedMessage = (type.GetValueOrDefault() == ChatMessageType.Server || type.GetValueOrDefault() == ChatMessageType.Error) ? TextManager.GetServerMessage(message).Value : message;
				if (!string.IsNullOrWhiteSpace(myReceivedMessage))
				{
					base.AddChatMessage(myReceivedMessage, type.Value, senderName, senderClient, senderCharacter, PlayerConnectionChangeType.None, null);
				}
			}
		}

		// Token: 0x06003437 RID: 13367 RVA: 0x00165E94 File Offset: 0x00164094
		public void SendOrderChatMessage(OrderChatMessage message)
		{
			if (message.SenderCharacter == null || message.SenderCharacter.SpeechImpediment >= 100f)
			{
				return;
			}
			foreach (Client client in this.ConnectedClients)
			{
				if (message.SenderCharacter == null || client.Character == null || client.Character.IsDead || client.Character.CanHearCharacter(message.SenderCharacter))
				{
					this.SendDirectChatMessage(new OrderChatMessage(message.Order, message.Text, message.TargetCharacter, message.Sender, message.IsNewOrder), client);
				}
			}
			if (!string.IsNullOrWhiteSpace(message.Text))
			{
				this.AddChatMessage(new OrderChatMessage(message.Order, message.Text, message.TargetCharacter, message.Sender, message.IsNewOrder));
				WifiComponent senderRadio;
				if (ChatMessage.CanUseRadio(message.SenderCharacter, out senderRadio, false))
				{
					Signal s = new Signal(message.Text, 0, message.SenderCharacter, senderRadio.Item, 0f, 1f);
					senderRadio.TransmitSignal(s, true);
				}
			}
		}

		// Token: 0x06003438 RID: 13368 RVA: 0x00165FC4 File Offset: 0x001641C4
		private void FileTransferChanged(FileSender.FileTransferOut transfer)
		{
			Client recipient = this.connectedClients.Find((Client c) => c.Connection == transfer.Connection);
			if (transfer.FileType == FileTransferType.CampaignSave && (transfer.Status == FileTransferStatus.Sending || transfer.Status == FileTransferStatus.Finished))
			{
				ValueTuple<ushort, float> lastCampaignSaveSendTime = recipient.LastCampaignSaveSendTime;
				ushort item = lastCampaignSaveSendTime.Item1;
				float item2 = lastCampaignSaveSendTime.Item2;
				if (item != 0 || item2 != 0f)
				{
					recipient.LastCampaignSaveSendTime.Item2 = (float)NetTime.Now;
				}
			}
		}

		// Token: 0x06003439 RID: 13369 RVA: 0x00166050 File Offset: 0x00164250
		public void SendCancelTransferMsg(FileSender.FileTransferOut transfer)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(9);
			msg.WriteByte(4);
			msg.WriteByte((byte)transfer.ID);
			this.serverPeer.Send(msg, transfer.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x0600343A RID: 13370 RVA: 0x00166094 File Offset: 0x00164294
		public void UpdateVoteStatus(bool checkActiveVote = true)
		{
			if (this.connectedClients.Count == 0)
			{
				return;
			}
			if (checkActiveVote && Voting.ActiveVote != null)
			{
				IEnumerable<Client> inGameClients = from c in GameMain.Server.ConnectedClients
				where c.InGame
				select c;
				if (inGameClients.Count<Client>() == 1 && inGameClients.First<Client>() == Voting.ActiveVote.VoteStarter)
				{
					Voting.ActiveVote.Finish(this.Voting, true);
				}
				else if (inGameClients.Any<Client>())
				{
					IEnumerable<Client> eligibleClients = from c in inGameClients
					where c != Voting.ActiveVote.VoteStarter
					select c;
					int yes = eligibleClients.Count((Client c) => c.GetVote<int>(Voting.ActiveVote.VoteType) == 2);
					int no = eligibleClients.Count((Client c) => c.GetVote<int>(Voting.ActiveVote.VoteType) == 1);
					int max = eligibleClients.Count<Client>();
					if ((float)no / (float)max > 1f - base.ServerSettings.VoteRequiredRatio)
					{
						Voting.ActiveVote.Finish(this.Voting, false);
					}
					else if ((float)yes / (float)max >= base.ServerSettings.VoteRequiredRatio)
					{
						Voting.ActiveVote.Finish(this.Voting, true);
					}
				}
			}
			Client.UpdateKickVotes(this.connectedClients);
			IEnumerable<Client> kickVoteEligibleClients = from c in this.connectedClients
			where (DateTime.Now - c.JoinTime).TotalSeconds > (double)this.ServerSettings.DisallowKickVoteTime
			select c;
			float minimumKickVotes = Math.Max(2f, (float)kickVoteEligibleClients.Count<Client>() * base.ServerSettings.KickVoteRequiredRatio);
			List<Client> clientsToKick = this.connectedClients.FindAll((Client c) => c.Connection != this.OwnerConnection && !c.HasPermission(ClientPermissions.Kick) && !c.HasPermission(ClientPermissions.Ban) && !c.HasPermission(ClientPermissions.Unban) && (float)c.KickVoteCount >= minimumKickVotes);
			using (List<Client>.Enumerator enumerator = clientsToKick.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Client c = enumerator.Current;
					c.ResetVotes(true);
					(from p in this.previousPlayers
					where p.MatchesClient(c)
					select p).ForEach(delegate(PreviousPlayer p)
					{
						p.KickVoters.Clear();
					});
					this.BanClient(c, "ServerMessage.KickedByVoteAutoBan", new TimeSpan?(TimeSpan.FromSeconds((double)base.ServerSettings.AutoBanTime)));
				}
			}
			this.SendVoteStatus(this.connectedClients);
			IEnumerable<Client> endVoteEligibleClients = from c in this.connectedClients
			where this.Voting.CanVoteToEndRound(c)
			select c;
			int endVoteCount = endVoteEligibleClients.Count((Client c) => c.GetVote<bool>(VoteType.EndRound));
			int endVoteMax = endVoteEligibleClients.Count<Client>();
			if (base.ServerSettings.AllowEndVoting && endVoteMax > 0 && (float)endVoteCount / (float)endVoteMax >= base.ServerSettings.EndVoteRequiredRatio)
			{
				GameServer.Log(string.Concat(new string[]
				{
					"Ending round by votes (",
					endVoteCount.ToString(),
					"/",
					(endVoteMax - endVoteCount).ToString(),
					")"
				}), ServerLog.MessageType.ServerMessage);
				this.EndGame(CampaignMode.TransitionType.None, false, null);
			}
		}

		// Token: 0x0600343B RID: 13371 RVA: 0x001663F4 File Offset: 0x001645F4
		public void SendVoteStatus(List<Client> recipients)
		{
			if (!recipients.Any<Client>())
			{
				return;
			}
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(2);
			using (SegmentTableWriter<ServerNetSegment> segmentTable = SegmentTableWriter<ServerNetSegment>.StartWriting(msg))
			{
				segmentTable.StartNewSegment(ServerNetSegment.Vote);
				this.Voting.ServerWrite(msg);
			}
			foreach (Client c in recipients)
			{
				this.serverPeer.Send(msg, c.Connection, DeliveryMethod.Reliable, true);
			}
		}

		// Token: 0x0600343C RID: 13372 RVA: 0x00166498 File Offset: 0x00164698
		public bool TrySwitchSubmarine()
		{
			Voting.SubmarineVote subVote = Voting.ActiveVote as Voting.SubmarineVote;
			if (subVote == null)
			{
				return false;
			}
			SubmarineInfo targetSubmarine = subVote.Sub;
			VoteType voteType = Voting.ActiveVote.VoteType;
			Client starter = Voting.ActiveVote.VoteStarter;
			bool purchaseFailed = false;
			if (voteType - VoteType.PurchaseAndSwitchSub > 1)
			{
				if (voteType != VoteType.SwitchSub)
				{
					return false;
				}
			}
			else
			{
				purchaseFailed = !GameMain.GameSession.TryPurchaseSubmarine(targetSubmarine, starter);
			}
			if (voteType != VoteType.PurchaseSub && !purchaseFailed)
			{
				GameMain.GameSession.SwitchSubmarine(targetSubmarine, subVote.TransferItems, starter);
			}
			this.Voting.StopSubmarineVote(!purchaseFailed);
			return !purchaseFailed;
		}

		// Token: 0x0600343D RID: 13373 RVA: 0x00166528 File Offset: 0x00164728
		public void UpdateClientPermissions(Client client)
		{
			AccountId accountId;
			if (client.AccountId.TryUnwrap(out accountId))
			{
				base.ServerSettings.ClientPermissions.RemoveAll((ServerSettings.SavedClientPermission scp) => scp.AddressOrAccountId == accountId);
				if (client.Permissions != ClientPermissions.None)
				{
					base.ServerSettings.ClientPermissions.Add(new ServerSettings.SavedClientPermission(client.Name, accountId, client.Permissions, client.PermittedConsoleCommands));
				}
			}
			else
			{
				base.ServerSettings.ClientPermissions.RemoveAll((ServerSettings.SavedClientPermission scp) => client.Connection.Endpoint.Address == scp.AddressOrAccountId);
				if (client.Permissions != ClientPermissions.None)
				{
					base.ServerSettings.ClientPermissions.Add(new ServerSettings.SavedClientPermission(client.Name, client.Connection.Endpoint.Address, client.Permissions, client.PermittedConsoleCommands));
				}
			}
			foreach (Client recipient in this.connectedClients)
			{
				CoroutineManager.StartCoroutine(this.SendClientPermissionsAfterClientListSynced(recipient, client), "");
			}
			base.ServerSettings.SaveClientPermissions();
		}

		// Token: 0x0600343E RID: 13374 RVA: 0x001666AC File Offset: 0x001648AC
		private IEnumerable<CoroutineStatus> SendClientPermissionsAfterClientListSynced(Client recipient, Client client)
		{
			GameServer.<SendClientPermissionsAfterClientListSynced>d__155 <SendClientPermissionsAfterClientListSynced>d__ = new GameServer.<SendClientPermissionsAfterClientListSynced>d__155(-2);
			<SendClientPermissionsAfterClientListSynced>d__.<>4__this = this;
			<SendClientPermissionsAfterClientListSynced>d__.<>3__recipient = recipient;
			<SendClientPermissionsAfterClientListSynced>d__.<>3__client = client;
			return <SendClientPermissionsAfterClientListSynced>d__;
		}

		// Token: 0x0600343F RID: 13375 RVA: 0x001666CC File Offset: 0x001648CC
		private void SendClientPermissions(Client recipient, Client client)
		{
			if (((recipient != null) ? recipient.Connection : null) == null)
			{
				return;
			}
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(4);
			client.WritePermissions(msg);
			this.serverPeer.Send(msg, recipient.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06003440 RID: 13376 RVA: 0x00166710 File Offset: 0x00164910
		public void GiveAchievement(Character character, Identifier achievementIdentifier)
		{
			foreach (Client client in this.connectedClients)
			{
				if (client.Character == character)
				{
					this.GiveAchievement(client, achievementIdentifier);
					break;
				}
			}
		}

		// Token: 0x06003441 RID: 13377 RVA: 0x00166770 File Offset: 0x00164970
		public void IncrementStat(Character character, AchievementStat stat, int amount)
		{
			foreach (Client client in this.connectedClients)
			{
				if (client.Character == character)
				{
					this.IncrementStat(client, stat, amount);
					break;
				}
			}
		}

		// Token: 0x06003442 RID: 13378 RVA: 0x001667D0 File Offset: 0x001649D0
		public void GiveAchievement(Client client, Identifier achievementIdentifier)
		{
			if (client.GivenAchievements.Contains(achievementIdentifier))
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Attempting to give the achievement ");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(achievementIdentifier);
			defaultInterpolatedStringHandler.AppendLiteral(" to ");
			defaultInterpolatedStringHandler.AppendFormatted(client.Name);
			defaultInterpolatedStringHandler.AppendLiteral("...");
			DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
			client.GivenAchievements.Add(achievementIdentifier);
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(5);
			msg.WriteIdentifier(achievementIdentifier);
			this.serverPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06003443 RID: 13379 RVA: 0x0016687C File Offset: 0x00164A7C
		public void UnlockRecipe(CharacterTeamType team, Identifier identifier)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(28);
			msg.WriteByte((byte)team);
			msg.WriteIdentifier(identifier);
			foreach (Client client in this.connectedClients)
			{
				this.serverPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
			}
		}

		// Token: 0x06003444 RID: 13380 RVA: 0x001668FC File Offset: 0x00164AFC
		public void IncrementStat(Client client, AchievementStat stat, int amount)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(6);
			INetSerializableStruct incrementedStat = new NetIncrementedStat(stat, (float)amount);
			incrementedStat.Write(msg);
			this.serverPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06003445 RID: 13381 RVA: 0x0016693F File Offset: 0x00164B3F
		public void SendTraitorMessage(WriteOnlyMessage msg, Client client)
		{
			if (client == null)
			{
				return;
			}
			this.serverPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06003446 RID: 13382 RVA: 0x0016695C File Offset: 0x00164B5C
		public void UpdateCheatsEnabled()
		{
			if (!this.connectedClients.Any<Client>())
			{
				return;
			}
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(7);
			msg.WriteBoolean(DebugConsole.CheatsEnabled);
			msg.WritePadBits();
			foreach (Client c in this.connectedClients)
			{
				this.serverPeer.Send(msg, c.Connection, DeliveryMethod.Reliable, true);
			}
		}

		// Token: 0x06003447 RID: 13383 RVA: 0x001669E8 File Offset: 0x00164BE8
		public void SetClientCharacter(Client client, Character newCharacter)
		{
			if (client == null)
			{
				return;
			}
			if (client.Character != null)
			{
				client.Character.SetOwnerClient(null);
			}
			if (newCharacter == null)
			{
				if (client.Character != null)
				{
					this.CreateEntityEvent(client.Character, new Character.ControlEventData(null));
					client.Character = null;
					return;
				}
			}
			else
			{
				newCharacter.ClientDisconnected = false;
				newCharacter.KillDisconnectedTimer = 0f;
				newCharacter.ResetNetState();
				if (client.Character != null)
				{
					newCharacter.LastNetworkUpdateID = client.Character.LastNetworkUpdateID;
				}
				CharacterInfo info = newCharacter.Info;
				if (info != null && info.Character == null)
				{
					newCharacter.Info.Character = newCharacter;
				}
				newCharacter.SetOwnerClient(client);
				newCharacter.Enabled = true;
				newCharacter.AnimController.Frozen = false;
				client.Character = newCharacter;
				client.CharacterInfo = newCharacter.Info;
				this.CreateEntityEvent(newCharacter, new Character.ControlEventData(client));
			}
		}

		// Token: 0x06003448 RID: 13384 RVA: 0x00166AC8 File Offset: 0x00164CC8
		private void UpdateCharacterInfo(IReadMessage message, Client sender)
		{
			GameServer.<>c__DisplayClass166_0 CS$<>8__locals1;
			CS$<>8__locals1.sender = sender;
			bool spectateOnly = message.ReadBoolean();
			bool characterDiscarded = message.ReadBoolean();
			bool readInfo = message.ReadBoolean();
			message.ReadPadBits();
			CS$<>8__locals1.sender.SpectateOnly = (spectateOnly && (base.ServerSettings.AllowSpectating || CS$<>8__locals1.sender.Connection == this.OwnerConnection));
			if (!readInfo)
			{
				return;
			}
			NetCharacterInfo netInfo = INetSerializableStruct.Read<NetCharacterInfo>(message);
			if (CS$<>8__locals1.sender.SpectateOnly)
			{
				return;
			}
			if (this.charInfoRateLimiter.IsLimitReached(CS$<>8__locals1.sender))
			{
				return;
			}
			string newName = netInfo.NewName;
			if (string.IsNullOrEmpty(newName))
			{
				newName = CS$<>8__locals1.sender.Name;
			}
			else
			{
				newName = Client.SanitizeName(newName, 32);
				if (!this.IsNameValid(CS$<>8__locals1.sender, newName, true))
				{
					newName = CS$<>8__locals1.sender.Name;
				}
				else
				{
					CS$<>8__locals1.sender.PendingName = newName;
				}
			}
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
			if (mpCampaign != null)
			{
				if (characterDiscarded)
				{
					mpCampaign.DiscardClientCharacterData(CS$<>8__locals1.sender);
				}
				CharacterCampaignData existingCampaignData = mpCampaign.GetClientCharacterData(CS$<>8__locals1.sender);
				if (existingCampaignData != null)
				{
					DebugConsole.NewMessage("Client attempted to modify their CharacterInfo, but they already have an existing campaign character. Ignoring the modifications.", null, false);
					CS$<>8__locals1.sender.CharacterInfo = existingCampaignData.CharacterInfo;
					return;
				}
			}
			CS$<>8__locals1.sender.CharacterInfo = new CharacterInfo(CharacterPrefab.HumanSpeciesName, newName, "", null, 0, Rand.RandSync.Unsynced, default(Identifier));
			CS$<>8__locals1.sender.CharacterInfo.RecreateHead(netInfo.Tags.ToImmutableHashSet<Identifier>(), (int)netInfo.HairIndex, (int)netInfo.BeardIndex, (int)netInfo.MoustacheIndex, (int)netInfo.FaceAttachmentIndex);
			CS$<>8__locals1.sender.CharacterInfo.Head.SkinColor = GameServer.<UpdateCharacterInfo>g__validateColor|166_3(netInfo.SkinColor, "skin color", from kvp in CS$<>8__locals1.sender.CharacterInfo.SkinColors
			select kvp.Item1, ref CS$<>8__locals1);
			CS$<>8__locals1.sender.CharacterInfo.Head.HairColor = GameServer.<UpdateCharacterInfo>g__validateColor|166_3(netInfo.HairColor, "hair color", from kvp in CS$<>8__locals1.sender.CharacterInfo.HairColors
			select kvp.Item1, ref CS$<>8__locals1);
			CS$<>8__locals1.sender.CharacterInfo.Head.FacialHairColor = GameServer.<UpdateCharacterInfo>g__validateColor|166_3(netInfo.FacialHairColor, "facial hair color", from kvp in CS$<>8__locals1.sender.CharacterInfo.FacialHairColors
			select kvp.Item1, ref CS$<>8__locals1);
			if (netInfo.JobVariants.Length > 0)
			{
				List<JobVariant> variants = new List<JobVariant>();
				foreach (NetJobVariant jv in netInfo.JobVariants)
				{
					JobVariant variant = jv.ToJobVariant();
					if (variant != null)
					{
						variants.Add(variant);
					}
				}
				CS$<>8__locals1.sender.JobPreferences = variants;
			}
		}

		// Token: 0x06003449 RID: 13385 RVA: 0x00166DF8 File Offset: 0x00164FF8
		public void AssignJobs(List<Client> unassigned)
		{
			GameServer.<>c__DisplayClass168_0 CS$<>8__locals1 = new GameServer.<>c__DisplayClass168_0();
			CS$<>8__locals1.unassigned = unassigned;
			this.JobAssignmentDebugLog.Clear();
			CS$<>8__locals1.jobList = JobPrefab.Prefabs.ToList<JobPrefab>();
			CS$<>8__locals1.unassigned = new List<Client>(CS$<>8__locals1.unassigned);
			CS$<>8__locals1.unassigned = (from sp in CS$<>8__locals1.unassigned
			orderby Rand.Int(int.MaxValue, Rand.RandSync.Unsynced)
			select sp).ToList<Client>();
			CS$<>8__locals1.assignedClientCount = new Dictionary<JobPrefab, int>();
			foreach (JobPrefab jp2 in CS$<>8__locals1.jobList)
			{
				CS$<>8__locals1.assignedClientCount.Add(jp2, 0);
			}
			CharacterTeamType teamID = CharacterTeamType.None;
			if (CS$<>8__locals1.unassigned.Count > 0)
			{
				teamID = CS$<>8__locals1.unassigned[0].TeamID;
			}
			MultiPlayerCampaign multiplayerCampaign = GameMain.GameSession.GameMode as MultiPlayerCampaign;
			if (multiplayerCampaign != null)
			{
				Dictionary<Client, Job> campaignAssigned = multiplayerCampaign.GetAssignedJobs(this.connectedClients);
				CS$<>8__locals1.unassigned.RemoveAll((Client u) => campaignAssigned.ContainsKey(u));
				foreach (KeyValuePair<Client, Job> keyValuePair in campaignAssigned)
				{
					Client client5;
					Job job2;
					keyValuePair.Deconstruct(out client5, out job2);
					Client client = client5;
					Job job = job2;
					Dictionary<JobPrefab, int> assignedClientCount = CS$<>8__locals1.assignedClientCount;
					JobPrefab key = job.Prefab;
					int num = assignedClientCount[key];
					assignedClientCount[key] = num + 1;
					client.AssignedJob = new JobVariant(job.Prefab, job.Variant);
					List<string> jobAssignmentDebugLog = this.JobAssignmentDebugLog;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Client ");
					defaultInterpolatedStringHandler.AppendFormatted(client.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" has an existing campaign character, keeping the job ");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(job.Name);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					jobAssignmentDebugLog.Add(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			foreach (Client c in this.connectedClients)
			{
				if (c.TeamID == teamID && !CS$<>8__locals1.unassigned.Contains(c))
				{
					Character character = c.Character;
					bool flag;
					if (character == null)
					{
						flag = (null != null);
					}
					else
					{
						CharacterInfo info = character.Info;
						flag = (((info != null) ? info.Job : null) != null);
					}
					if (flag && !c.Character.IsDead)
					{
						Dictionary<JobPrefab, int> assignedClientCount2 = CS$<>8__locals1.assignedClientCount;
						JobPrefab key = c.Character.Info.Job.Prefab;
						int num = assignedClientCount2[key];
						assignedClientCount2[key] = num + 1;
					}
				}
			}
			for (int i = CS$<>8__locals1.unassigned.Count - 1; i >= 0; i--)
			{
				if (CS$<>8__locals1.unassigned[i].JobPreferences.Count != 0 && CS$<>8__locals1.unassigned[i].JobPreferences.Any<JobVariant>() && CS$<>8__locals1.unassigned[i].JobPreferences[0].Prefab.AllowAlways)
				{
					List<string> jobAssignmentDebugLog2 = this.JobAssignmentDebugLog;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(87, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Client ");
					defaultInterpolatedStringHandler2.AppendFormatted(CS$<>8__locals1.unassigned[i].Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" has ");
					defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(CS$<>8__locals1.unassigned[i].JobPreferences[0].Prefab.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" as their first preference, assigning it because the job is always allowed.");
					jobAssignmentDebugLog2.Add(defaultInterpolatedStringHandler2.ToStringAndClear());
					CS$<>8__locals1.unassigned[i].AssignedJob = CS$<>8__locals1.unassigned[i].JobPreferences[0];
					CS$<>8__locals1.unassigned.RemoveAt(i);
				}
			}
			CS$<>8__locals1.unassignedJobsFound = true;
			while (CS$<>8__locals1.unassignedJobsFound && CS$<>8__locals1.unassigned.Any<Client>())
			{
				CS$<>8__locals1.unassignedJobsFound = false;
				foreach (JobPrefab jobPrefab in CS$<>8__locals1.jobList)
				{
					if (CS$<>8__locals1.unassigned.Count == 0)
					{
						break;
					}
					if (jobPrefab.MinNumber >= 1 && CS$<>8__locals1.assignedClientCount[jobPrefab] < jobPrefab.MinNumber)
					{
						Client client2 = this.FindClientWithJobPreference(CS$<>8__locals1.unassigned, jobPrefab, false);
						if (client2 != null)
						{
							List<string> jobAssignmentDebugLog3 = this.JobAssignmentDebugLog;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(73, 4);
							defaultInterpolatedStringHandler3.AppendLiteral("At least ");
							defaultInterpolatedStringHandler3.AppendFormatted<int>(jobPrefab.MinNumber);
							defaultInterpolatedStringHandler3.AppendLiteral(" ");
							defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(jobPrefab.Name);
							defaultInterpolatedStringHandler3.AppendLiteral(" required. Assigning ");
							defaultInterpolatedStringHandler3.AppendFormatted(client2.Name);
							defaultInterpolatedStringHandler3.AppendLiteral(" as a ");
							defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(jobPrefab.Name);
							defaultInterpolatedStringHandler3.AppendLiteral(" (has the job in their preferences).");
							jobAssignmentDebugLog3.Add(defaultInterpolatedStringHandler3.ToStringAndClear());
							CS$<>8__locals1.<AssignJobs>g__AssignJob|1(client2, jobPrefab);
						}
					}
				}
				if (CS$<>8__locals1.unassigned.Any<Client>())
				{
					foreach (JobPrefab jobPrefab2 in CS$<>8__locals1.jobList)
					{
						if (CS$<>8__locals1.unassigned.Count == 0)
						{
							break;
						}
						if (jobPrefab2.MinNumber >= 1 && CS$<>8__locals1.assignedClientCount[jobPrefab2] < jobPrefab2.MinNumber)
						{
							Client client3 = this.FindClientWithJobPreference(CS$<>8__locals1.unassigned, jobPrefab2, true);
							List<string> jobAssignmentDebugLog4 = this.JobAssignmentDebugLog;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(124, 4);
							defaultInterpolatedStringHandler4.AppendLiteral("At least ");
							defaultInterpolatedStringHandler4.AppendFormatted<int>(jobPrefab2.MinNumber);
							defaultInterpolatedStringHandler4.AppendLiteral(" ");
							defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(jobPrefab2.Name);
							defaultInterpolatedStringHandler4.AppendLiteral(" required. ");
							defaultInterpolatedStringHandler4.AppendLiteral("A random client needs to be assigned because no one has the job in their preferences. Assigning ");
							defaultInterpolatedStringHandler4.AppendFormatted(client3.Name);
							defaultInterpolatedStringHandler4.AppendLiteral(" as a ");
							defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(jobPrefab2.Name);
							defaultInterpolatedStringHandler4.AppendLiteral(".");
							jobAssignmentDebugLog4.Add(defaultInterpolatedStringHandler4.ToStringAndClear());
							CS$<>8__locals1.<AssignJobs>g__AssignJob|1(client3, jobPrefab2);
						}
					}
				}
			}
			for (int preferenceIndex = 0; preferenceIndex < 3; preferenceIndex++)
			{
				for (int j = CS$<>8__locals1.unassigned.Count - 1; j >= 0; j--)
				{
					Client client4 = CS$<>8__locals1.unassigned[j];
					if (preferenceIndex < client4.JobPreferences.Count)
					{
						JobVariant preferredJob = client4.JobPreferences[preferenceIndex];
						JobPrefab jobPrefab3 = preferredJob.Prefab;
						if (CS$<>8__locals1.assignedClientCount[jobPrefab3] >= jobPrefab3.MaxNumber)
						{
							List<string> jobAssignmentDebugLog5 = this.JobAssignmentDebugLog;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(87, 3);
							defaultInterpolatedStringHandler5.AppendFormatted(client4.Name);
							defaultInterpolatedStringHandler5.AppendLiteral(" has ");
							defaultInterpolatedStringHandler5.AppendFormatted<LocalizedString>(jobPrefab3.Name);
							defaultInterpolatedStringHandler5.AppendLiteral(" as their ");
							defaultInterpolatedStringHandler5.AppendFormatted<int>(preferenceIndex + 1);
							defaultInterpolatedStringHandler5.AppendLiteral(". preference. Cannot assign, maximum number of the job has been reached.");
							jobAssignmentDebugLog5.Add(defaultInterpolatedStringHandler5.ToStringAndClear());
						}
						else if (client4.Karma < jobPrefab3.MinKarma)
						{
							List<string> jobAssignmentDebugLog6 = this.JobAssignmentDebugLog;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(64, 5);
							defaultInterpolatedStringHandler6.AppendFormatted(client4.Name);
							defaultInterpolatedStringHandler6.AppendLiteral(" has ");
							defaultInterpolatedStringHandler6.AppendFormatted<LocalizedString>(jobPrefab3.Name);
							defaultInterpolatedStringHandler6.AppendLiteral(" as their ");
							defaultInterpolatedStringHandler6.AppendFormatted<int>(preferenceIndex + 1);
							defaultInterpolatedStringHandler6.AppendLiteral(". preference. Cannot assign, karma too low (");
							defaultInterpolatedStringHandler6.AppendFormatted<float>(client4.Karma);
							defaultInterpolatedStringHandler6.AppendLiteral(" < ");
							defaultInterpolatedStringHandler6.AppendFormatted<float>(jobPrefab3.MinKarma);
							defaultInterpolatedStringHandler6.AppendLiteral(").");
							jobAssignmentDebugLog6.Add(defaultInterpolatedStringHandler6.ToStringAndClear());
						}
						else
						{
							List<string> jobAssignmentDebugLog7 = this.JobAssignmentDebugLog;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(46, 5);
							defaultInterpolatedStringHandler7.AppendFormatted(client4.Name);
							defaultInterpolatedStringHandler7.AppendLiteral(" has ");
							defaultInterpolatedStringHandler7.AppendFormatted<LocalizedString>(jobPrefab3.Name);
							defaultInterpolatedStringHandler7.AppendLiteral(" as their ");
							defaultInterpolatedStringHandler7.AppendFormatted<int>(preferenceIndex + 1);
							defaultInterpolatedStringHandler7.AppendLiteral(". preference. Assigning ");
							defaultInterpolatedStringHandler7.AppendFormatted(client4.Name);
							defaultInterpolatedStringHandler7.AppendLiteral(" as a ");
							defaultInterpolatedStringHandler7.AppendFormatted<LocalizedString>(jobPrefab3.Name);
							defaultInterpolatedStringHandler7.AppendLiteral(".");
							jobAssignmentDebugLog7.Add(defaultInterpolatedStringHandler7.ToStringAndClear());
							client4.AssignedJob = preferredJob;
							Dictionary<JobPrefab, int> assignedClientCount3 = CS$<>8__locals1.assignedClientCount;
							JobPrefab key = jobPrefab3;
							int num = assignedClientCount3[key];
							assignedClientCount3[key] = num + 1;
							CS$<>8__locals1.unassigned.RemoveAt(j);
						}
					}
				}
			}
			using (List<Client>.Enumerator enumerator6 = CS$<>8__locals1.unassigned.GetEnumerator())
			{
				while (enumerator6.MoveNext())
				{
					GameServer.<>c__DisplayClass168_3 CS$<>8__locals3 = new GameServer.<>c__DisplayClass168_3();
					CS$<>8__locals3.CS$<>8__locals1 = CS$<>8__locals1;
					CS$<>8__locals3.c = enumerator6.Current;
					List<JobPrefab> remainingJobs = CS$<>8__locals3.CS$<>8__locals1.jobList.FindAll((JobPrefab jp) => !jp.HiddenJob && CS$<>8__locals3.CS$<>8__locals1.assignedClientCount[jp] < jp.MaxNumber && CS$<>8__locals3.c.Karma >= jp.MinKarma);
					if (remainingJobs.Count == 0)
					{
						string errorMsg = "Failed to assign a suitable job for \"" + CS$<>8__locals3.c.Name + "\" (all jobs already have the maximum numbers of players). Assigning a random job...";
						DebugConsole.ThrowError(errorMsg, null, null, false, false);
						this.JobAssignmentDebugLog.Add(errorMsg);
						int jobIndex = Rand.Range(0, CS$<>8__locals3.CS$<>8__locals1.jobList.Count, Rand.RandSync.Unsynced);
						int skips = 0;
						int num;
						while (CS$<>8__locals3.c.Karma < CS$<>8__locals3.CS$<>8__locals1.jobList[jobIndex].MinKarma)
						{
							num = jobIndex;
							jobIndex = num + 1;
							skips++;
							if (jobIndex >= CS$<>8__locals3.CS$<>8__locals1.jobList.Count)
							{
								jobIndex -= CS$<>8__locals3.CS$<>8__locals1.jobList.Count;
							}
							if (skips >= CS$<>8__locals3.CS$<>8__locals1.jobList.Count)
							{
								break;
							}
						}
						CS$<>8__locals3.c.AssignedJob = (CS$<>8__locals3.c.JobPreferences.FirstOrDefault((JobVariant jp) => jp.Prefab == CS$<>8__locals3.CS$<>8__locals1.jobList[jobIndex]) ?? new JobVariant(CS$<>8__locals3.CS$<>8__locals1.jobList[jobIndex], 0));
						Dictionary<JobPrefab, int> assignedClientCount4 = CS$<>8__locals3.CS$<>8__locals1.assignedClientCount;
						JobPrefab key = CS$<>8__locals3.c.AssignedJob.Prefab;
						num = assignedClientCount4[key];
						assignedClientCount4[key] = num + 1;
					}
					else
					{
						JobVariant remainingJob = CS$<>8__locals3.c.JobPreferences.FirstOrDefault((JobVariant jp) => remainingJobs.Contains(jp.Prefab));
						if (remainingJob != null)
						{
							List<string> jobAssignmentDebugLog8 = this.JobAssignmentDebugLog;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(73, 5);
							defaultInterpolatedStringHandler8.AppendFormatted(CS$<>8__locals3.c.Name);
							defaultInterpolatedStringHandler8.AppendLiteral(" has ");
							defaultInterpolatedStringHandler8.AppendFormatted<LocalizedString>(remainingJob.Prefab.Name);
							defaultInterpolatedStringHandler8.AppendLiteral(" as their ");
							defaultInterpolatedStringHandler8.AppendFormatted<int>(CS$<>8__locals3.c.JobPreferences.IndexOf(remainingJob) + 1);
							defaultInterpolatedStringHandler8.AppendLiteral(". preference, and it is still available.");
							defaultInterpolatedStringHandler8.AppendLiteral(" Assigning ");
							defaultInterpolatedStringHandler8.AppendFormatted(CS$<>8__locals3.c.Name);
							defaultInterpolatedStringHandler8.AppendLiteral(" as a ");
							defaultInterpolatedStringHandler8.AppendFormatted<LocalizedString>(remainingJob.Prefab.Name);
							defaultInterpolatedStringHandler8.AppendLiteral(".");
							jobAssignmentDebugLog8.Add(defaultInterpolatedStringHandler8.ToStringAndClear());
							CS$<>8__locals3.c.AssignedJob = remainingJob;
							Dictionary<JobPrefab, int> assignedClientCount5 = CS$<>8__locals3.CS$<>8__locals1.assignedClientCount;
							JobPrefab key = remainingJob.Prefab;
							int num = assignedClientCount5[key];
							assignedClientCount5[key] = num + 1;
						}
						else
						{
							CS$<>8__locals3.c.AssignedJob = new JobVariant(remainingJobs[Rand.Range(0, remainingJobs.Count, Rand.RandSync.Unsynced)], 0);
							Dictionary<JobPrefab, int> assignedClientCount6 = CS$<>8__locals3.CS$<>8__locals1.assignedClientCount;
							JobPrefab key = CS$<>8__locals3.c.AssignedJob.Prefab;
							int num = assignedClientCount6[key];
							assignedClientCount6[key] = num + 1;
							List<string> jobAssignmentDebugLog9 = this.JobAssignmentDebugLog;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(67, 3);
							defaultInterpolatedStringHandler9.AppendLiteral("No suitable jobs available for ");
							defaultInterpolatedStringHandler9.AppendFormatted(CS$<>8__locals3.c.Name);
							defaultInterpolatedStringHandler9.AppendLiteral(" (karma ");
							defaultInterpolatedStringHandler9.AppendFormatted<float>(CS$<>8__locals3.c.Karma);
							defaultInterpolatedStringHandler9.AppendLiteral("). Assigning a random job: ");
							defaultInterpolatedStringHandler9.AppendFormatted<LocalizedString>(CS$<>8__locals3.c.AssignedJob.Prefab.Name);
							defaultInterpolatedStringHandler9.AppendLiteral(".");
							jobAssignmentDebugLog9.Add(defaultInterpolatedStringHandler9.ToStringAndClear());
						}
					}
				}
			}
		}

		// Token: 0x0600344A RID: 13386 RVA: 0x00167C84 File Offset: 0x00165E84
		public void AssignBotJobs(List<CharacterInfo> bots, CharacterTeamType teamID, bool isPvP)
		{
			GameServer.<>c__DisplayClass169_0 CS$<>8__locals1 = new GameServer.<>c__DisplayClass169_0();
			CS$<>8__locals1.isPvP = isPvP;
			List<JobPrefab> shuffledPrefabs = (from jp in JobPrefab.Prefabs
			where !jp.HiddenJob
			select jp).ToList<JobPrefab>();
			shuffledPrefabs.Shuffle(Rand.RandSync.Unsynced);
			CS$<>8__locals1.assignedPlayerCount = new Dictionary<JobPrefab, int>();
			foreach (JobPrefab jp2 in shuffledPrefabs)
			{
				if (!jp2.HiddenJob)
				{
					CS$<>8__locals1.assignedPlayerCount.Add(jp2, 0);
				}
			}
			foreach (Client c in this.connectedClients)
			{
				if (c.TeamID == teamID)
				{
					Character character = c.Character;
					bool flag;
					if (character == null)
					{
						flag = (null != null);
					}
					else
					{
						CharacterInfo info = character.Info;
						flag = (((info != null) ? info.Job : null) != null);
					}
					if (flag && !c.Character.IsDead)
					{
						Dictionary<JobPrefab, int> assignedPlayerCount = CS$<>8__locals1.assignedPlayerCount;
						JobPrefab key = c.Character.Info.Job.Prefab;
						int num = assignedPlayerCount[key];
						assignedPlayerCount[key] = num + 1;
					}
					else
					{
						CharacterInfo characterInfo = c.CharacterInfo;
						if (((characterInfo != null) ? characterInfo.Job : null) != null)
						{
							Dictionary<JobPrefab, int> assignedPlayerCount2 = CS$<>8__locals1.assignedPlayerCount;
							CharacterInfo characterInfo2 = c.CharacterInfo;
							JobPrefab key = (characterInfo2 != null) ? characterInfo2.Job.Prefab : null;
							int num = assignedPlayerCount2[key];
							assignedPlayerCount2[key] = num + 1;
						}
					}
				}
			}
			CS$<>8__locals1.unassignedBots = new List<CharacterInfo>(bots);
			while (CS$<>8__locals1.unassignedBots.Count > 0)
			{
				IEnumerable<JobPrefab> source = shuffledPrefabs;
				Func<JobPrefab, bool> predicate;
				if ((predicate = CS$<>8__locals1.<>9__2) == null)
				{
					predicate = (CS$<>8__locals1.<>9__2 = ((JobPrefab jp) => CS$<>8__locals1.assignedPlayerCount[jp] < jp.MinNumber));
				}
				IEnumerable<JobPrefab> jobsBelowMinNumber = source.Where(predicate);
				if (jobsBelowMinNumber.Any<JobPrefab>())
				{
					CS$<>8__locals1.<AssignBotJobs>g__AssignJob|1(CS$<>8__locals1.unassignedBots[0], jobsBelowMinNumber.GetRandomUnsynced<JobPrefab>());
				}
				else
				{
					IEnumerable<JobPrefab> source2 = shuffledPrefabs;
					Func<JobPrefab, bool> predicate2;
					if ((predicate2 = CS$<>8__locals1.<>9__3) == null)
					{
						predicate2 = (CS$<>8__locals1.<>9__3 = ((JobPrefab jp) => CS$<>8__locals1.assignedPlayerCount[jp] < jp.InitialCount));
					}
					IEnumerable<JobPrefab> jobsBelowInitialCount = source2.Where(predicate2);
					if (!jobsBelowInitialCount.Any<JobPrefab>())
					{
						break;
					}
					CS$<>8__locals1.<AssignBotJobs>g__AssignJob|1(CS$<>8__locals1.unassignedBots[0], jobsBelowInitialCount.GetRandomUnsynced<JobPrefab>());
				}
			}
			foreach (CharacterInfo c2 in CS$<>8__locals1.unassignedBots.ToList<CharacterInfo>())
			{
				IEnumerable<JobPrefab> source3 = shuffledPrefabs;
				Func<JobPrefab, bool> predicate3;
				if ((predicate3 = CS$<>8__locals1.<>9__4) == null)
				{
					predicate3 = (CS$<>8__locals1.<>9__4 = ((JobPrefab jp) => CS$<>8__locals1.assignedPlayerCount[jp] < jp.MaxNumber));
				}
				IEnumerable<JobPrefab> remainingJobs = source3.Where(predicate3);
				if (remainingJobs.None(null))
				{
					DebugConsole.ThrowError("Failed to assign a suitable job for bot \"" + c2.Name + "\" (all jobs already have the maximum numbers of players). Assigning a random job...", null, null, false, false);
					CS$<>8__locals1.<AssignBotJobs>g__AssignJob|1(c2, shuffledPrefabs.GetRandomUnsynced<JobPrefab>());
				}
				else
				{
					IEnumerable<JobPrefab> source4 = remainingJobs;
					Func<JobPrefab, float> weightSelector;
					if ((weightSelector = CS$<>8__locals1.<>9__5) == null)
					{
						weightSelector = (CS$<>8__locals1.<>9__5 = ((JobPrefab jp) => 1f / Math.Max((float)CS$<>8__locals1.assignedPlayerCount[jp], 0.01f)));
					}
					JobPrefab selectedJob = source4.GetRandomByWeight(weightSelector, Rand.RandSync.Unsynced);
					CS$<>8__locals1.<AssignBotJobs>g__AssignJob|1(c2, selectedJob);
				}
			}
		}

		// Token: 0x0600344B RID: 13387 RVA: 0x00167FC8 File Offset: 0x001661C8
		private Client FindClientWithJobPreference(List<Client> clients, JobPrefab job, bool forceAssign = false)
		{
			int bestPreference = int.MaxValue;
			Client preferredClient = null;
			Predicate<JobVariant> <>9__0;
			foreach (Client c in clients)
			{
				if (!base.ServerSettings.KarmaEnabled || c.Karma >= job.MinKarma)
				{
					List<JobVariant> jobPreferences = c.JobPreferences;
					List<JobVariant> jobPreferences2 = c.JobPreferences;
					Predicate<JobVariant> match;
					if ((match = <>9__0) == null)
					{
						match = (<>9__0 = ((JobVariant j) => j.Prefab == job));
					}
					int index = jobPreferences.IndexOf(jobPreferences2.Find(match));
					if (index > -1 && index < bestPreference)
					{
						bestPreference = index;
						preferredClient = c;
					}
				}
			}
			if (forceAssign && preferredClient == null)
			{
				preferredClient = clients[Rand.Int(clients.Count, Rand.RandSync.Unsynced)];
			}
			return preferredClient;
		}

		// Token: 0x0600344C RID: 13388 RVA: 0x001680AC File Offset: 0x001662AC
		public void UpdateMissionState(Mission mission)
		{
			foreach (Client client in this.connectedClients)
			{
				IWriteMessage msg = new WriteOnlyMessage();
				msg.WriteByte(20);
				int missionIndex = GameMain.GameSession.GetMissionIndex(mission);
				msg.WriteByte((byte)((missionIndex == -1) ? 255 : missionIndex));
				if (mission != null)
				{
					mission.ServerWrite(msg);
				}
				this.serverPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
			}
		}

		// Token: 0x0600344D RID: 13389 RVA: 0x00168144 File Offset: 0x00166344
		public static string CharacterLogName(Character character)
		{
			if (character == null)
			{
				return "[NULL]";
			}
			Client client = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == character);
			return NetworkMember.ClientLogName(client, character.LogName);
		}

		// Token: 0x0600344E RID: 13390 RVA: 0x0016819C File Offset: 0x0016639C
		public static void Log(string line, ServerLog.MessageType messageType)
		{
			if (GameMain.Server == null || !GameMain.Server.ServerSettings.SaveServerLogs)
			{
				return;
			}
			LuaCsSetup instance = LuaCsSetup.Instance;
			if (instance != null)
			{
				instance.EventService.PublishEvent<IEventServerLog>(delegate(IEventServerLog x)
				{
					x.OnServerLog(line, messageType);
				});
			}
			GameMain.Server.ServerSettings.ServerLog.WriteLine(line, messageType, true);
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				if (client.HasPermission(ClientPermissions.ServerLog))
				{
					GameMain.Server.SendDirectChatMessage(ChatMessage.Create(messageType.ToString(), line, ChatMessageType.ServerLog, null, null, PlayerConnectionChangeType.None, null), client);
				}
			}
		}

		// Token: 0x0600344F RID: 13391 RVA: 0x00168298 File Offset: 0x00166498
		public void Quit()
		{
			if (this.started)
			{
				this.started = false;
				base.ServerSettings.BanList.Save();
				if (GameMain.NetLobbyScreen.SelectedSub != null)
				{
					base.ServerSettings.SelectedSubmarine = GameMain.NetLobbyScreen.SelectedSub.Name;
				}
				if (GameMain.NetLobbyScreen.SelectedShuttle != null)
				{
					base.ServerSettings.SelectedShuttle = GameMain.NetLobbyScreen.SelectedShuttle.Name;
				}
				base.ServerSettings.SaveSettings();
				ModSender modSender = this.ModSender;
				if (modSender != null)
				{
					modSender.Dispose();
				}
				if (base.ServerSettings.SaveServerLogs)
				{
					GameServer.Log("Shutting down the server...", ServerLog.MessageType.ServerMessage);
					base.ServerSettings.ServerLog.Save();
				}
				GameAnalyticsManager.AddDesignEvent("GameServer:ShutDown");
				ServerPeer serverPeer = this.serverPeer;
				if (serverPeer != null)
				{
					serverPeer.Close();
				}
				SteamManager.CloseServer();
			}
		}

		// Token: 0x06003450 RID: 13392 RVA: 0x00168378 File Offset: 0x00166578
		private void UpdateClientLobbies()
		{
			ushort lastClientListUpdateID = base.LastClientListUpdateID;
			base.LastClientListUpdateID = lastClientListUpdateID + 1;
		}

		// Token: 0x06003451 RID: 13393 RVA: 0x00168398 File Offset: 0x00166598
		private List<Client> GetPlayingClients()
		{
			List<Client> playingClients = new List<Client>(from c in this.connectedClients
			where !c.AFK || !base.ServerSettings.AllowAFK
			select c);
			if (base.ServerSettings.AllowSpectating)
			{
				playingClients.RemoveAll((Client c) => c.SpectateOnly);
			}
			playingClients.RemoveAll((Client c) => c.Connection == this.OwnerConnection && c.SpectateOnly);
			return playingClients;
		}

		// Token: 0x06003452 RID: 13394 RVA: 0x0016840C File Offset: 0x0016660C
		public void RefreshPvpTeamAssignments(bool assignUnassignedNow = false, bool autoBalanceNow = false)
		{
			GameServer.<>c__DisplayClass177_0 CS$<>8__locals1 = new GameServer.<>c__DisplayClass177_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.team1 = new List<Client>();
			CS$<>8__locals1.team2 = new List<Client>();
			List<Client> playingClients = this.GetPlayingClients();
			CS$<>8__locals1.unassignedClients = new List<Client>(playingClients);
			for (int i = 0; i < CS$<>8__locals1.unassignedClients.Count; i++)
			{
				if (CS$<>8__locals1.unassignedClients[i].PreferredTeam == CharacterTeamType.Team1 || CS$<>8__locals1.unassignedClients[i].PreferredTeam == CharacterTeamType.Team2)
				{
					CS$<>8__locals1.<RefreshPvpTeamAssignments>g__assignTeam|0(CS$<>8__locals1.unassignedClients[i], CS$<>8__locals1.unassignedClients[i].PreferredTeam);
					i--;
				}
			}
			if (assignUnassignedNow)
			{
				if (CS$<>8__locals1.unassignedClients.Any<Client>())
				{
					this.SendChatMessage(TextManager.Get("PvP.WithoutTeamWillBeRandomlyAssigned").Value, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
				}
				while (CS$<>8__locals1.unassignedClients.Any<Client>())
				{
					Client randomClient = CS$<>8__locals1.unassignedClients.GetRandom(Rand.RandSync.Unsynced);
					CS$<>8__locals1.<RefreshPvpTeamAssignments>g__assignTeam|0(randomClient, (CS$<>8__locals1.team1.Count < CS$<>8__locals1.team2.Count) ? CharacterTeamType.Team1 : CharacterTeamType.Team2);
				}
			}
			if (base.ServerSettings.PvpAutoBalanceThreshold > 0)
			{
				int sizeDifference = Math.Abs(CS$<>8__locals1.team1.Count - CS$<>8__locals1.team2.Count);
				if (sizeDifference > base.ServerSettings.PvpAutoBalanceThreshold)
				{
					if (autoBalanceNow)
					{
						this.SendChatMessage(TextManager.Get("AutoBalance.Activating").Value, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
						while (Math.Abs(CS$<>8__locals1.team1.Count - CS$<>8__locals1.team2.Count) > base.ServerSettings.PvpAutoBalanceThreshold)
						{
							IEnumerable<Client> playingClients2 = this.GetPlayingClients();
							Func<Client, bool> predicate;
							if ((predicate = CS$<>8__locals1.<>9__2) == null)
							{
								predicate = (CS$<>8__locals1.<>9__2 = delegate(Client c)
								{
									if (CS$<>8__locals1.team1.Count <= CS$<>8__locals1.team2.Count)
									{
										return c.TeamID == CharacterTeamType.Team2;
									}
									return c.TeamID == CharacterTeamType.Team1;
								});
							}
							List<Client> biggerTeam = playingClients2.Where(predicate).ToList<Client>();
							CS$<>8__locals1.<RefreshPvpTeamAssignments>g__switchTeam|1(biggerTeam.GetRandom(Rand.RandSync.Unsynced), (CS$<>8__locals1.team1.Count < CS$<>8__locals1.team2.Count) ? CharacterTeamType.Team1 : CharacterTeamType.Team2);
						}
					}
					else if (base.ServerSettings.PvpTeamSelectionMode != PvpTeamSelectionMode.PlayerPreference && GameServer.pvpAutoBalanceCountdownRemaining == -1f)
					{
						this.SendChatMessage(TextManager.GetWithVariables("AutoBalance.CountdownStarted", new ValueTuple<string, LocalizedString>[]
						{
							new ValueTuple<string, LocalizedString>("[teamname]", TextManager.Get((CS$<>8__locals1.team1.Count > CS$<>8__locals1.team2.Count) ? "teampreference.team1" : "teampreference.team2")),
							new ValueTuple<string, LocalizedString>("[numberplayers]", (sizeDifference - base.ServerSettings.PvpAutoBalanceThreshold).ToString()),
							new ValueTuple<string, LocalizedString>("[numberseconds]", 10.ToString())
						}).Value, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
						GameServer.pvpAutoBalanceCountdownRemaining = 10f;
					}
				}
				else
				{
					this.StopAutoBalanceCountdown();
				}
			}
			else
			{
				this.StopAutoBalanceCountdown();
			}
			this.UpdateClientLobbies();
		}

		// Token: 0x06003453 RID: 13395 RVA: 0x00168704 File Offset: 0x00166904
		public void AssignClientToPvpTeamMidgame(Client client)
		{
			if (client.PreferredTeam == CharacterTeamType.None)
			{
				if (this.Team1Count == this.Team2Count)
				{
					client.TeamID = ((Rand.Value(Rand.RandSync.Unsynced) > 0.5f) ? CharacterTeamType.Team1 : CharacterTeamType.Team2);
					return;
				}
				client.TeamID = ((this.Team1Count < this.Team2Count) ? CharacterTeamType.Team1 : CharacterTeamType.Team2);
				return;
			}
			else
			{
				if (base.ServerSettings.PvpAutoBalanceThreshold <= 0)
				{
					client.TeamID = client.PreferredTeam;
					return;
				}
				int newTeam1Count = this.Team1Count + ((client.PreferredTeam == CharacterTeamType.Team1) ? 1 : 0);
				int newTeam2Count = this.Team2Count + ((client.PreferredTeam == CharacterTeamType.Team2) ? 1 : 0);
				if (Math.Abs(newTeam1Count - newTeam2Count) <= base.ServerSettings.PvpAutoBalanceThreshold)
				{
					client.TeamID = client.PreferredTeam;
					return;
				}
				client.TeamID = ((this.Team1Count < this.Team2Count) ? CharacterTeamType.Team1 : CharacterTeamType.Team2);
				return;
			}
		}

		// Token: 0x06003454 RID: 13396 RVA: 0x001687CF File Offset: 0x001669CF
		private void StopAutoBalanceCountdown()
		{
			if (GameServer.pvpAutoBalanceCountdownRemaining != -1f)
			{
				this.SendChatMessage(TextManager.Get("AutoBalance.CountdownCancelled").Value, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
			}
			GameServer.pvpAutoBalanceCountdownRemaining = -1f;
		}

		// Token: 0x0600345F RID: 13407 RVA: 0x001689AC File Offset: 0x00166BAC
		[CompilerGenerated]
		internal static float <ClientWriteIngame>g__GetShortestDistance|110_0(Vector2 viewPos, Character targetCharacter)
		{
			float distSqr = Vector2.DistanceSquared(viewPos, targetCharacter.WorldPosition);
			if (targetCharacter.ViewTarget != null && targetCharacter.ViewTarget != targetCharacter)
			{
				distSqr = Math.Min(distSqr, Vector2.DistanceSquared(viewPos, targetCharacter.ViewTarget.WorldPosition));
			}
			return distSqr;
		}

		// Token: 0x06003461 RID: 13409 RVA: 0x00168A14 File Offset: 0x00166C14
		[CompilerGenerated]
		internal static Color <UpdateCharacterInfo>g__validateColor|166_3(Color newColor, string colorName, IEnumerable<Color> supportedColors, ref GameServer.<>c__DisplayClass166_0 A_3)
		{
			if (!supportedColors.Contains(newColor))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Client ");
				defaultInterpolatedStringHandler.AppendFormatted(A_3.sender.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" attempted to set their ");
				defaultInterpolatedStringHandler.AppendFormatted(colorName);
				defaultInterpolatedStringHandler.AppendLiteral(" to an unsupported value (");
				defaultInterpolatedStringHandler.AppendFormatted<Color>(newColor);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return supportedColors.First<Color>();
			}
			return newColor;
		}

		// Token: 0x040019EB RID: 6635
		public bool SubmarineSwitchLoad;

		// Token: 0x040019EC RID: 6636
		private readonly List<Client> connectedClients = new List<Client>();

		// Token: 0x040019ED RID: 6637
		private readonly List<Client> clientsAttemptingToReconnectSoon = new List<Client>();

		// Token: 0x040019EE RID: 6638
		private readonly List<PreviousPlayer> previousPlayers = new List<PreviousPlayer>();

		// Token: 0x040019EF RID: 6639
		private int roundStartSeed;

		// Token: 0x040019F0 RID: 6640
		private bool started;

		// Token: 0x040019F1 RID: 6641
		private ServerPeer serverPeer;

		// Token: 0x040019F2 RID: 6642
		private DateTime refreshMasterTimer;

		// Token: 0x040019F3 RID: 6643
		private readonly TimeSpan refreshMasterInterval = new TimeSpan(0, 0, 60);

		// Token: 0x040019F4 RID: 6644
		private bool registeredToSteamMaster;

		// Token: 0x040019F5 RID: 6645
		private DateTime roundStartTime;

		// Token: 0x040019F6 RID: 6646
		private bool wasReadyToStartAutomatically;

		// Token: 0x040019F7 RID: 6647
		private bool autoRestartTimerRunning;

		// Token: 0x040019FA RID: 6650
		private const int PvpAutoBalanceCountdown = 10;

		// Token: 0x040019FB RID: 6651
		private static float pvpAutoBalanceCountdownRemaining = -1f;

		// Token: 0x040019FC RID: 6652
		private static readonly Queue<ChatMessage> pendingMessagesToOwner = new Queue<ChatMessage>();

		// Token: 0x040019FE RID: 6654
		private bool initiatedStartGame;

		// Token: 0x040019FF RID: 6655
		private CoroutineHandle startGameCoroutine;

		// Token: 0x04001A00 RID: 6656
		private readonly ServerEntityEventManager entityEventManager;

		// Token: 0x04001A03 RID: 6659
		private TraitorManager traitorManager;

		// Token: 0x04001A05 RID: 6661
		private readonly Option<int> ownerKey;

		// Token: 0x04001A06 RID: 6662
		private readonly Option<P2PEndpoint> ownerEndpoint;

		// Token: 0x04001A07 RID: 6663
		private double lastPingTime;

		// Token: 0x04001A08 RID: 6664
		private byte[] lastPingData;

		// Token: 0x04001A09 RID: 6665
		private readonly DoSProtection dosProtection = new DoSProtection();

		// Token: 0x04001A0A RID: 6666
		private bool isRoundStartWarningActive;

		// Token: 0x04001A0B RID: 6667
		private readonly RateLimiter charInfoRateLimiter = new RateLimiter(5, 10, new ValueTuple<RateLimitAction, RateLimitPunishment>[]
		{
			new ValueTuple<RateLimitAction, RateLimitPunishment>(RateLimitAction.OnLimitReached, RateLimitPunishment.Announce),
			new ValueTuple<RateLimitAction, RateLimitPunishment>(RateLimitAction.OnLimitDoubled, RateLimitPunishment.Kick)
		});

		// Token: 0x04001A0C RID: 6668
		public readonly List<string> JobAssignmentDebugLog = new List<string>();

		// Token: 0x02000BC6 RID: 3014
		public enum TryStartGameResult
		{
			// Token: 0x04003A47 RID: 14919
			Success,
			// Token: 0x04003A48 RID: 14920
			GameAlreadyStarted,
			// Token: 0x04003A49 RID: 14921
			PerksExceedAllowance,
			// Token: 0x04003A4A RID: 14922
			SubmarineNotFound,
			// Token: 0x04003A4B RID: 14923
			GameModeNotSelected,
			// Token: 0x04003A4C RID: 14924
			CannotStartMultiplayerCampaign
		}
	}
}
