using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000561 RID: 1377
	internal interface IEventItemSecondaryUse : IEvent<IEventItemSecondaryUse>, IEvent
	{
		// Token: 0x060055CE RID: 21966
		bool? OnItemSecondaryUsed(Item item, Character user);

		// Token: 0x060055CF RID: 21967 RVA: 0x002D14BE File Offset: 0x002CF6BE
		private static IEventItemSecondaryUse GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemSecondaryUse.LuaWrapper(luaFunc);
		}

		// Token: 0x0200137A RID: 4986
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemSecondaryUse, IEvent<IEventItemSecondaryUse>, IEvent
		{
			// Token: 0x0600978F RID: 38799 RVA: 0x003DBB66 File Offset: 0x003D9D66
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009790 RID: 38800 RVA: 0x003DBB70 File Offset: 0x003D9D70
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
