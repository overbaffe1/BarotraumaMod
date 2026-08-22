using System;

namespace Barotrauma
{
	// Token: 0x02000280 RID: 640
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class Serialize : Attribute
	{
		// Token: 0x06002D34 RID: 11572 RVA: 0x00129A86 File Offset: 0x00127C86
		public Serialize(object defaultValue, IsPropertySaveable isSaveable, string description = "", string translationTextTag = "", bool alwaysUseInstanceValues = false)
		{
			this.DefaultValue = defaultValue;
			this.IsSaveable = isSaveable;
			this.TranslationTextTag = translationTextTag.ToIdentifier();
			this.Description = description;
			this.AlwaysUseInstanceValues = alwaysUseInstanceValues;
		}

		// Token: 0x0400163D RID: 5693
		public readonly object DefaultValue;

		// Token: 0x0400163E RID: 5694
		public readonly IsPropertySaveable IsSaveable;

		// Token: 0x0400163F RID: 5695
		public readonly Identifier TranslationTextTag;

		// Token: 0x04001640 RID: 5696
		public bool AlwaysUseInstanceValues;

		// Token: 0x04001641 RID: 5697
		public string Description;
	}
}
