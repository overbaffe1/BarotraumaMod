using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000366 RID: 870
	[NullableContext(1)]
	[Nullable(0)]
	public class FallbackLString : LocalizedString
	{
		// Token: 0x1700119A RID: 4506
		// (get) Token: 0x06004317 RID: 17175 RVA: 0x0025238C File Offset: 0x0025058C
		// (set) Token: 0x06004318 RID: 17176 RVA: 0x00252394 File Offset: 0x00250594
		public bool PrimaryIsLoaded { get; private set; }

		// Token: 0x06004319 RID: 17177 RVA: 0x002523A0 File Offset: 0x002505A0
		public FallbackLString(LocalizedString primary, LocalizedString fallback, bool useDefaultLanguageIfFound = true)
		{
			this.useDefaultLanguageIfFound = useDefaultLanguageIfFound;
			FallbackLString fallbackLString = primary as FallbackLString;
			if (fallbackLString != null)
			{
				LocalizedString innerPrimary = fallbackLString.primary;
				if (innerPrimary != null)
				{
					LocalizedString innerFallback = fallbackLString.fallback;
					if (innerFallback != null)
					{
						this.primary = innerPrimary;
						this.fallback = innerFallback.Fallback(fallback, true);
						return;
					}
				}
			}
			this.primary = primary;
			this.fallback = fallback;
		}

		// Token: 0x0600431A RID: 17178 RVA: 0x002523FC File Offset: 0x002505FC
		protected override bool MustRetrieveValue()
		{
			return base.MustRetrieveValue() || LocalizedString.MustRetrieveValue(this.primary) || LocalizedString.MustRetrieveValue(this.fallback) || this.PrimaryIsLoaded != this.primary.Loaded;
		}

		// Token: 0x1700119B RID: 4507
		// (get) Token: 0x0600431B RID: 17179 RVA: 0x00252438 File Offset: 0x00250638
		public override bool Loaded
		{
			get
			{
				return this.primary.Loaded || this.fallback.Loaded;
			}
		}

		// Token: 0x0600431C RID: 17180 RVA: 0x00252454 File Offset: 0x00250654
		public override void RetrieveValue()
		{
			this.cachedValue = this.primary.Value;
			this.PrimaryIsLoaded = this.primary.Loaded;
			TagLString tagLString = this.primary as TagLString;
			bool defaultLanguageFallbackAvailable = tagLString != null && tagLString.UsingDefaultLanguageAsFallback;
			if (!this.primary.Loaded && (!defaultLanguageFallbackAvailable || !this.useDefaultLanguageIfFound))
			{
				this.cachedValue = this.fallback.Value;
			}
		}

		// Token: 0x0600431D RID: 17181 RVA: 0x002524C8 File Offset: 0x002506C8
		public LocalizedString GetLastFallback()
		{
			FallbackLString innerFallback = this.fallback as FallbackLString;
			if (innerFallback != null)
			{
				return innerFallback.GetLastFallback();
			}
			return this.fallback;
		}

		// Token: 0x0400233D RID: 9021
		private readonly LocalizedString primary;

		// Token: 0x0400233E RID: 9022
		private readonly LocalizedString fallback;

		// Token: 0x04002340 RID: 9024
		private readonly bool useDefaultLanguageIfFound;
	}
}
