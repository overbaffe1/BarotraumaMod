using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200043E RID: 1086
	internal interface IEventItemReadPropertyChange : IEvent<IEventItemReadPropertyChange>, IEvent
	{
		// Token: 0x06003C93 RID: 15507
		bool? OnItemReadPropertyChange(Item item, SerializableProperty property, object parentObject, bool allowEditing, Client sender);

		// Token: 0x06003C94 RID: 15508 RVA: 0x0018CC7E File Offset: 0x0018AE7E
		private static IEventItemReadPropertyChange GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventItemReadPropertyChange.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D4B RID: 3403
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventItemReadPropertyChange, IEvent<IEventItemReadPropertyChange>, IEvent
		{
			// Token: 0x060066DD RID: 26333 RVA: 0x0021F0EB File Offset: 0x0021D2EB
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066DE RID: 26334 RVA: 0x0021F0F4 File Offset: 0x0021D2F4
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
