using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x0200045A RID: 1114
	internal class Client : IDisposable
	{
		// Token: 0x1700131A RID: 4890
		// (get) Token: 0x06004AC5 RID: 19141 RVA: 0x00291ACC File Offset: 0x0028FCCC
		// (set) Token: 0x06004AC6 RID: 19142 RVA: 0x00291AD4 File Offset: 0x0028FCD4
		public VoipSound VoipSound { get; set; }

		// Token: 0x1700131B RID: 4891
		// (get) Token: 0x06004AC7 RID: 19143 RVA: 0x00291ADD File Offset: 0x0028FCDD
		// (set) Token: 0x06004AC8 RID: 19144 RVA: 0x00291AE5 File Offset: 0x0028FCE5
		public float VoiceVolume
		{
			get
			{
				return this.voiceVolume;
			}
			set
			{
				this.voiceVolume = Math.Clamp(value, 0f, 2f);
			}
		}

		// Token: 0x1700131C RID: 4892
		// (get) Token: 0x06004AC9 RID: 19145 RVA: 0x00291AFD File Offset: 0x0028FCFD
		// (set) Token: 0x06004ACA RID: 19146 RVA: 0x00291B05 File Offset: 0x0028FD05
		public float RadioNoise
		{
			get
			{
				return this.radioNoise;
			}
			set
			{
				this.radioNoise = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x1700131D RID: 4893
		// (get) Token: 0x06004ACB RID: 19147 RVA: 0x00291B1D File Offset: 0x0028FD1D
		// (set) Token: 0x06004ACC RID: 19148 RVA: 0x00291B28 File Offset: 0x0028FD28
		public bool MutedLocally
		{
			get
			{
				return this.mutedLocally;
			}
			set
			{
				if (this.mutedLocally == value)
				{
					return;
				}
				this.mutedLocally = value;
				GameMain.NetLobbyScreen.SetPlayerVoiceIconState(this, this.muted, this.mutedLocally);
				GameSession gameSession = GameMain.GameSession;
				if (gameSession == null)
				{
					return;
				}
				CrewManager crewManager = gameSession.CrewManager;
				if (crewManager == null)
				{
					return;
				}
				crewManager.SetPlayerVoiceIconState(this, this.muted, this.mutedLocally);
			}
		}

		// Token: 0x1700131E RID: 4894
		// (get) Token: 0x06004ACD RID: 19149 RVA: 0x00291B83 File Offset: 0x0028FD83
		public bool AllowKicking
		{
			get
			{
				return !this.IsOwner && !this.HasPermission(ClientPermissions.Ban) && !this.HasPermission(ClientPermissions.Kick) && !this.HasPermission(ClientPermissions.Unban);
			}
		}

		// Token: 0x06004ACE RID: 19150 RVA: 0x00291BAC File Offset: 0x0028FDAC
		public void UpdateVoipSound()
		{
			if (this.VoipSound == null || !this.VoipSound.IsPlaying)
			{
				SoundChannel soundChannel = this.radioNoiseChannel;
				if (soundChannel != null)
				{
					soundChannel.Dispose();
				}
				this.radioNoiseChannel = null;
				if (this.VoipSound != null)
				{
					DebugConsole.Log("Destroying voipsound");
					this.VoipSound.Dispose();
				}
				this.VoipSound = null;
				return;
			}
			if (Screen.Selected is ModDownloadScreen)
			{
				this.VoipSound.Gain = 0f;
			}
			float gain = 1f;
			float noiseGain = 0f;
			Vector3? position = null;
			if (this.character != null && !this.character.IsDead)
			{
				if (GameSettings.CurrentConfig.Audio.UseDirectionalVoiceChat)
				{
					position = new Vector3?(new Vector3(this.character.WorldPosition.X, this.character.WorldPosition.Y, 0f));
				}
				else
				{
					float dist = Vector3.Distance(new Vector3(this.character.WorldPosition, 0f), GameMain.SoundManager.ListenerPosition);
					gain = 1f - MathUtils.InverseLerp(this.VoipSound.Near, this.VoipSound.Far, dist);
				}
				if (!this.VoipSound.UsingRadio)
				{
					float garbleAmount = ChatMessage.GetGarbleAmount(Character.Controlled, this.character, 1000f, 2f);
					gain *= 1f - garbleAmount;
				}
				if (this.RadioNoise > 0f)
				{
					noiseGain = gain * this.RadioNoise;
					gain *= 1f - this.RadioNoise;
				}
			}
			this.VoipSound.SetPosition(position);
			this.VoipSound.Gain = gain;
			if (noiseGain > 0f)
			{
				if (this.radioNoiseChannel == null || !this.radioNoiseChannel.IsPlaying)
				{
					this.radioNoiseChannel = SoundPlayer.PlaySound("radiostatic", 1f);
					this.radioNoiseChannel.Category = SoundManager.SoundCategoryVoip;
					this.radioNoiseChannel.Looping = true;
				}
				this.radioNoiseChannel.Near = this.VoipSound.Near;
				this.radioNoiseChannel.Far = this.VoipSound.Far;
				this.radioNoiseChannel.Position = position;
				this.radioNoiseChannel.Gain = noiseGain;
				return;
			}
			if (this.radioNoiseChannel != null)
			{
				this.radioNoiseChannel.Gain = 0f;
			}
		}

		// Token: 0x06004ACF RID: 19151 RVA: 0x00291E04 File Offset: 0x00290004
		public void SetPermissions(ClientPermissions permissions, IEnumerable<Identifier> permittedConsoleCommands)
		{
			List<DebugConsole.Command> permittedCommands = new List<DebugConsole.Command>();
			using (IEnumerator<Identifier> enumerator = permittedConsoleCommands.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Identifier commandName = enumerator.Current;
					DebugConsole.Command consoleCommand = DebugConsole.Commands.Find((DebugConsole.Command c) => c.Names.Contains(commandName));
					if (consoleCommand != null)
					{
						permittedCommands.Add(consoleCommand);
					}
				}
			}
			this.SetPermissions(permissions, permittedCommands);
		}

		// Token: 0x06004AD0 RID: 19152 RVA: 0x00291E80 File Offset: 0x00290080
		public void SetPermissions(ClientPermissions permissions, IEnumerable<DebugConsole.Command> permittedConsoleCommands)
		{
			if (GameMain.Client == null)
			{
				return;
			}
			this.Permissions = permissions;
			this.PermittedConsoleCommands.Clear();
			foreach (DebugConsole.Command command in permittedConsoleCommands)
			{
				this.PermittedConsoleCommands.Add(command);
			}
		}

		// Token: 0x06004AD1 RID: 19153 RVA: 0x00291EE8 File Offset: 0x002900E8
		public void GivePermission(ClientPermissions permission)
		{
			if (GameMain.Client == null || !GameMain.Client.HasPermission(ClientPermissions.ManagePermissions))
			{
				return;
			}
			if (!this.Permissions.HasFlag(permission))
			{
				this.Permissions |= permission;
			}
		}

		// Token: 0x06004AD2 RID: 19154 RVA: 0x00291F34 File Offset: 0x00290134
		public void RemovePermission(ClientPermissions permission)
		{
			if (GameMain.Client == null || !GameMain.Client.HasPermission(ClientPermissions.ManagePermissions))
			{
				return;
			}
			if (this.Permissions.HasFlag(permission))
			{
				this.Permissions &= ~permission;
			}
		}

		// Token: 0x06004AD3 RID: 19155 RVA: 0x00291F81 File Offset: 0x00290181
		public bool HasPermission(ClientPermissions permission)
		{
			return GameMain.Client != null && this.Permissions.HasFlag(permission);
		}

		// Token: 0x06004AD4 RID: 19156 RVA: 0x00291FA4 File Offset: 0x002901A4
		public void ResetVotes()
		{
			for (int i = 0; i < this.votes.Length; i++)
			{
				this.votes[i] = null;
			}
		}

		// Token: 0x1700131F RID: 4895
		// (get) Token: 0x06004AD5 RID: 19157 RVA: 0x00291FCD File Offset: 0x002901CD
		public Option<AccountId> AccountId
		{
			get
			{
				return this.AccountInfo.AccountId;
			}
		}

		// Token: 0x17001320 RID: 4896
		// (get) Token: 0x06004AD6 RID: 19158 RVA: 0x00291FDA File Offset: 0x002901DA
		// (set) Token: 0x06004AD7 RID: 19159 RVA: 0x00291FE4 File Offset: 0x002901E4
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

		// Token: 0x17001321 RID: 4897
		// (get) Token: 0x06004AD8 RID: 19160 RVA: 0x00292080 File Offset: 0x00290280
		// (set) Token: 0x06004AD9 RID: 19161 RVA: 0x002920D8 File Offset: 0x002902D8
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
					GameSession gameSession = GameMain.GameSession;
					if (gameSession == null)
					{
						return;
					}
					CrewManager crewManager = gameSession.CrewManager;
					if (crewManager == null)
					{
						return;
					}
					crewManager.SetPlayerVoiceIconState(this, this.muted, this.mutedLocally);
				}
			}
		}

		// Token: 0x17001322 RID: 4898
		// (get) Token: 0x06004ADA RID: 19162 RVA: 0x00292160 File Offset: 0x00290360
		// (set) Token: 0x06004ADB RID: 19163 RVA: 0x00292197 File Offset: 0x00290397
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

		// Token: 0x17001323 RID: 4899
		// (get) Token: 0x06004ADC RID: 19164 RVA: 0x002921A6 File Offset: 0x002903A6
		public bool Spectating
		{
			get
			{
				return this.inGame && this.character == null;
			}
		}

		// Token: 0x17001324 RID: 4900
		// (get) Token: 0x06004ADD RID: 19165 RVA: 0x002921BB File Offset: 0x002903BB
		// (set) Token: 0x06004ADE RID: 19166 RVA: 0x002921C4 File Offset: 0x002903C4
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
				GameMain.NetLobbyScreen.SetPlayerVoiceIconState(this, this.muted, this.mutedLocally);
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null)
				{
					CrewManager crewManager = gameSession.CrewManager;
					if (crewManager != null)
					{
						crewManager.SetPlayerVoiceIconState(this, this.muted, this.mutedLocally);
					}
				}
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					ushort lastClientListUpdateID = networkMember.LastClientListUpdateID;
					networkMember.LastClientListUpdateID = lastClientListUpdateID + 1;
				}
			}
		}

		// Token: 0x17001325 RID: 4901
		// (get) Token: 0x06004ADF RID: 19167 RVA: 0x00292249 File Offset: 0x00290449
		public bool HasPermissions
		{
			get
			{
				return this.Permissions > ClientPermissions.None;
			}
		}

		// Token: 0x17001326 RID: 4902
		// (get) Token: 0x06004AE0 RID: 19168 RVA: 0x00292254 File Offset: 0x00290454
		// (set) Token: 0x06004AE1 RID: 19169 RVA: 0x0029225C File Offset: 0x0029045C
		public VoipQueue VoipQueue { get; private set; }

		// Token: 0x17001327 RID: 4903
		// (get) Token: 0x06004AE2 RID: 19170 RVA: 0x00292265 File Offset: 0x00290465
		// (set) Token: 0x06004AE3 RID: 19171 RVA: 0x00292270 File Offset: 0x00290470
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

		// Token: 0x06004AE4 RID: 19172 RVA: 0x002922AC File Offset: 0x002904AC
		private void InitProjSpecific()
		{
			this.VoipQueue = null;
			this.VoipSound = null;
			if (this.SessionId == GameMain.Client.SessionId)
			{
				return;
			}
			this.VoipQueue = new VoipQueue(this.SessionId, false, true);
			GameClient client = GameMain.Client;
			if (client != null)
			{
				VoipClient voipClient = client.VoipClient;
				if (voipClient != null)
				{
					voipClient.RegisterQueue(this.VoipQueue);
				}
			}
			this.VoipSound = null;
		}

		// Token: 0x06004AE5 RID: 19173 RVA: 0x00292318 File Offset: 0x00290518
		private void DisposeProjSpecific()
		{
			if (this.VoipQueue != null)
			{
				GameMain.Client.VoipClient.UnregisterQueue(this.VoipQueue);
			}
			if (this.VoipSound != null)
			{
				this.VoipSound.Dispose();
				this.VoipSound = null;
			}
			if (this.radioNoiseChannel != null)
			{
				this.radioNoiseChannel.Dispose();
				this.radioNoiseChannel = null;
			}
		}

		// Token: 0x06004AE6 RID: 19174 RVA: 0x00292378 File Offset: 0x00290578
		public Client(string name, byte sessionId)
		{
			this.Name = name;
			this.SessionId = sessionId;
			this.votes = new object[Enum.GetNames(typeof(VoteType)).Length];
			this.InitProjSpecific();
		}

		// Token: 0x06004AE7 RID: 19175 RVA: 0x002923DC File Offset: 0x002905DC
		public T GetVote<T>(VoteType voteType)
		{
			object obj = this.votes[(int)voteType];
			if (obj is T)
			{
				return (T)((object)obj);
			}
			return default(T);
		}

		// Token: 0x06004AE8 RID: 19176 RVA: 0x0029240E File Offset: 0x0029060E
		public void SetVote(VoteType voteType, object value)
		{
			this.votes[(int)voteType] = value;
		}

		// Token: 0x06004AE9 RID: 19177 RVA: 0x0029241C File Offset: 0x0029061C
		public bool SessionOrAccountIdMatches(string userId)
		{
			byte sessionId;
			return (this.AccountId.IsSome() && Barotrauma.Networking.AccountId.Parse(userId) == this.AccountId) || (byte.TryParse(userId, out sessionId) && this.SessionId == sessionId);
		}

		// Token: 0x06004AEA RID: 19178 RVA: 0x00292464 File Offset: 0x00290664
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

		// Token: 0x06004AEB RID: 19179 RVA: 0x00292500 File Offset: 0x00290700
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

		// Token: 0x06004AEC RID: 19180 RVA: 0x002925A0 File Offset: 0x002907A0
		public void ReadPermissions(IReadMessage inc)
		{
			ClientPermissions permissions;
			List<DebugConsole.Command> permittedCommands;
			Client.ReadPermissions(inc, out permissions, out permittedCommands);
			this.SetPermissions(permissions, permittedCommands);
		}

		// Token: 0x06004AED RID: 19181 RVA: 0x002925C0 File Offset: 0x002907C0
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

		// Token: 0x06004AEE RID: 19182 RVA: 0x0029264B File Offset: 0x0029084B
		public void Dispose()
		{
			this.DisposeProjSpecific();
		}

		// Token: 0x04002729 RID: 10025
		public const float MaxVoiceChatBoost = 2f;

		// Token: 0x0400272A RID: 10026
		private float voiceVolume = 1f;

		// Token: 0x0400272B RID: 10027
		private SoundChannel radioNoiseChannel;

		// Token: 0x0400272C RID: 10028
		private float radioNoise;

		// Token: 0x0400272D RID: 10029
		private bool mutedLocally;

		// Token: 0x0400272E RID: 10030
		public bool IsOwner;

		// Token: 0x0400272F RID: 10031
		public bool IsDownloading;

		// Token: 0x04002730 RID: 10032
		public float Karma;

		// Token: 0x04002731 RID: 10033
		public const int MaxNameLength = 32;

		// Token: 0x04002732 RID: 10034
		public string Name;

		// Token: 0x04002733 RID: 10035
		public ushort NameId;

		// Token: 0x04002734 RID: 10036
		public readonly byte SessionId;

		// Token: 0x04002735 RID: 10037
		public AccountInfo AccountInfo;

		// Token: 0x04002736 RID: 10038
		public LanguageIdentifier Language;

		// Token: 0x04002737 RID: 10039
		public ushort Ping;

		// Token: 0x04002738 RID: 10040
		public Identifier PreferredJob;

		// Token: 0x04002739 RID: 10041
		private CharacterTeamType teamID;

		// Token: 0x0400273A RID: 10042
		public CharacterTeamType PreferredTeam;

		// Token: 0x0400273B RID: 10043
		private Character character;

		// Token: 0x0400273C RID: 10044
		public bool UsingFreeCam;

		// Token: 0x0400273D RID: 10045
		public ushort CharacterID;

		// Token: 0x0400273E RID: 10046
		private Vector2 spectatePos;

		// Token: 0x0400273F RID: 10047
		private bool muted;

		// Token: 0x04002741 RID: 10049
		private bool inGame;

		// Token: 0x04002742 RID: 10050
		public bool HasSpawned;

		// Token: 0x04002743 RID: 10051
		public HashSet<Identifier> GivenAchievements = new HashSet<Identifier>();

		// Token: 0x04002744 RID: 10052
		public ClientPermissions Permissions;

		// Token: 0x04002745 RID: 10053
		public readonly HashSet<DebugConsole.Command> PermittedConsoleCommands = new HashSet<DebugConsole.Command>();

		// Token: 0x04002746 RID: 10054
		private readonly object[] votes;
	}
}
