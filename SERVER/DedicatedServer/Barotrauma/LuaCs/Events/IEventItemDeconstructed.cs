using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000441 RID: 1089
	internal interface IEventItemDeconstructed : IEvent<IEventItemDeconstructed>, IEvent
	{
		// Token: 0x06003C99 RID: 15513
		bool? OnItemDeconstructed(Item item, Deconstructor deconstructor, Character user, bool allowRemove);

		// Token: 0x06003C9A RID: 15514 RVA: 0x0018CC96 File Offset: 0x0018AE96
		private static IEventItemDeconstructed GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemDeconstructed.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D4E RID: 3406
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemDeconstructed, IEvent<IEventItemDeconstructed>, IEvent
		{
			// Token: 0x060066E3 RID: 26339 RVA: 0x0021F22A File Offset: 0x0021D42A
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066E4 RID: 26340 RVA: 0x0021F234 File Offset: 0x0021D434
			public bool? OnItemDeconstructed(Item item, Deconstructor deconstructor, Character user, bool allowRemove)
			{
				object result = this.LuaFuncs["OnItemDeconstructed"](new object[]
				{
					item,
					deconstructor,
					user,
					allowRemove
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
