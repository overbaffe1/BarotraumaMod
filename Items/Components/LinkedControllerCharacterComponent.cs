using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005F4 RID: 1524
	[NullableContext(2)]
	[Nullable(0)]
	internal class LinkedControllerCharacterComponent : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x17001932 RID: 6450
		// (get) Token: 0x06006395 RID: 25493 RVA: 0x0033DF54 File Offset: 0x0033C154
		// (set) Token: 0x06006396 RID: 25494 RVA: 0x0033DF5C File Offset: 0x0033C15C
		[Serialize(0.5f, IsPropertySaveable.No, "Maximum value which DeconstructTimeMultiplier can be.", "", false)]
		public float MaxDeconstructTimeMultiplier { get; set; }

		// Token: 0x17001933 RID: 6451
		// (get) Token: 0x06006397 RID: 25495 RVA: 0x0033DF65 File Offset: 0x0033C165
		// (set) Token: 0x06006398 RID: 25496 RVA: 0x0033DF6D File Offset: 0x0033C16D
		[Serialize(true, IsPropertySaveable.No, "Should this item be removed if the linked character is null?", "", false)]
		public bool RemoveItemIfCharacterNull { get; set; }

		// Token: 0x17001934 RID: 6452
		// (get) Token: 0x06006399 RID: 25497 RVA: 0x0033DF76 File Offset: 0x0033C176
		// (set) Token: 0x0600639A RID: 25498 RVA: 0x0033DF7E File Offset: 0x0033C17E
		public Character Character { get; private set; }

		// Token: 0x17001935 RID: 6453
		// (get) Token: 0x0600639B RID: 25499 RVA: 0x0033DF87 File Offset: 0x0033C187
		public bool DoesBleed
		{
			get
			{
				Character character = this.Character;
				return character != null && character.DoesBleed;
			}
		}

		// Token: 0x17001936 RID: 6454
		// (get) Token: 0x0600639C RID: 25500 RVA: 0x0033DF9A File Offset: 0x0033C19A
		// (set) Token: 0x0600639D RID: 25501 RVA: 0x0033DFA2 File Offset: 0x0033C1A2
		public float DeconstructTimeMultiplier { get; private set; } = 1f;

		// Token: 0x0600639E RID: 25502 RVA: 0x0033DFAC File Offset: 0x0033C1AC
		[NullableContext(1)]
		public LinkedControllerCharacterComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.spriteOverrides = (from e in element.Elements()
			where e.Name.LocalName.ToLowerInvariant() == "spriteoverride"
			select new LinkedControllerCharacterComponent.SpriteOverride(e)).ToImmutableArray<LinkedControllerCharacterComponent.SpriteOverride>();
		}

		// Token: 0x0600639F RID: 25503 RVA: 0x0033E02C File Offset: 0x0033C22C
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

		// Token: 0x060063A0 RID: 25504 RVA: 0x0033E084 File Offset: 0x0033C284
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
			if (character != null)
			{
				LinkedControllerCharacterComponent.SpriteOverride spriteOverride = this.spriteOverrides.Where(delegate(LinkedControllerCharacterComponent.SpriteOverride s)
				{
					Identifier speciesName = character.SpeciesName;
					return s.SpeciesName == speciesName;
				}).FirstOrDefault<LinkedControllerCharacterComponent.SpriteOverride>() ?? this.spriteOverrides.Where(delegate(LinkedControllerCharacterComponent.SpriteOverride s)
				{
					Identifier group = character.Group;
					return s.SpeciesGroup == group;
				}).FirstOrDefault<LinkedControllerCharacterComponent.SpriteOverride>();
				if (spriteOverride != null)
				{
					this.item.OverrideInventorySprite = spriteOverride.Sprite;
					return;
				}
			}
			else
			{
				this.item.OverrideInventorySprite = null;
			}
		}

		// Token: 0x060063A1 RID: 25505 RVA: 0x0033E178 File Offset: 0x0033C378
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

		// Token: 0x060063A2 RID: 25506 RVA: 0x0033E1AD File Offset: 0x0033C3AD
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

		// Token: 0x040033A4 RID: 13220
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<LinkedControllerCharacterComponent.SpriteOverride> spriteOverrides;

		// Token: 0x02001497 RID: 5271
		[NullableContext(0)]
		private class SpriteOverride
		{
			// Token: 0x06009B71 RID: 39793 RVA: 0x003E54C4 File Offset: 0x003E36C4
			[NullableContext(1)]
			public SpriteOverride(ContentXElement element)
			{
				ContentXElement spriteElement = element.GetChildElement("Sprite");
				if (spriteElement != null)
				{
					this.Sprite = new Sprite(spriteElement, "", "", false, 1f);
				}
				this.SpeciesName = element.GetAttributeIdentifier("speciesname", Identifier.Empty);
				this.SpeciesGroup = element.GetAttributeIdentifier("speciesgroup", Identifier.Empty);
			}

			// Token: 0x0400664D RID: 26189
			[Nullable(2)]
			public readonly Sprite Sprite;

			// Token: 0x0400664E RID: 26190
			public readonly Identifier SpeciesName;

			// Token: 0x0400664F RID: 26191
			public readonly Identifier SpeciesGroup;
		}
	}
}
