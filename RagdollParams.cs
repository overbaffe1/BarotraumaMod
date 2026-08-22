using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Lights;
using Barotrauma.SpriteDeformations;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001EF RID: 495
	internal class RagdollParams : EditableParams, IMemorizable<RagdollParams>
	{
		// Token: 0x17000DF7 RID: 3575
		// (get) Token: 0x06003442 RID: 13378 RVA: 0x0020C947 File Offset: 0x0020AB47
		// (set) Token: 0x06003443 RID: 13379 RVA: 0x0020C94F File Offset: 0x0020AB4F
		public Identifier SpeciesName { get; private set; }

		// Token: 0x17000DF8 RID: 3576
		// (get) Token: 0x06003444 RID: 13380 RVA: 0x0020C958 File Offset: 0x0020AB58
		// (set) Token: 0x06003445 RID: 13381 RVA: 0x0020C960 File Offset: 0x0020AB60
		[Serialize("", IsPropertySaveable.Yes, "Default path for the limb sprite textures. Used only if the limb specific path for the limb is not defined", "", false)]
		[Editable]
		public string Texture { get; set; }

		// Token: 0x17000DF9 RID: 3577
		// (get) Token: 0x06003446 RID: 13382 RVA: 0x0020C969 File Offset: 0x0020AB69
		// (set) Token: 0x06003447 RID: 13383 RVA: 0x0020C971 File Offset: 0x0020AB71
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color Color { get; set; }

		// Token: 0x17000DFA RID: 3578
		// (get) Token: 0x06003448 RID: 13384 RVA: 0x0020C97A File Offset: 0x0020AB7A
		// (set) Token: 0x06003449 RID: 13385 RVA: 0x0020C982 File Offset: 0x0020AB82
		[Serialize(0f, IsPropertySaveable.Yes, "General orientation of the sprites as drawn on the spritesheet. Defines the \"forward direction\" of the sprites. Should be configured as the direction pointing outwards from the main limb. Incorrectly defined orientations may lead to limbs being rotated incorrectly when e.g. when the character aims or flips to face a different direction. Can be overridden per sprite by setting a value for Limb's 'Sprite Orientation'.", "", false)]
		[Editable(-360, 360)]
		public float SpritesheetOrientation { get; set; }

		// Token: 0x17000DFB RID: 3579
		// (get) Token: 0x0600344A RID: 13386 RVA: 0x0020C98B File Offset: 0x0020AB8B
		public bool IsSpritesheetOrientationHorizontal
		{
			get
			{
				return (this.SpritesheetOrientation > 45f && this.SpritesheetOrientation < 135f) || (this.SpritesheetOrientation > 255f && this.SpritesheetOrientation < 315f);
			}
		}

		// Token: 0x17000DFC RID: 3580
		// (get) Token: 0x0600344B RID: 13387 RVA: 0x0020C9C5 File Offset: 0x0020ABC5
		// (set) Token: 0x0600344C RID: 13388 RVA: 0x0020C9CD File Offset: 0x0020ABCD
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(0.1f, 2f, 1, DecimalCount = 3)]
		public float LimbScale
		{
			get
			{
				return this.limbScale;
			}
			set
			{
				this.limbScale = MathHelper.Clamp(value, 0.1f, 2f);
			}
		}

		// Token: 0x17000DFD RID: 3581
		// (get) Token: 0x0600344D RID: 13389 RVA: 0x0020C9E5 File Offset: 0x0020ABE5
		// (set) Token: 0x0600344E RID: 13390 RVA: 0x0020C9ED File Offset: 0x0020ABED
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(0.1f, 2f, 1, DecimalCount = 3)]
		public float JointScale
		{
			get
			{
				return this.jointScale;
			}
			set
			{
				this.jointScale = MathHelper.Clamp(value, 0.1f, 2f);
			}
		}

		// Token: 0x17000DFE RID: 3582
		// (get) Token: 0x0600344F RID: 13391 RVA: 0x0020CA05 File Offset: 0x0020AC05
		// (set) Token: 0x06003450 RID: 13392 RVA: 0x0020CA0D File Offset: 0x0020AC0D
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float TextureScale { get; set; }

		// Token: 0x17000DFF RID: 3583
		// (get) Token: 0x06003451 RID: 13393 RVA: 0x0020CA16 File Offset: 0x0020AC16
		// (set) Token: 0x06003452 RID: 13394 RVA: 0x0020CA1E File Offset: 0x0020AC1E
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float SourceRectScale { get; set; }

		// Token: 0x17000E00 RID: 3584
		// (get) Token: 0x06003453 RID: 13395 RVA: 0x0020CA27 File Offset: 0x0020AC27
		// (set) Token: 0x06003454 RID: 13396 RVA: 0x0020CA2F File Offset: 0x0020AC2F
		[Serialize(45f, IsPropertySaveable.Yes, "How high from the ground the main collider levitates when the character is standing? Doesn't affect swimming.", "", false)]
		[Editable(0f, 1000f, 1)]
		public float ColliderHeightFromFloor { get; set; }

		// Token: 0x17000E01 RID: 3585
		// (get) Token: 0x06003455 RID: 13397 RVA: 0x0020CA38 File Offset: 0x0020AC38
		// (set) Token: 0x06003456 RID: 13398 RVA: 0x0020CA40 File Offset: 0x0020AC40
		[Serialize(50f, IsPropertySaveable.Yes, "How much impact is required before the character takes impact damage?", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float ImpactTolerance { get; set; }

		// Token: 0x17000E02 RID: 3586
		// (get) Token: 0x06003457 RID: 13399 RVA: 0x0020CA49 File Offset: 0x0020AC49
		// (set) Token: 0x06003458 RID: 13400 RVA: 0x0020CA51 File Offset: 0x0020AC51
		[Serialize(CanEnterSubmarine.True, IsPropertySaveable.Yes, "Can the creature enter submarine. Creatures that cannot enter submarines, always collide with it, even when there is a gap.", "", false)]
		[Editable]
		public CanEnterSubmarine CanEnterSubmarine { get; set; }

		// Token: 0x17000E03 RID: 3587
		// (get) Token: 0x06003459 RID: 13401 RVA: 0x0020CA5A File Offset: 0x0020AC5A
		// (set) Token: 0x0600345A RID: 13402 RVA: 0x0020CA62 File Offset: 0x0020AC62
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool CanWalk { get; set; }

		// Token: 0x17000E04 RID: 3588
		// (get) Token: 0x0600345B RID: 13403 RVA: 0x0020CA6B File Offset: 0x0020AC6B
		// (set) Token: 0x0600345C RID: 13404 RVA: 0x0020CA73 File Offset: 0x0020AC73
		[Serialize(true, IsPropertySaveable.Yes, "Can the character be dragged around by other creatures?", "", false)]
		[Editable]
		public bool Draggable { get; set; }

		// Token: 0x17000E05 RID: 3589
		// (get) Token: 0x0600345D RID: 13405 RVA: 0x0020CA7C File Offset: 0x0020AC7C
		// (set) Token: 0x0600345E RID: 13406 RVA: 0x0020CA84 File Offset: 0x0020AC84
		[Serialize(LimbType.Torso, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public LimbType MainLimb { get; set; }

		// Token: 0x17000E06 RID: 3590
		// (get) Token: 0x0600345F RID: 13407 RVA: 0x0020CA8D File Offset: 0x0020AC8D
		// (set) Token: 0x06003460 RID: 13408 RVA: 0x0020CA95 File Offset: 0x0020AC95
		public List<RagdollParams.ColliderParams> Colliders { get; private set; } = new List<RagdollParams.ColliderParams>();

		// Token: 0x17000E07 RID: 3591
		// (get) Token: 0x06003461 RID: 13409 RVA: 0x0020CA9E File Offset: 0x0020AC9E
		// (set) Token: 0x06003462 RID: 13410 RVA: 0x0020CAA6 File Offset: 0x0020ACA6
		public List<RagdollParams.LimbParams> Limbs { get; private set; } = new List<RagdollParams.LimbParams>();

		// Token: 0x17000E08 RID: 3592
		// (get) Token: 0x06003463 RID: 13411 RVA: 0x0020CAAF File Offset: 0x0020ACAF
		// (set) Token: 0x06003464 RID: 13412 RVA: 0x0020CAB7 File Offset: 0x0020ACB7
		public List<RagdollParams.JointParams> Joints { get; private set; } = new List<RagdollParams.JointParams>();

		// Token: 0x06003465 RID: 13413 RVA: 0x0020CAC0 File Offset: 0x0020ACC0
		protected IEnumerable<RagdollParams.SubParam> GetAllSubParams()
		{
			return this.Colliders.Concat(this.Limbs).Concat(this.Joints);
		}

		// Token: 0x06003466 RID: 13414 RVA: 0x0020CADE File Offset: 0x0020ACDE
		public static string GetDefaultFileName(Identifier speciesName)
		{
			return speciesName.Value.CapitaliseFirstInvariant() + "DefaultRagdoll";
		}

		// Token: 0x06003467 RID: 13415 RVA: 0x0020CAF6 File Offset: 0x0020ACF6
		public static string GetDefaultFile(Identifier speciesName)
		{
			return Barotrauma.IO.Path.Combine(new string[]
			{
				RagdollParams.GetFolder(speciesName),
				RagdollParams.GetDefaultFileName(speciesName) + ".xml"
			});
		}

		// Token: 0x06003468 RID: 13416 RVA: 0x0020CB20 File Offset: 0x0020AD20
		public static string GetFolder(Identifier speciesName)
		{
			CharacterPrefab prefab = CharacterPrefab.FindBySpeciesName(speciesName);
			ContentXElement contentXElement = (prefab != null) ? prefab.ConfigElement : null;
			ContentXElement contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to find config file for '");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return string.Empty;
			}
			return RagdollParams.GetFolder(prefab.ConfigElement, prefab.ContentFile.Path.Value);
		}

		// Token: 0x06003469 RID: 13417 RVA: 0x0020CBAC File Offset: 0x0020ADAC
		private static string GetFolder(ContentXElement root, string filePath)
		{
			ContentXElement contentXElement = root.GetChildElement("ragdolls") ?? root.GetChildElement("ragdoll");
			string text;
			if (contentXElement == null)
			{
				text = null;
			}
			else
			{
				ContentPath attributeContentPath = contentXElement.GetAttributeContentPath("folder");
				text = ((attributeContentPath != null) ? attributeContentPath.Value : null);
			}
			string folder = text;
			if (folder.IsNullOrEmpty() || folder.Equals("default", StringComparison.OrdinalIgnoreCase))
			{
				ReadOnlySpan<char> str = Barotrauma.IO.Path.Combine(new string[]
				{
					Barotrauma.IO.Path.GetDirectoryName(filePath),
					"Ragdolls"
				});
				char directorySeparatorChar = Barotrauma.IO.Path.DirectorySeparatorChar;
				folder = str + new ReadOnlySpan<char>(ref directorySeparatorChar);
			}
			return folder.CleanUpPathCrossPlatform(true, "");
		}

		// Token: 0x0600346A RID: 13418 RVA: 0x0020CC47 File Offset: 0x0020AE47
		public static T GetDefaultRagdollParams<T>(Character character) where T : RagdollParams, new()
		{
			return RagdollParams.GetDefaultRagdollParams<T>(character.SpeciesName, character.Params, character.Prefab.ContentPackage);
		}

		// Token: 0x0600346B RID: 13419 RVA: 0x0020CC68 File Offset: 0x0020AE68
		public static T GetDefaultRagdollParams<T>(Identifier speciesName, CharacterParams characterParams, ContentPackage contentPackage) where T : RagdollParams, new()
		{
			XDocument variantFile = characterParams.VariantFile;
			XElement mainElement = ((variantFile != null) ? variantFile.Root : null) ?? characterParams.MainElement;
			return RagdollParams.GetDefaultRagdollParams<T>(speciesName, mainElement, contentPackage);
		}

		// Token: 0x0600346C RID: 13420 RVA: 0x0020CCA0 File Offset: 0x0020AEA0
		public static T GetDefaultRagdollParams<T>(Identifier speciesName, XElement characterRootElement, ContentPackage contentPackage) where T : RagdollParams, new()
		{
			if (characterRootElement.IsOverride())
			{
				characterRootElement = characterRootElement.FirstElement();
			}
			Identifier ragdollSpecies = speciesName;
			Identifier variantOf = characterRootElement.VariantOf();
			if (characterRootElement != null)
			{
				XElement ragdollElement = characterRootElement.GetChildElement("ragdolls", StringComparison.OrdinalIgnoreCase) ?? characterRootElement.GetChildElement("ragdoll", StringComparison.OrdinalIgnoreCase);
				if (ragdollElement != null)
				{
					ContentPath path = ragdollElement.GetAttributeContentPath("path", contentPackage) ?? ragdollElement.GetAttributeContentPath("file", contentPackage);
					if (path != null)
					{
						return RagdollParams.GetRagdollParams<T>(speciesName, ragdollSpecies, path, contentPackage);
					}
					if (variantOf.IsEmpty)
					{
						goto IL_F2;
					}
					ContentPath attributeContentPath = ragdollElement.GetAttributeContentPath("folder", contentPackage);
					string folder = (attributeContentPath != null) ? attributeContentPath.Value : null;
					if (!folder.IsNullOrEmpty() && !folder.Equals("default", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_F2;
					}
					CharacterPrefab prefab = CharacterPrefab.FindBySpeciesName(variantOf);
					if (prefab != null)
					{
						ragdollSpecies = prefab.GetBaseCharacterSpeciesName(variantOf);
						goto IL_F2;
					}
					goto IL_F2;
				}
			}
			if (!variantOf.IsEmpty)
			{
				CharacterPrefab parentPrefab = CharacterPrefab.FindBySpeciesName(variantOf);
				if (parentPrefab != null)
				{
					return RagdollParams.GetDefaultRagdollParams<T>(variantOf, parentPrefab.ConfigElement, parentPrefab.ContentPackage);
				}
			}
			IL_F2:
			return RagdollParams.GetRagdollParams<T>(speciesName, ragdollSpecies, null, contentPackage);
		}

		// Token: 0x0600346D RID: 13421 RVA: 0x0020CDA8 File Offset: 0x0020AFA8
		public static T GetRagdollParams<T>(Identifier speciesName, Identifier ragdollSpecies, Either<string, ContentPath> file, ContentPackage contentPackage) where T : RagdollParams, new()
		{
			ContentPath contentPath = null;
			string fileName = null;
			if (file != null && !file.TryGet(out fileName))
			{
				file.TryGet(out contentPath);
			}
			Dictionary<string, RagdollParams> ragdolls;
			if (!RagdollParams.allRagdolls.TryGetValue(speciesName, out ragdolls))
			{
				ragdolls = new Dictionary<string, RagdollParams>();
				RagdollParams.allRagdolls.Add(speciesName, ragdolls);
			}
			string text;
			if ((text = fileName) == null)
			{
				text = (((contentPath != null) ? contentPath.Value : null) ?? RagdollParams.GetDefaultFileName(ragdollSpecies));
			}
			string key = text;
			RagdollParams ragdoll;
			if (ragdolls.TryGetValue(key, out ragdoll))
			{
				return (T)((object)ragdoll);
			}
			if (!contentPath.IsNullOrEmpty())
			{
				T ragdollInstance = Activator.CreateInstance<T>();
				if (ragdollInstance.Load(contentPath, ragdollSpecies))
				{
					ragdolls.TryAdd(contentPath.Value, ragdollInstance);
					return ragdollInstance;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(94, 3);
				defaultInterpolatedStringHandler.AppendLiteral("[RagdollParams] Failed to load a ragdoll ");
				defaultInterpolatedStringHandler.AppendFormatted<T>(ragdollInstance);
				defaultInterpolatedStringHandler.AppendLiteral(" from ");
				defaultInterpolatedStringHandler.AppendFormatted(contentPath.Value);
				defaultInterpolatedStringHandler.AppendLiteral(" for the character ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
				defaultInterpolatedStringHandler.AppendLiteral(". Using the default ragdoll.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, contentPackage, false, false);
			}
			string folder = RagdollParams.GetFolder(ragdollSpecies);
			string selectedFile;
			if (Directory.Exists(folder))
			{
				IOrderedEnumerable<string> files = Directory.GetFiles(folder).OrderBy((string f) => f, StringComparer.OrdinalIgnoreCase);
				if (files.None(null))
				{
					DebugConsole.ThrowError("[RagdollParams] Could not find any ragdoll files from the folder: " + folder + ". Using the default ragdoll.", null, contentPackage, false, false);
					selectedFile = RagdollParams.GetDefaultFile(ragdollSpecies);
				}
				else if (string.IsNullOrEmpty(fileName))
				{
					string defaultFileName = RagdollParams.GetDefaultFileName(ragdollSpecies);
					selectedFile = (files.FirstOrDefault((string f) => f.Contains(defaultFileName, StringComparison.OrdinalIgnoreCase)) ?? files.First<string>());
				}
				else
				{
					selectedFile = files.FirstOrDefault((string f) => Barotrauma.IO.Path.GetFileNameWithoutExtension(f).Equals(fileName, StringComparison.OrdinalIgnoreCase));
					if (selectedFile == null)
					{
						DebugConsole.ThrowError("[RagdollParams] Could not find a ragdoll file that matches the name " + fileName + ". Using the default ragdoll.", null, contentPackage, false, false);
						selectedFile = RagdollParams.GetDefaultFile(ragdollSpecies);
					}
				}
			}
			else
			{
				DebugConsole.ThrowError("[RagdollParams] Invalid directory: " + folder + ". Using the default ragdoll.", null, contentPackage, false, false);
				selectedFile = RagdollParams.GetDefaultFile(ragdollSpecies);
			}
			DebugConsole.Log("[RagdollParams] Loading the ragdoll from " + selectedFile + ".");
			T r = Activator.CreateInstance<T>();
			if (r.Load(ContentPath.FromRaw(contentPackage, selectedFile), speciesName))
			{
				ragdolls.TryAdd(key, r);
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(65, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("[RagdollParams] Failed to load ragdoll ");
				defaultInterpolatedStringHandler2.AppendFormatted(r.Name);
				defaultInterpolatedStringHandler2.AppendLiteral(" from ");
				defaultInterpolatedStringHandler2.AppendFormatted(selectedFile);
				defaultInterpolatedStringHandler2.AppendLiteral(" for the character ");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(speciesName);
				defaultInterpolatedStringHandler2.AppendLiteral(".");
				string error = defaultInterpolatedStringHandler2.ToStringAndClear();
				if (contentPackage == GameMain.VanillaContent)
				{
					CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(speciesName);
					if (((characterPrefab != null) ? characterPrefab.ParentPrefab : null) == null || characterPrefab.ParentPrefab.ContentPackage == GameMain.VanillaContent)
					{
						throw new Exception(error);
					}
				}
				DebugConsole.ThrowError(error, null, contentPackage, false, false);
				if (typeof(T) == typeof(HumanRagdollParams))
				{
					Identifier fallbackSpecies = CharacterPrefab.HumanSpeciesName;
					r = RagdollParams.GetRagdollParams<T>(fallbackSpecies, fallbackSpecies, ContentPath.FromRaw(contentPackage, "Content/Characters/Human/Ragdolls/HumanDefaultRagdoll.xml"), GameMain.VanillaContent);
				}
				else
				{
					Identifier fallbackSpecies2 = "crawler".ToIdentifier();
					r = RagdollParams.GetRagdollParams<T>(fallbackSpecies2, fallbackSpecies2, ContentPath.FromRaw(contentPackage, "Content/Characters/Crawler/Ragdolls/CrawlerDefaultRagdoll.xml"), GameMain.VanillaContent);
				}
			}
			return r;
		}

		// Token: 0x0600346E RID: 13422 RVA: 0x0020D160 File Offset: 0x0020B360
		public static T CreateDefault<T>(string fullPath, Identifier speciesName, XElement mainElement) where T : RagdollParams, new()
		{
			if (RagdollParams.allRagdolls.ContainsKey(speciesName))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[RagdollParams] Removing the old ragdolls from ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Red), false);
				RagdollParams.allRagdolls.Remove(speciesName);
			}
			Dictionary<string, RagdollParams> ragdolls = new Dictionary<string, RagdollParams>();
			RagdollParams.allRagdolls.Add(speciesName, ragdolls);
			T t = Activator.CreateInstance<T>();
			t.doc = new XDocument(new object[]
			{
				mainElement
			});
			T instance = t;
			CharacterPrefab characterPrefab = CharacterPrefab.Prefabs[speciesName];
			ContentPath contentPath = ContentPath.FromRaw(characterPrefab.ContentPackage, fullPath);
			instance.UpdatePath(contentPath);
			instance.IsLoaded = instance.Deserialize(mainElement, true, true);
			instance.Save(null);
			instance.Load(contentPath, speciesName);
			ragdolls.Add(instance.FileNameWithoutExtension, instance);
			DebugConsole.NewMessage("[RagdollParams] New default ragdoll params successfully created at " + fullPath, new Color?(Color.NavajoWhite), false);
			return instance;
		}

		// Token: 0x0600346F RID: 13423 RVA: 0x0020D28A File Offset: 0x0020B48A
		public static void ClearCache()
		{
			RagdollParams.allRagdolls.Clear();
		}

		// Token: 0x06003470 RID: 13424 RVA: 0x0020D298 File Offset: 0x0020B498
		protected override void UpdatePath(ContentPath fullPath)
		{
			Identifier speciesName = this.SpeciesName;
			if (speciesName == null)
			{
				base.UpdatePath(fullPath);
				return;
			}
			string fileName = base.FileNameWithoutExtension;
			Dictionary<string, RagdollParams> ragdolls;
			if (RagdollParams.allRagdolls.TryGetValue(this.SpeciesName, out ragdolls))
			{
				ragdolls.Remove(fileName);
			}
			base.UpdatePath(fullPath);
			if (ragdolls != null && !ragdolls.ContainsKey(fileName))
			{
				ragdolls.Add(fileName, this);
			}
		}

		// Token: 0x06003471 RID: 13425 RVA: 0x0020D2FC File Offset: 0x0020B4FC
		public bool Save(string fileNameWithoutExtension = null)
		{
			base.OriginalElement = this.MainElement;
			this.GetAllSubParams().ForEach(delegate(RagdollParams.SubParam p)
			{
				p.SetCurrentElementAsOriginalElement();
			});
			this.Serialize(null, true, true);
			return base.Save(fileNameWithoutExtension, new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = true,
				NewLineOnAttributes = false
			});
		}

		// Token: 0x06003472 RID: 13426 RVA: 0x0020D36A File Offset: 0x0020B56A
		protected bool Load(ContentPath file, Identifier speciesName)
		{
			if (this.Load(file))
			{
				this.isVariantScaleApplied = false;
				this.SpeciesName = speciesName;
				this.CreateColliders();
				this.CreateLimbs();
				this.CreateJoints();
				return true;
			}
			return false;
		}

		// Token: 0x06003473 RID: 13427 RVA: 0x0020D398 File Offset: 0x0020B598
		public void Apply()
		{
			this.Serialize(null, true, true);
		}

		// Token: 0x06003474 RID: 13428 RVA: 0x0020D3A4 File Offset: 0x0020B5A4
		public override bool Reset(bool forceReload = false)
		{
			if (forceReload)
			{
				return this.Load(base.Path, this.SpeciesName);
			}
			this.Deserialize(base.OriginalElement, false, false);
			this.GetAllSubParams().ForEach(delegate(RagdollParams.SubParam sp)
			{
				sp.Reset();
			});
			return true;
		}

		// Token: 0x06003475 RID: 13429 RVA: 0x0020D408 File Offset: 0x0020B608
		protected void CreateColliders()
		{
			this.Colliders.Clear();
			ContentXElement mainElement = this.MainElement;
			IEnumerable<ContentXElement> colliderElements = (mainElement != null) ? mainElement.GetChildElements("collider") : null;
			if (colliderElements != null)
			{
				for (int i = 0; i < colliderElements.Count<ContentXElement>(); i++)
				{
					ContentXElement element = colliderElements.ElementAt(i);
					string name = (i > 0) ? "Secondary Collider" : "Main Collider";
					this.Colliders.Add(new RagdollParams.ColliderParams(element, this, name));
				}
			}
		}

		// Token: 0x06003476 RID: 13430 RVA: 0x0020D478 File Offset: 0x0020B678
		protected void CreateLimbs()
		{
			this.Limbs.Clear();
			ContentXElement mainElement = this.MainElement;
			IEnumerable<ContentXElement> childElements = (mainElement != null) ? mainElement.GetChildElements("limb") : null;
			if (childElements != null)
			{
				foreach (ContentXElement element in childElements)
				{
					this.Limbs.Add(new RagdollParams.LimbParams(element, this));
				}
			}
			this.Limbs = (from l in this.Limbs
			orderby l.ID
			select l).ToList<RagdollParams.LimbParams>();
		}

		// Token: 0x06003477 RID: 13431 RVA: 0x0020D528 File Offset: 0x0020B728
		protected void CreateJoints()
		{
			this.Joints.Clear();
			foreach (ContentXElement element in this.MainElement.GetChildElements("joint"))
			{
				this.Joints.Add(new RagdollParams.JointParams(element, this));
			}
		}

		// Token: 0x06003478 RID: 13432 RVA: 0x0020D598 File Offset: 0x0020B798
		public bool Deserialize(XElement element = null, bool alsoChildren = true, bool recursive = true)
		{
			if (base.Deserialize(element))
			{
				if (alsoChildren)
				{
					this.GetAllSubParams().ForEach(delegate(RagdollParams.SubParam p)
					{
						p.Deserialize(null, recursive);
					});
				}
				return true;
			}
			return false;
		}

		// Token: 0x06003479 RID: 13433 RVA: 0x0020D5D8 File Offset: 0x0020B7D8
		public bool Serialize(XElement element = null, bool alsoChildren = true, bool recursive = true)
		{
			if (base.Serialize(element))
			{
				if (alsoChildren)
				{
					this.GetAllSubParams().ForEach(delegate(RagdollParams.SubParam p)
					{
						p.Serialize(null, recursive);
					});
				}
				return true;
			}
			return false;
		}

		// Token: 0x0600347A RID: 13434 RVA: 0x0020D618 File Offset: 0x0020B818
		public void AddToEditor(ParamsEditor editor, bool alsoChildren = true, int space = 0)
		{
			base.AddToEditor(editor, 0);
			if (alsoChildren)
			{
				IEnumerable<RagdollParams.SubParam> subParams = this.GetAllSubParams();
				foreach (RagdollParams.SubParam subParam in subParams)
				{
					subParam.AddToEditor(editor, true, space);
				}
			}
			if (space > 0)
			{
				new GUIFrame(new RectTransform(new Point(editor.EditorBox.Rect.Width, space), editor.EditorBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, new Color?(ParamsEditor.Color)).CanBeFocused = false;
			}
		}

		// Token: 0x0600347B RID: 13435 RVA: 0x0020D6C8 File Offset: 0x0020B8C8
		public void TryApplyVariantScale(XDocument variantFile)
		{
			if (this.isVariantScaleApplied)
			{
				return;
			}
			if (variantFile == null)
			{
				return;
			}
			XElement root = variantFile.GetRootExcludingOverride();
			if (root != null)
			{
				XElement ragdollElement = root.GetChildElement("ragdoll", StringComparison.OrdinalIgnoreCase) ?? root.GetChildElement("ragdolls", StringComparison.OrdinalIgnoreCase);
				if (ragdollElement != null)
				{
					float scaleMultiplier = ragdollElement.GetAttributeFloat("scalemultiplier", 1f);
					this.JointScale *= scaleMultiplier;
					this.LimbScale *= scaleMultiplier;
					float textureScale = ragdollElement.GetAttributeFloat("TextureScale", 0f);
					if (textureScale > 0f)
					{
						this.TextureScale = textureScale;
					}
					float sourceRectScale = ragdollElement.GetAttributeFloat("SourceRectScale", 0f);
					if (sourceRectScale > 0f)
					{
						this.SourceRectScale = sourceRectScale;
					}
				}
			}
			this.isVariantScaleApplied = true;
		}

		// Token: 0x17000E09 RID: 3593
		// (get) Token: 0x0600347C RID: 13436 RVA: 0x0020D786 File Offset: 0x0020B986
		// (set) Token: 0x0600347D RID: 13437 RVA: 0x0020D78E File Offset: 0x0020B98E
		public Memento<RagdollParams> Memento { get; protected set; } = new Memento<RagdollParams>();

		// Token: 0x0600347E RID: 13438 RVA: 0x0020D798 File Offset: 0x0020B998
		public void StoreSnapshot()
		{
			this.Serialize(null, true, true);
			if (this.doc == null)
			{
				DebugConsole.ThrowError("[RagdollParams] The source XML Document is null!", null, null, false, false);
				return;
			}
			RagdollParams copy = new RagdollParams
			{
				SpeciesName = this.SpeciesName,
				IsLoaded = true,
				doc = new XDocument(this.doc),
				Path = base.Path
			};
			copy.CreateColliders();
			copy.CreateLimbs();
			copy.CreateJoints();
			copy.Deserialize(null, true, true);
			copy.Serialize(null, true, true);
			this.Memento.Store(copy);
		}

		// Token: 0x0600347F RID: 13439 RVA: 0x0020D82E File Offset: 0x0020BA2E
		public void Undo()
		{
			this.RevertTo(this.Memento.Undo());
		}

		// Token: 0x06003480 RID: 13440 RVA: 0x0020D841 File Offset: 0x0020BA41
		public void Redo()
		{
			this.RevertTo(this.Memento.Redo());
		}

		// Token: 0x06003481 RID: 13441 RVA: 0x0020D854 File Offset: 0x0020BA54
		public void ClearHistory()
		{
			this.Memento.Clear();
		}

		// Token: 0x06003482 RID: 13442 RVA: 0x0020D864 File Offset: 0x0020BA64
		private void RevertTo(RagdollParams source)
		{
			ContentXElement mainElement = source.MainElement;
			ContentXElement contentXElement = null;
			if (mainElement == contentXElement)
			{
				string error = "[RagdollParams] The source XML Element of the given RagdollParams is null!";
				Exception e = null;
				ContentXElement mainElement2 = source.MainElement;
				DebugConsole.ThrowError(error, e, (mainElement2 != null) ? mainElement2.ContentPackage : null, false, false);
				return;
			}
			this.Deserialize(source.MainElement, false, true);
			List<RagdollParams.SubParam> sourceSubParams = source.GetAllSubParams().ToList<RagdollParams.SubParam>();
			List<RagdollParams.SubParam> subParams = this.GetAllSubParams().ToList<RagdollParams.SubParam>();
			if (sourceSubParams.Count != subParams.Count)
			{
				string error2 = "[RagdollParams] The count of the sub params differs! Failed to revert to the previous snapshot! Please reset the ragdoll to undo the changes.";
				Exception e2 = null;
				ContentXElement mainElement3 = source.MainElement;
				DebugConsole.ThrowError(error2, e2, (mainElement3 != null) ? mainElement3.ContentPackage : null, false, false);
				return;
			}
			for (int i = 0; i < subParams.Count; i++)
			{
				List<RagdollParams.SubParam> subSubParams = subParams[i].SubParams;
				if (subSubParams.Count != sourceSubParams[i].SubParams.Count)
				{
					string error3 = "[RagdollParams] The count of the sub sub params differs! Failed to revert to the previous snapshot! Please reset the ragdoll to undo the changes.";
					Exception e3 = null;
					ContentXElement mainElement4 = source.MainElement;
					DebugConsole.ThrowError(error3, e3, (mainElement4 != null) ? mainElement4.ContentPackage : null, false, false);
					return;
				}
				subParams[i].Deserialize(sourceSubParams[i].Element, false);
				for (int j = 0; j < subSubParams.Count; j++)
				{
					subSubParams[j].Deserialize(sourceSubParams[i].SubParams[j].Element, false);
				}
			}
		}

		// Token: 0x04001B41 RID: 6977
		public const float MIN_SCALE = 0.1f;

		// Token: 0x04001B42 RID: 6978
		public const float MAX_SCALE = 2f;

		// Token: 0x04001B47 RID: 6983
		private float limbScale;

		// Token: 0x04001B48 RID: 6984
		private float jointScale;

		// Token: 0x04001B51 RID: 6993
		private static readonly Dictionary<Identifier, Dictionary<string, RagdollParams>> allRagdolls = new Dictionary<Identifier, Dictionary<string, RagdollParams>>();

		// Token: 0x04001B55 RID: 6997
		private bool isVariantScaleApplied;

		// Token: 0x02000EC7 RID: 3783
		public class JointParams : RagdollParams.SubParam
		{
			// Token: 0x17001BAA RID: 7082
			// (get) Token: 0x06008664 RID: 34404 RVA: 0x003A27D2 File Offset: 0x003A09D2
			// (set) Token: 0x06008665 RID: 34405 RVA: 0x003A27F3 File Offset: 0x003A09F3
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public override string Name
			{
				get
				{
					if (string.IsNullOrWhiteSpace(this.name))
					{
						this.name = this.GenerateName();
					}
					return this.name;
				}
				set
				{
					this.name = value;
				}
			}

			// Token: 0x06008666 RID: 34406 RVA: 0x003A27FC File Offset: 0x003A09FC
			public override string GenerateName()
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Joint ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Limb1);
				defaultInterpolatedStringHandler.AppendLiteral(" - ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Limb2);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}

			// Token: 0x17001BAB RID: 7083
			// (get) Token: 0x06008667 RID: 34407 RVA: 0x003A284C File Offset: 0x003A0A4C
			// (set) Token: 0x06008668 RID: 34408 RVA: 0x003A2854 File Offset: 0x003A0A54
			[Serialize(-1, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public int Limb1 { get; set; }

			// Token: 0x17001BAC RID: 7084
			// (get) Token: 0x06008669 RID: 34409 RVA: 0x003A285D File Offset: 0x003A0A5D
			// (set) Token: 0x0600866A RID: 34410 RVA: 0x003A2865 File Offset: 0x003A0A65
			[Serialize(-1, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public int Limb2 { get; set; }

			// Token: 0x17001BAD RID: 7085
			// (get) Token: 0x0600866B RID: 34411 RVA: 0x003A286E File Offset: 0x003A0A6E
			// (set) Token: 0x0600866C RID: 34412 RVA: 0x003A2876 File Offset: 0x003A0A76
			[Serialize("1.0, 1.0", IsPropertySaveable.Yes, "Local position of the joint in the Limb1.", "", false)]
			[Editable]
			public Vector2 Limb1Anchor { get; set; }

			// Token: 0x17001BAE RID: 7086
			// (get) Token: 0x0600866D RID: 34413 RVA: 0x003A287F File Offset: 0x003A0A7F
			// (set) Token: 0x0600866E RID: 34414 RVA: 0x003A2887 File Offset: 0x003A0A87
			[Serialize("1.0, 1.0", IsPropertySaveable.Yes, "Local position of the joint in the Limb2.", "", false)]
			[Editable]
			public Vector2 Limb2Anchor { get; set; }

			// Token: 0x17001BAF RID: 7087
			// (get) Token: 0x0600866F RID: 34415 RVA: 0x003A2890 File Offset: 0x003A0A90
			// (set) Token: 0x06008670 RID: 34416 RVA: 0x003A2898 File Offset: 0x003A0A98
			[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool CanBeSevered { get; set; }

			// Token: 0x17001BB0 RID: 7088
			// (get) Token: 0x06008671 RID: 34417 RVA: 0x003A28A1 File Offset: 0x003A0AA1
			// (set) Token: 0x06008672 RID: 34418 RVA: 0x003A28A9 File Offset: 0x003A0AA9
			[Serialize(0f, IsPropertySaveable.Yes, "Default 0 (Can't be severed when the creature is alive). Modifies the severance probability (defined per item/attack) when the character is alive. Currently only affects non-humanoid ragdolls. Also note that if CanBeSevered is false, this property doesn't have any effect.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, ValueStep = 0.1f, DecimalCount = 2)]
			public float SeveranceProbabilityModifier { get; set; }

			// Token: 0x17001BB1 RID: 7089
			// (get) Token: 0x06008673 RID: 34419 RVA: 0x003A28B2 File Offset: 0x003A0AB2
			// (set) Token: 0x06008674 RID: 34420 RVA: 0x003A28BA File Offset: 0x003A0ABA
			[Serialize("gore", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string BreakSound { get; set; }

			// Token: 0x17001BB2 RID: 7090
			// (get) Token: 0x06008675 RID: 34421 RVA: 0x003A28C3 File Offset: 0x003A0AC3
			// (set) Token: 0x06008676 RID: 34422 RVA: 0x003A28CB File Offset: 0x003A0ACB
			[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool LimitEnabled { get; set; }

			// Token: 0x17001BB3 RID: 7091
			// (get) Token: 0x06008677 RID: 34423 RVA: 0x003A28D4 File Offset: 0x003A0AD4
			// (set) Token: 0x06008678 RID: 34424 RVA: 0x003A28DC File Offset: 0x003A0ADC
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float UpperLimit { get; set; }

			// Token: 0x17001BB4 RID: 7092
			// (get) Token: 0x06008679 RID: 34425 RVA: 0x003A28E5 File Offset: 0x003A0AE5
			// (set) Token: 0x0600867A RID: 34426 RVA: 0x003A28ED File Offset: 0x003A0AED
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float LowerLimit { get; set; }

			// Token: 0x17001BB5 RID: 7093
			// (get) Token: 0x0600867B RID: 34427 RVA: 0x003A28F6 File Offset: 0x003A0AF6
			// (set) Token: 0x0600867C RID: 34428 RVA: 0x003A28FE File Offset: 0x003A0AFE
			[Serialize(0.25f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float Stiffness { get; set; }

			// Token: 0x17001BB6 RID: 7094
			// (get) Token: 0x0600867D RID: 34429 RVA: 0x003A2907 File Offset: 0x003A0B07
			// (set) Token: 0x0600867E RID: 34430 RVA: 0x003A290F File Offset: 0x003A0B0F
			[Serialize(1f, IsPropertySaveable.Yes, "CAUTION: Not fully implemented. Only use for limb joints that connect non-animated limbs!", "", false)]
			[Editable(DecimalCount = 2)]
			public float Scale { get; set; }

			// Token: 0x17001BB7 RID: 7095
			// (get) Token: 0x0600867F RID: 34431 RVA: 0x003A2918 File Offset: 0x003A0B18
			// (set) Token: 0x06008680 RID: 34432 RVA: 0x003A2920 File Offset: 0x003A0B20
			[Serialize(false, IsPropertySaveable.No, "", "", false)]
			[Editable(ReadOnly = true)]
			public bool WeldJoint { get; set; }

			// Token: 0x17001BB8 RID: 7096
			// (get) Token: 0x06008681 RID: 34433 RVA: 0x003A2929 File Offset: 0x003A0B29
			// (set) Token: 0x06008682 RID: 34434 RVA: 0x003A2931 File Offset: 0x003A0B31
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool ClockWiseRotation { get; set; }

			// Token: 0x06008683 RID: 34435 RVA: 0x003A293A File Offset: 0x003A0B3A
			public JointParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
			}

			// Token: 0x040053A4 RID: 21412
			private string name;
		}

		// Token: 0x02000EC8 RID: 3784
		public class LimbParams : RagdollParams.SubParam
		{
			// Token: 0x17001BB9 RID: 7097
			// (get) Token: 0x06008684 RID: 34436 RVA: 0x003A2944 File Offset: 0x003A0B44
			// (set) Token: 0x06008685 RID: 34437 RVA: 0x003A294C File Offset: 0x003A0B4C
			public RagdollParams.AttackParams Attack { get; private set; }

			// Token: 0x17001BBA RID: 7098
			// (get) Token: 0x06008686 RID: 34438 RVA: 0x003A2955 File Offset: 0x003A0B55
			// (set) Token: 0x06008687 RID: 34439 RVA: 0x003A295D File Offset: 0x003A0B5D
			public RagdollParams.SoundParams Sound { get; private set; }

			// Token: 0x17001BBB RID: 7099
			// (get) Token: 0x06008688 RID: 34440 RVA: 0x003A2966 File Offset: 0x003A0B66
			// (set) Token: 0x06008689 RID: 34441 RVA: 0x003A296E File Offset: 0x003A0B6E
			public RagdollParams.LightSourceParams LightSource { get; private set; }

			// Token: 0x17001BBC RID: 7100
			// (get) Token: 0x0600868A RID: 34442 RVA: 0x003A2977 File Offset: 0x003A0B77
			// (set) Token: 0x0600868B RID: 34443 RVA: 0x003A297F File Offset: 0x003A0B7F
			public List<RagdollParams.DamageModifierParams> DamageModifiers { get; private set; } = new List<RagdollParams.DamageModifierParams>();

			// Token: 0x17001BBD RID: 7101
			// (get) Token: 0x0600868C RID: 34444 RVA: 0x003A2988 File Offset: 0x003A0B88
			// (set) Token: 0x0600868D RID: 34445 RVA: 0x003A29A9 File Offset: 0x003A0BA9
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public override string Name
			{
				get
				{
					if (string.IsNullOrWhiteSpace(this.name))
					{
						this.name = this.GenerateName();
					}
					return this.name;
				}
				set
				{
					this.name = value;
				}
			}

			// Token: 0x0600868E RID: 34446 RVA: 0x003A29B4 File Offset: 0x003A0BB4
			public override string GenerateName()
			{
				if (this.Type == LimbType.None)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Limb ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.ID);
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler2.AppendFormatted<LimbType>(this.Type);
				defaultInterpolatedStringHandler2.AppendLiteral(" (");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.ID);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				return defaultInterpolatedStringHandler2.ToStringAndClear();
			}

			// Token: 0x0600868F RID: 34447 RVA: 0x003A2A35 File Offset: 0x003A0C35
			public RagdollParams.SpriteParams GetSprite()
			{
				return this.deformSpriteParams ?? this.normalSpriteParams;
			}

			// Token: 0x17001BBE RID: 7102
			// (get) Token: 0x06008690 RID: 34448 RVA: 0x003A2A47 File Offset: 0x003A0C47
			// (set) Token: 0x06008691 RID: 34449 RVA: 0x003A2A4F File Offset: 0x003A0C4F
			[Serialize(-1, IsPropertySaveable.Yes, "", "", false)]
			[Editable(ReadOnly = true)]
			public int ID { get; set; }

			// Token: 0x17001BBF RID: 7103
			// (get) Token: 0x06008692 RID: 34450 RVA: 0x003A2A58 File Offset: 0x003A0C58
			// (set) Token: 0x06008693 RID: 34451 RVA: 0x003A2A60 File Offset: 0x003A0C60
			[Serialize(LimbType.None, IsPropertySaveable.Yes, "The limb type affects many things, like the animations. Torso or Head are considered as the main limbs. Every character should have at least one Torso or Head.", "", false)]
			[Editable]
			public LimbType Type { get; set; }

			// Token: 0x17001BC0 RID: 7104
			// (get) Token: 0x06008694 RID: 34452 RVA: 0x003A2A69 File Offset: 0x003A0C69
			// (set) Token: 0x06008695 RID: 34453 RVA: 0x003A2A71 File Offset: 0x003A0C71
			[Serialize(LimbType.None, IsPropertySaveable.Yes, "Secondary limb type to be used for generic purposes. Currently only used in climbing animations.", "", false)]
			[Editable]
			public LimbType SecondaryType { get; set; }

			// Token: 0x06008696 RID: 34454 RVA: 0x003A2A7A File Offset: 0x003A0C7A
			public float GetSpriteOrientation()
			{
				return MathHelper.ToRadians(this.GetSpriteOrientationInDegrees());
			}

			// Token: 0x06008697 RID: 34455 RVA: 0x003A2A87 File Offset: 0x003A0C87
			public float GetSpriteOrientationInDegrees()
			{
				if (!float.IsNaN(this.SpriteOrientation))
				{
					return this.SpriteOrientation;
				}
				return base.Ragdoll.SpritesheetOrientation;
			}

			// Token: 0x17001BC1 RID: 7105
			// (get) Token: 0x06008698 RID: 34456 RVA: 0x003A2AA8 File Offset: 0x003A0CA8
			// (set) Token: 0x06008699 RID: 34457 RVA: 0x003A2AB0 File Offset: 0x003A0CB0
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string Notes { get; set; }

			// Token: 0x17001BC2 RID: 7106
			// (get) Token: 0x0600869A RID: 34458 RVA: 0x003A2AB9 File Offset: 0x003A0CB9
			// (set) Token: 0x0600869B RID: 34459 RVA: 0x003A2AC1 File Offset: 0x003A0CC1
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(DecimalCount = 2)]
			public float Scale { get; set; }

			// Token: 0x17001BC3 RID: 7107
			// (get) Token: 0x0600869C RID: 34460 RVA: 0x003A2ACA File Offset: 0x003A0CCA
			// (set) Token: 0x0600869D RID: 34461 RVA: 0x003A2AD2 File Offset: 0x003A0CD2
			[Serialize(true, IsPropertySaveable.Yes, "Does the limb flip when the character flips?", "", false)]
			[Editable]
			public bool Flip { get; set; }

			// Token: 0x17001BC4 RID: 7108
			// (get) Token: 0x0600869E RID: 34462 RVA: 0x003A2ADB File Offset: 0x003A0CDB
			// (set) Token: 0x0600869F RID: 34463 RVA: 0x003A2AE3 File Offset: 0x003A0CE3
			[Serialize(false, IsPropertySaveable.Yes, "Currently only works with non-deformable (normal) sprites.", "", false)]
			[Editable]
			public bool MirrorVertically { get; set; }

			// Token: 0x17001BC5 RID: 7109
			// (get) Token: 0x060086A0 RID: 34464 RVA: 0x003A2AEC File Offset: 0x003A0CEC
			// (set) Token: 0x060086A1 RID: 34465 RVA: 0x003A2AF4 File Offset: 0x003A0CF4
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool MirrorHorizontally { get; set; }

			// Token: 0x17001BC6 RID: 7110
			// (get) Token: 0x060086A2 RID: 34466 RVA: 0x003A2AFD File Offset: 0x003A0CFD
			// (set) Token: 0x060086A3 RID: 34467 RVA: 0x003A2B05 File Offset: 0x003A0D05
			[Serialize(false, IsPropertySaveable.Yes, "Disable drawing for this limb.", "", false)]
			[Editable]
			public bool Hide { get; set; }

			// Token: 0x17001BC7 RID: 7111
			// (get) Token: 0x060086A4 RID: 34468 RVA: 0x003A2B0E File Offset: 0x003A0D0E
			// (set) Token: 0x060086A5 RID: 34469 RVA: 0x003A2B16 File Offset: 0x003A0D16
			[Serialize(float.NaN, IsPropertySaveable.Yes, "Orientation of the sprite as drawn on the spritesheet. Defines the \"forward direction\" of the sprite. Should be configured as the direction pointing outwards from the main limb.Incorrectly defined orientations may lead to limbs being rotated incorrectly when e.g. when the character aims or flips to face a different direction. Overrides the value of 'Spritesheet Orientation' for this limb.", "", false)]
			[Editable(-360, 360, ValueStep = 90f, DecimalCount = 0)]
			public float SpriteOrientation { get; set; }

			// Token: 0x17001BC8 RID: 7112
			// (get) Token: 0x060086A6 RID: 34470 RVA: 0x003A2B1F File Offset: 0x003A0D1F
			// (set) Token: 0x060086A7 RID: 34471 RVA: 0x003A2B27 File Offset: 0x003A0D27
			[Serialize(LimbType.None, IsPropertySaveable.Yes, "If set, the limb sprite will use the same sprite depth as the specified limb. Generally only useful for limbs that get added on the ragdoll on the fly (e.g. extra limbs added via gene splicing).", "", false)]
			public LimbType InheritLimbDepth { get; set; }

			// Token: 0x17001BC9 RID: 7113
			// (get) Token: 0x060086A8 RID: 34472 RVA: 0x003A2B30 File Offset: 0x003A0D30
			// (set) Token: 0x060086A9 RID: 34473 RVA: 0x003A2B38 File Offset: 0x003A0D38
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 500f)]
			public float SteerForce { get; set; }

			// Token: 0x17001BCA RID: 7114
			// (get) Token: 0x060086AA RID: 34474 RVA: 0x003A2B41 File Offset: 0x003A0D41
			// (set) Token: 0x060086AB RID: 34475 RVA: 0x003A2B49 File Offset: 0x003A0D49
			[Serialize(0f, IsPropertySaveable.Yes, "Radius of the collider.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Radius { get; set; }

			// Token: 0x17001BCB RID: 7115
			// (get) Token: 0x060086AC RID: 34476 RVA: 0x003A2B52 File Offset: 0x003A0D52
			// (set) Token: 0x060086AD RID: 34477 RVA: 0x003A2B5A File Offset: 0x003A0D5A
			[Serialize(0f, IsPropertySaveable.Yes, "Height of the collider.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Height { get; set; }

			// Token: 0x17001BCC RID: 7116
			// (get) Token: 0x060086AE RID: 34478 RVA: 0x003A2B63 File Offset: 0x003A0D63
			// (set) Token: 0x060086AF RID: 34479 RVA: 0x003A2B6B File Offset: 0x003A0D6B
			[Serialize(0f, IsPropertySaveable.Yes, "Width of the collider.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Width { get; set; }

			// Token: 0x17001BCD RID: 7117
			// (get) Token: 0x060086B0 RID: 34480 RVA: 0x003A2B74 File Offset: 0x003A0D74
			// (set) Token: 0x060086B1 RID: 34481 RVA: 0x003A2B7C File Offset: 0x003A0D7C
			[Serialize(10f, IsPropertySaveable.Yes, "The more the density the heavier the limb is.", "", false)]
			[Editable(MinValueFloat = 0.01f, MaxValueFloat = 100f, DecimalCount = 2)]
			public float Density { get; set; }

			// Token: 0x17001BCE RID: 7118
			// (get) Token: 0x060086B2 RID: 34482 RVA: 0x003A2B85 File Offset: 0x003A0D85
			// (set) Token: 0x060086B3 RID: 34483 RVA: 0x003A2B8D File Offset: 0x003A0D8D
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool IgnoreCollisions { get; set; }

			// Token: 0x17001BCF RID: 7119
			// (get) Token: 0x060086B4 RID: 34484 RVA: 0x003A2B96 File Offset: 0x003A0D96
			// (set) Token: 0x060086B5 RID: 34485 RVA: 0x003A2B9E File Offset: 0x003A0D9E
			[Serialize(7f, IsPropertySaveable.Yes, "Increasing the damping makes the limb stop rotating more quickly.", "", false)]
			[Editable]
			public float AngularDamping { get; set; }

			// Token: 0x17001BD0 RID: 7120
			// (get) Token: 0x060086B6 RID: 34486 RVA: 0x003A2BA7 File Offset: 0x003A0DA7
			// (set) Token: 0x060086B7 RID: 34487 RVA: 0x003A2BAF File Offset: 0x003A0DAF
			[Serialize(1f, IsPropertySaveable.Yes, "Higher values make AI characters prefer attacking this limb.", "", false)]
			[Editable(MinValueFloat = 0.1f, MaxValueFloat = 10f)]
			public float AttackPriority { get; set; }

			// Token: 0x17001BD1 RID: 7121
			// (get) Token: 0x060086B8 RID: 34488 RVA: 0x003A2BB8 File Offset: 0x003A0DB8
			// (set) Token: 0x060086B9 RID: 34489 RVA: 0x003A2BC0 File Offset: 0x003A0DC0
			[Serialize("0, 0", IsPropertySaveable.Yes, "The position which is used to lead the IK chain to the IK goal. Only applicable if the limb is hand or foot.", "", false)]
			[Editable]
			public Vector2 PullPos { get; set; }

			// Token: 0x17001BD2 RID: 7122
			// (get) Token: 0x060086BA RID: 34490 RVA: 0x003A2BC9 File Offset: 0x003A0DC9
			// (set) Token: 0x060086BB RID: 34491 RVA: 0x003A2BD1 File Offset: 0x003A0DD1
			[Serialize("0, 0", IsPropertySaveable.Yes, "Only applicable if this limb is a foot. Determines the \"neutral position\" of the foot relative to a joint determined by the \"RefJoint\" parameter. For example, a value of {-100, 0} would mean that the foot is positioned on the floor, 100 units behind the reference joint.", "", false)]
			[Editable]
			public Vector2 StepOffset { get; set; }

			// Token: 0x17001BD3 RID: 7123
			// (get) Token: 0x060086BC RID: 34492 RVA: 0x003A2BDA File Offset: 0x003A0DDA
			// (set) Token: 0x060086BD RID: 34493 RVA: 0x003A2BE2 File Offset: 0x003A0DE2
			[Serialize(-1, IsPropertySaveable.Yes, "The id of the refecence joint. Determines which joint is used as the \"neutral x-position\" for the foot movement. For example in the case of a humanoid-shaped characters this would usually be the waist. The position can be offset using the StepOffset parameter. Only applicable if this limb is a foot.", "", false)]
			[Editable]
			public int RefJoint { get; set; }

			// Token: 0x17001BD4 RID: 7124
			// (get) Token: 0x060086BE RID: 34494 RVA: 0x003A2BEB File Offset: 0x003A0DEB
			// (set) Token: 0x060086BF RID: 34495 RVA: 0x003A2BF3 File Offset: 0x003A0DF3
			[Serialize("0, 0", IsPropertySaveable.Yes, "Relative offset for the mouth position (starting from the center). Only applicable for LimbType.Head. Used for eating.", "", false)]
			[Editable(DecimalCount = 2, MinValueFloat = -10f, MaxValueFloat = 10f)]
			public Vector2 MouthPos { get; set; }

			// Token: 0x17001BD5 RID: 7125
			// (get) Token: 0x060086C0 RID: 34496 RVA: 0x003A2BFC File Offset: 0x003A0DFC
			// (set) Token: 0x060086C1 RID: 34497 RVA: 0x003A2C04 File Offset: 0x003A0E04
			[Serialize(50f, IsPropertySaveable.Yes, "How much torque is applied on the head while updating the eating animations?", "", false)]
			[Editable]
			public float EatTorque { get; set; }

			// Token: 0x17001BD6 RID: 7126
			// (get) Token: 0x060086C2 RID: 34498 RVA: 0x003A2C0D File Offset: 0x003A0E0D
			// (set) Token: 0x060086C3 RID: 34499 RVA: 0x003A2C15 File Offset: 0x003A0E15
			[Serialize(2f, IsPropertySaveable.Yes, "How strong a linear impulse is applied on the head while updating the eating animations?", "", false)]
			[Editable]
			public float EatImpulse { get; set; }

			// Token: 0x17001BD7 RID: 7127
			// (get) Token: 0x060086C4 RID: 34500 RVA: 0x003A2C1E File Offset: 0x003A0E1E
			// (set) Token: 0x060086C5 RID: 34501 RVA: 0x003A2C26 File Offset: 0x003A0E26
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float ConstantTorque { get; set; }

			// Token: 0x17001BD8 RID: 7128
			// (get) Token: 0x060086C6 RID: 34502 RVA: 0x003A2C2F File Offset: 0x003A0E2F
			// (set) Token: 0x060086C7 RID: 34503 RVA: 0x003A2C37 File Offset: 0x003A0E37
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float ConstantAngle { get; set; }

			// Token: 0x17001BD9 RID: 7129
			// (get) Token: 0x060086C8 RID: 34504 RVA: 0x003A2C40 File Offset: 0x003A0E40
			// (set) Token: 0x060086C9 RID: 34505 RVA: 0x003A2C48 File Offset: 0x003A0E48
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(DecimalCount = 2, MinValueFloat = 0f, MaxValueFloat = 10f)]
			public float AttackForceMultiplier { get; set; }

			// Token: 0x17001BDA RID: 7130
			// (get) Token: 0x060086CA RID: 34506 RVA: 0x003A2C51 File Offset: 0x003A0E51
			// (set) Token: 0x060086CB RID: 34507 RVA: 0x003A2C59 File Offset: 0x003A0E59
			[Serialize(1f, IsPropertySaveable.Yes, "How much damage must be done by the attack in order to be able to cut off the limb. Note that it's evaluated after the damage modifiers.", "", false)]
			[Editable(DecimalCount = 0, MinValueFloat = 0f, MaxValueFloat = 1000f)]
			public float MinSeveranceDamage { get; set; }

			// Token: 0x17001BDB RID: 7131
			// (get) Token: 0x060086CC RID: 34508 RVA: 0x003A2C62 File Offset: 0x003A0E62
			// (set) Token: 0x060086CD RID: 34509 RVA: 0x003A2C6A File Offset: 0x003A0E6A
			[Serialize(true, IsPropertySaveable.Yes, "Disable if you don't want to allow severing this joint while the creature is alive. Note: Does nothing if the 'Severance Probability Modifier' in the joint settings is 0 (default). Also note that the setting doesn't override certain limitations, e.g. severing the main limb, or legs of a walking creature is not allowed.", "", false)]
			[Editable]
			public bool CanBeSeveredAlive { get; set; }

			// Token: 0x17001BDC RID: 7132
			// (get) Token: 0x060086CE RID: 34510 RVA: 0x003A2C73 File Offset: 0x003A0E73
			// (set) Token: 0x060086CF RID: 34511 RVA: 0x003A2C7B File Offset: 0x003A0E7B
			[Serialize(10f, IsPropertySaveable.Yes, "How long it takes for the severed limb to fade out", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, ValueStep = 1f)]
			public float SeveredFadeOutTime { get; set; } = 10f;

			// Token: 0x17001BDD RID: 7133
			// (get) Token: 0x060086D0 RID: 34512 RVA: 0x003A2C84 File Offset: 0x003A0E84
			// (set) Token: 0x060086D1 RID: 34513 RVA: 0x003A2C8C File Offset: 0x003A0E8C
			[Serialize(false, IsPropertySaveable.Yes, "Should the tail angle be applied on this limb? If none of the limbs have been defined to use the angle and an angle is defined in the animation parameters, the first tail limb is used.", "", false)]
			[Editable]
			public bool ApplyTailAngle { get; set; }

			// Token: 0x17001BDE RID: 7134
			// (get) Token: 0x060086D2 RID: 34514 RVA: 0x003A2C95 File Offset: 0x003A0E95
			// (set) Token: 0x060086D3 RID: 34515 RVA: 0x003A2C9D File Offset: 0x003A0E9D
			[Serialize(false, IsPropertySaveable.Yes, "Should this limb be moved like a tail when swimming? Always true for tail limbs. On tails, disable by setting SineFrequencyMultiplier to 0.", "", false)]
			[Editable]
			public bool ApplySineMovement { get; set; }

			// Token: 0x17001BDF RID: 7135
			// (get) Token: 0x060086D4 RID: 34516 RVA: 0x003A2CA6 File Offset: 0x003A0EA6
			// (set) Token: 0x060086D5 RID: 34517 RVA: 0x003A2CAE File Offset: 0x003A0EAE
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(ValueStep = 0.1f, DecimalCount = 2)]
			public float SineFrequencyMultiplier { get; set; }

			// Token: 0x17001BE0 RID: 7136
			// (get) Token: 0x060086D6 RID: 34518 RVA: 0x003A2CB7 File Offset: 0x003A0EB7
			// (set) Token: 0x060086D7 RID: 34519 RVA: 0x003A2CBF File Offset: 0x003A0EBF
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(ValueStep = 0.1f, DecimalCount = 2)]
			public float SineAmplitudeMultiplier { get; set; }

			// Token: 0x17001BE1 RID: 7137
			// (get) Token: 0x060086D8 RID: 34520 RVA: 0x003A2CC8 File Offset: 0x003A0EC8
			// (set) Token: 0x060086D9 RID: 34521 RVA: 0x003A2CD0 File Offset: 0x003A0ED0
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0, 100, ValueStep = 1f, DecimalCount = 1)]
			public float BlinkFrequency { get; set; }

			// Token: 0x17001BE2 RID: 7138
			// (get) Token: 0x060086DA RID: 34522 RVA: 0x003A2CD9 File Offset: 0x003A0ED9
			// (set) Token: 0x060086DB RID: 34523 RVA: 0x003A2CE1 File Offset: 0x003A0EE1
			[Serialize(0.2f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0.01f, 10f, 1, ValueStep = 1f, DecimalCount = 2)]
			public float BlinkDurationIn { get; set; }

			// Token: 0x17001BE3 RID: 7139
			// (get) Token: 0x060086DC RID: 34524 RVA: 0x003A2CEA File Offset: 0x003A0EEA
			// (set) Token: 0x060086DD RID: 34525 RVA: 0x003A2CF2 File Offset: 0x003A0EF2
			[Serialize(0.5f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0.01f, 10f, 1, ValueStep = 1f, DecimalCount = 2)]
			public float BlinkDurationOut { get; set; }

			// Token: 0x17001BE4 RID: 7140
			// (get) Token: 0x060086DE RID: 34526 RVA: 0x003A2CFB File Offset: 0x003A0EFB
			// (set) Token: 0x060086DF RID: 34527 RVA: 0x003A2D03 File Offset: 0x003A0F03
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0, 10, ValueStep = 1f, DecimalCount = 2)]
			public float BlinkHoldTime { get; set; }

			// Token: 0x17001BE5 RID: 7141
			// (get) Token: 0x060086E0 RID: 34528 RVA: 0x003A2D0C File Offset: 0x003A0F0C
			// (set) Token: 0x060086E1 RID: 34529 RVA: 0x003A2D14 File Offset: 0x003A0F14
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(-360, 360, ValueStep = 1f, DecimalCount = 0)]
			public float BlinkRotationIn { get; set; }

			// Token: 0x17001BE6 RID: 7142
			// (get) Token: 0x060086E2 RID: 34530 RVA: 0x003A2D1D File Offset: 0x003A0F1D
			// (set) Token: 0x060086E3 RID: 34531 RVA: 0x003A2D25 File Offset: 0x003A0F25
			[Serialize(45f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(-360, 360, ValueStep = 1f, DecimalCount = 0)]
			public float BlinkRotationOut { get; set; }

			// Token: 0x17001BE7 RID: 7143
			// (get) Token: 0x060086E4 RID: 34532 RVA: 0x003A2D2E File Offset: 0x003A0F2E
			// (set) Token: 0x060086E5 RID: 34533 RVA: 0x003A2D36 File Offset: 0x003A0F36
			[Serialize(50f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float BlinkForce { get; set; }

			// Token: 0x17001BE8 RID: 7144
			// (get) Token: 0x060086E6 RID: 34534 RVA: 0x003A2D3F File Offset: 0x003A0F3F
			// (set) Token: 0x060086E7 RID: 34535 RVA: 0x003A2D47 File Offset: 0x003A0F47
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool OnlyBlinkInWater { get; set; }

			// Token: 0x17001BE9 RID: 7145
			// (get) Token: 0x060086E8 RID: 34536 RVA: 0x003A2D50 File Offset: 0x003A0F50
			// (set) Token: 0x060086E9 RID: 34537 RVA: 0x003A2D58 File Offset: 0x003A0F58
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool UseTextureOffsetForBlinking { get; set; }

			// Token: 0x17001BEA RID: 7146
			// (get) Token: 0x060086EA RID: 34538 RVA: 0x003A2D61 File Offset: 0x003A0F61
			// (set) Token: 0x060086EB RID: 34539 RVA: 0x003A2D69 File Offset: 0x003A0F69
			[Serialize("0.5, 0.5", IsPropertySaveable.Yes, "", "", false)]
			[Editable(DecimalCount = 2, MinValueFloat = 0f, MaxValueFloat = 1f)]
			public Vector2 BlinkTextureOffsetIn { get; set; }

			// Token: 0x17001BEB RID: 7147
			// (get) Token: 0x060086EC RID: 34540 RVA: 0x003A2D72 File Offset: 0x003A0F72
			// (set) Token: 0x060086ED RID: 34541 RVA: 0x003A2D7A File Offset: 0x003A0F7A
			[Serialize("0.5, 0.5", IsPropertySaveable.Yes, "", "", false)]
			[Editable(DecimalCount = 2, MinValueFloat = 0f, MaxValueFloat = 1f)]
			public Vector2 BlinkTextureOffsetOut { get; set; }

			// Token: 0x17001BEC RID: 7148
			// (get) Token: 0x060086EE RID: 34542 RVA: 0x003A2D83 File Offset: 0x003A0F83
			// (set) Token: 0x060086EF RID: 34543 RVA: 0x003A2D8B File Offset: 0x003A0F8B
			[Serialize(TransitionMode.Linear, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public TransitionMode BlinkTransitionIn { get; private set; }

			// Token: 0x17001BED RID: 7149
			// (get) Token: 0x060086F0 RID: 34544 RVA: 0x003A2D94 File Offset: 0x003A0F94
			// (set) Token: 0x060086F1 RID: 34545 RVA: 0x003A2D9C File Offset: 0x003A0F9C
			[Serialize(TransitionMode.Linear, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public TransitionMode BlinkTransitionOut { get; private set; }

			// Token: 0x17001BEE RID: 7150
			// (get) Token: 0x060086F2 RID: 34546 RVA: 0x003A2DA5 File Offset: 0x003A0FA5
			// (set) Token: 0x060086F3 RID: 34547 RVA: 0x003A2DAD File Offset: 0x003A0FAD
			[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
			public int HealthIndex { get; set; }

			// Token: 0x17001BEF RID: 7151
			// (get) Token: 0x060086F4 RID: 34548 RVA: 0x003A2DB6 File Offset: 0x003A0FB6
			// (set) Token: 0x060086F5 RID: 34549 RVA: 0x003A2DBE File Offset: 0x003A0FBE
			[Serialize(0.3f, IsPropertySaveable.Yes, "", "", false)]
			public float Friction { get; set; }

			// Token: 0x17001BF0 RID: 7152
			// (get) Token: 0x060086F6 RID: 34550 RVA: 0x003A2DC7 File Offset: 0x003A0FC7
			// (set) Token: 0x060086F7 RID: 34551 RVA: 0x003A2DCF File Offset: 0x003A0FCF
			[Serialize(0.05f, IsPropertySaveable.Yes, "", "", false)]
			public float Restitution { get; set; }

			// Token: 0x17001BF1 RID: 7153
			// (get) Token: 0x060086F8 RID: 34552 RVA: 0x003A2DD8 File Offset: 0x003A0FD8
			// (set) Token: 0x060086F9 RID: 34553 RVA: 0x003A2DE0 File Offset: 0x003A0FE0
			[Serialize(true, IsPropertySaveable.Yes, "Can the limb enter submarines? Only valid if the ragdoll's CanEnterSubmarine is set to Partial, otherwise the limb can enter if the ragdoll can.", "", false)]
			[Editable]
			public bool CanEnterSubmarine { get; private set; }

			// Token: 0x17001BF2 RID: 7154
			// (get) Token: 0x060086FA RID: 34554 RVA: 0x003A2DE9 File Offset: 0x003A0FE9
			// (set) Token: 0x060086FB RID: 34555 RVA: 0x003A2DF1 File Offset: 0x003A0FF1
			[Serialize(LimbType.None, IsPropertySaveable.Yes, "When set to something else than None, this limb will be hidden if the limb of the specified type is hidden.", "", false)]
			[Editable]
			public LimbType InheritHiding { get; set; }

			// Token: 0x060086FC RID: 34556 RVA: 0x003A2DFC File Offset: 0x003A0FFC
			public LimbParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				ContentXElement spriteElement = element.GetChildElement("sprite");
				ContentXElement contentXElement = null;
				if (spriteElement != contentXElement)
				{
					this.normalSpriteParams = new RagdollParams.SpriteParams(spriteElement, ragdoll);
					base.SubParams.Add(this.normalSpriteParams);
				}
				ContentXElement damagedSpriteElement = element.GetChildElement("damagedsprite");
				contentXElement = null;
				if (damagedSpriteElement != contentXElement)
				{
					this.damagedSpriteParams = new RagdollParams.SpriteParams(damagedSpriteElement, ragdoll);
				}
				ContentXElement deformSpriteElement = element.GetChildElement("deformablesprite");
				contentXElement = null;
				if (deformSpriteElement != contentXElement)
				{
					this.deformSpriteParams = new RagdollParams.DeformSpriteParams(deformSpriteElement, ragdoll);
					base.SubParams.Add(this.deformSpriteParams);
				}
				foreach (ContentXElement decorativeSpriteElement in element.GetChildElements("decorativesprite"))
				{
					RagdollParams.DecorativeSpriteParams decorativeParams = new RagdollParams.DecorativeSpriteParams(decorativeSpriteElement, ragdoll);
					this.decorativeSpriteParams.Add(decorativeParams);
					base.SubParams.Add(decorativeParams);
				}
				ContentXElement attackElement = element.GetChildElement("attack");
				contentXElement = null;
				if (attackElement != contentXElement)
				{
					this.Attack = new RagdollParams.AttackParams(attackElement, ragdoll);
					base.SubParams.Add(this.Attack);
				}
				foreach (ContentXElement damageElement in element.GetChildElements("damagemodifier"))
				{
					RagdollParams.DamageModifierParams damageModifier = new RagdollParams.DamageModifierParams(damageElement, ragdoll);
					this.DamageModifiers.Add(damageModifier);
					base.SubParams.Add(damageModifier);
				}
				ContentXElement soundElement = element.GetChildElement("sound");
				contentXElement = null;
				if (soundElement != contentXElement)
				{
					this.Sound = new RagdollParams.SoundParams(soundElement, ragdoll);
					base.SubParams.Add(this.Sound);
				}
				ContentXElement lightElement = element.GetChildElement("lightsource");
				contentXElement = null;
				if (lightElement != contentXElement)
				{
					this.LightSource = new RagdollParams.LightSourceParams(lightElement, ragdoll);
					base.SubParams.Add(this.LightSource);
				}
			}

			// Token: 0x060086FD RID: 34557 RVA: 0x003A3044 File Offset: 0x003A1244
			public bool AddAttack()
			{
				if (this.Attack != null)
				{
					return false;
				}
				RagdollParams.AttackParams newAttack;
				this.TryAddSubParam<RagdollParams.AttackParams>(base.CreateElement("attack", Array.Empty<object>()), (ContentXElement e, RagdollParams c) => new RagdollParams.AttackParams(e, c), out newAttack, null, null);
				this.Attack = newAttack;
				return this.Attack != null;
			}

			// Token: 0x060086FE RID: 34558 RVA: 0x003A30A8 File Offset: 0x003A12A8
			public bool AddSound()
			{
				if (this.Sound != null)
				{
					return false;
				}
				RagdollParams.SoundParams newSound;
				this.TryAddSubParam<RagdollParams.SoundParams>(base.CreateElement("sound", Array.Empty<object>()), (ContentXElement e, RagdollParams c) => new RagdollParams.SoundParams(e, c), out newSound, null, null);
				this.Sound = newSound;
				return this.Sound != null;
			}

			// Token: 0x060086FF RID: 34559 RVA: 0x003A310C File Offset: 0x003A130C
			public bool AddLight()
			{
				if (this.LightSource != null)
				{
					return false;
				}
				ContentXElement lightSourceElement = base.CreateElement("lightsource", new object[]
				{
					new XElement("lighttexture", new XAttribute("texture", "Content/Lights/pointlight_bright.png"))
				});
				RagdollParams.LightSourceParams newLightSource;
				this.TryAddSubParam<RagdollParams.LightSourceParams>(lightSourceElement, (ContentXElement e, RagdollParams c) => new RagdollParams.LightSourceParams(e, c), out newLightSource, null, null);
				this.LightSource = newLightSource;
				return this.LightSource != null;
			}

			// Token: 0x06008700 RID: 34560 RVA: 0x003A3198 File Offset: 0x003A1398
			public bool AddDamageModifier()
			{
				RagdollParams.DamageModifierParams damageModifierParams;
				return this.TryAddSubParam<RagdollParams.DamageModifierParams>(base.CreateElement("damagemodifier", Array.Empty<object>()), (ContentXElement e, RagdollParams c) => new RagdollParams.DamageModifierParams(e, c), out damageModifierParams, this.DamageModifiers, null);
			}

			// Token: 0x06008701 RID: 34561 RVA: 0x003A31E3 File Offset: 0x003A13E3
			public bool RemoveAttack()
			{
				if (this.RemoveSubParam<RagdollParams.AttackParams>(this.Attack, null))
				{
					this.Attack = null;
					return true;
				}
				return false;
			}

			// Token: 0x06008702 RID: 34562 RVA: 0x003A31FE File Offset: 0x003A13FE
			public bool RemoveSound()
			{
				if (this.RemoveSubParam<RagdollParams.SoundParams>(this.Sound, null))
				{
					this.Sound = null;
					return true;
				}
				return false;
			}

			// Token: 0x06008703 RID: 34563 RVA: 0x003A3219 File Offset: 0x003A1419
			public bool RemoveLight()
			{
				if (this.RemoveSubParam<RagdollParams.LightSourceParams>(this.LightSource, null))
				{
					this.LightSource = null;
					return true;
				}
				return false;
			}

			// Token: 0x06008704 RID: 34564 RVA: 0x003A3234 File Offset: 0x003A1434
			public bool RemoveDamageModifier(RagdollParams.DamageModifierParams damageModifier)
			{
				return this.RemoveSubParam<RagdollParams.DamageModifierParams>(damageModifier, this.DamageModifiers);
			}

			// Token: 0x06008705 RID: 34565 RVA: 0x003A3244 File Offset: 0x003A1444
			protected bool TryAddSubParam<T>(ContentXElement element, Func<ContentXElement, RagdollParams, T> constructor, out T subParam, IList<T> collection = null, Func<IList<T>, bool> filter = null) where T : RagdollParams.SubParam
			{
				subParam = constructor(element, base.Ragdoll);
				if (collection != null && filter != null && filter(collection))
				{
					return false;
				}
				base.Element.Add(element);
				base.SubParams.Add(subParam);
				if (collection != null)
				{
					collection.Add(subParam);
				}
				return subParam != null;
			}

			// Token: 0x06008706 RID: 34566 RVA: 0x003A32BC File Offset: 0x003A14BC
			protected bool RemoveSubParam<T>(T subParam, IList<T> collection = null) where T : RagdollParams.SubParam
			{
				if (subParam != null)
				{
					ContentXElement element = subParam.Element;
					ContentXElement contentXElement = null;
					if (!(element == contentXElement))
					{
						ContentXElement parent = subParam.Element.Parent;
						ContentXElement contentXElement2 = null;
						if (!(parent == contentXElement2))
						{
							if (collection != null && !collection.Contains(subParam))
							{
								return false;
							}
							if (!base.SubParams.Contains(subParam))
							{
								return false;
							}
							if (collection != null)
							{
								collection.Remove(subParam);
							}
							base.SubParams.Remove(subParam);
							subParam.Element.Remove();
							return true;
						}
					}
				}
				return false;
			}

			// Token: 0x040053B3 RID: 21427
			public readonly RagdollParams.SpriteParams normalSpriteParams;

			// Token: 0x040053B4 RID: 21428
			public readonly RagdollParams.SpriteParams damagedSpriteParams;

			// Token: 0x040053B5 RID: 21429
			public readonly RagdollParams.DeformSpriteParams deformSpriteParams;

			// Token: 0x040053B6 RID: 21430
			public readonly List<RagdollParams.DecorativeSpriteParams> decorativeSpriteParams = new List<RagdollParams.DecorativeSpriteParams>();

			// Token: 0x040053BB RID: 21435
			private string name;
		}

		// Token: 0x02000EC9 RID: 3785
		public class DecorativeSpriteParams : RagdollParams.SpriteParams
		{
			// Token: 0x06008707 RID: 34567 RVA: 0x003A335B File Offset: 0x003A155B
			public DecorativeSpriteParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				this.DecorativeSprite = new DecorativeSprite(element, "", "", false);
			}

			// Token: 0x17001BF3 RID: 7155
			// (get) Token: 0x06008708 RID: 34568 RVA: 0x003A337C File Offset: 0x003A157C
			// (set) Token: 0x06008709 RID: 34569 RVA: 0x003A3384 File Offset: 0x003A1584
			public DecorativeSprite DecorativeSprite { get; private set; }

			// Token: 0x0600870A RID: 34570 RVA: 0x003A338D File Offset: 0x003A158D
			public override bool Deserialize(ContentXElement element = null, bool recursive = true)
			{
				base.Deserialize(element, recursive);
				this.DecorativeSprite.SerializableProperties = SerializableProperty.DeserializeProperties(this.DecorativeSprite, element ?? base.Element);
				return base.SerializableProperties != null;
			}

			// Token: 0x0600870B RID: 34571 RVA: 0x003A33C7 File Offset: 0x003A15C7
			public override bool Serialize(ContentXElement element = null, bool recursive = true)
			{
				base.Serialize(element, recursive);
				SerializableProperty.SerializeProperties(this.DecorativeSprite, element ?? base.Element, false, false);
				return true;
			}

			// Token: 0x0600870C RID: 34572 RVA: 0x003A33F0 File Offset: 0x003A15F0
			public override void Reset()
			{
				base.Reset();
				this.DecorativeSprite.SerializableProperties = SerializableProperty.DeserializeProperties(this.DecorativeSprite, base.OriginalElement);
			}
		}

		// Token: 0x02000ECA RID: 3786
		public class DeformSpriteParams : RagdollParams.SpriteParams
		{
			// Token: 0x17001BF4 RID: 7156
			// (get) Token: 0x0600870D RID: 34573 RVA: 0x003A3419 File Offset: 0x003A1619
			// (set) Token: 0x0600870E RID: 34574 RVA: 0x003A3421 File Offset: 0x003A1621
			public RagdollParams.DeformationParams Deformation { get; private set; }

			// Token: 0x0600870F RID: 34575 RVA: 0x003A342A File Offset: 0x003A162A
			public DeformSpriteParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				this.Deformation = new RagdollParams.DeformationParams(element, ragdoll);
				base.SubParams.Add(this.Deformation);
			}
		}

		// Token: 0x02000ECB RID: 3787
		public class SpriteParams : RagdollParams.SubParam
		{
			// Token: 0x17001BF5 RID: 7157
			// (get) Token: 0x06008710 RID: 34576 RVA: 0x003A3452 File Offset: 0x003A1652
			// (set) Token: 0x06008711 RID: 34577 RVA: 0x003A345A File Offset: 0x003A165A
			[Serialize("0, 0, 0, 0", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public Rectangle SourceRect { get; set; }

			// Token: 0x17001BF6 RID: 7158
			// (get) Token: 0x06008712 RID: 34578 RVA: 0x003A3463 File Offset: 0x003A1663
			// (set) Token: 0x06008713 RID: 34579 RVA: 0x003A346B File Offset: 0x003A166B
			[Serialize("0.5, 0.5", IsPropertySaveable.Yes, "The origin of the sprite relative to the collider.", "", false)]
			[Editable(DecimalCount = 3)]
			public Vector2 Origin { get; set; }

			// Token: 0x17001BF7 RID: 7159
			// (get) Token: 0x06008714 RID: 34580 RVA: 0x003A3474 File Offset: 0x003A1674
			// (set) Token: 0x06008715 RID: 34581 RVA: 0x003A347C File Offset: 0x003A167C
			[Serialize(0f, IsPropertySaveable.Yes, "The Z-depth of the limb relative to other limbs of the same character. 1 is front, 0 is behind.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 3)]
			public float Depth { get; set; }

			// Token: 0x17001BF8 RID: 7160
			// (get) Token: 0x06008716 RID: 34582 RVA: 0x003A3485 File Offset: 0x003A1685
			// (set) Token: 0x06008717 RID: 34583 RVA: 0x003A348D File Offset: 0x003A168D
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string Texture { get; set; }

			// Token: 0x17001BF9 RID: 7161
			// (get) Token: 0x06008718 RID: 34584 RVA: 0x003A3496 File Offset: 0x003A1696
			// (set) Token: 0x06008719 RID: 34585 RVA: 0x003A349E File Offset: 0x003A169E
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool IgnoreTint { get; set; }

			// Token: 0x17001BFA RID: 7162
			// (get) Token: 0x0600871A RID: 34586 RVA: 0x003A34A7 File Offset: 0x003A16A7
			// (set) Token: 0x0600871B RID: 34587 RVA: 0x003A34AF File Offset: 0x003A16AF
			[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public Color Color { get; set; }

			// Token: 0x17001BFB RID: 7163
			// (get) Token: 0x0600871C RID: 34588 RVA: 0x003A34B8 File Offset: 0x003A16B8
			// (set) Token: 0x0600871D RID: 34589 RVA: 0x003A34C0 File Offset: 0x003A16C0
			[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "Target color when the character is dead.", "", false)]
			[Editable]
			public Color DeadColor { get; set; }

			// Token: 0x17001BFC RID: 7164
			// (get) Token: 0x0600871E RID: 34590 RVA: 0x003A34C9 File Offset: 0x003A16C9
			// (set) Token: 0x0600871F RID: 34591 RVA: 0x003A34D1 File Offset: 0x003A16D1
			[Serialize(0f, IsPropertySaveable.Yes, "How long it takes to fade into the dead color? 0 = Not applied.", "", false)]
			[Editable(DecimalCount = 1, MinValueFloat = 0f, MaxValueFloat = 10f)]
			public float DeadColorTime { get; set; }

			// Token: 0x17001BFD RID: 7165
			// (get) Token: 0x06008720 RID: 34592 RVA: 0x003A34DA File Offset: 0x003A16DA
			public override string Name
			{
				get
				{
					return "Sprite";
				}
			}

			// Token: 0x06008721 RID: 34593 RVA: 0x003A34E1 File Offset: 0x003A16E1
			public SpriteParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
			}

			// Token: 0x06008722 RID: 34594 RVA: 0x003A34EB File Offset: 0x003A16EB
			public string GetTexturePath()
			{
				if (!string.IsNullOrWhiteSpace(this.Texture))
				{
					return this.Texture;
				}
				return base.Ragdoll.Texture;
			}
		}

		// Token: 0x02000ECC RID: 3788
		public class DeformationParams : RagdollParams.SubParam
		{
			// Token: 0x06008723 RID: 34595 RVA: 0x003A350C File Offset: 0x003A170C
			public DeformationParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				this.Deformations = new Dictionary<SpriteDeformationParams, XElement>();
				foreach (ContentXElement deformationElement in element.GetChildElements("spritedeformation"))
				{
					string typeName = deformationElement.GetAttributeString("type", null) ?? deformationElement.GetAttributeString("typename", string.Empty);
					SpriteDeformationParams deformation = null;
					string a = typeName.ToLowerInvariant();
					if (!(a == "inflate"))
					{
						if (!(a == "custom"))
						{
							if (!(a == "noise"))
							{
								if (!(a == "jointbend") && !(a == "bendjoint"))
								{
									if (!(a == "reacttotriggerers"))
									{
										DebugConsole.ThrowError("SpriteDeformationParams not implemented: '" + typeName + "'", null, element.ContentPackage, false, false);
									}
									else
									{
										deformation = new PositionalDeformationParams(deformationElement);
									}
								}
								else
								{
									deformation = new JointBendDeformationParams(deformationElement);
								}
							}
							else
							{
								deformation = new NoiseDeformationParams(deformationElement);
							}
						}
						else
						{
							deformation = new CustomDeformationParams(deformationElement);
						}
					}
					else
					{
						deformation = new InflateParams(deformationElement);
					}
					if (deformation != null)
					{
						deformation.Type = typeName;
					}
					this.Deformations.Add(deformation, deformationElement);
				}
			}

			// Token: 0x17001BFE RID: 7166
			// (get) Token: 0x06008724 RID: 34596 RVA: 0x003A3680 File Offset: 0x003A1880
			// (set) Token: 0x06008725 RID: 34597 RVA: 0x003A3688 File Offset: 0x003A1888
			public Dictionary<SpriteDeformationParams, XElement> Deformations { get; private set; }

			// Token: 0x06008726 RID: 34598 RVA: 0x003A3691 File Offset: 0x003A1891
			public override bool Deserialize(ContentXElement element = null, bool recursive = true)
			{
				base.Deserialize(element, recursive);
				this.Deformations.ForEach(delegate(KeyValuePair<SpriteDeformationParams, XElement> d)
				{
					d.Key.SerializableProperties = SerializableProperty.DeserializeProperties(d.Key, d.Value);
				});
				return base.SerializableProperties != null;
			}

			// Token: 0x06008727 RID: 34599 RVA: 0x003A36CF File Offset: 0x003A18CF
			public override bool Serialize(ContentXElement element = null, bool recursive = true)
			{
				base.Serialize(element, recursive);
				this.Deformations.ForEach(delegate(KeyValuePair<SpriteDeformationParams, XElement> d)
				{
					SerializableProperty.SerializeProperties(d.Key, d.Value, false, false);
				});
				return true;
			}

			// Token: 0x06008728 RID: 34600 RVA: 0x003A3705 File Offset: 0x003A1905
			public override void Reset()
			{
				base.Reset();
				this.Deformations.ForEach(delegate(KeyValuePair<SpriteDeformationParams, XElement> d)
				{
					d.Key.SerializableProperties = SerializableProperty.DeserializeProperties(d.Key, d.Value);
				});
			}
		}

		// Token: 0x02000ECD RID: 3789
		public class ColliderParams : RagdollParams.SubParam
		{
			// Token: 0x17001BFF RID: 7167
			// (get) Token: 0x06008729 RID: 34601 RVA: 0x003A3737 File Offset: 0x003A1937
			// (set) Token: 0x0600872A RID: 34602 RVA: 0x003A3758 File Offset: 0x003A1958
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public override string Name
			{
				get
				{
					if (string.IsNullOrWhiteSpace(this.name))
					{
						this.name = this.GenerateName();
					}
					return this.name;
				}
				set
				{
					this.name = value;
				}
			}

			// Token: 0x17001C00 RID: 7168
			// (get) Token: 0x0600872B RID: 34603 RVA: 0x003A3761 File Offset: 0x003A1961
			// (set) Token: 0x0600872C RID: 34604 RVA: 0x003A3769 File Offset: 0x003A1969
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Radius { get; set; }

			// Token: 0x17001C01 RID: 7169
			// (get) Token: 0x0600872D RID: 34605 RVA: 0x003A3772 File Offset: 0x003A1972
			// (set) Token: 0x0600872E RID: 34606 RVA: 0x003A377A File Offset: 0x003A197A
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Height { get; set; }

			// Token: 0x17001C02 RID: 7170
			// (get) Token: 0x0600872F RID: 34607 RVA: 0x003A3783 File Offset: 0x003A1983
			// (set) Token: 0x06008730 RID: 34608 RVA: 0x003A378B File Offset: 0x003A198B
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Width { get; set; }

			// Token: 0x17001C03 RID: 7171
			// (get) Token: 0x06008731 RID: 34609 RVA: 0x003A3794 File Offset: 0x003A1994
			// (set) Token: 0x06008732 RID: 34610 RVA: 0x003A379C File Offset: 0x003A199C
			[Serialize(BodyType.Dynamic, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public BodyType BodyType { get; set; }

			// Token: 0x06008733 RID: 34611 RVA: 0x003A37A5 File Offset: 0x003A19A5
			public ColliderParams(ContentXElement element, RagdollParams ragdoll, string name = null) : base(element, ragdoll)
			{
				this.Name = name;
			}

			// Token: 0x040053FC RID: 21500
			private string name;
		}

		// Token: 0x02000ECE RID: 3790
		public class LightSourceParams : RagdollParams.SubParam
		{
			// Token: 0x17001C04 RID: 7172
			// (get) Token: 0x06008734 RID: 34612 RVA: 0x003A37B6 File Offset: 0x003A19B6
			// (set) Token: 0x06008735 RID: 34613 RVA: 0x003A37BE File Offset: 0x003A19BE
			public RagdollParams.LightSourceParams.LightTexture Texture { get; private set; }

			// Token: 0x17001C05 RID: 7173
			// (get) Token: 0x06008736 RID: 34614 RVA: 0x003A37C7 File Offset: 0x003A19C7
			// (set) Token: 0x06008737 RID: 34615 RVA: 0x003A37CF File Offset: 0x003A19CF
			public Barotrauma.Lights.LightSourceParams LightSource { get; private set; }

			// Token: 0x06008738 RID: 34616 RVA: 0x003A37D8 File Offset: 0x003A19D8
			public LightSourceParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				this.LightSource = new Barotrauma.Lights.LightSourceParams(element);
				ContentXElement lightTextureElement = element.GetChildElement("lighttexture");
				ContentXElement contentXElement = null;
				if (lightTextureElement != contentXElement)
				{
					this.Texture = new RagdollParams.LightSourceParams.LightTexture(lightTextureElement, ragdoll);
					base.SubParams.Add(this.Texture);
				}
			}

			// Token: 0x06008739 RID: 34617 RVA: 0x003A3830 File Offset: 0x003A1A30
			public override bool Deserialize(ContentXElement element = null, bool recursive = true)
			{
				base.Deserialize(element, recursive);
				this.LightSource.Deserialize(element ?? base.Element);
				return base.SerializableProperties != null;
			}

			// Token: 0x0600873A RID: 34618 RVA: 0x003A3860 File Offset: 0x003A1A60
			public override bool Serialize(ContentXElement element = null, bool recursive = true)
			{
				base.Serialize(element, recursive);
				this.LightSource.Serialize(element ?? base.Element);
				return true;
			}

			// Token: 0x0600873B RID: 34619 RVA: 0x003A3887 File Offset: 0x003A1A87
			public override void Reset()
			{
				base.Reset();
				this.LightSource.Serialize(base.OriginalElement);
			}

			// Token: 0x0200155F RID: 5471
			public class LightTexture : RagdollParams.SubParam
			{
				// Token: 0x17001D9E RID: 7582
				// (get) Token: 0x06009DA1 RID: 40353 RVA: 0x003ED46A File Offset: 0x003EB66A
				public override string Name
				{
					get
					{
						return "Light Texture";
					}
				}

				// Token: 0x17001D9F RID: 7583
				// (get) Token: 0x06009DA2 RID: 40354 RVA: 0x003ED471 File Offset: 0x003EB671
				// (set) Token: 0x06009DA3 RID: 40355 RVA: 0x003ED479 File Offset: 0x003EB679
				[Serialize("Content/Lights/pointlight_bright.png", IsPropertySaveable.Yes, "", "", false)]
				[Editable]
				public string Texture { get; private set; }

				// Token: 0x17001DA0 RID: 7584
				// (get) Token: 0x06009DA4 RID: 40356 RVA: 0x003ED482 File Offset: 0x003EB682
				// (set) Token: 0x06009DA5 RID: 40357 RVA: 0x003ED48A File Offset: 0x003EB68A
				[Serialize("0.5, 0.5", IsPropertySaveable.Yes, "", "", false)]
				[Editable(DecimalCount = 2)]
				public Vector2 Origin { get; set; }

				// Token: 0x17001DA1 RID: 7585
				// (get) Token: 0x06009DA6 RID: 40358 RVA: 0x003ED493 File Offset: 0x003EB693
				// (set) Token: 0x06009DA7 RID: 40359 RVA: 0x003ED49B File Offset: 0x003EB69B
				[Serialize("1.0, 1.0", IsPropertySaveable.Yes, "", "", false)]
				[Editable(DecimalCount = 2)]
				public Vector2 Size { get; set; }

				// Token: 0x06009DA8 RID: 40360 RVA: 0x003ED4A4 File Offset: 0x003EB6A4
				public LightTexture(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
				{
				}
			}
		}

		// Token: 0x02000ECF RID: 3791
		public class AttackParams : RagdollParams.SubParam
		{
			// Token: 0x17001C06 RID: 7174
			// (get) Token: 0x0600873C RID: 34620 RVA: 0x003A38A5 File Offset: 0x003A1AA5
			// (set) Token: 0x0600873D RID: 34621 RVA: 0x003A38AD File Offset: 0x003A1AAD
			public Attack Attack { get; private set; }

			// Token: 0x0600873E RID: 34622 RVA: 0x003A38B8 File Offset: 0x003A1AB8
			public AttackParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				this.Attack = new Attack(element, ragdoll.SpeciesName.Value);
			}

			// Token: 0x0600873F RID: 34623 RVA: 0x003A38E8 File Offset: 0x003A1AE8
			public override bool Deserialize(ContentXElement element = null, bool recursive = true)
			{
				base.Deserialize(element, recursive);
				Attack attack = this.Attack;
				ContentXElement element2 = element ?? base.Element;
				RagdollParams ragdoll = base.Ragdoll;
				attack.Deserialize(element2, ((ragdoll != null) ? ragdoll.SpeciesName.ToString() : null) ?? "null");
				return base.SerializableProperties != null;
			}

			// Token: 0x06008740 RID: 34624 RVA: 0x003A3946 File Offset: 0x003A1B46
			public override bool Serialize(ContentXElement element = null, bool recursive = true)
			{
				base.Serialize(element, recursive);
				this.Attack.Serialize(element ?? base.Element);
				return true;
			}

			// Token: 0x06008741 RID: 34625 RVA: 0x003A3968 File Offset: 0x003A1B68
			public override void Reset()
			{
				base.Reset();
				Attack attack = this.Attack;
				ContentXElement originalElement = base.OriginalElement;
				RagdollParams ragdoll = base.Ragdoll;
				attack.Deserialize(originalElement, ((ragdoll != null) ? ragdoll.SpeciesName.ToString() : null) ?? "null");
				Attack attack2 = this.Attack;
				ContentXElement originalElement2 = base.OriginalElement;
				RagdollParams ragdoll2 = base.Ragdoll;
				attack2.ReloadAfflictions(originalElement2, ((ragdoll2 != null) ? ragdoll2.SpeciesName.ToString() : null) ?? "null");
			}

			// Token: 0x06008742 RID: 34626 RVA: 0x003A39F0 File Offset: 0x003A1BF0
			public bool AddNewAffliction()
			{
				this.Serialize(null, true);
				ContentXElement subElement = base.CreateElement("affliction", new object[]
				{
					new XAttribute("identifier", "internaldamage"),
					new XAttribute("strength", 0f),
					new XAttribute("probability", 1f)
				});
				base.Element.Add(subElement);
				Attack attack = this.Attack;
				ContentXElement element = base.Element;
				RagdollParams ragdoll = base.Ragdoll;
				attack.ReloadAfflictions(element, ((ragdoll != null) ? ragdoll.SpeciesName.ToString() : null) ?? "null");
				this.Serialize(null, true);
				return true;
			}

			// Token: 0x06008743 RID: 34627 RVA: 0x003A3AB8 File Offset: 0x003A1CB8
			public bool RemoveAffliction(XElement affliction)
			{
				this.Serialize(null, true);
				affliction.Remove();
				Attack attack = this.Attack;
				ContentXElement element = base.Element;
				RagdollParams ragdoll = base.Ragdoll;
				attack.ReloadAfflictions(element, ((ragdoll != null) ? ragdoll.SpeciesName.ToString() : null) ?? "null");
				return this.Serialize(null, true);
			}
		}

		// Token: 0x02000ED0 RID: 3792
		public class DamageModifierParams : RagdollParams.SubParam
		{
			// Token: 0x17001C07 RID: 7175
			// (get) Token: 0x06008744 RID: 34628 RVA: 0x003A3B16 File Offset: 0x003A1D16
			// (set) Token: 0x06008745 RID: 34629 RVA: 0x003A3B1E File Offset: 0x003A1D1E
			public DamageModifier DamageModifier { get; private set; }

			// Token: 0x06008746 RID: 34630 RVA: 0x003A3B28 File Offset: 0x003A1D28
			public DamageModifierParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				this.DamageModifier = new DamageModifier(element, ragdoll.SpeciesName.Value, true);
			}

			// Token: 0x06008747 RID: 34631 RVA: 0x003A3B58 File Offset: 0x003A1D58
			public override bool Deserialize(ContentXElement element = null, bool recursive = true)
			{
				base.Deserialize(element, recursive);
				this.DamageModifier.Deserialize(element ?? base.Element);
				return base.SerializableProperties != null;
			}

			// Token: 0x06008748 RID: 34632 RVA: 0x003A3B87 File Offset: 0x003A1D87
			public override bool Serialize(ContentXElement element = null, bool recursive = true)
			{
				base.Serialize(element, recursive);
				this.DamageModifier.Serialize(element ?? base.Element);
				return true;
			}

			// Token: 0x06008749 RID: 34633 RVA: 0x003A3BAE File Offset: 0x003A1DAE
			public override void Reset()
			{
				base.Reset();
				this.DamageModifier.Deserialize(base.OriginalElement);
			}
		}

		// Token: 0x02000ED1 RID: 3793
		public class SoundParams : RagdollParams.SubParam
		{
			// Token: 0x17001C08 RID: 7176
			// (get) Token: 0x0600874A RID: 34634 RVA: 0x003A3BCC File Offset: 0x003A1DCC
			public override string Name
			{
				get
				{
					return "Sound";
				}
			}

			// Token: 0x17001C09 RID: 7177
			// (get) Token: 0x0600874B RID: 34635 RVA: 0x003A3BD3 File Offset: 0x003A1DD3
			// (set) Token: 0x0600874C RID: 34636 RVA: 0x003A3BDB File Offset: 0x003A1DDB
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string Tag { get; private set; }

			// Token: 0x0600874D RID: 34637 RVA: 0x003A3BE4 File Offset: 0x003A1DE4
			public SoundParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
			}
		}

		// Token: 0x02000ED2 RID: 3794
		public abstract class SubParam : ISerializableEntity
		{
			// Token: 0x17001C0A RID: 7178
			// (get) Token: 0x0600874E RID: 34638 RVA: 0x003A3BEE File Offset: 0x003A1DEE
			// (set) Token: 0x0600874F RID: 34639 RVA: 0x003A3BF6 File Offset: 0x003A1DF6
			public virtual string Name { get; set; }

			// Token: 0x17001C0B RID: 7179
			// (get) Token: 0x06008750 RID: 34640 RVA: 0x003A3BFF File Offset: 0x003A1DFF
			// (set) Token: 0x06008751 RID: 34641 RVA: 0x003A3C07 File Offset: 0x003A1E07
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

			// Token: 0x17001C0C RID: 7180
			// (get) Token: 0x06008752 RID: 34642 RVA: 0x003A3C10 File Offset: 0x003A1E10
			// (set) Token: 0x06008753 RID: 34643 RVA: 0x003A3C18 File Offset: 0x003A1E18
			public ContentXElement Element { get; set; }

			// Token: 0x17001C0D RID: 7181
			// (get) Token: 0x06008754 RID: 34644 RVA: 0x003A3C21 File Offset: 0x003A1E21
			// (set) Token: 0x06008755 RID: 34645 RVA: 0x003A3C29 File Offset: 0x003A1E29
			public ContentXElement OriginalElement { get; protected set; }

			// Token: 0x17001C0E RID: 7182
			// (get) Token: 0x06008756 RID: 34646 RVA: 0x003A3C32 File Offset: 0x003A1E32
			// (set) Token: 0x06008757 RID: 34647 RVA: 0x003A3C3A File Offset: 0x003A1E3A
			public List<RagdollParams.SubParam> SubParams { get; set; } = new List<RagdollParams.SubParam>();

			// Token: 0x17001C0F RID: 7183
			// (get) Token: 0x06008758 RID: 34648 RVA: 0x003A3C43 File Offset: 0x003A1E43
			// (set) Token: 0x06008759 RID: 34649 RVA: 0x003A3C4B File Offset: 0x003A1E4B
			public RagdollParams Ragdoll { get; private set; }

			// Token: 0x0600875A RID: 34650 RVA: 0x003A3C54 File Offset: 0x003A1E54
			public virtual string GenerateName()
			{
				return this.Element.Name.ToString();
			}

			// Token: 0x0600875B RID: 34651 RVA: 0x003A3C66 File Offset: 0x003A1E66
			protected ContentXElement CreateElement(string name, params object[] attrs)
			{
				return new XElement(name, attrs).FromPackage(this.Element.ContentPackage);
			}

			// Token: 0x0600875C RID: 34652 RVA: 0x003A3C84 File Offset: 0x003A1E84
			public SubParam(ContentXElement element, RagdollParams ragdoll)
			{
				this.Element = element;
				this.OriginalElement = new ContentXElement(element.ContentPackage, element);
				this.Ragdoll = ragdoll;
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			}

			// Token: 0x0600875D RID: 34653 RVA: 0x003A3CDC File Offset: 0x003A1EDC
			public virtual bool Deserialize(ContentXElement element = null, bool recursive = true)
			{
				if (element == null)
				{
					element = this.Element;
				}
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
				if (recursive)
				{
					this.SubParams.ForEach(delegate(RagdollParams.SubParam sp)
					{
						sp.Deserialize(null, true);
					});
				}
				return this.SerializableProperties != null;
			}

			// Token: 0x0600875E RID: 34654 RVA: 0x003A3D3C File Offset: 0x003A1F3C
			public virtual bool Serialize(ContentXElement element = null, bool recursive = true)
			{
				if (element == null)
				{
					element = this.Element;
				}
				SerializableProperty.SerializeProperties(this, element, true, false);
				if (recursive)
				{
					this.SubParams.ForEach(delegate(RagdollParams.SubParam sp)
					{
						sp.Serialize(null, true);
					});
				}
				return true;
			}

			// Token: 0x0600875F RID: 34655 RVA: 0x003A3D90 File Offset: 0x003A1F90
			public virtual void SetCurrentElementAsOriginalElement()
			{
				this.OriginalElement = this.Element;
				this.SubParams.ForEach(delegate(RagdollParams.SubParam sp)
				{
					sp.SetCurrentElementAsOriginalElement();
				});
			}

			// Token: 0x06008760 RID: 34656 RVA: 0x003A3DC8 File Offset: 0x003A1FC8
			public virtual void Reset()
			{
				this.Deserialize(this.OriginalElement, false);
				this.SubParams.ForEach(delegate(RagdollParams.SubParam sp)
				{
					sp.Reset();
				});
			}

			// Token: 0x17001C10 RID: 7184
			// (get) Token: 0x06008761 RID: 34657 RVA: 0x003A3E02 File Offset: 0x003A2002
			// (set) Token: 0x06008762 RID: 34658 RVA: 0x003A3E0A File Offset: 0x003A200A
			public SerializableEntityEditor SerializableEntityEditor { get; protected set; }

			// Token: 0x17001C11 RID: 7185
			// (get) Token: 0x06008763 RID: 34659 RVA: 0x003A3E13 File Offset: 0x003A2013
			// (set) Token: 0x06008764 RID: 34660 RVA: 0x003A3E1B File Offset: 0x003A201B
			public Dictionary<Affliction, SerializableEntityEditor> AfflictionEditors { get; private set; }

			// Token: 0x06008765 RID: 34661 RVA: 0x003A3E24 File Offset: 0x003A2024
			public virtual void AddToEditor(ParamsEditor editor, bool recursive = true, int space = 0)
			{
				this.SerializableEntityEditor = new SerializableEntityEditor(editor.EditorBox.Content.RectTransform, this, false, true, "", 24, GUIStyle.LargeFont, true);
				RagdollParams.DecorativeSpriteParams decSpriteParams = this as RagdollParams.DecorativeSpriteParams;
				if (decSpriteParams != null)
				{
					new SerializableEntityEditor(editor.EditorBox.Content.RectTransform, decSpriteParams.DecorativeSprite, false, true, "", 24, GUIStyle.LargeFont, true);
				}
				else
				{
					RagdollParams.DeformSpriteParams deformSpriteParams = this as RagdollParams.DeformSpriteParams;
					if (deformSpriteParams != null)
					{
						using (Dictionary<SpriteDeformationParams, XElement>.KeyCollection.Enumerator enumerator = deformSpriteParams.Deformation.Deformations.Keys.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								SpriteDeformationParams deformation = enumerator.Current;
								new SerializableEntityEditor(editor.EditorBox.Content.RectTransform, deformation, false, true, "", 24, GUIStyle.LargeFont, true);
							}
							goto IL_25F;
						}
					}
					RagdollParams.AttackParams attackParams = this as RagdollParams.AttackParams;
					if (attackParams != null)
					{
						this.SerializableEntityEditor = new SerializableEntityEditor(editor.EditorBox.Content.RectTransform, attackParams.Attack, false, true, "", 24, GUIStyle.LargeFont, true);
						if (this.AfflictionEditors == null)
						{
							this.AfflictionEditors = new Dictionary<Affliction, SerializableEntityEditor>();
						}
						else
						{
							this.AfflictionEditors.Clear();
						}
						using (Dictionary<Affliction, XElement>.KeyCollection.Enumerator enumerator2 = attackParams.Attack.Afflictions.Keys.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								Affliction affliction = enumerator2.Current;
								SerializableEntityEditor afflictionEditor = new SerializableEntityEditor(this.SerializableEntityEditor.RectTransform, affliction, false, true, "", 24, null, true);
								this.AfflictionEditors.Add(affliction, afflictionEditor);
								this.SerializableEntityEditor.AddCustomContent(afflictionEditor, this.SerializableEntityEditor.ContentCount);
							}
							goto IL_25F;
						}
					}
					RagdollParams.LightSourceParams lightParams = this as RagdollParams.LightSourceParams;
					if (lightParams != null)
					{
						this.SerializableEntityEditor = new SerializableEntityEditor(editor.EditorBox.Content.RectTransform, lightParams.LightSource, false, true, "", 24, GUIStyle.LargeFont, true);
					}
					else
					{
						RagdollParams.DamageModifierParams damageModifierParams = this as RagdollParams.DamageModifierParams;
						if (damageModifierParams != null)
						{
							this.SerializableEntityEditor = new SerializableEntityEditor(editor.EditorBox.Content.RectTransform, damageModifierParams.DamageModifier, false, true, "", 24, GUIStyle.LargeFont, true);
						}
					}
				}
				IL_25F:
				if (recursive)
				{
					this.SubParams.ForEach(delegate(RagdollParams.SubParam sp)
					{
						sp.AddToEditor(editor, true, 0);
					});
				}
				if (space > 0)
				{
					new GUIFrame(new RectTransform(new Point(editor.EditorBox.Rect.Width, space), editor.EditorBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, new Color?(new Color(20, 20, 20, 255))).CanBeFocused = false;
				}
			}
		}
	}
}
