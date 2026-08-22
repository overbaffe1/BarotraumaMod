using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005DA RID: 1498
	internal abstract class ConnectionSelectorComponent : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x060060D0 RID: 24784 RVA: 0x00326AEA File Offset: 0x00324CEA
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.SelectedConnection = msg.ReadRangedInteger(0, 255);
		}

		// Token: 0x1700186B RID: 6251
		// (get) Token: 0x060060D1 RID: 24785 RVA: 0x00326AFE File Offset: 0x00324CFE
		// (set) Token: 0x060060D2 RID: 24786 RVA: 0x00326B08 File Offset: 0x00324D08
		[InGameEditable]
		[Serialize(0, IsPropertySaveable.Yes, "The index of the selected connection.", "", true)]
		public int SelectedConnection
		{
			get
			{
				return this.selectedConnectionIndex;
			}
			set
			{
				int prevIndex = this.selectedConnectionIndex;
				this.selectedConnectionIndex = Math.Max(0, value);
				if (this.connectionCount > -1)
				{
					this.selectedConnectionIndex = Math.Min(this.selectedConnectionIndex, this.connectionCount - 1);
				}
				this.selectedConnectionName = this.GetConnectionName(this.selectedConnectionIndex);
				this.selectedConnectionIndexStr = this.selectedConnectionIndex.ToString();
				int num = this.selectedConnectionIndex;
			}
		}

		// Token: 0x1700186C RID: 6252
		// (get) Token: 0x060060D3 RID: 24787 RVA: 0x00326B77 File Offset: 0x00324D77
		// (set) Token: 0x060060D4 RID: 24788 RVA: 0x00326B7F File Offset: 0x00324D7F
		[InGameEditable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the selected connection go back to the first one when moving past the last one?", "", true)]
		public bool WrapAround { get; set; }

		// Token: 0x1700186D RID: 6253
		// (get) Token: 0x060060D5 RID: 24789 RVA: 0x00326B88 File Offset: 0x00324D88
		// (set) Token: 0x060060D6 RID: 24790 RVA: 0x00326B90 File Offset: 0x00324D90
		[InGameEditable]
		[Serialize(true, IsPropertySaveable.Yes, "Should empty connections (connections with no wires in them) be skipped over when moving the selection?", "", true)]
		public bool SkipEmptyConnections { get; set; }

		// Token: 0x060060D7 RID: 24791 RVA: 0x00326B99 File Offset: 0x00324D99
		public ConnectionSelectorComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060060D8 RID: 24792
		protected abstract string GetConnectionName(int connectionIndex);

		// Token: 0x1700186E RID: 6254
		// (get) Token: 0x060060D9 RID: 24793
		protected abstract string InputNameSetConnection { get; }

		// Token: 0x1700186F RID: 6255
		// (get) Token: 0x060060DA RID: 24794
		protected abstract string InputNameMoveInput { get; }

		// Token: 0x060060DB RID: 24795
		protected abstract IEnumerable<Connection> GetConnections();

		// Token: 0x060060DC RID: 24796 RVA: 0x00326BAA File Offset: 0x00324DAA
		public override void OnItemLoaded()
		{
			this.connectionCount = this.GetConnections().Count<Connection>();
		}

		// Token: 0x060060DD RID: 24797 RVA: 0x00326BC0 File Offset: 0x00324DC0
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			int moveAmount;
			if (connection.Name == this.InputNameSetConnection)
			{
				int newInput;
				if (int.TryParse(signal.value, out newInput))
				{
					this.SelectedConnection = newInput;
					return;
				}
			}
			else if (connection.Name == this.InputNameMoveInput && int.TryParse(signal.value, out moveAmount))
			{
				if (this.SkipEmptyConnections)
				{
					for (int i = 0; i < this.connectionCount; i++)
					{
						this.<ReceiveSignal>g__moveInput|25_0(moveAmount);
						if (this.item.Connections.Any((Connection c) => c.Name == this.selectedConnectionName && (c.Wires.Any<Wire>() || c.CircuitBoxConnections.Any<CircuitBoxConnection>())))
						{
							return;
						}
					}
					return;
				}
				this.<ReceiveSignal>g__moveInput|25_0(moveAmount);
			}
		}

		// Token: 0x060060DF RID: 24799 RVA: 0x00326C90 File Offset: 0x00324E90
		[CompilerGenerated]
		private void <ReceiveSignal>g__moveInput|25_0(int moveAmount)
		{
			if (this.WrapAround)
			{
				this.SelectedConnection = MathUtils.PositiveModulo(this.selectedConnectionIndex + moveAmount, this.connectionCount);
				return;
			}
			this.SelectedConnection += moveAmount;
		}

		// Token: 0x040031FE RID: 12798
		protected int selectedConnectionIndex;

		// Token: 0x040031FF RID: 12799
		protected string selectedConnectionIndexStr;

		// Token: 0x04003200 RID: 12800
		protected string selectedConnectionName;

		// Token: 0x04003201 RID: 12801
		private int connectionCount = -1;
	}
}
