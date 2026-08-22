using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000550 RID: 1360
	internal interface IEventItemReadPropertyChange : IEvent<IEventItemReadPropertyChange>, IEvent
	{
		// Token: 0x060055AC RID: 21932
		bool? OnItemReadPropertyChange(Item item, SerializableProperty property, object parentObject, bool allowEditing, Client sender);

		// Token: 0x060055AD RID: 21933 RVA: 0x002D1436 File Offset: 0x002CF636
		private static IEventItemReadPropertyChange GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemReadPropertyChange.LuaWrapper(luaFunc);
		}

		// Token: 0x02001369 RID: 4969
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemReadPropertyChange, IEvent<IEventItemReadPropertyChange>, IEvent
		{
			// Token: 0x0600976D RID: 38765 RVA: 0x003DB6EB File Offset: 0x003D98EB
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600976E RID: 38766 RVA: 0x003DB6F4 File Offset: 0x003D98F4
			public bool? OnItemReadPropertyChange(Item item, SerializableProperty property, object parentObject, bool allowEditing, Client sender)
			{
				object result = this.LuaFuncs["OnItemReadPropertyChange"](new object[]
				{
					item,
					property,
					parentObject,
					allowEditing,
					sender
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
