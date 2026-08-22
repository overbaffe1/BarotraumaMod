using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000498 RID: 1176
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class EosP2PEndpoint : P2PEndpoint
	{
		// Token: 0x1700140B RID: 5131
		// (get) Token: 0x06004E40 RID: 20032 RVA: 0x002AC9D0 File Offset: 0x002AABD0
		public EosInterface.ProductUserId ProductUserId
		{
			get
			{
				return new EosInterface.ProductUserId((this.Address as EosP2PAddress).EosStringRepresentation);
			}
		}

		// Token: 0x06004E41 RID: 20033 RVA: 0x002AC9E7 File Offset: 0x002AABE7
		public EosP2PEndpoint(EosInterface.ProductUserId puid) : this(new EosP2PAddress(puid.Value))
		{
		}

		// Token: 0x06004E42 RID: 20034 RVA: 0x002AC9FB File Offset: 0x002AABFB
		public EosP2PEndpoint(EosP2PAddress address) : base(address)
		{
		}

		// Token: 0x1700140C RID: 5132
		// (get) Token: 0x06004E43 RID: 20035 RVA: 0x002ACA14 File Offset: 0x002AAC14
		public override string StringRepresentation
		{
			get
			{
				return (this.Address as EosP2PAddress).StringRepresentation;
			}
		}

		// Token: 0x1700140D RID: 5133
		// (get) Token: 0x06004E44 RID: 20036 RVA: 0x002ACA26 File Offset: 0x002AAC26
		public override LocalizedString ServerTypeString { get; } = TextManager.Get("PlayerHostedServer");

		// Token: 0x06004E45 RID: 20037 RVA: 0x002ACA2E File Offset: 0x002AAC2E
		public override int GetHashCode()
		{
			return (this.Address as EosP2PAddress).GetHashCode();
		}

		// Token: 0x06004E46 RID: 20038 RVA: 0x002ACA40 File Offset: 0x002AAC40
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			EosP2PEndpoint otherEndpoint = obj as EosP2PEndpoint;
			return otherEndpoint != null && this.ProductUserId == otherEndpoint.ProductUserId;
		}

		// Token: 0x06004E47 RID: 20039 RVA: 0x002ACA6C File Offset: 0x002AAC6C
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

		// Token: 0x06004E48 RID: 20040 RVA: 0x002ACAA6 File Offset: 0x002AACA6
		public override P2PConnection MakeConnectionFromEndpoint()
		{
			return new EosP2PConnection(this);
		}

		// Token: 0x040029B0 RID: 10672
		public const string SocketName = "Barotrauma.EosP2PSocket";
	}
}
