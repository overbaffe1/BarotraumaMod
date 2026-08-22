using System;
using Lidgren.Network;
using Steamworks;

namespace Barotrauma.Networking
{
	// Token: 0x020003B7 RID: 951
	internal static class DeliveryMethodExtensions
	{
		// Token: 0x0600378B RID: 14219 RVA: 0x00175DC4 File Offset: 0x00173FC4
		public static NetDeliveryMethod ToLidgren(this DeliveryMethod deliveryMethod)
		{
			NetDeliveryMethod result;
			if (deliveryMethod != DeliveryMethod.Unreliable)
			{
				if (deliveryMethod != DeliveryMethod.Reliable)
				{
					result = NetDeliveryMethod.Unreliable;
				}
				else
				{
					result = NetDeliveryMethod.ReliableOrdered;
				}
			}
			else
			{
				result = NetDeliveryMethod.Unreliable;
			}
			return result;
		}

		// Token: 0x0600378C RID: 14220 RVA: 0x00175DE8 File Offset: 0x00173FE8
		public static P2PSend ToSteam(this DeliveryMethod deliveryMethod)
		{
			P2PSend result;
			if (deliveryMethod == DeliveryMethod.Reliable)
			{
				result = P2PSend.Reliable;
			}
			else
			{
				result = P2PSend.Unreliable;
			}
			return result;
		}
	}
}
