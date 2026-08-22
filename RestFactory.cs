using System;
using RestSharp;

namespace Barotrauma
{
	// Token: 0x02000395 RID: 917
	public static class RestFactory
	{
		// Token: 0x060044B3 RID: 17587 RVA: 0x00264DA1 File Offset: 0x00262FA1
		public static RestClient CreateClient(string baseUrl)
		{
			return new RestClient(baseUrl)
			{
				Timeout = GameSettings.CurrentConfig.RemoteContentTimeoutMs
			};
		}

		// Token: 0x060044B4 RID: 17588 RVA: 0x00264DB9 File Offset: 0x00262FB9
		public static RestRequest CreateRequest(string resource, Method method = Method.GET)
		{
			return new RestRequest(resource, method)
			{
				Timeout = GameSettings.CurrentConfig.RemoteContentTimeoutMs
			};
		}
	}
}
