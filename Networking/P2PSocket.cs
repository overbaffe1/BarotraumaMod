using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;

namespace Barotrauma.Networking
{
	// Token: 0x02000465 RID: 1125
	internal abstract class P2PSocket : IDisposable
	{
		// Token: 0x06004BC6 RID: 19398 RVA: 0x0029CAEA File Offset: 0x0029ACEA
		protected P2PSocket(P2PSocket.Callbacks callbacks, P2PSocket.OwnerOrClient type)
		{
			this.callbacks = callbacks;
			this.Type = type;
			this.dosProtection = new P2POwnerDoSProtection(callbacks.OnExcessivePackets);
		}

		// Token: 0x06004BC7 RID: 19399
		public abstract void ProcessIncomingMessages();

		// Token: 0x06004BC8 RID: 19400
		[NullableContext(1)]
		public abstract bool SendMessage(P2PEndpoint endpoint, IWriteMessage outMsg, DeliveryMethod deliveryMethod);

		// Token: 0x06004BC9 RID: 19401
		[NullableContext(1)]
		public abstract void CloseConnection(P2PEndpoint endpoint);

		// Token: 0x06004BCA RID: 19402
		public abstract void Dispose();

		// Token: 0x040027A2 RID: 10146
		[Nullable(1)]
		public readonly P2POwnerDoSProtection dosProtection;

		// Token: 0x040027A3 RID: 10147
		public readonly P2PSocket.OwnerOrClient Type;

		// Token: 0x040027A4 RID: 10148
		protected readonly P2PSocket.Callbacks callbacks;

		// Token: 0x020011ED RID: 4589
		public enum OwnerOrClient
		{
			// Token: 0x04005DA4 RID: 23972
			Client,
			// Token: 0x04005DA5 RID: 23973
			Owner
		}

		// Token: 0x020011EE RID: 4590
		public enum ErrorCode
		{
			// Token: 0x04005DA7 RID: 23975
			EosNotInitialized,
			// Token: 0x04005DA8 RID: 23976
			EosNotLoggedIn,
			// Token: 0x04005DA9 RID: 23977
			FailedToCreateEosP2PSocket,
			// Token: 0x04005DAA RID: 23978
			SteamNotInitialized,
			// Token: 0x04005DAB RID: 23979
			FailedToCreateSteamP2PSocket
		}

		// Token: 0x020011EF RID: 4591
		public readonly struct Error : IEquatable<P2PSocket.Error>
		{
			// Token: 0x0600928C RID: 37516 RVA: 0x003C95F6 File Offset: 0x003C77F6
			public Error([TupleElementNames(new string[]
			{
				"Code",
				"AdditionalInfo"
			})] [Nullable(new byte[]
			{
				0,
				0,
				1
			})] ImmutableArray<ValueTuple<P2PSocket.ErrorCode, string>> CodesAndInfo)
			{
				this.CodesAndInfo = CodesAndInfo;
			}

			// Token: 0x17001CD6 RID: 7382
			// (get) Token: 0x0600928D RID: 37517 RVA: 0x003C95FF File Offset: 0x003C77FF
			// (set) Token: 0x0600928E RID: 37518 RVA: 0x003C9607 File Offset: 0x003C7807
			[TupleElementNames(new string[]
			{
				"Code",
				"AdditionalInfo"
			})]
			[Nullable(new byte[]
			{
				0,
				0,
				1
			})]
			public ImmutableArray<ValueTuple<P2PSocket.ErrorCode, string>> CodesAndInfo { [return: TupleElementNames(new string[]
			{
				"Code",
				"AdditionalInfo"
			})] [return: Nullable(new byte[]
			{
				0,
				0,
				1
			})] get; [param: TupleElementNames(new string[]
			{
				"Code",
				"AdditionalInfo"
			})] [param: Nullable(new byte[]
			{
				0,
				0,
				1
			})] set; }

			// Token: 0x0600928F RID: 37519 RVA: 0x003C9610 File Offset: 0x003C7810
			[NullableContext(2)]
			public Error(P2PSocket.ErrorCode code, string additionalInfo = "")
			{
				this = new P2PSocket.Error(new ValueTuple<P2PSocket.ErrorCode, string>(code, additionalInfo ?? "").ToEnumerable<ValueTuple<P2PSocket.ErrorCode, string>>().ToImmutableArray<ValueTuple<P2PSocket.ErrorCode, string>>());
			}

			// Token: 0x06009290 RID: 37520 RVA: 0x003C9632 File Offset: 0x003C7832
			[NullableContext(1)]
			public Error(params P2PSocket.Error[] innerErrors)
			{
				this = new P2PSocket.Error(innerErrors.SelectMany((P2PSocket.Error ie) => ie.CodesAndInfo).ToImmutableArray<ValueTuple<P2PSocket.ErrorCode, string>>());
			}

			// Token: 0x06009291 RID: 37521 RVA: 0x003C9664 File Offset: 0x003C7864
			[NullableContext(2)]
			public override string ToString()
			{
				if (this.CodesAndInfo.IsDefault)
				{
					return "default(Error)";
				}
				return "Errors(" + string.Join<ValueTuple<P2PSocket.ErrorCode, string>>("; ", this.CodesAndInfo) + ")";
			}

			// Token: 0x06009292 RID: 37522 RVA: 0x003C96AC File Offset: 0x003C78AC
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("CodesAndInfo = ");
				builder.Append(this.CodesAndInfo.ToString());
				return true;
			}

			// Token: 0x06009293 RID: 37523 RVA: 0x003C96E1 File Offset: 0x003C78E1
			[CompilerGenerated]
			public static bool operator !=(P2PSocket.Error left, P2PSocket.Error right)
			{
				return !(left == right);
			}

			// Token: 0x06009294 RID: 37524 RVA: 0x003C96ED File Offset: 0x003C78ED
			[CompilerGenerated]
			public static bool operator ==(P2PSocket.Error left, P2PSocket.Error right)
			{
				return left.Equals(right);
			}

			// Token: 0x06009295 RID: 37525 RVA: 0x003C96F7 File Offset: 0x003C78F7
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<ImmutableArray<ValueTuple<P2PSocket.ErrorCode, string>>>.Default.GetHashCode(this.<CodesAndInfo>k__BackingField);
			}

			// Token: 0x06009296 RID: 37526 RVA: 0x003C9709 File Offset: 0x003C7909
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is P2PSocket.Error && this.Equals((P2PSocket.Error)obj);
			}

			// Token: 0x06009297 RID: 37527 RVA: 0x003C9721 File Offset: 0x003C7921
			[CompilerGenerated]
			public bool Equals(P2PSocket.Error other)
			{
				return EqualityComparer<ImmutableArray<ValueTuple<P2PSocket.ErrorCode, string>>>.Default.Equals(this.<CodesAndInfo>k__BackingField, other.<CodesAndInfo>k__BackingField);
			}

			// Token: 0x06009298 RID: 37528 RVA: 0x003C9739 File Offset: 0x003C7939
			[CompilerGenerated]
			public void Deconstruct([TupleElementNames(new string[]
			{
				"Code",
				"AdditionalInfo"
			})] [Nullable(new byte[]
			{
				0,
				0,
				1
			})] out ImmutableArray<ValueTuple<P2PSocket.ErrorCode, string>> CodesAndInfo)
			{
				CodesAndInfo = this.CodesAndInfo;
			}
		}

		// Token: 0x020011F0 RID: 4592
		[NullableContext(1)]
		[Nullable(0)]
		public readonly struct Callbacks : IEquatable<P2PSocket.Callbacks>
		{
			// Token: 0x06009299 RID: 37529 RVA: 0x003C9747 File Offset: 0x003C7947
			public Callbacks(Predicate<P2PEndpoint> OnIncomingConnection, Action<P2PEndpoint, PeerDisconnectPacket> OnConnectionClosed, P2POwnerDoSProtection.ExcessivePacketDelegate OnExcessivePackets, Action<P2PEndpoint, IReadMessage> OnData)
			{
				this.OnIncomingConnection = OnIncomingConnection;
				this.OnConnectionClosed = OnConnectionClosed;
				this.OnExcessivePackets = OnExcessivePackets;
				this.OnData = OnData;
			}

			// Token: 0x17001CD7 RID: 7383
			// (get) Token: 0x0600929A RID: 37530 RVA: 0x003C9766 File Offset: 0x003C7966
			// (set) Token: 0x0600929B RID: 37531 RVA: 0x003C976E File Offset: 0x003C796E
			public Predicate<P2PEndpoint> OnIncomingConnection { get; set; }

			// Token: 0x17001CD8 RID: 7384
			// (get) Token: 0x0600929C RID: 37532 RVA: 0x003C9777 File Offset: 0x003C7977
			// (set) Token: 0x0600929D RID: 37533 RVA: 0x003C977F File Offset: 0x003C797F
			public Action<P2PEndpoint, PeerDisconnectPacket> OnConnectionClosed { get; set; }

			// Token: 0x17001CD9 RID: 7385
			// (get) Token: 0x0600929E RID: 37534 RVA: 0x003C9788 File Offset: 0x003C7988
			// (set) Token: 0x0600929F RID: 37535 RVA: 0x003C9790 File Offset: 0x003C7990
			public P2POwnerDoSProtection.ExcessivePacketDelegate OnExcessivePackets { get; set; }

			// Token: 0x17001CDA RID: 7386
			// (get) Token: 0x060092A0 RID: 37536 RVA: 0x003C9799 File Offset: 0x003C7999
			// (set) Token: 0x060092A1 RID: 37537 RVA: 0x003C97A1 File Offset: 0x003C79A1
			public Action<P2PEndpoint, IReadMessage> OnData { get; set; }

			// Token: 0x060092A2 RID: 37538 RVA: 0x003C97AC File Offset: 0x003C79AC
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

			// Token: 0x060092A3 RID: 37539 RVA: 0x003C97F8 File Offset: 0x003C79F8
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("OnIncomingConnection = ");
				builder.Append(this.OnIncomingConnection);
				builder.Append(", OnConnectionClosed = ");
				builder.Append(this.OnConnectionClosed);
				builder.Append(", OnExcessivePackets = ");
				builder.Append(this.OnExcessivePackets);
				builder.Append(", OnData = ");
				builder.Append(this.OnData);
				return true;
			}

			// Token: 0x060092A4 RID: 37540 RVA: 0x003C986A File Offset: 0x003C7A6A
			[CompilerGenerated]
			public static bool operator !=(P2PSocket.Callbacks left, P2PSocket.Callbacks right)
			{
				return !(left == right);
			}

			// Token: 0x060092A5 RID: 37541 RVA: 0x003C9876 File Offset: 0x003C7A76
			[CompilerGenerated]
			public static bool operator ==(P2PSocket.Callbacks left, P2PSocket.Callbacks right)
			{
				return left.Equals(right);
			}

			// Token: 0x060092A6 RID: 37542 RVA: 0x003C9880 File Offset: 0x003C7A80
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<Predicate<P2PEndpoint>>.Default.GetHashCode(this.<OnIncomingConnection>k__BackingField) * -1521134295 + EqualityComparer<Action<P2PEndpoint, PeerDisconnectPacket>>.Default.GetHashCode(this.<OnConnectionClosed>k__BackingField)) * -1521134295 + EqualityComparer<P2POwnerDoSProtection.ExcessivePacketDelegate>.Default.GetHashCode(this.<OnExcessivePackets>k__BackingField)) * -1521134295 + EqualityComparer<Action<P2PEndpoint, IReadMessage>>.Default.GetHashCode(this.<OnData>k__BackingField);
			}

			// Token: 0x060092A7 RID: 37543 RVA: 0x003C98E2 File Offset: 0x003C7AE2
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is P2PSocket.Callbacks && this.Equals((P2PSocket.Callbacks)obj);
			}

			// Token: 0x060092A8 RID: 37544 RVA: 0x003C98FC File Offset: 0x003C7AFC
			[CompilerGenerated]
			public bool Equals(P2PSocket.Callbacks other)
			{
				return EqualityComparer<Predicate<P2PEndpoint>>.Default.Equals(this.<OnIncomingConnection>k__BackingField, other.<OnIncomingConnection>k__BackingField) && EqualityComparer<Action<P2PEndpoint, PeerDisconnectPacket>>.Default.Equals(this.<OnConnectionClosed>k__BackingField, other.<OnConnectionClosed>k__BackingField) && EqualityComparer<P2POwnerDoSProtection.ExcessivePacketDelegate>.Default.Equals(this.<OnExcessivePackets>k__BackingField, other.<OnExcessivePackets>k__BackingField) && EqualityComparer<Action<P2PEndpoint, IReadMessage>>.Default.Equals(this.<OnData>k__BackingField, other.<OnData>k__BackingField);
			}

			// Token: 0x060092A9 RID: 37545 RVA: 0x003C9969 File Offset: 0x003C7B69
			[CompilerGenerated]
			public void Deconstruct(out Predicate<P2PEndpoint> OnIncomingConnection, out Action<P2PEndpoint, PeerDisconnectPacket> OnConnectionClosed, out P2POwnerDoSProtection.ExcessivePacketDelegate OnExcessivePackets, out Action<P2PEndpoint, IReadMessage> OnData)
			{
				OnIncomingConnection = this.OnIncomingConnection;
				OnConnectionClosed = this.OnConnectionClosed;
				OnExcessivePackets = this.OnExcessivePackets;
				OnData = this.OnData;
			}
		}
	}
}
