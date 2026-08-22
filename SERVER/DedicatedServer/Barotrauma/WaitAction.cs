using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001BD RID: 445
	internal class WaitAction : EventAction
	{
		// Token: 0x17000979 RID: 2425
		// (get) Token: 0x0600211C RID: 8476 RVA: 0x000DE836 File Offset: 0x000DCA36
		// (set) Token: 0x0600211D RID: 8477 RVA: 0x000DE83E File Offset: 0x000DCA3E
		[Serialize(0f, IsPropertySaveable.Yes, "How long to wait (in seconds).", "", false)]
		public float Time { get; set; }

		// Token: 0x0600211E RID: 8478 RVA: 0x000DE847 File Offset: 0x000DCA47
		public WaitAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.timeRemaining = this.Time;
		}

		// Token: 0x0600211F RID: 8479 RVA: 0x000DE85D File Offset: 0x000DCA5D
		public override bool IsFinished(ref string goTo)
		{
			return this.timeRemaining <= 0f;
		}

		// Token: 0x06002120 RID: 8480 RVA: 0x000DE86F File Offset: 0x000DCA6F
		public override void Reset()
		{
			this.timeRemaining = this.Time;
		}

		// Token: 0x06002121 RID: 8481 RVA: 0x000DE87D File Offset: 0x000DCA7D
		public override void Update(float deltaTime)
		{
			this.timeRemaining -= deltaTime;
			if (this.timeRemaining < 0f)
			{
				this.timeRemaining = 0f;
			}
		}

		// Token: 0x06002122 RID: 8482 RVA: 0x000DE8A8 File Offset: 0x000DCAA8
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

		// Token: 0x04000F9D RID: 3997
		private float timeRemaining;
	}
}
