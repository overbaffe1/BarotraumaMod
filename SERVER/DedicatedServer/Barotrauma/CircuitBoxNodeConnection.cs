using System;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000101 RID: 257
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxNodeConnection : CircuitBoxConnection
	{
		// Token: 0x170007C7 RID: 1991
		// (get) Token: 0x060019F6 RID: 6646 RVA: 0x000C8AA3 File Offset: 0x000C6CA3
		public override bool IsOutput
		{
			get
			{
				return this.Connection.IsOutput;
			}
		}

		// Token: 0x170007C8 RID: 1992
		// (get) Token: 0x060019F7 RID: 6647 RVA: 0x000C8AB0 File Offset: 0x000C6CB0
		public bool HasAvailableSlots
		{
			get
			{
				return this.Connection.WireSlotsAvailable();
			}
		}

		// Token: 0x060019F8 RID: 6648 RVA: 0x000C8ABD File Offset: 0x000C6CBD
		public CircuitBoxNodeConnection(Vector2 position, CircuitBoxComponent component, Connection connection, CircuitBox circuitBox) : base(position, connection, circuitBox)
		{
			this.Component = component;
		}

		// Token: 0x060019F9 RID: 6649 RVA: 0x000C8AD0 File Offset: 0x000C6CD0
		public override void ReceiveSignal(Signal signal)
		{
			Connection.SendSignalIntoConnection(signal, this.Connection);
		}

		// Token: 0x04000C70 RID: 3184
		public CircuitBoxComponent Component;
	}
}
