using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200054B RID: 1355
	internal interface IEventTryClientChangeName : IEvent<IEventTryClientChangeName>, IEvent
	{
		// Token: 0x060055A2 RID: 21922
		bool? OnTryClienChangeName(Client client, string newName, Identifier newJob, CharacterTeamType newTeam);

		// Token: 0x060055A3 RID: 21923 RVA: 0x002D140E File Offset: 0x002CF60E
		private static IEventTryClientChangeName GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventTryClientChangeName.LuaWrapper(luaFunc);
		}

		// Token: 0x02001364 RID: 4964
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventTryClientChangeName, IEvent<IEventTryClientChangeName>, IEvent
		{
			// Token: 0x06009763 RID: 38755 RVA: 0x003DB4A7 File Offset: 0x003D96A7
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009764 RID: 38756 RVA: 0x003DB4B0 File Offset: 0x003D96B0
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
