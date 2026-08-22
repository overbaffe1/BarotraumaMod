using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000433 RID: 1075
	internal interface IEventHumanCPRSuccess : IEvent<IEventHumanCPRSuccess>, IEvent
	{
		// Token: 0x06003C7D RID: 15485
		void OnCharacterCPRSuccess(HumanoidAnimController animController);

		// Token: 0x06003C7E RID: 15486 RVA: 0x0018CC26 File Offset: 0x0018AE26
		private static IEventHumanCPRSuccess GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventHumanCPRSuccess.LuaWrapper(luaFunc);
		}

		// Token: 0x02000D40 RID: 3392
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventHumanCPRSuccess, IEvent<IEventHumanCPRSuccess>, IEvent
		{
			// Token: 0x060066C7 RID: 26311 RVA: 0x0021ED4D File Offset: 0x0021CF4D
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x060066C8 RID: 26312 RVA: 0x0021ED56 File Offset: 0x0021CF56
			public void OnCharacterCPRSuccess(HumanoidAnimController animController)
			{
				this.LuaFuncs["OnCharacterCPRSuccess"](new object[]
				{
					animController
				});
			}
		}
	}
}
