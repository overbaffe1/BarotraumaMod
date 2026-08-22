using System;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001FB RID: 507
	internal class CircuitBoxOutputConnection : CircuitBoxConnection
	{
		// Token: 0x17000E2D RID: 3629
		// (get) Token: 0x060034EF RID: 13551 RVA: 0x0020F1C8 File Offset: 0x0020D3C8
		public override bool IsOutput
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060034F0 RID: 13552 RVA: 0x0020F1CB File Offset: 0x0020D3CB
		[NullableContext(1)]
		public CircuitBoxOutputConnection(Vector2 position, Connection connection, CircuitBox circuitBox) : base(position, connection, circuitBox)
		{
		}

		// Token: 0x060034F1 RID: 13553 RVA: 0x0020F1D6 File Offset: 0x0020D3D6
		public override void ReceiveSignal(Signal signal)
		{
			this.CircuitBox.Item.SendSignal(signal, this.Connection);
		}
	}
}
