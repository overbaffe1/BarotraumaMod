using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000544 RID: 1348
	internal interface IEventCharacterCreated : IEvent<IEventCharacterCreated>, IEvent
	{
		// Token: 0x06005594 RID: 21908
		void OnCharacterCreated(Character character);

		// Token: 0x06005595 RID: 21909 RVA: 0x002D13D6 File Offset: 0x002CF5D6
		private static IEventCharacterCreated GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventCharacterCreated.LuaWrapper(luaFunc);
		}

		// Token: 0x0200135D RID: 4957
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventCharacterCreated, IEvent<IEventCharacterCreated>, IEvent
		{
			// Token: 0x06009755 RID: 38741 RVA: 0x003DB322 File Offset: 0x003D9522
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009756 RID: 38742 RVA: 0x003DB32B File Offset: 0x003D952B
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
