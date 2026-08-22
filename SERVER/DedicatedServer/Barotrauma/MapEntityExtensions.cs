using System;

namespace Barotrauma
{
	// Token: 0x02000201 RID: 513
	internal static class MapEntityExtensions
	{
		// Token: 0x060024D9 RID: 9433 RVA: 0x000F3171 File Offset: 0x000F1371
		public static void AddLinked(this MapEntity entity, MapEntity other)
		{
			entity.linkedTo.Add(other);
		}
	}
}
