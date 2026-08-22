using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000439 RID: 1081
	internal interface IEventTryClientChangeName : IEvent<IEventTryClientChangeName>, IEvent
	{
		// Token: 0x06003C89 RID: 15497
		bool? OnTryClienChangeName(Client client, string newName, Identifier newJob, CharacterTeamType newTeam);

		// Token: 0x06003C8A RID: 15498 RVA: 0x0018CC56 File Offset: 0x0018AE56
		private static IEventTryClientChangeName GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventTryClientChangeName.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D46 RID: 3398
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventTryClientChangeName, IEvent<IEventTryClientChangeName>, IEvent
		{
			// Token: 0x060066D3 RID: 26323 RVA: 0x0021EEA7 File Offset: 0x0021D0A7
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066D4 RID: 26324 RVA: 0x0021EEB0 File Offset: 0x0021D0B0
			public bool? OnTryClienChangeName(Client client, string newName, Identifier newJob, CharacterTeamType newTeam)
			{
				object result = this.LuaFuncs["OnTryClienChangeName"](new object[]
				{
					client,
					newName,
					newJob,
					newTeam
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
