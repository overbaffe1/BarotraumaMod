using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200015C RID: 348
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SteamWorkshopId : ContentPackageId
	{
		// Token: 0x06001CC9 RID: 7369 RVA: 0x000D0302 File Offset: 0x000CE502
		public SteamWorkshopId(ulong value)
		{
			this.Value = value;
		}

		// Token: 0x17000847 RID: 2119
		// (get) Token: 0x06001CCA RID: 7370 RVA: 0x000D0311 File Offset: 0x000CE511
		public override string StringRepresentation
		{
			get
			{
				return this.Value.ToString(CultureInfo.InvariantCulture);
			}
		}

		// Token: 0x06001CCB RID: 7371 RVA: 0x000D0324 File Offset: 0x000CE524
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			SteamWorkshopId otherWorkshopId = obj as SteamWorkshopId;
			return otherWorkshopId != null && otherWorkshopId.Value == this.Value;
		}

		// Token: 0x06001CCC RID: 7372 RVA: 0x000D034B File Offset: 0x000CE54B
		public override int GetHashCode()
		{
			return this.Value.GetHashCode();
		}

		// Token: 0x06001CCD RID: 7373 RVA: 0x000D0358 File Offset: 0x000CE558
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public new static Option<SteamWorkshopId> Parse(string s)
		{
			if (s.StartsWith("STEAM_WORKSHOP_"))
			{
				s = s.Substring("STEAM_WORKSHOP_".Length);
			}
			ulong id;
			if (!ulong.TryParse(s, out id) || id == 0UL)
			{
				return Option<SteamWorkshopId>.None();
			}
			return Option<SteamWorkshopId>.Some(new SteamWorkshopId(id));
		}

		// Token: 0x04000D16 RID: 3350
		public readonly ulong Value;

		// Token: 0x04000D17 RID: 3351
		private const string Prefix = "STEAM_WORKSHOP_";
	}
}
