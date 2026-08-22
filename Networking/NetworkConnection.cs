using System;

namespace Barotrauma.Networking
{
	// Token: 0x020004AB RID: 1195
	public abstract class NetworkConnection<T> : NetworkConnection where T : Endpoint
	{
		// Token: 0x06004F3D RID: 20285 RVA: 0x002AE652 File Offset: 0x002AC852
		protected NetworkConnection(T endpoint) : base(endpoint)
		{
		}

		// Token: 0x17001432 RID: 5170
		// (get) Token: 0x06004F3E RID: 20286 RVA: 0x002AE660 File Offset: 0x002AC860
		public new T Endpoint
		{
			get
			{
				return this.Endpoint as T;
			}
		}
	}
}
