using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000432 RID: 1074
	internal interface IEventCharacterCreated : IEvent<IEventCharacterCreated>, IEvent
	{
		// Token: 0x06003C7B RID: 15483
		void OnCharacterCreated(Character character);

		// Token: 0x06003C7C RID: 15484 RVA: 0x0018CC1E File Offset: 0x0018AE1E
		private static IEventCharacterCreated GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCharacterCreated.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D3F RID: 3391
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCharacterCreated, IEvent<IEventCharacterCreated>, IEvent
		{
			// Token: 0x060066C5 RID: 26309 RVA: 0x0021ED22 File Offset: 0x0021CF22
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066C6 RID: 26310 RVA: 0x0021ED2B File Offset: 0x0021CF2B
			public void OnCharacterCreated(Character character)
			{
				this.LuaFuncs["OnCharacterCreated"](new object[]
				{
					character
				});
			}
		}
	}
}
