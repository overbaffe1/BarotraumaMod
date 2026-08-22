using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000369 RID: 873
	internal class Client : IDisposable
	{
		// Token: 0x17000E58 RID: 3672
		// (get) Token: 0x06003394 RID: 13204 RVA: 0x0015CB6C File Offset: 0x0015AD6C
		// (set) Token: 0x06003395 RID: 13205 RVA: 0x0015CB74 File Offset: 0x0015AD74
		public List<JobVariant> JobPreferences { get; set; }

		// Token: 0x17000E59 RID: 3673
		// (get) Token: 0x06003396 RID: 13206 RVA: 0x0015CB7D File Offset: 0x0015AD7D
		// (set) Token: 0x06003397 RID: 13207 RVA: 0x0015CB88 File Offset: 0x0015AD88
		public CharacterInfo CharacterInfo
		{
			get
			{
				return this.characterInfo;
			}
			set
			{
				if (this.characterInfo == value)
				{
					return;
				}
				CharacterInfo characterInfo = this.characterInfo;
				if (characterInfo != null && characterInfo.Character == null)
				{
					this.characterInfo.Remove();
				}
				this.characterInfo = value;
			}
		}

		// Token: 0x17000E5A RID: 3674
		// (get) Token: 0x06003398 RID: 13208 RVA: 0x0015CBC3 File Offset: 0x0015ADC3
		// (set) Token: 0x06003399 RID: 13209 RVA: 0x0015CBCB File Offset: 0x0015ADCB
		public NetworkConnection Connection { get; set; }

		// Token: 0x17000E5B RID: 3675
		// (get) Token: 0x0600339A RID: 13210 RVA: 0x0015CBD4 File Offset: 0x0015ADD4
		// (set) Token: 0x0600339B RID: 13211 RVA: 0x0015CC30 File Offset: 0x0015AE30
		public float Karma
		{
			get
			{
				if (GameMain.Server != null && GameMain.Server.ServerSettings.KarmaEnabled)
				{
					GameSession gameSession = GameMain.GameSession;
					if (!(((gameSession != null) ? gameSession.GameMode : null) is PvPMode))
					{
						if (this.HasPermission(ClientPermissions.KarmaImmunity))
						{
							return 100f;
						}
						return this.karma;
					}
				}
				return 100f;
			}
			set
			{
				if (GameMain.Server != null && GameMain.Server.ServerSettings.KarmaEnabled)
				{
					GameSession gameSession = GameMain.GameSession;
					if (!(((gameSession != null) ? gameSession.GameMode : null) is PvPMode))
					{
						this.karma = Math.Min(Math.Max(value, 0f), 100f);
						if (!MathUtils.NearlyEqual(this.karma, this.syncedKarma, 10f))
						{
							this.syncedKarma = this.karma;
							NetworkMember networkMember = GameMain.NetworkMember;
							ushort lastClientListUpdateID = networkMember.LastClientListUpdateID;
							networkMember.LastClientListUpdateID = lastClientListUpdateID + 1;
						}
						return;
					}
				}
			}
		}

		// Token: 0x17000E5C RID: 3676
		// (get) Token: 0x0600339C RID: 13212 RVA: 0x0015CCC2 File Offset: 0x0015AEC2
		public int KickVoteCount
		{
			get
			{
				return this.kickVoters.Count;
			}
		}

		// Token: 0x0600339D RID: 13213 RVA: 0x0015CCD0 File Offset: 0x0015AED0
		public void InitClientSync()
		{
			this.LastSentChatMsgID = 0;
			this.LastRecvChatMsgID = ChatMessage.LastID;
			this.LastRecvLobbyUpdate = NetIdUtils.GetIdOlderThan(GameMain.NetLobbyScreen.LastUpdateID);
			this.InitialLobbyUpdateSent = false;
			this.LastRecvEntityEventID = 0;
			this.UnreceivedEntityEventCount = 0;
			this.NeedsMidRoundSync = false;
		}

		// Token: 0x0600339E RID: 13214 RVA: 0x0015CD20 File Offset: 0x0015AF20
		public static bool IsValidName(string name, ServerSettings serverSettings)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}
			char[] disallowedChars = new char[]
			{
				';',
				'<',
				'>',
				'/',
				'\\',
				'[',
				']',
				'"',
				'?'
			};
			if (name.Any((char c) => disallowedChars.Contains(c)))
			{
				return false;
			}
			for (int i = 0; i < name.Length; i++)
			{
				char character = name[i];
				if (!serverSettings.AllowedClientNameChars.Any((Range<int> charRange) => (int)character >= charRange.Start && (int)character <= charRange.End))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600339F RID: 13215 RVA: 0x0015CDAD File Offset: 0x0015AFAD
		public bool AddressMatches(Address address)
		{
			return this.Connection.Endpoint.Address.Equals(address);
		}

		// Token: 0x060033A0 RID: 13216 RVA: 0x0015CDC5 File Offset: 0x0015AFC5
		public void AddKickVote(Client voter)
		{
			if (voter != null && !this.kickVoters.Contains(voter))
			{
				this.kickVoters.Add(voter);
			}
		}

		// Token: 0x060033A1 RID: 13217 RVA: 0x0015CDE4 File Offset: 0x0015AFE4
		public void RemoveKickVote(Client voter)
		{
			this.kickVoters.Remove(voter);
		}

		// Token: 0x060033A2 RID: 13218 RVA: 0x0015CDF3 File Offset: 0x0015AFF3
		public bool HasKickVoteFrom(Client voter)
		{
			return this.kickVoters.Contains(voter);
		}

		// Token: 0x060033A3 RID: 13219 RVA: 0x0015CE04 File Offset: 0x0015B004
		public bool HasKickVoteFromSessionId(int id)
		{
			return this.kickVoters.Any((Client k) => (int)k.SessionId == id);
		}

		// Token: 0x060033A4 RID: 13220 RVA: 0x0015CE38 File Offset: 0x0015B038
		public static void UpdateKickVotes(IReadOnlyList<Client> connectedClients)
		{
			Predicate<Client> <>9__0;
			foreach (Client client in connectedClients)
			{
				List<Client> list = client.kickVoters;
				Predicate<Client> match;
				if ((match = <>9__0) == null)
				{
					match = (<>9__0 = ((Client voter) => !connectedClients.Contains(voter)));
				}
				list.RemoveAll(match);
			}
		}

		// Token: 0x060033A5 RID: 13221 RVA: 0x0015CEB8 File Offset: 0x0015B0B8
		public void ResetVotes(bool resetKickVotes)
		{
			for (int i = 0; i < this.votes.Length; i++)
			{
				this.votes[i] = null;
			}
			if (resetKickVotes)
			{
				this.kickVoters.Clear();
			}
		}

		// Token: 0x060033A6 RID: 13222 RVA: 0x0015CEF0 File Offset: 0x0015B0F0
		public void SetPermissions(ClientPermissions permissions, IEnumerable<DebugConsole.Command> permittedConsoleCommands)
		{
			this.Permissions = permissions;
			this.PermittedConsoleCommands.Clear();
			this.PermittedConsoleCommands.UnionWith(permittedConsoleCommands);
			if (this.Permissions.HasFlag(ClientPermissions.ManageSettings))
			{
				GameServer server = GameMain.Server;
				if (server == null)
				{
					return;
				}
				ServerSettings serverSettings = server.ServerSettings;
				if (serverSettings == null)
				{
					return;
				}
				serverSettings.ForcePropertyUpdate();
			}
		}

		// Token: 0x060033A7 RID: 13223 RVA: 0x0015CF50 File Offset: 0x0015B150
		public void GivePermission(ClientPermissions permission)
		{
			if (!this.Permissions.HasFlag(permission))
			{
				this.Permissions |= permission;
				if (permission.HasFlag(ClientPermissions.ManageSettings))
				{
					GameServer server = GameMain.Server;
					if (server == null)
					{
						return;
					}
					ServerSettings serverSettings = server.ServerSettings;
					if (serverSettings == null)
					{
						return;
					}
					serverSettings.ForcePropertyUpdate();
				}
			}
		}

		// Token: 0x060033A8 RID: 13224 RVA: 0x0015CFB3 File Offset: 0x0015B1B3
		public void RemovePermission(ClientPermissions permission)
		{
			this.Permissions &= ~permission;
		}

		// Token: 0x060033A9 RID: 13225 RVA: 0x0015CFC4 File Offset: 0x0015B1C4
		public bool HasPermission(ClientPermissions permission)
		{
			return this.Permissions.HasFlag(permission);
		}

		// Token: 0x060033AA RID: 13226 RVA: 0x0015CFDC File Offset: 0x0015B1DC
		public bool TryTakeOverBot(Character botCharacter)
		{
			if (GameMain.Server == null)
			{
				DebugConsole.ThrowError("TryTakeOverBot: Client " + this.Name + " requested to take over a bot but GameMain.Server is null!", null, null, false, false);
				return false;
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null)
			{
				ServerSettings serverSettings = networkMember.ServerSettings;
				if (serverSettings != null && serverSettings.RespawnMode == RespawnMode.Permadeath)
				{
					if (this.CharacterInfo == null)
					{
						DebugConsole.ThrowError("Permadeath: Client " + this.Name + " requested to take over a bot, but they don't seem to have a character at all yet.", null, null, false, false);
						GameMain.Server.SendConsoleMessage("Permadeath: Taking over a bot requires having a character that died first.", this, new Color?(Color.Red));
						return false;
					}
					CharacterInfo characterInfo = this.CharacterInfo;
					if (characterInfo == null || !characterInfo.PermanentlyDead)
					{
						DebugConsole.ThrowError("Permadeath: Client " + this.Name + " requested to take over a bot, but their character has not been permanently killed.", null, null, false, false);
						GameMain.Server.SendConsoleMessage("Permadeath: Could not take over the bot, previous character not permanently killed.", this, new Color?(Color.Red));
						return false;
					}
					if (!botCharacter.IsBot)
					{
						DebugConsole.ThrowError("Permadeath: " + this.Name + " requested to take over a bot character, but the target character is not a bot!", null, null, false, false);
						GameMain.Server.SendConsoleMessage("Permadeath: Could not take over the target character because it is not a bot.", this, new Color?(Color.Red));
						return false;
					}
					if (botCharacter.Info != null)
					{
						botCharacter.Info.RenamingEnabled = true;
					}
					GameSession gameSession = GameMain.GameSession;
					MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
					if (mpCampaign != null)
					{
						mpCampaign.DiscardClientCharacterData(this);
					}
					GameMain.Server.SetClientCharacter(this, botCharacter);
					CharacterCampaignData characterData = (mpCampaign != null) ? mpCampaign.SetClientCharacterData(this) : null;
					if (characterData != null)
					{
						characterData.HasSpawned = true;
						mpCampaign.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.CharacterInfo);
					}
					this.SpectateOnly = false;
					return true;
				}
			}
			DebugConsole.ThrowError("Client " + this.Name + " requested to take over a bot but Permadeath is not enabled!", null, null, false, false);
			GameMain.Server.SendConsoleMessage("Permadeath mode is not enabled, cannot take over a bot.", this, new Color?(Color.Red));
			return false;
		}

		// Token: 0x060033AB RID: 13227 RVA: 0x0015D1A5 File Offset: 0x0015B3A5
		public void ResetSync()
		{
			this.NeedsMidRoundSync = false;
			this.PendingPositionUpdates.Clear();
			this.EntityEventLastSent.Clear();
			this.LastSentEntityEventID = 0;
			this.LastRecvEntityEventID = 0;
			this.UnreceivedEntityEventCount = 0;
		}

		// Token: 0x17000E5D RID: 3677
		// (get) Token: 0x060033AC RID: 13228 RVA: 0x0015D1D9 File Offset: 0x0015B3D9
		public Option<AccountId> AccountId
		{
			get
			{
				return this.AccountInfo.AccountId;
			}
		}

		// Token: 0x17000E5E RID: 3678
		// (get) Token: 0x060033AD RID: 13229 RVA: 0x0015D1E6 File Offset: 0x0015B3E6
		// (set) Token: 0x060033AE RID: 13230 RVA: 0x0015D1F0 File Offset: 0x0015B3F0
		public CharacterTeamType TeamID
		{
			get
			{
				return this.teamID;
			}
			set
			{
				if (value != this.teamID)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Changed client ");
					defaultInterpolatedStringHandler.AppendFormatted(this.Name);
					defaultInterpolatedStringHandler.AppendLiteral("'s team to ");
					defaultInterpolatedStringHandler.AppendFormatted<CharacterTeamType>(this.teamID);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
					if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
					{
						NetworkMember networkMember = GameMain.NetworkMember;
						ushort lastClientListUpdateID = networkMember.LastClientListUpdateID;
						networkMember.LastClientListUpdateID = lastClientListUpdateID + 1;
					}
					this.teamID = value;
				}
			}
		}

		// Token: 0x17000E5F RID: 3679
		// (get) Token: 0x060033AF RID: 13231 RVA: 0x0015D28C File Offset: 0x0015B48C
		// (set) Token: 0x060033B0 RID: 13232 RVA: 0x0015D2E4 File Offset: 0x0015B4E4
		public Character Character
		{
			get
			{
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					Character character = this.character;
					if (((character != null) ? character.ID : 0) != this.CharacterID)
					{
						this.Character = (Entity.FindEntityByID(this.CharacterID) as Character);
					}
				}
				return this.character;
			}
			set
			{
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					ushort lastClientListUpdateID = networkMember.LastClientListUpdateID;
					networkMember.LastClientListUpdateID = lastClientListUpdateID + 1;
					if (value != null)
					{
						this.CharacterID = value.ID;
					}
				}
				this.character = value;
				if (this.character != null)
				{
					this.HasSpawned = true;
					this.UsingFreeCam = false;
				}
			}
		}

		// Token: 0x17000E60 RID: 3680
		// (get) Token: 0x060033B1 RID: 13233 RVA: 0x0015D348 File Offset: 0x0015B548
		// (set) Token: 0x060033B2 RID: 13234 RVA: 0x0015D37F File Offset: 0x0015B57F
		public Vector2? SpectatePos
		{
			get
			{
				if (this.character == null || this.character.IsDead)
				{
					return new Vector2?(this.spectatePos);
				}
				return null;
			}
			set
			{
				this.spectatePos = value.Value;
			}
		}

		// Token: 0x17000E61 RID: 3681
		// (get) Token: 0x060033B3 RID: 13235 RVA: 0x0015D38E File Offset: 0x0015B58E
		public bool Spectating
		{
			get
			{
				return this.inGame && this.character == null;
			}
		}

		// Token: 0x17000E62 RID: 3682
		// (get) Token: 0x060033B4 RID: 13236 RVA: 0x0015D3A3 File Offset: 0x0015B5A3
		// (set) Token: 0x060033B5 RID: 13237 RVA: 0x0015D3AC File Offset: 0x0015B5AC
		public bool Muted
		{
			get
			{
				return this.muted;
			}
			set
			{
				if (this.muted == value)
				{
					return;
				}
				this.muted = value;
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					ushort lastClientListUpdateID = networkMember.LastClientListUpdateID;
					networkMember.LastClientListUpdateID = lastClientListUpdateID + 1;
				}
			}
		}

		// Token: 0x17000E63 RID: 3683
		// (get) Token: 0x060033B6 RID: 13238 RVA: 0x0015D3F2 File Offset: 0x0015B5F2
		public bool HasPermissions
		{
			get
			{
				return this.Permissions > ClientPermissions.None;
			}
		}

		// Token: 0x17000E64 RID: 3684
		// (get) Token: 0x060033B7 RID: 13239 RVA: 0x0015D3FD File Offset: 0x0015B5FD
		// (set) Token: 0x060033B8 RID: 13240 RVA: 0x0015D405 File Offset: 0x0015B605
		public VoipQueue VoipQueue { get; private set; }

		// Token: 0x17000E65 RID: 3685
		// (get) Token: 0x060033B9 RID: 13241 RVA: 0x0015D40E File Offset: 0x0015B60E
		// (set) Token: 0x060033BA RID: 13242 RVA: 0x0015D418 File Offset: 0x0015B618
		public bool InGame
		{
			get
			{
				return this.inGame;
			}
			set
			{
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					ushort lastClientListUpdateID = networkMember.LastClientListUpdateID;
					networkMember.LastClientListUpdateID = lastClientListUpdateID + 1;
				}
				this.inGame = value;
			}
		}

		// Token: 0x060033BB RID: 13243 RVA: 0x0015D454 File Offset: 0x0015B654
		private void InitProjSpecific()
		{
			this.kickVoters = new List<Client>();
			this.JobPreferences = new List<JobVariant>();
			this.VoipQueue = new VoipQueue(this.SessionId, true, true);
			this.VoipServerDecoder = new VoipServerDecoder(this.VoipQueue, this);
			GameMain.Server.VoipServer.RegisterQueue(this.VoipQueue);
			this.MidRoundSyncTimeOut = double.PositiveInfinity;
			this.JoinTime = DateTime.Now;
		}

		// Token: 0x060033BC RID: 13244 RVA: 0x0015D4CC File Offset: 0x0015B6CC
		private void DisposeProjSpecific()
		{
			GameMain.Server.VoipServer.UnregisterQueue(this.VoipQueue);
			this.VoipQueue.Dispose();
			if (this.characterInfo != null && (this.characterInfo.Character == null || this.characterInfo.Character.Removed))
			{
				CharacterInfo characterInfo = this.characterInfo;
				if (characterInfo != null)
				{
					characterInfo.Remove();
				}
				this.characterInfo = null;
			}
		}

		// Token: 0x060033BD RID: 13245 RVA: 0x0015D538 File Offset: 0x0015B738
		public Client(string name, byte sessionId)
		{
			this.Name = name;
			this.SessionId = sessionId;
			this.votes = new object[Enum.GetNames(typeof(VoteType)).Length];
			this.InitProjSpecific();
		}

		// Token: 0x060033BE RID: 13246 RVA: 0x0015D65C File Offset: 0x0015B85C
		public T GetVote<T>(VoteType voteType)
		{
			object obj = this.votes[(int)voteType];
			if (obj is T)
			{
				return (T)((object)obj);
			}
			return default(T);
		}

		// Token: 0x060033BF RID: 13247 RVA: 0x0015D68E File Offset: 0x0015B88E
		public void SetVote(VoteType voteType, object value)
		{
			this.votes[(int)voteType] = value;
		}

		// Token: 0x060033C0 RID: 13248 RVA: 0x0015D69C File Offset: 0x0015B89C
		public bool SessionOrAccountIdMatches(string userId)
		{
			byte sessionId;
			return (this.AccountId.IsSome() && Barotrauma.Networking.AccountId.Parse(userId) == this.AccountId) || (byte.TryParse(userId, out sessionId) && this.SessionId == sessionId);
		}

		// Token: 0x060033C1 RID: 13249 RVA: 0x0015D6E4 File Offset: 0x0015B8E4
		public void WritePermissions(IWriteMessage msg)
		{
			msg.WriteByte(this.SessionId);
			msg.WriteRangedInteger((int)this.Permissions, 0, 524287);
			if (this.HasPermission(ClientPermissions.ConsoleCommands))
			{
				msg.WriteUInt16((ushort)this.PermittedConsoleCommands.Count);
				foreach (DebugConsole.Command command in this.PermittedConsoleCommands)
				{
					msg.WriteIdentifier(command.Names[0]);
				}
			}
		}

		// Token: 0x060033C2 RID: 13250 RVA: 0x0015D780 File Offset: 0x0015B980
		public static void ReadPermissions(IReadMessage inc, out ClientPermissions permissions, out List<DebugConsole.Command> permittedCommands)
		{
			int permissionsInt = inc.ReadRangedInteger(0, 524287);
			permissions = ClientPermissions.None;
			permittedCommands = new List<DebugConsole.Command>();
			try
			{
				permissions = (ClientPermissions)permissionsInt;
			}
			catch (InvalidCastException)
			{
				return;
			}
			if (permissions.HasFlag(ClientPermissions.ConsoleCommands))
			{
				ushort commandCount = inc.ReadUInt16();
				for (int i = 0; i < (int)commandCount; i++)
				{
					Identifier commandName = inc.ReadIdentifier();
					DebugConsole.Command consoleCommand = DebugConsole.Commands.Find((DebugConsole.Command c) => c.Names.Contains(commandName));
					if (consoleCommand != null)
					{
						permittedCommands.Add(consoleCommand);
					}
				}
			}
		}

		// Token: 0x060033C3 RID: 13251 RVA: 0x0015D820 File Offset: 0x0015BA20
		public void ReadPermissions(IReadMessage inc)
		{
			ClientPermissions permissions;
			List<DebugConsole.Command> permittedCommands;
			Client.ReadPermissions(inc, out permissions, out permittedCommands);
			this.SetPermissions(permissions, permittedCommands);
		}

		// Token: 0x060033C4 RID: 13252 RVA: 0x0015D840 File Offset: 0x0015BA40
		public static string SanitizeName(string name, int maxLength = 32)
		{
			name = name.Trim().Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
			if (name.Length > maxLength)
			{
				name = name.Substring(0, maxLength);
			}
			string rName = "";
			for (int i = 0; i < name.Length; i++)
			{
				ReadOnlySpan<char> str = rName;
				char c = (name[i] < ' ') ? '?' : name[i];
				rName = str + new ReadOnlySpan<char>(ref c);
			}
			return rName;
		}

		// Token: 0x060033C5 RID: 13253 RVA: 0x0015D8CB File Offset: 0x0015BACB
		public void Dispose()
		{
			this.DisposeProjSpecific();
		}

		// Token: 0x04001999 RID: 6553
		public bool VoiceEnabled = true;

		// Token: 0x0400199A RID: 6554
		public VoipServerDecoder VoipServerDecoder;

		// Token: 0x0400199B RID: 6555
		public ushort LastRecvClientListUpdate = NetIdUtils.GetIdOlderThan(GameMain.Server.LastClientListUpdateID);

		// Token: 0x0400199C RID: 6556
		public ushort LastSentServerSettingsUpdate = NetIdUtils.GetIdOlderThan(GameMain.Server.ServerSettings.LastUpdateIdForFlag[ServerSettings.NetFlags.Properties]);

		// Token: 0x0400199D RID: 6557
		public ushort LastRecvServerSettingsUpdate = NetIdUtils.GetIdOlderThan(GameMain.Server.ServerSettings.LastUpdateIdForFlag[ServerSettings.NetFlags.Properties]);

		// Token: 0x0400199E RID: 6558
		public ushort LastRecvLobbyUpdate = NetIdUtils.GetIdOlderThan(GameMain.NetLobbyScreen.LastUpdateID);

		// Token: 0x0400199F RID: 6559
		public bool InitialLobbyUpdateSent;

		// Token: 0x040019A0 RID: 6560
		public ushort LastSentChatMsgID;

		// Token: 0x040019A1 RID: 6561
		public ushort LastRecvChatMsgID;

		// Token: 0x040019A2 RID: 6562
		public ushort LastSentEntityEventID;

		// Token: 0x040019A3 RID: 6563
		public ushort LastRecvEntityEventID;

		// Token: 0x040019A4 RID: 6564
		public readonly Dictionary<MultiPlayerCampaign.NetFlags, ushort> LastRecvCampaignUpdate = new Dictionary<MultiPlayerCampaign.NetFlags, ushort>();

		// Token: 0x040019A5 RID: 6565
		public ushort LastRecvCampaignSave;

		// Token: 0x040019A6 RID: 6566
		[TupleElementNames(new string[]
		{
			"saveId",
			"time"
		})]
		public ValueTuple<ushort, float> LastCampaignSaveSendTime;

		// Token: 0x040019A7 RID: 6567
		public readonly List<ChatMessage> ChatMsgQueue = new List<ChatMessage>();

		// Token: 0x040019A8 RID: 6568
		public ushort LastChatMsgQueueID;

		// Token: 0x040019A9 RID: 6569
		public readonly List<string> LastSentChatMessages = new List<string>();

		// Token: 0x040019AA RID: 6570
		public float ChatSpamSpeed;

		// Token: 0x040019AB RID: 6571
		public float ChatSpamTimer;

		// Token: 0x040019AC RID: 6572
		public int ChatSpamCount;

		// Token: 0x040019AD RID: 6573
		public string RejectedName;

		// Token: 0x040019AE RID: 6574
		public float KickAFKTimer;

		// Token: 0x040019AF RID: 6575
		public double MidRoundSyncTimeOut;

		// Token: 0x040019B0 RID: 6576
		public bool NeedsMidRoundSync;

		// Token: 0x040019B1 RID: 6577
		public ushort UnreceivedEntityEventCount;

		// Token: 0x040019B2 RID: 6578
		public ushort FirstNewEventID;

		// Token: 0x040019B3 RID: 6579
		public readonly Dictionary<ushort, double> EntityEventLastSent = new Dictionary<ushort, double>();

		// Token: 0x040019B4 RID: 6580
		public readonly Dictionary<Entity, float> PositionUpdateLastSent = new Dictionary<Entity, float>();

		// Token: 0x040019B5 RID: 6581
		public readonly Queue<Entity> PendingPositionUpdates = new Queue<Entity>();

		// Token: 0x040019B6 RID: 6582
		public bool ReadyToStart;

		// Token: 0x040019B8 RID: 6584
		public JobVariant AssignedJob;

		// Token: 0x040019B9 RID: 6585
		public float DeleteDisconnectedTimer;

		// Token: 0x040019BA RID: 6586
		public DateTime JoinTime;

		// Token: 0x040019BB RID: 6587
		public static readonly TimeSpan NameChangeCoolDown = new TimeSpan(0, 0, 30);

		// Token: 0x040019BC RID: 6588
		public DateTime LastNameChangeTime;

		// Token: 0x040019BD RID: 6589
		private CharacterInfo characterInfo;

		// Token: 0x040019BE RID: 6590
		public string PendingName;

		// Token: 0x040019C0 RID: 6592
		public bool SpectateOnly;

		// Token: 0x040019C1 RID: 6593
		public bool AFK;

		// Token: 0x040019C2 RID: 6594
		public bool? WaitForNextRoundRespawn;

		// Token: 0x040019C3 RID: 6595
		public int KarmaKickCount;

		// Token: 0x040019C4 RID: 6596
		private float syncedKarma = 100f;

		// Token: 0x040019C5 RID: 6597
		private float karma = 100f;

		// Token: 0x040019C6 RID: 6598
		private List<Client> kickVoters;

		// Token: 0x040019C7 RID: 6599
		public WeakReference<Character> PreviousCharacter;

		// Token: 0x040019C8 RID: 6600
		public const int MaxNameLength = 32;

		// Token: 0x040019C9 RID: 6601
		public string Name;

		// Token: 0x040019CA RID: 6602
		public ushort NameId;

		// Token: 0x040019CB RID: 6603
		public readonly byte SessionId;

		// Token: 0x040019CC RID: 6604
		public AccountInfo AccountInfo;

		// Token: 0x040019CD RID: 6605
		public LanguageIdentifier Language;

		// Token: 0x040019CE RID: 6606
		public ushort Ping;

		// Token: 0x040019CF RID: 6607
		public Identifier PreferredJob;

		// Token: 0x040019D0 RID: 6608
		private CharacterTeamType teamID;

		// Token: 0x040019D1 RID: 6609
		public CharacterTeamType PreferredTeam;

		// Token: 0x040019D2 RID: 6610
		private Character character;

		// Token: 0x040019D3 RID: 6611
		public bool UsingFreeCam;

		// Token: 0x040019D4 RID: 6612
		public ushort CharacterID;

		// Token: 0x040019D5 RID: 6613
		private Vector2 spectatePos;

		// Token: 0x040019D6 RID: 6614
		private bool muted;

		// Token: 0x040019D8 RID: 6616
		private bool inGame;

		// Token: 0x040019D9 RID: 6617
		public bool HasSpawned;

		// Token: 0x040019DA RID: 6618
		public HashSet<Identifier> GivenAchievements = new HashSet<Identifier>();

		// Token: 0x040019DB RID: 6619
		public ClientPermissions Permissions;

		// Token: 0x040019DC RID: 6620
		public readonly HashSet<DebugConsole.Command> PermittedConsoleCommands = new HashSet<DebugConsole.Command>();

		// Token: 0x040019DD RID: 6621
		private readonly object[] votes;
	}
}
