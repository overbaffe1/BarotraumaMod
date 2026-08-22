using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000C0 RID: 192
	internal class CharacterStateInfo : PosInfo
	{
		// Token: 0x060015B3 RID: 5555 RVA: 0x000B9778 File Offset: 0x000B7978
		public CharacterStateInfo(Vector2 pos, float? rotation, Vector2 velocity, float? angularVelocity, float time, Direction dir, Character selectedCharacter, Item selectedItem, Item selectedSecondaryItem, Vector2 targetMovement, AnimController.Animation animation = AnimController.Animation.None, bool ignorePlatforms = false) : this(pos, rotation, velocity, angularVelocity, 0, time, dir, selectedCharacter, selectedItem, selectedSecondaryItem, targetMovement, animation, ignorePlatforms)
		{
		}

		// Token: 0x060015B4 RID: 5556 RVA: 0x000B97A4 File Offset: 0x000B79A4
		public CharacterStateInfo(Vector2 pos, float? rotation, ushort ID, Direction dir, Character selectedCharacter, Item selectedItem, Item selectedSecondaryItem, Vector2 targetMovement, AnimController.Animation animation = AnimController.Animation.None, bool ignorePlatforms = false) : this(pos, rotation, Vector2.Zero, new float?(0f), ID, 0f, dir, selectedCharacter, selectedItem, selectedSecondaryItem, targetMovement, animation, ignorePlatforms)
		{
		}

		// Token: 0x060015B5 RID: 5557 RVA: 0x000B97DC File Offset: 0x000B79DC
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

		// Token: 0x04000A51 RID: 2641
		public readonly Direction Direction;

		// Token: 0x04000A52 RID: 2642
		public readonly Character SelectedCharacter;

		// Token: 0x04000A53 RID: 2643
		public readonly Item SelectedItem;

		// Token: 0x04000A54 RID: 2644
		public readonly Item SelectedSecondaryItem;

		// Token: 0x04000A55 RID: 2645
		public readonly AnimController.Animation Animation;

		// Token: 0x04000A56 RID: 2646
		public bool IgnorePlatforms;

		// Token: 0x04000A57 RID: 2647
		public readonly Vector2 TargetMovement;
	}
}
