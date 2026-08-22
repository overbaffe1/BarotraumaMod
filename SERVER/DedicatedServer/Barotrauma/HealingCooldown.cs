using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000014 RID: 20
	[NullableContext(1)]
	[Nullable(0)]
	internal static class HealingCooldown
	{
		// Token: 0x060002B6 RID: 694 RVA: 0x00015DD5 File Offset: 0x00013FD5
		public static bool IsOnCooldown(Client client)
		{
			HealingCooldown.RemoveExpiredCooldowns();
			return HealingCooldown.HealingCooldowns.ContainsKey(client);
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x00015DE8 File Offset: 0x00013FE8
		public static void SetCooldown(Client client)
		{
			HealingCooldown.RemoveExpiredCooldowns();
			DateTimeOffset newCooldown = DateTimeOffset.UtcNow.AddSeconds(0.4000000059604645);
			HealingCooldown.HealingCooldowns[client] = newCooldown;
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x00015E20 File Offset: 0x00014020
		private static void RemoveExpiredCooldowns()
		{
			HashSet<Client> expiredCooldowns = null;
			DateTimeOffset now = DateTimeOffset.UtcNow;
			foreach (KeyValuePair<Client, DateTimeOffset> keyValuePair in HealingCooldown.HealingCooldowns)
			{
				Client client2;
				DateTimeOffset dateTimeOffset;
				keyValuePair.Deconstruct(out client2, out dateTimeOffset);
				Client client = client2;
				DateTimeOffset cooldown = dateTimeOffset;
				if (!(now < cooldown))
				{
					if (expiredCooldowns == null)
					{
						expiredCooldowns = new HashSet<Client>();
					}
					expiredCooldowns.Add(client);
				}
			}
			if (expiredCooldowns == null)
			{
				return;
			}
			foreach (Client expiredCooldown in expiredCooldowns)
			{
				HealingCooldown.HealingCooldowns.Remove(expiredCooldown);
			}
		}

		// Token: 0x0400014A RID: 330
		private static readonly Dictionary<Client, DateTimeOffset> HealingCooldowns = new Dictionary<Client, DateTimeOffset>();

		// Token: 0x0400014B RID: 331
		private const float CooldownDuration = 0.4f;
	}
}
