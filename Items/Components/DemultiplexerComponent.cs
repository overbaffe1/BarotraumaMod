using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000610 RID: 1552
	internal sealed class DemultiplexerComponent : ConnectionSelectorComponent
	{
		// Token: 0x06006417 RID: 25623 RVA: 0x0033F913 File Offset: 0x0033DB13
		public DemultiplexerComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x17001954 RID: 6484
		// (get) Token: 0x06006418 RID: 25624 RVA: 0x0033F91D File Offset: 0x0033DB1D
		protected override string InputNameSetConnection
		{
			get
			{
				return "set_output";
			}
		}

		// Token: 0x17001955 RID: 6485
		// (get) Token: 0x06006419 RID: 25625 RVA: 0x0033F924 File Offset: 0x0033DB24
		protected override string InputNameMoveInput
		{
			get
			{
				return "move_output";
			}
		}

		// Token: 0x0600641A RID: 25626 RVA: 0x0033F92C File Offset: 0x0033DB2C
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			bool isActive;
			if (this.item.Connections != null)
			{
				isActive = this.item.Connections.Any((Connection c) => c.Name == "selected_output_out");
			}
			else
			{
				isActive = false;
			}
			this.IsActive = isActive;
		}

		// Token: 0x0600641B RID: 25627 RVA: 0x0033F984 File Offset: 0x0033DB84
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (connection.Name == "signal_in")
			{
				this.item.SendSignal(signal, this.selectedConnectionName);
				return;
			}
			base.ReceiveSignal(signal, connection);
		}

		// Token: 0x0600641C RID: 25628 RVA: 0x0033F9B3 File Offset: 0x0033DBB3
		public override void Update(float deltaTime, Camera cam)
		{
			this.item.SendSignal(this.selectedConnectionIndexStr, "selected_output_out");
		}

		// Token: 0x0600641D RID: 25629 RVA: 0x0033F9CB File Offset: 0x0033DBCB
		protected override string GetConnectionName(int connectionIndex)
		{
			return "signal_out" + connectionIndex.ToString();
		}

		// Token: 0x0600641E RID: 25630 RVA: 0x0033F9E0 File Offset: 0x0033DBE0
		protected override IEnumerable<Connection> GetConnections()
		{
			ConnectionPanel connectionPanel = this.item.GetComponent<ConnectionPanel>();
			if (connectionPanel != null)
			{
				return from c in connectionPanel.Connections
				where c.IsOutput && c.Name.StartsWith("signal_out")
				select c;
			}
			return Enumerable.Empty<Connection>();
		}
	}
}
