using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000FF RID: 255
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxInputConnection : CircuitBoxConnection
	{
		// Token: 0x170007C5 RID: 1989
		// (get) Token: 0x060019F0 RID: 6640 RVA: 0x000C89F7 File Offset: 0x000C6BF7
		public override bool IsOutput
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060019F1 RID: 6641 RVA: 0x000C89FA File Offset: 0x000C6BFA
		public CircuitBoxInputConnection(Vector2 position, Connection connection, CircuitBox box) : base(position, connection, box)
		{
		}

		// Token: 0x060019F2 RID: 6642 RVA: 0x000C8A10 File Offset: 0x000C6C10
		public override void ReceiveSignal(Signal signal)
		{
			foreach (CircuitBoxConnection connector in this.ExternallyConnectedTo)
			{
				CircuitBoxOutputConnection output = connector as CircuitBoxOutputConnection;
				if (output != null)
				{
					output.ReceiveSignal(signal);
				}
				else
				{
					Connection.SendSignalIntoConnection(signal, connector.Connection);
				}
			}
		}

		// Token: 0x04000C6F RID: 3183
		public readonly List<CircuitBoxConnection> ExternallyConnectedTo = new List<CircuitBoxConnection>();
	}
}
