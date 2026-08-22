using System;
using FarseerPhysics.Dynamics;

namespace Barotrauma
{
	// Token: 0x0200026E RID: 622
	internal static class Physics
	{
		// Token: 0x06002C9D RID: 11421 RVA: 0x00126CB4 File Offset: 0x00124EB4
		public static bool TryParseCollisionCategory(string categoryName, out Category category)
		{
			category = Category.None;
			if (string.IsNullOrEmpty(categoryName))
			{
				return false;
			}
			string text = categoryName.ToLowerInvariant();
			if (text != null)
			{
				switch (text.Length)
				{
				case 3:
					if (!(text == "all"))
					{
						return false;
					}
					category = Category.All;
					return true;
				case 4:
				{
					char c = text[0];
					if (c != 'i')
					{
						if (c != 'w')
						{
							return false;
						}
						if (!(text == "wall"))
						{
							return false;
						}
					}
					else
					{
						if (!(text == "item"))
						{
							return false;
						}
						category = Category.Cat5;
						return true;
					}
					break;
				}
				case 5:
					if (!(text == "level"))
					{
						return false;
					}
					category = Category.Cat8;
					return true;
				case 6:
				{
					char c = text[0];
					if (c != 'r')
					{
						if (c != 's')
						{
							return false;
						}
						if (!(text == "stairs"))
						{
							return false;
						}
						category = Category.Cat4;
						return true;
					}
					else
					{
						if (!(text == "repair"))
						{
							return false;
						}
						category = Category.Cat9;
						return true;
					}
					break;
				}
				case 7:
				case 11:
					return false;
				case 8:
					if (!(text == "platform"))
					{
						return false;
					}
					category = Category.Cat3;
					return true;
				case 9:
				{
					char c = text[0];
					if (c != 'c')
					{
						if (c != 's')
						{
							return false;
						}
						if (!(text == "structure"))
						{
							return false;
						}
					}
					else
					{
						if (!(text == "character"))
						{
							return false;
						}
						category = Category.Cat2;
						return true;
					}
					break;
				}
				case 10:
					if (!(text == "projectile"))
					{
						return false;
					}
					category = Category.Cat7;
					return true;
				case 12:
					if (!(text == "itemblocking"))
					{
						return false;
					}
					category = Category.Cat6;
					return true;
				default:
					return false;
				}
				category = Category.Cat1;
				return true;
			}
			return false;
		}

		// Token: 0x040015F3 RID: 5619
		public const Category CollisionNone = Category.None;

		// Token: 0x040015F4 RID: 5620
		public const Category CollisionAll = Category.All;

		// Token: 0x040015F5 RID: 5621
		public const Category CollisionWall = Category.Cat1;

		// Token: 0x040015F6 RID: 5622
		public const Category CollisionCharacter = Category.Cat2;

		// Token: 0x040015F7 RID: 5623
		public const Category CollisionPlatform = Category.Cat3;

		// Token: 0x040015F8 RID: 5624
		public const Category CollisionStairs = Category.Cat4;

		// Token: 0x040015F9 RID: 5625
		public const Category CollisionItem = Category.Cat5;

		// Token: 0x040015FA RID: 5626
		public const Category CollisionItemBlocking = Category.Cat6;

		// Token: 0x040015FB RID: 5627
		public const Category CollisionProjectile = Category.Cat7;

		// Token: 0x040015FC RID: 5628
		public const Category CollisionLevel = Category.Cat8;

		// Token: 0x040015FD RID: 5629
		public const Category CollisionRepairableWall = Category.Cat9;

		// Token: 0x040015FE RID: 5630
		public const Category CollisionLagCompensationBody = Category.Cat10;

		// Token: 0x040015FF RID: 5631
		public const Category DefaultItemCollidesWith = Category.Cat1 | Category.Cat3 | Category.Cat8 | Category.Cat9;

		// Token: 0x04001600 RID: 5632
		public static float DisplayToRealWorldRatio = 0.01f;

		// Token: 0x04001601 RID: 5633
		public const float DisplayToSimRation = 100f;

		// Token: 0x04001602 RID: 5634
		public const float NeutralDensity = 10f;
	}
}
