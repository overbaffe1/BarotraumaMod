using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000434 RID: 1076
	internal interface IEventHumanCPRFailed : IEvent<IEventHumanCPRFailed>, IEvent
	{
		// Token: 0x06003C7F RID: 15487
		void OnCharacterCPRFailed(HumanoidAnimController animController);

		// Token: 0x06003C80 RID: 15488 RVA: 0x0018CC2E File Offset: 0x0018AE2E
		private static IEventHumanCPRFailed GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventHumanCPRFailed.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D41 RID: 3393
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventHumanCPRFailed, IEvent<IEventHumanCPRFailed>, IEvent
		{
			// Token: 0x060066C9 RID: 26313 RVA: 0x0021ED78 File Offset: 0x0021CF78
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066CA RID: 26314 RVA: 0x0021ED81 File Offset: 0x0021CF81
			public void OnCharacterCPRFailed(HumanoidAnimController animController)
			{
				this.LuaFuncs["OnCharacterCPRFailed"](new object[]
				{
					animController
				});
			}
		}
	}
}
