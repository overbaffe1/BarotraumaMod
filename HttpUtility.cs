using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x02000388 RID: 904
	public sealed class HttpUtility
	{
		// Token: 0x06004435 RID: 17461 RVA: 0x00263900 File Offset: 0x00261B00
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
