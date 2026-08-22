using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000109 RID: 265
	[NetworkSerialize(50)]
	internal readonly struct CircuitBoxConnectorIdentifier : INetSerializableStruct, IEquatable<CircuitBoxConnectorIdentifier>
	{
		// Token: 0x06001A34 RID: 6708 RVA: 0x000C97AB File Offset: 0x000C79AB
		public CircuitBoxConnectorIdentifier(Identifier SignalConnection, Option<ushort> TargetId)
		{
			this.SignalConnection = SignalConnection;
			this.TargetId = TargetId;
		}

		// Token: 0x170007D5 RID: 2005
		// (get) Token: 0x06001A35 RID: 6709 RVA: 0x000C97BB File Offset: 0x000C79BB
		// (set) Token: 0x06001A36 RID: 6710 RVA: 0x000C97C3 File Offset: 0x000C79C3
		public Identifier SignalConnection { get; set; }

		// Token: 0x170007D6 RID: 2006
		// (get) Token: 0x06001A37 RID: 6711 RVA: 0x000C97CC File Offset: 0x000C79CC
		// (set) Token: 0x06001A38 RID: 6712 RVA: 0x000C97D4 File Offset: 0x000C79D4
		public Option<ushort> TargetId { get; set; }

		// Token: 0x06001A39 RID: 6713 RVA: 0x000C97E0 File Offset: 0x000C79E0
		[NullableContext(1)]
		public static CircuitBoxConnectorIdentifier FromConnection(CircuitBoxConnection connection)
		{
			CircuitBoxConnectorIdentifier result;
			if (!(connection is CircuitBoxInputConnection) && !(connection is CircuitBoxOutputConnection))
			{
				CircuitBoxNodeConnection nodeConnection = connection as CircuitBoxNodeConnection;
				if (nodeConnection == null)
				{
					throw new ArgumentOutOfRangeException("connection");
				}
				result = new CircuitBoxConnectorIdentifier(connection.Name.ToIdentifier(), Option.Some<ushort>(nodeConnection.Component.ID));
			}
			else
			{
				Identifier signalConnection = connection.Name.ToIdentifier();
				Option.UnspecifiedNone none = Option.None;
				result = new CircuitBoxConnectorIdentifier(signalConnection, none);
			}
			return result;
		}

		// Token: 0x06001A3A RID: 6714 RVA: 0x000C9858 File Offset: 0x000C7A58
		[NullableContext(1)]
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<CircuitBoxConnection> FindConnection(CircuitBox circuitBox)
		{
			ushort id;
			if (!this.TargetId.TryUnwrap(out id))
			{
				return circuitBox.FindInputOutputConnection(this.SignalConnection);
			}
			foreach (CircuitBoxComponent boxNode in circuitBox.Components)
			{
				if (boxNode.ID == id)
				{
					foreach (CircuitBoxConnection conn in boxNode.Connectors)
					{
						string name = conn.Name;
						Identifier signalConnection = this.SignalConnection;
						if (!(name != signalConnection))
						{
							return Option.Some<CircuitBoxConnection>(conn);
						}
					}
				}
			}
			Option.UnspecifiedNone none = Option.None;
			return none;
		}

		// Token: 0x06001A3B RID: 6715 RVA: 0x000C9924 File Offset: 0x000C7B24
		[NullableContext(1)]
		public XElement Save(string name)
		{
			ushort value;
			return new XElement(name, new object[]
			{
				new XAttribute("name", this.SignalConnection),
				new XAttribute("target", this.TargetId.TryUnwrap(out value) ? value.ToString() : string.Empty)
			});
		}

		// Token: 0x06001A3C RID: 6716 RVA: 0x000C9994 File Offset: 0x000C7B94
		[NullableContext(1)]
		public static CircuitBoxConnectorIdentifier Load(ContentXElement element)
		{
			string name = element.GetAttributeString("name", string.Empty);
			string target = element.GetAttributeString("target", string.Empty);
			Option.UnspecifiedNone none = Option.None;
			Option<ushort> targetId = none;
			if (!string.IsNullOrWhiteSpace(target))
			{
				ushort value;
				Option<ushort> option;
				if (!ushort.TryParse(target, out value))
				{
					none = Option.None;
					option = none;
				}
				else
				{
					option = Option.Some<ushort>(value);
				}
				targetId = option;
			}
			return new CircuitBoxConnectorIdentifier(name.ToIdentifier(), targetId);
		}

		// Token: 0x06001A3D RID: 6717 RVA: 0x000C9A08 File Offset: 0x000C7C08
		[NullableContext(1)]
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 2);
			defaultInterpolatedStringHandler.AppendLiteral("{Name: ");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.SignalConnection);
			defaultInterpolatedStringHandler.AppendLiteral(", ID: ");
			ushort value;
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetId.TryUnwrap(out value) ? value.ToString() : "N/A");
			defaultInterpolatedStringHandler.AppendLiteral("}");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06001A3E RID: 6718 RVA: 0x000C9A80 File Offset: 0x000C7C80
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("SignalConnection = ");
			builder.Append(this.SignalConnection.ToString());
			builder.Append(", TargetId = ");
			builder.Append(this.TargetId.ToString());
			return true;
		}

		// Token: 0x06001A3F RID: 6719 RVA: 0x000C9ADC File Offset: 0x000C7CDC
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxConnectorIdentifier left, CircuitBoxConnectorIdentifier right)
		{
			return !(left == right);
		}

		// Token: 0x06001A40 RID: 6720 RVA: 0x000C9AE8 File Offset: 0x000C7CE8
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxConnectorIdentifier left, CircuitBoxConnectorIdentifier right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001A41 RID: 6721 RVA: 0x000C9AF2 File Offset: 0x000C7CF2
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Identifier>.Default.GetHashCode(this.<SignalConnection>k__BackingField) * -1521134295 + EqualityComparer<Option<ushort>>.Default.GetHashCode(this.<TargetId>k__BackingField);
		}

		// Token: 0x06001A42 RID: 6722 RVA: 0x000C9B1B File Offset: 0x000C7D1B
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxConnectorIdentifier && this.Equals((CircuitBoxConnectorIdentifier)obj);
		}

		// Token: 0x06001A43 RID: 6723 RVA: 0x000C9B33 File Offset: 0x000C7D33
		[CompilerGenerated]
		public bool Equals(CircuitBoxConnectorIdentifier other)
		{
			return EqualityComparer<Identifier>.Default.Equals(this.<SignalConnection>k__BackingField, other.<SignalConnection>k__BackingField) && EqualityComparer<Option<ushort>>.Default.Equals(this.<TargetId>k__BackingField, other.<TargetId>k__BackingField);
		}

		// Token: 0x06001A44 RID: 6724 RVA: 0x000C9B65 File Offset: 0x000C7D65
		[CompilerGenerated]
		public void Deconstruct(out Identifier SignalConnection, out Option<ushort> TargetId)
		{
			SignalConnection = this.SignalConnection;
			TargetId = this.TargetId;
		}
	}
}
