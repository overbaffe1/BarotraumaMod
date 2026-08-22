using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000104 RID: 260
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CircuitBoxInputOutputNode : CircuitBoxNode
	{
		// Token: 0x06001A17 RID: 6679 RVA: 0x000C9027 File Offset: 0x000C7227
		public CircuitBoxInputOutputNode(IReadOnlyList<CircuitBoxConnection> conns, Vector2 initialPosition, CircuitBoxInputOutputNode.Type type, CircuitBox circuitBox) : base(circuitBox)
		{
			this.InitSize(conns);
			this.Connectors = conns.ToImmutableArray<CircuitBoxConnection>();
			base.Position = initialPosition;
			this.NodeType = type;
			base.UpdatePositions();
		}

		// Token: 0x06001A18 RID: 6680 RVA: 0x000C9064 File Offset: 0x000C7264
		public void ReplaceAllConnectionLabelOverrides(Dictionary<string, string> replace)
		{
			foreach (KeyValuePair<string, string> keyValuePair in replace)
			{
				string text;
				string text2;
				keyValuePair.Deconstruct(out text, out text2);
				string value = text2;
				if (value.Length > 32)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Label override value \"");
					defaultInterpolatedStringHandler.AppendFormatted(value);
					defaultInterpolatedStringHandler.AppendLiteral("\" is too long (max ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(32);
					defaultInterpolatedStringHandler.AppendLiteral(" characters)");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return;
				}
			}
			foreach (KeyValuePair<string, string> keyValuePair in replace)
			{
				string text;
				string text2;
				keyValuePair.Deconstruct(out text2, out text);
				string name = text2;
				string value2 = text;
				if (string.IsNullOrWhiteSpace(value2))
				{
					this.ConnectionLabelOverrides.Remove(name);
				}
				else
				{
					this.ConnectionLabelOverrides[name] = value2;
				}
			}
			this.InitSize(this.Connectors);
			base.UpdatePositions();
		}

		// Token: 0x06001A19 RID: 6681 RVA: 0x000C91A0 File Offset: 0x000C73A0
		private void InitSize(IReadOnlyList<CircuitBoxConnection> conns)
		{
			this.Size = CircuitBoxNode.CalculateSize(conns);
		}

		// Token: 0x06001A1A RID: 6682 RVA: 0x000C91B0 File Offset: 0x000C73B0
		public XElement Save()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 1);
			defaultInterpolatedStringHandler.AppendFormatted<CircuitBoxInputOutputNode.Type>(this.NodeType);
			defaultInterpolatedStringHandler.AppendLiteral("Node");
			XElement element = new XElement(defaultInterpolatedStringHandler.ToStringAndClear(), new XAttribute("pos", XMLExtensions.Vector2ToString(base.Position)));
			foreach (KeyValuePair<string, string> keyValuePair in this.ConnectionLabelOverrides)
			{
				string text;
				string text2;
				keyValuePair.Deconstruct(out text, out text2);
				string name = text;
				string value = text2;
				element.Add(new XElement("ConnectionLabelOverride", new object[]
				{
					new XAttribute("name", name),
					new XAttribute("value", value)
				}));
			}
			return element;
		}

		// Token: 0x06001A1B RID: 6683 RVA: 0x000C92A0 File Offset: 0x000C74A0
		public void Load(ContentXElement element)
		{
			string key = "pos";
			Vector2 zero = Vector2.Zero;
			base.Position = element.GetAttributeVector2(key, zero);
			Dictionary<string, string> loadedOverrides = new Dictionary<string, string>();
			foreach (ContentXElement subElement in element.Elements())
			{
				if (!(subElement.Name != "ConnectionLabelOverride"))
				{
					string name = subElement.GetAttributeString("name", string.Empty);
					string value = subElement.GetAttributeString("value", string.Empty);
					loadedOverrides[name] = value;
				}
			}
			this.ConnectionLabelOverrides = loadedOverrides;
			this.InitSize(this.Connectors);
			base.UpdatePositions();
		}

		// Token: 0x04000C7D RID: 3197
		public readonly CircuitBoxInputOutputNode.Type NodeType;

		// Token: 0x04000C7E RID: 3198
		private const int MaxConnectionLabelLength = 32;

		// Token: 0x04000C7F RID: 3199
		private const string ConnectionLabelOverrideElementName = "ConnectionLabelOverride";

		// Token: 0x04000C80 RID: 3200
		public Dictionary<string, string> ConnectionLabelOverrides = new Dictionary<string, string>();

		// Token: 0x020008CE RID: 2254
		[NullableContext(0)]
		public enum Type
		{
			// Token: 0x0400314E RID: 12622
			Invalid,
			// Token: 0x0400314F RID: 12623
			Input,
			// Token: 0x04003150 RID: 12624
			Output
		}
	}
}
