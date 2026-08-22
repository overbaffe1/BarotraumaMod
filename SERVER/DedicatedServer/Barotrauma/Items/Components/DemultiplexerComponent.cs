using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004EC RID: 1260
	internal sealed class DemultiplexerComponent : ConnectionSelectorComponent
	{
		// Token: 0x06004726 RID: 18214 RVA: 0x001C60DF File Offset: 0x001C42DF
		public DemultiplexerComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x17001317 RID: 4887
		// (get) Token: 0x06004727 RID: 18215 RVA: 0x001C60E9 File Offset: 0x001C42E9
		protected override string InputNameSetConnection
		{
			get
			{
				return "set_output";
			}
		}

		// Token: 0x17001318 RID: 4888
		// (get) Token: 0x06004728 RID: 18216 RVA: 0x001C60F0 File Offset: 0x001C42F0
		protected override string InputNameMoveInput
		{
			get
			{
				return "move_output";
			}
		}

		// Token: 0x06004729 RID: 18217 RVA: 0x001C60F8 File Offset: 0x001C42F8
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

		// Token: 0x0600472A RID: 18218 RVA: 0x001C6150 File Offset: 0x001C4350
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (connection.Name == "signal_in")
			{
				this.item.SendSignal(signal, this.selectedConnectionName);
				return;
			}
			base.ReceiveSignal(signal, connection);
		}

		// Token: 0x0600472B RID: 18219 RVA: 0x001C617F File Offset: 0x001C437F
		public override void Update(float deltaTime, Camera cam)
		{
			this.item.SendSignal(this.selectedConnectionIndexStr, "selected_output_out");
		}

		// Token: 0x0600472C RID: 18220 RVA: 0x001C6197 File Offset: 0x001C4397
		protected override string GetConnectionName(int connectionIndex)
		{
			return "signal_out" + connectionIndex.ToString();
		}

		// Token: 0x0600472D RID: 18221 RVA: 0x001C61AC File Offset: 0x001C43AC
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
