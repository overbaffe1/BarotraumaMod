using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200013F RID: 319
	[NullableContext(1)]
	[Nullable(0)]
	public static class SteamIcons
	{
		// Token: 0x06002996 RID: 10646 RVA: 0x001CE108 File Offset: 0x001CC308
		public static string GetNumberIcon(int number)
		{
			bool flag = number < 0 || number > 99;
			if (flag)
			{
				throw new ArgumentOutOfRangeException("number", "Number must be between 0 and 99");
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
			defaultInterpolatedStringHandler.AppendLiteral("steam_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(number);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x0400157B RID: 5499
		public const string Marker = "steam_marker";

		// Token: 0x0400157C RID: 5500
		public const string Achievement = "steam_achievement";

		// Token: 0x0400157D RID: 5501
		public const string Attack = "steam_attack";

		// Token: 0x0400157E RID: 5502
		public const string Bolt = "steam_bolt";

		// Token: 0x0400157F RID: 5503
		public const string Bookmark = "steam_bookmark";

		// Token: 0x04001580 RID: 5504
		public const string Bug = "steam_bug";

		// Token: 0x04001581 RID: 5505
		public const string Cart = "steam_cart";

		// Token: 0x04001582 RID: 5506
		public const string Caution = "steam_caution";

		// Token: 0x04001583 RID: 5507
		public const string Chat = "steam_chat";

		// Token: 0x04001584 RID: 5508
		public const string Checkmark = "steam_checkmark";

		// Token: 0x04001585 RID: 5509
		public const string Chest = "steam_chest";

		// Token: 0x04001586 RID: 5510
		public const string Circle = "steam_circle";

		// Token: 0x04001587 RID: 5511
		public const string Combat = "steam_combat";

		// Token: 0x04001588 RID: 5512
		public const string Completed = "steam_completed";

		// Token: 0x04001589 RID: 5513
		public const string Crown = "steam_crown";

		// Token: 0x0400158A RID: 5514
		public const string Death = "steam_death";

		// Token: 0x0400158B RID: 5515
		public const string Defend = "steam_defend";

		// Token: 0x0400158C RID: 5516
		public const string Diamond = "steam_diamond";

		// Token: 0x0400158D RID: 5517
		public const string Edit = "steam_edit";

		// Token: 0x0400158E RID: 5518
		public const string Effect = "steam_effect";

		// Token: 0x0400158F RID: 5519
		public const string Explosion = "steam_explosion";

		// Token: 0x04001590 RID: 5520
		public const string Fix = "steam_fix";

		// Token: 0x04001591 RID: 5521
		public const string Flag = "steam_flag";

		// Token: 0x04001592 RID: 5522
		public const string Gem = "steam_gem";

		// Token: 0x04001593 RID: 5523
		public const string Group = "steam_group";

		// Token: 0x04001594 RID: 5524
		public const string Heart = "steam_heart";

		// Token: 0x04001595 RID: 5525
		public const string Info = "steam_info";

		// Token: 0x04001596 RID: 5526
		public const string Invalid = "steam_invalid";

		// Token: 0x04001597 RID: 5527
		public const string Minus = "steam_minus";

		// Token: 0x04001598 RID: 5528
		public const string Pair = "steam_pair";

		// Token: 0x04001599 RID: 5529
		public const string Person = "steam_person";

		// Token: 0x0400159A RID: 5530
		public const string Plus = "steam_plus";

		// Token: 0x0400159B RID: 5531
		public const string Purchase = "steam_purchase";

		// Token: 0x0400159C RID: 5532
		public const string Question = "steam_question";

		// Token: 0x0400159D RID: 5533
		public const string Ribbon = "steam_ribbon";

		// Token: 0x0400159E RID: 5534
		public const string Screenshot = "steam_screenshot";

		// Token: 0x0400159F RID: 5535
		public const string Scroll = "steam_scroll";

		// Token: 0x040015A0 RID: 5536
		public const string Square = "steam_square";

		// Token: 0x040015A1 RID: 5537
		public const string Star = "steam_star";

		// Token: 0x040015A2 RID: 5538
		public const string Starburst = "steam_starburst";

		// Token: 0x040015A3 RID: 5539
		public const string Timer = "steam_timer";

		// Token: 0x040015A4 RID: 5540
		public const string Transfer = "steam_transfer";

		// Token: 0x040015A5 RID: 5541
		public const string Triangle = "steam_triangle";

		// Token: 0x040015A6 RID: 5542
		public const string Trophy = "steam_trophy";

		// Token: 0x040015A7 RID: 5543
		public const string View = "steam_view";

		// Token: 0x040015A8 RID: 5544
		public const string X = "steam_x";

		// Token: 0x040015A9 RID: 5545
		public const string Zero = "steam_0";

		// Token: 0x040015AA RID: 5546
		public const string One = "steam_1";

		// Token: 0x040015AB RID: 5547
		public const string Two = "steam_2";

		// Token: 0x040015AC RID: 5548
		public const string Three = "steam_3";

		// Token: 0x040015AD RID: 5549
		public const string Four = "steam_4";

		// Token: 0x040015AE RID: 5550
		public const string Five = "steam_5";

		// Token: 0x040015AF RID: 5551
		public const string Six = "steam_6";

		// Token: 0x040015B0 RID: 5552
		public const string Seven = "steam_7";

		// Token: 0x040015B1 RID: 5553
		public const string Eight = "steam_8";

		// Token: 0x040015B2 RID: 5554
		public const string Nine = "steam_9";

		// Token: 0x040015B3 RID: 5555
		public const string Ten = "steam_10";
	}
}
