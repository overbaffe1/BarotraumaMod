using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x0200039E RID: 926
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SteamP2PEndpoint : P2PEndpoint
	{
		// Token: 0x17000F16 RID: 3862
		// (get) Token: 0x06003682 RID: 13954 RVA: 0x0017414A File Offset: 0x0017234A
		public SteamId SteamId
		{
			get
			{
				return (this.Address as SteamP2PAddress).SteamId;
			}
		}

		// Token: 0x17000F17 RID: 3863
		// (get) Token: 0x06003683 RID: 13955 RVA: 0x0017415C File Offset: 0x0017235C
		public override string StringRepresentation
		{
			get
			{
				return this.SteamId.StringRepresentation;
			}
		}

		// Token: 0x17000F18 RID: 3864
		// (get) Token: 0x06003684 RID: 13956 RVA: 0x00174169 File Offset: 0x00172369
		public override LocalizedString ServerTypeString { get; } = TextManager.Get("PlayerHostedServer");

		// Token: 0x06003685 RID: 13957 RVA: 0x00174171 File Offset: 0x00172371
		public SteamP2PEndpoint(SteamId steamId) : base(new SteamP2PAddress(steamId))
		{
		}

		// Token: 0x06003686 RID: 13958 RVA: 0x0017418F File Offset: 0x0017238F
		public override int GetHashCode()
		{
			return this.SteamId.GetHashCode();
		}

		// Token: 0x06003687 RID: 13959 RVA: 0x0017419C File Offset: 0x0017239C
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			SteamP2PEndpoint otherEndpoint = obj as SteamP2PEndpoint;
			return otherEndpoint != null && this.SteamId == otherEndpoint.SteamId;
		}

		// Token: 0x06003688 RID: 13960 RVA: 0x001741C8 File Offset: 0x001723C8
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

		// Token: 0x06003689 RID: 13961 RVA: 0x00174202 File Offset: 0x00172402
		public override P2PConnection MakeConnectionFromEndpoint()
		{
			return new SteamP2PConnection(this);
		}
	}
}
