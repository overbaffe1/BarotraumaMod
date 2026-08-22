using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000196 RID: 406
	internal class ClearTagAction : EventAction
	{
		// Token: 0x170008C7 RID: 2247
		// (get) Token: 0x06001EA8 RID: 7848 RVA: 0x000D6A2B File Offset: 0x000D4C2B
		// (set) Token: 0x06001EA9 RID: 7849 RVA: 0x000D6A33 File Offset: 0x000D4C33
		[Serialize("", IsPropertySaveable.Yes, "The tag to clear.", "", false)]
		public Identifier Tag { get; set; }

		// Token: 0x06001EAA RID: 7850 RVA: 0x000D6A3C File Offset: 0x000D4C3C
		public ClearTagAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001EAB RID: 7851 RVA: 0x000D6A46 File Offset: 0x000D4C46
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06001EAC RID: 7852 RVA: 0x000D6A4E File Offset: 0x000D4C4E
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001EAD RID: 7853 RVA: 0x000D6A58 File Offset: 0x000D4C58
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			if (!this.Tag.IsEmpty)
			{
				this.ParentEvent.RemoveTag(this.Tag);
			}
			this.isFinished = true;
		}

		// Token: 0x06001EAE RID: 7854 RVA: 0x000D6A98 File Offset: 0x000D4C98
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("ClearTagAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Tag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Tag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000EB1 RID: 3761
		private bool isFinished;
	}
}
