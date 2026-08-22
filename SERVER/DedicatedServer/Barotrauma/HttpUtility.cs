using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x020002BD RID: 701
	public sealed class HttpUtility
	{
		// Token: 0x06002FB7 RID: 12215 RVA: 0x00149E5C File Offset: 0x0014805C
		public static Dictionary<Identifier, string> ParseQueryString(string query)
		{
			Dictionary<Identifier, string> collection = new Dictionary<Identifier, string>();
			string[] splitGet = query.Split('?', StringSplitOptions.None);
			if (splitGet.Length > 1)
			{
				string get = splitGet[1];
				foreach (string kvp in get.Split('&', StringSplitOptions.None))
				{
					string[] splitKeyValue = kvp.Split('=', StringSplitOptions.None);
					if (splitKeyValue.Length > 1)
					{
						collection.Add(splitKeyValue[0].ToIdentifier(), splitKeyValue[1]);
					}
				}
			}
			return collection;
		}
	}
}
