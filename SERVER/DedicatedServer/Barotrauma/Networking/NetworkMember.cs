using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000370 RID: 880
	internal abstract class NetworkMember
	{
		// Token: 0x17000E7F RID: 3711
		// (get) Token: 0x06003480 RID: 13440 RVA: 0x00169D20 File Offset: 0x00167F20
		public Character Character
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000E80 RID: 3712
		// (get) Token: 0x06003481 RID: 13441 RVA: 0x00169D23 File Offset: 0x00167F23
		// (set) Token: 0x06003482 RID: 13442 RVA: 0x00169D2B File Offset: 0x00167F2B
		public ushort LastClientListUpdateID { get; set; }

		// Token: 0x17000E81 RID: 3713
		// (get) Token: 0x06003483 RID: 13443
		public abstract bool IsServer { get; }

		// Token: 0x17000E82 RID: 3714
		// (get) Token: 0x06003484 RID: 13444
		public abstract bool IsClient { get; }

		// Token: 0x06003485 RID: 13445
		public abstract void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData = null);

		// Token: 0x17000E83 RID: 3715
		// (get) Token: 0x06003486 RID: 13446
		public abstract Voting Voting { get; }

		// Token: 0x17000E84 RID: 3716
		// (get) Token: 0x06003487 RID: 13447 RVA: 0x00169D34 File Offset: 0x00167F34
		// (set) Token: 0x06003488 RID: 13448 RVA: 0x00169D3C File Offset: 0x00167F3C
		public KarmaManager KarmaManager { get; private set; } = new KarmaManager();

		// Token: 0x17000E85 RID: 3717
		// (get) Token: 0x06003489 RID: 13449 RVA: 0x00169D45 File Offset: 0x00167F45
		// (set) Token: 0x0600348A RID: 13450 RVA: 0x00169D4D File Offset: 0x00167F4D
		public bool GameStarted { get; protected set; }

		// Token: 0x17000E86 RID: 3718
		// (get) Token: 0x0600348B RID: 13451
		public abstract IReadOnlyList<Client> ConnectedClients { get; }

		// Token: 0x17000E87 RID: 3719
		// (get) Token: 0x0600348C RID: 13452 RVA: 0x00169D56 File Offset: 0x00167F56
		// (set) Token: 0x0600348D RID: 13453 RVA: 0x00169D5E File Offset: 0x00167F5E
		public RespawnManager RespawnManager { get; protected set; }

		// Token: 0x17000E88 RID: 3720
		// (get) Token: 0x0600348E RID: 13454 RVA: 0x00169D67 File Offset: 0x00167F67
		// (set) Token: 0x0600348F RID: 13455 RVA: 0x00169D6F File Offset: 0x00167F6F
		public ServerSettings ServerSettings { get; protected set; }

		// Token: 0x17000E89 RID: 3721
		// (get) Token: 0x06003490 RID: 13456 RVA: 0x00169D78 File Offset: 0x00167F78
		public TimeSpan UpdateInterval
		{
			get
			{
				return new TimeSpan(0, 0, 0, 0, MathHelper.Clamp(1000 / this.ServerSettings.TickRate, 1, 500));
			}
		}

		// Token: 0x06003491 RID: 13457 RVA: 0x00169D9F File Offset: 0x00167F9F
		public void AddChatMessage(string message, ChatMessageType type, string senderName = "", Client senderClient = null, Entity senderEntity = null, PlayerConnectionChangeType changeType = PlayerConnectionChangeType.None, Color? textColor = null)
		{
			this.AddChatMessage(ChatMessage.Create(senderName, message, type, senderEntity, senderClient, changeType, textColor));
		}

		// Token: 0x06003492 RID: 13458
		public abstract void AddChatMessage(ChatMessage message);

		// Token: 0x06003493 RID: 13459 RVA: 0x00169DB8 File Offset: 0x00167FB8
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

		// Token: 0x06003494 RID: 13460
		public abstract void KickPlayer(string kickedName, string reason);

		// Token: 0x06003495 RID: 13461
		public abstract void BanPlayer(string kickedName, string reason, TimeSpan? duration = null);

		// Token: 0x06003496 RID: 13462
		public abstract void UnbanPlayer(string playerName);

		// Token: 0x06003497 RID: 13463
		public abstract void UnbanPlayer(Endpoint endpoint);

		// Token: 0x06003498 RID: 13464 RVA: 0x00169E57 File Offset: 0x00168057
		public static bool IsCompatible(Version myVersion, Version remoteVersion)
		{
			return myVersion.Major == remoteVersion.Major && myVersion.Minor == remoteVersion.Minor && myVersion.Build == remoteVersion.Build;
		}

		// Token: 0x04001A1F RID: 6687
		protected const int MaxSubNameLengthInErrorMessages = 16;

		// Token: 0x04001A21 RID: 6689
		protected DateTime updateTimer;

		// Token: 0x04001A22 RID: 6690
		public bool ShowNetStats;

		// Token: 0x04001A23 RID: 6691
		public float SimulatedRandomLatency;

		// Token: 0x04001A24 RID: 6692
		public float SimulatedMinimumLatency;

		// Token: 0x04001A25 RID: 6693
		public float SimulatedLoss;

		// Token: 0x04001A26 RID: 6694
		public float SimulatedDuplicatesChance;
	}
}
