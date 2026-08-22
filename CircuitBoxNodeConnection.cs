using System;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001FC RID: 508
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxNodeConnection : CircuitBoxConnection
	{
		// Token: 0x17000E2E RID: 3630
		// (get) Token: 0x060034F2 RID: 13554 RVA: 0x0020F1EF File Offset: 0x0020D3EF
		public override bool IsOutput
		{
			get
			{
				return this.Connection.IsOutput;
			}
		}

		// Token: 0x17000E2F RID: 3631
		// (get) Token: 0x060034F3 RID: 13555 RVA: 0x0020F1FC File Offset: 0x0020D3FC
		public bool HasAvailableSlots
		{
			get
			{
				return this.Connection.WireSlotsAvailable();
			}
		}

		// Token: 0x060034F4 RID: 13556 RVA: 0x0020F209 File Offset: 0x0020D409
		public CircuitBoxNodeConnection(Vector2 position, CircuitBoxComponent component, Connection connection, CircuitBox circuitBox) : base(position, connection, circuitBox)
		{
			this.Component = component;
		}

		// Token: 0x060034F5 RID: 13557 RVA: 0x0020F21C File Offset: 0x0020D41C
		public override void ReceiveSignal(Signal signal)
		{
			Connection.SendSignalIntoConnection(signal, this.Connection);
		}

		// Token: 0x04001B94 RID: 7060
		public CircuitBoxComponent Component;
	}
}
