using System;
using System.Reflection;

namespace Barotrauma
{
	// Token: 0x02000203 RID: 515
	internal static class ItemExtensions
	{
		// Token: 0x060024DE RID: 9438 RVA: 0x000F31FC File Offset: 0x000F13FC
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

		// Token: 0x060024DF RID: 9439 RVA: 0x000F3260 File Offset: 0x000F1460
		public static object CreateServerEventString(this Item item, string component)
		{
			object comp = item.GetComponentString(component);
			if (comp == null)
			{
				return null;
			}
			MethodInfo method = typeof(Item).GetMethod("CreateServerEvent", new Type[]
			{
				Type.MakeGenericMethodParameter(0)
			});
			MethodInfo generic = method.MakeGenericMethod(new Type[]
			{
				comp.GetType()
			});
			return generic.Invoke(item, new object[]
			{
				comp
			});
		}

		// Token: 0x060024E0 RID: 9440 RVA: 0x000F32C8 File Offset: 0x000F14C8
		public static object CreateServerEventString(this Item item, string component, object[] extraData)
		{
			object comp = item.GetComponentString(component);
			if (comp == null)
			{
				return null;
			}
			MethodInfo method = typeof(Item).GetMethod("CreateServerEvent", new Type[]
			{
				Type.MakeGenericMethodParameter(0),
				typeof(object[])
			});
			MethodInfo generic = method.MakeGenericMethod(new Type[]
			{
				comp.GetType()
			});
			return generic.Invoke(item, new object[]
			{
				comp,
				extraData
			});
		}
	}
}
