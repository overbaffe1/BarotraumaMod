using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000132 RID: 306
	[TagNames(new string[]
	{
		"damagesound"
	})]
	internal class DamageSound : SoundPrefab
	{
		// Token: 0x0600288D RID: 10381 RVA: 0x001C3A2C File Offset: 0x001C1C2C
		public DamageSound(ContentXElement element, SoundsFile file) : base(element, file, false)
		{
			string key = "damagerange";
			Vector2 zero = Vector2.Zero;
			this.DamageRange = element.GetAttributeVector2(key, zero);
			this.DamageType = element.GetAttributeIdentifier("damagesoundtype", "None");
			this.IgnoreMuffling = element.GetAttributeBool("ignoremuffling", false);
			this.RequiredTag = element.GetAttributeIdentifier("requiredtag", "");
		}

		// Token: 0x040014AD RID: 5293
		public static readonly PrefabCollection<DamageSound> DamageSoundPrefabs = new PrefabCollection<DamageSound>();

		// Token: 0x040014AE RID: 5294
		public readonly Vector2 DamageRange;

		// Token: 0x040014AF RID: 5295
		public readonly Identifier DamageType;

		// Token: 0x040014B0 RID: 5296
		public readonly Identifier RequiredTag;

		// Token: 0x040014B1 RID: 5297
		public bool IgnoreMuffling;
	}
}
