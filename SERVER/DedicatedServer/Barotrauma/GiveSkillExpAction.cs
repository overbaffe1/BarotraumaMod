using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200019E RID: 414
	internal class GiveSkillExpAction : EventAction
	{
		// Token: 0x170008E3 RID: 2275
		// (get) Token: 0x06001F0E RID: 7950 RVA: 0x000D7E7B File Offset: 0x000D607B
		// (set) Token: 0x06001F0F RID: 7951 RVA: 0x000D7E83 File Offset: 0x000D6083
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the skill to increase.", "", false)]
		public Identifier Skill { get; set; }

		// Token: 0x170008E4 RID: 2276
		// (get) Token: 0x06001F10 RID: 7952 RVA: 0x000D7E8C File Offset: 0x000D608C
		// (set) Token: 0x06001F11 RID: 7953 RVA: 0x000D7E94 File Offset: 0x000D6094
		[Serialize(0f, IsPropertySaveable.Yes, "How much the skill should increase.", "", false)]
		public float Amount { get; set; }

		// Token: 0x170008E5 RID: 2277
		// (get) Token: 0x06001F12 RID: 7954 RVA: 0x000D7E9D File Offset: 0x000D609D
		// (set) Token: 0x06001F13 RID: 7955 RVA: 0x000D7EA5 File Offset: 0x000D60A5
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) whose skill to increase.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06001F14 RID: 7956 RVA: 0x000D7EB0 File Offset: 0x000D60B0
		public GiveSkillExpAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.TargetTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(89, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": ");
				defaultInterpolatedStringHandler.AppendFormatted("GiveSkillExpAction");
				defaultInterpolatedStringHandler.AppendLiteral(" without a target tag (the action needs to know whose skill to check).");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x06001F15 RID: 7957 RVA: 0x000D7F36 File Offset: 0x000D6136
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001F16 RID: 7958 RVA: 0x000D7F3E File Offset: 0x000D613E
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001F17 RID: 7959 RVA: 0x000D7F48 File Offset: 0x000D6148
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			IEnumerable<Character> targets = from e in this.ParentEvent.GetTargets(this.TargetTag)
			where e is Character
			select e as Character;
			foreach (Character target in targets)
			{
				CharacterInfo info = target.Info;
				if (info != null)
				{
					info.IncreaseSkillLevel(this.Skill, this.Amount, false, false);
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06001F18 RID: 7960 RVA: 0x000D8014 File Offset: 0x000D6214
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 5);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("GiveSkillExpAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Skill: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Skill.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Amount: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Amount.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000ED8 RID: 3800
		private bool isFinished;
	}
}
