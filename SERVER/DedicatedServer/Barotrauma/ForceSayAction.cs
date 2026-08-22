using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200019C RID: 412
	internal class ForceSayAction : EventAction
	{
		// Token: 0x170008DD RID: 2269
		// (get) Token: 0x06001EF8 RID: 7928 RVA: 0x000D7AAF File Offset: 0x000D5CAF
		// (set) Token: 0x06001EF9 RID: 7929 RVA: 0x000D7AB7 File Offset: 0x000D5CB7
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character that should say the message.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170008DE RID: 2270
		// (get) Token: 0x06001EFA RID: 7930 RVA: 0x000D7AC0 File Offset: 0x000D5CC0
		// (set) Token: 0x06001EFB RID: 7931 RVA: 0x000D7AC8 File Offset: 0x000D5CC8
		[Serialize("", IsPropertySaveable.Yes, "The message that the character should say. Can be the text as-is, or a tag referring to a line in a text file.", "", false)]
		public string Message { get; set; }

		// Token: 0x170008DF RID: 2271
		// (get) Token: 0x06001EFC RID: 7932 RVA: 0x000D7AD1 File Offset: 0x000D5CD1
		// (set) Token: 0x06001EFD RID: 7933 RVA: 0x000D7AD9 File Offset: 0x000D5CD9
		[Serialize(false, IsPropertySaveable.Yes, "Should the message that the character says be sent in radio?", "", false)]
		public bool SayInRadio { get; set; }

		// Token: 0x170008E0 RID: 2272
		// (get) Token: 0x06001EFE RID: 7934 RVA: 0x000D7AE2 File Offset: 0x000D5CE2
		// (set) Token: 0x06001EFF RID: 7935 RVA: 0x000D7AEA File Offset: 0x000D5CEA
		[Serialize(true, IsPropertySaveable.Yes, "Should the message be stripped of any quotation mark characters?", "", false)]
		public bool RemoveQuotes { get; set; }

		// Token: 0x06001F00 RID: 7936 RVA: 0x000D7AF3 File Offset: 0x000D5CF3
		public ForceSayAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001F01 RID: 7937 RVA: 0x000D7AFD File Offset: 0x000D5CFD
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001F02 RID: 7938 RVA: 0x000D7B05 File Offset: 0x000D5D05
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001F03 RID: 7939 RVA: 0x000D7B10 File Offset: 0x000D5D10
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.TargetTag);
			LocalizedString messageToSay = TextManager.Get(this.Message).Fallback(this.Message, true);
			foreach (Entity target in targets)
			{
				if (target != null)
				{
					Character character = target as Character;
					if (character != null)
					{
						character.ForceSay(messageToSay, this.SayInRadio, this.RemoveQuotes, 0f);
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06001F04 RID: 7940 RVA: 0x000D7BB8 File Offset: 0x000D5DB8
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("ForceSayAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Message: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Message);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000ED1 RID: 3793
		private bool isFinished;
	}
}
