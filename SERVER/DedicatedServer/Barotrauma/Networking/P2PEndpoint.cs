using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x0200039D RID: 925
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class P2PEndpoint : Endpoint
	{
		// Token: 0x0600367F RID: 13951 RVA: 0x00174104 File Offset: 0x00172304
		protected P2PEndpoint(P2PAddress address) : base(address)
		{
		}

		// Token: 0x06003680 RID: 13952
		public abstract P2PConnection MakeConnectionFromEndpoint();

		// Token: 0x06003681 RID: 13953 RVA: 0x00174110 File Offset: 0x00172310
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public new static Option<P2PEndpoint> Parse(string str)
		{
			return Endpoint.Parse(str).Bind<P2PEndpoint>(delegate(Endpoint ep)
			{
				P2PEndpoint pep = ep as P2PEndpoint;
				if (pep == null)
				{
					Option.UnspecifiedNone none = Option.None;
					return none;
				}
				return Option.Some<P2PEndpoint>(pep);
			});
		}
	}
}
