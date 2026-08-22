using System;

namespace Barotrauma
{
	// Token: 0x02000351 RID: 849
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class Serialize : Attribute
	{
		// Token: 0x06004246 RID: 16966 RVA: 0x00249B76 File Offset: 0x00247D76
		public Serialize(object defaultValue, IsPropertySaveable isSaveable, string description = "", string translationTextTag = "", bool alwaysUseInstanceValues = false)
		{
			this.DefaultValue = defaultValue;
			this.IsSaveable = isSaveable;
			this.TranslationTextTag = translationTextTag.ToIdentifier();
			this.Description = description;
			this.AlwaysUseInstanceValues = alwaysUseInstanceValues;
		}

		// Token: 0x04002281 RID: 8833
		public readonly object DefaultValue;

		// Token: 0x04002282 RID: 8834
		public readonly IsPropertySaveable IsSaveable;

		// Token: 0x04002283 RID: 8835
		public readonly Identifier TranslationTextTag;

		// Token: 0x04002284 RID: 8836
		public bool AlwaysUseInstanceValues;

		// Token: 0x04002285 RID: 8837
		public string Description;
	}
}
