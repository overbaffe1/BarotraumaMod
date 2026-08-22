using System;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020002ED RID: 749
	internal static class QualityExtensions
	{
		// Token: 0x06003DB9 RID: 15801 RVA: 0x002307CC File Offset: 0x0022E9CC
		public static void SetValue(this Quality quality, Quality.StatType statType, float value)
		{
			quality.statValues[statType] = value;
		}
	}
}
