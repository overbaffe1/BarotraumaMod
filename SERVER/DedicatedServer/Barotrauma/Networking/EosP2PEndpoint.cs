using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x0200039B RID: 923
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class EosP2PEndpoint : P2PEndpoint
	{
		// Token: 0x17000F10 RID: 3856
		// (get) Token: 0x0600366B RID: 13931 RVA: 0x00173E94 File Offset: 0x00172094
		public EosInterface.ProductUserId ProductUserId
		{
			get
			{
				return new EosInterface.ProductUserId((this.Address as EosP2PAddress).EosStringRepresentation);
			}
		}

		// Token: 0x0600366C RID: 13932 RVA: 0x00173EAB File Offset: 0x001720AB
		public EosP2PEndpoint(EosInterface.ProductUserId puid) : this(new EosP2PAddress(puid.Value))
		{
		}

		// Token: 0x0600366D RID: 13933 RVA: 0x00173EBF File Offset: 0x001720BF
		public EosP2PEndpoint(EosP2PAddress address) : base(address)
		{
		}

		// Token: 0x17000F11 RID: 3857
		// (get) Token: 0x0600366E RID: 13934 RVA: 0x00173ED8 File Offset: 0x001720D8
		public override string StringRepresentation
		{
			get
			{
				return (this.Address as EosP2PAddress).StringRepresentation;
			}
		}

		// Token: 0x17000F12 RID: 3858
		// (get) Token: 0x0600366F RID: 13935 RVA: 0x00173EEA File Offset: 0x001720EA
		public override LocalizedString ServerTypeString { get; } = TextManager.Get("PlayerHostedServer");

		// Token: 0x06003670 RID: 13936 RVA: 0x00173EF2 File Offset: 0x001720F2
		public override int GetHashCode()
		{
			return (this.Address as EosP2PAddress).GetHashCode();
		}

		// Token: 0x06003671 RID: 13937 RVA: 0x00173F04 File Offset: 0x00172104
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			EosP2PEndpoint otherEndpoint = obj as EosP2PEndpoint;
			return otherEndpoint != null && this.ProductUserId == otherEndpoint.ProductUserId;
		}

		// Token: 0x06003672 RID: 13938 RVA: 0x00173F30 File Offset: 0x00172130
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public new static Option<EosP2PEndpoint> Parse(string endpointStr)
		{
			return from eosAddress in EosP2PAddress.Parse(endpointStr)
			select new EosP2PEndpoint(eosAddress);
		}

		// Token: 0x06003673 RID: 13939 RVA: 0x00173F6A File Offset: 0x0017216A
		public override P2PConnection MakeConnectionFromEndpoint()
		{
			return new EosP2PConnection(this);
		}

		// Token: 0x04001BB9 RID: 7097
		public const string SocketName = "Barotrauma.EosP2PSocket";
	}
}
