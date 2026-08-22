using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004F4 RID: 1268
	internal sealed class MultiplexerComponent : ConnectionSelectorComponent
	{
		// Token: 0x0600476B RID: 18283 RVA: 0x001C74E4 File Offset: 0x001C56E4
		public MultiplexerComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x1700132E RID: 4910
		// (get) Token: 0x0600476C RID: 18284 RVA: 0x001C74EE File Offset: 0x001C56EE
		protected override string InputNameSetConnection
		{
			get
			{
				return "set_input";
			}
		}

		// Token: 0x1700132F RID: 4911
		// (get) Token: 0x0600476D RID: 18285 RVA: 0x001C74F5 File Offset: 0x001C56F5
		protected override string InputNameMoveInput
		{
			get
			{
				return "move_input";
			}
		}

		// Token: 0x0600476E RID: 18286 RVA: 0x001C74FC File Offset: 0x001C56FC
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

		// Token: 0x0600476F RID: 18287 RVA: 0x001C7554 File Offset: 0x001C5754
		public override void Update(float deltaTime, Camera cam)
		{
			this.item.SendSignal(this.selectedConnectionIndexStr, "selected_input_out");
		}

		// Token: 0x06004770 RID: 18288 RVA: 0x001C756C File Offset: 0x001C576C
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

		// Token: 0x06004771 RID: 18289 RVA: 0x001C75B8 File Offset: 0x001C57B8
		protected override string GetConnectionName(int connectionIndex)
		{
			return "signal_in" + connectionIndex.ToString();
		}

		// Token: 0x06004772 RID: 18290 RVA: 0x001C75CC File Offset: 0x001C57CC
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
