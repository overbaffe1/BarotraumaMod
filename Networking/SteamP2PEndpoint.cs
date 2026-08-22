using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x0200049B RID: 1179
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SteamP2PEndpoint : P2PEndpoint
	{
		// Token: 0x17001411 RID: 5137
		// (get) Token: 0x06004E57 RID: 20055 RVA: 0x002ACC86 File Offset: 0x002AAE86
		public SteamId SteamId
		{
			get
			{
				return (this.Address as SteamP2PAddress).SteamId;
			}
		}

		// Token: 0x17001412 RID: 5138
		// (get) Token: 0x06004E58 RID: 20056 RVA: 0x002ACC98 File Offset: 0x002AAE98
		public override string StringRepresentation
		{
			get
			{
				return this.SteamId.StringRepresentation;
			}
		}

		// Token: 0x17001413 RID: 5139
		// (get) Token: 0x06004E59 RID: 20057 RVA: 0x002ACCA5 File Offset: 0x002AAEA5
		public override LocalizedString ServerTypeString { get; } = TextManager.Get("PlayerHostedServer");

		// Token: 0x06004E5A RID: 20058 RVA: 0x002ACCAD File Offset: 0x002AAEAD
		public SteamP2PEndpoint(SteamId steamId) : base(new SteamP2PAddress(steamId))
		{
		}

		// Token: 0x06004E5B RID: 20059 RVA: 0x002ACCCB File Offset: 0x002AAECB
		public override int GetHashCode()
		{
			return this.SteamId.GetHashCode();
		}

		// Token: 0x06004E5C RID: 20060 RVA: 0x002ACCD8 File Offset: 0x002AAED8
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			SteamP2PEndpoint otherEndpoint = obj as SteamP2PEndpoint;
			return otherEndpoint != null && this.SteamId == otherEndpoint.SteamId;
		}

		// Token: 0x06004E5D RID: 20061 RVA: 0x002ACD04 File Offset: 0x002AAF04
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public new static Option<SteamP2PEndpoint> Parse(string endpointStr)
		{
			return from steamId in SteamId.Parse(endpointStr)
			select new SteamP2PEndpoint(steamId);
		}

		// Token: 0x06004E5E RID: 20062 RVA: 0x002ACD3E File Offset: 0x002AAF3E
		public override P2PConnection MakeConnectionFromEndpoint()
		{
			return new SteamP2PConnection(this);
		}
	}
}
