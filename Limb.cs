using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.Particles;
using Barotrauma.SpriteDeformations;
using Barotrauma.Utils;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000033 RID: 51
	internal class Limb : ISerializableEntity, ISpatialEntity
	{
		// Token: 0x1700024A RID: 586
		// (get) Token: 0x0600082E RID: 2094 RVA: 0x00049E48 File Offset: 0x00048048
		// (set) Token: 0x0600082F RID: 2095 RVA: 0x00049E50 File Offset: 0x00048050
		private List<SpriteDeformation> Deformations { get; set; } = new List<SpriteDeformation>();

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x06000830 RID: 2096 RVA: 0x00049E59 File Offset: 0x00048059
		// (set) Token: 0x06000831 RID: 2097 RVA: 0x00049E61 File Offset: 0x00048061
		private List<SpriteDeformation> NonConditionalDeformations { get; set; } = new List<SpriteDeformation>();

		// Token: 0x1700024C RID: 588
		// (get) Token: 0x06000832 RID: 2098 RVA: 0x00049E6A File Offset: 0x0004806A
		// (set) Token: 0x06000833 RID: 2099 RVA: 0x00049E72 File Offset: 0x00048072
		private List<ValueTuple<ConditionalSprite, IEnumerable<SpriteDeformation>>> ConditionalDeformations { get; set; } = new List<ValueTuple<ConditionalSprite, IEnumerable<SpriteDeformation>>>();

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x06000834 RID: 2100 RVA: 0x00049E7B File Offset: 0x0004807B
		// (set) Token: 0x06000835 RID: 2101 RVA: 0x00049E83 File Offset: 0x00048083
		public List<SpriteDeformation> ActiveDeformations { get; set; } = new List<SpriteDeformation>();

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x06000836 RID: 2102 RVA: 0x00049E8C File Offset: 0x0004808C
		// (set) Token: 0x06000837 RID: 2103 RVA: 0x00049E94 File Offset: 0x00048094
		public Sprite Sprite { get; protected set; }

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x06000838 RID: 2104 RVA: 0x00049E9D File Offset: 0x0004809D
		// (set) Token: 0x06000839 RID: 2105 RVA: 0x00049EA5 File Offset: 0x000480A5
		public Sprite TintMask { get; protected set; }

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x0600083A RID: 2106 RVA: 0x00049EAE File Offset: 0x000480AE
		// (set) Token: 0x0600083B RID: 2107 RVA: 0x00049EB6 File Offset: 0x000480B6
		public Sprite HuskMask { get; protected set; }

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x0600083C RID: 2108 RVA: 0x00049EBF File Offset: 0x000480BF
		// (set) Token: 0x0600083D RID: 2109 RVA: 0x00049EC7 File Offset: 0x000480C7
		public float TintHighlightThreshold { get; protected set; }

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x0600083E RID: 2110 RVA: 0x00049ED0 File Offset: 0x000480D0
		// (set) Token: 0x0600083F RID: 2111 RVA: 0x00049ED8 File Offset: 0x000480D8
		public float TintHighlightMultiplier { get; protected set; }

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x06000840 RID: 2112 RVA: 0x00049EE4 File Offset: 0x000480E4
		public DeformableSprite DeformSprite
		{
			get
			{
				ConditionalSprite conditionalSprite = null;
				foreach (ConditionalSprite cs in this.ConditionalSprites)
				{
					if (cs.Exclusive && cs.IsActive && cs.DeformableSprite != null)
					{
						conditionalSprite = cs;
						break;
					}
				}
				if (conditionalSprite != null)
				{
					return conditionalSprite.DeformableSprite;
				}
				return this._deformSprite;
			}
		}

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x06000841 RID: 2113 RVA: 0x00049F60 File Offset: 0x00048160
		// (set) Token: 0x06000842 RID: 2114 RVA: 0x00049F68 File Offset: 0x00048168
		public List<DecorativeSprite> DecorativeSprites { get; private set; } = new List<DecorativeSprite>();

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x06000843 RID: 2115 RVA: 0x00049F74 File Offset: 0x00048174
		public Sprite ActiveSprite
		{
			get
			{
				ConditionalSprite conditionalSprite = null;
				foreach (ConditionalSprite cs in this.ConditionalSprites)
				{
					if (cs.Exclusive && cs.IsActive && cs.ActiveSprite != null)
					{
						conditionalSprite = cs;
						break;
					}
				}
				if (conditionalSprite != null)
				{
					return conditionalSprite.ActiveSprite;
				}
				if (this._deformSprite == null)
				{
					return this.Sprite;
				}
				return this._deformSprite.Sprite;
			}
		}

		// Token: 0x06000844 RID: 2116 RVA: 0x0004A004 File Offset: 0x00048204
		public Sprite GetActiveSprite(bool excludeConditionalSprites = true)
		{
			if (!excludeConditionalSprites)
			{
				return this.ActiveSprite;
			}
			if (this._deformSprite == null)
			{
				return this.Sprite;
			}
			return this._deformSprite.Sprite;
		}

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x06000845 RID: 2117 RVA: 0x0004A02A File Offset: 0x0004822A
		// (set) Token: 0x06000846 RID: 2118 RVA: 0x0004A032 File Offset: 0x00048232
		public float DefaultSpriteDepth { get; private set; }

		// Token: 0x17000257 RID: 599
		// (get) Token: 0x06000847 RID: 2119 RVA: 0x0004A03B File Offset: 0x0004823B
		// (set) Token: 0x06000848 RID: 2120 RVA: 0x0004A043 File Offset: 0x00048243
		public WearableSprite HairWithHatSprite { get; set; }

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x06000849 RID: 2121 RVA: 0x0004A04C File Offset: 0x0004824C
		// (set) Token: 0x0600084A RID: 2122 RVA: 0x0004A054 File Offset: 0x00048254
		public WearableSprite HuskSprite { get; private set; }

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x0600084B RID: 2123 RVA: 0x0004A05D File Offset: 0x0004825D
		// (set) Token: 0x0600084C RID: 2124 RVA: 0x0004A065 File Offset: 0x00048265
		public WearableSprite HerpesSprite { get; private set; }

		// Token: 0x0600084D RID: 2125 RVA: 0x0004A06E File Offset: 0x0004826E
		public void LoadHuskSprite()
		{
			this.HuskSprite = this.GetWearableSprite(WearableType.Husk);
		}

		// Token: 0x0600084E RID: 2126 RVA: 0x0004A07D File Offset: 0x0004827D
		public void LoadHerpesSprite()
		{
			this.HerpesSprite = this.GetWearableSprite(WearableType.Herpes);
		}

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x0600084F RID: 2127 RVA: 0x0004A08C File Offset: 0x0004828C
		public float TextureScale
		{
			get
			{
				return this.Params.Ragdoll.TextureScale;
			}
		}

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x06000850 RID: 2128 RVA: 0x0004A09E File Offset: 0x0004829E
		// (set) Token: 0x06000851 RID: 2129 RVA: 0x0004A0A6 File Offset: 0x000482A6
		public Sprite DamagedSprite { get; private set; }

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x06000852 RID: 2130 RVA: 0x0004A0AF File Offset: 0x000482AF
		// (set) Token: 0x06000853 RID: 2131 RVA: 0x0004A0B7 File Offset: 0x000482B7
		public List<ConditionalSprite> ConditionalSprites { get; private set; } = new List<ConditionalSprite>();

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x06000854 RID: 2132 RVA: 0x0004A0C0 File Offset: 0x000482C0
		// (set) Token: 0x06000855 RID: 2133 RVA: 0x0004A0C8 File Offset: 0x000482C8
		public Color InitialLightSourceColor { get; private set; }

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x06000856 RID: 2134 RVA: 0x0004A0D1 File Offset: 0x000482D1
		// (set) Token: 0x06000857 RID: 2135 RVA: 0x0004A0D9 File Offset: 0x000482D9
		public float? InitialLightSpriteAlpha { get; private set; }

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x06000858 RID: 2136 RVA: 0x0004A0E2 File Offset: 0x000482E2
		// (set) Token: 0x06000859 RID: 2137 RVA: 0x0004A0EA File Offset: 0x000482EA
		public LightSource LightSource { get; private set; }

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x0600085A RID: 2138 RVA: 0x0004A0F3 File Offset: 0x000482F3
		// (set) Token: 0x0600085B RID: 2139 RVA: 0x0004A0FB File Offset: 0x000482FB
		public float DamageOverlayStrength
		{
			get
			{
				return this.damageOverlayStrength;
			}
			set
			{
				this.damageOverlayStrength = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x0600085C RID: 2140 RVA: 0x0004A113 File Offset: 0x00048313
		// (set) Token: 0x0600085D RID: 2141 RVA: 0x0004A11B File Offset: 0x0004831B
		public float BurnOverlayStrength
		{
			get
			{
				return this.burnOverLayStrength;
			}
			set
			{
				this.burnOverLayStrength = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x0600085E RID: 2142 RVA: 0x0004A133 File Offset: 0x00048333
		public string HitSoundTag
		{
			get
			{
				RagdollParams.LimbParams @params = this.Params;
				if (@params == null)
				{
					return null;
				}
				RagdollParams.SoundParams sound = @params.Sound;
				if (sound == null)
				{
					return null;
				}
				return sound.Tag;
			}
		}

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x0600085F RID: 2143 RVA: 0x0004A151 File Offset: 0x00048351
		// (set) Token: 0x06000860 RID: 2144 RVA: 0x0004A15C File Offset: 0x0004835C
		public bool EnableHuskSprite
		{
			get
			{
				return this.enableHuskSprite;
			}
			set
			{
				if (this.enableHuskSprite == value)
				{
					return;
				}
				this.enableHuskSprite = value;
				if (this.enableHuskSprite && this.HuskSprite == null)
				{
					this.LoadHuskSprite();
				}
				if (this.HuskSprite != null)
				{
					if (this.enableHuskSprite)
					{
						this.OtherWearables.Insert(0, this.HuskSprite);
						this.UpdateWearableTypesToHide();
						return;
					}
					this.OtherWearables.Remove(this.HuskSprite);
					this.UpdateWearableTypesToHide();
				}
			}
		}

		// Token: 0x06000861 RID: 2145 RVA: 0x0004A1D4 File Offset: 0x000483D4
		private void RefreshDeformations()
		{
			if (this._deformSprite == null)
			{
				return;
			}
			if (this.ConditionalSprites.None(null))
			{
				this.ActiveDeformations = this.Deformations;
				return;
			}
			this.ActiveDeformations.Clear();
			if (this._deformSprite == this.DeformSprite)
			{
				this.ActiveDeformations.AddRange(this.NonConditionalDeformations);
			}
			foreach (ValueTuple<ConditionalSprite, IEnumerable<SpriteDeformation>> conditionalDeformation in this.ConditionalDeformations)
			{
				if (conditionalDeformation.Item1.IsActive)
				{
					this.ActiveDeformations.AddRange(conditionalDeformation.Item2);
				}
			}
		}

		// Token: 0x06000862 RID: 2146 RVA: 0x0004A28C File Offset: 0x0004848C
		public void RecreateSprites()
		{
			if (this.Sprite != null)
			{
				ContentXElement source = this.Sprite.SourceElement;
				this.Sprite.Remove();
				this.Sprite = new Sprite(source, "", this.GetSpritePath(source, this.Params.normalSpriteParams, ref this._texturePath), false, 1f);
			}
			if (this._deformSprite != null)
			{
				ContentXElement source2 = this._deformSprite.Sprite.SourceElement;
				this._deformSprite.Remove();
				ContentXElement element = source2;
				string spritePath = this.GetSpritePath(source2, this.Params.deformSpriteParams, ref this._texturePath);
				this._deformSprite = new DeformableSprite(element, null, null, spritePath, false, false, 1f);
			}
			if (this.DamagedSprite != null)
			{
				ContentXElement source3 = this.DamagedSprite.SourceElement;
				this.DamagedSprite.Remove();
				this.DamagedSprite = new Sprite(source3, "", this.GetSpritePath(source3, this.Params.damagedSpriteParams, ref this._damagedTexturePath), false, 1f);
			}
			for (int i = 0; i < this.ConditionalSprites.Count; i++)
			{
				ConditionalSprite conditionalSprite = this.ConditionalSprites[i];
				ContentXElement source4 = conditionalSprite.ActiveSprite.SourceElement;
				conditionalSprite.Remove();
				this.ConditionalSprites[i] = new ConditionalSprite(source4, this.character, this.GetSpritePath(source4, null, ref this._texturePath), false, 1f);
			}
			for (int j = 0; j < this.DecorativeSprites.Count; j++)
			{
				DecorativeSprite decorativeSprite = this.DecorativeSprites[j];
				ContentXElement source5 = decorativeSprite.Sprite.SourceElement;
				decorativeSprite.Remove();
				this.DecorativeSprites[j] = new DecorativeSprite(source5, "", this.GetSpritePath(source5, this.Params.decorativeSpriteParams[j], ref this._texturePath), false);
			}
		}

		// Token: 0x06000863 RID: 2147 RVA: 0x0004A480 File Offset: 0x00048680
		private void CalculateHeadPosition(Sprite sprite)
		{
			if (this.type != LimbType.Head)
			{
				return;
			}
			CharacterInfo info = this.character.Info;
			if (info == null)
			{
				return;
			}
			info.CalculateHeadPosition(sprite);
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x0004A4A4 File Offset: 0x000486A4
		private string GetSpritePath(ContentXElement element, RagdollParams.SpriteParams spriteParams, ref string path)
		{
			if (path.IsNullOrEmpty())
			{
				if (spriteParams != null)
				{
					ContentPath definedTexturePath = (element != null) ? element.GetAttributeContentPath("texture") : null;
					ContentPath texturePath;
					if (!definedTexturePath.IsNullOrEmpty())
					{
						texturePath = definedTexturePath;
					}
					else
					{
						XDocument variantFile = this.character.Params.VariantFile;
						ContentPath contentPath;
						if (variantFile == null)
						{
							contentPath = null;
						}
						else
						{
							XElement rootExcludingOverride = variantFile.GetRootExcludingOverride();
							contentPath = ((rootExcludingOverride != null) ? rootExcludingOverride.GetAttributeContentPath("texture", this.character.Prefab.ContentPackage) : null);
						}
						texturePath = contentPath;
					}
					if (texturePath.IsNullOrEmpty() && !this.character.Prefab.VariantOf.IsEmpty)
					{
						Identifier speciesName = this.character.GetBaseCharacterSpeciesName();
						RagdollParams parentRagdollParams = this.character.IsHumanoid ? RagdollParams.GetDefaultRagdollParams<HumanRagdollParams>(speciesName, this.character.Params, this.character.Prefab.ContentPackage) : RagdollParams.GetDefaultRagdollParams<FishRagdollParams>(speciesName, this.character.Params, this.character.Prefab.ContentPackage);
						ContentXElement originalElement = parentRagdollParams.OriginalElement;
						texturePath = ((originalElement != null) ? originalElement.GetAttributeContentPath("texture") : null);
					}
					if (texturePath == null)
					{
						texturePath = ContentPath.FromRaw(spriteParams.Element.ContentPackage ?? this.character.Prefab.ContentPackage, spriteParams.GetTexturePath());
					}
					path = this.GetSpritePath(texturePath);
				}
				else
				{
					ContentPath texturePath2 = element.GetAttributeContentPath("texture");
					texturePath2 = (texturePath2.IsNullOrWhiteSpace() ? ContentPath.FromRaw(this.character.Prefab.ContentPackage, this.ragdoll.RagdollParams.Texture) : texturePath2);
					path = this.GetSpritePath(texturePath2);
				}
			}
			return path;
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x0004A644 File Offset: 0x00048844
		public static string GetSpritePath(ContentPath texturePath, CharacterInfo characterInfo)
		{
			string spritePath = texturePath.Value;
			string spritePathWithTags = spritePath;
			if (characterInfo != null)
			{
				spritePath = characterInfo.ReplaceVars(spritePath);
				characterInfo.VerifySpriteTagsLoaded();
				if (characterInfo.SpriteTags.Any<Identifier>())
				{
					string tags = "";
					characterInfo.SpriteTags.ForEach(delegate(Identifier tag)
					{
						string tags = tags;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(tag);
						defaultInterpolatedStringHandler.AppendLiteral("]");
						tags += defaultInterpolatedStringHandler.ToStringAndClear();
					});
					spritePathWithTags = Path.Combine(new string[]
					{
						Path.GetDirectoryName(spritePath),
						Path.GetFileNameWithoutExtension(spritePath) + tags + Path.GetExtension(spritePath)
					});
				}
			}
			if (!File.Exists(spritePathWithTags))
			{
				return spritePath;
			}
			return spritePathWithTags;
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x0004A6DA File Offset: 0x000488DA
		private string GetSpritePath(ContentPath texturePath)
		{
			if (!this.character.IsHumanoid)
			{
				return texturePath.Value;
			}
			return Limb.GetSpritePath(texturePath, this.character.Info);
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x0004A704 File Offset: 0x00048904
		public void Draw(SpriteBatch spriteBatch, Camera cam, Color? overrideColor = null, bool disableDeformations = false)
		{
			RagdollParams.SpriteParams spriteParams = this.Params.GetSprite();
			if (spriteParams == null || this.Alpha <= 0f)
			{
				return;
			}
			float burn = spriteParams.IgnoreTint ? 0f : this.burnOverLayStrength;
			float brightness = Math.Max(1f - burn, 0.2f);
			Color baseColor = this.randomColor ?? spriteParams.Color;
			Color tintedColor = baseColor;
			if (!spriteParams.IgnoreTint)
			{
				tintedColor = tintedColor.Multiply(this.ragdoll.RagdollParams.Color);
				if (this.character.Info != null)
				{
					tintedColor = tintedColor.Multiply(this.character.Info.Head.SkinColor);
				}
				if (this.character.CharacterHealth.FaceTint.A > 0 && this.type == LimbType.Head)
				{
					tintedColor = Color.Lerp(tintedColor, this.character.CharacterHealth.FaceTint.Opaque(), (float)this.character.CharacterHealth.FaceTint.A / 255f);
				}
				if (this.character.CharacterHealth.BodyTint.A > 0)
				{
					tintedColor = Color.Lerp(tintedColor, this.character.CharacterHealth.BodyTint.Opaque(), (float)this.character.CharacterHealth.BodyTint.A / 255f);
				}
			}
			Color color = new Color(tintedColor.Multiply(brightness, false), (int)tintedColor.A);
			Color colorWithoutTint = new Color(baseColor.Multiply(brightness, false), (int)baseColor.A);
			Color blankColor = new Color(brightness, brightness, brightness, 1f);
			if (this.deadTimer > 0f)
			{
				color = Color.Lerp(color, spriteParams.DeadColor, MathUtils.InverseLerp(0f, spriteParams.DeadColorTime, this.deadTimer));
				colorWithoutTint = Color.Lerp(colorWithoutTint, spriteParams.DeadColor, MathUtils.InverseLerp(0f, spriteParams.DeadColorTime, this.deadTimer));
			}
			color = overrideColor.GetValueOrDefault(color);
			colorWithoutTint = overrideColor.GetValueOrDefault(colorWithoutTint);
			blankColor = overrideColor.GetValueOrDefault(blankColor);
			color *= this.Alpha;
			blankColor *= this.Alpha;
			if (this.isSevered)
			{
				if (this.severedFadeOutTimer > this.SeveredFadeOutTime)
				{
					if (this.LightSource != null)
					{
						this.LightSource.Enabled = false;
					}
					return;
				}
				if (this.severedFadeOutTimer > this.SeveredFadeOutTime - 1f)
				{
					color *= this.SeveredFadeOutTime - this.severedFadeOutTimer;
					colorWithoutTint *= this.SeveredFadeOutTime - this.severedFadeOutTimer;
				}
			}
			float herpesStrength = this.character.CharacterHealth.GetAfflictionStrengthByType(AfflictionPrefab.SpaceHerpesType, true);
			bool hideLimb = Limb.<Draw>g__ShouldHideLimb|122_0(this);
			if (!hideLimb && this.Params.InheritHiding != LimbType.None)
			{
				Limb otherLimb = this.character.AnimController.GetLimb(this.Params.InheritHiding, true, false, false);
				if (otherLimb != null)
				{
					hideLimb = Limb.<Draw>g__ShouldHideLimb|122_0(otherLimb);
				}
			}
			bool drawHuskSprite = this.HuskSprite != null && !this.wearableTypesToHide.Contains(WearableType.Husk);
			Sprite activeSprite = this.ActiveSprite;
			if (this.type == LimbType.Head)
			{
				this.CalculateHeadPosition(activeSprite);
			}
			this.body.UpdateDrawPosition(true);
			float depthStep = 1E-06f;
			if (!hideLimb)
			{
				DeformableSprite deformSprite = this.DeformSprite;
				if (deformSprite != null && !disableDeformations)
				{
					if (this.ActiveDeformations.Any<SpriteDeformation>())
					{
						Vector2[,] deformation = SpriteDeformation.GetDeformation(this.ActiveDeformations, deformSprite.Size, this.IsFlipped, false);
						deformSprite.Deform(deformation);
						if (this.LightSource != null && this.LightSource.DeformableLightSprite != null)
						{
							deformation = SpriteDeformation.GetDeformation(this.ActiveDeformations, deformSprite.Size, this.IsFlipped, this.dir == Direction.Left);
							this.LightSource.DeformableLightSprite.Deform(deformation);
						}
					}
					else
					{
						deformSprite.Reset();
					}
					this.body.Draw(deformSprite, cam, Vector2.One * this.Scale * this.TextureScale, color, this.Params.MirrorHorizontally);
				}
				else
				{
					bool useTintMask = this.TintMask != null && spriteBatch.GetCurrentEffect() == null;
					if (useTintMask)
					{
						Sprite sprite = this.Sprite;
						if (((sprite != null) ? sprite.Texture : null) != null)
						{
							Sprite tintMask = this.TintMask;
							if (((tintMask != null) ? tintMask.Texture : null) != null)
							{
								ref Effect ptr = ref this.tintEffectParams.Effect;
								if (ptr == null)
								{
									ptr = GameMain.GameScreen.ThresholdTintEffect;
								}
								ref Dictionary<string, object> ptr2 = ref this.tintEffectParams.Params;
								if (ptr2 == null)
								{
									ptr2 = new Dictionary<string, object>();
								}
								Dictionary<string, object> parameters = this.tintEffectParams.Params;
								parameters["xBaseTexture"] = this.Sprite.Texture;
								parameters["xTintMaskTexture"] = this.TintMask.Texture;
								if (drawHuskSprite && this.HuskMask != null)
								{
									parameters["xCutoffTexture"] = this.HuskMask.Texture;
									parameters["baseToCutoffSizeRatio"] = (float)this.Sprite.Texture.Width / (float)this.HuskMask.Texture.Width;
								}
								else
								{
									parameters["xCutoffTexture"] = GUI.WhiteTexture;
									parameters["baseToCutoffSizeRatio"] = 1f;
								}
								parameters["highlightThreshold"] = this.TintHighlightThreshold;
								parameters["highlightMultiplier"] = this.TintHighlightMultiplier;
								spriteBatch.SwapEffect(this.tintEffectParams);
							}
						}
					}
					this.body.Draw(spriteBatch, activeSprite, color, null, this.Scale * this.TextureScale, this.Params.MirrorHorizontally, this.Params.MirrorVertically, null);
					if (useTintMask)
					{
						spriteBatch.SwapEffect(null, null);
					}
				}
				foreach (ConditionalSprite conditionalSprite in this.ConditionalSprites)
				{
					if (!conditionalSprite.Exclusive && conditionalSprite.IsActive)
					{
						if (conditionalSprite.DeformableSprite != null)
						{
							DeformableSprite defSprite = conditionalSprite.DeformableSprite;
							if (this.ActiveDeformations.Any<SpriteDeformation>())
							{
								Vector2[,] deformation2 = SpriteDeformation.GetDeformation(this.ActiveDeformations, defSprite.Size, this.IsFlipped, false);
								defSprite.Deform(deformation2);
							}
							else
							{
								defSprite.Reset();
							}
							this.body.Draw(defSprite, cam, Vector2.One * this.Scale * this.TextureScale, color, this.Params.MirrorHorizontally);
						}
						else
						{
							this.body.Draw(spriteBatch, conditionalSprite.Sprite, color, new float?(activeSprite.Depth - depthStep * 50f), this.Scale * this.TextureScale, this.Params.MirrorHorizontally, this.Params.MirrorVertically, null);
						}
					}
				}
			}
			SpriteEffects spriteEffect = (this.dir == Direction.Right) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			if (this.LightSource != null)
			{
				this.LightSource.LightSpriteEffect = ((this.dir == Direction.Right) ? SpriteEffects.None : SpriteEffects.FlipVertically);
			}
			float step = depthStep;
			WearableSprite onlyDrawable = this.WearingItems.Find((WearableSprite w) => w.HideOtherWearables);
			if (this.Params.MirrorHorizontally)
			{
				spriteEffect = ((spriteEffect == SpriteEffects.None) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
			}
			if (this.Params.MirrorVertically)
			{
				spriteEffect |= SpriteEffects.FlipVertically;
			}
			if (onlyDrawable == null)
			{
				if (this.HerpesSprite != null && !this.wearableTypesToHide.Contains(WearableType.Herpes) && herpesStrength > 0f)
				{
					float alpha = Math.Min(herpesStrength * 2f / 100f, 1f);
					this.DrawWearable(this.HerpesSprite, depthStep, spriteBatch, blankColor, alpha, spriteEffect);
					depthStep += step;
				}
				if (drawHuskSprite)
				{
					bool useTintEffect = this.HuskMask != null && spriteBatch.GetCurrentEffect() == null;
					if (useTintEffect)
					{
						ref Effect ptr = ref this.huskSpriteParams.Effect;
						if (ptr == null)
						{
							ptr = GameMain.GameScreen.ThresholdTintEffect;
						}
						ref Dictionary<string, object> ptr2 = ref this.huskSpriteParams.Params;
						if (ptr2 == null)
						{
							ptr2 = new Dictionary<string, object>();
						}
						Dictionary<string, object> parameters2 = this.huskSpriteParams.Params;
						parameters2["xCutoffTexture"] = GUI.WhiteTexture;
						parameters2["baseToCutoffSizeRatio"] = 1f;
						spriteBatch.SwapEffect(this.huskSpriteParams);
					}
					this.DrawWearable(this.HuskSprite, depthStep, spriteBatch, color, (float)color.A / 255f, spriteEffect);
					if (useTintEffect)
					{
						spriteBatch.SwapEffect(null, null);
					}
					depthStep += step;
				}
				if (!hideLimb)
				{
					foreach (WearableSprite wearable in this.OtherWearables)
					{
						if (wearable.Type != WearableType.Husk)
						{
							if (this.wearableTypesToHide.Contains(wearable.Type))
							{
								if (wearable.Type == WearableType.Hair && this.HairWithHatSprite != null)
								{
									this.DrawWearable(this.HairWithHatSprite, depthStep, spriteBatch, blankColor, (float)color.A / 255f, spriteEffect);
									depthStep += step;
								}
							}
							else
							{
								this.DrawWearable(wearable, depthStep, spriteBatch, blankColor, (float)color.A / 255f, spriteEffect);
								depthStep += step;
							}
						}
					}
				}
			}
			foreach (WearableSprite wearable2 in this.WearingItems)
			{
				if (onlyDrawable == null || onlyDrawable == wearable2 || !wearable2.CanBeHiddenByOtherWearables)
				{
					if (wearable2.CanBeHiddenByItem.Any<Identifier>())
					{
						bool hiddenByOtherItem = false;
						foreach (WearableSprite otherWearable in this.WearingItems)
						{
							if (otherWearable != wearable2)
							{
								if (wearable2.CanBeHiddenByItem.Contains(otherWearable.WearableComponent.Item.Prefab.Identifier))
								{
									hiddenByOtherItem = true;
									break;
								}
								foreach (Identifier tag in wearable2.CanBeHiddenByItem)
								{
									if (otherWearable.WearableComponent.Item.HasTag(tag))
									{
										hiddenByOtherItem = true;
										break;
									}
								}
							}
						}
						if (hiddenByOtherItem)
						{
							continue;
						}
					}
					this.DrawWearable(wearable2, depthStep, spriteBatch, blankColor, (float)color.A / 255f, spriteEffect);
					depthStep += step;
				}
			}
			if (!this.Hide && onlyDrawable == null)
			{
				foreach (DecorativeSprite decorativeSprite in this.DecorativeSprites)
				{
					if (this.spriteAnimState[decorativeSprite].IsActive)
					{
						Color c = new Color((float)decorativeSprite.Color.R / 255f * brightness, (float)decorativeSprite.Color.G / 255f * brightness, (float)decorativeSprite.Color.B / 255f * brightness, (float)decorativeSprite.Color.A / 255f);
						if (this.deadTimer > 0f)
						{
							c = Color.Lerp(c, spriteParams.DeadColor, MathUtils.InverseLerp(0f, this.Params.GetSprite().DeadColorTime, this.deadTimer));
						}
						c = overrideColor.GetValueOrDefault(c);
						float rotation = decorativeSprite.GetRotation(ref this.spriteAnimState[decorativeSprite].RotationState, this.spriteAnimState[decorativeSprite].RandomRotationFactor);
						Vector2 offset = decorativeSprite.GetOffset(ref this.spriteAnimState[decorativeSprite].OffsetState, this.spriteAnimState[decorativeSprite].RandomOffsetMultiplier, 0f) * this.Scale;
						float ca = (float)Math.Cos((double)(-(double)this.body.Rotation));
						float sa = (float)Math.Sin((double)(-(double)this.body.Rotation));
						Vector2 transformedOffset = new Vector2(ca * offset.X + sa * offset.Y, -sa * offset.X + ca * offset.Y);
						decorativeSprite.Sprite.Draw(spriteBatch, new Vector2(this.body.DrawPosition.X + transformedOffset.X, -(this.body.DrawPosition.Y + transformedOffset.Y)), c, decorativeSprite.Sprite.Origin, -this.body.Rotation + rotation, decorativeSprite.GetScale(ref this.spriteAnimState[decorativeSprite].ScaleState, this.spriteAnimState[decorativeSprite].RandomScaleFactor) * this.Scale, spriteEffect, new float?(activeSprite.Depth - depthStep));
						depthStep += step;
					}
				}
				if (this.damageOverlayStrength > 0f && this.DamagedSprite != null)
				{
					this.DamagedSprite.Draw(spriteBatch, new Vector2(this.body.DrawPosition.X, -this.body.DrawPosition.Y), colorWithoutTint * this.damageOverlayStrength, activeSprite.Origin, -this.body.DrawRotation, this.Scale * this.TextureScale, spriteEffect, new float?(activeSprite.Depth - depthStep * (float)Math.Max(1, this.WearingItems.Count * 2)));
				}
			}
			if (GameMain.DebugDraw)
			{
				if (this.pullJoint != null)
				{
					Vector2 pos = ConvertUnits.ToDisplayUnits(this.pullJoint.WorldAnchorB);
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)pos.X, (int)(-(int)pos.Y), 5, 5), GUIStyle.Red, true, 0f, 1f);
				}
				Vector2 bodyDrawPos = this.body.DrawPosition;
				bodyDrawPos.Y = -bodyDrawPos.Y;
				if (this.IsStuck)
				{
					Vector2 from = ConvertUnits.ToDisplayUnits(this.attachJoint.WorldAnchorA);
					from.Y = -from.Y;
					Vector2 to = ConvertUnits.ToDisplayUnits(this.attachJoint.WorldAnchorB);
					to.Y = -to.Y;
					Vector2 localFront = this.body.GetLocalFront(new float?(this.Params.GetSpriteOrientation()));
					Vector2 front = ConvertUnits.ToDisplayUnits(this.body.FarseerBody.GetWorldPoint(localFront));
					front.Y = -front.Y;
					GUI.DrawLine(spriteBatch, bodyDrawPos, front, Color.Yellow, 0f, 2f);
					GUI.DrawLine(spriteBatch, from, to, GUIStyle.Red, 0f, 1f);
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)from.X, (int)from.Y, 12, 12), Color.White, true, 0f, 1f);
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)to.X, (int)to.Y, 12, 12), Color.White, true, 0f, 1f);
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)from.X, (int)from.Y, 10, 10), Color.Blue, true, 0f, 1f);
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)to.X, (int)to.Y, 10, 10), GUIStyle.Red, true, 0f, 1f);
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)front.X, (int)front.Y, 10, 10), Color.Yellow, true, 0f, 1f);
				}
			}
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x0004B7CC File Offset: 0x000499CC
		public void UpdateWearableTypesToHide()
		{
			Dictionary<WearableSprite, Dictionary<string, object>> dictionary = this.alphaClipEffectParams;
			if (dictionary != null)
			{
				dictionary.Clear();
			}
			this.wearableTypeHidingSprites.Clear();
			this.<UpdateWearableTypesToHide>g__addWearablesFrom|123_0(this.WearingItems);
			this.<UpdateWearableTypesToHide>g__addWearablesFrom|123_0(this.OtherWearables);
			this.wearableTypesToHide.Clear();
			if (this.wearableTypeHidingSprites.Count <= 0)
			{
				return;
			}
			foreach (WearableSprite sprite in this.wearableTypeHidingSprites)
			{
				this.wearableTypesToHide.UnionWith(sprite.HideWearablesOfType);
			}
		}

		// Token: 0x06000869 RID: 2153 RVA: 0x0004B878 File Offset: 0x00049A78
		private void UpdateSpriteStates(float deltaTime)
		{
			foreach (int spriteGroup in this.DecorativeSpriteGroups.Keys)
			{
				for (int i = 0; i < this.DecorativeSpriteGroups[spriteGroup].Count; i++)
				{
					DecorativeSprite decorativeSprite = this.DecorativeSpriteGroups[spriteGroup][i];
					if (decorativeSprite != null)
					{
						Limb.SpriteState spriteState = this.spriteAnimState[decorativeSprite];
						spriteState.IsActive = true;
						foreach (PropertyConditional conditional in decorativeSprite.IsActiveConditionals)
						{
							if (!conditional.Matches(this))
							{
								spriteState.IsActive = false;
								break;
							}
						}
						if (spriteState.IsActive)
						{
							bool animate = true;
							foreach (PropertyConditional conditional2 in decorativeSprite.AnimationConditionals)
							{
								if (!conditional2.Matches(this))
								{
									animate = false;
									break;
								}
							}
							if (animate)
							{
								spriteState.OffsetState += deltaTime;
								spriteState.RotationState += deltaTime;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600086A RID: 2154 RVA: 0x0004BA14 File Offset: 0x00049C14
		public void DrawDamageModifiers(SpriteBatch spriteBatch, Camera cam, Vector2 startPos, bool isScreenSpace)
		{
			foreach (DamageModifier modifier in this.DamageModifiers)
			{
				Color color = (modifier.DamageMultiplier > 1f) ? GUIStyle.Red : GUIStyle.Green;
				float size = ConvertUnits.ToDisplayUnits(this.body.GetSize().Length() / 2f);
				if (isScreenSpace)
				{
					size *= cam.Zoom;
				}
				int thickness = 2;
				if (!isScreenSpace)
				{
					thickness = (int)Math.Round((double)((float)thickness / cam.Zoom));
				}
				float bodyRotation = -this.body.Rotation;
				float constantOffset = -1.5707964f;
				Vector2 armorSector = modifier.ArmorSectorInRadians;
				float armorSectorSize = Math.Abs(armorSector.X - armorSector.Y);
				float radians = armorSectorSize * this.Dir;
				float armorSectorOffset = armorSector.X * this.Dir;
				float finalOffset = bodyRotation + constantOffset + armorSectorOffset;
				spriteBatch.DrawSector(startPos, size, radians, 40, color, finalOffset, (float)thickness);
			}
		}

		// Token: 0x0600086B RID: 2155 RVA: 0x0004BB38 File Offset: 0x00049D38
		[return: TupleElementNames(new string[]
		{
			"FinalColor",
			"Origin",
			"Rotation",
			"Scale",
			"Depth"
		})]
		private ValueTuple<Color, Vector2, float, float, float> CalculateDrawParameters(WearableSprite wearable, float depthStep, Color color, float alpha)
		{
			Sprite sprite = this.ActiveSprite;
			if (wearable.InheritSourceRect)
			{
				if (wearable.SheetIndex != null)
				{
					wearable.Sprite.SourceRect = new Rectangle(CharacterInfo.CalculateOffset(sprite, wearable.SheetIndex.Value), sprite.SourceRect.Size);
				}
				else if (this.type == LimbType.Head && this.character.Info != null)
				{
					wearable.Sprite.SourceRect = new Rectangle(CharacterInfo.CalculateOffset(sprite, this.character.Info.Head.SheetIndex.ToPoint()), sprite.SourceRect.Size);
				}
				else
				{
					wearable.Sprite.SourceRect = sprite.SourceRect;
				}
			}
			Vector2 origin;
			if (wearable.InheritOrigin)
			{
				origin = sprite.Origin;
				wearable.Sprite.Origin = origin;
			}
			else
			{
				origin = wearable.Sprite.Origin;
				if (this.body.Dir == -1f)
				{
					origin.X = (float)wearable.Sprite.SourceRect.Width - origin.X;
				}
			}
			float depth = wearable.Sprite.Depth;
			if (wearable.InheritLimbDepth)
			{
				depth = sprite.Depth - depthStep;
				Limb depthLimb = (wearable.DepthLimb == LimbType.None) ? this : this.character.AnimController.GetLimb(wearable.DepthLimb, true, false, false);
				if (depthLimb != null)
				{
					depth = depthLimb.ActiveSprite.Depth - depthStep;
				}
			}
			Wearable wearableItemComponent = wearable.WearableComponent;
			Color wearableColor = Color.White;
			if (wearableItemComponent != null)
			{
				if (wearableItemComponent.AllowedSlots.Contains(InvSlotType.OuterClothes))
				{
					depth -= depthStep;
				}
				if (wearableItemComponent.AllowedSlots.Contains(InvSlotType.Bag))
				{
					depth -= depthStep * 4f;
				}
				wearableColor = wearableItemComponent.Item.GetSpriteColor(null, false);
			}
			else if (this.character.Info != null)
			{
				if (wearable.Type == WearableType.Hair)
				{
					wearableColor = this.character.Info.Head.HairColor;
				}
				else if (wearable.Type == WearableType.Beard || wearable.Type == WearableType.Moustache)
				{
					wearableColor = this.character.Info.Head.FacialHairColor;
				}
			}
			float scale = wearable.Scale;
			if (wearable.InheritScale)
			{
				if (!wearable.IgnoreTextureScale)
				{
					scale *= this.TextureScale;
				}
				if (!wearable.IgnoreLimbScale)
				{
					scale *= this.Params.Scale;
				}
				if (!wearable.IgnoreRagdollScale)
				{
					scale *= this.ragdoll.RagdollParams.LimbScale;
				}
			}
			float rotation = -this.body.DrawRotation - wearable.Rotation * this.Dir;
			float finalAlpha = alpha * (float)wearableColor.A;
			Color finalColor = color.Multiply(wearableColor);
			finalColor = new Color(finalColor.R, finalColor.G, finalColor.B, (byte)finalAlpha);
			return new ValueTuple<Color, Vector2, float, float, float>(finalColor, origin, rotation, scale, depth);
		}

		// Token: 0x0600086C RID: 2156 RVA: 0x0004BE24 File Offset: 0x0004A024
		private void ApplyAlphaClip(SpriteBatch spriteBatch, WearableSprite wearable, WearableSprite alphaClipper, SpriteEffects spriteEffect)
		{
			Limb.<>c__DisplayClass129_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.spriteEffect = spriteEffect;
			SpriteRecorder.Command wearableCommand = this.<ApplyAlphaClip>g__makeCommand|129_0(wearable, ref CS$<>8__locals1);
			SpriteRecorder.Command clipperCommand = this.<ApplyAlphaClip>g__makeCommand|129_0(alphaClipper, ref CS$<>8__locals1);
			CoordinateSpace2D wearableTextureSpace;
			CoordinateSpace2D wearableWorldSpace;
			this.<ApplyAlphaClip>g__spacesFromCommand|129_1(wearable, wearableCommand, out wearableTextureSpace, out wearableWorldSpace, ref CS$<>8__locals1);
			CoordinateSpace2D clipperTextureSpace;
			CoordinateSpace2D clipperWorldSpace;
			this.<ApplyAlphaClip>g__spacesFromCommand|129_1(alphaClipper, clipperCommand, out clipperTextureSpace, out clipperWorldSpace, ref CS$<>8__locals1);
			Matrix wearableUvToClipperUv = wearableTextureSpace.CanonicalToLocal * wearableWorldSpace.LocalToCanonical * clipperWorldSpace.CanonicalToLocal * clipperTextureSpace.LocalToCanonical;
			if (Limb.alphaClipEffect == null)
			{
				Limb.alphaClipEffect = EffectLoader.Load("Effects/wearableclip");
			}
			if (this.alphaClipEffectParams == null)
			{
				this.alphaClipEffectParams = new Dictionary<WearableSprite, Dictionary<string, object>>();
			}
			if (!this.alphaClipEffectParams.ContainsKey(wearable))
			{
				this.alphaClipEffectParams.Add(wearable, new Dictionary<string, object>());
			}
			SpriteBatch.EffectWithParams paramsToPass = new SpriteBatch.EffectWithParams
			{
				Effect = Limb.alphaClipEffect,
				Params = this.alphaClipEffectParams[wearable]
			};
			paramsToPass.Params["wearableUvToClipperUv"] = wearableUvToClipperUv;
			paramsToPass.Params["stencilUVmin"] = new Vector2((float)alphaClipper.Sprite.SourceRect.X / (float)alphaClipper.Sprite.Texture.Width, (float)alphaClipper.Sprite.SourceRect.Y / (float)alphaClipper.Sprite.Texture.Height);
			paramsToPass.Params["stencilUVmax"] = new Vector2((float)alphaClipper.Sprite.SourceRect.Right / (float)alphaClipper.Sprite.Texture.Width, (float)alphaClipper.Sprite.SourceRect.Bottom / (float)alphaClipper.Sprite.Texture.Height);
			paramsToPass.Params["clipperTexelSize"] = 2f / (float)alphaClipper.Sprite.Texture.Width;
			paramsToPass.Params["aCutoff"] = 0.007843138f;
			paramsToPass.Params["xTexture"] = wearable.Sprite.Texture;
			paramsToPass.Params["xStencil"] = alphaClipper.Sprite.Texture;
			spriteBatch.SwapEffect(paramsToPass);
		}

		// Token: 0x0600086D RID: 2157 RVA: 0x0004C07C File Offset: 0x0004A27C
		private void DrawWearable(WearableSprite wearable, float depthStep, SpriteBatch spriteBatch, Color color, float alpha, SpriteEffects spriteEffect)
		{
			Sprite sprite = wearable.Sprite;
			if (((sprite != null) ? sprite.Texture : null) == null)
			{
				return;
			}
			ValueTuple<Color, Vector2, float, float, float> valueTuple = this.CalculateDrawParameters(wearable, depthStep, color, alpha);
			Color finalColor = valueTuple.Item1;
			Vector2 origin = valueTuple.Item2;
			float rotation = valueTuple.Item3;
			float scale = valueTuple.Item4;
			float depth = valueTuple.Item5;
			Effect prevEffect = spriteBatch.GetCurrentEffect();
			WearableSprite alphaClipper = this.WearingItems.Find((WearableSprite w) => w.AlphaClipOtherWearables);
			bool flag;
			if (alphaClipper == null)
			{
				flag = (null != null);
			}
			else
			{
				Sprite sprite2 = alphaClipper.Sprite;
				flag = (((sprite2 != null) ? sprite2.Texture : null) != null);
			}
			bool shouldApplyAlphaClip = flag && wearable != alphaClipper;
			if (shouldApplyAlphaClip)
			{
				this.ApplyAlphaClip(spriteBatch, wearable, alphaClipper, spriteEffect);
			}
			wearable.Sprite.Draw(spriteBatch, new Vector2(this.body.DrawPosition.X, -this.body.DrawPosition.Y), finalColor, origin, rotation, scale, spriteEffect, new float?(depth));
			if (shouldApplyAlphaClip)
			{
				spriteBatch.SwapEffect(prevEffect, null);
			}
		}

		// Token: 0x0600086E RID: 2158 RVA: 0x0004C188 File Offset: 0x0004A388
		private WearableSprite GetWearableSprite(WearableType type)
		{
			CharacterInfo info = this.character.Info;
			if (info == null)
			{
				return null;
			}
			IEnumerable<ContentXElement> enumerable = info.FilterElements(info.Wearables, info.Head.Preset.TagSet, new WearableType?(type));
			ContentXElement element = (enumerable != null) ? enumerable.FirstOrDefault<ContentXElement>() : null;
			ContentXElement contentXElement = null;
			if (!(element != contentXElement))
			{
				return null;
			}
			return new WearableSprite(element.GetChildElement("sprite"), type);
		}

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x0600086F RID: 2159 RVA: 0x0004C1F5 File Offset: 0x0004A3F5
		// (set) Token: 0x06000870 RID: 2160 RVA: 0x0004C1FD File Offset: 0x0004A3FD
		public float SeveredFadeOutTime { get; private set; } = 10f;

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000871 RID: 2161 RVA: 0x0004C206 File Offset: 0x0004A406
		public Vector2 StepOffset
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Params.StepOffset) * this.ragdoll.RagdollParams.JointScale;
			}
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000872 RID: 2162 RVA: 0x0004C22D File Offset: 0x0004A42D
		// (set) Token: 0x06000873 RID: 2163 RVA: 0x0004C235 File Offset: 0x0004A435
		public bool InWater { get; set; }

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x06000874 RID: 2164 RVA: 0x0004C23E File Offset: 0x0004A43E
		// (set) Token: 0x06000875 RID: 2165 RVA: 0x0004C248 File Offset: 0x0004A448
		public bool IgnoreCollisions
		{
			get
			{
				return this.ignoreCollisions;
			}
			set
			{
				this.ignoreCollisions = value;
				if (this.body != null)
				{
					if (this.ignoreCollisions)
					{
						this.body.CollisionCategories = Category.None;
						this.body.CollidesWith = Category.None;
						return;
					}
					this.body.CollisionCategories = Category.Cat2;
					this.body.CollidesWith = (Category.Cat1 | Category.Cat3 | Category.Cat4 | Category.Cat7 | Category.Cat8 | Category.Cat9 | Category.Cat10 | Category.Cat11 | Category.Cat12 | Category.Cat13 | Category.Cat14 | Category.Cat15 | Category.Cat16 | Category.Cat17 | Category.Cat18 | Category.Cat19 | Category.Cat20 | Category.Cat21 | Category.Cat22 | Category.Cat23 | Category.Cat24 | Category.Cat25 | Category.Cat26 | Category.Cat27 | Category.Cat28 | Category.Cat29 | Category.Cat30 | Category.Cat31);
				}
			}
		}

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06000876 RID: 2166 RVA: 0x0004C2A4 File Offset: 0x0004A4A4
		// (set) Token: 0x06000877 RID: 2167 RVA: 0x0004C2ED File Offset: 0x0004A4ED
		public Vector2 MouthPos
		{
			get
			{
				Vector2 valueOrDefault = this.mouthPos.GetValueOrDefault();
				if (this.mouthPos == null)
				{
					valueOrDefault = this.Params.MouthPos;
					this.mouthPos = new Vector2?(valueOrDefault);
				}
				return this.mouthPos.Value;
			}
			set
			{
				this.mouthPos = new Vector2?(value);
			}
		}

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000878 RID: 2168 RVA: 0x0004C2FB File Offset: 0x0004A4FB
		// (set) Token: 0x06000879 RID: 2169 RVA: 0x0004C303 File Offset: 0x0004A503
		public List<DamageModifier> DamageModifiers { get; private set; } = new List<DamageModifier>();

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x0600087A RID: 2170 RVA: 0x0004C30C File Offset: 0x0004A50C
		public int HealthIndex
		{
			get
			{
				return this.Params.HealthIndex;
			}
		}

		// Token: 0x1700026B RID: 619
		// (get) Token: 0x0600087B RID: 2171 RVA: 0x0004C319 File Offset: 0x0004A519
		public float Scale
		{
			get
			{
				return this.Params.Scale * this.Params.Ragdoll.LimbScale;
			}
		}

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x0600087C RID: 2172 RVA: 0x0004C337 File Offset: 0x0004A537
		public float AttackPriority
		{
			get
			{
				return this.Params.AttackPriority;
			}
		}

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x0600087D RID: 2173 RVA: 0x0004C344 File Offset: 0x0004A544
		public bool DoesFlip
		{
			get
			{
				Character character = this.character;
				return (((character != null) ? character.AnimController.CurrentAnimationParams : null) is GroundedMovementParams && this.IsLeg) || this.Params.Flip;
			}
		}

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x0600087E RID: 2174 RVA: 0x0004C379 File Offset: 0x0004A579
		public bool DoesMirror
		{
			get
			{
				return this.IsLeg || this.DoesFlip;
			}
		}

		// Token: 0x1700026F RID: 623
		// (get) Token: 0x0600087F RID: 2175 RVA: 0x0004C38B File Offset: 0x0004A58B
		public float SteerForce
		{
			get
			{
				return this.Params.SteerForce;
			}
		}

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x06000880 RID: 2176 RVA: 0x0004C398 File Offset: 0x0004A598
		public bool IsLowerBody
		{
			get
			{
				LimbType limbType = this.type;
				return limbType - LimbType.LeftLeg <= 3 || limbType - LimbType.Tail <= 4;
			}
		}

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x06000881 RID: 2177 RVA: 0x0004C3BC File Offset: 0x0004A5BC
		public bool IsLeg
		{
			get
			{
				LimbType limbType = this.type;
				return limbType - LimbType.LeftLeg <= 3 || limbType - LimbType.RightThigh <= 1;
			}
		}

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000882 RID: 2178 RVA: 0x0004C3E4 File Offset: 0x0004A5E4
		public bool IsArm
		{
			get
			{
				LimbType limbType = this.type;
				return limbType - LimbType.LeftHand <= 5;
			}
		}

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000883 RID: 2179 RVA: 0x0004C405 File Offset: 0x0004A605
		// (set) Token: 0x06000884 RID: 2180 RVA: 0x0004C410 File Offset: 0x0004A610
		public bool IsSevered
		{
			get
			{
				return this.isSevered;
			}
			set
			{
				if (this.isSevered == value)
				{
					return;
				}
				if (value)
				{
					IEnumerable<Limb> connectedLimbs = this.GetConnectedLimbs();
					float severedFadeOutTime = this.Params.SeveredFadeOutTime;
					float val;
					if (!connectedLimbs.Any<Limb>())
					{
						val = 0f;
					}
					else
					{
						val = connectedLimbs.Max((Limb l) => l.SeveredFadeOutTime);
					}
					this.SeveredFadeOutTime = Math.Max(severedFadeOutTime, val);
				}
				this.isSevered = value;
				if (this.isSevered)
				{
					this.ragdoll.SubtractMass(this);
					if (this.type == LimbType.Head && this.character.Params.Health.DieFromBeheading)
					{
						this.character.Kill(CauseOfDeathType.Unknown, null, false, true);
					}
				}
				else
				{
					this.severedFadeOutTimer = 0f;
				}
				if (this.isSevered)
				{
					this.damageOverlayStrength = 1f;
				}
			}
		}

		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06000885 RID: 2181 RVA: 0x0004C4E6 File Offset: 0x0004A6E6
		public Submarine Submarine
		{
			get
			{
				Character character = this.character;
				if (character == null)
				{
					return null;
				}
				return character.Submarine;
			}
		}

		// Token: 0x17000275 RID: 629
		// (get) Token: 0x06000886 RID: 2182 RVA: 0x0004C4F9 File Offset: 0x0004A6F9
		// (set) Token: 0x06000887 RID: 2183 RVA: 0x0004C510 File Offset: 0x0004A710
		public bool Hidden
		{
			get
			{
				return this._hidden || this.Params.Hide;
			}
			set
			{
				this._hidden = value;
			}
		}

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06000888 RID: 2184 RVA: 0x0004C519 File Offset: 0x0004A719
		// (set) Token: 0x06000889 RID: 2185 RVA: 0x0004C521 File Offset: 0x0004A721
		public bool Hide
		{
			get
			{
				return this.Hidden;
			}
			set
			{
				this.Hidden = value;
			}
		}

		// Token: 0x17000277 RID: 631
		// (get) Token: 0x0600088A RID: 2186 RVA: 0x0004C52A File Offset: 0x0004A72A
		public Vector2 WorldPosition
		{
			get
			{
				Character character = this.character;
				if (((character != null) ? character.Submarine : null) != null)
				{
					return this.Position + this.character.Submarine.Position;
				}
				return this.Position;
			}
		}

		// Token: 0x17000278 RID: 632
		// (get) Token: 0x0600088B RID: 2187 RVA: 0x0004C562 File Offset: 0x0004A762
		public Vector2 Position
		{
			get
			{
				PhysicsBody physicsBody = this.body;
				return ConvertUnits.ToDisplayUnits((physicsBody != null) ? physicsBody.SimPosition : Vector2.Zero);
			}
		}

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x0600088C RID: 2188 RVA: 0x0004C57F File Offset: 0x0004A77F
		public Vector2 SimPosition
		{
			get
			{
				if (this.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("Limb.LinearVelocity:SimPosition", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed limb.\n" + Environment.StackTrace.CleanupStackTrace());
					return Vector2.Zero;
				}
				return this.body.SimPosition;
			}
		}

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x0600088D RID: 2189 RVA: 0x0004C5B9 File Offset: 0x0004A7B9
		public Vector2 DrawPosition
		{
			get
			{
				if (this.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("Limb.LinearVelocity:DrawPosition", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed limb.\n" + Environment.StackTrace.CleanupStackTrace());
					return Vector2.Zero;
				}
				return this.body.DrawPosition;
			}
		}

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x0600088E RID: 2190 RVA: 0x0004C5F3 File Offset: 0x0004A7F3
		public float Rotation
		{
			get
			{
				if (this.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("Limb.LinearVelocity:SimPosition", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed limb.\n" + Environment.StackTrace.CleanupStackTrace());
					return 0f;
				}
				return this.body.Rotation;
			}
		}

		// Token: 0x1700027C RID: 636
		// (get) Token: 0x0600088F RID: 2191 RVA: 0x0004C62D File Offset: 0x0004A82D
		// (set) Token: 0x06000890 RID: 2192 RVA: 0x0004C635 File Offset: 0x0004A835
		public Vector2 AnimTargetPos { get; private set; }

		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06000891 RID: 2193 RVA: 0x0004C63E File Offset: 0x0004A83E
		public float Mass
		{
			get
			{
				if (this.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("Limb.Mass:AccessRemoved", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed limb.\n" + Environment.StackTrace.CleanupStackTrace());
					return 1f;
				}
				return this.body.Mass;
			}
		}

		// Token: 0x1700027E RID: 638
		// (get) Token: 0x06000892 RID: 2194 RVA: 0x0004C678 File Offset: 0x0004A878
		// (set) Token: 0x06000893 RID: 2195 RVA: 0x0004C680 File Offset: 0x0004A880
		public bool Disabled { get; set; }

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x06000894 RID: 2196 RVA: 0x0004C689 File Offset: 0x0004A889
		public Vector2 LinearVelocity
		{
			get
			{
				if (this.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("Limb.LinearVelocity:AccessRemoved", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed limb.\n" + Environment.StackTrace.CleanupStackTrace());
					return Vector2.Zero;
				}
				return this.body.LinearVelocity;
			}
		}

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x06000895 RID: 2197 RVA: 0x0004C6C3 File Offset: 0x0004A8C3
		// (set) Token: 0x06000896 RID: 2198 RVA: 0x0004C6D9 File Offset: 0x0004A8D9
		public float Dir
		{
			get
			{
				if (this.dir != Direction.Left)
				{
					return 1f;
				}
				return -1f;
			}
			set
			{
				this.dir = ((value == -1f) ? Direction.Left : Direction.Right);
				if (this.body != null)
				{
					this.body.Dir = this.Dir;
				}
			}
		}

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x06000897 RID: 2199 RVA: 0x0004C706 File Offset: 0x0004A906
		// (set) Token: 0x06000898 RID: 2200 RVA: 0x0004C70E File Offset: 0x0004A90E
		public float Alpha
		{
			get
			{
				return this._alpha;
			}
			set
			{
				this._alpha = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x06000899 RID: 2201 RVA: 0x0004C726 File Offset: 0x0004A926
		public int RefJointIndex
		{
			get
			{
				return this.Params.RefJoint;
			}
		}

		// Token: 0x17000283 RID: 643
		// (get) Token: 0x0600089A RID: 2202 RVA: 0x0004C733 File Offset: 0x0004A933
		// (set) Token: 0x0600089B RID: 2203 RVA: 0x0004C740 File Offset: 0x0004A940
		public bool PullJointEnabled
		{
			get
			{
				return this.pullJoint.Enabled;
			}
			set
			{
				this.pullJoint.Enabled = value;
			}
		}

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x0600089C RID: 2204 RVA: 0x0004C74E File Offset: 0x0004A94E
		// (set) Token: 0x0600089D RID: 2205 RVA: 0x0004C75B File Offset: 0x0004A95B
		public float PullJointMaxForce
		{
			get
			{
				return this.pullJoint.MaxForce;
			}
			set
			{
				this.pullJoint.MaxForce = value;
			}
		}

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x0600089E RID: 2206 RVA: 0x0004C769 File Offset: 0x0004A969
		// (set) Token: 0x0600089F RID: 2207 RVA: 0x0004C778 File Offset: 0x0004A978
		public Vector2 PullJointWorldAnchorA
		{
			get
			{
				return this.pullJoint.WorldAnchorA;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					string str = "Attempted to set the anchor A of a limb's pull joint to an invalid value (";
					Vector2 vector = value;
					string errorMsg = str + vector.ToString() + ")\n" + Environment.StackTrace.CleanupStackTrace();
					GameAnalyticsManager.AddErrorEventOnce("Limb.SetPullJointAnchorA:InvalidValue", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					return;
				}
				if (Vector2.DistanceSquared(this.SimPosition, value) > 2500f)
				{
					Vector2 diff = value - this.SimPosition;
					string[] array = new string[8];
					array[0] = "Attempted to move the anchor A of a limb's pull joint extremely far from the limb (diff: ";
					int num = 1;
					Vector2 vector = diff;
					array[num] = vector.ToString();
					array[2] = ", limb enabled: ";
					array[3] = this.body.Enabled.ToString();
					array[4] = ", simple physics enabled: ";
					array[5] = this.character.AnimController.SimplePhysicsEnabled.ToString();
					array[6] = ")\n";
					array[7] = Environment.StackTrace.CleanupStackTrace();
					string errorMsg2 = string.Concat(array);
					GameAnalyticsManager.AddErrorEventOnce("Limb.SetPullJointAnchorA:ExcessiveValue", GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
					return;
				}
				this.pullJoint.WorldAnchorA = value;
			}
		}

		// Token: 0x17000286 RID: 646
		// (get) Token: 0x060008A0 RID: 2208 RVA: 0x0004C87D File Offset: 0x0004AA7D
		// (set) Token: 0x060008A1 RID: 2209 RVA: 0x0004C88C File Offset: 0x0004AA8C
		public Vector2 PullJointWorldAnchorB
		{
			get
			{
				return this.pullJoint.WorldAnchorB;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					string str = "Attempted to set the anchor B of a limb's pull joint to an invalid value (";
					Vector2 vector = value;
					string errorMsg = str + vector.ToString() + ")\n" + Environment.StackTrace.CleanupStackTrace();
					GameAnalyticsManager.AddErrorEventOnce("Limb.SetPullJointAnchorB:InvalidValue", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					return;
				}
				if (Vector2.DistanceSquared(this.pullJoint.WorldAnchorA, value) > 2500f)
				{
					Vector2 diff = value - this.pullJoint.WorldAnchorA;
					string[] array = new string[8];
					array[0] = "Attempted to move the anchor B of a limb's pull joint extremely far from the limb (diff: ";
					int num = 1;
					Vector2 vector = diff;
					array[num] = vector.ToString();
					array[2] = ", limb enabled: ";
					array[3] = this.body.Enabled.ToString();
					array[4] = ", simple physics enabled: ";
					array[5] = this.character.AnimController.SimplePhysicsEnabled.ToString();
					array[6] = ")\n";
					array[7] = Environment.StackTrace.CleanupStackTrace();
					string errorMsg2 = string.Concat(array);
					GameAnalyticsManager.AddErrorEventOnce("Limb.SetPullJointAnchorB:ExcessiveValue", GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
					return;
				}
				this.pullJoint.WorldAnchorB = value;
			}
		}

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x060008A2 RID: 2210 RVA: 0x0004C99B File Offset: 0x0004AB9B
		public Vector2 PullJointLocalAnchorA
		{
			get
			{
				return this.pullJoint.LocalAnchorA;
			}
		}

		// Token: 0x17000288 RID: 648
		// (get) Token: 0x060008A3 RID: 2211 RVA: 0x0004C9A8 File Offset: 0x0004ABA8
		// (set) Token: 0x060008A4 RID: 2212 RVA: 0x0004C9B0 File Offset: 0x0004ABB0
		public bool Removed { get; private set; }

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x060008A5 RID: 2213 RVA: 0x0004C9B9 File Offset: 0x0004ABB9
		// (set) Token: 0x060008A6 RID: 2214 RVA: 0x0004C9C1 File Offset: 0x0004ABC1
		public Rope AttachedRope { get; set; }

		// Token: 0x1700028A RID: 650
		// (get) Token: 0x060008A7 RID: 2215 RVA: 0x0004C9CA File Offset: 0x0004ABCA
		public string Name
		{
			get
			{
				return this.Params.Name;
			}
		}

		// Token: 0x1700028B RID: 651
		// (get) Token: 0x060008A8 RID: 2216 RVA: 0x0004C9D7 File Offset: 0x0004ABD7
		public bool IsDead
		{
			get
			{
				return this.character.IsDead;
			}
		}

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x060008A9 RID: 2217 RVA: 0x0004C9E4 File Offset: 0x0004ABE4
		public float Health
		{
			get
			{
				return this.character.Health;
			}
		}

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x060008AA RID: 2218 RVA: 0x0004C9F1 File Offset: 0x0004ABF1
		public float HealthPercentage
		{
			get
			{
				return this.character.HealthPercentage;
			}
		}

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x060008AB RID: 2219 RVA: 0x0004C9FE File Offset: 0x0004ABFE
		public bool IsHuman
		{
			get
			{
				return this.character.IsHuman;
			}
		}

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x060008AC RID: 2220 RVA: 0x0004CA0C File Offset: 0x0004AC0C
		public AIState AIState
		{
			get
			{
				EnemyAIController enemyAI = this.character.AIController as EnemyAIController;
				if (enemyAI == null)
				{
					return AIState.Idle;
				}
				return enemyAI.State;
			}
		}

		// Token: 0x17000290 RID: 656
		// (get) Token: 0x060008AD RID: 2221 RVA: 0x0004CA35 File Offset: 0x0004AC35
		public bool IsFlipped
		{
			get
			{
				return this.character.AnimController.IsFlipped;
			}
		}

		// Token: 0x17000291 RID: 657
		// (get) Token: 0x060008AE RID: 2222 RVA: 0x0004CA48 File Offset: 0x0004AC48
		public bool CanBeSeveredAlive
		{
			get
			{
				if (this.character.IsHumanoid)
				{
					return false;
				}
				if (this == this.character.AnimController.MainLimb)
				{
					return false;
				}
				bool canBeSevered = this.Params.CanBeSeveredAlive;
				if (this.character.AnimController.CanWalk && !this.character.Params.Health.AllowSeveringLegs)
				{
					LimbType limbType = this.type;
					if (limbType - LimbType.LeftLeg <= 3 || limbType - LimbType.Legs <= 3)
					{
						return false;
					}
				}
				return canBeSevered;
			}
		}

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x060008AF RID: 2223 RVA: 0x0004CAC5 File Offset: 0x0004ACC5
		// (set) Token: 0x060008B0 RID: 2224 RVA: 0x0004CACD File Offset: 0x0004ACCD
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x17000293 RID: 659
		// (get) Token: 0x060008B1 RID: 2225 RVA: 0x0004CAD6 File Offset: 0x0004ACD6
		public Dictionary<ActionType, List<StatusEffect>> StatusEffects
		{
			get
			{
				return this.statusEffects;
			}
		}

		// Token: 0x060008B2 RID: 2226 RVA: 0x0004CAE0 File Offset: 0x0004ACE0
		public Limb(Ragdoll ragdoll, Character character, RagdollParams.LimbParams limbParams)
		{
			this.ragdoll = ragdoll;
			this.character = character;
			this.Params = limbParams;
			this.dir = Direction.Right;
			this.body = new PhysicsBody(limbParams, false);
			this.type = limbParams.Type;
			this.IgnoreCollisions = limbParams.IgnoreCollisions;
			this.body.UserData = this;
			this.pullJoint = new FixedMouseJoint(this.body.FarseerBody, ConvertUnits.ToSimUnits(limbParams.PullPos * this.Scale))
			{
				Enabled = false,
				MaxForce = 1000f * this.Mass
			};
			GameMain.World.Add(this.pullJoint);
			ContentXElement element = limbParams.Element;
			this.body.BodyType = BodyType.Dynamic;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "attack"))
				{
					if (!(a == "damagemodifier"))
					{
						if (a == "statuseffect")
						{
							StatusEffect statusEffect = StatusEffect.Load(subElement, character.Name + ", " + this.Name);
							if (statusEffect != null)
							{
								if (!this.statusEffects.ContainsKey(statusEffect.type))
								{
									this.statusEffects.Add(statusEffect.type, new List<StatusEffect>());
								}
								this.statusEffects[statusEffect.type].Add(statusEffect);
							}
						}
					}
					else
					{
						this.DamageModifiers.Add(new DamageModifier(subElement, character.Name, true));
					}
				}
				else
				{
					this.attack = new Attack(subElement, ((character == null) ? "null" : character.Name) + ", limb " + this.type.ToString());
					if (this.attack.DamageRange <= 0f)
					{
						switch (this.body.BodyShape)
						{
						case PhysicsBody.Shape.Circle:
							this.attack.DamageRange = this.body.Radius;
							break;
						case PhysicsBody.Shape.Rectangle:
							this.attack.DamageRange = new Vector2(this.body.Width / 2f, this.body.Height / 2f).Length();
							break;
						case PhysicsBody.Shape.Capsule:
							this.attack.DamageRange = this.body.Height / 2f + this.body.Radius;
							break;
						}
						this.attack.DamageRange = ConvertUnits.ToDisplayUnits(this.attack.DamageRange);
					}
					if (character != null && !character.VariantOf.IsEmpty)
					{
						XElement attackElement = character.Params.VariantFile.GetRootExcludingOverride().GetChildElement("attack", StringComparison.OrdinalIgnoreCase);
						if (attackElement != null)
						{
							this.attack.SetInitialDamageMultiplier(attackElement.GetAttributeFloat("damagemultiplier", 1f));
							this.attack.RangeMultiplier = attackElement.GetAttributeFloat("rangemultiplier", 1f);
							this.attack.ImpactMultiplier = attackElement.GetAttributeFloat("impactmultiplier", 1f);
						}
					}
				}
			}
			this.SerializableProperties = SerializableProperty.GetProperties(this);
			this.InitProjSpecific(element);
		}

		// Token: 0x060008B3 RID: 2227 RVA: 0x0004CF54 File Offset: 0x0004B154
		private void InitProjSpecific(ContentXElement element)
		{
			for (int i = 0; i < this.Params.decorativeSpriteParams.Count; i++)
			{
				RagdollParams.DecorativeSpriteParams param = this.Params.decorativeSpriteParams[i];
				DecorativeSprite decorativeSprite = new DecorativeSprite(param.Element, "", this.GetSpritePath(param.Element, param, ref this._texturePath), false);
				this.DecorativeSprites.Add(decorativeSprite);
				int groupID = decorativeSprite.RandomGroupID;
				if (!this.DecorativeSpriteGroups.ContainsKey(groupID))
				{
					this.DecorativeSpriteGroups.Add(groupID, new List<DecorativeSprite>());
				}
				this.DecorativeSpriteGroups[groupID].Add(decorativeSprite);
				this.spriteAnimState.Add(decorativeSprite, new Limb.SpriteState());
			}
			this.TintMask = null;
			float sourceRectScale = this.ragdoll.RagdollParams.SourceRectScale;
			foreach (ContentXElement subElement in element.Elements())
			{
				Limb.<>c__DisplayClass276_0 CS$<>8__locals1;
				CS$<>8__locals1.subElement = subElement;
				string text = CS$<>8__locals1.subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					if (length != 6)
					{
						if (length != 8)
						{
							switch (length)
							{
							case 11:
							{
								char c = text[0];
								if (c != 'l')
								{
									if (c == 'r')
									{
										if (text == "randomcolor")
										{
											Color[] attributeColorArray = CS$<>8__locals1.subElement.GetAttributeColorArray("colors", null);
											this.randomColor = ((attributeColorArray != null) ? new Color?(attributeColorArray.GetRandomUnsynced<Color>()) : null);
										}
									}
								}
								else if (text == "lightsource")
								{
									this.LightSource = new LightSource(CS$<>8__locals1.subElement, this.<InitProjSpecific>g__GetConditionalTarget|276_0(ref CS$<>8__locals1))
									{
										ParentBody = this.body,
										SpriteScale = Vector2.One * this.Scale * this.TextureScale
									};
									if (this.randomColor != null)
									{
										this.LightSource.Color = new Color(this.randomColor.Value.R, this.randomColor.Value.G, this.randomColor.Value.B, this.LightSource.Color.A);
									}
									this.InitialLightSourceColor = this.LightSource.Color;
									this.InitialLightSpriteAlpha = this.LightSource.OverrideLightSpriteAlpha;
								}
								break;
							}
							case 13:
								if (text == "damagedsprite")
								{
									this.DamagedSprite = new Sprite(CS$<>8__locals1.subElement, "", this.GetSpritePath(CS$<>8__locals1.subElement, this.Params.damagedSpriteParams, ref this._damagedTexturePath), false, sourceRectScale);
								}
								break;
							case 16:
								if (text == "deformablesprite")
								{
									ContentXElement subElement2 = CS$<>8__locals1.subElement;
									string spritePath = this.GetSpritePath(CS$<>8__locals1.subElement, this.Params.deformSpriteParams, ref this._texturePath);
									float sourceRectScale2 = sourceRectScale;
									this._deformSprite = new DeformableSprite(subElement2, null, null, spritePath, false, false, sourceRectScale2);
									IEnumerable<SpriteDeformation> deformations = this.<InitProjSpecific>g__CreateDeformations|276_1(CS$<>8__locals1.subElement);
									this.Deformations.AddRange(deformations);
									this.NonConditionalDeformations.AddRange(deformations);
								}
								break;
							case 17:
								if (text == "conditionalsprite")
								{
									string conditionalSpritePath = string.Empty;
									ContentXElement element2;
									if ((element2 = CS$<>8__locals1.subElement.GetChildElement("sprite")) == null)
									{
										element2 = (CS$<>8__locals1.subElement.GetChildElement("deformablesprite") ?? CS$<>8__locals1.subElement);
									}
									this.GetSpritePath(element2, null, ref conditionalSpritePath);
									if (conditionalSpritePath.IsNullOrEmpty())
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
										defaultInterpolatedStringHandler.AppendLiteral("Failed to find a sprite path in the conditional sprite defined in ");
										defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.character.SpeciesName);
										defaultInterpolatedStringHandler.AppendLiteral(", limb ");
										defaultInterpolatedStringHandler.AppendFormatted<LimbType>(this.type);
										defaultInterpolatedStringHandler.AppendLiteral(".");
										DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, CS$<>8__locals1.subElement.ContentPackage, false, false);
									}
									ConditionalSprite conditionalSprite = new ConditionalSprite(CS$<>8__locals1.subElement, this.<InitProjSpecific>g__GetConditionalTarget|276_0(ref CS$<>8__locals1), conditionalSpritePath, false, sourceRectScale);
									this.ConditionalSprites.Add(conditionalSprite);
									if (conditionalSprite.DeformableSprite != null)
									{
										IEnumerable<SpriteDeformation> conditionalDeformations = this.<InitProjSpecific>g__CreateDeformations|276_1(CS$<>8__locals1.subElement.GetChildElement("deformablesprite"));
										this.Deformations.AddRange(conditionalDeformations);
										this.ConditionalDeformations.Add(new ValueTuple<ConditionalSprite, IEnumerable<SpriteDeformation>>(conditionalSprite, conditionalDeformations));
									}
								}
								break;
							}
						}
						else
						{
							char c = text[0];
							if (c != 'h')
							{
								if (c == 't')
								{
									if (text == "tintmask")
									{
										ContentPath tintMaskPath = CS$<>8__locals1.subElement.GetAttributeContentPath("texture");
										if (!tintMaskPath.IsNullOrWhiteSpace())
										{
											this.TintMask = new Sprite(CS$<>8__locals1.subElement, "", this.GetSpritePath(tintMaskPath), false, sourceRectScale);
											this.TintHighlightThreshold = CS$<>8__locals1.subElement.GetAttributeFloat("highlightthreshold", 0.6f);
											this.TintHighlightMultiplier = CS$<>8__locals1.subElement.GetAttributeFloat("highlightmultiplier", 0.8f);
										}
									}
								}
							}
							else if (text == "huskmask")
							{
								ContentPath huskMaskPath = CS$<>8__locals1.subElement.GetAttributeContentPath("texture");
								if (!huskMaskPath.IsNullOrWhiteSpace())
								{
									this.HuskMask = new Sprite(CS$<>8__locals1.subElement, "", this.GetSpritePath(huskMaskPath), false, sourceRectScale);
								}
							}
						}
					}
					else if (text == "sprite")
					{
						this.Sprite = new Sprite(CS$<>8__locals1.subElement, "", this.GetSpritePath(CS$<>8__locals1.subElement, this.Params.normalSpriteParams, ref this._texturePath), false, sourceRectScale);
					}
				}
			}
			Sprite activeSprite = this.GetActiveSprite(true);
			this.DefaultSpriteDepth = ((activeSprite != null) ? activeSprite.Depth : 0f);
			LightSource lightSource = this.LightSource;
			if (lightSource == null)
			{
				return;
			}
			lightSource.CheckConditionals();
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x0004D5E8 File Offset: 0x0004B7E8
		public void MoveToPos(Vector2 pos, float force, bool pullFromCenter = false)
		{
			Vector2 pullPos = this.body.SimPosition;
			if (!pullFromCenter)
			{
				pullPos = this.pullJoint.WorldAnchorA;
			}
			this.AnimTargetPos = pos;
			this.body.MoveToPos(pos, force, new Vector2?(pullPos));
		}

		// Token: 0x060008B5 RID: 2229 RVA: 0x0004D62A File Offset: 0x0004B82A
		public void MirrorPullJoint()
		{
			this.pullJoint.LocalAnchorA = new Vector2(-this.pullJoint.LocalAnchorA.X, this.pullJoint.LocalAnchorA.Y);
		}

		// Token: 0x060008B6 RID: 2230 RVA: 0x0004D660 File Offset: 0x0004B860
		public AttackResult AddDamage(Vector2 simPosition, float damage, float bleedingDamage, float burnDamage, bool playSound)
		{
			List<Affliction> afflictions = new List<Affliction>();
			if (damage > 0f)
			{
				afflictions.Add(AfflictionPrefab.InternalDamage.Instantiate(damage, null));
			}
			if (bleedingDamage > 0f)
			{
				afflictions.Add(AfflictionPrefab.Bleeding.Instantiate(bleedingDamage, null));
			}
			if (burnDamage > 0f)
			{
				afflictions.Add(AfflictionPrefab.Burn.Instantiate(burnDamage, null));
			}
			return this.AddDamage(simPosition, afflictions, playSound, 1f, 0f, null);
		}

		// Token: 0x060008B7 RID: 2231 RVA: 0x0004D6D8 File Offset: 0x0004B8D8
		public AttackResult AddDamage(Vector2 simPosition, IEnumerable<Affliction> afflictions, bool playSound, float damageMultiplier = 1f, float penetration = 0f, Character attacker = null)
		{
			this.appliedDamageModifiers.Clear();
			this.afflictionsCopy.Clear();
			foreach (Affliction affliction in afflictions)
			{
				this.tempModifiers.Clear();
				Affliction newAffliction = affliction;
				float random = Rand.Value(Rand.RandSync.Unsynced);
				bool foundMatchingModifier = false;
				bool applyAffliction = true;
				foreach (DamageModifier damageModifier in this.DamageModifiers)
				{
					if (damageModifier.MatchesAffliction(affliction))
					{
						foundMatchingModifier = true;
						if (random > affliction.Probability * damageModifier.ProbabilityMultiplier)
						{
							applyAffliction = false;
						}
						else if (this.SectorHit(damageModifier.ArmorSectorInRadians, simPosition))
						{
							this.tempModifiers.Add(damageModifier);
						}
					}
				}
				foreach (WearableSprite wearable in this.WearingItems)
				{
					foreach (DamageModifier damageModifier2 in wearable.WearableComponent.DamageModifiers)
					{
						if (damageModifier2.MatchesAffliction(affliction))
						{
							foundMatchingModifier = true;
							if (random > affliction.Probability * damageModifier2.ProbabilityMultiplier)
							{
								applyAffliction = false;
							}
							else if (this.SectorHit(damageModifier2.ArmorSectorInRadians, simPosition))
							{
								this.tempModifiers.Add(damageModifier2);
							}
						}
					}
				}
				if (foundMatchingModifier || random <= affliction.Probability)
				{
					float finalDamageModifier = affliction.AffectedByAttackMultipliers ? damageMultiplier : 1f;
					if (this.character.EmpVulnerability > 0f && affliction.Prefab.AfflictionType == AfflictionPrefab.EMPType)
					{
						finalDamageModifier *= this.character.EmpVulnerability;
					}
					if (!this.character.Params.Health.PoisonImmunity && (affliction.Prefab.AfflictionType == AfflictionPrefab.PoisonType || affliction.Prefab.AfflictionType == AfflictionPrefab.ParalysisType))
					{
						finalDamageModifier *= this.character.PoisonVulnerability;
					}
					foreach (DamageModifier damageModifier3 in this.tempModifiers)
					{
						float damageModifierValue = damageModifier3.DamageMultiplier;
						if (damageModifier3.DeflectProjectiles && damageModifierValue < 1f)
						{
							damageModifierValue = MathHelper.Lerp(damageModifierValue, 1f, penetration);
						}
						finalDamageModifier *= damageModifierValue;
					}
					if (affliction.MultiplyByMaxVitality)
					{
						finalDamageModifier *= this.character.MaxVitality / 100f;
					}
					if (!MathUtils.NearlyEqual(finalDamageModifier, 1f, 0.0001f))
					{
						newAffliction = affliction.CreateMultiplied(finalDamageModifier, affliction);
					}
					else
					{
						newAffliction.SetStrength(affliction.NonClampedStrength);
					}
					if (attacker != null)
					{
						AbilityAfflictionCharacter abilityAfflictionCharacter = new AbilityAfflictionCharacter(newAffliction, this.character);
						attacker.CheckTalents(AbilityEffectType.OnAddDamageAffliction, abilityAfflictionCharacter);
						newAffliction = abilityAfflictionCharacter.Affliction;
					}
					if (applyAffliction)
					{
						this.afflictionsCopy.Add(newAffliction);
						Affliction affliction3 = newAffliction;
						if (affliction3.Source == null)
						{
							affliction3.Source = attacker;
						}
					}
					this.appliedDamageModifiers.AddRange(this.tempModifiers);
				}
			}
			AttackResult result = new AttackResult(this.afflictionsCopy, this, this.appliedDamageModifiers);
			if (result.Afflictions.None(null))
			{
				playSound = false;
			}
			this.AddDamageProjSpecific(playSound, result);
			float bleedingDamage = 0f;
			if (this.character.CharacterHealth.DoesBleed)
			{
				foreach (Affliction affliction2 in result.Afflictions)
				{
					if (affliction2 is AfflictionBleeding)
					{
						bleedingDamage += affliction2.GetVitalityDecrease(this.character.CharacterHealth);
					}
				}
				if (bleedingDamage > 0f)
				{
					float bloodDecalSize = MathHelper.Clamp(bleedingDamage / 5f, 0.1f, 1f);
					if (this.character.CurrentHull != null && !string.IsNullOrEmpty(this.character.BloodDecalName))
					{
						this.character.CurrentHull.AddDecal(this.character.BloodDecalName, this.WorldPosition, MathHelper.Clamp(bloodDecalSize, 0.5f, 1f), false, null);
					}
				}
			}
			return result;
		}

		// Token: 0x060008B8 RID: 2232 RVA: 0x0004DBC8 File Offset: 0x0004BDC8
		private void AddDamageProjSpecific(bool playSound, AttackResult result)
		{
			float bleedingDamage = 0f;
			if (this.character.CharacterHealth.DoesBleed)
			{
				foreach (Affliction affliction in result.Afflictions)
				{
					AfflictionBleeding bleeding = affliction as AfflictionBleeding;
					if (bleeding != null && bleeding.Prefab.DamageParticles)
					{
						bleedingDamage += affliction.GetVitalityDecrease(null);
					}
				}
			}
			float damage = 0f;
			foreach (Affliction affliction2 in result.Afflictions)
			{
				if (affliction2.Prefab.DamageParticles && affliction2.Prefab.AfflictionType == AfflictionPrefab.DamageType)
				{
					damage += affliction2.GetVitalityDecrease(null);
				}
			}
			float damageMultiplier = 1f;
			float bleedingDamageMultiplier = 1f;
			foreach (DamageModifier damageModifier in result.AppliedDamageModifiers)
			{
				if (damageModifier.MatchesAfflictionType(AfflictionPrefab.DamageType))
				{
					damageMultiplier *= damageModifier.DamageMultiplier;
				}
				else if (damageModifier.MatchesAfflictionType(AfflictionPrefab.BleedingType))
				{
					bleedingDamageMultiplier *= damageModifier.DamageMultiplier;
				}
			}
			if (playSound)
			{
				string damageSoundType = (bleedingDamage > damage) ? "LimbSlash" : "LimbBlunt";
				foreach (DamageModifier damageModifier2 in result.AppliedDamageModifiers)
				{
					if (!string.IsNullOrWhiteSpace(damageModifier2.DamageSound))
					{
						damageSoundType = damageModifier2.DamageSound;
						break;
					}
				}
				SoundPlayer.PlayDamageSound(damageSoundType, Math.Max(damage, bleedingDamage), this.WorldPosition, 2000f, null, 1f);
			}
			if (this.character.InvisibleTimer > 0f)
			{
				return;
			}
			float damageParticleAmount = (damage < 1f) ? 0f : (Math.Min(damage / 5f, 1f) * damageMultiplier);
			if (damageParticleAmount > 0.001f)
			{
				foreach (ParticleEmitter emitter in this.character.DamageEmitters)
				{
					if (((emitter != null) ? emitter.Prefab : null) != null && (!this.InWater || emitter.Prefab.ParticlePrefab.DrawTarget != ParticlePrefab.DrawTargetType.Air) && (this.InWater || emitter.Prefab.ParticlePrefab.DrawTarget != ParticlePrefab.DrawTargetType.Water))
					{
						ParticlePrefab overrideParticle = null;
						foreach (DamageModifier damageModifier3 in result.AppliedDamageModifiers)
						{
							if (damageModifier3.DamageMultiplier > 0f && !string.IsNullOrWhiteSpace(damageModifier3.DamageParticle))
							{
								overrideParticle = ParticleManager.FindPrefab(damageModifier3.DamageParticle);
								break;
							}
						}
						ParticleEmitter particleEmitter = emitter;
						float deltaTime = 1f;
						Vector2 worldPosition = this.WorldPosition;
						Hull currentHull = this.character.CurrentHull;
						float angle = 0f;
						float particleRotation = 0f;
						float velocityMultiplier = 1f;
						float sizeMultiplier = 1f;
						float amountMultiplier = damageParticleAmount;
						ParticlePrefab overrideParticle2 = overrideParticle;
						particleEmitter.Emit(deltaTime, worldPosition, currentHull, angle, particleRotation, velocityMultiplier, sizeMultiplier, amountMultiplier, null, overrideParticle2, false, null);
					}
				}
			}
			if (bleedingDamage > 0f)
			{
				float bloodParticleAmount = Math.Min(bleedingDamage / 5f, 1f) * bleedingDamageMultiplier;
				float bloodParticleSize = MathHelper.Clamp(bleedingDamage / 5f, 0.1f, 1f);
				foreach (ParticleEmitter emitter2 in this.character.BloodEmitters)
				{
					if (((emitter2 != null) ? emitter2.Prefab : null) != null && (!this.InWater || emitter2.Prefab.ParticlePrefab.DrawTarget != ParticlePrefab.DrawTargetType.Air) && (this.InWater || emitter2.Prefab.ParticlePrefab.DrawTarget != ParticlePrefab.DrawTargetType.Water))
					{
						emitter2.Emit(1f, this.WorldPosition, this.character.CurrentHull, 0f, 0f, 1f, bloodParticleSize, bloodParticleAmount, null, null, false, null);
					}
				}
			}
		}

		// Token: 0x060008B9 RID: 2233 RVA: 0x0004E0B0 File Offset: 0x0004C2B0
		public bool SectorHit(Vector2 armorSector, Vector2 simPosition)
		{
			if (armorSector == Vector2.Zero)
			{
				return false;
			}
			if (Math.Abs(armorSector.Y - armorSector.X) >= 6.2831855f)
			{
				return true;
			}
			float rotation = this.body.TransformedRotation;
			float offset = (1.5707964f - MathUtils.GetMidAngle(armorSector.X, armorSector.Y)) * this.Dir;
			float hitAngle = VectorExtensions.Forward(rotation + offset, 1f).Angle(this.SimPosition - simPosition);
			float sectorSize = this.GetArmorSectorSize(armorSector);
			return hitAngle < sectorSize / 2f;
		}

		// Token: 0x060008BA RID: 2234 RVA: 0x0004E143 File Offset: 0x0004C343
		protected float GetArmorSectorSize(Vector2 armorSector)
		{
			return Math.Abs(armorSector.X - armorSector.Y);
		}

		// Token: 0x060008BB RID: 2235 RVA: 0x0004E158 File Offset: 0x0004C358
		public void Update(float deltaTime)
		{
			this.UpdateProjSpecific(deltaTime);
			this.ApplyStatusEffects(ActionType.Always, deltaTime);
			this.ApplyStatusEffects(ActionType.OnActive, deltaTime);
			if (this.InWater)
			{
				this.body.ApplyWaterForces();
			}
			if (this.isSevered)
			{
				this.severedFadeOutTimer += deltaTime;
				if (this.severedFadeOutTimer >= this.SeveredFadeOutTime)
				{
					this.body.Enabled = false;
				}
				else if (this.character.CurrentHull == null && Hull.FindHull(this.WorldPosition, null, true, true) != null)
				{
					this.severedFadeOutTimer = this.SeveredFadeOutTime;
				}
			}
			else if (!this.IsDead && (this.character.IsPlayer || this.character.AIState != AIState.PlayDead))
			{
				if (this.Params.BlinkFrequency > 0f)
				{
					if (this.BlinkTimer > -this.TotalBlinkDurationOut)
					{
						this.BlinkTimer -= deltaTime;
					}
					else
					{
						this.BlinkTimer = this.Params.BlinkFrequency;
					}
				}
				if (this.reEnableTimer > 0f)
				{
					this.reEnableTimer -= deltaTime;
				}
				else if (this.reEnableTimer > -1f)
				{
					this.ReEnable();
				}
			}
			Attack attack = this.attack;
			if (attack == null)
			{
				return;
			}
			attack.UpdateCoolDown(deltaTime);
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x0004E2A4 File Offset: 0x0004C4A4
		public void HideAndDisable(float duration = 0f, bool ignoreCollisions = true)
		{
			if (this.Hidden || this.Disabled)
			{
				return;
			}
			this.temporarilyDisabled = true;
			this.Hidden = true;
			this.Disabled = true;
			this.originalIgnoreCollisions = this.IgnoreCollisions;
			this.IgnoreCollisions = ignoreCollisions;
			if (duration > 0f)
			{
				this.reEnableTimer = duration;
			}
			if (this.Hidden && this.LightSource != null)
			{
				this.LightSource.Enabled = false;
			}
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x0004E315 File Offset: 0x0004C515
		public void ReEnable()
		{
			if (!this.temporarilyDisabled)
			{
				return;
			}
			this.temporarilyDisabled = false;
			this.Hidden = false;
			this.Disabled = false;
			this.IgnoreCollisions = this.originalIgnoreCollisions;
			this.reEnableTimer = -1f;
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x0004E34C File Offset: 0x0004C54C
		private void UpdateProjSpecific(float deltaTime)
		{
			if (!this.body.Enabled)
			{
				return;
			}
			if (this.IsDead)
			{
				RagdollParams.SpriteParams spriteParams = this.Params.GetSprite();
				if (spriteParams != null && spriteParams.DeadColorTime > 0f && this.deadTimer < spriteParams.DeadColorTime)
				{
					this.deadTimer += deltaTime;
				}
			}
			if (this.InWater)
			{
				this.wetTimer = 1f;
			}
			else
			{
				this.wetTimer -= deltaTime * 0.1f;
				if (this.wetTimer > 0f)
				{
					this.dripParticleTimer += this.wetTimer * deltaTime * this.Mass * ((this.wetTimer > 0.9f) ? 50f : 5f);
					if (this.dripParticleTimer > 1f)
					{
						float dropRadius = (this.body.BodyShape == PhysicsBody.Shape.Rectangle) ? Math.Min(this.body.Width, this.body.Height) : this.body.Radius;
						GameMain.ParticleManager.CreateParticle("waterdrop", this.WorldPosition + Rand.Vector(Rand.Range(0f, ConvertUnits.ToDisplayUnits(dropRadius), Rand.RandSync.Unsynced), Rand.RandSync.Unsynced), ConvertUnits.ToDisplayUnits(this.body.LinearVelocity), 0f, this.character.CurrentHull, 0f, null);
						this.dripParticleTimer = 0f;
					}
				}
			}
			foreach (ConditionalSprite conditionalSprite in this.ConditionalSprites)
			{
				conditionalSprite.CheckConditionals();
			}
			if (this.LightSource != null)
			{
				this.LightSource.ParentSub = this.body.Submarine;
				this.LightSource.Rotation = ((this.dir == Direction.Right) ? this.body.Rotation : (this.body.Rotation - 3.1415927f));
				if (this.LightSource.LightSprite != null)
				{
					this.LightSource.LightSprite.Depth = this.ActiveSprite.Depth;
				}
				if (this.LightSource.DeformableLightSprite != null)
				{
					this.LightSource.DeformableLightSprite.Sprite.Depth = this.ActiveSprite.Depth;
				}
				this.LightSource.CheckConditionals();
			}
			this.UpdateSpriteStates(deltaTime);
			this.RefreshDeformations();
		}

		// Token: 0x060008BF RID: 2239 RVA: 0x0004E5C8 File Offset: 0x0004C7C8
		public bool UpdateAttack(float deltaTime, Vector2 attackSimPos, IDamageable damageTarget, out AttackResult attackResult, float distance = -1f, Limb targetLimb = null)
		{
			attackResult = default(AttackResult);
			Vector2 simPos = this.ragdoll.SimplePhysicsEnabled ? this.character.SimPosition : this.SimPosition;
			float dist = (distance > -1f) ? distance : ConvertUnits.ToDisplayUnits(Vector2.Distance(simPos, attackSimPos));
			bool wasRunning = this.attack.IsRunning;
			this.attack.UpdateAttackTimer(deltaTime, this.character);
			if (this.attack.Blink)
			{
				if (this.attack.ForceOnLimbIndices != null && this.attack.ForceOnLimbIndices.Any<int>())
				{
					using (List<int>.Enumerator enumerator = this.attack.ForceOnLimbIndices.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							int limbIndex = enumerator.Current;
							if (limbIndex >= 0 && limbIndex < this.character.AnimController.Limbs.Length)
							{
								Limb limb = this.character.AnimController.Limbs[limbIndex];
								if (!limb.IsSevered)
								{
									limb.Blink();
								}
							}
						}
						goto IL_10F;
					}
				}
				this.Blink();
			}
			IL_10F:
			bool wasHit = false;
			if (damageTarget != null)
			{
				HitDetection hitDetectionType = this.attack.HitDetectionType;
				if (hitDetectionType != HitDetection.Distance)
				{
					if (hitDetectionType == HitDetection.Contact)
					{
						this.contactBodies.Clear();
						Character targetCharacter = damageTarget as Character;
						if (targetCharacter != null)
						{
							foreach (Limb limb2 in targetCharacter.AnimController.Limbs)
							{
								if (!limb2.IsSevered)
								{
									PhysicsBody physicsBody = limb2.body;
									if (((physicsBody != null) ? physicsBody.FarseerBody : null) != null)
									{
										this.contactBodies.Add(limb2.body.FarseerBody);
									}
								}
							}
						}
						else
						{
							Structure targetStructure = damageTarget as Structure;
							if (targetStructure != null)
							{
								if (this.character.Submarine == null && targetStructure.Submarine != null)
								{
									this.contactBodies.Add(targetStructure.Submarine.PhysicsBody.FarseerBody);
								}
								else
								{
									this.contactBodies.AddRange(targetStructure.Bodies);
								}
							}
							else if (damageTarget is Item)
							{
								Item targetItem = damageTarget as Item;
								PhysicsBody physicsBody2 = targetItem.body;
								if (((physicsBody2 != null) ? physicsBody2.FarseerBody : null) != null)
								{
									this.contactBodies.Add(targetItem.body.FarseerBody);
								}
							}
						}
						ContactEdge contactEdge;
						for (contactEdge = this.body.FarseerBody.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
						{
							if (contactEdge.Contact != null && contactEdge.Contact.IsTouching && this.contactBodies.Any(delegate(Body b)
							{
								Fixture fixtureA = contactEdge.Contact.FixtureA;
								if (b != ((fixtureA != null) ? fixtureA.Body : null))
								{
									Fixture fixtureB = contactEdge.Contact.FixtureB;
									return b == ((fixtureB != null) ? fixtureB.Body : null);
								}
								return true;
							}))
							{
								Body structureBody = this.contactBodies.LastOrDefault<Body>();
								wasHit = true;
								break;
							}
						}
					}
				}
				else if (dist < this.attack.DamageRange)
				{
					Vector2 rayStart = simPos;
					Vector2 rayEnd = attackSimPos;
					if (this.Submarine == null)
					{
						ISpatialEntity spatialEntity = damageTarget as ISpatialEntity;
						if (spatialEntity != null && spatialEntity.Submarine != null)
						{
							rayStart -= spatialEntity.Submarine.SimPosition;
							rayEnd -= spatialEntity.Submarine.SimPosition;
						}
					}
					Body structureBody = Submarine.CheckVisibility(rayStart, rayEnd, false, false, true, true, true, null);
					Item i = damageTarget as Item;
					if (i != null && i.GetComponent<Door>() != null)
					{
						wasHit = true;
					}
					else
					{
						Structure wall = damageTarget as Structure;
						if (wall != null && structureBody != null)
						{
							if (!(structureBody.UserData is Structure))
							{
								Submarine sub = structureBody.UserData as Submarine;
								if (sub == null || sub != wall.Submarine)
								{
									goto IL_20C;
								}
							}
							wasHit = true;
							goto IL_3A1;
						}
						IL_20C:
						wasHit = (structureBody == null);
					}
				}
			}
			IL_3A1:
			if (wasHit)
			{
				wasHit = (damageTarget != null);
			}
			if ((wasHit || this.attack.HitDetectionType == HitDetection.None) && (this.character == Character.Controlled || GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient))
			{
				this.ExecuteAttack(damageTarget, targetLimb, out attackResult);
			}
			Vector2 diff = attackSimPos - this.SimPosition;
			bool applyForces = !this.attack.ApplyForcesOnlyOnce || !wasRunning;
			if (applyForces)
			{
				if (this.attack.ForceOnLimbIndices != null && this.attack.ForceOnLimbIndices.Count > 0)
				{
					using (List<int>.Enumerator enumerator2 = this.attack.ForceOnLimbIndices.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							int limbIndex2 = enumerator2.Current;
							if (limbIndex2 >= 0 && limbIndex2 < this.character.AnimController.Limbs.Length)
							{
								Limb limb3 = this.character.AnimController.Limbs[limbIndex2];
								if (!limb3.IsSevered)
								{
									diff = attackSimPos - limb3.SimPosition;
									if (!(diff == Vector2.Zero))
									{
										limb3.body.ApplyTorque(limb3.Mass * this.character.AnimController.Dir * this.attack.Torque * limb3.Params.AttackForceMultiplier);
										Vector2 forcePos = (limb3.pullJoint == null) ? limb3.body.SimPosition : limb3.pullJoint.WorldAnchorA;
										limb3.body.ApplyLinearImpulse(limb3.Mass * this.attack.Force * limb3.Params.AttackForceMultiplier * Vector2.Normalize(diff), forcePos, 64f);
									}
								}
							}
						}
						goto IL_621;
					}
				}
				if (diff != Vector2.Zero)
				{
					this.body.ApplyTorque(this.Mass * this.character.AnimController.Dir * this.attack.Torque * this.Params.AttackForceMultiplier);
					Vector2 forcePos2 = (this.pullJoint == null) ? this.body.SimPosition : this.pullJoint.WorldAnchorA;
					this.body.ApplyLinearImpulse(this.Mass * this.attack.Force * this.Params.AttackForceMultiplier * Vector2.Normalize(diff), forcePos2, 64f);
				}
			}
			IL_621:
			Vector2 forceWorld = this.attack.CalculateAttackPhase(this.attack.RootTransitionEasing);
			forceWorld.X *= this.character.AnimController.Dir;
			this.character.AnimController.MainLimb.body.ApplyLinearImpulse(this.character.Mass * forceWorld, this.character.SimPosition, 64f);
			if (!this.attack.IsRunning && !this.attack.Ranged && Vector2.DistanceSquared(this.character.AnimController.Collider.SimPosition, this.character.AnimController.MainLimb.body.SimPosition) > 0.010000001f)
			{
				this.character.AnimController.Collider.SetTransformIgnoreContacts(this.character.AnimController.MainLimb.body.SimPosition, this.character.AnimController.Collider.Rotation, true);
			}
			return wasHit;
		}

		// Token: 0x060008C0 RID: 2240 RVA: 0x0004ED3C File Offset: 0x0004CF3C
		public void ExecuteAttack(IDamageable damageTarget, Limb targetLimb, out AttackResult attackResult)
		{
			bool playSound = (double)this.LastAttackSoundTime < Timing.TotalTime - 0.4000000059604645;
			if (playSound)
			{
				this.LastAttackSoundTime = 0.4f;
			}
			this.attack.ResetDamageMultiplier();
			this.attack.DamageMultiplier *= 1f + this.character.GetStatValue(this.attack.Ranged ? StatTypes.NaturalRangedAttackMultiplier : StatTypes.NaturalMeleeAttackMultiplier, true);
			if (damageTarget is Character && targetLimb != null)
			{
				attackResult = this.attack.DoDamageToLimb(this.character, targetLimb, this.WorldPosition, 1f, playSound, this.body, this);
			}
			else
			{
				Item targetItem = damageTarget as Item;
				if (targetItem != null && !targetItem.Prefab.DamagedByMonsters)
				{
					attackResult = default(AttackResult);
				}
				else
				{
					attackResult = this.attack.DoDamage(this.character, damageTarget, this.WorldPosition, 1f, playSound, this.body, this);
				}
			}
			this.attack.ResetAttackTimer();
			this.attack.SetCoolDown(!this.character.IsPlayer);
		}

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x060008C1 RID: 2241 RVA: 0x0004EE5B File Offset: 0x0004D05B
		public bool IsStuck
		{
			get
			{
				return this.attachJoint != null;
			}
		}

		// Token: 0x060008C2 RID: 2242 RVA: 0x0004EE68 File Offset: 0x0004D068
		private void StickTo(Body target, Vector2 from, Vector2 to)
		{
			if (this.attachJoint != null)
			{
				if (this.attachJoint.BodyB == target)
				{
					return;
				}
				this.Release();
			}
			if (!this.ragdoll.IsStuck)
			{
				PhysicsBody mainLimbBody = this.ragdoll.MainLimb.body;
				Body colliderBody = this.ragdoll.Collider.FarseerBody;
				Vector2 mainLimbLocalFront = mainLimbBody.GetLocalFront(new float?(this.ragdoll.MainLimb.Params.GetSpriteOrientation()));
				if (this.Dir < 0f)
				{
					mainLimbLocalFront.X = -mainLimbLocalFront.X;
				}
				Vector2 mainLimbFront = mainLimbBody.FarseerBody.GetWorldPoint(mainLimbLocalFront);
				colliderBody.SetTransform(mainLimbBody.SimPosition, mainLimbBody.Rotation);
				this.colliderJoint = new WeldJoint(colliderBody, mainLimbBody.FarseerBody, mainLimbFront, mainLimbFront, true)
				{
					KinematicBodyB = true,
					CollideConnected = false
				};
				GameMain.World.Add(this.colliderJoint);
			}
			this.attachJoint = new WeldJoint(this.body.FarseerBody, target, from, to, true)
			{
				FrequencyHz = 1f,
				DampingRatio = 0.5f,
				KinematicBodyB = true,
				CollideConnected = false
			};
			GameMain.World.Add(this.attachJoint);
		}

		// Token: 0x060008C3 RID: 2243 RVA: 0x0004EFA0 File Offset: 0x0004D1A0
		public void Release()
		{
			if (!this.IsStuck)
			{
				return;
			}
			GameMain.World.Remove(this.attachJoint);
			this.attachJoint = null;
			if (this.colliderJoint != null)
			{
				GameMain.World.Remove(this.colliderJoint);
				this.colliderJoint = null;
			}
		}

		// Token: 0x060008C4 RID: 2244 RVA: 0x0004EFEC File Offset: 0x0004D1EC
		public void ApplyStatusEffects(ActionType actionType, float deltaTime)
		{
			List<StatusEffect> statusEffectList;
			if (!this.statusEffects.TryGetValue(actionType, out statusEffectList))
			{
				return;
			}
			foreach (StatusEffect statusEffect in statusEffectList)
			{
				if (!statusEffect.ShouldWaitForInterval(this.character, deltaTime))
				{
					statusEffect.sourceBody = this.body;
					if (statusEffect.type != ActionType.OnDamaged || (statusEffect.HasRequiredAfflictions(this.character.LastDamage) && (!statusEffect.OnlyWhenDamagedByPlayer || (this.character.LastAttacker != null && this.character.LastAttacker.IsPlayer))))
					{
						if (statusEffect.HasTargetType(StatusEffect.TargetType.NearbyItems) || statusEffect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
						{
							this.targets.Clear();
							statusEffect.AddNearbyTargets(this.WorldPosition, this.targets);
							statusEffect.Apply(actionType, deltaTime, this.character, this.targets, null);
						}
						else if (statusEffect.targetLimbs != null)
						{
							LimbType[] targetLimbs = statusEffect.targetLimbs;
							for (int i = 0; i < targetLimbs.Length; i++)
							{
								LimbType limbType = targetLimbs[i];
								if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
								{
									foreach (Limb limb in this.ragdoll.Limbs)
									{
										if (!limb.IsSevered && limb.type == limbType)
										{
											Limb.<ApplyStatusEffects>g__ApplyToLimb|304_0(actionType, deltaTime, statusEffect, this.character, limb);
										}
									}
								}
								else if (statusEffect.HasTargetType(StatusEffect.TargetType.Limb) || statusEffect.HasTargetType(StatusEffect.TargetType.Character) || statusEffect.HasTargetType(StatusEffect.TargetType.This))
								{
									Limb limb2 = this.ragdoll.GetLimb(limbType, true, false, false);
									if (limb2 != null)
									{
										Limb.<ApplyStatusEffects>g__ApplyToLimb|304_0(actionType, deltaTime, statusEffect, this.character, limb2);
									}
								}
								else if (statusEffect.HasTargetType(StatusEffect.TargetType.LastLimb))
								{
									Limb limb3 = this.ragdoll.Limbs.LastOrDefault((Limb l) => l.type == limbType && !l.IsSevered && !l.Hidden);
									if (limb3 != null)
									{
										Limb.<ApplyStatusEffects>g__ApplyToLimb|304_0(actionType, deltaTime, statusEffect, this.character, limb3);
									}
								}
							}
						}
						else if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
						{
							foreach (Limb limb4 in this.ragdoll.Limbs)
							{
								if (!limb4.IsSevered)
								{
									Limb.<ApplyStatusEffects>g__ApplyToLimb|304_0(actionType, deltaTime, statusEffect, this.character, limb4);
								}
							}
						}
						else if (statusEffect.HasTargetType(StatusEffect.TargetType.Character))
						{
							statusEffect.Apply(actionType, deltaTime, this.character, this.character, new Vector2?(this.WorldPosition));
						}
						else if (statusEffect.HasTargetType(StatusEffect.TargetType.This) || statusEffect.HasTargetType(StatusEffect.TargetType.Limb))
						{
							Limb.<ApplyStatusEffects>g__ApplyToLimb|304_0(actionType, deltaTime, statusEffect, this.character, this);
						}
					}
				}
			}
		}

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x060008C5 RID: 2245 RVA: 0x0004F2E4 File Offset: 0x0004D4E4
		// (set) Token: 0x060008C6 RID: 2246 RVA: 0x0004F2EC File Offset: 0x0004D4EC
		public float BlinkTimer { get; private set; }

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x060008C7 RID: 2247 RVA: 0x0004F2F5 File Offset: 0x0004D4F5
		// (set) Token: 0x060008C8 RID: 2248 RVA: 0x0004F2FD File Offset: 0x0004D4FD
		public float BlinkPhase { get; set; }

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x060008C9 RID: 2249 RVA: 0x0004F306 File Offset: 0x0004D506
		private float TotalBlinkDurationOut
		{
			get
			{
				return this.Params.BlinkDurationOut + this.Params.BlinkHoldTime;
			}
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x0004F31F File Offset: 0x0004D51F
		public void Blink()
		{
			this.BlinkTimer = -this.TotalBlinkDurationOut;
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x0004F330 File Offset: 0x0004D530
		public void UpdateBlink(float deltaTime, float referenceRotation)
		{
			if (this.BlinkTimer > -this.TotalBlinkDurationOut)
			{
				if (!this.FreezeBlinkState)
				{
					this.BlinkPhase -= deltaTime;
				}
				if (this.BlinkPhase > 0f)
				{
					float t = ToolBox.GetEasing(this.Params.BlinkTransitionIn, MathUtils.InverseLerp(1f, 0f, this.BlinkPhase / this.Params.BlinkDurationIn));
					this.body.SmoothRotate(referenceRotation + MathHelper.ToRadians(this.Params.BlinkRotationIn) * this.Dir, this.Mass * this.Params.BlinkForce * t, true);
					if (this.Params.UseTextureOffsetForBlinking)
					{
						this.ActiveSprite.RelativeOrigin = Vector2.Lerp(this.Params.BlinkTextureOffsetOut, this.Params.BlinkTextureOffsetIn, t);
						return;
					}
				}
				else
				{
					if (Math.Abs(this.BlinkPhase) < this.Params.BlinkHoldTime)
					{
						this.body.SmoothRotate(referenceRotation + MathHelper.ToRadians(this.Params.BlinkRotationIn) * this.Dir, this.Mass * this.Params.BlinkForce, true);
						return;
					}
					float t2 = ToolBox.GetEasing(this.Params.BlinkTransitionOut, MathUtils.InverseLerp(0f, 1f, (-this.BlinkPhase - this.Params.BlinkHoldTime) / this.Params.BlinkDurationOut));
					this.body.SmoothRotate(referenceRotation + MathHelper.ToRadians(this.Params.BlinkRotationOut) * this.Dir, this.Mass * this.Params.BlinkForce * t2, true);
					if (this.Params.UseTextureOffsetForBlinking)
					{
						this.ActiveSprite.RelativeOrigin = Vector2.Lerp(this.Params.BlinkTextureOffsetIn, this.Params.BlinkTextureOffsetOut, t2);
						return;
					}
				}
			}
			else
			{
				if (!this.FreezeBlinkState)
				{
					this.BlinkPhase = this.Params.BlinkDurationIn;
				}
				this.body.SmoothRotate(referenceRotation + MathHelper.ToRadians(this.Params.BlinkRotationOut) * this.Dir, this.Mass * this.Params.BlinkForce, true);
			}
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x0004F565 File Offset: 0x0004D765
		public IEnumerable<LimbJoint> GetConnectedJoints()
		{
			return from j in this.ragdoll.LimbJoints
			where !j.IsSevered && (j.LimbA == this || j.LimbB == this)
			select j;
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x0004F584 File Offset: 0x0004D784
		public IEnumerable<Limb> GetConnectedLimbs()
		{
			IEnumerable<LimbJoint> connectedJoints = this.GetConnectedJoints();
			HashSet<Limb> connectedLimbs = new HashSet<Limb>();
			foreach (Limb limb in this.ragdoll.Limbs)
			{
				IEnumerable<LimbJoint> otherJoints = limb.GetConnectedJoints();
				foreach (LimbJoint connectedJoint in connectedJoints)
				{
					if (otherJoints.Contains(connectedJoint))
					{
						connectedLimbs.Add(limb);
					}
				}
			}
			return connectedLimbs;
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x0004F618 File Offset: 0x0004D818
		public void Remove()
		{
			this.ragdoll.SubtractMass(this);
			PhysicsBody physicsBody = this.body;
			if (physicsBody != null)
			{
				physicsBody.Remove();
			}
			this.body = null;
			if (this.pullJoint != null)
			{
				if (GameMain.World.JointList.Contains(this.pullJoint))
				{
					GameMain.World.Remove(this.pullJoint);
				}
				this.pullJoint = null;
			}
			this.Release();
			this.RemoveProjSpecific();
			this.Removed = true;
		}

		// Token: 0x060008CF RID: 2255 RVA: 0x0004F694 File Offset: 0x0004D894
		private void RemoveProjSpecific()
		{
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.Sprite = null;
			Sprite damagedSprite = this.DamagedSprite;
			if (damagedSprite != null)
			{
				damagedSprite.Remove();
			}
			this.DamagedSprite = null;
			DeformableSprite deformSprite = this._deformSprite;
			if (deformSprite != null)
			{
				Sprite sprite2 = deformSprite.Sprite;
				if (sprite2 != null)
				{
					sprite2.Remove();
				}
			}
			this._deformSprite = null;
			this.DecorativeSprites.ForEach(delegate(DecorativeSprite s)
			{
				s.Remove();
			});
			this.ConditionalSprites.Clear();
			this.ConditionalSprites.ForEach(delegate(ConditionalSprite s)
			{
				s.Remove();
			});
			this.ConditionalSprites.Clear();
			LightSource lightSource = this.LightSource;
			if (lightSource != null)
			{
				lightSource.Remove();
			}
			this.LightSource = null;
			this.OtherWearables.ForEach(delegate(WearableSprite w)
			{
				w.Sprite.Remove();
			});
			this.OtherWearables.Clear();
			WearableSprite huskSprite = this.HuskSprite;
			if (huskSprite != null)
			{
				Sprite sprite3 = huskSprite.Sprite;
				if (sprite3 != null)
				{
					sprite3.Remove();
				}
			}
			this.HuskSprite = null;
			WearableSprite hairWithHatSprite = this.HairWithHatSprite;
			if (hairWithHatSprite != null)
			{
				Sprite sprite4 = hairWithHatSprite.Sprite;
				if (sprite4 != null)
				{
					sprite4.Remove();
				}
			}
			this.HairWithHatSprite = null;
			WearableSprite herpesSprite = this.HerpesSprite;
			if (herpesSprite != null)
			{
				Sprite sprite5 = herpesSprite.Sprite;
				if (sprite5 != null)
				{
					sprite5.Remove();
				}
			}
			this.HerpesSprite = null;
			Sprite tintMask = this.TintMask;
			if (tintMask != null)
			{
				tintMask.Remove();
			}
			this.TintMask = null;
		}

		// Token: 0x060008D0 RID: 2256 RVA: 0x0004F82C File Offset: 0x0004DA2C
		public void LoadParams()
		{
			this.pullJoint.LocalAnchorA = ConvertUnits.ToSimUnits(this.Params.PullPos * this.Scale);
			this.LoadParamsProjSpecific();
		}

		// Token: 0x060008D1 RID: 2257 RVA: 0x0004F85C File Offset: 0x0004DA5C
		private void LoadParamsProjSpecific()
		{
			bool isFlipped = this.dir == Direction.Left;
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.LoadParams(this.Params.normalSpriteParams, isFlipped);
			}
			Sprite damagedSprite = this.DamagedSprite;
			if (damagedSprite != null)
			{
				damagedSprite.LoadParams(this.Params.damagedSpriteParams, isFlipped);
			}
			DeformableSprite deformSprite = this._deformSprite;
			if (deformSprite != null)
			{
				deformSprite.Sprite.LoadParams(this.Params.deformSpriteParams, isFlipped);
			}
			for (int i = 0; i < this.DecorativeSprites.Count; i++)
			{
				Sprite sprite2 = this.DecorativeSprites[i].Sprite;
				if (sprite2 != null)
				{
					sprite2.LoadParams(this.Params.decorativeSpriteParams[i], isFlipped);
				}
			}
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x0004F914 File Offset: 0x0004DB14
		[CompilerGenerated]
		internal static bool <Draw>g__ShouldHideLimb|122_0(Limb limb)
		{
			if (limb.Hide)
			{
				return true;
			}
			foreach (WearableSprite wearable in limb.OtherWearables)
			{
				if (wearable.HideLimb)
				{
					return true;
				}
			}
			foreach (WearableSprite wearable2 in limb.WearingItems)
			{
				if (wearable2.HideLimb)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x0004F9C4 File Offset: 0x0004DBC4
		[CompilerGenerated]
		private void <UpdateWearableTypesToHide>g__addWearablesFrom|123_0(IReadOnlyList<WearableSprite> wearableSprites)
		{
			if (wearableSprites.Count <= 0)
			{
				return;
			}
			this.wearableTypeHidingSprites.AddRange(from w in wearableSprites
			where w.HideWearablesOfType.Count > 0
			select w);
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x0004FA00 File Offset: 0x0004DC00
		[CompilerGenerated]
		private SpriteRecorder.Command <ApplyAlphaClip>g__makeCommand|129_0(WearableSprite w, ref Limb.<>c__DisplayClass129_0 A_2)
		{
			ValueTuple<Color, Vector2, float, float, float> valueTuple = this.CalculateDrawParameters(w, 0f, Color.White, 0f);
			Vector2 origin = valueTuple.Item2;
			float rotation = valueTuple.Item3;
			float scale = valueTuple.Item4;
			return SpriteRecorder.Command.FromTransform(w.Sprite.Texture, new Vector2(this.body.DrawPosition.X, -this.body.DrawPosition.Y), w.Sprite.SourceRect, Color.White, rotation, origin, new Vector2(scale, scale), A_2.spriteEffect, 0f, 0);
		}

		// Token: 0x060008D5 RID: 2261 RVA: 0x0004FA94 File Offset: 0x0004DC94
		[CompilerGenerated]
		private void <ApplyAlphaClip>g__spacesFromCommand|129_1(WearableSprite w, SpriteRecorder.Command command, out CoordinateSpace2D textureSpace, out CoordinateSpace2D worldSpace, ref Limb.<>c__DisplayClass129_0 A_5)
		{
			ValueTuple<VertexPositionColorTexture, VertexPositionColorTexture, VertexPositionColorTexture> valueTuple;
			switch (A_5.spriteEffect)
			{
			case SpriteEffects.None:
				valueTuple = new ValueTuple<VertexPositionColorTexture, VertexPositionColorTexture, VertexPositionColorTexture>(command.VertexTL, command.VertexBL, command.VertexTR);
				break;
			case SpriteEffects.FlipHorizontally:
				valueTuple = new ValueTuple<VertexPositionColorTexture, VertexPositionColorTexture, VertexPositionColorTexture>(command.VertexTR, command.VertexBR, command.VertexTL);
				break;
			case SpriteEffects.FlipVertically:
				valueTuple = new ValueTuple<VertexPositionColorTexture, VertexPositionColorTexture, VertexPositionColorTexture>(command.VertexBL, command.VertexTL, command.VertexBR);
				break;
			case SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically:
				valueTuple = new ValueTuple<VertexPositionColorTexture, VertexPositionColorTexture, VertexPositionColorTexture>(command.VertexBR, command.VertexTR, command.VertexBL);
				break;
			default:
				<PrivateImplementationDetails>.ThrowSwitchExpressionException(A_5.spriteEffect);
				break;
			}
			ValueTuple<VertexPositionColorTexture, VertexPositionColorTexture, VertexPositionColorTexture> valueTuple2 = valueTuple;
			VertexPositionColorTexture topLeft = valueTuple2.Item1;
			VertexPositionColorTexture bottomLeft = valueTuple2.Item2;
			VertexPositionColorTexture topRight = valueTuple2.Item3;
			textureSpace = new CoordinateSpace2D
			{
				Origin = topLeft.TextureCoordinate,
				I = topRight.TextureCoordinate - topLeft.TextureCoordinate,
				J = bottomLeft.TextureCoordinate - topLeft.TextureCoordinate
			};
			worldSpace = new CoordinateSpace2D
			{
				Origin = topLeft.Position.DiscardZ(),
				I = topRight.Position.DiscardZ() - topLeft.Position.DiscardZ(),
				J = bottomLeft.Position.DiscardZ() - topLeft.Position.DiscardZ()
			};
		}

		// Token: 0x060008D6 RID: 2262 RVA: 0x0004FC18 File Offset: 0x0004DE18
		[CompilerGenerated]
		private ISerializableEntity <InitProjSpecific>g__GetConditionalTarget|276_0(ref Limb.<>c__DisplayClass276_0 A_1)
		{
			string target = A_1.subElement.GetAttributeString("target", null);
			ISerializableEntity targetEntity;
			if (string.Equals(target, "character", StringComparison.OrdinalIgnoreCase))
			{
				targetEntity = this.character;
			}
			else
			{
				targetEntity = this;
			}
			return targetEntity;
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x0004FC54 File Offset: 0x0004DE54
		[CompilerGenerated]
		private IEnumerable<SpriteDeformation> <InitProjSpecific>g__CreateDeformations|276_1(XElement e)
		{
			List<SpriteDeformation> deformations = new List<SpriteDeformation>();
			foreach (XElement animationElement in e.GetChildElements("spritedeformation", StringComparison.OrdinalIgnoreCase))
			{
				int sync = animationElement.GetAttributeInt("sync", -1);
				SpriteDeformation deformation = null;
				if (sync > -1)
				{
					string typeName = animationElement.GetAttributeString("type", "").ToLowerInvariant();
					deformation = (from l in this.ragdoll.Limbs
					where l != null
					select l).SelectMany((Limb l) => l.Deformations).FirstOrDefault((SpriteDeformation d) => d.TypeName == typeName && d.Sync == sync);
				}
				if (deformation == null)
				{
					deformation = SpriteDeformation.Load(animationElement, this.character.SpeciesName.Value);
					if (deformation != null)
					{
						this.ragdoll.SpriteDeformations.Add(deformation);
					}
				}
				if (deformation != null)
				{
					deformations.Add(deformation);
				}
			}
			return deformations;
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x0004FDA4 File Offset: 0x0004DFA4
		[CompilerGenerated]
		internal static void <ApplyStatusEffects>g__ApplyToLimb|304_0(ActionType actionType, float deltaTime, StatusEffect statusEffect, Character character, Limb limb)
		{
			statusEffect.sourceBody = limb.body;
			statusEffect.Apply(actionType, deltaTime, character, limb, null);
		}

		// Token: 0x04000441 RID: 1089
		public const float SoundInterval = 0.4f;

		// Token: 0x04000442 RID: 1090
		public float LastAttackSoundTime;

		// Token: 0x04000443 RID: 1091
		public float LastImpactSoundTime;

		// Token: 0x04000444 RID: 1092
		private float wetTimer;

		// Token: 0x04000445 RID: 1093
		private float dripParticleTimer;

		// Token: 0x04000446 RID: 1094
		private float deadTimer;

		// Token: 0x04000447 RID: 1095
		private Color? randomColor;

		// Token: 0x04000451 RID: 1105
		private SpriteBatch.EffectWithParams tintEffectParams;

		// Token: 0x04000452 RID: 1106
		private SpriteBatch.EffectWithParams huskSpriteParams;

		// Token: 0x04000453 RID: 1107
		protected DeformableSprite _deformSprite;

		// Token: 0x0400045B RID: 1115
		private Dictionary<DecorativeSprite, Limb.SpriteState> spriteAnimState = new Dictionary<DecorativeSprite, Limb.SpriteState>();

		// Token: 0x0400045C RID: 1116
		private Dictionary<int, List<DecorativeSprite>> DecorativeSpriteGroups = new Dictionary<int, List<DecorativeSprite>>();

		// Token: 0x04000460 RID: 1120
		private float damageOverlayStrength;

		// Token: 0x04000461 RID: 1121
		private float burnOverLayStrength;

		// Token: 0x04000462 RID: 1122
		private readonly List<WearableSprite> wearableTypeHidingSprites = new List<WearableSprite>();

		// Token: 0x04000463 RID: 1123
		private readonly HashSet<WearableType> wearableTypesToHide = new HashSet<WearableType>();

		// Token: 0x04000464 RID: 1124
		private bool enableHuskSprite;

		// Token: 0x04000465 RID: 1125
		private string _texturePath;

		// Token: 0x04000466 RID: 1126
		private string _damagedTexturePath;

		// Token: 0x04000467 RID: 1127
		private static Effect alphaClipEffect;

		// Token: 0x04000468 RID: 1128
		private Dictionary<WearableSprite, Dictionary<string, object>> alphaClipEffectParams;

		// Token: 0x0400046A RID: 1130
		public readonly Character character;

		// Token: 0x0400046B RID: 1131
		public readonly Ragdoll ragdoll;

		// Token: 0x0400046C RID: 1132
		public readonly RagdollParams.LimbParams Params;

		// Token: 0x0400046D RID: 1133
		public PhysicsBody body;

		// Token: 0x0400046E RID: 1134
		public Hull Hull;

		// Token: 0x04000470 RID: 1136
		private FixedMouseJoint pullJoint;

		// Token: 0x04000471 RID: 1137
		public readonly LimbType type;

		// Token: 0x04000472 RID: 1138
		private bool ignoreCollisions;

		// Token: 0x04000473 RID: 1139
		private bool isSevered;

		// Token: 0x04000474 RID: 1140
		private float severedFadeOutTimer;

		// Token: 0x04000475 RID: 1141
		private Vector2? mouthPos;

		// Token: 0x04000476 RID: 1142
		public readonly Attack attack;

		// Token: 0x04000478 RID: 1144
		private Direction dir;

		// Token: 0x04000479 RID: 1145
		public Vector2 DebugTargetPos;

		// Token: 0x0400047A RID: 1146
		public Vector2 DebugRefPos;

		// Token: 0x0400047B RID: 1147
		private bool _hidden;

		// Token: 0x0400047E RID: 1150
		private float _alpha = 1f;

		// Token: 0x0400047F RID: 1151
		public readonly List<WearableSprite> WearingItems = new List<WearableSprite>();

		// Token: 0x04000480 RID: 1152
		public readonly List<WearableSprite> OtherWearables = new List<WearableSprite>();

		// Token: 0x04000484 RID: 1156
		private readonly Dictionary<ActionType, List<StatusEffect>> statusEffects = new Dictionary<ActionType, List<StatusEffect>>();

		// Token: 0x04000485 RID: 1157
		private readonly List<DamageModifier> appliedDamageModifiers = new List<DamageModifier>();

		// Token: 0x04000486 RID: 1158
		private readonly List<DamageModifier> tempModifiers = new List<DamageModifier>();

		// Token: 0x04000487 RID: 1159
		private readonly List<Affliction> afflictionsCopy = new List<Affliction>();

		// Token: 0x04000488 RID: 1160
		private bool temporarilyDisabled;

		// Token: 0x04000489 RID: 1161
		private float reEnableTimer = -1f;

		// Token: 0x0400048A RID: 1162
		private bool originalIgnoreCollisions;

		// Token: 0x0400048B RID: 1163
		private readonly List<Body> contactBodies = new List<Body>();

		// Token: 0x0400048C RID: 1164
		private WeldJoint attachJoint;

		// Token: 0x0400048D RID: 1165
		private WeldJoint colliderJoint;

		// Token: 0x0400048E RID: 1166
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x04000491 RID: 1169
		public bool FreezeBlinkState;

		// Token: 0x02000712 RID: 1810
		private class SpriteState
		{
			// Token: 0x040038A5 RID: 14501
			public float RotationState;

			// Token: 0x040038A6 RID: 14502
			public float OffsetState;

			// Token: 0x040038A7 RID: 14503
			public float ScaleState;

			// Token: 0x040038A8 RID: 14504
			public Vector2 RandomOffsetMultiplier = new Vector2(Rand.Range(-1f, 1f, Rand.RandSync.Unsynced), Rand.Range(-1f, 1f, Rand.RandSync.Unsynced));

			// Token: 0x040038A9 RID: 14505
			public float RandomRotationFactor = Rand.Range(0f, 1f, Rand.RandSync.Unsynced);

			// Token: 0x040038AA RID: 14506
			public float RandomScaleFactor = Rand.Range(0f, 1f, Rand.RandSync.Unsynced);

			// Token: 0x040038AB RID: 14507
			public bool IsActive = true;
		}
	}
}
