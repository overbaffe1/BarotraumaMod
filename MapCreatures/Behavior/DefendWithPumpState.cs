using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020004DD RID: 1245
	internal class DefendWithPumpState : IBallastFloraState
	{
		// Token: 0x06005159 RID: 20825 RVA: 0x002BC710 File Offset: 0x002BA910
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

		// Token: 0x0600515A RID: 20826 RVA: 0x002BC7C8 File Offset: 0x002BA9C8
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

		// Token: 0x0600515B RID: 20827 RVA: 0x002BC7F4 File Offset: 0x002BA9F4
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

		// Token: 0x0600515C RID: 20828 RVA: 0x002BC968 File Offset: 0x002BAB68
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

		// Token: 0x0600515D RID: 20829 RVA: 0x002BCA00 File Offset: 0x002BAC00
		private void SetPump(Pump pump)
		{
			if (pump.TargetLevel != null)
			{
				pump.TargetLevel = new float?(100f);
				return;
			}
			pump.FlowPercentage = 100f;
		}

		// Token: 0x0600515E RID: 20830 RVA: 0x002BCA2C File Offset: 0x002BAC2C
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

		// Token: 0x04002B27 RID: 11047
		private readonly BallastFloraBranch targetBranch;

		// Token: 0x04002B28 RID: 11048
		private readonly List<Pump> allAvailablePumps = new List<Pump>();

		// Token: 0x04002B29 RID: 11049
		private readonly List<Door> allAvailableDoors = new List<Door>();

		// Token: 0x04002B2A RID: 11050
		private readonly List<Pump> targetPumps = new List<Pump>();

		// Token: 0x04002B2B RID: 11051
		private readonly List<Door> jammedDoors = new List<Door>();

		// Token: 0x04002B2C RID: 11052
		private bool isFinished;

		// Token: 0x04002B2D RID: 11053
		private float timer = 10f;

		// Token: 0x04002B2E RID: 11054
		private bool filled;

		// Token: 0x04002B2F RID: 11055
		private bool tryDrown;

		// Token: 0x04002B30 RID: 11056
		private readonly Character attacker;
	}
}
