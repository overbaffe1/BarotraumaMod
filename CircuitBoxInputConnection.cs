using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001FA RID: 506
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxInputConnection : CircuitBoxConnection
	{
		// Token: 0x17000E2C RID: 3628
		// (get) Token: 0x060034EC RID: 13548 RVA: 0x0020F140 File Offset: 0x0020D340
		public override bool IsOutput
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060034ED RID: 13549 RVA: 0x0020F143 File Offset: 0x0020D343
		public CircuitBoxInputConnection(Vector2 position, Connection connection, CircuitBox box) : base(position, connection, box)
		{
		}

		// Token: 0x060034EE RID: 13550 RVA: 0x0020F15C File Offset: 0x0020D35C
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

		// Token: 0x04001B93 RID: 7059
		public readonly List<CircuitBoxConnection> ExternallyConnectedTo = new List<CircuitBoxConnection>();
	}
}
