using System;

namespace Barotrauma
{
	// Token: 0x0200027A RID: 634
	internal abstract class Screen
	{
		// Token: 0x17000D66 RID: 3430
		// (get) Token: 0x06002D22 RID: 11554 RVA: 0x001295F9 File Offset: 0x001277F9
		// (set) Token: 0x06002D23 RID: 11555 RVA: 0x00129600 File Offset: 0x00127800
		public static Screen Selected { get; private set; }

		// Token: 0x06002D24 RID: 11556 RVA: 0x00129608 File Offset: 0x00127808
		public static void SelectNull()
		{
			Screen.Selected = null;
		}

		// Token: 0x06002D25 RID: 11557 RVA: 0x00129610 File Offset: 0x00127810
		public virtual void Deselect()
		{
		}

		// Token: 0x06002D26 RID: 11558 RVA: 0x00129612 File Offset: 0x00127812
		public virtual void Select()
		{
			if (Screen.Selected != null && Screen.Selected != this)
			{
				Screen.Selected.Deselect();
			}
			Screen.Selected = this;
		}

		// Token: 0x17000D67 RID: 3431
		// (get) Token: 0x06002D27 RID: 11559 RVA: 0x00129633 File Offset: 0x00127833
		public virtual Camera Cam
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000D68 RID: 3432
		// (get) Token: 0x06002D28 RID: 11560 RVA: 0x00129636 File Offset: 0x00127836
		public virtual bool IsEditor
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002D29 RID: 11561 RVA: 0x00129639 File Offset: 0x00127839
		public virtual void Update(double deltaTime)
		{
		}
	}
}
