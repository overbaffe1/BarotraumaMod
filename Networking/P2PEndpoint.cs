using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x0200049A RID: 1178
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class P2PEndpoint : Endpoint
	{
		// Token: 0x06004E54 RID: 20052 RVA: 0x002ACC40 File Offset: 0x002AAE40
		protected P2PEndpoint(P2PAddress address) : base(address)
		{
		}

		// Token: 0x06004E55 RID: 20053
		public abstract P2PConnection MakeConnectionFromEndpoint();

		// Token: 0x06004E56 RID: 20054 RVA: 0x002ACC4C File Offset: 0x002AAE4C
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
