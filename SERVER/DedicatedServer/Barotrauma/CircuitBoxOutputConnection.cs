using System;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000100 RID: 256
	internal class CircuitBoxOutputConnection : CircuitBoxConnection
	{
		// Token: 0x170007C6 RID: 1990
		// (get) Token: 0x060019F3 RID: 6643 RVA: 0x000C8A7C File Offset: 0x000C6C7C
		public override bool IsOutput
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060019F4 RID: 6644 RVA: 0x000C8A7F File Offset: 0x000C6C7F
		[NullableContext(1)]
		public CircuitBoxOutputConnection(Vector2 position, Connection connection, CircuitBox circuitBox) : base(position, connection, circuitBox)
		{
		}

		// Token: 0x060019F5 RID: 6645 RVA: 0x000C8A8A File Offset: 0x000C6C8A
		public override void ReceiveSignal(Signal signal)
		{
			this.CircuitBox.Item.SendSignal(signal, this.Connection);
		}
	}
}
