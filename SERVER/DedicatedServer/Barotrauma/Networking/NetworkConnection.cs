using System;

namespace Barotrauma.Networking
{
	// Token: 0x020003AE RID: 942
	public abstract class NetworkConnection<T> : NetworkConnection where T : Endpoint
	{
		// Token: 0x06003768 RID: 14184 RVA: 0x00175B16 File Offset: 0x00173D16
		protected NetworkConnection(T endpoint) : base(endpoint)
		{
		}

		// Token: 0x17000F37 RID: 3895
		// (get) Token: 0x06003769 RID: 14185 RVA: 0x00175B24 File Offset: 0x00173D24
		public new T Endpoint
		{
			get
			{
				return this.Endpoint as T;
			}
		}
	}
}
