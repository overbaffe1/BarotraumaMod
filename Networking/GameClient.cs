using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Eos;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.Steam;
using EventInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;

namespace Barotrauma.Networking
{
	// Token: 0x0200045E RID: 1118
	internal sealed class GameClient : NetworkMember
	{
		// Token: 0x1700132D RID: 4909
		// (get) Token: 0x06004B0D RID: 19213 RVA: 0x00293AC8 File Offset: 0x00291CC8
		public override bool IsClient
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700132E RID: 4910
		// (get) Token: 0x06004B0E RID: 19214 RVA: 0x00293ACB File Offset: 0x00291CCB
		public override bool IsServer
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700132F RID: 4911
		// (get) Token: 0x06004B0F RID: 19215 RVA: 0x00293ACE File Offset: 0x00291CCE
		public override Voting Voting { get; }

		// Token: 0x17001330 RID: 4912
		// (get) Token: 0x06004B10 RID: 19216 RVA: 0x00293AD6 File Offset: 0x00291CD6
		// (set) Token: 0x06004B11 RID: 19217 RVA: 0x00293ADE File Offset: 0x00291CDE
		public string Name { get; private set; }

		// Token: 0x06004B12 RID: 19218 RVA: 0x00293AE7 File Offset: 0x00291CE7
		public void SetName(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			this.Name = value;
			this.ForceNameJobTeamUpdate();
		}

		// Token: 0x06004B13 RID: 19219 RVA: 0x00293AFF File Offset: 0x00291CFF
		public void ForceNameJobTeamUpdate()
		{
			this.nameId += 1;
		}

		// Token: 0x17001331 RID: 4913
		// (get) Token: 0x06004B14 RID: 19220 RVA: 0x00293B10 File Offset: 0x00291D10
		// (set) Token: 0x06004B15 RID: 19221 RVA: 0x00293B18 File Offset: 0x00291D18
		public ClientPeer ClientPeer { get; private set; }

		// Token: 0x17001332 RID: 4914
		// (get) Token: 0x06004B16 RID: 19222 RVA: 0x00293B21 File Offset: 0x00291D21
		public GUITickBox FollowSubTickBox
		{
			get
			{
				return this.cameraFollowsSub;
			}
		}

		// Token: 0x17001333 RID: 4915
		// (get) Token: 0x06004B17 RID: 19223 RVA: 0x00293B29 File Offset: 0x00291D29
		public bool IsFollowSubTickBoxVisible
		{
			get
			{
				return base.GameStarted && Screen.Selected == GameMain.GameScreen && this.cameraFollowsSub != null && this.cameraFollowsSub.Visible;
			}
		}

		// Token: 0x17001334 RID: 4916
		// (get) Token: 0x06004B18 RID: 19224 RVA: 0x00293B54 File Offset: 0x00291D54
		public bool RoundStarting
		{
			get
			{
				return this.roundInitStatus == GameClient.RoundInitStatus.Starting || this.roundInitStatus == GameClient.RoundInitStatus.WaitingForStartGameFinalize;
			}
		}

		// Token: 0x17001335 RID: 4917
		// (get) Token: 0x06004B19 RID: 19225 RVA: 0x00293B6A File Offset: 0x00291D6A
		public string ServerName
		{
			get
			{
				return base.ServerSettings.ServerName;
			}
		}

		// Token: 0x17001336 RID: 4918
		// (get) Token: 0x06004B1A RID: 19226 RVA: 0x00293B77 File Offset: 0x00291D77
		public bool IsBlockedBySpamFilter
		{
			get
			{
				return this.BlockedBySpamFilterTimer > 0f;
			}
		}

		// Token: 0x17001337 RID: 4919
		// (get) Token: 0x06004B1B RID: 19227 RVA: 0x00293B86 File Offset: 0x00291D86
		// (set) Token: 0x06004B1C RID: 19228 RVA: 0x00293B8E File Offset: 0x00291D8E
		public float EndRoundTimeRemaining { get; private set; }

		// Token: 0x17001338 RID: 4920
		// (get) Token: 0x06004B1D RID: 19229 RVA: 0x00293B97 File Offset: 0x00291D97
		// (set) Token: 0x06004B1E RID: 19230 RVA: 0x00293B9F File Offset: 0x00291D9F
		public byte SessionId { get; private set; }

		// Token: 0x17001339 RID: 4921
		// (get) Token: 0x06004B1F RID: 19231 RVA: 0x00293BA8 File Offset: 0x00291DA8
		// (set) Token: 0x06004B20 RID: 19232 RVA: 0x00293BB0 File Offset: 0x00291DB0
		public VoipClient VoipClient { get; private set; }

		// Token: 0x1700133A RID: 4922
		// (get) Token: 0x06004B21 RID: 19233 RVA: 0x00293BB9 File Offset: 0x00291DB9
		public override IReadOnlyList<Client> ConnectedClients
		{
			get
			{
				return this.otherClients;
			}
		}

		// Token: 0x1700133B RID: 4923
		// (get) Token: 0x06004B22 RID: 19234 RVA: 0x00293BC1 File Offset: 0x00291DC1
		public Client MyClient
		{
			get
			{
				return this.ConnectedClients.FirstOrDefault((Client c) => c.SessionId == this.SessionId);
			}
		}

		// Token: 0x1700133C RID: 4924
		// (get) Token: 0x06004B23 RID: 19235 RVA: 0x00293BDA File Offset: 0x00291DDA
		public Option<int> Ping
		{
			get
			{
				if (this.MyClient == null || this.MyClient.Ping == 0)
				{
					return Option<int>.None();
				}
				return Option<int>.Some((int)this.MyClient.Ping);
			}
		}

		// Token: 0x1700133D RID: 4925
		// (get) Token: 0x06004B24 RID: 19236 RVA: 0x00293C07 File Offset: 0x00291E07
		public IEnumerable<Client> PreviouslyConnectedClients
		{
			get
			{
				return this.previouslyConnectedClients;
			}
		}

		// Token: 0x1700133E RID: 4926
		// (get) Token: 0x06004B25 RID: 19237 RVA: 0x00293C0F File Offset: 0x00291E0F
		public bool MidRoundSyncing
		{
			get
			{
				return this.EntityEventManager.MidRoundSyncing;
			}
		}

		// Token: 0x1700133F RID: 4927
		// (get) Token: 0x06004B26 RID: 19238 RVA: 0x00293C1C File Offset: 0x00291E1C
		// (set) Token: 0x06004B27 RID: 19239 RVA: 0x00293C24 File Offset: 0x00291E24
		public bool? WaitForNextRoundRespawn { get; set; }

		// Token: 0x17001340 RID: 4928
		// (get) Token: 0x06004B28 RID: 19240 RVA: 0x00293C2D File Offset: 0x00291E2D
		public bool IsServerOwner
		{
			get
			{
				return this.ownerKey.IsSome();
			}
		}

		// Token: 0x06004B29 RID: 19241 RVA: 0x00293C3A File Offset: 0x00291E3A
		public GameClient(string newName, Endpoint endpoint, string serverName, Option<int> ownerKey) : this(newName, endpoint.ToEnumerable<Endpoint>().ToImmutableArray<Endpoint>(), serverName, ownerKey)
		{
		}

		// Token: 0x06004B2A RID: 19242 RVA: 0x00293C54 File Offset: 0x00291E54
		public GameClient(string newName, ImmutableArray<Endpoint> endpoints, string serverName, Option<int> ownerKey)
		{
			this.ownerKey = ownerKey;
			this.roundInitStatus = GameClient.RoundInitStatus.NotStarted;
			this.NetStats = new NetStats();
			this.inGameHUD = new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			this.chatBox = new ChatBox(this.inGameHUD, false);
			ChatBox chatBox = this.chatBox;
			chatBox.OnEnterMessage = (GUITextBox.OnEnterHandler)Delegate.Combine(chatBox.OnEnterMessage, new GUITextBox.OnEnterHandler(this.EnterChatMessage));
			this.chatBox.InputBox.OnTextChanged += this.TypingChatMessage;
			this.buttonContainer = new GUILayoutGroup(HUDLayoutSettings.ToRectTransform(HUDLayoutSettings.ButtonAreaTop, this.inGameHUD.RectTransform), true, Anchor.CenterRight)
			{
				AbsoluteSpacing = 5,
				CanBeFocused = false
			};
			this.endRoundVoteText = TextManager.Get("EndRound");
			this.EndVoteTickBox = new GUITickBox(new RectTransform(new Vector2(0.1f, 0.4f), this.buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(150, 0)
			}, this.endRoundVoteText, null, "")
			{
				OnSelected = new GUITickBox.OnSelectedHandler(this.ToggleEndRoundVote),
				Visible = false
			};
			this.EndVoteTickBox.TextBlock.Wrap = true;
			this.ShowLogButton = new GUIButton(new RectTransform(new Vector2(0.1f, 0.6f), this.buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(150, 0)
			}, TextManager.Get("ServerLog"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton button, object userData)
				{
					if (base.ServerSettings.ServerLog.LogFrame == null)
					{
						base.ServerSettings.ServerLog.CreateLogFrame();
					}
					else
					{
						base.ServerSettings.ServerLog.LogFrame = null;
						GUI.KeyboardDispatcher.Subscriber = null;
					}
					return true;
				}
			};
			this.ShowLogButton.TextBlock.AutoScaleHorizontal = true;
			GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(0.1f, 0.4f), this.buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(150, 0)
			}, TextManager.Get("CamFollowSubmarine"), null, "");
			guitickBox.Selected = Camera.FollowSub;
			guitickBox.OnSelected = delegate(GUITickBox tbox)
			{
				Camera.FollowSub = tbox.Selected;
				return true;
			};
			this.cameraFollowsSub = guitickBox;
			GameMain.DebugDraw = false;
			Hull.EditFire = false;
			Hull.EditWater = false;
			this.SetName(newName);
			this.EntityEventManager = new ClientEntityEventManager(this);
			this.FileReceiver = new FileReceiver();
			FileReceiver fileReceiver = this.FileReceiver;
			fileReceiver.OnFinished = (FileReceiver.TransferInDelegate)Delegate.Combine(fileReceiver.OnFinished, new FileReceiver.TransferInDelegate(this.OnFileReceived));
			FileReceiver fileReceiver2 = this.FileReceiver;
			fileReceiver2.OnTransferFailed = (FileReceiver.TransferInDelegate)Delegate.Combine(fileReceiver2.OnTransferFailed, new FileReceiver.TransferInDelegate(this.OnTransferFailed));
			this.characterInfo = new CharacterInfo(CharacterPrefab.HumanSpeciesName, this.Name, null, null, 0, Rand.RandSync.Unsynced, default(Identifier))
			{
				Job = null
			};
			this.otherClients = new List<Client>();
			base.ServerSettings = new ServerSettings(this, serverName, 0, 0, 0, false, false, IPAddress.Any);
			this.Voting = new Voting();
			this.serverEndpoints = endpoints;
			this.InitiateServerJoin();
			ChatMessage.LastID = 0;
			GameMain.ResetNetLobbyScreen();
		}

		// Token: 0x06004B2B RID: 19243 RVA: 0x00294070 File Offset: 0x00292270
		public ServerInfo CreateServerInfoFromSettings()
		{
			ServerInfo serverInfo = ServerInfo.FromServerEndpoints(this.ClientPeer.AllServerEndpoints, base.ServerSettings);
			GameMain.ServerListScreen.UpdateOrAddServerInfo(serverInfo);
			return serverInfo;
		}

		// Token: 0x06004B2C RID: 19244 RVA: 0x002940A0 File Offset: 0x002922A0
		private void InitiateServerJoin()
		{
			base.LastClientListUpdateID = 0;
			foreach (Client c in this.ConnectedClients)
			{
				GameMain.NetLobbyScreen.RemovePlayer(c);
				c.Dispose();
			}
			this.otherClients.Clear();
			this.chatBox.InputBox.Enabled = false;
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			if (((netLobbyScreen != null) ? netLobbyScreen.ChatInput : null) != null)
			{
				GameMain.NetLobbyScreen.ChatInput.Enabled = false;
			}
			this.myCharacter = Character.Controlled;
			ChatMessage.LastID = 0;
			ClientPeer clientPeer = this.ClientPeer;
			if (clientPeer != null)
			{
				clientPeer.Close(PeerDisconnectPacket.WithReason(DisconnectReason.Disconnected));
			}
			this.ClientPeer = this.CreateNetPeer();
			this.ClientPeer.Start();
			CoroutineManager.StartCoroutine(this.WaitForStartingInfo(), "WaitForStartingInfo");
		}

		// Token: 0x06004B2D RID: 19245 RVA: 0x00294190 File Offset: 0x00292390
		public static void SetLobbyPublic(bool isPublic)
		{
			SteamManager.SetLobbyPublic(isPublic);
		}

		// Token: 0x06004B2E RID: 19246 RVA: 0x00294198 File Offset: 0x00292398
		private ClientPeer CreateNetPeer()
		{
			ClientPeer.Callbacks callbacks = new ClientPeer.Callbacks(new ClientPeer.Callbacks.MessageCallback(this.ReadDataMessage), new ClientPeer.Callbacks.DisconnectCallback(this.OnClientPeerDisconnect), new ClientPeer.Callbacks.InitializationCompleteCallback(this.OnConnectionInitializationComplete));
			Endpoint endpoint = this.serverEndpoints.First<Endpoint>();
			LidgrenEndpoint lidgrenEndpoint2 = endpoint as LidgrenEndpoint;
			if (lidgrenEndpoint2 == null)
			{
				if (endpoint is P2PEndpoint)
				{
					int key;
					if (this.ownerKey.TryUnwrap(out key))
					{
						return new P2POwnerPeer(callbacks, key, this.serverEndpoints.Cast<P2PEndpoint>().ToImmutableArray<P2PEndpoint>());
					}
					if (this.ownerKey.IsNone())
					{
						return new P2PClientPeer(this.serverEndpoints.Cast<P2PEndpoint>().ToImmutableArray<P2PEndpoint>(), callbacks);
					}
				}
				throw new ArgumentOutOfRangeException();
			}
			LidgrenEndpoint lidgrenEndpoint = lidgrenEndpoint2;
			return new LidgrenClientPeer(lidgrenEndpoint, callbacks, this.ownerKey);
		}

		// Token: 0x06004B2F RID: 19247 RVA: 0x00294268 File Offset: 0x00292468
		public void CreateServerCrashMessage()
		{
			LocalizedString basicServerCrashMsg = TextManager.Get("DisconnectReason.ServerCrashed");
			GUIMessageBox.MessageBoxes.OfType<GUIMessageBox>().Where(delegate(GUIMessageBox mb)
			{
				GUITextBlock text = mb.Text;
				return ((text != null) ? text.Text : null) == basicServerCrashMsg;
			}).ToArray<GUIMessageBox>().ForEach(delegate(GUIMessageBox mb)
			{
				mb.Close();
			});
			if (GUIMessageBox.MessageBoxes.All(delegate(GUIComponent mb)
			{
				GUIMessageBox guimessageBox = mb as GUIMessageBox;
				RichString a;
				if (guimessageBox == null)
				{
					a = null;
				}
				else
				{
					GUITextBlock text = guimessageBox.Text;
					a = ((text != null) ? text.Text : null);
				}
				return a != ChildServerRelay.CrashMessage;
			}))
			{
				GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("ConnectionLost"), ChildServerRelay.CrashMessage, null, null, GUIMessageBox.Type.Default);
				GUIButton guibutton = msgBox.Buttons[0];
				guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(this.ReturnToPreviousMenu));
			}
		}

		// Token: 0x06004B30 RID: 19248 RVA: 0x0029434F File Offset: 0x0029254F
		private bool ReturnToPreviousMenu(GUIButton button, object obj)
		{
			Submarine.Unload();
			GameMain.Client = null;
			GameMain.GameSession = null;
			if (this.IsServerOwner)
			{
				GameMain.MainMenuScreen.Select();
			}
			else
			{
				GameMain.ServerListScreen.Select();
			}
			GUIMessageBox.MessageBoxes.Clear();
			return true;
		}

		// Token: 0x06004B31 RID: 19249 RVA: 0x0029438B File Offset: 0x0029258B
		private void CancelConnect()
		{
			this.Quit();
		}

		// Token: 0x06004B32 RID: 19250 RVA: 0x00294393 File Offset: 0x00292593
		private IEnumerable<CoroutineStatus> WaitForStartingInfo()
		{
			GameClient.<WaitForStartingInfo>d__102 <WaitForStartingInfo>d__ = new GameClient.<WaitForStartingInfo>d__102(-2);
			<WaitForStartingInfo>d__.<>4__this = this;
			return <WaitForStartingInfo>d__;
		}

		// Token: 0x06004B33 RID: 19251 RVA: 0x002943A4 File Offset: 0x002925A4
		public void Update(float deltaTime)
		{
			this.BlockedBySpamFilterTimer -= deltaTime;
			foreach (Client c in this.ConnectedClients)
			{
				if (c.Character != null && c.Character.Removed)
				{
					c.Character = null;
				}
				c.UpdateVoipSound();
			}
			if (VoipCapture.Instance != null && VoipCapture.Instance.LastEnqueueAudio > DateTime.Now - new TimeSpan(0, 0, 0, 0, 100))
			{
				if (Screen.Selected == GameMain.NetLobbyScreen)
				{
					GameMain.NetLobbyScreen.SetPlayerSpeaking(this.MyClient);
				}
				else
				{
					GameSession gameSession = GameMain.GameSession;
					if (gameSession != null)
					{
						CrewManager crewManager = gameSession.CrewManager;
						if (crewManager != null)
						{
							crewManager.SetClientSpeaking(this.MyClient);
						}
					}
				}
			}
			this.NetStats.Update(deltaTime);
			this.UpdateHUD(deltaTime);
			try
			{
				this.incomingMessagesToProcess.Clear();
				this.incomingMessagesToProcess.AddRange(this.pendingIncomingMessages);
				foreach (IReadMessage inc in this.incomingMessagesToProcess)
				{
					this.ReadDataMessage(inc);
				}
				this.pendingIncomingMessages.Clear();
				ClientPeer clientPeer = this.ClientPeer;
				if (clientPeer != null)
				{
					clientPeer.Update(deltaTime);
				}
			}
			catch (Exception e)
			{
				string errorMsg = "Error while reading a message from server. ";
				if (GameMain.Client == null)
				{
					errorMsg += "Client disposed.";
				}
				Entity causingEntity;
				GameClient.AppendExceptionInfo(ref errorMsg, out causingEntity, e);
				MethodBase targetSite2 = e.TargetSite;
				string targetSite = ((targetSite2 != null) ? targetSite2.ToString() : null) ?? "unknown";
				GameAnalyticsManager.AddErrorEventOnce("GameClient.Update:CheckServerMessagesException" + targetSite, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				DebugConsole.ThrowError(errorMsg, null, (causingEntity != null) ? causingEntity.ContentPackage : null, false, false);
				new GUIMessageBox(TextManager.Get("Error"), TextManager.GetWithVariables("MessageReadError", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[message]", e.Message),
					new ValueTuple<string, string>("[targetsite]", targetSite)
				}), null, null, GUIMessageBox.Type.Default).DisplayInLoadingScreens = true;
				this.Quit();
				GUI.DisableHUD = false;
				GameMain.ServerListScreen.Select();
				return;
			}
			if (!this.connected)
			{
				return;
			}
			this.CloseReconnectBox();
			if (base.GameStarted && Screen.Selected == GameMain.GameScreen)
			{
				this.EndVoteTickBox.Visible = (base.ServerSettings.AllowEndVoting && this.HasSpawned);
				RespawnManager respawnManager = base.RespawnManager;
				if (respawnManager != null)
				{
					respawnManager.Update(deltaTime);
				}
				if (this.updateTimer <= DateTime.Now)
				{
					this.SendIngameUpdate();
				}
			}
			else
			{
				if (this.updateTimer <= DateTime.Now)
				{
					this.SendLobbyUpdate();
				}
				if (Timing.TotalTime > this.LastMissingCampaignSubRequestTime)
				{
					this.TryRequestMissingCampaignSubs();
					this.LastMissingCampaignSubRequestTime = Timing.TotalTime + 10.0;
				}
			}
			if (base.ServerSettings.VoiceChatEnabled)
			{
				VoipClient voipClient = this.VoipClient;
				if (voipClient != null)
				{
					voipClient.SendToServer();
				}
			}
			if (this.IsServerOwner && this.connected && !this.connectCancelled && GameMain.WindowActive && !ChildServerRelay.IsProcessAlive)
			{
				this.Quit();
				this.CreateServerCrashMessage();
			}
			if (this.updateTimer <= DateTime.Now)
			{
				this.updateTimer = DateTime.Now + base.UpdateInterval;
			}
		}

		// Token: 0x06004B34 RID: 19252 RVA: 0x00294740 File Offset: 0x00292940
		private void ReadDataMessage(IReadMessage inc)
		{
			GameClient.<>c__DisplayClass108_0 CS$<>8__locals1 = new GameClient.<>c__DisplayClass108_0();
			ServerPacketHeader header = (ServerPacketHeader)inc.ReadByte();
			bool flag2 = this.roundInitStatus == GameClient.RoundInitStatus.WaitingForStartGameFinalize;
			bool flag3 = flag2;
			if (flag3)
			{
				bool flag4 = header == ServerPacketHeader.FILE_TRANSFER || header == ServerPacketHeader.PING_REQUEST || header - ServerPacketHeader.STARTGAMEFINALIZE <= 1;
				flag3 = !flag4;
			}
			if (flag3)
			{
				inc.BitPosition -= 8;
				this.pendingIncomingMessages.Add(inc);
				return;
			}
			GameClient.<>c__DisplayClass108_0 CS$<>8__locals2 = CS$<>8__locals1;
			GameModePreset selectedMode = GameMain.NetLobbyScreen.SelectedMode;
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign campaign;
			if (selectedMode != ((gameSession != null) ? gameSession.GameMode.Preset : null))
			{
				campaign = null;
			}
			else
			{
				GameSession gameSession2 = GameMain.GameSession;
				campaign = (((gameSession2 != null) ? gameSession2.GameMode : null) as MultiPlayerCampaign);
			}
			CS$<>8__locals2.campaign = campaign;
			if (Screen.Selected is ModDownloadScreen)
			{
				if (header <= ServerPacketHeader.CHEATS_ENABLED)
				{
					if (header != ServerPacketHeader.UPDATE_LOBBY && header != ServerPacketHeader.PERMISSIONS && header != ServerPacketHeader.CHEATS_ENABLED)
					{
						return;
					}
				}
				else if (header <= ServerPacketHeader.PING_REQUEST)
				{
					if (header != ServerPacketHeader.FILE_TRANSFER && header != ServerPacketHeader.PING_REQUEST)
					{
						return;
					}
				}
				else
				{
					if (header == ServerPacketHeader.STARTGAME)
					{
						base.GameStarted = true;
						return;
					}
					if (header != ServerPacketHeader.ENDGAME)
					{
						return;
					}
					base.GameStarted = false;
					return;
				}
			}
			switch (header)
			{
			case ServerPacketHeader.UPDATE_LOBBY:
				this.ReadLobbyUpdate(inc);
				return;
			case ServerPacketHeader.UPDATE_INGAME:
				try
				{
					this.ReadIngameUpdate(inc);
					return;
				}
				catch (Exception e)
				{
					string errorMsg = "Error while reading an ingame update message from server.";
					Entity causingEntity;
					GameClient.AppendExceptionInfo(ref errorMsg, out causingEntity, e);
					GameAnalyticsManager.AddErrorEventOnce("GameClient.ReadDataMessage:ReadIngameUpdate", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					throw;
				}
				break;
			case ServerPacketHeader.PERMISSIONS:
				this.ReadPermissions(inc);
				return;
			case ServerPacketHeader.ACHIEVEMENT:
				GameClient.ReadAchievement(inc);
				return;
			case ServerPacketHeader.ACHIEVEMENT_STAT:
				GameClient.ReadAchievementStat(inc);
				return;
			case ServerPacketHeader.CHEATS_ENABLED:
			{
				bool cheatsEnabled = inc.ReadBoolean();
				inc.ReadPadBits();
				if (cheatsEnabled == DebugConsole.CheatsEnabled)
				{
					return;
				}
				DebugConsole.CheatsEnabled = cheatsEnabled;
				AchievementManager.CheatsEnabled = cheatsEnabled;
				if (cheatsEnabled)
				{
					GUIMessageBox cheatMessageBox = new GUIMessageBox(TextManager.Get("CheatsEnabledTitle"), TextManager.Get("CheatsEnabledDescription"), null, null, GUIMessageBox.Type.Default);
					GUIButton guibutton = cheatMessageBox.Buttons[0];
					guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
					{
						DebugConsole.TextBox.Select(-1, false);
						return true;
					}));
					return;
				}
				return;
			}
			case ServerPacketHeader.CAMPAIGN_SETUP_INFO:
			{
				byte saveCount = inc.ReadByte();
				List<CampaignMode.SaveInfo> saveInfos = new List<CampaignMode.SaveInfo>();
				for (int i = 0; i < (int)saveCount; i++)
				{
					saveInfos.Add(INetSerializableStruct.Read<CampaignMode.SaveInfo>(inc));
				}
				MultiPlayerCampaign.StartCampaignSetup(saveInfos);
				return;
			}
			case ServerPacketHeader.FILE_TRANSFER:
				this.FileReceiver.ReadMessage(inc);
				return;
			case ServerPacketHeader.VOICE:
				break;
			case ServerPacketHeader.VOICE_AMPLITUDE_DEBUG:
				return;
			case ServerPacketHeader.PING_REQUEST:
			{
				IWriteMessage response = new WriteOnlyMessage();
				response.WriteByte(7);
				byte requestLen = inc.ReadByte();
				response.WriteByte(requestLen);
				for (int j = 0; j < (int)requestLen; j++)
				{
					byte b = inc.ReadByte();
					response.WriteByte(b);
				}
				this.ClientPeer.Send(response, DeliveryMethod.Unreliable, true);
				return;
			}
			case ServerPacketHeader.CLIENT_PINGS:
			{
				byte clientCount = inc.ReadByte();
				for (int k = 0; k < (int)clientCount; k++)
				{
					byte clientId = inc.ReadByte();
					ushort clientPing = inc.ReadUInt16();
					Client client = this.ConnectedClients.Find((Client c) => c.SessionId == clientId);
					if (client != null)
					{
						client.Ping = clientPing;
					}
				}
				return;
			}
			case ServerPacketHeader.QUERY_STARTGAME:
			{
				DebugConsole.Log("Received QUERY_STARTGAME packet.");
				string subName = inc.ReadString();
				string subHash = inc.ReadString();
				bool hasEnemySub = inc.ReadBoolean();
				string enemySubName = subName;
				string enemySubHash = subHash;
				if (hasEnemySub)
				{
					enemySubName = inc.ReadString();
					enemySubHash = inc.ReadString();
				}
				bool usingShuttle = inc.ReadBoolean();
				string shuttleName = inc.ReadString();
				string shuttleHash = inc.ReadString();
				byte campaignID = inc.ReadByte();
				ushort campaignSaveID = inc.ReadUInt16();
				Dictionary<MultiPlayerCampaign.NetFlags, ushort> campaignUpdateIDs = new Dictionary<MultiPlayerCampaign.NetFlags, ushort>();
				foreach (object obj in Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)))
				{
					MultiPlayerCampaign.NetFlags flag = (MultiPlayerCampaign.NetFlags)obj;
					campaignUpdateIDs[flag] = inc.ReadUInt16();
				}
				if (CS$<>8__locals1.campaign != null)
				{
					CS$<>8__locals1.campaign.PendingSubmarineSwitch = null;
				}
				GameMain.NetLobbyScreen.UsingShuttle = usingShuttle;
				bool readyToStart;
				if (CS$<>8__locals1.campaign == null && campaignID == 0)
				{
					readyToStart = (GameMain.NetLobbyScreen.TrySelectSub(subName, subHash, SelectedSubType.Sub, GameMain.NetLobbyScreen.SubList, true) && GameMain.NetLobbyScreen.TrySelectSub(shuttleName, shuttleHash, SelectedSubType.Shuttle, GameMain.NetLobbyScreen.ShuttleList.ListBox, true));
					if (hasEnemySub && !GameMain.NetLobbyScreen.TrySelectSub(enemySubName, enemySubHash, SelectedSubType.EnemySub, GameMain.NetLobbyScreen.SubList, true))
					{
						readyToStart = false;
					}
				}
				else
				{
					readyToStart = (CS$<>8__locals1.campaign != null && CS$<>8__locals1.campaign.CampaignID == campaignID && CS$<>8__locals1.campaign.LastSaveID == campaignSaveID && campaignUpdateIDs.All((KeyValuePair<MultiPlayerCampaign.NetFlags, ushort> kvp) => CS$<>8__locals1.campaign.GetLastUpdateIdForFlag(kvp.Key) == kvp.Value));
				}
				DebugConsole.Log(readyToStart ? "Ready to start." : "Not ready to start.");
				this.SendStartGameResponse(readyToStart);
				if (readyToStart && !CoroutineManager.IsCoroutineRunning("WaitForStartRound"))
				{
					CoroutineManager.StartCoroutine(NetLobbyScreen.WaitForStartRound(null), "WaitForStartRound");
					return;
				}
				return;
			}
			case ServerPacketHeader.WARN_STARTGAME:
			{
				DebugConsole.Log("Received WARN_STARTGAME packet.");
				RoundStartWarningData warningData = INetSerializableStruct.Read<RoundStartWarningData>(inc);
				ImmutableArray<DisembarkPerkPrefab> team1IncompatiblePerks = ToolBox.UintIdentifierArrayToPrefabCollection<DisembarkPerkPrefab>(DisembarkPerkPrefab.Prefabs, warningData.Team1IncompatiblePerks);
				ImmutableArray<DisembarkPerkPrefab> team2IncompatiblePerks = ToolBox.UintIdentifierArrayToPrefabCollection<DisembarkPerkPrefab>(DisembarkPerkPrefab.Prefabs, warningData.Team2IncompatiblePerks);
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				if (netLobbyScreen == null)
				{
					return;
				}
				SerializableDateTime utcNow = SerializableDateTime.UtcNow;
				TimeSpan timeSpan = TimeSpan.FromSeconds((double)warningData.RoundStartsAnywaysTimeInSeconds);
				netLobbyScreen.ShowStartRoundWarning(utcNow + timeSpan, warningData.Team1Sub, team1IncompatiblePerks, warningData.Team2Sub, team2IncompatiblePerks);
				return;
			}
			case ServerPacketHeader.CANCEL_STARTGAME:
			{
				DebugConsole.Log("Received CANCEL_STARTGAME packet.");
				NetLobbyScreen netLobbyScreen2 = GameMain.NetLobbyScreen;
				if (netLobbyScreen2 != null)
				{
					netLobbyScreen2.CloseStartRoundWarning();
				}
				NetLobbyScreen netLobbyScreen3 = GameMain.NetLobbyScreen;
				GUITickBox readyToStartBox = (netLobbyScreen3 != null) ? netLobbyScreen3.ReadyToStartBox : null;
				if (readyToStartBox != null)
				{
					readyToStartBox.Selected = false;
					this.SetReadyToStart(readyToStartBox);
					return;
				}
				return;
			}
			case ServerPacketHeader.STARTGAME:
			{
				DebugConsole.Log("Received STARTGAME packet.");
				NetLobbyScreen netLobbyScreen4 = GameMain.NetLobbyScreen;
				if (netLobbyScreen4 == null || !netLobbyScreen4.AFKSelected || !base.ServerSettings.AllowAFK)
				{
					if (this.startGameCoroutine != null && CoroutineManager.IsCoroutineRunning(this.startGameCoroutine))
					{
						DebugConsole.Log("New round started before the previous one had finished loading. Starting a new round once loading the round finishes...");
						this.requestNewRoundStart = true;
						return;
					}
					if (Screen.Selected == GameMain.GameScreen)
					{
						GameSession gameSession3 = GameMain.GameSession;
						if (((gameSession3 != null) ? gameSession3.GameMode : null) is CampaignMode)
						{
							DebugConsole.Log("Starting StartGame coroutine...");
							this.startGameCoroutine = CoroutineManager.StartCoroutine(this.StartGame(inc), "");
							return;
						}
					}
					GUIMessageBox.CloseAll();
					DebugConsole.Log("Starting StartGame coroutine with a loading screen...");
					this.startGameCoroutine = GameMain.Instance.ShowLoading(this.StartGame(inc), false);
					return;
				}
				else
				{
					base.GameStarted = true;
					NetLobbyScreen netLobbyScreen5 = GameMain.NetLobbyScreen;
					if (netLobbyScreen5 == null)
					{
						return;
					}
					netLobbyScreen5.Select();
					return;
				}
				break;
			}
			case ServerPacketHeader.STARTGAMEFINALIZE:
				DebugConsole.NewMessage("Received STARTGAMEFINALIZE packet. Round init status: " + this.roundInitStatus.ToString(), null, false);
				if (this.roundInitStatus == GameClient.RoundInitStatus.WaitingForStartGameFinalize)
				{
					if (CS$<>8__locals1.campaign != null && NetIdUtils.IdMoreRecent(CS$<>8__locals1.campaign.PendingSaveID, CS$<>8__locals1.campaign.LastSaveID))
					{
						if (this.FileReceiver.ActiveTransfers.Any((FileReceiver.FileTransferIn t) => t.FileType == FileTransferType.CampaignSave))
						{
							return;
						}
					}
					this.ReadStartGameFinalize(inc);
					return;
				}
				return;
			case ServerPacketHeader.ENDGAME:
			{
				CampaignMode.TransitionType transitionType = (CampaignMode.TransitionType)inc.ReadByte();
				bool save = inc.ReadBoolean();
				string endMessage = string.Empty;
				endMessage = inc.ReadString();
				byte missionCount = inc.ReadByte();
				for (int l = 0; l < (int)missionCount; l++)
				{
					bool missionSuccessful = inc.ReadBoolean();
					GameSession gameSession4 = GameMain.GameSession;
					Mission mission = (gameSession4 != null) ? gameSession4.GetMission(l) : null;
					if (mission != null)
					{
						mission.Completed = missionSuccessful;
					}
				}
				CharacterTeamType winningTeam = (CharacterTeamType)inc.ReadByte();
				if (winningTeam != CharacterTeamType.None)
				{
					GameMain.GameSession.WinningTeam = new CharacterTeamType?(winningTeam);
					Mission combatMission = GameMain.GameSession.Missions.FirstOrDefault((Mission m) => m is CombatMission);
					if (combatMission != null)
					{
						combatMission.Completed = true;
					}
				}
				bool includesTraitorInfo = inc.ReadBoolean();
				TraitorManager.TraitorResults? traitorResults = null;
				if (includesTraitorInfo)
				{
					traitorResults = new TraitorManager.TraitorResults?(INetSerializableStruct.Read<TraitorManager.TraitorResults>(inc));
				}
				this.roundInitStatus = GameClient.RoundInitStatus.Interrupted;
				CoroutineManager.StartCoroutine(this.EndGame(endMessage, transitionType, traitorResults), "EndGame");
				GUI.SetSavingIndicatorState(save);
				return;
			}
			case ServerPacketHeader.MISSION:
			{
				int missionIndex = (int)inc.ReadByte();
				GameSession gameSession5 = GameMain.GameSession;
				Mission mission2 = (gameSession5 != null) ? gameSession5.GetMission(missionIndex) : null;
				if (mission2 != null)
				{
					mission2.ClientRead(inc);
					return;
				}
				return;
			}
			case ServerPacketHeader.EVENTACTION:
			{
				GameSession gameSession6 = GameMain.GameSession;
				if (gameSession6 == null)
				{
					return;
				}
				gameSession6.EventManager.ClientRead(inc);
				return;
			}
			case ServerPacketHeader.TRAITOR_MESSAGE:
				TraitorManager.ClientRead(inc);
				return;
			case ServerPacketHeader.CREW:
			{
				MultiPlayerCampaign campaign2 = CS$<>8__locals1.campaign;
				if (campaign2 == null)
				{
					return;
				}
				campaign2.ClientReadCrew(inc);
				return;
			}
			case ServerPacketHeader.MEDICAL:
			{
				MultiPlayerCampaign campaign3 = CS$<>8__locals1.campaign;
				if (campaign3 == null)
				{
					return;
				}
				MedicalClinic medicalClinic = campaign3.MedicalClinic;
				if (medicalClinic == null)
				{
					return;
				}
				medicalClinic.ClientRead(inc);
				return;
			}
			case ServerPacketHeader.CIRCUITBOX:
				GameClient.ReadCircuitBoxMessage(inc);
				return;
			case ServerPacketHeader.MONEY:
			{
				MultiPlayerCampaign campaign4 = CS$<>8__locals1.campaign;
				if (campaign4 == null)
				{
					return;
				}
				campaign4.ClientReadMoney(inc);
				return;
			}
			case ServerPacketHeader.READY_CHECK:
				ReadyCheck.ClientRead(inc);
				return;
			case ServerPacketHeader.UNLOCKRECIPE:
			{
				CharacterTeamType team = (CharacterTeamType)inc.ReadByte();
				Identifier identifier = inc.ReadIdentifier();
				GameSession gameSession7 = GameMain.GameSession;
				if (gameSession7 == null)
				{
					return;
				}
				gameSession7.UnlockRecipe(team, identifier, true);
				return;
			}
			case ServerPacketHeader.SEND_BACKUP_INDICES:
			{
				NetLobbyScreen netLobbyScreen6 = GameMain.NetLobbyScreen;
				if (netLobbyScreen6 == null)
				{
					return;
				}
				MultiPlayerCampaignSetupUI campaignSetupUI = netLobbyScreen6.CampaignSetupUI;
				if (campaignSetupUI == null)
				{
					return;
				}
				campaignSetupUI.OnBackupIndicesReceived(inc);
				return;
			}
			default:
				return;
			}
			if (this.VoipClient == null)
			{
				string errorMsg2 = "Failed to read a voice packet from the server (VoipClient == null). ";
				if (GameMain.Client == null)
				{
					errorMsg2 += "Client disposed. ";
				}
				errorMsg2 = errorMsg2 + "\n" + Environment.StackTrace.CleanupStackTrace();
				GameAnalyticsManager.AddErrorEventOnce("GameClient.ReadDataMessage:VoipClientNull", (GameMain.Client == null) ? GameAnalyticsManager.ErrorSeverity.Error : GameAnalyticsManager.ErrorSeverity.Warning, errorMsg2);
				return;
			}
			this.VoipClient.Read(inc);
			return;
		}

		// Token: 0x06004B35 RID: 19253 RVA: 0x002950E8 File Offset: 0x002932E8
		private void ReadStartGameFinalize(IReadMessage inc)
		{
			Action<string> log;
			if ((log = GameClient.<>O.<0>__Log) == null)
			{
				log = (GameClient.<>O.<0>__Log = new Action<string>(DebugConsole.Log));
			}
			TaskPool.ListTasks(log);
			ushort contentToPreloadCount = inc.ReadUInt16();
			List<ContentFile> contentToPreload = new List<ContentFile>();
			for (int i = 0; i < (int)contentToPreloadCount; i++)
			{
				string filePath = inc.ReadString();
				Func<ContentFile, bool> <>9__5;
				ContentFile file = ContentPackageManager.EnabledPackages.All.Select(delegate(ContentPackage p)
				{
					ImmutableArray<ContentFile> files = p.Files;
					Func<ContentFile, bool> predicate;
					if ((predicate = <>9__5) == null)
					{
						predicate = (<>9__5 = ((ContentFile f) => f.Path == filePath));
					}
					return files.FirstOrDefault(predicate);
				}).FirstOrDefault((ContentFile f) => f != null);
				contentToPreload.AddIfNotNull(file);
			}
			byte roundId = inc.ReadByte();
			string campaignErrorInfo = string.Empty;
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
			if (campaign != null)
			{
				if (roundId != campaign.RoundID)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(176, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Received a StartGameFinalize message for an incorrect round (client: ");
					defaultInterpolatedStringHandler.AppendFormatted<byte>(campaign.RoundID);
					defaultInterpolatedStringHandler.AppendLiteral(", server: ");
					defaultInterpolatedStringHandler.AppendFormatted<byte>(roundId);
					defaultInterpolatedStringHandler.AppendLiteral("). The server might have started a new round before the client finished loading the previous one.");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					this.requestNewRoundStart = true;
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(58, 3);
				defaultInterpolatedStringHandler2.AppendLiteral(" Round start save ID: ");
				defaultInterpolatedStringHandler2.AppendFormatted<ushort?>(this.debugStartGameCampaignSaveID);
				defaultInterpolatedStringHandler2.AppendLiteral(", last save id: ");
				defaultInterpolatedStringHandler2.AppendFormatted<ushort>(campaign.LastSaveID);
				defaultInterpolatedStringHandler2.AppendLiteral(", pending save id: ");
				defaultInterpolatedStringHandler2.AppendFormatted<ushort>(campaign.PendingSaveID);
				defaultInterpolatedStringHandler2.AppendLiteral(".");
				campaignErrorInfo = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			GameMain.GameSession.EventManager.PreloadContent(contentToPreload);
			int subEqualityCheckValue = inc.ReadInt32();
			int num = subEqualityCheckValue;
			Submarine mainSub = Submarine.MainSub;
			int? num2;
			if (mainSub == null)
			{
				num2 = null;
			}
			else
			{
				SubmarineInfo info = mainSub.Info;
				num2 = ((info != null) ? new int?(info.EqualityCheckVal) : null);
			}
			int? num3 = num2;
			if (num != num3.GetValueOrDefault())
			{
				string str = "Submarine equality check failed. The submarine loaded at your end doesn't match the one loaded by the server. ";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(109, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("There may have been an error in receiving the up-to-date submarine file from the server. Round init status: ");
				defaultInterpolatedStringHandler3.AppendFormatted<GameClient.RoundInitStatus>(this.roundInitStatus);
				defaultInterpolatedStringHandler3.AppendLiteral(".");
				string errorMsg = str + defaultInterpolatedStringHandler3.ToStringAndClear() + campaignErrorInfo;
				GameAnalyticsManager.AddErrorEventOnce("GameClient.StartGame:SubsDontMatch" + Level.Loaded.Seed, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				throw new Exception(errorMsg);
			}
			byte missionCount = inc.ReadByte();
			List<Identifier> serverMissionIdentifiers = new List<Identifier>();
			for (int j = 0; j < (int)missionCount; j++)
			{
				serverMissionIdentifiers.Add(inc.ReadIdentifier());
			}
			if ((int)missionCount != GameMain.GameSession.GameMode.Missions.Count<Mission>())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(128, 4);
				defaultInterpolatedStringHandler4.AppendLiteral("Mission equality check failed. Mission count doesn't match the server. ");
				defaultInterpolatedStringHandler4.AppendLiteral("Server: ");
				defaultInterpolatedStringHandler4.AppendFormatted(string.Join<Identifier>(", ", serverMissionIdentifiers));
				defaultInterpolatedStringHandler4.AppendLiteral(", ");
				defaultInterpolatedStringHandler4.AppendLiteral("client: ");
				defaultInterpolatedStringHandler4.AppendFormatted(string.Join<Identifier>(", ", from m in GameMain.GameSession.GameMode.Missions
				select m.Prefab.Identifier));
				defaultInterpolatedStringHandler4.AppendLiteral(", ");
				defaultInterpolatedStringHandler4.AppendLiteral("game session: ");
				defaultInterpolatedStringHandler4.AppendFormatted(string.Join<Identifier>(", ", from m in GameMain.GameSession.Missions
				select m.Prefab.Identifier));
				defaultInterpolatedStringHandler4.AppendLiteral("). Round init status: ");
				defaultInterpolatedStringHandler4.AppendFormatted<GameClient.RoundInitStatus>(this.roundInitStatus);
				defaultInterpolatedStringHandler4.AppendLiteral(".");
				string errorMsg2 = defaultInterpolatedStringHandler4.ToStringAndClear() + campaignErrorInfo;
				GameAnalyticsManager.AddErrorEventOnce("GameClient.StartGame:MissionsCountMismatch" + Level.Loaded.Seed, GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
				throw new Exception(errorMsg2);
			}
			if (missionCount > 0)
			{
				if (!(from m in GameMain.GameSession.GameMode.Missions
				select m.Prefab.Identifier into id
				orderby id
				select id).SequenceEqual(from id in serverMissionIdentifiers
				orderby id
				select id))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(164, 4);
					defaultInterpolatedStringHandler5.AppendLiteral("Mission equality check failed. The mission selected at your end doesn't match the one loaded by the server ");
					defaultInterpolatedStringHandler5.AppendLiteral("Server: ");
					defaultInterpolatedStringHandler5.AppendFormatted(string.Join<Identifier>(", ", serverMissionIdentifiers));
					defaultInterpolatedStringHandler5.AppendLiteral(", ");
					defaultInterpolatedStringHandler5.AppendLiteral("client: ");
					defaultInterpolatedStringHandler5.AppendFormatted(string.Join<Identifier>(", ", from m in GameMain.GameSession.GameMode.Missions
					select m.Prefab.Identifier));
					defaultInterpolatedStringHandler5.AppendLiteral(", ");
					defaultInterpolatedStringHandler5.AppendLiteral("game session: ");
					defaultInterpolatedStringHandler5.AppendFormatted(string.Join<Identifier>(", ", from m in GameMain.GameSession.Missions
					select m.Prefab.Identifier));
					defaultInterpolatedStringHandler5.AppendLiteral("). Round init status: ");
					defaultInterpolatedStringHandler5.AppendFormatted<GameClient.RoundInitStatus>(this.roundInitStatus);
					defaultInterpolatedStringHandler5.AppendLiteral(".");
					string errorMsg3 = defaultInterpolatedStringHandler5.ToStringAndClear() + campaignErrorInfo;
					GameAnalyticsManager.AddErrorEventOnce("GameClient.StartGame:MissionsDontMatch" + Level.Loaded.Seed, GameAnalyticsManager.ErrorSeverity.Error, errorMsg3);
					throw new Exception(errorMsg3);
				}
				GameMain.GameSession.EnforceMissionOrder(serverMissionIdentifiers);
			}
			Dictionary<Level.LevelGenStage, int> levelEqualityCheckValues = new Dictionary<Level.LevelGenStage, int>();
			foreach (Level.LevelGenStage stage in from s in Enum.GetValues(typeof(Level.LevelGenStage)).OfType<Level.LevelGenStage>()
			orderby s
			select s)
			{
				levelEqualityCheckValues.Add(stage, inc.ReadInt32());
			}
			foreach (Level.LevelGenStage stage2 in levelEqualityCheckValues.Keys)
			{
				if (Level.Loaded.EqualityCheckValues[stage2] != levelEqualityCheckValues[stage2])
				{
					string[] array = new string[13];
					array[0] = "Level equality check failed. The level generated at your end doesn't match the level generated by the server, ";
					int num4 = 1;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(17, 2);
					defaultInterpolatedStringHandler6.AppendLiteral("(client value ");
					defaultInterpolatedStringHandler6.AppendFormatted<Level.LevelGenStage>(stage2);
					defaultInterpolatedStringHandler6.AppendLiteral(":");
					defaultInterpolatedStringHandler6.AppendFormatted(Level.Loaded.EqualityCheckValues[stage2].ToString("X"));
					defaultInterpolatedStringHandler6.AppendLiteral(", ");
					array[num4] = defaultInterpolatedStringHandler6.ToStringAndClear();
					int num5 = 2;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(17, 2);
					defaultInterpolatedStringHandler7.AppendLiteral("server value ");
					defaultInterpolatedStringHandler7.AppendFormatted<Level.LevelGenStage>(stage2);
					defaultInterpolatedStringHandler7.AppendLiteral(": ");
					defaultInterpolatedStringHandler7.AppendFormatted(levelEqualityCheckValues[stage2].ToString("X"));
					defaultInterpolatedStringHandler7.AppendLiteral(", ");
					array[num5] = defaultInterpolatedStringHandler7.ToStringAndClear();
					int num6 = 3;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(21, 1);
					defaultInterpolatedStringHandler8.AppendLiteral("level value count: ");
					defaultInterpolatedStringHandler8.AppendFormatted<int>(levelEqualityCheckValues.Count);
					defaultInterpolatedStringHandler8.AppendLiteral(", ");
					array[num6] = defaultInterpolatedStringHandler8.ToStringAndClear();
					array[4] = "seed: ";
					array[5] = Level.Loaded.Seed;
					array[6] = ", missions: ";
					array[7] = string.Join<Identifier>(", ", from m in GameMain.GameSession.GameMode.Missions
					select m.Prefab.Identifier);
					array[8] = ", sub: ";
					array[9] = ((Submarine.MainSub == null) ? "null" : (Submarine.MainSub.Info.Name + " (" + Submarine.MainSub.Info.MD5Hash.ShortRepresentation));
					array[10] = ", ";
					int num7 = 11;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(33, 2);
					defaultInterpolatedStringHandler9.AppendLiteral("mirrored: ");
					defaultInterpolatedStringHandler9.AppendFormatted<bool>(Level.Loaded.Mirrored);
					defaultInterpolatedStringHandler9.AppendLiteral("). Round init status: ");
					defaultInterpolatedStringHandler9.AppendFormatted<GameClient.RoundInitStatus>(this.roundInitStatus);
					defaultInterpolatedStringHandler9.AppendLiteral(".");
					array[num7] = defaultInterpolatedStringHandler9.ToStringAndClear();
					array[12] = campaignErrorInfo;
					string errorMsg4 = string.Concat(array);
					GameAnalyticsManager.AddErrorEventOnce("GameClient.StartGame:LevelsDontMatch" + Level.Loaded.Seed, GameAnalyticsManager.ErrorSeverity.Error, errorMsg4);
					throw new Exception(errorMsg4);
				}
			}
			foreach (Mission mission in GameMain.GameSession.Missions)
			{
				mission.ClientReadInitial(inc);
			}
			if (inc.ReadBoolean())
			{
				CrewManager.ClientReadActiveOrders(inc);
			}
			if (inc.ReadBoolean())
			{
				this.ApplyDisembarkPerk();
			}
			this.roundInitStatus = GameClient.RoundInitStatus.Started;
		}

		// Token: 0x06004B36 RID: 19254 RVA: 0x00295A6C File Offset: 0x00293C6C
		private void ApplyDisembarkPerk()
		{
			ImmutableHashSet<Character> characters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			ImmutableArray<Character> team1Characters = (from c in characters
			where c.TeamID == CharacterTeamType.Team1
			select c).ToImmutableArray<Character>();
			ImmutableArray<Character> team2Characters = (from c in characters
			where c.TeamID == CharacterTeamType.Team2
			select c).ToImmutableArray<Character>();
			GameSession.GetPerks().ApplyAll(team1Characters, team2Characters);
		}

		// Token: 0x06004B37 RID: 19255 RVA: 0x00295AF0 File Offset: 0x00293CF0
		private void OnClientPeerDisconnect(PeerDisconnectPacket disconnectPacket)
		{
			bool wasConnected = this.connected;
			this.connected = false;
			this.connectCancelled = true;
			CoroutineManager.StopCoroutines("WaitForStartingInfo");
			this.CloseReconnectBox();
			GUI.ClearCursorWait();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Client received a disconnect message. Reason: ");
			defaultInterpolatedStringHandler.AppendFormatted<DisconnectReason>(disconnectPacket.DisconnectReason);
			string disconnectMessage = defaultInterpolatedStringHandler.ToStringAndClear();
			SteamTimelineManager.OnClientDisconnect(disconnectMessage);
			if (disconnectPacket.ShouldCreateAnalyticsEvent)
			{
				GameAnalyticsManager.AddErrorEventOnce("GameClient.HandleDisconnectMessage", GameAnalyticsManager.ErrorSeverity.Debug, disconnectMessage);
			}
			if (disconnectPacket.DisconnectReason == DisconnectReason.ServerFull)
			{
				this.AskToWaitInQueue();
				return;
			}
			if (disconnectPacket.ShouldAttemptReconnect && !this.IsServerOwner && wasConnected)
			{
				if (disconnectPacket.IsEventSyncError)
				{
					GameMain.NetLobbyScreen.Select();
					GameSession gameSession = GameMain.GameSession;
					if (gameSession != null)
					{
						gameSession.EndRound("", CampaignMode.TransitionType.None, null, true);
					}
					base.GameStarted = false;
					this.myCharacter = null;
				}
				this.AttemptReconnect(disconnectPacket);
				return;
			}
			ClientPeer clientPeer = this.ClientPeer;
			bool flag = clientPeer is P2PClientPeer || clientPeer is P2POwnerPeer;
			if (flag)
			{
				EosSessionManager.LeaveSession();
				SteamManager.LeaveLobby();
			}
			GameMain.ModDownloadScreen.Reset();
			ContentPackageManager.EnabledPackages.Restore();
			GameSession gameSession2 = GameMain.GameSession;
			if (gameSession2 != null)
			{
				CampaignMode campaign = gameSession2.Campaign;
				if (campaign != null)
				{
					campaign.CancelStartRound();
				}
			}
			this.UpdatePresence("");
			foreach (FileReceiver.FileTransferIn fileTransfer in this.FileReceiver.ActiveTransfers.ToArray<FileReceiver.FileTransferIn>())
			{
				this.FileReceiver.StopTransfer(fileTransfer, true);
			}
			ChildServerRelay.AttemptGracefulShutDown(20);
			GUIMessageBox.MessageBoxes.RemoveAll((GUIComponent c) => ((c != null) ? c.UserData : null) is RoundSummary);
			CharacterInfo characterInfo = this.characterInfo;
			if (characterInfo != null)
			{
				characterInfo.Remove();
			}
			VoipClient voipClient = this.VoipClient;
			if (voipClient != null)
			{
				voipClient.Dispose();
			}
			this.VoipClient = null;
			GameMain.Client = null;
			GameMain.GameSession = null;
			this.ReturnToPreviousMenu(null, null);
			if (disconnectPacket.DisconnectReason != DisconnectReason.Disconnected)
			{
				new GUIMessageBox(TextManager.Get(wasConnected ? "ConnectionLost" : "CouldNotConnectToServer"), disconnectPacket.PopupMessage, null, null, GUIMessageBox.Type.Default).DisplayInLoadingScreens = true;
			}
		}

		// Token: 0x06004B38 RID: 19256 RVA: 0x00295D34 File Offset: 0x00293F34
		private void CreateReconnectBox(LocalizedString headerText, LocalizedString bodyText)
		{
			this.reconnectBox = new GUIMessageBox(headerText, bodyText, new LocalizedString[]
			{
				TextManager.Get("Cancel")
			}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
			{
				DisplayInLoadingScreens = true
			};
			GUIButton guibutton = this.reconnectBox.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
			{
				this.CancelConnect();
				return true;
			}));
			GUIButton guibutton2 = this.reconnectBox.Buttons[0];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(this.reconnectBox.Close));
		}

		// Token: 0x06004B39 RID: 19257 RVA: 0x00295DFE File Offset: 0x00293FFE
		private void CloseReconnectBox()
		{
			GUIMessageBox guimessageBox = this.reconnectBox;
			if (guimessageBox != null)
			{
				guimessageBox.Close();
			}
			this.reconnectBox = null;
		}

		// Token: 0x06004B3A RID: 19258 RVA: 0x00295E18 File Offset: 0x00294018
		private void AskToWaitInQueue()
		{
			CoroutineManager.StopCoroutines("WaitForStartingInfo");
			if (CoroutineManager.IsCoroutineRunning("WaitInServerQueue"))
			{
				return;
			}
			GUIMessageBox queueBox = new GUIMessageBox(TextManager.Get("DisconnectReason.ServerFull"), TextManager.Get("ServerFullQuestionPrompt"), new LocalizedString[]
			{
				TextManager.Get("Cancel"),
				TextManager.Get("ServerQueue")
			}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUIButton guibutton = queueBox.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(queueBox.Close));
			GUIButton guibutton2 = queueBox.Buttons[1];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(queueBox.Close));
			GUIButton guibutton3 = queueBox.Buttons[1];
			guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
			{
				this.CloseReconnectBox();
				CoroutineManager.StartCoroutine(this.WaitInServerQueue(), "WaitInServerQueue");
				return true;
			}));
		}

		// Token: 0x06004B3B RID: 19259 RVA: 0x00295F2C File Offset: 0x0029412C
		private void AttemptReconnect(PeerDisconnectPacket peerDisconnectPacket)
		{
			this.connectCancelled = false;
			this.CreateReconnectBox(TextManager.Get("ConnectionLost"), peerDisconnectPacket.ReconnectMessage);
			ImmutableArray<ServerContentPackage> prevContentPackages = this.ClientPeer.ServerContentPackages;
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			ushort lastUpdateID = netLobbyScreen.LastUpdateID;
			netLobbyScreen.LastUpdateID = lastUpdateID - 1;
			this.InitiateServerJoin();
			if (this.ClientPeer != null)
			{
				this.ClientPeer.ContentPackageOrderReceived = true;
				this.ClientPeer.ServerContentPackages = prevContentPackages;
			}
		}

		// Token: 0x06004B3C RID: 19260 RVA: 0x00295FA0 File Offset: 0x002941A0
		private void UpdatePresence(string connectCommand)
		{
			GameClient.<>c__DisplayClass116_0 CS$<>8__locals1 = new GameClient.<>c__DisplayClass116_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.connectCommand = connectCommand;
			CS$<>8__locals1.desc = TextManager.GetWithVariable("FriendPlayingOnServer", "[servername]", this.ServerName, FormatCapitals.No);
			TaskPool.Add("UpdateEosPresence", CS$<>8__locals1.<UpdatePresence>g__updateEosPresence|0(), delegate(Task _)
			{
			});
			if (SteamManager.IsInitialized)
			{
				SteamFriends.ClearRichPresence();
				if (!CS$<>8__locals1.connectCommand.IsNullOrWhiteSpace())
				{
					SteamFriends.SetRichPresence("servername", this.ServerName);
					SteamFriends.SetRichPresence("status", CS$<>8__locals1.desc.Value);
					SteamFriends.SetRichPresence("connect", CS$<>8__locals1.connectCommand);
				}
			}
		}

		// Token: 0x06004B3D RID: 19261 RVA: 0x00296064 File Offset: 0x00294264
		private void OnConnectionInitializationComplete()
		{
			bool connectedToLocalHost = this.serverEndpoints.All(delegate(Endpoint e)
			{
				LidgrenEndpoint lidgrenEndpoint = e as LidgrenEndpoint;
				return lidgrenEndpoint != null && lidgrenEndpoint.Address.IsLocalHost;
			});
			string escapedServerName = this.ServerName.IsNullOrWhiteSpace() ? "Server" : ToolBox.EscapeCharacters(this.ServerName);
			string text;
			if (!connectedToLocalHost)
			{
				text = "-connect \"" + escapedServerName + "\" " + string.Join(",", from e in this.serverEndpoints
				select e.StringRepresentation);
			}
			else
			{
				text = string.Empty;
			}
			string connectCommand = text;
			this.UpdatePresence(connectCommand);
			this.canStart = true;
			this.connected = true;
			this.VoipClient = new VoipClient(this, this.ClientPeer);
			Screen selected = Screen.Selected;
			bool flag = selected is GameScreen || selected is RoundSummaryScreen || selected is NetLobbyScreen;
			if (flag)
			{
				this.EntityEventManager.ClearSelf();
				using (List<Character>.Enumerator enumerator = Character.CharacterList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character c = enumerator.Current;
						c.ResetNetState();
					}
					goto IL_137;
				}
			}
			GameMain.ModDownloadScreen.Select();
			IL_137:
			this.chatBox.InputBox.Enabled = true;
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			if (((netLobbyScreen != null) ? netLobbyScreen.ChatInput : null) != null)
			{
				GameMain.NetLobbyScreen.ChatInput.Enabled = true;
			}
		}

		// Token: 0x06004B3E RID: 19262 RVA: 0x002961EC File Offset: 0x002943EC
		private IEnumerable<CoroutineStatus> WaitInServerQueue()
		{
			GameClient.<WaitInServerQueue>d__118 <WaitInServerQueue>d__ = new GameClient.<WaitInServerQueue>d__118(-2);
			<WaitInServerQueue>d__.<>4__this = this;
			return <WaitInServerQueue>d__;
		}

		// Token: 0x06004B3F RID: 19263 RVA: 0x002961FC File Offset: 0x002943FC
		private static void ReadAchievement(IReadMessage inc)
		{
			Identifier achievementIdentifier = inc.ReadIdentifier();
			AchievementManager.UnlockAchievement(achievementIdentifier, false, null, null);
		}

		// Token: 0x06004B40 RID: 19264 RVA: 0x0029621C File Offset: 0x0029441C
		private static void ReadAchievementStat(IReadMessage inc)
		{
			NetIncrementedStat netStat = INetSerializableStruct.Read<NetIncrementedStat>(inc);
			AchievementManager.IncrementStat(netStat.Stat, netStat.Amount);
		}

		// Token: 0x06004B41 RID: 19265 RVA: 0x00296244 File Offset: 0x00294444
		private static void ReadCircuitBoxMessage(IReadMessage inc)
		{
			NetCircuitBoxHeader header = INetSerializableStruct.Read<NetCircuitBoxHeader>(inc);
			CircuitBoxOpcode opcode = header.Opcode;
			INetSerializableStruct netSerializableStruct;
			if (opcode != CircuitBoxOpcode.Error)
			{
				if (opcode != CircuitBoxOpcode.Cursor)
				{
					throw new ArgumentOutOfRangeException("Opcode", header.Opcode, "This data cannot be handled using direct network messages.");
				}
				netSerializableStruct = INetSerializableStruct.Read<NetCircuitBoxCursorInfo>(inc);
			}
			else
			{
				netSerializableStruct = INetSerializableStruct.Read<CircuitBoxErrorEvent>(inc);
			}
			INetSerializableStruct data = netSerializableStruct;
			CircuitBox box;
			if (header.FindTarget().TryUnwrap(out box))
			{
				box.ClientRead(data);
			}
		}

		// Token: 0x06004B42 RID: 19266 RVA: 0x002962C0 File Offset: 0x002944C0
		private void ReadPermissions(IReadMessage inc)
		{
			List<string> permittedConsoleCommands = new List<string>();
			byte clientId = inc.ReadByte();
			ClientPermissions permissions = ClientPermissions.None;
			List<DebugConsole.Command> permittedCommands = new List<DebugConsole.Command>();
			Client.ReadPermissions(inc, out permissions, out permittedCommands);
			Client targetClient = this.ConnectedClients.Find((Client c) => c.SessionId == clientId);
			if (targetClient != null)
			{
				targetClient.SetPermissions(permissions, permittedCommands);
			}
			if (clientId == this.SessionId)
			{
				this.SetMyPermissions(permissions, from command in permittedCommands
				select command.Names[0]);
			}
		}

		// Token: 0x06004B43 RID: 19267 RVA: 0x00296358 File Offset: 0x00294558
		private void SetMyPermissions(ClientPermissions newPermissions, IEnumerable<Identifier> permittedConsoleCommands)
		{
			if (!this.permittedConsoleCommands.Any((Identifier c) => !permittedConsoleCommands.Contains(c)) && !permittedConsoleCommands.Any((Identifier c) => !this.permittedConsoleCommands.Contains(c)) && newPermissions == this.permissions)
			{
				return;
			}
			bool refreshCampaignUI = this.permissions.HasFlag(ClientPermissions.ManageCampaign) != newPermissions.HasFlag(ClientPermissions.ManageCampaign) || this.permissions.HasFlag(ClientPermissions.ManageRound) != newPermissions.HasFlag(ClientPermissions.ManageRound);
			this.permissions = newPermissions;
			this.permittedConsoleCommands = permittedConsoleCommands.ToList<Identifier>();
			if (!this.IsServerOwner)
			{
				GUIMessageBox.MessageBoxes.RemoveAll((GUIComponent mb) => mb.UserData as string == "permissions");
				GUIMessageBox msgBox = new GUIMessageBox("", "", null, null, GUIMessageBox.Type.Default)
				{
					UserData = "permissions"
				};
				msgBox.Content.ClearChildren();
				msgBox.Content.RectTransform.RelativeSize = new Vector2(0.95f, 0.9f);
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = TextManager.Get("PermissionsChanged");
				GUIFont font = GUIStyle.LargeFont;
				GUITextBlock header = new GUITextBlock(rectT, text, null, font, Alignment.Center, false, "", null);
				header.RectTransform.IsFixedSize = true;
				GUILayoutGroup permissionArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				GUILayoutGroup leftColumn = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), permissionArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				GUILayoutGroup rightColumn = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), permissionArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				RectTransform rectT2 = new RectTransform(new Vector2((newPermissions == ClientPermissions.None) ? 2f : 1f, 0f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = TextManager.Get((newPermissions == ClientPermissions.None) ? "PermissionsRemoved" : "CurrentPermissions");
				font = ((newPermissions == ClientPermissions.None) ? GUIStyle.Font : GUIStyle.SubHeadingFont);
				GUITextBlock permissionsLabel = new GUITextBlock(rectT2, text2, null, font, Alignment.Left, true, "", null);
				permissionsLabel.RectTransform.NonScaledSize = new Point(permissionsLabel.Rect.Width, permissionsLabel.Rect.Height);
				permissionsLabel.RectTransform.IsFixedSize = true;
				if (newPermissions != ClientPermissions.None)
				{
					LocalizedString permissionList = "";
					foreach (object obj in Enum.GetValues(typeof(ClientPermissions)))
					{
						ClientPermissions permission = (ClientPermissions)obj;
						if (newPermissions.HasFlag(permission) && permission != ClientPermissions.None)
						{
							permissionList += "   - " + TextManager.Get("ClientPermission." + permission.ToString()) + "\n";
						}
					}
					new GUITextBlock(new RectTransform(new Vector2(1f, 0f), leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), permissionList, null, null, Alignment.Left, false, "", null);
				}
				if (newPermissions.HasFlag(ClientPermissions.ConsoleCommands))
				{
					RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), rightColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text3 = TextManager.Get("PermittedConsoleCommands");
					font = GUIStyle.SubHeadingFont;
					GUITextBlock commandsLabel = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, true, "", null);
					GUIListBox commandList = new GUIListBox(new RectTransform(new Vector2(1f, 1f), rightColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
					foreach (Identifier permittedCommand in permittedConsoleCommands)
					{
						Vector2 relativeSize = new Vector2(1f, 0.05f);
						RectTransform rectTransform = commandList.Content.RectTransform;
						Anchor anchor = Anchor.TopLeft;
						Point? minSize = new Point?(new Point(0, 15));
						RectTransform rectT4 = new RectTransform(relativeSize, rectTransform, anchor, null, minSize, null, ScaleBasis.Normal);
						RichString text4 = permittedCommand.Value;
						font = GUIStyle.SmallFont;
						new GUITextBlock(rectT4, text4, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
					}
					RectTransform rectTransform2 = permissionsLabel.RectTransform;
					RectTransform rectTransform3 = commandsLabel.RectTransform;
					Point nonScaledSize = new Point(permissionsLabel.Rect.Width, Math.Max(permissionsLabel.Rect.Height, commandsLabel.Rect.Height));
					rectTransform3.NonScaledSize = nonScaledSize;
					rectTransform2.NonScaledSize = nonScaledSize;
					commandsLabel.RectTransform.IsFixedSize = true;
				}
				new GUIButton(new RectTransform(new Vector2(0.5f, 0.05f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ok"), Alignment.Center, "", null).OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
				permissionArea.RectTransform.MinSize = new Point(0, Math.Max(leftColumn.RectTransform.Children.Sum((RectTransform c) => c.Rect.Height), rightColumn.RectTransform.Children.Sum((RectTransform c) => c.Rect.Height)));
				permissionArea.RectTransform.IsFixedSize = true;
				int contentHeight = (int)((float)msgBox.Content.RectTransform.Children.Sum((RectTransform c) => c.Rect.Height + msgBox.Content.AbsoluteSpacing) * 1.05f);
				msgBox.Content.ChildAnchor = Anchor.TopCenter;
				msgBox.Content.Stretch = true;
				msgBox.Content.RectTransform.MinSize = new Point(0, contentHeight);
				msgBox.InnerFrame.RectTransform.MinSize = new Point(0, (int)((float)contentHeight / permissionArea.RectTransform.RelativeSize.Y / msgBox.Content.RectTransform.RelativeSize.Y));
			}
			if (refreshCampaignUI)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign != null)
				{
					CampaignUI campaignUI = campaign.CampaignUI;
					if (campaignUI != null)
					{
						UpgradeStore upgradeStore = campaignUI.UpgradeStore;
						if (upgradeStore != null)
						{
							upgradeStore.RequestRefresh(false);
						}
					}
					CampaignUI campaignUI2 = campaign.CampaignUI;
					if (campaignUI2 != null)
					{
						HRManagerUI hrmanagerUI = campaignUI2.HRManagerUI;
						if (hrmanagerUI != null)
						{
							hrmanagerUI.RefreshUI();
						}
					}
				}
			}
			GameMain.NetLobbyScreen.RefreshEnabledElements();
			base.ServerSettings.Close();
			this.OnPermissionChanged.Invoke(new GameClient.PermissionChangedEvent(this.permissions, this.permittedConsoleCommands));
		}

		// Token: 0x06004B44 RID: 19268 RVA: 0x00296CA0 File Offset: 0x00294EA0
		private IEnumerable<CoroutineStatus> StartGame(IReadMessage inc)
		{
			GameClient.<StartGame>d__124 <StartGame>d__ = new GameClient.<StartGame>d__124(-2);
			<StartGame>d__.<>4__this = this;
			<StartGame>d__.<>3__inc = inc;
			return <StartGame>d__;
		}

		// Token: 0x06004B45 RID: 19269 RVA: 0x00296CB7 File Offset: 0x00294EB7
		public IEnumerable<CoroutineStatus> EndGame(string endMessage, CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None, TraitorManager.TraitorResults? traitorResults = null)
		{
			GameClient.<EndGame>d__125 <EndGame>d__ = new GameClient.<EndGame>d__125(-2);
			<EndGame>d__.<>4__this = this;
			<EndGame>d__.<>3__endMessage = endMessage;
			<EndGame>d__.<>3__transitionType = transitionType;
			<EndGame>d__.<>3__traitorResults = traitorResults;
			return <EndGame>d__;
		}

		// Token: 0x06004B46 RID: 19270 RVA: 0x00296CDC File Offset: 0x00294EDC
		private void ReadInitialUpdate(IReadMessage inc)
		{
			this.SessionId = inc.ReadByte();
			ushort subListCount = inc.ReadUInt16();
			this.ServerSubmarines.Clear();
			for (int i = 0; i < (int)subListCount; i++)
			{
				string subName = inc.ReadString();
				string subHash = inc.ReadString();
				SubmarineClass subClass = (SubmarineClass)inc.ReadByte();
				bool isShuttle = inc.ReadBoolean();
				bool requiredContentPackagesInstalled = inc.ReadBoolean();
				SubmarineInfo matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == subName && s.MD5Hash.StringRepresentation == subHash);
				if (matchingSub == null)
				{
					matchingSub = new SubmarineInfo(Path.Combine(new string[]
					{
						SaveUtil.SubmarineDownloadFolder,
						subName
					}) + ".sub", subHash, null, false, false)
					{
						SubmarineClass = subClass
					};
					if (isShuttle)
					{
						matchingSub.AddTag(SubmarineTag.Shuttle);
					}
				}
				matchingSub.RequiredContentPackagesInstalled = requiredContentPackagesInstalled;
				this.ServerSubmarines.Add(matchingSub);
			}
			GameMain.NetLobbyScreen.UpdateSubList(GameMain.NetLobbyScreen.SubList, this.ServerSubmarines);
			GameMain.NetLobbyScreen.UpdateSubList(GameMain.NetLobbyScreen.ShuttleList.ListBox, from s in this.ServerSubmarines
			where s.HasTag(SubmarineTag.Shuttle)
			select s);
			base.GameStarted = inc.ReadBoolean();
			bool allowSpectating = inc.ReadBoolean();
			bool allowAFK = inc.ReadBoolean();
			bool permadeathMode = inc.ReadBoolean();
			bool ironmanMode = inc.ReadBoolean();
			this.ReadPermissions(inc);
			if (base.GameStarted && Screen.Selected != GameMain.GameScreen)
			{
				LocalizedString message;
				if (permadeathMode)
				{
					message = TextManager.Get(ironmanMode ? "RoundRunningIronman" : "RoundRunningPermadeath");
				}
				else
				{
					message = TextManager.Get(allowSpectating ? "RoundRunningSpectateEnabled" : "RoundRunningSpectateDisabled");
				}
				new GUIMessageBox(TextManager.Get("PleaseWait"), message, null, null, GUIMessageBox.Type.Default);
				if (!(Screen.Selected is ModDownloadScreen))
				{
					GameMain.NetLobbyScreen.Select();
				}
			}
		}

		// Token: 0x06004B47 RID: 19271 RVA: 0x00296EE8 File Offset: 0x002950E8
		private void ReadClientList(IReadMessage inc)
		{
			bool refreshCampaignUI = false;
			ushort listId = inc.ReadUInt16();
			GameMain.NetLobbyScreen.Team1Count = (int)inc.ReadByte();
			GameMain.NetLobbyScreen.Team2Count = (int)inc.ReadByte();
			List<TempClient> tempClients = new List<TempClient>();
			int clientCount = (int)inc.ReadByte();
			for (int i = 0; i < clientCount; i++)
			{
				tempClients.Add(INetSerializableStruct.Read<TempClient>(inc));
				inc.ReadPadBits();
			}
			if (NetIdUtils.IdMoreRecent(listId, base.LastClientListUpdateID))
			{
				bool updateClientListId = true;
				List<Client> currentClients = new List<Client>();
				using (List<TempClient>.Enumerator enumerator = tempClients.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						TempClient tc = enumerator.Current;
						Client existingClient = this.ConnectedClients.Find((Client c) => c.SessionId == tc.SessionId && c.Name == tc.Name);
						if (existingClient == null)
						{
							existingClient = new Client(tc.Name, tc.SessionId)
							{
								AccountInfo = tc.AccountInfo,
								Muted = tc.Muted,
								InGame = tc.InGame,
								IsOwner = tc.IsOwner
							};
							this.otherClients.Add(existingClient);
							refreshCampaignUI = true;
							GameMain.NetLobbyScreen.AddPlayer(existingClient);
						}
						existingClient.NameId = tc.NameId;
						existingClient.PreferredJob = tc.PreferredJob;
						existingClient.PreferredTeam = tc.PreferredTeam;
						existingClient.TeamID = tc.TeamID;
						existingClient.Character = null;
						existingClient.Karma = tc.Karma;
						existingClient.Muted = tc.Muted;
						existingClient.InGame = tc.InGame;
						existingClient.IsOwner = tc.IsOwner;
						existingClient.IsDownloading = tc.IsDownloading;
						GameMain.NetLobbyScreen.SetPlayerNameAndJobPreference(existingClient);
						if (Screen.Selected != GameMain.NetLobbyScreen && tc.CharacterId > 0)
						{
							existingClient.CharacterID = tc.CharacterId;
						}
						if (existingClient.SessionId == this.SessionId)
						{
							MultiplayerPreferences.Instance.TeamPreference = existingClient.PreferredTeam;
							if (MultiplayerPreferences.Instance.TeamPreference != CharacterTeamType.None)
							{
								GUIListBox teamPreferenceListBox = GameMain.NetLobbyScreen.TeamPreferenceListBox;
								if (teamPreferenceListBox != null)
								{
									teamPreferenceListBox.Select(MultiplayerPreferences.Instance.TeamPreference, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
								}
							}
							else
							{
								GameMain.NetLobbyScreen.RefreshPvpTeamSelectionButtons();
							}
							existingClient.SetPermissions(this.permissions, this.permittedConsoleCommands);
							if (!NetIdUtils.IdMoreRecent(this.nameId, tc.NameId))
							{
								this.Name = tc.Name;
								this.nameId = tc.NameId;
							}
							GUITextBox characterNameBox = GameMain.NetLobbyScreen.CharacterNameBox;
							if (characterNameBox != null && !characterNameBox.Selected && characterNameBox.Enabled)
							{
								GameMain.NetLobbyScreen.CharacterNameBox.Text = this.Name;
							}
						}
						currentClients.Add(existingClient);
					}
				}
				for (int j = this.ConnectedClients.Count - 1; j >= 0; j--)
				{
					if (!currentClients.Contains(this.ConnectedClients[j]))
					{
						GameMain.NetLobbyScreen.RemovePlayer(this.ConnectedClients[j]);
						this.otherClients[j].Dispose();
						this.otherClients.RemoveAt(j);
						refreshCampaignUI = true;
					}
				}
				using (IEnumerator<Client> enumerator2 = this.ConnectedClients.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Client client = enumerator2.Current;
						int index = this.previouslyConnectedClients.FindIndex((Client c) => c.SessionId == client.SessionId);
						if (index < 0)
						{
							if (this.previouslyConnectedClients.Count > 100)
							{
								this.previouslyConnectedClients.RemoveRange(0, this.previouslyConnectedClients.Count - 100);
							}
						}
						else
						{
							this.previouslyConnectedClients.RemoveAt(index);
						}
						this.previouslyConnectedClients.Add(client);
					}
				}
				if (updateClientListId)
				{
					base.LastClientListUpdateID = listId;
				}
				if (this.ClientPeer is P2POwnerPeer)
				{
					EosSessionManager.UpdateOwnedSession(this.ClientPeer.ServerConnection.Endpoint, base.ServerSettings);
					TaskPool.Add("WaitForPingDataAsync (owner)", SteamNetworkingUtils.WaitForPingDataAsync(300f), delegate(Task task)
					{
						SteamManager.UpdateLobby(base.ServerSettings);
					});
					SteamManager.UpdateLobby(base.ServerSettings);
				}
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				if (netLobbyScreen != null)
				{
					netLobbyScreen.UpdateDisembarkPointListFromServerSettings();
				}
			}
			if (refreshCampaignUI)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign != null)
				{
					CampaignUI campaignUI = campaign.CampaignUI;
					if (campaignUI != null)
					{
						UpgradeStore upgradeStore = campaignUI.UpgradeStore;
						if (upgradeStore != null)
						{
							upgradeStore.RequestRefresh(false);
						}
					}
					CampaignUI campaignUI2 = campaign.CampaignUI;
					if (campaignUI2 == null)
					{
						return;
					}
					HRManagerUI hrmanagerUI = campaignUI2.HRManagerUI;
					if (hrmanagerUI == null)
					{
						return;
					}
					hrmanagerUI.RefreshUI();
				}
			}
		}

		// Token: 0x06004B48 RID: 19272 RVA: 0x00297434 File Offset: 0x00295634
		private void ReadLobbyUpdate(IReadMessage inc)
		{
			SegmentTableReader<ServerNetSegment>.Read(inc2, delegate(ServerNetSegment segment, [Nullable(1)] IReadMessage inc)
			{
				switch (segment)
				{
				case ServerNetSegment.SyncIds:
				{
					bool lobbyUpdated = inc.ReadBoolean();
					inc.ReadPadBits();
					if (lobbyUpdated)
					{
						ServerSettings.SuppressNetworkMessages = true;
						IKeyboardSubscriber prevDispatcher = GUI.KeyboardDispatcher.Subscriber;
						ushort updateID = inc.ReadUInt16();
						ushort settingsLen = inc.ReadUInt16();
						byte[] settingsData = inc.ReadBytes((int)settingsLen);
						bool isInitialUpdate = inc.ReadBoolean();
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Received ");
						defaultInterpolatedStringHandler.AppendFormatted(isInitialUpdate ? "initial" : string.Empty);
						defaultInterpolatedStringHandler.AppendLiteral(" lobby update ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<ushort>(updateID);
						defaultInterpolatedStringHandler.AppendLiteral(", last ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<ushort>(GameMain.NetLobbyScreen.LastUpdateID);
						defaultInterpolatedStringHandler.AppendLiteral(".");
						DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
						if (isInitialUpdate)
						{
							this.ReadInitialUpdate(inc);
							this.initialUpdateReceived = true;
						}
						string selectSubName = inc.ReadString();
						string selectSubHash = inc.ReadString();
						bool usingEnemySub = inc.ReadBoolean();
						string selectEnemySubName = selectSubName;
						string selectEnemySubHash = selectSubHash;
						if (usingEnemySub)
						{
							selectEnemySubName = inc.ReadString();
							selectEnemySubHash = inc.ReadString();
						}
						bool usingShuttle = inc.ReadBoolean();
						string selectShuttleName = inc.ReadString();
						string selectShuttleHash = inc.ReadString();
						bool allowSubVoting = inc.ReadBoolean();
						bool allowModeVoting = inc.ReadBoolean();
						bool voiceChatEnabled = inc.ReadBoolean();
						bool allowSpectating = inc.ReadBoolean();
						bool allowAFK = inc.ReadBoolean();
						float traitorProbability = inc.ReadSingle();
						int traitorDangerLevel = inc.ReadRangedInteger(1, 3);
						List<Identifier> missionTypes = new List<Identifier>();
						uint missionTypeCount = inc.ReadVariableUInt32();
						int i = 0;
						while ((long)i < (long)((ulong)missionTypeCount))
						{
							missionTypes.Add(inc.ReadIdentifier());
							i++;
						}
						int modeIndex = (int)inc.ReadByte();
						string levelSeed = inc.ReadString();
						float levelDifficulty = inc.ReadSingle();
						byte botCount = inc.ReadByte();
						BotSpawnMode botSpawnMode = inc.ReadBoolean() ? BotSpawnMode.Fill : BotSpawnMode.Normal;
						bool autoRestartEnabled = inc.ReadBoolean();
						float autoRestartTimer = autoRestartEnabled ? inc.ReadSingle() : 0f;
						if (NetIdUtils.IdMoreRecent(updateID, GameMain.NetLobbyScreen.LastUpdateID) && (isInitialUpdate || this.initialUpdateReceived))
						{
							ReadWriteMessage settingsBuf = new ReadWriteMessage();
							settingsBuf.WriteBytes(settingsData, 0, (int)settingsLen);
							settingsBuf.BitPosition = 0;
							base.ServerSettings.ClientRead(settingsBuf);
							if (!this.IsServerOwner)
							{
								ServerInfo info = this.CreateServerInfoFromSettings();
								GameMain.ServerListScreen.AddToRecentServers(info);
								GameMain.NetLobbyScreen.Favorite.Visible = true;
								GameMain.NetLobbyScreen.Favorite.Selected = GameMain.ServerListScreen.IsFavorite(info);
							}
							else
							{
								GameMain.NetLobbyScreen.Favorite.Visible = false;
							}
							GameMain.NetLobbyScreen.LastUpdateID = updateID;
							base.ServerSettings.ServerLog.ServerName = base.ServerSettings.ServerName;
							GameMain.NetLobbyScreen.UsingShuttle = usingShuttle;
							if (!allowSubVoting || GameMain.NetLobbyScreen.SelectedSub == null)
							{
								GameMain.NetLobbyScreen.TrySelectSub(selectSubName, selectSubHash, SelectedSubType.Sub, GameMain.NetLobbyScreen.SubList, true);
								if (usingEnemySub)
								{
									GameMain.NetLobbyScreen.TrySelectSub(selectEnemySubName, selectEnemySubHash, SelectedSubType.EnemySub, GameMain.NetLobbyScreen.SubList, true);
								}
							}
							GameMain.NetLobbyScreen.TrySelectSub(selectShuttleName, selectShuttleHash, SelectedSubType.Shuttle, GameMain.NetLobbyScreen.ShuttleList.ListBox, true);
							GameMain.NetLobbyScreen.SetTraitorProbability(traitorProbability);
							GameMain.NetLobbyScreen.SetTraitorDangerLevel(traitorDangerLevel);
							GameMain.NetLobbyScreen.SetMissionTypes(missionTypes);
							GameMain.NetLobbyScreen.LevelSeed = levelSeed;
							GameMain.NetLobbyScreen.SelectMode(modeIndex);
							if (isInitialUpdate && GameMain.NetLobbyScreen.SelectedMode == GameModePreset.MultiPlayerCampaign && GameMain.Client.IsServerOwner)
							{
								this.RequestSelectMode(modeIndex);
							}
							this.TryRequestMissingCampaignSubs();
							GameMain.NetLobbyScreen.SetAllowSpectating(allowSpectating);
							GameMain.NetLobbyScreen.SetAllowAFK(allowAFK);
							GameMain.NetLobbyScreen.SetLevelDifficulty(levelDifficulty);
							GameMain.NetLobbyScreen.SetBotSpawnMode(botSpawnMode);
							GameMain.NetLobbyScreen.SetBotCount((int)botCount);
							GameMain.NetLobbyScreen.SetAutoRestart(autoRestartEnabled, autoRestartTimer);
							base.ServerSettings.VoiceChatEnabled = voiceChatEnabled;
							base.ServerSettings.AllowSubVoting = allowSubVoting;
							base.ServerSettings.AllowModeVoting = allowModeVoting;
							if (this.ClientPeer is P2POwnerPeer)
							{
								EosSessionManager.UpdateOwnedSession(this.ClientPeer.ServerConnection.Endpoint, base.ServerSettings);
								SteamManager.UpdateLobby(base.ServerSettings);
							}
							GUI.KeyboardDispatcher.Subscriber = prevDispatcher;
						}
					}
					bool campaignUpdated = inc.ReadBoolean();
					inc.ReadPadBits();
					if (campaignUpdated)
					{
						MultiPlayerCampaign.ClientRead(inc);
					}
					else if (GameMain.NetLobbyScreen.SelectedMode != GameModePreset.MultiPlayerCampaign)
					{
						GameMain.NetLobbyScreen.SetCampaignCharacterInfo(null);
					}
					this.lastSentChatMsgID = inc.ReadUInt16();
					ServerSettings.SuppressNetworkMessages = false;
					break;
				}
				case ServerNetSegment.ChatMessage:
					ChatMessage.ClientRead(inc);
					break;
				case ServerNetSegment.Vote:
					this.Voting.ClientRead(inc);
					break;
				case ServerNetSegment.ClientList:
					this.ReadClientList(inc);
					break;
				}
				return SegmentTableReader<ServerNetSegment>.BreakSegmentReading.No;
			}, null);
		}

		// Token: 0x06004B49 RID: 19273 RVA: 0x0029744C File Offset: 0x0029564C
		private void ReadIngameUpdate(IReadMessage inc)
		{
			this.debugEntityList.Clear();
			float sendingTime = inc2.ReadSingle() - 0f;
			this.EndRoundTimeRemaining = inc2.ReadSingle();
			SegmentTableReader<ServerNetSegment>.Read(inc2, delegate(ServerNetSegment segment, [Nullable(1)] IReadMessage inc)
			{
				switch (segment)
				{
				case ServerNetSegment.SyncIds:
				{
					this.lastSentChatMsgID = inc.ReadUInt16();
					this.LastSentEntityEventID = inc.ReadUInt16();
					bool campaignUpdated = inc.ReadBoolean();
					inc.ReadPadBits();
					if (campaignUpdated)
					{
						MultiPlayerCampaign.ClientRead(inc);
						return SegmentTableReader<ServerNetSegment>.BreakSegmentReading.No;
					}
					if (GameMain.NetLobbyScreen.SelectedMode != GameModePreset.MultiPlayerCampaign)
					{
						GameMain.NetLobbyScreen.SetCampaignCharacterInfo(null);
						return SegmentTableReader<ServerNetSegment>.BreakSegmentReading.No;
					}
					return SegmentTableReader<ServerNetSegment>.BreakSegmentReading.No;
				}
				case ServerNetSegment.ChatMessage:
					ChatMessage.ClientRead(inc);
					return SegmentTableReader<ServerNetSegment>.BreakSegmentReading.No;
				case ServerNetSegment.ClientList:
					this.ReadClientList(inc);
					return SegmentTableReader<ServerNetSegment>.BreakSegmentReading.No;
				case ServerNetSegment.EntityPosition:
				{
					inc.ReadPadBits();
					uint msgLength = inc.ReadVariableUInt32();
					int msgEndPos = (int)((long)inc.BitPosition + (long)((ulong)(msgLength * 8U)));
					EntityPositionHeader header = INetSerializableStruct.Read<EntityPositionHeader>(inc);
					IServerPositionSync entity = Entity.FindEntityByID(header.EntityId) as IServerPositionSync;
					if (msgEndPos > inc.LengthBits)
					{
						DebugConsole.ThrowError("Error while reading a position update for the entity \"(" + (((entity != null) ? entity.ToString() : null) ?? "null") + ")\". Message length exceeds the size of the buffer.", null, null, false, false);
						return SegmentTableReader<ServerNetSegment>.BreakSegmentReading.Yes;
					}
					this.debugEntityList.Add(entity);
					if (entity != null)
					{
						if (entity is Item != header.IsItem)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(146, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Received a potentially invalid ENTITY_POSITION message. Entity type does not match (server entity is ");
							defaultInterpolatedStringHandler.AppendFormatted(header.IsItem ? "an item" : "not an item");
							defaultInterpolatedStringHandler.AppendLiteral(", client entity is ");
							defaultInterpolatedStringHandler.AppendFormatted(((entity != null) ? entity.GetType().ToString() : null) ?? "null");
							defaultInterpolatedStringHandler.AppendLiteral("). Ignoring the message...");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						else
						{
							MapEntity me = entity as MapEntity;
							if (me != null)
							{
								MapEntityPrefab prefab = me.Prefab;
								if (prefab != null)
								{
									uint uintIdentifier = prefab.UintIdentifier;
									if (uintIdentifier != header.PrefabUintIdentifier)
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(151, 2);
										defaultInterpolatedStringHandler2.AppendLiteral("Received a potentially invalid ENTITY_POSITION message.");
										defaultInterpolatedStringHandler2.AppendLiteral("Entity identifier does not match (server entity is ");
										MapEntityPrefab mapEntityPrefab = MapEntityPrefab.List.FirstOrDefault((MapEntityPrefab p) => p.UintIdentifier == header.PrefabUintIdentifier);
										defaultInterpolatedStringHandler2.AppendFormatted(((mapEntityPrefab != null) ? mapEntityPrefab.Identifier.Value : null) ?? "[not found]");
										defaultInterpolatedStringHandler2.AppendLiteral(", ");
										defaultInterpolatedStringHandler2.AppendLiteral("client entity is ");
										defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(me.Prefab.Identifier);
										defaultInterpolatedStringHandler2.AppendLiteral("). Ignoring the message...");
										DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
										goto IL_2AA;
									}
								}
							}
							entity.ClientReadPosition(inc, sendingTime);
						}
					}
					IL_2AA:
					inc.BitPosition = msgEndPos;
					inc.ReadPadBits();
					return SegmentTableReader<ServerNetSegment>.BreakSegmentReading.No;
				}
				case ServerNetSegment.EntityEvent:
				case ServerNetSegment.EntityEventInitial:
					if (!this.EntityEventManager.Read(segment, inc, sendingTime))
					{
						return SegmentTableReader<ServerNetSegment>.BreakSegmentReading.Yes;
					}
					return SegmentTableReader<ServerNetSegment>.BreakSegmentReading.No;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(20, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Unknown segment \"");
				defaultInterpolatedStringHandler3.AppendFormatted<ServerNetSegment>(segment);
				defaultInterpolatedStringHandler3.AppendLiteral("\"!)");
				throw new Exception(defaultInterpolatedStringHandler3.ToStringAndClear());
			}, delegate(Segment<ServerNetSegment> segment, [Nullable(new byte[]
			{
				1,
				0
			})] Segment<ServerNetSegment>[] prevSegments, [Nullable(1)] Exception ex)
			{
				List<string> list = new List<string>();
				list.Add(ex.Message);
				list.Add(string.Concat(new string[]
				{
					"Message length: ",
					inc2.LengthBits.ToString(),
					" (",
					inc2.LengthBytes.ToString(),
					" bytes)"
				}));
				list.Add("Read position: " + inc2.BitPosition.ToString());
				List<string> list2 = list;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Segment with error: ");
				defaultInterpolatedStringHandler.AppendFormatted<Segment<ServerNetSegment>>(segment);
				list2.Add(defaultInterpolatedStringHandler.ToStringAndClear());
				List<string> errorLines = list;
				if (prevSegments.Any<Segment<ServerNetSegment>>())
				{
					errorLines.Add("Prev segments: " + string.Join<Segment<ServerNetSegment>>(", ", prevSegments));
					errorLines.Add(" ");
				}
				errorLines.Add(ex.StackTrace.CleanupStackTrace());
				errorLines.Add(" ");
				if (prevSegments.Concat(segment.ToEnumerable<Segment<ServerNetSegment>>()).Any(delegate(Segment<ServerNetSegment> s)
				{
					ServerNetSegment identifier = s.Identifier;
					return identifier - ServerNetSegment.EntityPosition <= 2;
				}))
				{
					foreach (IServerSerializable ent in this.debugEntityList)
					{
						if (ent == null)
						{
							errorLines.Add(" - NULL");
						}
						else
						{
							Entity e = ent as Entity;
							errorLines.Add(" - " + e.ToString());
						}
					}
				}
				errorLines.Add("Last console messages:");
				for (int i = DebugConsole.Messages.Count - 1; i > Math.Max(0, DebugConsole.Messages.Count - 20); i--)
				{
					errorLines.Add("[" + DebugConsole.Messages[i].Time + "] " + DebugConsole.Messages[i].Text);
				}
				GameAnalyticsManager.AddErrorEventOnce("GameClient.ReadInGameUpdate", GameAnalyticsManager.ErrorSeverity.Critical, string.Join("\n", errorLines));
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(69, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Exception thrown while reading a message of the type \"");
				defaultInterpolatedStringHandler2.AppendFormatted<ServerNetSegment>(segment.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\" at position ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(segment.Pointer);
				defaultInterpolatedStringHandler2.AppendLiteral(".");
				throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear() + (prevSegments.Any<Segment<ServerNetSegment>>() ? (" Previous segments: " + string.Join<Segment<ServerNetSegment>>(", ", prevSegments)) : ""), ex);
			});
		}

		// Token: 0x06004B4A RID: 19274 RVA: 0x002974C4 File Offset: 0x002956C4
		private void SendLobbyUpdate()
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(0);
			SegmentTableWriter<ClientNetSegment> segmentTable = SegmentTableWriter<ClientNetSegment>.StartWriting(outmsg);
			try
			{
				segmentTable.StartNewSegment(ClientNetSegment.SyncIds);
				outmsg.WriteUInt16(GameMain.NetLobbyScreen.LastUpdateID);
				outmsg.WriteUInt16(ChatMessage.LastID);
				outmsg.WriteUInt16(base.LastClientListUpdateID);
				outmsg.WriteBoolean(GameMain.NetLobbyScreen.AFKSelected);
				outmsg.WriteUInt16(this.nameId);
				outmsg.WriteString(this.Name);
				List<JobVariant> jobPreferences = GameMain.NetLobbyScreen.JobPreferences;
				if (jobPreferences.Count > 0)
				{
					outmsg.WriteIdentifier(jobPreferences[0].Prefab.Identifier);
				}
				else
				{
					outmsg.WriteIdentifier(Identifier.Empty);
				}
				outmsg.WriteByte((byte)MultiplayerPreferences.Instance.TeamPreference);
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
				if (campaign == null || campaign.LastSaveID == 0)
				{
					outmsg.WriteUInt16(0);
				}
				else
				{
					outmsg.WriteUInt16(campaign.LastSaveID);
					outmsg.WriteByte(campaign.CampaignID);
					foreach (object obj in Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)))
					{
						MultiPlayerCampaign.NetFlags netFlag = (MultiPlayerCampaign.NetFlags)obj;
						outmsg.WriteUInt16(campaign.GetLastUpdateIdForFlag(netFlag));
					}
					outmsg.WriteBoolean(GameMain.NetLobbyScreen.CampaignCharacterDiscarded);
				}
				this.chatMsgQueue.RemoveAll((ChatMessage cMsg) => !NetIdUtils.IdMoreRecent(cMsg.NetStateID, this.lastSentChatMsgID));
				int i = 0;
				while (i < this.chatMsgQueue.Count && i < 10)
				{
					if (outmsg.LengthBytes + this.chatMsgQueue[i].EstimateLengthBytesClient() > 1165)
					{
						break;
					}
					this.chatMsgQueue[i].ClientWrite(segmentTable, outmsg);
					i++;
				}
			}
			finally
			{
				segmentTable.Dispose();
			}
			if (outmsg.LengthBytes > 1170)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Maximum packet size exceeded (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(outmsg.LengthBytes);
				defaultInterpolatedStringHandler.AppendLiteral(" > ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(1170);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			this.ClientPeer.Send(outmsg, DeliveryMethod.Unreliable, true);
		}

		// Token: 0x06004B4B RID: 19275 RVA: 0x0029774C File Offset: 0x0029594C
		private void SendIngameUpdate()
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(1);
			outmsg.WriteBoolean(this.EntityEventManager.MidRoundSyncingDone);
			outmsg.WritePadBits();
			SegmentTableWriter<ClientNetSegment> segmentTable = SegmentTableWriter<ClientNetSegment>.StartWriting(outmsg);
			try
			{
				segmentTable.StartNewSegment(ClientNetSegment.SyncIds);
				outmsg.WriteUInt16(ChatMessage.LastID);
				outmsg.WriteUInt16(this.SpoofEntityManagerReceivedId ? 1 : this.EntityEventManager.LastReceivedID);
				outmsg.WriteUInt16(base.LastClientListUpdateID);
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
				if (campaign == null || campaign.LastSaveID == 0)
				{
					outmsg.WriteUInt16(0);
				}
				else
				{
					outmsg.WriteUInt16(campaign.LastSaveID);
					outmsg.WriteByte(campaign.CampaignID);
					foreach (object obj in Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)))
					{
						MultiPlayerCampaign.NetFlags flag = (MultiPlayerCampaign.NetFlags)obj;
						outmsg.WriteUInt16(campaign.GetLastUpdateIdForFlag(flag));
					}
					outmsg.WriteBoolean(GameMain.NetLobbyScreen.CampaignCharacterDiscarded);
				}
				Character controlled = Character.Controlled;
				if (controlled != null)
				{
					controlled.ClientWriteInput(segmentTable, outmsg);
				}
				Camera cam = GameMain.GameScreen.Cam;
				if (cam != null)
				{
					cam.ClientWrite(segmentTable, outmsg);
				}
				ClientEntityEventManager entityEventManager = this.EntityEventManager;
				IWriteMessage msg = outmsg;
				ClientPeer clientPeer = this.ClientPeer;
				entityEventManager.Write(segmentTable, msg, (clientPeer != null) ? clientPeer.ServerConnection : null);
				this.chatMsgQueue.RemoveAll((ChatMessage cMsg) => !NetIdUtils.IdMoreRecent(cMsg.NetStateID, this.lastSentChatMsgID));
				int i = 0;
				while (i < this.chatMsgQueue.Count && i < 10)
				{
					if (outmsg.LengthBytes + this.chatMsgQueue[i].EstimateLengthBytesClient() > 1165)
					{
						break;
					}
					this.chatMsgQueue[i].ClientWrite(segmentTable, outmsg);
					i++;
				}
			}
			finally
			{
				segmentTable.Dispose();
			}
			if (outmsg.LengthBytes > 1170)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Maximum packet size exceeded (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(outmsg.LengthBytes);
				defaultInterpolatedStringHandler.AppendLiteral(" > ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(1170);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			this.ClientPeer.Send(outmsg, DeliveryMethod.Unreliable, true);
		}

		// Token: 0x06004B4C RID: 19276 RVA: 0x002979CC File Offset: 0x00295BCC
		public void SendChatMessage(ChatMessage msg)
		{
			ClientPeer clientPeer = this.ClientPeer;
			if (((clientPeer != null) ? clientPeer.ServerConnection : null) == null)
			{
				return;
			}
			this.lastQueueChatMsgID += 1;
			msg.NetStateID = this.lastQueueChatMsgID;
			this.chatMsgQueue.Add(msg);
		}

		// Token: 0x06004B4D RID: 19277 RVA: 0x00297A0C File Offset: 0x00295C0C
		public void SendChatMessage(string message, ChatMessageType type = ChatMessageType.Default)
		{
			ClientPeer clientPeer = this.ClientPeer;
			if (((clientPeer != null) ? clientPeer.ServerConnection : null) == null)
			{
				return;
			}
			ChatMessage chatMessage = ChatMessage.Create((base.GameStarted && this.myCharacter != null) ? this.myCharacter.Name : this.Name, message, type, (base.GameStarted && this.myCharacter != null) ? this.myCharacter : null, null, PlayerConnectionChangeType.None, null);
			chatMessage.ChatMode = GameMain.ActiveChatMode;
			this.lastQueueChatMsgID += 1;
			chatMessage.NetStateID = this.lastQueueChatMsgID;
			this.chatMsgQueue.Add(chatMessage);
		}

		// Token: 0x06004B4E RID: 19278 RVA: 0x00297AB0 File Offset: 0x00295CB0
		public void SendRespawnPromptResponse(bool waitForNextRoundRespawn)
		{
			this.WaitForNextRoundRespawn = new bool?(waitForNextRoundRespawn);
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(23);
			msg.WriteBoolean(GameMain.NetLobbyScreen.Spectating);
			msg.WriteBoolean(waitForNextRoundRespawn);
			ClientPeer clientPeer = this.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B4F RID: 19279 RVA: 0x00297B04 File Offset: 0x00295D04
		public void SendTakeOverBotRequest(CharacterInfo bot)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(24);
			msg.WriteUInt16(bot.ID);
			ClientPeer clientPeer = this.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B50 RID: 19280 RVA: 0x00297B40 File Offset: 0x00295D40
		public void ToggleReserveBench(CharacterInfo bot, bool pendingHire = false)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(25);
			msg.WriteUInt16(bot.ID);
			msg.WriteBoolean(pendingHire);
			ClientPeer clientPeer = this.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B51 RID: 19281 RVA: 0x00297B84 File Offset: 0x00295D84
		private void TryRequestMissingCampaignSubs()
		{
			if (GameMain.NetLobbyScreen.SelectedMode == GameModePreset.MultiPlayerCampaign)
			{
				foreach (SubmarineInfo sub in from s in this.ServerSubmarines
				where !base.ServerSettings.HiddenSubs.Contains(s.Name)
				select s)
				{
					GameMain.NetLobbyScreen.CheckIfCampaignSubMatches(sub, NetLobbyScreen.SubmarineDeliveryData.Campaign);
				}
			}
		}

		// Token: 0x06004B52 RID: 19282 RVA: 0x00297BFC File Offset: 0x00295DFC
		public void RequestFile(FileTransferType fileType, string file, string fileHash)
		{
			string message;
			if (fileType != FileTransferType.CampaignSave)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Sending a file request to the server (type: ");
				defaultInterpolatedStringHandler.AppendFormatted<FileTransferType>(fileType);
				defaultInterpolatedStringHandler.AppendLiteral(", path: ");
				defaultInterpolatedStringHandler.AppendFormatted(file ?? "null");
				message = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else
			{
				message = "Sending a campaign file request to the server.";
			}
			DebugConsole.Log(message);
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(5);
			msg.WriteByte(1);
			msg.WriteByte((byte)fileType);
			if (fileType != FileTransferType.CampaignSave)
			{
				IWriteMessage writeMessage = msg;
				if (file == null)
				{
					throw new ArgumentNullException("file");
				}
				writeMessage.WriteString(file);
				IWriteMessage writeMessage2 = msg;
				if (fileHash == null)
				{
					throw new ArgumentNullException("fileHash");
				}
				writeMessage2.WriteString(fileHash);
			}
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B53 RID: 19283 RVA: 0x00297CB5 File Offset: 0x00295EB5
		public void CancelFileTransfer(FileReceiver.FileTransferIn transfer)
		{
			this.CancelFileTransfer(transfer.ID);
		}

		// Token: 0x06004B54 RID: 19284 RVA: 0x00297CC4 File Offset: 0x00295EC4
		public void UpdateFileTransfer(FileReceiver.FileTransferIn transfer, int expecting, int lastSeen, bool reliable = false)
		{
			if (!reliable && (DateTime.Now - transfer.LastOffsetAckTime).TotalSeconds < 1.0)
			{
				return;
			}
			transfer.RecordOffsetAckTime();
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(5);
			msg.WriteByte(2);
			msg.WriteByte((byte)transfer.ID);
			msg.WriteInt32(expecting);
			msg.WriteInt32(lastSeen);
			this.ClientPeer.Send(msg, reliable ? DeliveryMethod.Reliable : DeliveryMethod.Unreliable, true);
		}

		// Token: 0x06004B55 RID: 19285 RVA: 0x00297D44 File Offset: 0x00295F44
		public void CancelFileTransfer(int id)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(5);
			msg.WriteByte(4);
			msg.WriteByte((byte)id);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B56 RID: 19286 RVA: 0x00297D7C File Offset: 0x00295F7C
		private void OnFileReceived(FileReceiver.FileTransferIn transfer)
		{
			switch (transfer.FileType)
			{
			case FileTransferType.Submarine:
			{
				SubmarineInfo newSub = new SubmarineInfo(transfer.FilePath, "", null, true, false);
				if (newSub.IsFileCorrupted)
				{
					return;
				}
				List<SubmarineInfo> existingSubs = (from s in SubmarineInfo.SavedSubmarines
				where s.Name == newSub.Name && s.MD5Hash == newSub.MD5Hash
				select s).ToList<SubmarineInfo>();
				foreach (SubmarineInfo existingSub in existingSubs)
				{
					existingSub.Dispose();
				}
				SubmarineInfo.AddToSavedSubs(newSub);
				Func<GUIComponent, bool> <>9__4;
				for (int i = 0; i < 2; i++)
				{
					IEnumerable<GUIComponent> subListChildren = (i == 0) ? GameMain.NetLobbyScreen.ShuttleList.ListBox.Content.Children : GameMain.NetLobbyScreen.SubList.Content.Children;
					IEnumerable<GUIComponent> source = subListChildren;
					Func<GUIComponent, bool> predicate;
					if ((predicate = <>9__4) == null)
					{
						predicate = (<>9__4 = ((GUIComponent c) => ((SubmarineInfo)c.UserData).Name == newSub.Name && ((SubmarineInfo)c.UserData).MD5Hash.StringRepresentation == newSub.MD5Hash.StringRepresentation));
					}
					GUIComponent subElement = source.FirstOrDefault(predicate);
					if (subElement != null)
					{
						GUITextBlock nameTextBlock = subElement.FindChild("nametext", true) as GUITextBlock;
						if (nameTextBlock != null)
						{
							nameTextBlock.TextColor = new Color(nameTextBlock.TextColor, 1f);
						}
						GUITextBlock classTextBlock = subElement.FindChild("classtext", true) as GUITextBlock;
						if (classTextBlock != null)
						{
							GUITextBlock guitextBlock = classTextBlock;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
							defaultInterpolatedStringHandler.AppendLiteral("submarineclass.");
							defaultInterpolatedStringHandler.AppendFormatted<SubmarineClass>(newSub.SubmarineClass);
							guitextBlock.Text = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
							classTextBlock.TextColor = new Color(classTextBlock.TextColor, 0.8f);
						}
						GUITextBlock priceTextBlock = subElement.FindChild("pricetext", true) as GUITextBlock;
						if (priceTextBlock != null)
						{
							priceTextBlock.Text = TextManager.GetWithVariable("currencyformat", "[credits]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", newSub.Price), FormatCapitals.No);
							priceTextBlock.TextColor = new Color(priceTextBlock.TextColor, 0.8f);
						}
						subElement.UserData = newSub;
						subElement.ToolTip = newSub.Description;
					}
				}
				if (GameMain.NetLobbyScreen.FailedSelectedSub != null && GameMain.NetLobbyScreen.FailedSelectedSub.Value.Name == newSub.Name && GameMain.NetLobbyScreen.FailedSelectedSub.Value.Hash == newSub.MD5Hash.StringRepresentation)
				{
					GameMain.NetLobbyScreen.TrySelectSub(newSub.Name, newSub.MD5Hash.StringRepresentation, SelectedSubType.Sub, GameMain.NetLobbyScreen.SubList, true);
				}
				if (GameMain.NetLobbyScreen.FailedSelectedShuttle != null && GameMain.NetLobbyScreen.FailedSelectedShuttle.Value.Name == newSub.Name && GameMain.NetLobbyScreen.FailedSelectedShuttle.Value.Hash == newSub.MD5Hash.StringRepresentation)
				{
					GameMain.NetLobbyScreen.TrySelectSub(newSub.Name, newSub.MD5Hash.StringRepresentation, SelectedSubType.Shuttle, GameMain.NetLobbyScreen.ShuttleList.ListBox, true);
				}
				if (GameMain.NetLobbyScreen.SelectedMode == GameModePreset.PvP && GameMain.NetLobbyScreen.FailedSelectedEnemySub != null && GameMain.NetLobbyScreen.FailedSelectedEnemySub.Value.Name == newSub.Name && GameMain.NetLobbyScreen.FailedSelectedEnemySub.Value.Hash == newSub.MD5Hash.StringRepresentation)
				{
					GameMain.NetLobbyScreen.TrySelectSub(newSub.Name, newSub.MD5Hash.StringRepresentation, SelectedSubType.EnemySub, GameMain.NetLobbyScreen.SubList, true);
				}
				NetLobbyScreen.FailedSubInfo failedCampaignSub = GameMain.NetLobbyScreen.FailedCampaignSubs.Find((NetLobbyScreen.FailedSubInfo s) => s.Name == newSub.Name && s.Hash == newSub.MD5Hash.StringRepresentation);
				if (failedCampaignSub != default(NetLobbyScreen.FailedSubInfo))
				{
					GameMain.NetLobbyScreen.FailedCampaignSubs.Remove(failedCampaignSub);
				}
				NetLobbyScreen.FailedSubInfo failedOwnedSub = GameMain.NetLobbyScreen.FailedOwnedSubs.Find((NetLobbyScreen.FailedSubInfo s) => s.Name == newSub.Name && s.Hash == newSub.MD5Hash.StringRepresentation);
				if (failedOwnedSub != default(NetLobbyScreen.FailedSubInfo))
				{
					GameMain.NetLobbyScreen.FailedOwnedSubs.Remove(failedOwnedSub);
				}
				SubmarineInfo existingServerSub = this.ServerSubmarines.Find((SubmarineInfo s) => s.Name == newSub.Name && s.MD5Hash == newSub.MD5Hash);
				if (existingServerSub != null)
				{
					int existingIndex = this.ServerSubmarines.IndexOf(existingServerSub);
					this.ServerSubmarines[existingIndex] = newSub;
					existingServerSub.Dispose();
					return;
				}
				return;
			}
			case FileTransferType.CampaignSave:
			{
				XDocument xdocument = SaveUtil.DecompressSaveAndLoadGameSessionDoc(transfer.FilePath);
				XElement gameSessionDocRoot = (xdocument != null) ? xdocument.Root : null;
				byte campaignID = (byte)MathHelper.Clamp(gameSessionDocRoot.GetAttributeInt("campaignid", 0), 0, 255);
				GameSession gameSession = GameMain.GameSession;
				MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
				if (campaign == null || campaign.CampaignID != campaignID)
				{
					string savePath = transfer.FilePath;
					SubmarineInfo submarineInfo = null;
					Option.UnspecifiedNone none = Option.None;
					GameMain.GameSession = new GameSession(submarineInfo, none, CampaignDataPath.CreateRegular(savePath), GameModePreset.MultiPlayerCampaign, CampaignSettings.Empty, null, null);
					campaign = (MultiPlayerCampaign)GameMain.GameSession.GameMode;
					campaign.CampaignID = campaignID;
					GameMain.NetLobbyScreen.ToggleCampaignMode(true);
				}
				GameMain.GameSession.DataPath = CampaignDataPath.CreateRegular(transfer.FilePath);
				if (GameMain.GameSession.SubmarineInfo == null || campaign.Map == null)
				{
					string subPath = Path.Combine(new string[]
					{
						SaveUtil.TempPath,
						gameSessionDocRoot.GetAttributeString("submarine", "")
					}) + ".sub";
					GameMain.GameSession.SubmarineInfo = new SubmarineInfo(subPath, "", null, true, false);
				}
				campaign.LoadState(GameMain.GameSession.DataPath.LoadPath);
				GameSession gameSession2 = GameMain.GameSession;
				if (gameSession2 != null)
				{
					SubmarineInfo submarineInfo2 = gameSession2.SubmarineInfo;
					if (submarineInfo2 != null)
					{
						submarineInfo2.Reload();
					}
				}
				GameSession gameSession3 = GameMain.GameSession;
				if (gameSession3 != null)
				{
					SubmarineInfo submarineInfo3 = gameSession3.SubmarineInfo;
					if (submarineInfo3 != null)
					{
						submarineInfo3.CheckSubsLeftBehind(null);
					}
				}
				GameSession gameSession4 = GameMain.GameSession;
				bool flag2;
				if (gameSession4 == null)
				{
					flag2 = (null != null);
				}
				else
				{
					SubmarineInfo submarineInfo4 = gameSession4.SubmarineInfo;
					flag2 = (((submarineInfo4 != null) ? submarineInfo4.Name : null) != null);
				}
				if (flag2)
				{
					GameMain.NetLobbyScreen.TryDisplayCampaignSubmarine(GameMain.GameSession.SubmarineInfo);
				}
				campaign.LastSaveID = campaign.PendingSaveID;
				if (Screen.Selected == GameMain.NetLobbyScreen)
				{
					GameMain.NetLobbyScreen.SaveAppearance();
					GameMain.NetLobbyScreen.Select();
				}
				DebugConsole.Log("Campaign save received (" + GameMain.GameSession.DataPath.ToString() + "), save ID " + campaign.LastSaveID.ToString());
				using (IEnumerator enumerator2 = Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)).GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						object obj = enumerator2.Current;
						MultiPlayerCampaign.NetFlags flag = (MultiPlayerCampaign.NetFlags)obj;
						campaign.SetLastUpdateIdForFlag(flag, campaign.GetLastUpdateIdForFlag(flag) - 1);
					}
					return;
				}
				break;
			}
			case FileTransferType.Mod:
				break;
			default:
				return;
			}
			if (!(Screen.Selected is ModDownloadScreen))
			{
				return;
			}
			GameMain.ModDownloadScreen.CurrentDownloadFinished(transfer);
		}

		// Token: 0x06004B57 RID: 19287 RVA: 0x00298528 File Offset: 0x00296728
		private void OnTransferFailed(FileReceiver.FileTransferIn transfer)
		{
			if (transfer.FileType == FileTransferType.CampaignSave)
			{
				GameMain.Client.RequestFile(FileTransferType.CampaignSave, null, null);
			}
		}

		// Token: 0x06004B58 RID: 19288 RVA: 0x00298540 File Offset: 0x00296740
		public override void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData = null)
		{
			this.CreateEntityEvent(entity, extraData, true);
		}

		// Token: 0x06004B59 RID: 19289 RVA: 0x0029854C File Offset: 0x0029674C
		public void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData, bool requireControlledCharacter)
		{
			IClientSerializable clientSerializable = entity as IClientSerializable;
			if (clientSerializable == null)
			{
				throw new InvalidCastException("Entity is not IClientSerializable");
			}
			this.EntityEventManager.CreateEvent(clientSerializable, extraData, requireControlledCharacter);
		}

		// Token: 0x06004B5A RID: 19290 RVA: 0x0029857C File Offset: 0x0029677C
		public bool HasPermission(ClientPermissions permission)
		{
			return this.permissions.HasFlag(permission);
		}

		// Token: 0x06004B5B RID: 19291 RVA: 0x00298594 File Offset: 0x00296794
		public bool HasConsoleCommandPermission(Identifier commandName)
		{
			if (!this.permissions.HasFlag(ClientPermissions.ConsoleCommands))
			{
				return false;
			}
			if (this.permittedConsoleCommands.Contains(commandName))
			{
				return true;
			}
			foreach (DebugConsole.Command command in DebugConsole.Commands)
			{
				if (command.Names.Contains(commandName))
				{
					if (command.Names.Intersect(this.permittedConsoleCommands).Any<Identifier>())
					{
						return true;
					}
					break;
				}
			}
			return false;
		}

		// Token: 0x06004B5C RID: 19292 RVA: 0x00298640 File Offset: 0x00296840
		public void Quit()
		{
			ClientPeer clientPeer = this.ClientPeer;
			if (clientPeer != null)
			{
				clientPeer.Close(PeerDisconnectPacket.WithReason(DisconnectReason.Disconnected));
			}
			GUIMessageBox.MessageBoxes.RemoveAll((GUIComponent c) => ((c != null) ? c.UserData : null) is RoundSummary);
		}

		// Token: 0x06004B5D RID: 19293 RVA: 0x00298690 File Offset: 0x00296890
		public void SendCharacterInfo(string newName = null)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(14);
			this.WriteCharacterInfo(msg, newName);
			ClientPeer clientPeer = this.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B5E RID: 19294 RVA: 0x002986C8 File Offset: 0x002968C8
		public void WriteCharacterInfo(IWriteMessage msg, string newName = null)
		{
			msg.WriteBoolean(GameMain.NetLobbyScreen.Spectating);
			msg.WriteBoolean(GameMain.NetLobbyScreen.CampaignCharacterDiscarded);
			bool writeInfo = this.characterInfo != null;
			msg.WriteBoolean(writeInfo);
			msg.WritePadBits();
			if (!writeInfo)
			{
				return;
			}
			CharacterInfo.HeadInfo head = this.characterInfo.Head;
			string newName2 = newName ?? string.Empty;
			ImmutableArray<Identifier> tags = head.Preset.TagSet.ToImmutableArray<Identifier>();
			byte hairIndex = (byte)head.HairIndex;
			byte beardIndex = (byte)head.BeardIndex;
			byte moustacheIndex = (byte)head.MoustacheIndex;
			byte faceAttachmentIndex = (byte)head.FaceAttachmentIndex;
			Color skinColor = head.SkinColor;
			Color hairColor = head.HairColor;
			Color facialHairColor = head.FacialHairColor;
			IEnumerable<JobVariant> jobPreferences = GameMain.NetLobbyScreen.JobPreferences;
			Func<JobVariant, NetJobVariant> selector;
			if ((selector = GameClient.<>O.<1>__FromJobVariant) == null)
			{
				selector = (GameClient.<>O.<1>__FromJobVariant = new Func<JobVariant, NetJobVariant>(NetJobVariant.FromJobVariant));
			}
			NetCharacterInfo netInfo = new NetCharacterInfo(newName2, tags, hairIndex, beardIndex, moustacheIndex, faceAttachmentIndex, skinColor, hairColor, facialHairColor, jobPreferences.Select(selector).ToImmutableArray<NetJobVariant>());
			msg.WriteNetSerializableStruct(netInfo);
		}

		// Token: 0x06004B5F RID: 19295 RVA: 0x002987A4 File Offset: 0x002969A4
		public void Vote(VoteType voteType, object data)
		{
			if (this.ClientPeer == null)
			{
				return;
			}
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(0);
			using (SegmentTableWriter<ClientNetSegment> segmentTable = SegmentTableWriter<ClientNetSegment>.StartWriting(msg))
			{
				segmentTable.StartNewSegment(ClientNetSegment.Vote);
				if (!this.Voting.ClientWrite(msg, voteType, data))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to write vote of type ");
					defaultInterpolatedStringHandler.AppendFormatted<VoteType>(voteType);
					defaultInterpolatedStringHandler.AppendLiteral(": ");
					defaultInterpolatedStringHandler.AppendLiteral("data was of invalid type ");
					defaultInterpolatedStringHandler.AppendFormatted(((data != null) ? data.GetType().Name : null) ?? "NULL");
					throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B60 RID: 19296 RVA: 0x00298874 File Offset: 0x00296A74
		public void VoteForKick(Client votedClient)
		{
			if (votedClient == null)
			{
				return;
			}
			this.Vote(VoteType.Kick, votedClient);
		}

		// Token: 0x06004B61 RID: 19297 RVA: 0x00298882 File Offset: 0x00296A82
		public void InitiateSubmarineChange(SubmarineInfo sub, bool transferItems, VoteType voteType)
		{
			if (sub == null)
			{
				return;
			}
			this.Vote(voteType, new ValueTuple<SubmarineInfo, bool>(sub, transferItems));
		}

		// Token: 0x06004B62 RID: 19298 RVA: 0x0029889B File Offset: 0x00296A9B
		public void ShowSubmarineChangeVoteInterface(Client starter, SubmarineInfo info, VoteType type, bool transferItems, float timeOut)
		{
			if (info == null)
			{
				return;
			}
			if (this.votingInterface != null && this.votingInterface.VoteRunning)
			{
				return;
			}
			VotingInterface votingInterface = this.votingInterface;
			if (votingInterface != null)
			{
				votingInterface.Remove();
			}
			this.votingInterface = VotingInterface.CreateSubmarineVotingInterface(starter, info, type, transferItems, timeOut);
		}

		// Token: 0x06004B63 RID: 19299 RVA: 0x002988DC File Offset: 0x00296ADC
		public void ShowMoneyTransferVoteInterface(Client starter, Client from, int amount, Client to, float timeOut)
		{
			if (this.votingInterface != null && this.votingInterface.VoteRunning)
			{
				return;
			}
			if (from == null && to == null)
			{
				DebugConsole.ThrowError("Tried to initiate a vote for transferring from null to null!", null, null, false, false);
				return;
			}
			VotingInterface votingInterface = this.votingInterface;
			if (votingInterface != null)
			{
				votingInterface.Remove();
			}
			this.votingInterface = VotingInterface.CreateMoneyTransferVotingInterface(starter, from, to, amount, timeOut);
		}

		// Token: 0x06004B64 RID: 19300 RVA: 0x00298938 File Offset: 0x00296B38
		public override void AddChatMessage(ChatMessage message)
		{
			bool? should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventChatMessage>(delegate(IEventChatMessage x)
			{
				bool? flag = x.OnChatMessage(message.Text, message.SenderClient, message.Type, message);
				should = ((flag != null) ? flag : should);
			});
			if (should != null && should.Value)
			{
				return;
			}
			if (string.IsNullOrEmpty(message.Text))
			{
				return;
			}
			Character sender = message.SenderCharacter;
			if (sender != null && !sender.IsDead)
			{
				if (message.Text.IsNullOrEmpty())
				{
					sender.ShowTextlessSpeechBubble(2f, message.Color);
				}
				else
				{
					sender.ShowSpeechBubble(message.Color, message.Text);
					if (!sender.IsBot)
					{
						sender.TextChatVolume = 1f;
					}
				}
			}
			GameMain.NetLobbyScreen.NewChatMessage(message);
			this.chatBox.AddMessage(message);
		}

		// Token: 0x06004B65 RID: 19301 RVA: 0x00298A3C File Offset: 0x00296C3C
		public override void KickPlayer(string kickedName, string reason)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(2);
			msg.WriteString(kickedName);
			msg.WriteString(reason);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B66 RID: 19302 RVA: 0x00298A7C File Offset: 0x00296C7C
		public override void BanPlayer(string kickedName, string reason, TimeSpan? duration = null)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(4);
			msg.WriteString(kickedName);
			msg.WriteString(reason);
			msg.WriteDouble((duration != null) ? duration.Value.TotalSeconds : 0.0);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B67 RID: 19303 RVA: 0x00298AE4 File Offset: 0x00296CE4
		public override void UnbanPlayer(string playerName)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(8);
			msg.WriteBoolean(true);
			msg.WritePadBits();
			msg.WriteString(playerName);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B68 RID: 19304 RVA: 0x00298B28 File Offset: 0x00296D28
		public override void UnbanPlayer(Endpoint endpoint)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(8);
			msg.WriteBoolean(false);
			msg.WritePadBits();
			msg.WriteString(endpoint.StringRepresentation);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B69 RID: 19305 RVA: 0x00298B74 File Offset: 0x00296D74
		public void UpdateClientPermissions(Client targetClient)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(1024);
			targetClient.WritePermissions(msg);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B6A RID: 19306 RVA: 0x00298BB0 File Offset: 0x00296DB0
		public void SendCampaignState()
		{
			MultiPlayerCampaign campaign = GameMain.GameSession.GameMode as MultiPlayerCampaign;
			if (campaign == null)
			{
				DebugConsole.ThrowError("Failed send campaign state to the server (no campaign active).\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(64);
			campaign.ClientWrite(msg);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B6B RID: 19307 RVA: 0x00298C1C File Offset: 0x00296E1C
		public void SendConsoleCommand(string command)
		{
			if (string.IsNullOrWhiteSpace(command))
			{
				DebugConsole.ThrowError("Cannot send an empty console command to the server!\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(128);
			msg.WriteString(command);
			Vector2 cursorWorldPos = GameMain.GameScreen.Cam.ScreenToWorld(PlayerInput.MousePosition);
			msg.WriteSingle(cursorWorldPos.X);
			msg.WriteSingle(cursorWorldPos.Y);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B6C RID: 19308 RVA: 0x00298CAC File Offset: 0x00296EAC
		public void RequestStartRound(bool continueCampaign = false)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(1);
			msg.WriteBoolean(false);
			msg.WriteBoolean(continueCampaign);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B6D RID: 19309 RVA: 0x00298CEC File Offset: 0x00296EEC
		public void RequestSelectSub(SubmarineInfo sub, SelectedSubType type)
		{
			if (!this.HasPermission(ClientPermissions.SelectSub) || sub == null)
			{
				return;
			}
			if (ServerSettings.SuppressNetworkMessages)
			{
				return;
			}
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(16);
			msg.WriteByte((byte)type);
			msg.WriteString(sub.MD5Hash.StringRepresentation);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B6E RID: 19310 RVA: 0x00298D4C File Offset: 0x00296F4C
		public void RequestSelectMode(int modeIndex)
		{
			if (modeIndex < 0 || modeIndex >= GameMain.NetLobbyScreen.ModeList.Content.CountChildren)
			{
				DebugConsole.ThrowError("Gamemode index out of bounds (" + modeIndex.ToString() + ")\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(32);
			msg.WriteUInt16((ushort)modeIndex);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B6F RID: 19311 RVA: 0x00298DCC File Offset: 0x00296FCC
		public void SetupNewCampaign(SubmarineInfo sub, string saveName, string mapSeed, CampaignSettings settings)
		{
			GameMain.NetLobbyScreen.CampaignSetupFrame.Visible = false;
			GameMain.NetLobbyScreen.CampaignFrame.Visible = false;
			saveName = Path.GetFileNameWithoutExtension(saveName);
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(4);
			msg.WriteBoolean(true);
			msg.WritePadBits();
			msg.WriteString(saveName);
			msg.WriteString(mapSeed);
			msg.WriteString(sub.Name);
			msg.WriteString(sub.MD5Hash.StringRepresentation);
			msg.WriteNetSerializableStruct(settings);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B70 RID: 19312 RVA: 0x00298E5C File Offset: 0x0029705C
		public void SetupLoadCampaign(string filePath, Option<uint> backupIndex)
		{
			if (this.ClientPeer == null)
			{
				return;
			}
			GameMain.NetLobbyScreen.CampaignSetupFrame.Visible = false;
			GameMain.NetLobbyScreen.CampaignFrame.Visible = false;
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(4);
			msg.WriteBoolean(false);
			msg.WritePadBits();
			msg.WriteString(filePath);
			uint index;
			if (backupIndex.TryUnwrap(out index))
			{
				msg.WriteBoolean(true);
				msg.WritePadBits();
				msg.WriteUInt32(index);
			}
			else
			{
				msg.WriteBoolean(false);
				msg.WritePadBits();
			}
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B71 RID: 19313 RVA: 0x00298EF0 File Offset: 0x002970F0
		public void RequestEndRound(bool save, bool quitCampaign = false)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(10);
			msg.WriteUInt16(1);
			msg.WriteBoolean(true);
			msg.WriteBoolean(save);
			msg.WriteBoolean(quitCampaign);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B72 RID: 19314 RVA: 0x00298F38 File Offset: 0x00297138
		public void EndRoundForSelf()
		{
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				gameSession.EndRound(string.Empty, CampaignMode.TransitionType.None, null, false);
			}
			Submarine.Unload();
			GameMain.NetLobbyScreen.Select();
			Character.Controlled = null;
			this.WaitForNextRoundRespawn = null;
			base.RespawnManager = null;
			ClientEntityEventManager entityEventManager = this.EntityEventManager;
			if (entityEventManager != null)
			{
				entityEventManager.Clear();
			}
			this.LastSentEntityEventID = 0;
			this.MyClient.CharacterID = 0;
			this.roundInitStatus = GameClient.RoundInitStatus.NotStarted;
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(11);
			this.ClientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B73 RID: 19315 RVA: 0x00298FD8 File Offset: 0x002971D8
		public bool SendJoinOngoingRequest(GUIButton joinButton)
		{
			GameModePreset selectedMode = GameMain.NetLobbyScreen.SelectedMode;
			GameSession gameSession = GameMain.GameSession;
			MultiPlayerCampaign multiPlayerCampaign;
			if (selectedMode != ((gameSession != null) ? gameSession.GameMode.Preset : null))
			{
				multiPlayerCampaign = null;
			}
			else
			{
				GameSession gameSession2 = GameMain.GameSession;
				multiPlayerCampaign = (((gameSession2 != null) ? gameSession2.GameMode : null) as MultiPlayerCampaign);
			}
			MultiPlayerCampaign campaign = multiPlayerCampaign;
			if (this.FileReceiver.ActiveTransfers.Any((FileReceiver.FileTransferIn t) => t.FileType == FileTransferType.CampaignSave) || (campaign != null && NetIdUtils.IdMoreRecent(campaign.PendingSaveID, campaign.LastSaveID)))
			{
				new GUIMessageBox("", TextManager.Get("campaignfiletransferinprogress"), null, null, GUIMessageBox.Type.Default);
				return false;
			}
			if (joinButton != null)
			{
				joinButton.Enabled = false;
			}
			if (campaign != null)
			{
				this.LateCampaignJoin = true;
			}
			if (this.ClientPeer == null)
			{
				return false;
			}
			this.SendStartGameResponse(true);
			return false;
		}

		// Token: 0x06004B74 RID: 19316 RVA: 0x002990C0 File Offset: 0x002972C0
		private void SendStartGameResponse(bool readyToStart)
		{
			IWriteMessage readyToStartMsg = new WriteOnlyMessage();
			readyToStartMsg.WriteByte(9);
			readyToStartMsg.WriteBoolean(readyToStart);
			readyToStartMsg.WriteBoolean(GameMain.NetLobbyScreen.AFKSelected && base.ServerSettings.AllowAFK);
			this.WriteCharacterInfo(readyToStartMsg, null);
			this.ClientPeer.Send(readyToStartMsg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004B75 RID: 19317 RVA: 0x00299118 File Offset: 0x00297318
		public bool SetReadyToStart(GUITickBox tickBox)
		{
			if (base.GameStarted)
			{
				tickBox.Parent.Visible = false;
				return false;
			}
			this.Vote(VoteType.StartRound, tickBox.Selected);
			return true;
		}

		// Token: 0x06004B76 RID: 19318 RVA: 0x00299143 File Offset: 0x00297343
		public bool ToggleEndRoundVote(GUITickBox tickBox)
		{
			if (!base.GameStarted)
			{
				return false;
			}
			if (!base.ServerSettings.AllowEndVoting || !this.HasSpawned)
			{
				tickBox.Visible = false;
				return false;
			}
			this.Vote(VoteType.EndRound, tickBox.Selected);
			return false;
		}

		// Token: 0x17001341 RID: 4929
		// (get) Token: 0x06004B77 RID: 19319 RVA: 0x00299180 File Offset: 0x00297380
		// (set) Token: 0x06004B78 RID: 19320 RVA: 0x00299188 File Offset: 0x00297388
		public CharacterInfo CharacterInfo
		{
			get
			{
				return this.characterInfo;
			}
			set
			{
				this.characterInfo = value;
			}
		}

		// Token: 0x17001342 RID: 4930
		// (get) Token: 0x06004B79 RID: 19321 RVA: 0x00299191 File Offset: 0x00297391
		// (set) Token: 0x06004B7A RID: 19322 RVA: 0x00299199 File Offset: 0x00297399
		public Character Character
		{
			get
			{
				return this.myCharacter;
			}
			set
			{
				this.myCharacter = value;
			}
		}

		// Token: 0x06004B7B RID: 19323 RVA: 0x002991A2 File Offset: 0x002973A2
		public void UpdateLogButtonPermissions()
		{
			this.hasPermissionToUseLogButton = GameMain.Client.HasPermission(ClientPermissions.ServerLog);
			this.UpdateLogButtonVisibility();
		}

		// Token: 0x06004B7C RID: 19324 RVA: 0x002991C0 File Offset: 0x002973C0
		private void UpdateLogButtonVisibility()
		{
			if (this.ShowLogButton != null)
			{
				if (Screen.Selected != GameMain.GameScreen)
				{
					this.ShowLogButton.Visible = this.hasPermissionToUseLogButton;
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
				this.ShowLogButton.Visible = (this.hasPermissionToUseLogButton && (campaign == null || !campaign.ShowCampaignUI));
			}
		}

		// Token: 0x17001343 RID: 4931
		// (get) Token: 0x06004B7D RID: 19325 RVA: 0x0029922A File Offset: 0x0029742A
		public GUIFrame InGameHUD
		{
			get
			{
				return this.inGameHUD;
			}
		}

		// Token: 0x17001344 RID: 4932
		// (get) Token: 0x06004B7E RID: 19326 RVA: 0x00299232 File Offset: 0x00297432
		public ChatBox ChatBox
		{
			get
			{
				return this.chatBox;
			}
		}

		// Token: 0x17001345 RID: 4933
		// (get) Token: 0x06004B7F RID: 19327 RVA: 0x0029923A File Offset: 0x0029743A
		public VotingInterface VotingInterface
		{
			get
			{
				return this.votingInterface;
			}
		}

		// Token: 0x17001346 RID: 4934
		// (get) Token: 0x06004B80 RID: 19328 RVA: 0x00299242 File Offset: 0x00297442
		// (set) Token: 0x06004B81 RID: 19329 RVA: 0x0029924A File Offset: 0x0029744A
		public bool SpoofEntityManagerReceivedId { get; set; }

		// Token: 0x06004B82 RID: 19330 RVA: 0x00299253 File Offset: 0x00297453
		public bool TypingChatMessage(GUITextBox textBox, string text)
		{
			return this.chatBox.TypingChatMessage(textBox, text);
		}

		// Token: 0x06004B83 RID: 19331 RVA: 0x00299264 File Offset: 0x00297464
		public bool EnterChatMessage(GUITextBox textBox, string message)
		{
			ChatMessageType messageType = NetLobbyScreen.TeamChatSelected ? ChatMessageType.Team : ChatMessageType.Default;
			textBox.TextColor = ChatMessage.MessageColor[(int)messageType];
			if (string.IsNullOrWhiteSpace(message))
			{
				if (textBox == this.chatBox.InputBox)
				{
					textBox.Deselect();
				}
				return false;
			}
			this.chatBox.ChatManager.Store(message);
			this.SendChatMessage(message, messageType);
			if (textBox.DeselectAfterMessage)
			{
				textBox.Deselect();
			}
			textBox.Text = "";
			if (this.ChatBox.CloseAfterMessageSent)
			{
				this.ChatBox.ToggleOpen = false;
				this.ChatBox.CloseAfterMessageSent = false;
			}
			return true;
		}

		// Token: 0x06004B84 RID: 19332 RVA: 0x00299308 File Offset: 0x00297508
		public void AddToGUIUpdateList()
		{
			if (GUI.DisableHUD || GUI.DisableUpperHUD)
			{
				return;
			}
			if (base.GameStarted && Screen.Selected == GameMain.GameScreen)
			{
				this.inGameHUD.AddToGUIUpdateList(false, 0);
				GUIComponent fileTransferFrame = GameMain.NetLobbyScreen.FileTransferFrame;
				if (fileTransferFrame != null)
				{
					fileTransferFrame.AddToGUIUpdateList(false, 0);
				}
			}
			base.ServerSettings.AddToGUIUpdateList();
			if (base.ServerSettings.ServerLog.LogFrame != null)
			{
				base.ServerSettings.ServerLog.LogFrame.AddToGUIUpdateList(false, 0);
			}
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			if (netLobbyScreen == null)
			{
				return;
			}
			GUIButton playerFrame = netLobbyScreen.PlayerFrame;
			if (playerFrame == null)
			{
				return;
			}
			playerFrame.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x06004B85 RID: 19333 RVA: 0x002993AC File Offset: 0x002975AC
		public void UpdateHUD(float deltaTime)
		{
			GUITextBox msgBox = null;
			if (Screen.Selected == GameMain.GameScreen)
			{
				msgBox = this.chatBox.InputBox;
			}
			else if (Screen.Selected == GameMain.NetLobbyScreen)
			{
				msgBox = GameMain.NetLobbyScreen.ChatInput;
			}
			if (msgBox != null)
			{
				msgBox.Enabled = !this.IsBlockedBySpamFilter;
			}
			this.UpdateLogButtonVisibility();
			if (base.GameStarted && Screen.Selected == GameMain.GameScreen)
			{
				Character controlled = Character.Controlled;
				Barotrauma.Items.Components.Controller controller;
				if (controlled == null)
				{
					controller = null;
				}
				else
				{
					Item selectedItem = controlled.SelectedItem;
					controller = ((selectedItem != null) ? selectedItem.GetComponent<Barotrauma.Items.Components.Controller>() : null);
				}
				Barotrauma.Items.Components.Controller c = controller;
				bool flag;
				if (c == null || !c.HideHUD)
				{
					Character controlled2 = Character.Controlled;
					Barotrauma.Items.Components.Controller controller2;
					if (controlled2 == null)
					{
						controller2 = null;
					}
					else
					{
						Item selectedSecondaryItem = controlled2.SelectedSecondaryItem;
						controller2 = ((selectedSecondaryItem != null) ? selectedSecondaryItem.GetComponent<Barotrauma.Items.Components.Controller>() : null);
					}
					Barotrauma.Items.Components.Controller c2 = controller2;
					flag = (c2 != null && c2.HideHUD);
				}
				else
				{
					flag = true;
				}
				bool disableButtons = flag;
				this.buttonContainer.Visible = !disableButtons;
				if (!GUI.DisableHUD && !GUI.DisableUpperHUD)
				{
					this.inGameHUD.UpdateManually(deltaTime, false, true);
					this.chatBox.Update(deltaTime);
					if (this.votingInterface != null)
					{
						this.votingInterface.Update(deltaTime);
						if (!this.votingInterface.VoteRunning || this.votingInterface.TimedOut)
						{
							if (this.votingInterface.TimedOut)
							{
								DebugConsole.AddWarning("Voting interface timed out.", null);
							}
							this.votingInterface.Remove();
							this.votingInterface = null;
						}
					}
					this.cameraFollowsSub.Visible = (Character.Controlled == null);
				}
			}
			if (msgBox != null && GUI.KeyboardDispatcher.Subscriber == null)
			{
				ChatBox.ChatKeyStates chatKeyStates = ChatBox.ChatKeyStates.GetChatKeyStates();
				if (chatKeyStates.AnyHit)
				{
					if (msgBox.Selected)
					{
						msgBox.Text = "";
						msgBox.Deselect();
						return;
					}
					if (Screen.Selected == GameMain.GameScreen)
					{
						this.ChatBox.ApplySelectionInputs(msgBox, false, chatKeyStates);
					}
					msgBox.Select(msgBox.Text.Length, false);
				}
			}
		}

		// Token: 0x06004B86 RID: 19334 RVA: 0x00299584 File Offset: 0x00297784
		public void Draw(SpriteBatch spriteBatch)
		{
			if (GUI.DisableHUD || GUI.DisableUpperHUD)
			{
				return;
			}
			if (this.FileReceiver != null && this.FileReceiver.ActiveTransfers.Count > 0)
			{
				FileReceiver.FileTransferIn transfer = this.FileReceiver.ActiveTransfers.First<FileReceiver.FileTransferIn>();
				GameMain.NetLobbyScreen.FileTransferFrame.Visible = true;
				GameMain.NetLobbyScreen.FileTransferFrame.UserData = transfer;
				GameMain.NetLobbyScreen.FileTransferTitle.Text = ToolBox.LimitString(TextManager.GetWithVariable("DownloadingFile", "[filename]", transfer.FileName, FormatCapitals.No).Value, GameMain.NetLobbyScreen.FileTransferTitle.Font, GameMain.NetLobbyScreen.FileTransferTitle.Rect.Width);
				GameMain.NetLobbyScreen.FileTransferProgressBar.BarSize = transfer.Progress;
				GameMain.NetLobbyScreen.FileTransferProgressText.Text = MathUtils.GetBytesReadable((long)transfer.Received) + " / " + MathUtils.GetBytesReadable((long)transfer.FileSize);
			}
			else
			{
				GameMain.NetLobbyScreen.FileTransferFrame.Visible = false;
			}
			if (!base.GameStarted || Screen.Selected != GameMain.GameScreen)
			{
				return;
			}
			this.inGameHUD.DrawManually(spriteBatch, false, true);
			int endVoteCount = this.Voting.GetVoteCountYes(VoteType.EndRound);
			int endVoteMax = this.Voting.GetVoteCountMax(VoteType.EndRound);
			if (endVoteCount > 0)
			{
				if (this.EndVoteTickBox.Visible)
				{
					GUITickBox endVoteTickBox = this.EndVoteTickBox;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.endRoundVoteText);
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(endVoteCount);
					defaultInterpolatedStringHandler.AppendLiteral("/");
					defaultInterpolatedStringHandler.AppendFormatted<int>(endVoteMax);
					endVoteTickBox.Text = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					LocalizedString endVoteText = TextManager.GetWithVariables("EndRoundVotes", new ValueTuple<string, string>[]
					{
						new ValueTuple<string, string>("[votes]", endVoteCount.ToString()),
						new ValueTuple<string, string>("[max]", endVoteMax.ToString())
					});
					Vector2 pos = this.EndVoteTickBox.Rect.Center.ToVector2() - GUIStyle.SmallFont.MeasureString(endVoteText, false) / 2f;
					string value = endVoteText.Value;
					Color white = Color.White;
					GUIFont smallFont = GUIStyle.SmallFont;
					GUI.DrawString(spriteBatch, pos, value, white, null, 0, smallFont, ForceUpperCase.Inherit);
				}
			}
			else
			{
				this.EndVoteTickBox.Text = this.endRoundVoteText;
			}
			if (base.RespawnManager != null)
			{
				LocalizedString respawnText = string.Empty;
				Color textColor = Color.White;
				bool hideRespawnButtons = false;
				if (this.EndRoundTimeRemaining > 0f)
				{
					respawnText = TextManager.GetWithVariable("endinground", "[time]", ToolBox.SecondsToReadableTime(this.EndRoundTimeRemaining), FormatCapitals.No).Fallback(ToolBox.SecondsToReadableTime(this.EndRoundTimeRemaining), false);
				}
				if (base.RespawnManager.CurrentState == RespawnManager.State.Waiting)
				{
					if (base.RespawnManager.RespawnCountdownStarted)
					{
						float timeLeft = (float)(base.RespawnManager.RespawnTime - DateTime.Now).TotalSeconds;
						respawnText = TextManager.GetWithVariable("RespawningIn", "[time]", ToolBox.SecondsToReadableTime(timeLeft), FormatCapitals.No);
					}
					else if (base.RespawnManager.PendingRespawnCount > 0)
					{
						respawnText = TextManager.GetWithVariables("RespawnWaitingForMoreDeadPlayers", new ValueTuple<string, string>[]
						{
							new ValueTuple<string, string>("[deadplayers]", base.RespawnManager.PendingRespawnCount.ToString()),
							new ValueTuple<string, string>("[requireddeadplayers]", base.RespawnManager.RequiredRespawnCount.ToString())
						});
					}
				}
				else if (base.RespawnManager.CurrentState == RespawnManager.State.Transporting && base.RespawnManager.ReturnCountdownStarted)
				{
					float timeLeft2 = (float)(base.RespawnManager.ReturnTime - DateTime.Now).TotalSeconds;
					respawnText = ((timeLeft2 <= 0f) ? "" : TextManager.GetWithVariable("RespawnShuttleLeavingIn", "[time]", ToolBox.SecondsToReadableTime(timeLeft2), FormatCapitals.No));
					if (timeLeft2 < 20f)
					{
						float phase = (float)(Math.Sin((double)(timeLeft2 * 3.1415927f)) + 1.0) * 0.5f;
						textColor = Color.Lerp(GUIStyle.Red, Color.White, 1f - phase);
					}
					hideRespawnButtons = true;
				}
				GameMain.GameSession.SetRespawnInfo(respawnText.Value, textColor, this.WaitForNextRoundRespawn.GetValueOrDefault(true), hideRespawnButtons);
			}
			if (!this.ShowNetStats)
			{
				return;
			}
			this.NetStats.Draw(spriteBatch, new Rectangle(300, 10, 300, 150));
		}

		// Token: 0x06004B87 RID: 19335 RVA: 0x00299A40 File Offset: 0x00297C40
		public bool SelectCrewCharacter(Character character, GUIComponent frame)
		{
			if (character == null)
			{
				return false;
			}
			if (character != this.myCharacter)
			{
				Client client = this.previouslyConnectedClients.Find((Client c) => c.Character == character);
				if (client == null)
				{
					return false;
				}
				this.CreateSelectionRelatedButtons(client, frame);
			}
			return true;
		}

		// Token: 0x06004B88 RID: 19336 RVA: 0x00299A98 File Offset: 0x00297C98
		public bool SelectCrewClient(Client client, GUIComponent frame)
		{
			if (client == null || client.SessionId == this.SessionId)
			{
				return false;
			}
			this.CreateSelectionRelatedButtons(client, frame);
			return true;
		}

		// Token: 0x06004B89 RID: 19337 RVA: 0x00299AB8 File Offset: 0x00297CB8
		private void CreateSelectionRelatedButtons(Client client, GUIComponent frame)
		{
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(1f, 1f - frame.RectTransform.RelativeSize.Y), frame.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
			GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.2f), content.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), TextManager.Get("Mute"), null, "");
			guitickBox.Selected = client.MutedLocally;
			guitickBox.OnSelected = delegate(GUITickBox tickBox)
			{
				client.MutedLocally = tickBox.Selected;
				return true;
			};
			GUILayoutGroup volumeLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.45f), content.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup volumeTextLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), volumeLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			GUITextBlock label = new GUITextBlock(new RectTransform(new Vector2(0.6f, 1f), volumeTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("VoiceChatVolume"), null, null, Alignment.Left, false, "", null);
			GUITextBlock percentageText = new GUITextBlock(new RectTransform(new Vector2(0.4f, 1f), volumeTextLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), ToolBox.GetFormattedPercentage(client.VoiceVolume), null, null, Alignment.Right, false, "", null);
			GUIScrollBar guiscrollBar = new GUIScrollBar(new RectTransform(new Vector2(1f, 0.5f), volumeLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0.1f, null, "GUISlider", null);
			guiscrollBar.Range = new Vector2(0f, 1f);
			guiscrollBar.BarScroll = client.VoiceVolume / 2f;
			guiscrollBar.OnMoved = delegate(GUIScrollBar _, float barScroll)
			{
				float newVolume = barScroll * 2f;
				client.VoiceVolume = newVolume;
				percentageText.Text = ToolBox.GetFormattedPercentage(newVolume);
				return true;
			};
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.35f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.BottomLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			if (!GameMain.Client.GameStarted || ((GameMain.Client.Character == null || GameMain.Client.Character.IsDead) && (client.Character == null || client.Character.IsDead)))
			{
				GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(1f, 0.2f), content.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(0f, buttonContainer.RectTransform.RelativeSize.Y)
				}, TextManager.Get("message"), Alignment.Center, "GUIButtonSmall", null);
				guibutton.UserData = client;
				guibutton.OnClicked = delegate(GUIButton btn, object userdata)
				{
					this.chatBox.InputBox.Text = client.Name + "; ";
					CoroutineManager.StartCoroutine(base.<CreateSelectionRelatedButtons>g__selectCoroutine|0(), "");
					return false;
				};
			}
			if (this.HasPermission(ClientPermissions.Ban) && client.AllowKicking)
			{
				GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.45f, 0.9f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Ban"), Alignment.Center, "GUIButtonSmall", null);
				guibutton2.UserData = client;
				guibutton2.OnClicked = delegate(GUIButton btn, object userdata)
				{
					NetLobbyScreen.BanPlayer(client);
					return false;
				};
			}
			if (this.HasPermission(ClientPermissions.Kick) && client.AllowKicking)
			{
				GUIButton guibutton3 = new GUIButton(new RectTransform(new Vector2(0.45f, 0.9f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Kick"), Alignment.Center, "GUIButtonSmall", null);
				guibutton3.UserData = client;
				guibutton3.OnClicked = delegate(GUIButton btn, object userdata)
				{
					NetLobbyScreen.KickPlayer(client);
					return false;
				};
				return;
			}
			if (base.ServerSettings.AllowVoteKick && client.AllowKicking)
			{
				GUIButton guibutton4 = new GUIButton(new RectTransform(new Vector2(0.45f, 0.9f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("VoteToKick"), Alignment.Center, "GUIButtonSmall", null);
				guibutton4.UserData = client;
				guibutton4.OnClicked = delegate(GUIButton btn, object userdata)
				{
					this.VoteForKick(client);
					btn.Enabled = false;
					return true;
				};
			}
		}

		// Token: 0x06004B8A RID: 19338 RVA: 0x0029A0A0 File Offset: 0x002982A0
		public void CreateKickReasonPrompt(string clientName, bool ban)
		{
			GUIMessageBox banReasonPrompt = new GUIMessageBox(TextManager.Get(ban ? "BanReasonPrompt" : "KickReasonPrompt"), "", new LocalizedString[]
			{
				TextManager.Get("OK"),
				TextManager.Get("Cancel")
			}, new Vector2?(new Vector2(0.25f, 0.25f)), new Point?(new Point(400, 260)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.6f), banReasonPrompt.InnerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			GUITextBox banReasonBox = new GUITextBox(new RectTransform(new Vector2(1f, 0.3f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
			{
				Wrap = true,
				MaxTextLength = new int?(100)
			};
			GUINumberInput durationInputDays = null;
			GUINumberInput durationInputHours = null;
			GUITickBox permaBanTickBox = null;
			if (ban)
			{
				GUILayoutGroup labelContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), labelContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = TextManager.Get("BanDuration");
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null).Padding = Vector4.Zero;
				GUILayoutGroup buttonContent = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), labelContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
				permaBanTickBox = new GUITickBox(new RectTransform(new Vector2(0.4f, 0.15f), buttonContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("BanPermanent"), null, "")
				{
					Selected = true
				};
				GUILayoutGroup durationContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 1f), buttonContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Visible = false
				};
				GUITickBox permaBanTickBox2 = permaBanTickBox;
				permaBanTickBox2.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(permaBanTickBox2.OnSelected, new GUITickBox.OnSelectedHandler(delegate(GUITickBox tickBox)
				{
					durationContainer.Visible = !tickBox.Selected;
					return true;
				}));
				durationInputDays = new GUINumberInput(new RectTransform(new Vector2(0.2f, 1f), durationContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					MinValueInt = new int?(0),
					MaxValueFloat = new float?((float)1000)
				};
				new GUITextBlock(new RectTransform(new Vector2(0.2f, 1f), durationContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Days"), null, null, Alignment.Left, false, "", null);
				durationInputHours = new GUINumberInput(new RectTransform(new Vector2(0.2f, 1f), durationContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					MinValueInt = new int?(0),
					MaxValueFloat = new float?((float)24)
				};
				new GUITextBlock(new RectTransform(new Vector2(0.2f, 1f), durationContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Hours"), null, null, Alignment.Left, false, "", null);
			}
			GUIButton guibutton = banReasonPrompt.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userData)
			{
				if (ban)
				{
					if (!permaBanTickBox.Selected)
					{
						TimeSpan banDuration = new TimeSpan(durationInputDays.IntValue, durationInputHours.IntValue, 0, 0);
						this.BanPlayer(clientName, banReasonBox.Text, new TimeSpan?(banDuration));
					}
					else
					{
						this.BanPlayer(clientName, banReasonBox.Text, null);
					}
				}
				else
				{
					this.KickPlayer(clientName, banReasonBox.Text);
				}
				return true;
			}));
			GUIButton guibutton2 = banReasonPrompt.Buttons[0];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(banReasonPrompt.Close));
			GUIButton guibutton3 = banReasonPrompt.Buttons[1];
			guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(banReasonPrompt.Close));
		}

		// Token: 0x06004B8B RID: 19339 RVA: 0x0029A688 File Offset: 0x00298888
		public void ReportError(ClientNetError error, ushort expectedId = 0, ushort eventId = 0, ushort entityId = 0)
		{
			IWriteMessage outMsg = new WriteOnlyMessage();
			outMsg.WriteByte(15);
			outMsg.WriteByte((byte)error);
			if (error != ClientNetError.MISSING_EVENT)
			{
				if (error == ClientNetError.MISSING_ENTITY)
				{
					outMsg.WriteUInt16(eventId);
					outMsg.WriteUInt16(entityId);
					outMsg.WriteByte((byte)Submarine.Loaded.Count);
					foreach (Submarine sub in Submarine.Loaded.Take(5))
					{
						string subNameTruncated = (sub.Info.Name.Length > 16) ? sub.Info.Name.Substring(0, 16) : sub.Info.Name;
						outMsg.WriteString(subNameTruncated);
					}
				}
			}
			else
			{
				outMsg.WriteUInt16(expectedId);
				outMsg.WriteUInt16(eventId);
			}
			this.ClientPeer.Send(outMsg, DeliveryMethod.Reliable, true);
			this.WriteEventErrorData(error, expectedId, eventId, entityId);
		}

		// Token: 0x06004B8C RID: 19340 RVA: 0x0029A780 File Offset: 0x00298980
		private void WriteEventErrorData(ClientNetError error, ushort expectedID, ushort eventID, ushort entityID)
		{
			if (this.eventErrorWritten)
			{
				return;
			}
			List<string> errorLines = new List<string>
			{
				error.ToString(),
				""
			};
			if (this.IsServerOwner)
			{
				errorLines.Add("SERVER OWNER");
			}
			if (error == ClientNetError.MISSING_EVENT)
			{
				errorLines.Add("Expected ID: " + expectedID.ToString() + ", received " + eventID.ToString());
			}
			else if (error == ClientNetError.MISSING_ENTITY)
			{
				errorLines.Add("Event ID: " + eventID.ToString() + ", entity ID " + entityID.ToString());
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) != null)
			{
				errorLines.Add("Game mode: " + GameMain.GameSession.GameMode.Name.Value);
				GameSession gameSession2 = GameMain.GameSession;
				MultiPlayerCampaign campaign = ((gameSession2 != null) ? gameSession2.GameMode : null) as MultiPlayerCampaign;
				if (campaign != null)
				{
					errorLines.Add("Campaign ID: " + campaign.CampaignID.ToString());
					errorLines.Add(string.Concat(new string[]
					{
						"Campaign save ID: ",
						campaign.LastSaveID.ToString(),
						"(pending: ",
						campaign.PendingSaveID.ToString(),
						")"
					}));
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
			if (Entity.Spawner != null)
			{
				errorLines.Add("");
				errorLines.Add("EntitySpawner events:");
				foreach (ValueTuple<Entity, bool> valueTuple in Entity.Spawner.receivedEvents)
				{
					Entity entity = valueTuple.Item1;
					bool isRemoval = valueTuple.Item2;
					errorLines.Add(string.Concat(new string[]
					{
						isRemoval ? "Remove " : "Create ",
						entity.ToString(),
						" (",
						entity.ID.ToString(),
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
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 2);
			defaultInterpolatedStringHandler.AppendLiteral("event_error_log_client_");
			defaultInterpolatedStringHandler.AppendFormatted(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted(DateTime.UtcNow.ToShortTimeString());
			defaultInterpolatedStringHandler.AppendLiteral(".log");
			string filePath = defaultInterpolatedStringHandler.ToStringAndClear();
			filePath = Path.Combine(new string[]
			{
				"ServerLogs",
				ToolBox.RemoveInvalidFileNameChars(filePath)
			});
			if (!Directory.Exists("ServerLogs"))
			{
				Directory.CreateDirectory("ServerLogs", false);
			}
			File.WriteAllLines(filePath, errorLines, null, true);
			this.eventErrorWritten = true;
		}

		// Token: 0x06004B8D RID: 19341 RVA: 0x0029AD34 File Offset: 0x00298F34
		private static void AppendExceptionInfo(ref string errorMsg, out Entity causingEntity, Exception e)
		{
			if (!errorMsg.EndsWith("\n"))
			{
				errorMsg += "\n";
			}
			Exception innerMostException = e.GetInnermost();
			causingEntity = GameClient.GetCausingEntity(e);
			if (causingEntity != null)
			{
				string str = errorMsg;
				string str2 = "Entity: ";
				Entity entity = causingEntity;
				errorMsg = str + str2 + ((entity != null) ? entity.ToString() : null) + "\n";
			}
			errorMsg = errorMsg + e.Message + "\n";
			if (innerMostException != e)
			{
				errorMsg = string.Concat(new string[]
				{
					errorMsg,
					"Inner exception: ",
					innerMostException.Message,
					"\n",
					innerMostException.StackTrace.CleanupStackTrace()
				});
				return;
			}
			errorMsg += e.StackTrace.CleanupStackTrace();
		}

		// Token: 0x06004B8E RID: 19342 RVA: 0x0029ADF8 File Offset: 0x00298FF8
		private static Entity GetCausingEntity(Exception e)
		{
			Entity causingEntity = null;
			for (Exception currentException = e; currentException != null; currentException = currentException.InnerException)
			{
				EntityEventException entityEventException = currentException as EntityEventException;
				if (entityEventException != null)
				{
					causingEntity = entityEventException.Entity;
				}
			}
			return causingEntity;
		}

		// Token: 0x04002751 RID: 10065
		public static readonly TimeSpan CampaignSaveTransferTimeOut = new TimeSpan(0, 0, 100);

		// Token: 0x04002752 RID: 10066
		public static readonly TimeSpan LevelTransitionTimeOut = new TimeSpan(0, 0, 150);

		// Token: 0x04002754 RID: 10068
		private ushort nameId;

		// Token: 0x04002756 RID: 10070
		public string PendingName = string.Empty;

		// Token: 0x04002758 RID: 10072
		private GUIMessageBox reconnectBox;

		// Token: 0x04002759 RID: 10073
		private GUIMessageBox waitInServerQueueBox;

		// Token: 0x0400275A RID: 10074
		public LocalizedString endRoundVoteText;

		// Token: 0x0400275B RID: 10075
		public GUITickBox EndVoteTickBox;

		// Token: 0x0400275C RID: 10076
		private readonly GUIComponent buttonContainer;

		// Token: 0x0400275D RID: 10077
		public readonly NetStats NetStats;

		// Token: 0x0400275E RID: 10078
		protected GUITickBox cameraFollowsSub;

		// Token: 0x0400275F RID: 10079
		public CameraTransition EndCinematic;

		// Token: 0x04002760 RID: 10080
		public bool LateCampaignJoin;

		// Token: 0x04002761 RID: 10081
		private ClientPermissions permissions;

		// Token: 0x04002762 RID: 10082
		private List<Identifier> permittedConsoleCommands = new List<Identifier>();

		// Token: 0x04002763 RID: 10083
		private bool connected;

		// Token: 0x04002764 RID: 10084
		private ushort? debugStartGameCampaignSaveID;

		// Token: 0x04002765 RID: 10085
		private GameClient.RoundInitStatus roundInitStatus;

		// Token: 0x04002766 RID: 10086
		private readonly List<Client> otherClients;

		// Token: 0x04002767 RID: 10087
		public readonly List<SubmarineInfo> ServerSubmarines = new List<SubmarineInfo>();

		// Token: 0x04002768 RID: 10088
		private bool canStart;

		// Token: 0x04002769 RID: 10089
		private ushort lastSentChatMsgID;

		// Token: 0x0400276A RID: 10090
		private ushort lastQueueChatMsgID;

		// Token: 0x0400276B RID: 10091
		private readonly List<ChatMessage> chatMsgQueue = new List<ChatMessage>();

		// Token: 0x0400276C RID: 10092
		public float BlockedBySpamFilterTimer;

		// Token: 0x0400276D RID: 10093
		public ushort LastSentEntityEventID;

		// Token: 0x0400276E RID: 10094
		public bool HasSpawned;

		// Token: 0x04002770 RID: 10096
		public LocalizedString TraitorFirstObjective;

		// Token: 0x04002771 RID: 10097
		public TraitorEventPrefab TraitorMission;

		// Token: 0x04002774 RID: 10100
		private readonly List<Client> previouslyConnectedClients = new List<Client>();

		// Token: 0x04002775 RID: 10101
		public readonly FileReceiver FileReceiver;

		// Token: 0x04002776 RID: 10102
		public readonly ClientEntityEventManager EntityEventManager;

		// Token: 0x04002778 RID: 10104
		private readonly ImmutableArray<Endpoint> serverEndpoints;

		// Token: 0x04002779 RID: 10105
		private readonly Option<int> ownerKey;

		// Token: 0x0400277A RID: 10106
		public readonly NamedEvent<GameClient.PermissionChangedEvent> OnPermissionChanged = new NamedEvent<GameClient.PermissionChangedEvent>();

		// Token: 0x0400277B RID: 10107
		private bool connectCancelled;

		// Token: 0x0400277C RID: 10108
		private readonly List<IReadMessage> pendingIncomingMessages = new List<IReadMessage>();

		// Token: 0x0400277D RID: 10109
		private readonly List<IReadMessage> incomingMessagesToProcess = new List<IReadMessage>();

		// Token: 0x0400277E RID: 10110
		private CoroutineHandle startGameCoroutine;

		// Token: 0x0400277F RID: 10111
		private bool requestNewRoundStart;

		// Token: 0x04002780 RID: 10112
		private bool initialUpdateReceived;

		// Token: 0x04002781 RID: 10113
		private readonly List<IServerSerializable> debugEntityList = new List<IServerSerializable>();

		// Token: 0x04002782 RID: 10114
		private double LastMissingCampaignSubRequestTime;

		// Token: 0x04002783 RID: 10115
		private const double MissingCampaignSubRequestInterval = 10.0;

		// Token: 0x04002784 RID: 10116
		protected CharacterInfo characterInfo;

		// Token: 0x04002785 RID: 10117
		protected Character myCharacter;

		// Token: 0x04002786 RID: 10118
		protected GUIFrame inGameHUD;

		// Token: 0x04002787 RID: 10119
		protected ChatBox chatBox;

		// Token: 0x04002788 RID: 10120
		public GUIButton ShowLogButton;

		// Token: 0x04002789 RID: 10121
		private bool hasPermissionToUseLogButton;

		// Token: 0x0400278B RID: 10123
		private VotingInterface votingInterface;

		// Token: 0x0400278C RID: 10124
		private bool eventErrorWritten;

		// Token: 0x020011CC RID: 4556
		private enum RoundInitStatus
		{
			// Token: 0x04005D00 RID: 23808
			NotStarted,
			// Token: 0x04005D01 RID: 23809
			Starting,
			// Token: 0x04005D02 RID: 23810
			WaitingForStartGameFinalize,
			// Token: 0x04005D03 RID: 23811
			Started,
			// Token: 0x04005D04 RID: 23812
			Error,
			// Token: 0x04005D05 RID: 23813
			Interrupted
		}

		// Token: 0x020011CD RID: 4557
		internal readonly struct PermissionChangedEvent
		{
			// Token: 0x060091FB RID: 37371 RVA: 0x003C65E3 File Offset: 0x003C47E3
			public PermissionChangedEvent(ClientPermissions newPermissions, IReadOnlyList<Identifier> newPermittedConsoleCommands)
			{
				this.NewPermissions = newPermissions;
				this.NewPermittedConsoleCommands = newPermittedConsoleCommands.ToImmutableArray<Identifier>();
			}

			// Token: 0x04005D06 RID: 23814
			public readonly ClientPermissions NewPermissions;

			// Token: 0x04005D07 RID: 23815
			public readonly ImmutableArray<Identifier> NewPermittedConsoleCommands;
		}

		// Token: 0x020011CE RID: 4558
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005D08 RID: 23816
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Action<string> <0>__Log;

			// Token: 0x04005D09 RID: 23817
			public static Func<JobVariant, NetJobVariant> <1>__FromJobVariant;
		}
	}
}
