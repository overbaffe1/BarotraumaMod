using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200029A RID: 666
	[NullableContext(1)]
	[Nullable(0)]
	public class FallbackLString : LocalizedString
	{
		// Token: 0x17000D98 RID: 3480
		// (get) Token: 0x06002E82 RID: 11906 RVA: 0x0013807C File Offset: 0x0013627C
		// (set) Token: 0x06002E83 RID: 11907 RVA: 0x00138084 File Offset: 0x00136284
		public bool PrimaryIsLoaded { get; private set; }

		// Token: 0x06002E84 RID: 11908 RVA: 0x00138090 File Offset: 0x00136290
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

		// Token: 0x06002E85 RID: 11909 RVA: 0x001380EC File Offset: 0x001362EC
		protected override bool MustRetrieveValue()
		{
			return base.MustRetrieveValue() || LocalizedString.MustRetrieveValue(this.primary) || LocalizedString.MustRetrieveValue(this.fallback) || this.PrimaryIsLoaded != this.primary.Loaded;
		}

		// Token: 0x17000D99 RID: 3481
		// (get) Token: 0x06002E86 RID: 11910 RVA: 0x00138128 File Offset: 0x00136328
		public override bool Loaded
		{
			get
			{
				return this.primary.Loaded || this.fallback.Loaded;
			}
		}

		// Token: 0x06002E87 RID: 11911 RVA: 0x00138144 File Offset: 0x00136344
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

		// Token: 0x06002E88 RID: 11912 RVA: 0x001381B8 File Offset: 0x001363B8
		public LocalizedString GetLastFallback()
		{
			FallbackLString innerFallback = this.fallback as FallbackLString;
			if (innerFallback != null)
			{
				return innerFallback.GetLastFallback();
			}
			return this.fallback;
		}

		// Token: 0x0400175D RID: 5981
		private readonly LocalizedString primary;

		// Token: 0x0400175E RID: 5982
		private readonly LocalizedString fallback;

		// Token: 0x04001760 RID: 5984
		private readonly bool useDefaultLanguageIfFound;
	}
}
