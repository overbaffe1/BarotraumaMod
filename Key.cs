using System;

namespace Barotrauma
{
	// Token: 0x02000343 RID: 835
	internal class Key
	{
		// Token: 0x060041CC RID: 16844 RVA: 0x00247320 File Offset: 0x00245520
		public Key(InputType inputType)
		{
			this.inputType = inputType;
		}

		// Token: 0x17001172 RID: 4466
		// (get) Token: 0x060041CD RID: 16845 RVA: 0x0024732F File Offset: 0x0024552F
		private KeyOrMouse binding
		{
			get
			{
				return GameSettings.CurrentConfig.KeyMap.Bindings[this.inputType];
			}
		}

		// Token: 0x060041CE RID: 16846 RVA: 0x0024734B File Offset: 0x0024554B
		private static bool AllowOnGUI(InputType input)
		{
			return (input != InputType.Attack && input != InputType.Shoot) || GUI.MouseOn == null;
		}

		// Token: 0x17001173 RID: 4467
		// (get) Token: 0x060041CF RID: 16847 RVA: 0x00247360 File Offset: 0x00245560
		public KeyOrMouse State
		{
			get
			{
				return this.binding;
			}
		}

		// Token: 0x060041D0 RID: 16848 RVA: 0x00247368 File Offset: 0x00245568
		public void SetState()
		{
			this.hit = (this.binding.IsHit() && Key.AllowOnGUI(this.inputType));
			if (this.hit)
			{
				this.hitQueue = true;
			}
			this.held = (this.binding.IsDown() && Key.AllowOnGUI(this.inputType));
			if (this.held)
			{
				this.heldQueue = true;
			}
		}

		// Token: 0x17001174 RID: 4468
		// (get) Token: 0x060041D1 RID: 16849 RVA: 0x002473D5 File Offset: 0x002455D5
		// (set) Token: 0x060041D2 RID: 16850 RVA: 0x002473DD File Offset: 0x002455DD
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

		// Token: 0x17001175 RID: 4469
		// (get) Token: 0x060041D3 RID: 16851 RVA: 0x002473E6 File Offset: 0x002455E6
		// (set) Token: 0x060041D4 RID: 16852 RVA: 0x002473EE File Offset: 0x002455EE
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

		// Token: 0x060041D5 RID: 16853 RVA: 0x002473F7 File Offset: 0x002455F7
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

		// Token: 0x060041D6 RID: 16854 RVA: 0x00247410 File Offset: 0x00245610
		public bool DequeueHit()
		{
			bool value = this.hitQueue;
			this.hitQueue = false;
			return value;
		}

		// Token: 0x060041D7 RID: 16855 RVA: 0x0024742C File Offset: 0x0024562C
		public bool DequeueHeld()
		{
			bool value = this.heldQueue;
			this.heldQueue = false;
			return value;
		}

		// Token: 0x17001176 RID: 4470
		// (get) Token: 0x060041D8 RID: 16856 RVA: 0x00247448 File Offset: 0x00245648
		public bool GetHeldQueue
		{
			get
			{
				return this.heldQueue;
			}
		}

		// Token: 0x17001177 RID: 4471
		// (get) Token: 0x060041D9 RID: 16857 RVA: 0x00247450 File Offset: 0x00245650
		public bool GetHitQueue
		{
			get
			{
				return this.hitQueue;
			}
		}

		// Token: 0x060041DA RID: 16858 RVA: 0x00247458 File Offset: 0x00245658
		public void Reset()
		{
			this.hit = false;
			this.held = false;
		}

		// Token: 0x060041DB RID: 16859 RVA: 0x00247468 File Offset: 0x00245668
		public void ResetHit()
		{
			this.hit = false;
		}

		// Token: 0x060041DC RID: 16860 RVA: 0x00247471 File Offset: 0x00245671
		public void ResetHeld()
		{
			this.held = false;
		}

		// Token: 0x04002251 RID: 8785
		private bool hit;

		// Token: 0x04002252 RID: 8786
		private bool hitQueue;

		// Token: 0x04002253 RID: 8787
		private bool held;

		// Token: 0x04002254 RID: 8788
		private bool heldQueue;

		// Token: 0x04002255 RID: 8789
		private InputType inputType;
	}
}
