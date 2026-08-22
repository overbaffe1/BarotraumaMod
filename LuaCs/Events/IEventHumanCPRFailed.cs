using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000546 RID: 1350
	internal interface IEventHumanCPRFailed : IEvent<IEventHumanCPRFailed>, IEvent
	{
		// Token: 0x06005598 RID: 21912
		void OnCharacterCPRFailed(HumanoidAnimController animController);

		// Token: 0x06005599 RID: 21913 RVA: 0x002D13E6 File Offset: 0x002CF5E6
		private static IEventHumanCPRFailed GetLuaRunner(IDictionary<string, LuaCsFunc> luaFunc)
		{
			return new IEventHumanCPRFailed.LuaWrapper(luaFunc);
		}

		// Token: 0x0200135F RID: 4959
		public sealed class LuaWrapper : IEvent.LuaWrapperBase, IEventHumanCPRFailed, IEvent<IEventHumanCPRFailed>, IEvent
		{
			// Token: 0x06009759 RID: 38745 RVA: 0x003DB378 File Offset: 0x003D9578
			public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
			{
			}

			// Token: 0x0600975A RID: 38746 RVA: 0x003DB381 File Offset: 0x003D9581
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
