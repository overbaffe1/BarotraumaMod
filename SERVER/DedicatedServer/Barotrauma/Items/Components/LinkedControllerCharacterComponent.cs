using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004C7 RID: 1223
	[NullableContext(2)]
	[Nullable(0)]
	internal class LinkedControllerCharacterComponent : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x170012BE RID: 4798
		// (get) Token: 0x060045EF RID: 17903 RVA: 0x001BFEDB File Offset: 0x001BE0DB
		// (set) Token: 0x060045F0 RID: 17904 RVA: 0x001BFEE3 File Offset: 0x001BE0E3
		[Serialize(0.5f, IsPropertySaveable.No, "Maximum value which DeconstructTimeMultiplier can be.", "", false)]
		public float MaxDeconstructTimeMultiplier { get; set; }

		// Token: 0x170012BF RID: 4799
		// (get) Token: 0x060045F1 RID: 17905 RVA: 0x001BFEEC File Offset: 0x001BE0EC
		// (set) Token: 0x060045F2 RID: 17906 RVA: 0x001BFEF4 File Offset: 0x001BE0F4
		[Serialize(true, IsPropertySaveable.No, "Should this item be removed if the linked character is null?", "", false)]
		public bool RemoveItemIfCharacterNull { get; set; }

		// Token: 0x170012C0 RID: 4800
		// (get) Token: 0x060045F3 RID: 17907 RVA: 0x001BFEFD File Offset: 0x001BE0FD
		// (set) Token: 0x060045F4 RID: 17908 RVA: 0x001BFF05 File Offset: 0x001BE105
		public Character Character { get; private set; }

		// Token: 0x170012C1 RID: 4801
		// (get) Token: 0x060045F5 RID: 17909 RVA: 0x001BFF0E File Offset: 0x001BE10E
		public bool DoesBleed
		{
			get
			{
				Character character = this.Character;
				return character != null && character.DoesBleed;
			}
		}

		// Token: 0x170012C2 RID: 4802
		// (get) Token: 0x060045F6 RID: 17910 RVA: 0x001BFF21 File Offset: 0x001BE121
		// (set) Token: 0x060045F7 RID: 17911 RVA: 0x001BFF29 File Offset: 0x001BE129
		public float DeconstructTimeMultiplier { get; private set; } = 1f;

		// Token: 0x060045F8 RID: 17912 RVA: 0x001BFF32 File Offset: 0x001BE132
		[NullableContext(1)]
		public LinkedControllerCharacterComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x060045F9 RID: 17913 RVA: 0x001BFF50 File Offset: 0x001BE150
		[NullableContext(1)]
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			if (this.RemoveItemIfCharacterNull)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if ((networkMember == null || !networkMember.IsClient) && (this.Character == null || this.Character.Removed))
				{
					EntitySpawner spawner = Entity.Spawner;
					if (spawner == null)
					{
						return;
					}
					spawner.AddEntityToRemoveQueue(base.Item);
				}
			}
		}

		// Token: 0x060045FA RID: 17914 RVA: 0x001BFFA8 File Offset: 0x001BE1A8
		public void UpdateLinkedCharacter(Character character)
		{
			this.Character = character;
			if (character != null)
			{
				AnimController animController = character.AnimController;
				float totalLimbs = (float)animController.Limbs.Length;
				float nonSeveredLimbs = (float)animController.Limbs.Count((Limb l) => !l.IsSevered);
				this.DeconstructTimeMultiplier *= MathF.Max(this.MaxDeconstructTimeMultiplier, nonSeveredLimbs / totalLimbs);
			}
			base.Item.CreateServerEvent<LinkedControllerCharacterComponent>(this);
		}

		// Token: 0x060045FB RID: 17915 RVA: 0x001C0024 File Offset: 0x001BE224
		[NullableContext(1)]
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			ushort characterId = msg.ReadUInt16();
			if (characterId == 0)
			{
				this.UpdateLinkedCharacter(null);
				return;
			}
			Character character = Entity.FindEntityByID(characterId) as Character;
			if (character != null)
			{
				this.UpdateLinkedCharacter(character);
			}
		}

		// Token: 0x060045FC RID: 17916 RVA: 0x001C0059 File Offset: 0x001BE259
		[NullableContext(1)]
		public void ServerEventWrite(IWriteMessage msg, Client c, [Nullable(2)] NetEntityEvent.IData extraData = null)
		{
			if (this.Character != null)
			{
				msg.WriteUInt16(this.Character.ID);
				return;
			}
			msg.WriteUInt16(0);
		}
	}
}
