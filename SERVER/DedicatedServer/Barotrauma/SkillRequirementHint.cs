using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001F6 RID: 502
	internal readonly struct SkillRequirementHint
	{
		// Token: 0x060023C4 RID: 9156 RVA: 0x000EEC58 File Offset: 0x000ECE58
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

		// Token: 0x060023C5 RID: 9157 RVA: 0x000EECD8 File Offset: 0x000ECED8
		public SkillRequirementHint(ContentXElement element)
		{
			this.Skill = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			this.Level = element.GetAttributeFloat("level", 0f);
			this.SkillName = TextManager.Get("skillname." + this.Skill.ToString());
		}

		// Token: 0x04001186 RID: 4486
		public readonly Identifier Skill;

		// Token: 0x04001187 RID: 4487
		public readonly float Level;

		// Token: 0x04001188 RID: 4488
		public readonly LocalizedString SkillName;
	}
}
