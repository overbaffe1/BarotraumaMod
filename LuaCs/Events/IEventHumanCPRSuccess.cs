using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000545 RID: 1349
	internal interface IEventHumanCPRSuccess : IEvent<IEventHumanCPRSuccess>, IEvent
	{
		// Token: 0x06005596 RID: 21910
		void OnCharacterCPRSuccess(HumanoidAnimController animController);

		// Token: 0x06005597 RID: 21911 RVA: 0x002D13DE File Offset: 0x002CF5DE
		private static IEventHumanCPRSuccess GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventHumanCPRSuccess.LuaWrapper(luaFunc);
		}

		// Token: 0x0200135E RID: 4958
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventHumanCPRSuccess, IEvent<IEventHumanCPRSuccess>, IEvent
		{
			// Token: 0x06009757 RID: 38743 RVA: 0x003DB34D File Offset: 0x003D954D
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x06009758 RID: 38744 RVA: 0x003DB356 File Offset: 0x003D9556
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
