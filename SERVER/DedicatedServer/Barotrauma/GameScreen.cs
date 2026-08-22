using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;

namespace Barotrauma
{
	// Token: 0x02000278 RID: 632
	internal class GameScreen : Screen
	{
		// Token: 0x17000D5F RID: 3423
		// (get) Token: 0x06002D07 RID: 11527 RVA: 0x00129046 File Offset: 0x00127246
		public override Camera Cam
		{
			get
			{
				return Camera.Instance;
			}
		}

		// Token: 0x17000D60 RID: 3424
		// (get) Token: 0x06002D08 RID: 11528 RVA: 0x0012904D File Offset: 0x0012724D
		// (set) Token: 0x06002D09 RID: 11529 RVA: 0x00129055 File Offset: 0x00127255
		public double GameTime { get; private set; }

		// Token: 0x06002D0B RID: 11531 RVA: 0x00129071 File Offset: 0x00127271
		public override void Select()
		{
			base.Select();
			MapEntity.ClearHighlightedEntities();
		}

		// Token: 0x06002D0C RID: 11532 RVA: 0x0012907E File Offset: 0x0012727E
		public override void Deselect()
		{
			base.Deselect();
		}

		// Token: 0x06002D0D RID: 11533 RVA: 0x00129088 File Offset: 0x00127288
		public override void Update(double deltaTime)
		{
			this.GameTime += deltaTime;
			foreach (PhysicsBody body in PhysicsBody.List)
			{
				if ((body.Enabled || body.UserData is Character) && body.BodyType != BodyType.Static)
				{
					body.Update();
				}
			}
			MapEntity.ClearHighlightedEntities();
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				gameSession.Update((float)deltaTime);
			}
			if (Level.Loaded != null)
			{
				Level.Loaded.Update((float)deltaTime, Camera.Instance);
			}
			Character.UpdateAll((float)deltaTime, Camera.Instance);
			StatusEffect.UpdateAll((float)deltaTime);
			foreach (Submarine sub in Submarine.Loaded)
			{
				sub.SetPrevTransform(sub.Position);
			}
			foreach (PhysicsBody body2 in PhysicsBody.List)
			{
				if (body2.Enabled && body2.BodyType != BodyType.Static)
				{
					body2.SetPrevTransform(body2.SimPosition, body2.Rotation);
				}
			}
			MapEntity.UpdateAll((float)deltaTime, Camera.Instance);
			Character.UpdateAnimAll((float)deltaTime);
			Ragdoll.UpdateAll((float)deltaTime, Camera.Instance);
			foreach (Submarine sub2 in Submarine.Loaded)
			{
				sub2.Update((float)deltaTime);
			}
			try
			{
				GameMain.World.Step(0.016666668f);
			}
			catch (WorldLockedException e)
			{
				string errorMsg = "Attempted to modify the state of the physics simulation while a time step was running.";
				DebugConsole.ThrowError(errorMsg, e, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("GameScreen.Update:WorldLockedException" + e.Message, GameAnalyticsManager.ErrorSeverity.Critical, errorMsg);
			}
		}

		// Token: 0x06002D0E RID: 11534 RVA: 0x001292A4 File Offset: 0x001274A4
		private void ExecutePhysics()
		{
			for (;;)
			{
				if (this.physicsTime >= 0.016666666666666666)
				{
					object obj = this.updateLock;
					lock (obj)
					{
						GameMain.World.Step(0.016666668f);
						this.physicsTime -= 0.016666666666666666;
					}
				}
			}
		}

		// Token: 0x04001623 RID: 5667
		private object updateLock = new object();

		// Token: 0x04001624 RID: 5668
		private double physicsTime;
	}
}
