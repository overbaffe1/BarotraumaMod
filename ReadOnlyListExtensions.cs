using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000394 RID: 916
	public static class ReadOnlyListExtensions
	{
		// Token: 0x060044B0 RID: 17584 RVA: 0x00264D3C File Offset: 0x00262F3C
		public static int IndexOf<T>(this IReadOnlyList<T> list, T elem)
		{
			return list.IndexOf((T input) => input.Equals(elem));
		}

		// Token: 0x060044B1 RID: 17585 RVA: 0x00264D68 File Offset: 0x00262F68
		public static int IndexOf<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (predicate(list[i]))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060044B2 RID: 17586 RVA: 0x00264D98 File Offset: 0x00262F98
		public static T Find<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
		{
			return list.FirstOrDefault(predicate);
		}
	}
}
