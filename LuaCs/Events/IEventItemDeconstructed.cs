using System;
using System.Collections.Generic;
using Barotrauma.Items.Components;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000553 RID: 1363
	internal interface IEventItemDeconstructed : IEvent<IEventItemDeconstructed>, IEvent
	{
		// Token: 0x060055B2 RID: 21938
		bool? OnItemDeconstructed(Item item, Deconstructor deconstructor, Character user, bool allowRemove);

		// Token: 0x060055B3 RID: 21939 RVA: 0x002D144E File Offset: 0x002CF64E
		private static IEventItemDeconstructed GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemDeconstructed.LuaWrapper(luaFunc);
		}

		// Token: 0x0200136C RID: 4972
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemDeconstructed, IEvent<IEventItemDeconstructed>, IEvent
		{
			// Token: 0x06009773 RID: 38771 RVA: 0x003DB82A File Offset: 0x003D9A2A
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009774 RID: 38772 RVA: 0x003DB834 File Offset: 0x003D9A34
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
