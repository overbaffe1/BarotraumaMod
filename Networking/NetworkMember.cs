using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x0200048F RID: 1167
	internal abstract class NetworkMember
	{
		// Token: 0x170013FC RID: 5116
		// (get) Token: 0x06004DF8 RID: 19960 RVA: 0x002AC2B3 File Offset: 0x002AA4B3
		// (set) Token: 0x06004DF9 RID: 19961 RVA: 0x002AC2BB File Offset: 0x002AA4BB
		public ushort LastClientListUpdateID { get; set; }

		// Token: 0x170013FD RID: 5117
		// (get) Token: 0x06004DFA RID: 19962
		public abstract bool IsServer { get; }

		// Token: 0x170013FE RID: 5118
		// (get) Token: 0x06004DFB RID: 19963
		public abstract bool IsClient { get; }

		// Token: 0x06004DFC RID: 19964
		public abstract void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData = null);

		// Token: 0x170013FF RID: 5119
		// (get) Token: 0x06004DFD RID: 19965
		public abstract Voting Voting { get; }

		// Token: 0x17001400 RID: 5120
		// (get) Token: 0x06004DFE RID: 19966 RVA: 0x002AC2C4 File Offset: 0x002AA4C4
		// (set) Token: 0x06004DFF RID: 19967 RVA: 0x002AC2CC File Offset: 0x002AA4CC
		public KarmaManager KarmaManager { get; private set; } = new KarmaManager();

		// Token: 0x17001401 RID: 5121
		// (get) Token: 0x06004E00 RID: 19968 RVA: 0x002AC2D5 File Offset: 0x002AA4D5
		// (set) Token: 0x06004E01 RID: 19969 RVA: 0x002AC2DD File Offset: 0x002AA4DD
		public bool GameStarted { get; protected set; }

		// Token: 0x17001402 RID: 5122
		// (get) Token: 0x06004E02 RID: 19970
		public abstract IReadOnlyList<Client> ConnectedClients { get; }

		// Token: 0x17001403 RID: 5123
		// (get) Token: 0x06004E03 RID: 19971 RVA: 0x002AC2E6 File Offset: 0x002AA4E6
		// (set) Token: 0x06004E04 RID: 19972 RVA: 0x002AC2EE File Offset: 0x002AA4EE
		public RespawnManager RespawnManager { get; protected set; }

		// Token: 0x17001404 RID: 5124
		// (get) Token: 0x06004E05 RID: 19973 RVA: 0x002AC2F7 File Offset: 0x002AA4F7
		// (set) Token: 0x06004E06 RID: 19974 RVA: 0x002AC2FF File Offset: 0x002AA4FF
		public ServerSettings ServerSettings { get; protected set; }

		// Token: 0x17001405 RID: 5125
		// (get) Token: 0x06004E07 RID: 19975 RVA: 0x002AC308 File Offset: 0x002AA508
		public TimeSpan UpdateInterval
		{
			get
			{
				return new TimeSpan(0, 0, 0, 0, MathHelper.Clamp(1000 / this.ServerSettings.TickRate, 1, 500));
			}
		}

		// Token: 0x06004E08 RID: 19976 RVA: 0x002AC32F File Offset: 0x002AA52F
		public void AddChatMessage(string message, ChatMessageType type, string senderName = "", Client senderClient = null, Entity senderEntity = null, PlayerConnectionChangeType changeType = PlayerConnectionChangeType.None, Color? textColor = null)
		{
			this.AddChatMessage(ChatMessage.Create(senderName, message, type, senderEntity, senderClient, changeType, textColor));
		}

		// Token: 0x06004E09 RID: 19977
		public abstract void AddChatMessage(ChatMessage message);

		// Token: 0x06004E0A RID: 19978 RVA: 0x002AC348 File Offset: 0x002AA548
		public static string ClientLogName(Client client, string name = null)
		{
			if (client == null)
			{
				return name;
			}
			string retVal = "‖";
			if (client.Karma < 40f)
			{
				retVal += "color:#ff9900;";
			}
			AccountId accountId;
			return string.Concat(new string[]
			{
				retVal,
				"metadata:",
				client.AccountId.TryUnwrap(out accountId) ? accountId.ToString() : client.SessionId.ToString(),
				"‖",
				(name ?? client.Name).Replace("‖", ""),
				"‖end‖"
			});
		}

		// Token: 0x06004E0B RID: 19979
		public abstract void KickPlayer(string kickedName, string reason);

		// Token: 0x06004E0C RID: 19980
		public abstract void BanPlayer(string kickedName, string reason, TimeSpan? duration = null);

		// Token: 0x06004E0D RID: 19981
		public abstract void UnbanPlayer(string playerName);

		// Token: 0x06004E0E RID: 19982
		public abstract void UnbanPlayer(Endpoint endpoint);

		// Token: 0x06004E0F RID: 19983 RVA: 0x002AC3E7 File Offset: 0x002AA5E7
		public static bool IsCompatible(Version myVersion, Version remoteVersion)
		{
			return myVersion.Major == remoteVersion.Major && myVersion.Minor == remoteVersion.Minor && myVersion.Build == remoteVersion.Build;
		}

		// Token: 0x04002996 RID: 10646
		protected const int MaxSubNameLengthInErrorMessages = 16;

		// Token: 0x04002998 RID: 10648
		protected DateTime updateTimer;

		// Token: 0x04002999 RID: 10649
		public bool ShowNetStats;

		// Token: 0x0400299A RID: 10650
		public float SimulatedRandomLatency;

		// Token: 0x0400299B RID: 10651
		public float SimulatedMinimumLatency;

		// Token: 0x0400299C RID: 10652
		public float SimulatedLoss;

		// Token: 0x0400299D RID: 10653
		public float SimulatedDuplicatesChance;
	}
}
