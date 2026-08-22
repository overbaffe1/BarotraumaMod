using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000252 RID: 594
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SteamWorkshopId : ContentPackageId
	{
		// Token: 0x060037BA RID: 14266 RVA: 0x0021626E File Offset: 0x0021446E
		public SteamWorkshopId(ulong value)
		{
			this.Value = value;
		}

		// Token: 0x17000EAA RID: 3754
		// (get) Token: 0x060037BB RID: 14267 RVA: 0x0021627D File Offset: 0x0021447D
		public override string StringRepresentation
		{
			get
			{
				return this.Value.ToString(CultureInfo.InvariantCulture);
			}
		}

		// Token: 0x060037BC RID: 14268 RVA: 0x00216290 File Offset: 0x00214490
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			SteamWorkshopId otherWorkshopId = obj as SteamWorkshopId;
			return otherWorkshopId != null && otherWorkshopId.Value == this.Value;
		}

		// Token: 0x060037BD RID: 14269 RVA: 0x002162B7 File Offset: 0x002144B7
		public override int GetHashCode()
		{
			return this.Value.GetHashCode();
		}

		// Token: 0x060037BE RID: 14270 RVA: 0x002162C4 File Offset: 0x002144C4
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

		// Token: 0x04001C23 RID: 7203
		public readonly ulong Value;

		// Token: 0x04001C24 RID: 7204
		private const string Prefix = "STEAM_WORKSHOP_";
	}
}
