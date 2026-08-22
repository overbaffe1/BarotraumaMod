using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000D5 RID: 213
	internal class SkillPrefab
	{
		// Token: 0x170006D7 RID: 1751
		// (get) Token: 0x06001767 RID: 5991 RVA: 0x000C2E3D File Offset: 0x000C103D
		public bool IsPrimarySkill { get; }

		// Token: 0x06001768 RID: 5992 RVA: 0x000C2E48 File Offset: 0x000C1048
		public SkillPrefab(ContentXElement element)
		{
			this.Identifier = element.GetAttributeIdentifier("identifier", "");
			this.PriceMultiplier = element.GetAttributeFloat("pricemultiplier", 15f);
			this.levelRange = SkillPrefab.<.ctor>g__GetSkillRange|7_0("level", element, new Range<float>(0f, 0f));
			this.levelRangePvP = SkillPrefab.<.ctor>g__GetSkillRange|7_0("pvplevel", element, this.levelRange);
			this.IsPrimarySkill = element.GetAttributeBool("primary", false);
		}

		// Token: 0x06001769 RID: 5993 RVA: 0x000C2ED0 File Offset: 0x000C10D0
		public Range<float> GetLevelRange(bool isPvP)
		{
			if (!isPvP)
			{
				return this.levelRange;
			}
			return this.levelRangePvP;
		}

		// Token: 0x0600176A RID: 5994 RVA: 0x000C2EE4 File Offset: 0x000C10E4
		[CompilerGenerated]
		internal static Range<float> <.ctor>g__GetSkillRange|7_0(string attributeName, ContentXElement element, Range<float> defaultValue)
		{
			string levelString = element.GetAttributeString(attributeName, string.Empty);
			if (levelString.Contains(','))
			{
				Vector2 rangeVector2 = XMLExtensions.ParseVector2(levelString, false);
				return new Range<float>(rangeVector2.X, rangeVector2.Y);
			}
			float skillLevel;
			if (float.TryParse(levelString, NumberStyles.Any, CultureInfo.InvariantCulture, out skillLevel))
			{
				return new Range<float>(skillLevel, skillLevel);
			}
			return defaultValue;
		}

		// Token: 0x04000B53 RID: 2899
		public readonly Identifier Identifier;

		// Token: 0x04000B54 RID: 2900
		private readonly Range<float> levelRange;

		// Token: 0x04000B55 RID: 2901
		private readonly Range<float> levelRangePvP;

		// Token: 0x04000B56 RID: 2902
		public readonly float PriceMultiplier;
	}
}
