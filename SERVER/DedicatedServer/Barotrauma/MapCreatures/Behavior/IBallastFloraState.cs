using System;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020003D9 RID: 985
	internal interface IBallastFloraState
	{
		// Token: 0x060038C5 RID: 14533
		void Enter();

		// Token: 0x060038C6 RID: 14534
		void Exit();

		// Token: 0x060038C7 RID: 14535
		void Update(float deltaTime);

		// Token: 0x060038C8 RID: 14536
		ExitState GetState();
	}
}
