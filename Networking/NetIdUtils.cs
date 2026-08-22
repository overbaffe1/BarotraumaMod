using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000485 RID: 1157
	internal static class NetIdUtils
	{
		// Token: 0x06004DE2 RID: 19938 RVA: 0x002ABF6C File Offset: 0x002AA16C
		public static bool IdMoreRecent(ushort newID, ushort oldID)
		{
			return (newID > oldID && newID - oldID <= 32767) || (oldID > newID && oldID - newID > 32767);
		}

		// Token: 0x06004DE3 RID: 19939 RVA: 0x002ABF9D File Offset: 0x002AA19D
		public static bool IdMoreRecentOrMatches(ushort newId, ushort oldId)
		{
			return !NetIdUtils.IdMoreRecent(oldId, newId);
		}

		// Token: 0x06004DE4 RID: 19940 RVA: 0x002ABFA9 File Offset: 0x002AA1A9
		public static ushort GetIdOlderThan(ushort id)
		{
			return id - 1;
		}

		// Token: 0x06004DE5 RID: 19941 RVA: 0x002ABFB0 File Offset: 0x002AA1B0
		public static ushort Difference(ushort id1, ushort id2)
		{
			int diff = (int)((id2 > id1) ? (id2 - id1) : (id1 - id2));
			return (ushort)((diff > 32767) ? (65535 - diff) : diff);
		}

		// Token: 0x06004DE6 RID: 19942 RVA: 0x002ABFE0 File Offset: 0x002AA1E0
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

		// Token: 0x06004DE7 RID: 19943 RVA: 0x002AC058 File Offset: 0x002AA258
		public static bool IsValidId(ushort currentId, ushort previousId, ushort latestPossibleId)
		{
			return !NetIdUtils.IdMoreRecent(currentId, latestPossibleId) && (NetIdUtils.IdMoreRecent(currentId, previousId) || (previousId == 0 && currentId > 32767));
		}
	}
}
