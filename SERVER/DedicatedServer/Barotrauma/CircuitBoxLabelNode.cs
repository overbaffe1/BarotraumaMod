using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000105 RID: 261
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CircuitBoxLabelNode : CircuitBoxNode, ICircuitBoxIdentifiable
	{
		// Token: 0x170007CF RID: 1999
		// (get) Token: 0x06001A1C RID: 6684 RVA: 0x000C9368 File Offset: 0x000C7568
		public ushort ID { get; }

		// Token: 0x170007D0 RID: 2000
		// (get) Token: 0x06001A1D RID: 6685 RVA: 0x000C9370 File Offset: 0x000C7570
		public override bool IsResizable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170007D1 RID: 2001
		// (get) Token: 0x06001A1E RID: 6686 RVA: 0x000C9373 File Offset: 0x000C7573
		public static NetLimitedString DefaultHeaderText
		{
			get
			{
				return new NetLimitedString("label");
			}
		}

		// Token: 0x06001A1F RID: 6687 RVA: 0x000C9380 File Offset: 0x000C7580
		public CircuitBoxLabelNode(ushort id, Color color, Vector2 pos, CircuitBox circuitBox) : base(circuitBox)
		{
			this.Size = new Vector2(256f);
			base.Position = pos;
			this.ID = id;
			this.Color = color;
			base.UpdatePositions();
		}

		// Token: 0x06001A20 RID: 6688 RVA: 0x000C93D6 File Offset: 0x000C75D6
		public void EditText(NetLimitedString header, NetLimitedString body)
		{
			this.HeaderText = header;
			this.BodyText = body;
		}

		// Token: 0x06001A21 RID: 6689 RVA: 0x000C93E8 File Offset: 0x000C75E8
		public XElement Save()
		{
			return new XElement("Label", new object[]
			{
				new XAttribute("id", this.ID),
				new XAttribute("color", this.Color.ToStringHex()),
				new XAttribute("position", XMLExtensions.Vector2ToString(base.Position)),
				new XAttribute("size", XMLExtensions.Vector2ToString(this.Size)),
				new XAttribute("header", this.HeaderText),
				new XAttribute("body", this.BodyText)
			});
		}

		// Token: 0x06001A22 RID: 6690 RVA: 0x000C94BC File Offset: 0x000C76BC
		public static CircuitBoxLabelNode LoadFromXML(ContentXElement element, CircuitBox circuitBox)
		{
			ushort id = element.GetAttributeUInt16("id", ushort.MaxValue);
			string key = "position";
			Vector2 zero = Vector2.Zero;
			Vector2 position = element.GetAttributeVector2(key, zero);
			string key2 = "size";
			zero = Vector2.Zero;
			Vector2 size = element.GetAttributeVector2(key2, zero);
			string key3 = "color";
			Color white = Color.White;
			Color color = element.GetAttributeColor(key3, white);
			string header = element.GetAttributeString("header", string.Empty);
			string body = element.GetAttributeString("body", string.Empty);
			CircuitBoxLabelNode labelNode = new CircuitBoxLabelNode(id, color, position, circuitBox)
			{
				Size = size,
				HeaderText = new NetLimitedString(header),
				BodyText = new NetLimitedString(body)
			};
			labelNode.EditText(new NetLimitedString(header), new NetLimitedString(body));
			labelNode.UpdatePositions();
			return labelNode;
		}

		// Token: 0x04000C81 RID: 3201
		public Color Color;

		// Token: 0x04000C83 RID: 3203
		public NetLimitedString BodyText = NetLimitedString.Empty;

		// Token: 0x04000C84 RID: 3204
		public NetLimitedString HeaderText = CircuitBoxLabelNode.DefaultHeaderText;

		// Token: 0x04000C85 RID: 3205
		public static Vector2 MinSize = new Vector2(128f, 8f);
	}
}
