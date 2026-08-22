using System;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020004E1 RID: 1249
	internal interface IBallastFloraState
	{
		// Token: 0x06005171 RID: 20849
		void Enter();

		// Token: 0x06005172 RID: 20850
		void Exit();

		// Token: 0x06005173 RID: 20851
		void Update(float deltaTime);

		// Token: 0x06005174 RID: 20852
		ExitState GetState();
	}
}
