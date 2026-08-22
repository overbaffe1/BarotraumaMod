using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002AF RID: 687
	internal class WaitAction : EventAction
	{
		// Token: 0x17000FB8 RID: 4024
		// (get) Token: 0x06003BBA RID: 15290 RVA: 0x002246CC File Offset: 0x002228CC
		// (set) Token: 0x06003BBB RID: 15291 RVA: 0x002246D4 File Offset: 0x002228D4
		[Serialize(0f, IsPropertySaveable.Yes, "How long to wait (in seconds).", "", false)]
		public float Time { get; set; }

		// Token: 0x06003BBC RID: 15292 RVA: 0x002246DD File Offset: 0x002228DD
		public WaitAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.timeRemaining = this.Time;
		}

		// Token: 0x06003BBD RID: 15293 RVA: 0x002246F3 File Offset: 0x002228F3
		public override bool IsFinished(ref string goTo)
		{
			return this.timeRemaining <= 0f;
		}

		// Token: 0x06003BBE RID: 15294 RVA: 0x00224705 File Offset: 0x00222905
		public override void Reset()
		{
			this.timeRemaining = this.Time;
		}

		// Token: 0x06003BBF RID: 15295 RVA: 0x00224713 File Offset: 0x00222913
		public override void Update(float deltaTime)
		{
			this.timeRemaining -= deltaTime;
			if (this.timeRemaining < 0f)
			{
				this.timeRemaining = 0f;
			}
		}

		// Token: 0x06003BC0 RID: 15296 RVA: 0x0022473C File Offset: 0x0022293C
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.timeRemaining <= 0f, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("WaitAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Remaining: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.timeRemaining.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Time: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Time.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001E81 RID: 7809
		private float timeRemaining;
	}
}
