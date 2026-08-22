using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002E0 RID: 736
	internal readonly struct SkillRequirementHint
	{
		// Token: 0x06003D6A RID: 15722 RVA: 0x0022E3BC File Offset: 0x0022C5BC
		public LocalizedString GetFormattedText(int skillLevel, string levelColorTag)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 4);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.SkillName);
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted<float>(this.Level);
			defaultInterpolatedStringHandler.AppendLiteral(" (‖color:");
			defaultInterpolatedStringHandler.AppendFormatted(levelColorTag);
			defaultInterpolatedStringHandler.AppendLiteral("‖");
			defaultInterpolatedStringHandler.AppendFormatted<int>(skillLevel);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖)");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003D6B RID: 15723 RVA: 0x0022E43C File Offset: 0x0022C63C
		public SkillRequirementHint(ContentXElement element)
		{
			this.Skill = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			this.Level = element.GetAttributeFloat("level", 0f);
			this.SkillName = TextManager.Get("skillname." + this.Skill.ToString());
		}

		// Token: 0x0400200F RID: 8207
		public readonly Identifier Skill;

		// Token: 0x04002010 RID: 8208
		public readonly float Level;

		// Token: 0x04002011 RID: 8209
		public readonly LocalizedString SkillName;
	}
}
