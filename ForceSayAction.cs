using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200028F RID: 655
	internal class ForceSayAction : EventAction
	{
		// Token: 0x17000F31 RID: 3889
		// (get) Token: 0x060039C2 RID: 14786 RVA: 0x0021CF63 File Offset: 0x0021B163
		// (set) Token: 0x060039C3 RID: 14787 RVA: 0x0021CF6B File Offset: 0x0021B16B
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character that should say the message.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000F32 RID: 3890
		// (get) Token: 0x060039C4 RID: 14788 RVA: 0x0021CF74 File Offset: 0x0021B174
		// (set) Token: 0x060039C5 RID: 14789 RVA: 0x0021CF7C File Offset: 0x0021B17C
		[Serialize("", IsPropertySaveable.Yes, "The message that the character should say. Can be the text as-is, or a tag referring to a line in a text file.", "", false)]
		public string Message { get; set; }

		// Token: 0x17000F33 RID: 3891
		// (get) Token: 0x060039C6 RID: 14790 RVA: 0x0021CF85 File Offset: 0x0021B185
		// (set) Token: 0x060039C7 RID: 14791 RVA: 0x0021CF8D File Offset: 0x0021B18D
		[Serialize(false, IsPropertySaveable.Yes, "Should the message that the character says be sent in radio?", "", false)]
		public bool SayInRadio { get; set; }

		// Token: 0x17000F34 RID: 3892
		// (get) Token: 0x060039C8 RID: 14792 RVA: 0x0021CF96 File Offset: 0x0021B196
		// (set) Token: 0x060039C9 RID: 14793 RVA: 0x0021CF9E File Offset: 0x0021B19E
		[Serialize(true, IsPropertySaveable.Yes, "Should the message be stripped of any quotation mark characters?", "", false)]
		public bool RemoveQuotes { get; set; }

		// Token: 0x060039CA RID: 14794 RVA: 0x0021CFA7 File Offset: 0x0021B1A7
		public ForceSayAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060039CB RID: 14795 RVA: 0x0021CFB1 File Offset: 0x0021B1B1
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x060039CC RID: 14796 RVA: 0x0021CFB9 File Offset: 0x0021B1B9
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x060039CD RID: 14797 RVA: 0x0021CFC4 File Offset: 0x0021B1C4
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

		// Token: 0x060039CE RID: 14798 RVA: 0x0021D06C File Offset: 0x0021B26C
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

		// Token: 0x04001DC8 RID: 7624
		private bool isFinished;
	}
}
