using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000202 RID: 514
	[NetworkSerialize(50)]
	internal readonly struct CircuitBoxConnectorIdentifier : INetSerializableStruct, IEquatable<CircuitBoxConnectorIdentifier>
	{
		// Token: 0x06003523 RID: 13603 RVA: 0x0020F97F File Offset: 0x0020DB7F
		public CircuitBoxConnectorIdentifier(Identifier SignalConnection, Option<ushort> TargetId)
		{
			this.SignalConnection = SignalConnection;
			this.TargetId = TargetId;
		}

		// Token: 0x17000E39 RID: 3641
		// (get) Token: 0x06003524 RID: 13604 RVA: 0x0020F98F File Offset: 0x0020DB8F
		// (set) Token: 0x06003525 RID: 13605 RVA: 0x0020F997 File Offset: 0x0020DB97
		public Identifier SignalConnection { get; set; }

		// Token: 0x17000E3A RID: 3642
		// (get) Token: 0x06003526 RID: 13606 RVA: 0x0020F9A0 File Offset: 0x0020DBA0
		// (set) Token: 0x06003527 RID: 13607 RVA: 0x0020F9A8 File Offset: 0x0020DBA8
		public Option<ushort> TargetId { get; set; }

		// Token: 0x06003528 RID: 13608 RVA: 0x0020F9B4 File Offset: 0x0020DBB4
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

		// Token: 0x06003529 RID: 13609 RVA: 0x0020FA2C File Offset: 0x0020DC2C
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

		// Token: 0x0600352A RID: 13610 RVA: 0x0020FAF8 File Offset: 0x0020DCF8
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

		// Token: 0x0600352B RID: 13611 RVA: 0x0020FB68 File Offset: 0x0020DD68
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

		// Token: 0x0600352C RID: 13612 RVA: 0x0020FBDC File Offset: 0x0020DDDC
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

		// Token: 0x0600352D RID: 13613 RVA: 0x0020FC54 File Offset: 0x0020DE54
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("SignalConnection = ");
			builder.Append(this.SignalConnection.ToString());
			builder.Append(", TargetId = ");
			builder.Append(this.TargetId.ToString());
			return true;
		}

		// Token: 0x0600352E RID: 13614 RVA: 0x0020FCB0 File Offset: 0x0020DEB0
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxConnectorIdentifier left, CircuitBoxConnectorIdentifier right)
		{
			return !(left == right);
		}

		// Token: 0x0600352F RID: 13615 RVA: 0x0020FCBC File Offset: 0x0020DEBC
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxConnectorIdentifier left, CircuitBoxConnectorIdentifier right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003530 RID: 13616 RVA: 0x0020FCC6 File Offset: 0x0020DEC6
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Identifier>.Default.GetHashCode(this.<SignalConnection>k__BackingField) * -1521134295 + EqualityComparer<Option<ushort>>.Default.GetHashCode(this.<TargetId>k__BackingField);
		}

		// Token: 0x06003531 RID: 13617 RVA: 0x0020FCEF File Offset: 0x0020DEEF
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxConnectorIdentifier && this.Equals((CircuitBoxConnectorIdentifier)obj);
		}

		// Token: 0x06003532 RID: 13618 RVA: 0x0020FD07 File Offset: 0x0020DF07
		[CompilerGenerated]
		public bool Equals(CircuitBoxConnectorIdentifier other)
		{
			return EqualityComparer<Identifier>.Default.Equals(this.<SignalConnection>k__BackingField, other.<SignalConnection>k__BackingField) && EqualityComparer<Option<ushort>>.Default.Equals(this.<TargetId>k__BackingField, other.<TargetId>k__BackingField);
		}

		// Token: 0x06003533 RID: 13619 RVA: 0x0020FD39 File Offset: 0x0020DF39
		[CompilerGenerated]
		public void Deconstruct(out Identifier SignalConnection, out Option<ushort> TargetId)
		{
			SignalConnection = this.SignalConnection;
			TargetId = this.TargetId;
		}
	}
}
