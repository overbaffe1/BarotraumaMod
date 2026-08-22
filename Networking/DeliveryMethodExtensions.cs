using System;
using Lidgren.Network;
using Steamworks;

namespace Barotrauma.Networking
{
	// Token: 0x020004B4 RID: 1204
	internal static class DeliveryMethodExtensions
	{
		// Token: 0x06004F60 RID: 20320 RVA: 0x002AE900 File Offset: 0x002ACB00
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

		// Token: 0x06004F61 RID: 20321 RVA: 0x002AE924 File Offset: 0x002ACB24
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
