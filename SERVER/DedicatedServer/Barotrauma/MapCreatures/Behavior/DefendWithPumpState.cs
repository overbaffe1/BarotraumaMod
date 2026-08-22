using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020003D5 RID: 981
	internal class DefendWithPumpState : IBallastFloraState
	{
		// Token: 0x060038AD RID: 14509 RVA: 0x0017AA88 File Offset: 0x00178C88
		public DefendWithPumpState(BallastFloraBranch branch, IEnumerable<Item> items, Character attacker)
		{
			this.targetBranch = branch;
			this.attacker = attacker;
			foreach (Item item in items)
			{
				Pump pump = item.GetComponent<Pump>();
				if (pump != null)
				{
					this.allAvailablePumps.Add(pump);
				}
				Door door = item.GetComponent<Door>();
				if (door != null)
				{
					this.allAvailableDoors.Add(door);
				}
			}
		}

		// Token: 0x060038AE RID: 14510 RVA: 0x0017AB40 File Offset: 0x00178D40
		public ExitState GetState()
		{
			if (this.isFinished)
			{
				return ExitState.ReturnLast;
			}
			if (this.targetBranch.CurrentHull == null)
			{
				return ExitState.ReturnLast;
			}
			if (this.timer >= 0f)
			{
				return ExitState.Running;
			}
			return ExitState.ReturnLast;
		}

		// Token: 0x060038AF RID: 14511 RVA: 0x0017AB6C File Offset: 0x00178D6C
		public void Enter()
		{
			foreach (Pump pump in this.allAvailablePumps)
			{
				if (pump.Item.CurrentHull == this.targetBranch.CurrentHull)
				{
					this.targetPumps.Add(pump);
					this.SetPump(pump);
					pump.Hijacked = true;
				}
			}
			if (this.targetPumps.Any<Pump>())
			{
				if (!this.targetPumps.All((Pump p) => !p.HasPower))
				{
					goto IL_A1;
				}
			}
			this.isFinished = true;
			IL_A1:
			if (this.targetBranch.CurrentHull != null && this.attacker != null && this.attacker.CurrentHull == this.targetBranch.CurrentHull)
			{
				foreach (Door door in this.allAvailableDoors)
				{
					if (door.LinkedGap != null && door.LinkedGap.linkedTo.Contains(this.targetBranch.CurrentHull))
					{
						door.TrySetState(false, false, true);
						door.IsJammed = true;
						this.jammedDoors.Add(door);
					}
				}
				this.tryDrown = true;
			}
		}

		// Token: 0x060038B0 RID: 14512 RVA: 0x0017ACE0 File Offset: 0x00178EE0
		public void Exit()
		{
			foreach (Pump pump in this.targetPumps)
			{
				pump.Hijacked = false;
			}
			foreach (Door door in this.jammedDoors)
			{
				door.IsJammed = false;
			}
		}

		// Token: 0x060038B1 RID: 14513 RVA: 0x0017AD78 File Offset: 0x00178F78
		private void SetPump(Pump pump)
		{
			if (pump.TargetLevel != null)
			{
				pump.TargetLevel = new float?(100f);
				return;
			}
			pump.FlowPercentage = 100f;
		}

		// Token: 0x060038B2 RID: 14514 RVA: 0x0017ADA4 File Offset: 0x00178FA4
		public void Update(float deltaTime)
		{
			foreach (Pump pump in this.targetPumps)
			{
				this.SetPump(pump);
			}
			if (this.tryDrown && !this.filled && (this.targetBranch.CurrentHull == null || this.targetBranch.CurrentHull.WaterPercentage >= 95f))
			{
				this.filled = true;
				this.timer += 10f;
			}
			this.timer -= deltaTime;
		}

		// Token: 0x04001C8E RID: 7310
		private readonly BallastFloraBranch targetBranch;

		// Token: 0x04001C8F RID: 7311
		private readonly List<Pump> allAvailablePumps = new List<Pump>();

		// Token: 0x04001C90 RID: 7312
		private readonly List<Door> allAvailableDoors = new List<Door>();

		// Token: 0x04001C91 RID: 7313
		private readonly List<Pump> targetPumps = new List<Pump>();

		// Token: 0x04001C92 RID: 7314
		private readonly List<Door> jammedDoors = new List<Door>();

		// Token: 0x04001C93 RID: 7315
		private bool isFinished;

		// Token: 0x04001C94 RID: 7316
		private float timer = 10f;

		// Token: 0x04001C95 RID: 7317
		private bool filled;

		// Token: 0x04001C96 RID: 7318
		private bool tryDrown;

		// Token: 0x04001C97 RID: 7319
		private readonly Character attacker;
	}
}
