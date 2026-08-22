using System;

namespace Barotrauma
{
	// Token: 0x020002EA RID: 746
	internal static class MapEntityExtensions
	{
		// Token: 0x06003DB7 RID: 15799 RVA: 0x00230759 File Offset: 0x0022E959
		public static void AddLinked(this MapEntity entity, MapEntity other)
		{
			entity.linkedTo.Add(other);
		}
	}
}
