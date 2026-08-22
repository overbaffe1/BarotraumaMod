using System;

namespace Barotrauma
{
	// Token: 0x02000270 RID: 624
	internal class Key
	{
		// Token: 0x06002CAD RID: 11437 RVA: 0x00126FBC File Offset: 0x001251BC
		public Key(InputType inputType)
		{
			this.inputType = inputType;
		}

		// Token: 0x17000D4D RID: 3405
		// (get) Token: 0x06002CAE RID: 11438 RVA: 0x00126FCB File Offset: 0x001251CB
		// (set) Token: 0x06002CAF RID: 11439 RVA: 0x00126FD3 File Offset: 0x001251D3
		public bool Hit
		{
			get
			{
				return this.hit;
			}
			set
			{
				this.hit = value;
			}
		}

		// Token: 0x17000D4E RID: 3406
		// (get) Token: 0x06002CB0 RID: 11440 RVA: 0x00126FDC File Offset: 0x001251DC
		// (set) Token: 0x06002CB1 RID: 11441 RVA: 0x00126FE4 File Offset: 0x001251E4
		public bool Held
		{
			get
			{
				return this.held;
			}
			set
			{
				this.held = value;
			}
		}

		// Token: 0x06002CB2 RID: 11442 RVA: 0x00126FED File Offset: 0x001251ED
		public void SetState(bool hit, bool held)
		{
			if (hit)
			{
				this.hitQueue = true;
			}
			if (held)
			{
				this.heldQueue = true;
			}
		}

		// Token: 0x06002CB3 RID: 11443 RVA: 0x00127004 File Offset: 0x00125204
		public bool DequeueHit()
		{
			bool value = this.hitQueue;
			this.hitQueue = false;
			return value;
		}

		// Token: 0x06002CB4 RID: 11444 RVA: 0x00127020 File Offset: 0x00125220
		public bool DequeueHeld()
		{
			bool value = this.heldQueue;
			this.heldQueue = false;
			return value;
		}

		// Token: 0x17000D4F RID: 3407
		// (get) Token: 0x06002CB5 RID: 11445 RVA: 0x0012703C File Offset: 0x0012523C
		public bool GetHeldQueue
		{
			get
			{
				return this.heldQueue;
			}
		}

		// Token: 0x17000D50 RID: 3408
		// (get) Token: 0x06002CB6 RID: 11446 RVA: 0x00127044 File Offset: 0x00125244
		public bool GetHitQueue
		{
			get
			{
				return this.hitQueue;
			}
		}

		// Token: 0x06002CB7 RID: 11447 RVA: 0x0012704C File Offset: 0x0012524C
		public void Reset()
		{
			this.hit = false;
			this.held = false;
		}

		// Token: 0x06002CB8 RID: 11448 RVA: 0x0012705C File Offset: 0x0012525C
		public void ResetHit()
		{
			this.hit = false;
		}

		// Token: 0x06002CB9 RID: 11449 RVA: 0x00127065 File Offset: 0x00125265
		public void ResetHeld()
		{
			this.held = false;
		}

		// Token: 0x04001609 RID: 5641
		private bool hit;

		// Token: 0x0400160A RID: 5642
		private bool hitQueue;

		// Token: 0x0400160B RID: 5643
		private bool held;

		// Token: 0x0400160C RID: 5644
		private bool heldQueue;

		// Token: 0x0400160D RID: 5645
		private InputType inputType;
	}
}
