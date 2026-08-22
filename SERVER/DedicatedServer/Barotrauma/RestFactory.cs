using System;
using RestSharp;

namespace Barotrauma
{
	// Token: 0x020002CA RID: 714
	public static class RestFactory
	{
		// Token: 0x06003035 RID: 12341 RVA: 0x0014B2DD File Offset: 0x001494DD
		public static RestClient CreateClient(string baseUrl)
		{
			return new RestClient(baseUrl)
			{
				Timeout = GameSettings.CurrentConfig.RemoteContentTimeoutMs
			};
		}

		// Token: 0x06003036 RID: 12342 RVA: 0x0014B2F5 File Offset: 0x001494F5
		public static RestRequest CreateRequest(string resource, Method method = Method.GET)
		{
			return new RestRequest(resource, method)
			{
				Timeout = GameSettings.CurrentConfig.RemoteContentTimeoutMs
			};
		}
	}
}
