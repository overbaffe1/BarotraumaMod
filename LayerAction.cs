using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000295 RID: 661
	internal class LayerAction : EventAction
	{
		// Token: 0x17000F41 RID: 3905
		// (get) Token: 0x060039FF RID: 14847 RVA: 0x0021D856 File Offset: 0x0021BA56
		// (set) Token: 0x06003A00 RID: 14848 RVA: 0x0021D85E File Offset: 0x0021BA5E
		[Serialize("", IsPropertySaveable.Yes, "Which layer to enable/disable. Use \"All\" to apply it to all layers.", "", false)]
		public Identifier Layer { get; set; }

		// Token: 0x17000F42 RID: 3906
		// (get) Token: 0x06003A01 RID: 14849 RVA: 0x0021D867 File Offset: 0x0021BA67
		// (set) Token: 0x06003A02 RID: 14850 RVA: 0x0021D86F File Offset: 0x0021BA6F
		[Serialize(false, IsPropertySaveable.Yes, "Whether to enable or disable the layer.", "", false)]
		public bool Enabled { get; set; }

		// Token: 0x17000F43 RID: 3907
		// (get) Token: 0x06003A03 RID: 14851 RVA: 0x0021D878 File Offset: 0x0021BA78
		// (set) Token: 0x06003A04 RID: 14852 RVA: 0x0021D880 File Offset: 0x0021BA80
		[Serialize(TagAction.SubType.Any, IsPropertySaveable.Yes, "The type of submatine to enable or disable the layer in.", "", false)]
		public TagAction.SubType SubmarineType { get; set; }

		// Token: 0x17000F44 RID: 3908
		// (get) Token: 0x06003A05 RID: 14853 RVA: 0x0021D889 File Offset: 0x0021BA89
		// (set) Token: 0x06003A06 RID: 14854 RVA: 0x0021D891 File Offset: 0x0021BA91
		[Serialize(true, IsPropertySaveable.Yes, "Should the action continue if it can't find the specified layer in the specified submarine(s).", "", false)]
		public bool ContinueIfNotFound { get; set; }

		// Token: 0x06003A07 RID: 14855 RVA: 0x0021D89A File Offset: 0x0021BA9A
		public LayerAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003A08 RID: 14856 RVA: 0x0021D8A4 File Offset: 0x0021BAA4
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003A09 RID: 14857 RVA: 0x0021D8AC File Offset: 0x0021BAAC
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003A0A RID: 14858 RVA: 0x0021D8B8 File Offset: 0x0021BAB8
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			bool layerFound = false;
			foreach (Submarine submarine in Submarine.Loaded)
			{
				if (TagAction.SubmarineTypeMatches(submarine, this.SubmarineType) && submarine.LayerExists(this.Layer))
				{
					submarine.SetLayerEnabled(this.Layer, this.Enabled, true);
					layerFound = true;
				}
			}
			if (this.ContinueIfNotFound)
			{
				this.isFinished = true;
				return;
			}
			if (layerFound)
			{
				this.isFinished = true;
			}
		}

		// Token: 0x06003A0B RID: 14859 RVA: 0x0021D95C File Offset: 0x0021BB5C
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("LayerAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (");
			defaultInterpolatedStringHandler.AppendFormatted(this.Enabled ? "Enable" : "Disable");
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Layer.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001DDD RID: 7645
		private bool isFinished;
	}
}
