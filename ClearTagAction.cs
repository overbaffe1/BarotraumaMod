using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000289 RID: 649
	internal class ClearTagAction : EventAction
	{
		// Token: 0x17000F1B RID: 3867
		// (get) Token: 0x06003972 RID: 14706 RVA: 0x0021BEDF File Offset: 0x0021A0DF
		// (set) Token: 0x06003973 RID: 14707 RVA: 0x0021BEE7 File Offset: 0x0021A0E7
		[Serialize("", IsPropertySaveable.Yes, "The tag to clear.", "", false)]
		public Identifier Tag { get; set; }

		// Token: 0x06003974 RID: 14708 RVA: 0x0021BEF0 File Offset: 0x0021A0F0
		public ClearTagAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003975 RID: 14709 RVA: 0x0021BEFA File Offset: 0x0021A0FA
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06003976 RID: 14710 RVA: 0x0021BF02 File Offset: 0x0021A102
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003977 RID: 14711 RVA: 0x0021BF0C File Offset: 0x0021A10C
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

		// Token: 0x06003978 RID: 14712 RVA: 0x0021BF4C File Offset: 0x0021A14C
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

		// Token: 0x04001DA8 RID: 7592
		private bool isFinished;
	}
}
