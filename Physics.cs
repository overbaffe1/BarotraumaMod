using System;
using FarseerPhysics.Dynamics;

namespace Barotrauma
{
	// Token: 0x02000341 RID: 833
	internal static class Physics
	{
		// Token: 0x060041BC RID: 16828 RVA: 0x00247018 File Offset: 0x00245218
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

		// Token: 0x0400223B RID: 8763
		public const Category CollisionNone = Category.None;

		// Token: 0x0400223C RID: 8764
		public const Category CollisionAll = Category.All;

		// Token: 0x0400223D RID: 8765
		public const Category CollisionWall = Category.Cat1;

		// Token: 0x0400223E RID: 8766
		public const Category CollisionCharacter = Category.Cat2;

		// Token: 0x0400223F RID: 8767
		public const Category CollisionPlatform = Category.Cat3;

		// Token: 0x04002240 RID: 8768
		public const Category CollisionStairs = Category.Cat4;

		// Token: 0x04002241 RID: 8769
		public const Category CollisionItem = Category.Cat5;

		// Token: 0x04002242 RID: 8770
		public const Category CollisionItemBlocking = Category.Cat6;

		// Token: 0x04002243 RID: 8771
		public const Category CollisionProjectile = Category.Cat7;

		// Token: 0x04002244 RID: 8772
		public const Category CollisionLevel = Category.Cat8;

		// Token: 0x04002245 RID: 8773
		public const Category CollisionRepairableWall = Category.Cat9;

		// Token: 0x04002246 RID: 8774
		public const Category CollisionLagCompensationBody = Category.Cat10;

		// Token: 0x04002247 RID: 8775
		public const Category DefaultItemCollidesWith = Category.Cat1 | Category.Cat3 | Category.Cat8 | Category.Cat9;

		// Token: 0x04002248 RID: 8776
		public static float DisplayToRealWorldRatio = 0.01f;

		// Token: 0x04002249 RID: 8777
		public const float DisplayToSimRation = 100f;

		// Token: 0x0400224A RID: 8778
		public const float NeutralDensity = 10f;
	}
}
