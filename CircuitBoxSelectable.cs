using System;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000218 RID: 536
	internal class CircuitBoxSelectable
	{
		// Token: 0x17000E7B RID: 3707
		// (get) Token: 0x06003670 RID: 13936 RVA: 0x00212898 File Offset: 0x00210A98
		public bool IsSelectedByMe
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsServer)
				{
					throw new Exception("CircuitBoxSelectable.IsSelectedByMe should never be used by the server.");
				}
				Character controlled = Character.Controlled;
				return controlled != null && this.SelectedBy == controlled.ID;
			}
		}

		// Token: 0x06003671 RID: 13937 RVA: 0x002128DC File Offset: 0x00210ADC
		public void SetSelected(Option<ushort> selectedBy)
		{
			ushort id;
			if (selectedBy.TryUnwrap(out id))
			{
				this.SelectedBy = id;
				this.IsSelected = true;
				return;
			}
			this.IsSelected = false;
			this.SelectedBy = 0;
		}

		// Token: 0x04001BFB RID: 7163
		public bool IsSelected;

		// Token: 0x04001BFC RID: 7164
		public ushort SelectedBy;
	}
}
