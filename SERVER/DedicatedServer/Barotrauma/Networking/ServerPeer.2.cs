using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x02000375 RID: 885
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class ServerPeer
	{
		// Token: 0x060034CE RID: 13518 RVA: 0x0016CAA1 File Offset: 0x0016ACA1
		protected ServerPeer(ServerPeer.Callbacks callbacks)
		{
			this.callbacks = callbacks;
		}

		// Token: 0x060034CF RID: 13519
		public abstract void Start();

		// Token: 0x060034D0 RID: 13520
		public abstract void Close();

		// Token: 0x060034D1 RID: 13521
		public abstract void Update(float deltaTime);

		// Token: 0x060034D2 RID: 13522
		public abstract void Send(IWriteMessage msg, NetworkConnection conn, DeliveryMethod deliveryMethod, bool compressPastThreshold = true);

		// Token: 0x060034D3 RID: 13523
		public abstract void Disconnect(NetworkConnection conn, PeerDisconnectPacket peerDisconnectPacket);

		// Token: 0x060034D4 RID: 13524 RVA: 0x0016CAB0 File Offset: 0x0016ACB0
		private void LogMalformedMessage(NetworkConnection conn)
		{
			foreach (Client c in GameMain.Server.ConnectedClients)
			{
				if (c.Connection == conn)
				{
					DebugConsole.ThrowError("Received malformed message from " + c.Name + ".", null, null, false, false);
					return;
				}
			}
			DebugConsole.ThrowError("Received malformed message from remote peer.", null, null, false, false);
		}

		// Token: 0x060034D5 RID: 13525 RVA: 0x0016CB34 File Offset: 0x0016AD34
		protected static void LogMalformedMessage()
		{
			DebugConsole.ThrowError("Received malformed message from remote peer.", null, null, false, false);
		}

		// Token: 0x060034D6 RID: 13526 RVA: 0x0016CB44 File Offset: 0x0016AD44
		protected bool ShouldAskForPassword(ServerSettings serverSettings, NetworkConnection connection)
		{
			if (!serverSettings.HasPassword)
			{
				return false;
			}
			GameServer server = GameMain.Server;
			return server == null || !server.FindAndRemoveRecentlyDisconnectedConnection(connection);
		}

		// Token: 0x04001A3A RID: 6714
		protected readonly ServerPeer.Callbacks callbacks;

		// Token: 0x02000C1C RID: 3100
		[Nullable(0)]
		public readonly struct Callbacks : IEquatable<ServerPeer.Callbacks>
		{
			// Token: 0x06006302 RID: 25346 RVA: 0x00210DE0 File Offset: 0x0020EFE0
			public Callbacks(ServerPeer.Callbacks.MessageCallback OnMessageReceived, ServerPeer.Callbacks.DisconnectCallback OnDisconnect, ServerPeer.Callbacks.InitializationCompleteCallback OnInitializationComplete, ServerPeer.Callbacks.ShutdownCallback OnShutdown, ServerPeer.Callbacks.OwnerDeterminedCallback OnOwnerDetermined)
			{
				this.OnMessageReceived = OnMessageReceived;
				this.OnDisconnect = OnDisconnect;
				this.OnInitializationComplete = OnInitializationComplete;
				this.OnShutdown = OnShutdown;
				this.OnOwnerDetermined = OnOwnerDetermined;
			}

			// Token: 0x1700161B RID: 5659
			// (get) Token: 0x06006303 RID: 25347 RVA: 0x00210E07 File Offset: 0x0020F007
			// (set) Token: 0x06006304 RID: 25348 RVA: 0x00210E0F File Offset: 0x0020F00F
			public ServerPeer.Callbacks.MessageCallback OnMessageReceived { get; set; }

			// Token: 0x1700161C RID: 5660
			// (get) Token: 0x06006305 RID: 25349 RVA: 0x00210E18 File Offset: 0x0020F018
			// (set) Token: 0x06006306 RID: 25350 RVA: 0x00210E20 File Offset: 0x0020F020
			public ServerPeer.Callbacks.DisconnectCallback OnDisconnect { get; set; }

			// Token: 0x1700161D RID: 5661
			// (get) Token: 0x06006307 RID: 25351 RVA: 0x00210E29 File Offset: 0x0020F029
			// (set) Token: 0x06006308 RID: 25352 RVA: 0x00210E31 File Offset: 0x0020F031
			public ServerPeer.Callbacks.InitializationCompleteCallback OnInitializationComplete { get; set; }

			// Token: 0x1700161E RID: 5662
			// (get) Token: 0x06006309 RID: 25353 RVA: 0x00210E3A File Offset: 0x0020F03A
			// (set) Token: 0x0600630A RID: 25354 RVA: 0x00210E42 File Offset: 0x0020F042
			public ServerPeer.Callbacks.ShutdownCallback OnShutdown { get; set; }

			// Token: 0x1700161F RID: 5663
			// (get) Token: 0x0600630B RID: 25355 RVA: 0x00210E4B File Offset: 0x0020F04B
			// (set) Token: 0x0600630C RID: 25356 RVA: 0x00210E53 File Offset: 0x0020F053
			public ServerPeer.Callbacks.OwnerDeterminedCallback OnOwnerDetermined { get; set; }

			// Token: 0x0600630D RID: 25357 RVA: 0x00210E5C File Offset: 0x0020F05C
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Callbacks");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x0600630E RID: 25358 RVA: 0x00210EA8 File Offset: 0x0020F0A8
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("OnMessageReceived = ");
				builder.Append(this.OnMessageReceived);
				builder.Append(", OnDisconnect = ");
				builder.Append(this.OnDisconnect);
				builder.Append(", OnInitializationComplete = ");
				builder.Append(this.OnInitializationComplete);
				builder.Append(", OnShutdown = ");
				builder.Append(this.OnShutdown);
				builder.Append(", OnOwnerDetermined = ");
				builder.Append(this.OnOwnerDetermined);
				return true;
			}

			// Token: 0x0600630F RID: 25359 RVA: 0x00210F33 File Offset: 0x0020F133
			[CompilerGenerated]
			public static bool operator !=(ServerPeer.Callbacks left, ServerPeer.Callbacks right)
			{
				return !(left == right);
			}

			// Token: 0x06006310 RID: 25360 RVA: 0x00210F3F File Offset: 0x0020F13F
			[CompilerGenerated]
			public static bool operator ==(ServerPeer.Callbacks left, ServerPeer.Callbacks right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006311 RID: 25361 RVA: 0x00210F4C File Offset: 0x0020F14C
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (((EqualityComparer<ServerPeer.Callbacks.MessageCallback>.Default.GetHashCode(this.<OnMessageReceived>k__BackingField) * -1521134295 + EqualityComparer<ServerPeer.Callbacks.DisconnectCallback>.Default.GetHashCode(this.<OnDisconnect>k__BackingField)) * -1521134295 + EqualityComparer<ServerPeer.Callbacks.InitializationCompleteCallback>.Default.GetHashCode(this.<OnInitializationComplete>k__BackingField)) * -1521134295 + EqualityComparer<ServerPeer.Callbacks.ShutdownCallback>.Default.GetHashCode(this.<OnShutdown>k__BackingField)) * -1521134295 + EqualityComparer<ServerPeer.Callbacks.OwnerDeterminedCallback>.Default.GetHashCode(this.<OnOwnerDetermined>k__BackingField);
			}

			// Token: 0x06006312 RID: 25362 RVA: 0x00210FC5 File Offset: 0x0020F1C5
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ServerPeer.Callbacks && this.Equals((ServerPeer.Callbacks)obj);
			}

			// Token: 0x06006313 RID: 25363 RVA: 0x00210FE0 File Offset: 0x0020F1E0
			[CompilerGenerated]
			public bool Equals(ServerPeer.Callbacks other)
			{
				return EqualityComparer<ServerPeer.Callbacks.MessageCallback>.Default.Equals(this.<OnMessageReceived>k__BackingField, other.<OnMessageReceived>k__BackingField) && EqualityComparer<ServerPeer.Callbacks.DisconnectCallback>.Default.Equals(this.<OnDisconnect>k__BackingField, other.<OnDisconnect>k__BackingField) && EqualityComparer<ServerPeer.Callbacks.InitializationCompleteCallback>.Default.Equals(this.<OnInitializationComplete>k__BackingField, other.<OnInitializationComplete>k__BackingField) && EqualityComparer<ServerPeer.Callbacks.ShutdownCallback>.Default.Equals(this.<OnShutdown>k__BackingField, other.<OnShutdown>k__BackingField) && EqualityComparer<ServerPeer.Callbacks.OwnerDeterminedCallback>.Default.Equals(this.<OnOwnerDetermined>k__BackingField, other.<OnOwnerDetermined>k__BackingField);
			}

			// Token: 0x06006314 RID: 25364 RVA: 0x00211065 File Offset: 0x0020F265
			[CompilerGenerated]
			public void Deconstruct(out ServerPeer.Callbacks.MessageCallback OnMessageReceived, out ServerPeer.Callbacks.DisconnectCallback OnDisconnect, out ServerPeer.Callbacks.InitializationCompleteCallback OnInitializationComplete, out ServerPeer.Callbacks.ShutdownCallback OnShutdown, out ServerPeer.Callbacks.OwnerDeterminedCallback OnOwnerDetermined)
			{
				OnMessageReceived = this.OnMessageReceived;
				OnDisconnect = this.OnDisconnect;
				OnInitializationComplete = this.OnInitializationComplete;
				OnShutdown = this.OnShutdown;
				OnOwnerDetermined = this.OnOwnerDetermined;
			}

			// Token: 0x02000EE4 RID: 3812
			// (Invoke) Token: 0x06006B51 RID: 27473
			[NullableContext(0)]
			public delegate void MessageCallback(NetworkConnection connection, IReadMessage message);

			// Token: 0x02000EE5 RID: 3813
			// (Invoke) Token: 0x06006B55 RID: 27477
			[NullableContext(0)]
			public delegate void DisconnectCallback(NetworkConnection connection, PeerDisconnectPacket peerDisconnectPacket);

			// Token: 0x02000EE6 RID: 3814
			// (Invoke) Token: 0x06006B59 RID: 27481
			[NullableContext(0)]
			public delegate void InitializationCompleteCallback(NetworkConnection connection, [Nullable(2)] string clientName);

			// Token: 0x02000EE7 RID: 3815
			// (Invoke) Token: 0x06006B5D RID: 27485
			[NullableContext(0)]
			public delegate void ShutdownCallback();

			// Token: 0x02000EE8 RID: 3816
			// (Invoke) Token: 0x06006B61 RID: 27489
			[NullableContext(0)]
			public delegate void OwnerDeterminedCallback(NetworkConnection connection);
		}
	}
}
