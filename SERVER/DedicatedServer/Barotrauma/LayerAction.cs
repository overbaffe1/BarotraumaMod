using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001A3 RID: 419
	internal class LayerAction : EventAction
	{
		// Token: 0x170008F1 RID: 2289
		// (get) Token: 0x06001F41 RID: 8001 RVA: 0x000D8413 File Offset: 0x000D6613
		// (set) Token: 0x06001F42 RID: 8002 RVA: 0x000D841B File Offset: 0x000D661B
		[Serialize("", IsPropertySaveable.Yes, "Which layer to enable/disable. Use \"All\" to apply it to all layers.", "", false)]
		public Identifier Layer { get; set; }

		// Token: 0x170008F2 RID: 2290
		// (get) Token: 0x06001F43 RID: 8003 RVA: 0x000D8424 File Offset: 0x000D6624
		// (set) Token: 0x06001F44 RID: 8004 RVA: 0x000D842C File Offset: 0x000D662C
		[Serialize(false, IsPropertySaveable.Yes, "Whether to enable or disable the layer.", "", false)]
		public bool Enabled { get; set; }

		// Token: 0x170008F3 RID: 2291
		// (get) Token: 0x06001F45 RID: 8005 RVA: 0x000D8435 File Offset: 0x000D6635
		// (set) Token: 0x06001F46 RID: 8006 RVA: 0x000D843D File Offset: 0x000D663D
		[Serialize(TagAction.SubType.Any, IsPropertySaveable.Yes, "The type of submatine to enable or disable the layer in.", "", false)]
		public TagAction.SubType SubmarineType { get; set; }

		// Token: 0x170008F4 RID: 2292
		// (get) Token: 0x06001F47 RID: 8007 RVA: 0x000D8446 File Offset: 0x000D6646
		// (set) Token: 0x06001F48 RID: 8008 RVA: 0x000D844E File Offset: 0x000D664E
		[Serialize(true, IsPropertySaveable.Yes, "Should the action continue if it can't find the specified layer in the specified submarine(s).", "", false)]
		public bool ContinueIfNotFound { get; set; }

		// Token: 0x06001F49 RID: 8009 RVA: 0x000D8457 File Offset: 0x000D6657
		public LayerAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001F4A RID: 8010 RVA: 0x000D8461 File Offset: 0x000D6661
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001F4B RID: 8011 RVA: 0x000D8469 File Offset: 0x000D6669
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001F4C RID: 8012 RVA: 0x000D8474 File Offset: 0x000D6674
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

		// Token: 0x06001F4D RID: 8013 RVA: 0x000D8518 File Offset: 0x000D6718
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

		// Token: 0x04000EEB RID: 3819
		private bool isFinished;
	}
}
