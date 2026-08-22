using System;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000204 RID: 516
	internal static class QualityExtensions
	{
		// Token: 0x060024E1 RID: 9441 RVA: 0x000F333E File Offset: 0x000F153E
		public static void SetValue(this Quality quality, Quality.StatType statType, float value)
		{
			quality.statValues[statType] = value;
		}
	}
}
