using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000617 RID: 1559
	internal sealed class MultiplexerComponent : ConnectionSelectorComponent
	{
		// Token: 0x0600643B RID: 25659 RVA: 0x003401AE File Offset: 0x0033E3AE
		public MultiplexerComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x1700195D RID: 6493
		// (get) Token: 0x0600643C RID: 25660 RVA: 0x003401B8 File Offset: 0x0033E3B8
		protected override string InputNameSetConnection
		{
			get
			{
				return "set_input";
			}
		}

		// Token: 0x1700195E RID: 6494
		// (get) Token: 0x0600643D RID: 25661 RVA: 0x003401BF File Offset: 0x0033E3BF
		protected override string InputNameMoveInput
		{
			get
			{
				return "move_input";
			}
		}

		// Token: 0x0600643E RID: 25662 RVA: 0x003401C8 File Offset: 0x0033E3C8
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			bool isActive;
			if (this.item.Connections != null)
			{
				isActive = this.item.Connections.Any((Connection c) => c.Name == "selected_input_out");
			}
			else
			{
				isActive = false;
			}
			this.IsActive = isActive;
		}

		// Token: 0x0600643F RID: 25663 RVA: 0x00340220 File Offset: 0x0033E420
		public override void Update(float deltaTime, Camera cam)
		{
			this.item.SendSignal(this.selectedConnectionIndexStr, "selected_input_out");
		}

		// Token: 0x06006440 RID: 25664 RVA: 0x00340238 File Offset: 0x0033E438
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (connection.Name.StartsWith("signal_in"))
			{
				if (connection.Name == this.selectedConnectionName)
				{
					this.item.SendSignal(signal, "signal_out");
					return;
				}
			}
			else
			{
				base.ReceiveSignal(signal, connection);
			}
		}

		// Token: 0x06006441 RID: 25665 RVA: 0x00340284 File Offset: 0x0033E484
		protected override string GetConnectionName(int connectionIndex)
		{
			return "signal_in" + connectionIndex.ToString();
		}

		// Token: 0x06006442 RID: 25666 RVA: 0x00340298 File Offset: 0x0033E498
		protected override IEnumerable<Connection> GetConnections()
		{
			ConnectionPanel connectionPanel = this.item.GetComponent<ConnectionPanel>();
			if (connectionPanel != null)
			{
				return from c in connectionPanel.Connections
				where !c.IsOutput && c.Name.StartsWith("signal_in")
				select c;
			}
			return Enumerable.Empty<Connection>();
		}
	}
}
