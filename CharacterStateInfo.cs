using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001C1 RID: 449
	internal class CharacterStateInfo : PosInfo
	{
		// Token: 0x06003173 RID: 12659 RVA: 0x00204580 File Offset: 0x00202780
		public CharacterStateInfo(Vector2 pos, float? rotation, Vector2 velocity, float? angularVelocity, float time, Direction dir, Character selectedCharacter, Item selectedItem, Item selectedSecondaryItem, Vector2 targetMovement, AnimController.Animation animation = AnimController.Animation.None, bool ignorePlatforms = false) : this(pos, rotation, velocity, angularVelocity, 0, time, dir, selectedCharacter, selectedItem, selectedSecondaryItem, targetMovement, animation, ignorePlatforms)
		{
		}

		// Token: 0x06003174 RID: 12660 RVA: 0x002045AC File Offset: 0x002027AC
		public CharacterStateInfo(Vector2 pos, float? rotation, ushort ID, Direction dir, Character selectedCharacter, Item selectedItem, Item selectedSecondaryItem, Vector2 targetMovement, AnimController.Animation animation = AnimController.Animation.None, bool ignorePlatforms = false) : this(pos, rotation, Vector2.Zero, new float?(0f), ID, 0f, dir, selectedCharacter, selectedItem, selectedSecondaryItem, targetMovement, animation, ignorePlatforms)
		{
		}

		// Token: 0x06003175 RID: 12661 RVA: 0x002045E4 File Offset: 0x002027E4
		protected CharacterStateInfo(Vector2 pos, float? rotation, Vector2 velocity, float? angularVelocity, ushort ID, float time, Direction dir, Character selectedCharacter, Item selectedItem, Item selectedSecondaryItem, Vector2 targetMovement, AnimController.Animation animation = AnimController.Animation.None, bool ignorePlatforms = false) : base(pos, rotation, velocity, angularVelocity, ID, time)
		{
			this.Direction = dir;
			this.SelectedCharacter = selectedCharacter;
			this.SelectedItem = selectedItem;
			this.SelectedSecondaryItem = selectedSecondaryItem;
			this.IgnorePlatforms = ignorePlatforms;
			this.TargetMovement = targetMovement;
			this.Animation = animation;
		}

		// Token: 0x040019BC RID: 6588
		public readonly Direction Direction;

		// Token: 0x040019BD RID: 6589
		public readonly Character SelectedCharacter;

		// Token: 0x040019BE RID: 6590
		public readonly Item SelectedItem;

		// Token: 0x040019BF RID: 6591
		public readonly Item SelectedSecondaryItem;

		// Token: 0x040019C0 RID: 6592
		public readonly AnimController.Animation Animation;

		// Token: 0x040019C1 RID: 6593
		public bool IgnorePlatforms;

		// Token: 0x040019C2 RID: 6594
		public readonly Vector2 TargetMovement;
	}
}
