using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000389 RID: 905
	internal static class NetIdUtils
	{
		// Token: 0x06003626 RID: 13862 RVA: 0x001735A4 File Offset: 0x001717A4
		public static bool IdMoreRecent(ushort newID, ushort oldID)
		{
			return (newID > oldID && newID - oldID <= 32767) || (oldID > newID && oldID - newID > 32767);
		}

		// Token: 0x06003627 RID: 13863 RVA: 0x001735D5 File Offset: 0x001717D5
		public static bool IdMoreRecentOrMatches(ushort newId, ushort oldId)
		{
			return !NetIdUtils.IdMoreRecent(oldId, newId);
		}

		// Token: 0x06003628 RID: 13864 RVA: 0x001735E1 File Offset: 0x001717E1
		public static ushort GetIdOlderThan(ushort id)
		{
			return id - 1;
		}

		// Token: 0x06003629 RID: 13865 RVA: 0x001735E8 File Offset: 0x001717E8
		public static ushort Difference(ushort id1, ushort id2)
		{
			int diff = (int)((id2 > id1) ? (id2 - id1) : (id1 - id2));
			return (ushort)((diff > 32767) ? (65535 - diff) : diff);
		}

		// Token: 0x0600362A RID: 13866 RVA: 0x00173618 File Offset: 0x00171818
		public static ushort Clamp(ushort id, ushort min, ushort max)
		{
			if (NetIdUtils.IdMoreRecent(min, max))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Min cannot be larger than max (");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(min);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(max);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (!NetIdUtils.IdMoreRecent(id, min))
			{
				return min;
			}
			if (NetIdUtils.IdMoreRecent(id, max))
			{
				return max;
			}
			return id;
		}

		// Token: 0x0600362B RID: 13867 RVA: 0x00173690 File Offset: 0x00171890
		public static bool IsValidId(ushort currentId, ushort previousId, ushort latestPossibleId)
		{
			return !NetIdUtils.IdMoreRecent(currentId, latestPossibleId) && (NetIdUtils.IdMoreRecent(currentId, previousId) || (previousId == 0 && currentId > 32767));
		}
	}
}
