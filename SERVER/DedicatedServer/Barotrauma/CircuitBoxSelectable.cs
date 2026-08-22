using System;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000120 RID: 288
	internal class CircuitBoxSelectable
	{
		// Token: 0x17000819 RID: 2073
		// (get) Token: 0x06001B8A RID: 7050 RVA: 0x000CCB74 File Offset: 0x000CAD74
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

		// Token: 0x06001B8B RID: 7051 RVA: 0x000CCBB8 File Offset: 0x000CADB8
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

		// Token: 0x04000CE6 RID: 3302
		public bool IsSelected;

		// Token: 0x04000CE7 RID: 3303
		public ushort SelectedBy;
	}
}
