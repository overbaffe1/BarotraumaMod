using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004AA RID: 1194
	internal abstract class ConnectionSelectorComponent : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x0600433D RID: 17213 RVA: 0x001B0304 File Offset: 0x001AE504
		private IEnumerable<CoroutineStatus> SendStateAfterDelay()
		{
			ConnectionSelectorComponent.<SendStateAfterDelay>d__3 <SendStateAfterDelay>d__ = new ConnectionSelectorComponent.<SendStateAfterDelay>d__3(-2);
			<SendStateAfterDelay>d__.<>4__this = this;
			return <SendStateAfterDelay>d__;
		}

		// Token: 0x0600433E RID: 17214 RVA: 0x001B0314 File Offset: 0x001AE514
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteRangedInteger(this.selectedConnectionIndex, 0, 255);
			this.lastSentConnectionIndex = this.selectedConnectionIndex;
		}

		// Token: 0x170011E7 RID: 4583
		// (get) Token: 0x0600433F RID: 17215 RVA: 0x001B0334 File Offset: 0x001AE534
		// (set) Token: 0x06004340 RID: 17216 RVA: 0x001B033C File Offset: 0x001AE53C
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
				if (prevIndex != this.selectedConnectionIndex)
				{
					this.OnStateChanged();
				}
			}
		}

		// Token: 0x170011E8 RID: 4584
		// (get) Token: 0x06004341 RID: 17217 RVA: 0x001B03B1 File Offset: 0x001AE5B1
		// (set) Token: 0x06004342 RID: 17218 RVA: 0x001B03B9 File Offset: 0x001AE5B9
		[InGameEditable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the selected connection go back to the first one when moving past the last one?", "", true)]
		public bool WrapAround { get; set; }

		// Token: 0x170011E9 RID: 4585
		// (get) Token: 0x06004343 RID: 17219 RVA: 0x001B03C2 File Offset: 0x001AE5C2
		// (set) Token: 0x06004344 RID: 17220 RVA: 0x001B03CA File Offset: 0x001AE5CA
		[InGameEditable]
		[Serialize(true, IsPropertySaveable.Yes, "Should empty connections (connections with no wires in them) be skipped over when moving the selection?", "", true)]
		public bool SkipEmptyConnections { get; set; }

		// Token: 0x06004345 RID: 17221 RVA: 0x001B03D3 File Offset: 0x001AE5D3
		public ConnectionSelectorComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06004346 RID: 17222 RVA: 0x001B03E4 File Offset: 0x001AE5E4
		private void OnStateChanged()
		{
			this.sendStateTimer = 0.5f;
			if (this.sendStateCoroutine == null)
			{
				this.sendStateCoroutine = CoroutineManager.StartCoroutine(this.SendStateAfterDelay(), "");
			}
		}

		// Token: 0x06004347 RID: 17223
		protected abstract string GetConnectionName(int connectionIndex);

		// Token: 0x170011EA RID: 4586
		// (get) Token: 0x06004348 RID: 17224
		protected abstract string InputNameSetConnection { get; }

		// Token: 0x170011EB RID: 4587
		// (get) Token: 0x06004349 RID: 17225
		protected abstract string InputNameMoveInput { get; }

		// Token: 0x0600434A RID: 17226
		protected abstract IEnumerable<Connection> GetConnections();

		// Token: 0x0600434B RID: 17227 RVA: 0x001B040F File Offset: 0x001AE60F
		public override void OnItemLoaded()
		{
			this.connectionCount = this.GetConnections().Count<Connection>();
		}

		// Token: 0x0600434C RID: 17228 RVA: 0x001B0424 File Offset: 0x001AE624
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
						this.<ReceiveSignal>g__moveInput|29_0(moveAmount);
						if (this.item.Connections.Any((Connection c) => c.Name == this.selectedConnectionName && (c.Wires.Any<Wire>() || c.CircuitBoxConnections.Any<CircuitBoxConnection>())))
						{
							return;
						}
					}
					return;
				}
				this.<ReceiveSignal>g__moveInput|29_0(moveAmount);
			}
		}

		// Token: 0x0600434E RID: 17230 RVA: 0x001B04F4 File Offset: 0x001AE6F4
		[CompilerGenerated]
		private void <ReceiveSignal>g__moveInput|29_0(int moveAmount)
		{
			if (this.WrapAround)
			{
				this.SelectedConnection = MathUtils.PositiveModulo(this.selectedConnectionIndex + moveAmount, this.connectionCount);
				return;
			}
			this.SelectedConnection += moveAmount;
		}

		// Token: 0x0400202D RID: 8237
		private CoroutineHandle sendStateCoroutine;

		// Token: 0x0400202E RID: 8238
		private int lastSentConnectionIndex;

		// Token: 0x0400202F RID: 8239
		private float sendStateTimer;

		// Token: 0x04002030 RID: 8240
		protected int selectedConnectionIndex;

		// Token: 0x04002031 RID: 8241
		protected string selectedConnectionIndexStr;

		// Token: 0x04002032 RID: 8242
		protected string selectedConnectionName;

		// Token: 0x04002033 RID: 8243
		private int connectionCount = -1;
	}
}
