using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200044F RID: 1103
	internal interface IEventItemSecondaryUse : IEvent<IEventItemSecondaryUse>, IEvent
	{
		// Token: 0x06003CB5 RID: 15541
		bool? OnItemSecondaryUsed(Item item, Character user);

		// Token: 0x06003CB6 RID: 15542 RVA: 0x0018CD06 File Offset: 0x0018AF06
		private static IEventItemSecondaryUse GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemSecondaryUse.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D5C RID: 3420
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemSecondaryUse, IEvent<IEventItemSecondaryUse>, IEvent
		{
			// Token: 0x060066FF RID: 26367 RVA: 0x0021F566 File Offset: 0x0021D766
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06006700 RID: 26368 RVA: 0x0021F570 File Offset: 0x0021D770
			public bool? OnItemSecondaryUsed(Item item, Character user)
			{
				object result = this.LuaFuncs["OnItemSecondaryUsed"](new object[]
				{
					item,
					user
				});
				DynValue dynValue = result as DynValue;
				if (dynValue != null && dynValue.Type == DataType.Boolean)
				{
					return new bool?(dynValue.Boolean);
				}
				return null;
			}
		}
	}
}
