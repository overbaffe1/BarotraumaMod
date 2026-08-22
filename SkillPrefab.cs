using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001D2 RID: 466
	internal class SkillPrefab
	{
		// Token: 0x17000D4C RID: 3404
		// (get) Token: 0x0600327C RID: 12924 RVA: 0x00209B89 File Offset: 0x00207D89
		public bool IsPrimarySkill { get; }

		// Token: 0x0600327D RID: 12925 RVA: 0x00209B94 File Offset: 0x00207D94
		public SkillPrefab(ContentXElement element)
		{
			this.Identifier = element.GetAttributeIdentifier("identifier", "");
			this.PriceMultiplier = element.GetAttributeFloat("pricemultiplier", 15f);
			this.levelRange = SkillPrefab.<.ctor>g__GetSkillRange|7_0("level", element, new Range<float>(0f, 0f));
			this.levelRangePvP = SkillPrefab.<.ctor>g__GetSkillRange|7_0("pvplevel", element, this.levelRange);
			this.IsPrimarySkill = element.GetAttributeBool("primary", false);
		}

		// Token: 0x0600327E RID: 12926 RVA: 0x00209C1C File Offset: 0x00207E1C
		public Range<float> GetLevelRange(bool isPvP)
		{
			if (!isPvP)
			{
				return this.levelRange;
			}
			return this.levelRangePvP;
		}

		// Token: 0x0600327F RID: 12927 RVA: 0x00209C30 File Offset: 0x00207E30
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

		// Token: 0x04001A7F RID: 6783
		public readonly Identifier Identifier;

		// Token: 0x04001A80 RID: 6784
		private readonly Range<float> levelRange;

		// Token: 0x04001A81 RID: 6785
		private readonly Range<float> levelRangePvP;

		// Token: 0x04001A82 RID: 6786
		public readonly float PriceMultiplier;
	}
}
