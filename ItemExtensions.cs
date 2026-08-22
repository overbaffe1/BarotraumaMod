using System;
using System.Reflection;

namespace Barotrauma
{
	// Token: 0x020002EC RID: 748
	internal static class ItemExtensions
	{
		// Token: 0x06003DB8 RID: 15800 RVA: 0x00230768 File Offset: 0x0022E968
		public static object GetComponentString(this Item item, string component)
		{
			Type type = LuaCsSetup.Instance.PluginManagementService.GetType("Barotrauma.Items.Components." + component, false, false, true);
			if (type == null)
			{
				return null;
			}
			MethodInfo method = typeof(Item).GetMethod("GetComponent");
			MethodInfo generic = method.MakeGenericMethod(new Type[]
			{
				type
			});
			return generic.Invoke(item, null);
		}
	}
}
