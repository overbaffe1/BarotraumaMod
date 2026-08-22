using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000F3 RID: 243
	internal class RagdollParams : EditableParams, IMemorizable<RagdollParams>
	{
		// Token: 0x1700078F RID: 1935
		// (get) Token: 0x06001942 RID: 6466 RVA: 0x000C5FF2 File Offset: 0x000C41F2
		// (set) Token: 0x06001943 RID: 6467 RVA: 0x000C5FFA File Offset: 0x000C41FA
		public Identifier SpeciesName { get; private set; }

		// Token: 0x17000790 RID: 1936
		// (get) Token: 0x06001944 RID: 6468 RVA: 0x000C6003 File Offset: 0x000C4203
		// (set) Token: 0x06001945 RID: 6469 RVA: 0x000C600B File Offset: 0x000C420B
		[Serialize("", IsPropertySaveable.Yes, "Default path for the limb sprite textures. Used only if the limb specific path for the limb is not defined", "", false)]
		[Editable]
		public string Texture { get; set; }

		// Token: 0x17000791 RID: 1937
		// (get) Token: 0x06001946 RID: 6470 RVA: 0x000C6014 File Offset: 0x000C4214
		// (set) Token: 0x06001947 RID: 6471 RVA: 0x000C601C File Offset: 0x000C421C
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color Color { get; set; }

		// Token: 0x17000792 RID: 1938
		// (get) Token: 0x06001948 RID: 6472 RVA: 0x000C6025 File Offset: 0x000C4225
		// (set) Token: 0x06001949 RID: 6473 RVA: 0x000C602D File Offset: 0x000C422D
		[Serialize(0f, IsPropertySaveable.Yes, "General orientation of the sprites as drawn on the spritesheet. Defines the \"forward direction\" of the sprites. Should be configured as the direction pointing outwards from the main limb. Incorrectly defined orientations may lead to limbs being rotated incorrectly when e.g. when the character aims or flips to face a different direction. Can be overridden per sprite by setting a value for Limb's 'Sprite Orientation'.", "", false)]
		[Editable(-360, 360)]
		public float SpritesheetOrientation { get; set; }

		// Token: 0x17000793 RID: 1939
		// (get) Token: 0x0600194A RID: 6474 RVA: 0x000C6036 File Offset: 0x000C4236
		public bool IsSpritesheetOrientationHorizontal
		{
			get
			{
				return (this.SpritesheetOrientation > 45f && this.SpritesheetOrientation < 135f) || (this.SpritesheetOrientation > 255f && this.SpritesheetOrientation < 315f);
			}
		}

		// Token: 0x17000794 RID: 1940
		// (get) Token: 0x0600194B RID: 6475 RVA: 0x000C6070 File Offset: 0x000C4270
		// (set) Token: 0x0600194C RID: 6476 RVA: 0x000C6078 File Offset: 0x000C4278
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

		// Token: 0x17000795 RID: 1941
		// (get) Token: 0x0600194D RID: 6477 RVA: 0x000C6090 File Offset: 0x000C4290
		// (set) Token: 0x0600194E RID: 6478 RVA: 0x000C6098 File Offset: 0x000C4298
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

		// Token: 0x17000796 RID: 1942
		// (get) Token: 0x0600194F RID: 6479 RVA: 0x000C60B0 File Offset: 0x000C42B0
		// (set) Token: 0x06001950 RID: 6480 RVA: 0x000C60B8 File Offset: 0x000C42B8
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float TextureScale { get; set; }

		// Token: 0x17000797 RID: 1943
		// (get) Token: 0x06001951 RID: 6481 RVA: 0x000C60C1 File Offset: 0x000C42C1
		// (set) Token: 0x06001952 RID: 6482 RVA: 0x000C60C9 File Offset: 0x000C42C9
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float SourceRectScale { get; set; }

		// Token: 0x17000798 RID: 1944
		// (get) Token: 0x06001953 RID: 6483 RVA: 0x000C60D2 File Offset: 0x000C42D2
		// (set) Token: 0x06001954 RID: 6484 RVA: 0x000C60DA File Offset: 0x000C42DA
		[Serialize(45f, IsPropertySaveable.Yes, "How high from the ground the main collider levitates when the character is standing? Doesn't affect swimming.", "", false)]
		[Editable(0f, 1000f, 1)]
		public float ColliderHeightFromFloor { get; set; }

		// Token: 0x17000799 RID: 1945
		// (get) Token: 0x06001955 RID: 6485 RVA: 0x000C60E3 File Offset: 0x000C42E3
		// (set) Token: 0x06001956 RID: 6486 RVA: 0x000C60EB File Offset: 0x000C42EB
		[Serialize(50f, IsPropertySaveable.Yes, "How much impact is required before the character takes impact damage?", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float ImpactTolerance { get; set; }

		// Token: 0x1700079A RID: 1946
		// (get) Token: 0x06001957 RID: 6487 RVA: 0x000C60F4 File Offset: 0x000C42F4
		// (set) Token: 0x06001958 RID: 6488 RVA: 0x000C60FC File Offset: 0x000C42FC
		[Serialize(CanEnterSubmarine.True, IsPropertySaveable.Yes, "Can the creature enter submarine. Creatures that cannot enter submarines, always collide with it, even when there is a gap.", "", false)]
		[Editable]
		public CanEnterSubmarine CanEnterSubmarine { get; set; }

		// Token: 0x1700079B RID: 1947
		// (get) Token: 0x06001959 RID: 6489 RVA: 0x000C6105 File Offset: 0x000C4305
		// (set) Token: 0x0600195A RID: 6490 RVA: 0x000C610D File Offset: 0x000C430D
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool CanWalk { get; set; }

		// Token: 0x1700079C RID: 1948
		// (get) Token: 0x0600195B RID: 6491 RVA: 0x000C6116 File Offset: 0x000C4316
		// (set) Token: 0x0600195C RID: 6492 RVA: 0x000C611E File Offset: 0x000C431E
		[Serialize(true, IsPropertySaveable.Yes, "Can the character be dragged around by other creatures?", "", false)]
		[Editable]
		public bool Draggable { get; set; }

		// Token: 0x1700079D RID: 1949
		// (get) Token: 0x0600195D RID: 6493 RVA: 0x000C6127 File Offset: 0x000C4327
		// (set) Token: 0x0600195E RID: 6494 RVA: 0x000C612F File Offset: 0x000C432F
		[Serialize(LimbType.Torso, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public LimbType MainLimb { get; set; }

		// Token: 0x1700079E RID: 1950
		// (get) Token: 0x0600195F RID: 6495 RVA: 0x000C6138 File Offset: 0x000C4338
		// (set) Token: 0x06001960 RID: 6496 RVA: 0x000C6140 File Offset: 0x000C4340
		public List<RagdollParams.ColliderParams> Colliders { get; private set; } = new List<RagdollParams.ColliderParams>();

		// Token: 0x1700079F RID: 1951
		// (get) Token: 0x06001961 RID: 6497 RVA: 0x000C6149 File Offset: 0x000C4349
		// (set) Token: 0x06001962 RID: 6498 RVA: 0x000C6151 File Offset: 0x000C4351
		public List<RagdollParams.LimbParams> Limbs { get; private set; } = new List<RagdollParams.LimbParams>();

		// Token: 0x170007A0 RID: 1952
		// (get) Token: 0x06001963 RID: 6499 RVA: 0x000C615A File Offset: 0x000C435A
		// (set) Token: 0x06001964 RID: 6500 RVA: 0x000C6162 File Offset: 0x000C4362
		public List<RagdollParams.JointParams> Joints { get; private set; } = new List<RagdollParams.JointParams>();

		// Token: 0x06001965 RID: 6501 RVA: 0x000C616B File Offset: 0x000C436B
		protected IEnumerable<RagdollParams.SubParam> GetAllSubParams()
		{
			return this.Colliders.Concat(this.Limbs).Concat(this.Joints);
		}

		// Token: 0x06001966 RID: 6502 RVA: 0x000C6189 File Offset: 0x000C4389
		public static string GetDefaultFileName(Identifier speciesName)
		{
			return speciesName.Value.CapitaliseFirstInvariant() + "DefaultRagdoll";
		}

		// Token: 0x06001967 RID: 6503 RVA: 0x000C61A1 File Offset: 0x000C43A1
		public static string GetDefaultFile(Identifier speciesName)
		{
			return Barotrauma.IO.Path.Combine(new string[]
			{
				RagdollParams.GetFolder(speciesName),
				RagdollParams.GetDefaultFileName(speciesName) + ".xml"
			});
		}

		// Token: 0x06001968 RID: 6504 RVA: 0x000C61CC File Offset: 0x000C43CC
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

		// Token: 0x06001969 RID: 6505 RVA: 0x000C6258 File Offset: 0x000C4458
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

		// Token: 0x0600196A RID: 6506 RVA: 0x000C62F3 File Offset: 0x000C44F3
		public static T GetDefaultRagdollParams<T>(Character character) where T : RagdollParams, new()
		{
			return RagdollParams.GetDefaultRagdollParams<T>(character.SpeciesName, character.Params, character.Prefab.ContentPackage);
		}

		// Token: 0x0600196B RID: 6507 RVA: 0x000C6314 File Offset: 0x000C4514
		public static T GetDefaultRagdollParams<T>(Identifier speciesName, CharacterParams characterParams, ContentPackage contentPackage) where T : RagdollParams, new()
		{
			XDocument variantFile = characterParams.VariantFile;
			XElement mainElement = ((variantFile != null) ? variantFile.Root : null) ?? characterParams.MainElement;
			return RagdollParams.GetDefaultRagdollParams<T>(speciesName, mainElement, contentPackage);
		}

		// Token: 0x0600196C RID: 6508 RVA: 0x000C634C File Offset: 0x000C454C
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

		// Token: 0x0600196D RID: 6509 RVA: 0x000C6454 File Offset: 0x000C4654
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

		// Token: 0x0600196E RID: 6510 RVA: 0x000C680C File Offset: 0x000C4A0C
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

		// Token: 0x0600196F RID: 6511 RVA: 0x000C6936 File Offset: 0x000C4B36
		public static void ClearCache()
		{
			RagdollParams.allRagdolls.Clear();
		}

		// Token: 0x06001970 RID: 6512 RVA: 0x000C6944 File Offset: 0x000C4B44
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

		// Token: 0x06001971 RID: 6513 RVA: 0x000C69A8 File Offset: 0x000C4BA8
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

		// Token: 0x06001972 RID: 6514 RVA: 0x000C6A16 File Offset: 0x000C4C16
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

		// Token: 0x06001973 RID: 6515 RVA: 0x000C6A44 File Offset: 0x000C4C44
		public void Apply()
		{
			this.Serialize(null, true, true);
		}

		// Token: 0x06001974 RID: 6516 RVA: 0x000C6A50 File Offset: 0x000C4C50
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

		// Token: 0x06001975 RID: 6517 RVA: 0x000C6AB4 File Offset: 0x000C4CB4
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

		// Token: 0x06001976 RID: 6518 RVA: 0x000C6B24 File Offset: 0x000C4D24
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

		// Token: 0x06001977 RID: 6519 RVA: 0x000C6BD4 File Offset: 0x000C4DD4
		protected void CreateJoints()
		{
			this.Joints.Clear();
			foreach (ContentXElement element in this.MainElement.GetChildElements("joint"))
			{
				this.Joints.Add(new RagdollParams.JointParams(element, this));
			}
		}

		// Token: 0x06001978 RID: 6520 RVA: 0x000C6C44 File Offset: 0x000C4E44
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

		// Token: 0x06001979 RID: 6521 RVA: 0x000C6C84 File Offset: 0x000C4E84
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

		// Token: 0x0600197A RID: 6522 RVA: 0x000C6CC4 File Offset: 0x000C4EC4
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

		// Token: 0x170007A1 RID: 1953
		// (get) Token: 0x0600197B RID: 6523 RVA: 0x000C6D82 File Offset: 0x000C4F82
		// (set) Token: 0x0600197C RID: 6524 RVA: 0x000C6D8A File Offset: 0x000C4F8A
		public Memento<RagdollParams> Memento { get; protected set; } = new Memento<RagdollParams>();

		// Token: 0x0600197D RID: 6525 RVA: 0x000C6D94 File Offset: 0x000C4F94
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

		// Token: 0x0600197E RID: 6526 RVA: 0x000C6E2A File Offset: 0x000C502A
		public void Undo()
		{
			this.RevertTo(this.Memento.Undo());
		}

		// Token: 0x0600197F RID: 6527 RVA: 0x000C6E3D File Offset: 0x000C503D
		public void Redo()
		{
			this.RevertTo(this.Memento.Redo());
		}

		// Token: 0x06001980 RID: 6528 RVA: 0x000C6E50 File Offset: 0x000C5050
		public void ClearHistory()
		{
			this.Memento.Clear();
		}

		// Token: 0x06001981 RID: 6529 RVA: 0x000C6E60 File Offset: 0x000C5060
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

		// Token: 0x04000C1B RID: 3099
		public const float MIN_SCALE = 0.1f;

		// Token: 0x04000C1C RID: 3100
		public const float MAX_SCALE = 2f;

		// Token: 0x04000C21 RID: 3105
		private float limbScale;

		// Token: 0x04000C22 RID: 3106
		private float jointScale;

		// Token: 0x04000C2B RID: 3115
		private static readonly Dictionary<Identifier, Dictionary<string, RagdollParams>> allRagdolls = new Dictionary<Identifier, Dictionary<string, RagdollParams>>();

		// Token: 0x04000C2F RID: 3119
		private bool isVariantScaleApplied;

		// Token: 0x020008AE RID: 2222
		public class JointParams : RagdollParams.SubParam
		{
			// Token: 0x170014D5 RID: 5333
			// (get) Token: 0x06005699 RID: 22169 RVA: 0x001F3E04 File Offset: 0x001F2004
			// (set) Token: 0x0600569A RID: 22170 RVA: 0x001F3E25 File Offset: 0x001F2025
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

			// Token: 0x0600569B RID: 22171 RVA: 0x001F3E30 File Offset: 0x001F2030
			public override string GenerateName()
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Joint ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Limb1);
				defaultInterpolatedStringHandler.AppendLiteral(" - ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Limb2);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}

			// Token: 0x170014D6 RID: 5334
			// (get) Token: 0x0600569C RID: 22172 RVA: 0x001F3E80 File Offset: 0x001F2080
			// (set) Token: 0x0600569D RID: 22173 RVA: 0x001F3E88 File Offset: 0x001F2088
			[Serialize(-1, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public int Limb1 { get; set; }

			// Token: 0x170014D7 RID: 5335
			// (get) Token: 0x0600569E RID: 22174 RVA: 0x001F3E91 File Offset: 0x001F2091
			// (set) Token: 0x0600569F RID: 22175 RVA: 0x001F3E99 File Offset: 0x001F2099
			[Serialize(-1, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public int Limb2 { get; set; }

			// Token: 0x170014D8 RID: 5336
			// (get) Token: 0x060056A0 RID: 22176 RVA: 0x001F3EA2 File Offset: 0x001F20A2
			// (set) Token: 0x060056A1 RID: 22177 RVA: 0x001F3EAA File Offset: 0x001F20AA
			[Serialize("1.0, 1.0", IsPropertySaveable.Yes, "Local position of the joint in the Limb1.", "", false)]
			[Editable]
			public Vector2 Limb1Anchor { get; set; }

			// Token: 0x170014D9 RID: 5337
			// (get) Token: 0x060056A2 RID: 22178 RVA: 0x001F3EB3 File Offset: 0x001F20B3
			// (set) Token: 0x060056A3 RID: 22179 RVA: 0x001F3EBB File Offset: 0x001F20BB
			[Serialize("1.0, 1.0", IsPropertySaveable.Yes, "Local position of the joint in the Limb2.", "", false)]
			[Editable]
			public Vector2 Limb2Anchor { get; set; }

			// Token: 0x170014DA RID: 5338
			// (get) Token: 0x060056A4 RID: 22180 RVA: 0x001F3EC4 File Offset: 0x001F20C4
			// (set) Token: 0x060056A5 RID: 22181 RVA: 0x001F3ECC File Offset: 0x001F20CC
			[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool CanBeSevered { get; set; }

			// Token: 0x170014DB RID: 5339
			// (get) Token: 0x060056A6 RID: 22182 RVA: 0x001F3ED5 File Offset: 0x001F20D5
			// (set) Token: 0x060056A7 RID: 22183 RVA: 0x001F3EDD File Offset: 0x001F20DD
			[Serialize(0f, IsPropertySaveable.Yes, "Default 0 (Can't be severed when the creature is alive). Modifies the severance probability (defined per item/attack) when the character is alive. Currently only affects non-humanoid ragdolls. Also note that if CanBeSevered is false, this property doesn't have any effect.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, ValueStep = 0.1f, DecimalCount = 2)]
			public float SeveranceProbabilityModifier { get; set; }

			// Token: 0x170014DC RID: 5340
			// (get) Token: 0x060056A8 RID: 22184 RVA: 0x001F3EE6 File Offset: 0x001F20E6
			// (set) Token: 0x060056A9 RID: 22185 RVA: 0x001F3EEE File Offset: 0x001F20EE
			[Serialize("gore", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string BreakSound { get; set; }

			// Token: 0x170014DD RID: 5341
			// (get) Token: 0x060056AA RID: 22186 RVA: 0x001F3EF7 File Offset: 0x001F20F7
			// (set) Token: 0x060056AB RID: 22187 RVA: 0x001F3EFF File Offset: 0x001F20FF
			[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool LimitEnabled { get; set; }

			// Token: 0x170014DE RID: 5342
			// (get) Token: 0x060056AC RID: 22188 RVA: 0x001F3F08 File Offset: 0x001F2108
			// (set) Token: 0x060056AD RID: 22189 RVA: 0x001F3F10 File Offset: 0x001F2110
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float UpperLimit { get; set; }

			// Token: 0x170014DF RID: 5343
			// (get) Token: 0x060056AE RID: 22190 RVA: 0x001F3F19 File Offset: 0x001F2119
			// (set) Token: 0x060056AF RID: 22191 RVA: 0x001F3F21 File Offset: 0x001F2121
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float LowerLimit { get; set; }

			// Token: 0x170014E0 RID: 5344
			// (get) Token: 0x060056B0 RID: 22192 RVA: 0x001F3F2A File Offset: 0x001F212A
			// (set) Token: 0x060056B1 RID: 22193 RVA: 0x001F3F32 File Offset: 0x001F2132
			[Serialize(0.25f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float Stiffness { get; set; }

			// Token: 0x170014E1 RID: 5345
			// (get) Token: 0x060056B2 RID: 22194 RVA: 0x001F3F3B File Offset: 0x001F213B
			// (set) Token: 0x060056B3 RID: 22195 RVA: 0x001F3F43 File Offset: 0x001F2143
			[Serialize(1f, IsPropertySaveable.Yes, "CAUTION: Not fully implemented. Only use for limb joints that connect non-animated limbs!", "", false)]
			[Editable(DecimalCount = 2)]
			public float Scale { get; set; }

			// Token: 0x170014E2 RID: 5346
			// (get) Token: 0x060056B4 RID: 22196 RVA: 0x001F3F4C File Offset: 0x001F214C
			// (set) Token: 0x060056B5 RID: 22197 RVA: 0x001F3F54 File Offset: 0x001F2154
			[Serialize(false, IsPropertySaveable.No, "", "", false)]
			[Editable(ReadOnly = true)]
			public bool WeldJoint { get; set; }

			// Token: 0x170014E3 RID: 5347
			// (get) Token: 0x060056B6 RID: 22198 RVA: 0x001F3F5D File Offset: 0x001F215D
			// (set) Token: 0x060056B7 RID: 22199 RVA: 0x001F3F65 File Offset: 0x001F2165
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool ClockWiseRotation { get; set; }

			// Token: 0x060056B8 RID: 22200 RVA: 0x001F3F6E File Offset: 0x001F216E
			public JointParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
			}

			// Token: 0x040030C6 RID: 12486
			private string name;
		}

		// Token: 0x020008AF RID: 2223
		public class LimbParams : RagdollParams.SubParam
		{
			// Token: 0x170014E4 RID: 5348
			// (get) Token: 0x060056B9 RID: 22201 RVA: 0x001F3F78 File Offset: 0x001F2178
			// (set) Token: 0x060056BA RID: 22202 RVA: 0x001F3F80 File Offset: 0x001F2180
			public RagdollParams.AttackParams Attack { get; private set; }

			// Token: 0x170014E5 RID: 5349
			// (get) Token: 0x060056BB RID: 22203 RVA: 0x001F3F89 File Offset: 0x001F2189
			// (set) Token: 0x060056BC RID: 22204 RVA: 0x001F3F91 File Offset: 0x001F2191
			public RagdollParams.SoundParams Sound { get; private set; }

			// Token: 0x170014E6 RID: 5350
			// (get) Token: 0x060056BD RID: 22205 RVA: 0x001F3F9A File Offset: 0x001F219A
			// (set) Token: 0x060056BE RID: 22206 RVA: 0x001F3FA2 File Offset: 0x001F21A2
			public RagdollParams.LightSourceParams LightSource { get; private set; }

			// Token: 0x170014E7 RID: 5351
			// (get) Token: 0x060056BF RID: 22207 RVA: 0x001F3FAB File Offset: 0x001F21AB
			// (set) Token: 0x060056C0 RID: 22208 RVA: 0x001F3FB3 File Offset: 0x001F21B3
			public List<RagdollParams.DamageModifierParams> DamageModifiers { get; private set; } = new List<RagdollParams.DamageModifierParams>();

			// Token: 0x170014E8 RID: 5352
			// (get) Token: 0x060056C1 RID: 22209 RVA: 0x001F3FBC File Offset: 0x001F21BC
			// (set) Token: 0x060056C2 RID: 22210 RVA: 0x001F3FDD File Offset: 0x001F21DD
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

			// Token: 0x060056C3 RID: 22211 RVA: 0x001F3FE8 File Offset: 0x001F21E8
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

			// Token: 0x060056C4 RID: 22212 RVA: 0x001F4069 File Offset: 0x001F2269
			public RagdollParams.SpriteParams GetSprite()
			{
				return this.deformSpriteParams ?? this.normalSpriteParams;
			}

			// Token: 0x170014E9 RID: 5353
			// (get) Token: 0x060056C5 RID: 22213 RVA: 0x001F407B File Offset: 0x001F227B
			// (set) Token: 0x060056C6 RID: 22214 RVA: 0x001F4083 File Offset: 0x001F2283
			[Serialize(-1, IsPropertySaveable.Yes, "", "", false)]
			[Editable(ReadOnly = true)]
			public int ID { get; set; }

			// Token: 0x170014EA RID: 5354
			// (get) Token: 0x060056C7 RID: 22215 RVA: 0x001F408C File Offset: 0x001F228C
			// (set) Token: 0x060056C8 RID: 22216 RVA: 0x001F4094 File Offset: 0x001F2294
			[Serialize(LimbType.None, IsPropertySaveable.Yes, "The limb type affects many things, like the animations. Torso or Head are considered as the main limbs. Every character should have at least one Torso or Head.", "", false)]
			[Editable]
			public LimbType Type { get; set; }

			// Token: 0x170014EB RID: 5355
			// (get) Token: 0x060056C9 RID: 22217 RVA: 0x001F409D File Offset: 0x001F229D
			// (set) Token: 0x060056CA RID: 22218 RVA: 0x001F40A5 File Offset: 0x001F22A5
			[Serialize(LimbType.None, IsPropertySaveable.Yes, "Secondary limb type to be used for generic purposes. Currently only used in climbing animations.", "", false)]
			[Editable]
			public LimbType SecondaryType { get; set; }

			// Token: 0x060056CB RID: 22219 RVA: 0x001F40AE File Offset: 0x001F22AE
			public float GetSpriteOrientation()
			{
				return MathHelper.ToRadians(this.GetSpriteOrientationInDegrees());
			}

			// Token: 0x060056CC RID: 22220 RVA: 0x001F40BB File Offset: 0x001F22BB
			public float GetSpriteOrientationInDegrees()
			{
				if (!float.IsNaN(this.SpriteOrientation))
				{
					return this.SpriteOrientation;
				}
				return base.Ragdoll.SpritesheetOrientation;
			}

			// Token: 0x170014EC RID: 5356
			// (get) Token: 0x060056CD RID: 22221 RVA: 0x001F40DC File Offset: 0x001F22DC
			// (set) Token: 0x060056CE RID: 22222 RVA: 0x001F40E4 File Offset: 0x001F22E4
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string Notes { get; set; }

			// Token: 0x170014ED RID: 5357
			// (get) Token: 0x060056CF RID: 22223 RVA: 0x001F40ED File Offset: 0x001F22ED
			// (set) Token: 0x060056D0 RID: 22224 RVA: 0x001F40F5 File Offset: 0x001F22F5
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(DecimalCount = 2)]
			public float Scale { get; set; }

			// Token: 0x170014EE RID: 5358
			// (get) Token: 0x060056D1 RID: 22225 RVA: 0x001F40FE File Offset: 0x001F22FE
			// (set) Token: 0x060056D2 RID: 22226 RVA: 0x001F4106 File Offset: 0x001F2306
			[Serialize(true, IsPropertySaveable.Yes, "Does the limb flip when the character flips?", "", false)]
			[Editable]
			public bool Flip { get; set; }

			// Token: 0x170014EF RID: 5359
			// (get) Token: 0x060056D3 RID: 22227 RVA: 0x001F410F File Offset: 0x001F230F
			// (set) Token: 0x060056D4 RID: 22228 RVA: 0x001F4117 File Offset: 0x001F2317
			[Serialize(false, IsPropertySaveable.Yes, "Currently only works with non-deformable (normal) sprites.", "", false)]
			[Editable]
			public bool MirrorVertically { get; set; }

			// Token: 0x170014F0 RID: 5360
			// (get) Token: 0x060056D5 RID: 22229 RVA: 0x001F4120 File Offset: 0x001F2320
			// (set) Token: 0x060056D6 RID: 22230 RVA: 0x001F4128 File Offset: 0x001F2328
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool MirrorHorizontally { get; set; }

			// Token: 0x170014F1 RID: 5361
			// (get) Token: 0x060056D7 RID: 22231 RVA: 0x001F4131 File Offset: 0x001F2331
			// (set) Token: 0x060056D8 RID: 22232 RVA: 0x001F4139 File Offset: 0x001F2339
			[Serialize(false, IsPropertySaveable.Yes, "Disable drawing for this limb.", "", false)]
			[Editable]
			public bool Hide { get; set; }

			// Token: 0x170014F2 RID: 5362
			// (get) Token: 0x060056D9 RID: 22233 RVA: 0x001F4142 File Offset: 0x001F2342
			// (set) Token: 0x060056DA RID: 22234 RVA: 0x001F414A File Offset: 0x001F234A
			[Serialize(float.NaN, IsPropertySaveable.Yes, "Orientation of the sprite as drawn on the spritesheet. Defines the \"forward direction\" of the sprite. Should be configured as the direction pointing outwards from the main limb.Incorrectly defined orientations may lead to limbs being rotated incorrectly when e.g. when the character aims or flips to face a different direction. Overrides the value of 'Spritesheet Orientation' for this limb.", "", false)]
			[Editable(-360, 360, ValueStep = 90f, DecimalCount = 0)]
			public float SpriteOrientation { get; set; }

			// Token: 0x170014F3 RID: 5363
			// (get) Token: 0x060056DB RID: 22235 RVA: 0x001F4153 File Offset: 0x001F2353
			// (set) Token: 0x060056DC RID: 22236 RVA: 0x001F415B File Offset: 0x001F235B
			[Serialize(LimbType.None, IsPropertySaveable.Yes, "If set, the limb sprite will use the same sprite depth as the specified limb. Generally only useful for limbs that get added on the ragdoll on the fly (e.g. extra limbs added via gene splicing).", "", false)]
			public LimbType InheritLimbDepth { get; set; }

			// Token: 0x170014F4 RID: 5364
			// (get) Token: 0x060056DD RID: 22237 RVA: 0x001F4164 File Offset: 0x001F2364
			// (set) Token: 0x060056DE RID: 22238 RVA: 0x001F416C File Offset: 0x001F236C
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 500f)]
			public float SteerForce { get; set; }

			// Token: 0x170014F5 RID: 5365
			// (get) Token: 0x060056DF RID: 22239 RVA: 0x001F4175 File Offset: 0x001F2375
			// (set) Token: 0x060056E0 RID: 22240 RVA: 0x001F417D File Offset: 0x001F237D
			[Serialize(0f, IsPropertySaveable.Yes, "Radius of the collider.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Radius { get; set; }

			// Token: 0x170014F6 RID: 5366
			// (get) Token: 0x060056E1 RID: 22241 RVA: 0x001F4186 File Offset: 0x001F2386
			// (set) Token: 0x060056E2 RID: 22242 RVA: 0x001F418E File Offset: 0x001F238E
			[Serialize(0f, IsPropertySaveable.Yes, "Height of the collider.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Height { get; set; }

			// Token: 0x170014F7 RID: 5367
			// (get) Token: 0x060056E3 RID: 22243 RVA: 0x001F4197 File Offset: 0x001F2397
			// (set) Token: 0x060056E4 RID: 22244 RVA: 0x001F419F File Offset: 0x001F239F
			[Serialize(0f, IsPropertySaveable.Yes, "Width of the collider.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Width { get; set; }

			// Token: 0x170014F8 RID: 5368
			// (get) Token: 0x060056E5 RID: 22245 RVA: 0x001F41A8 File Offset: 0x001F23A8
			// (set) Token: 0x060056E6 RID: 22246 RVA: 0x001F41B0 File Offset: 0x001F23B0
			[Serialize(10f, IsPropertySaveable.Yes, "The more the density the heavier the limb is.", "", false)]
			[Editable(MinValueFloat = 0.01f, MaxValueFloat = 100f, DecimalCount = 2)]
			public float Density { get; set; }

			// Token: 0x170014F9 RID: 5369
			// (get) Token: 0x060056E7 RID: 22247 RVA: 0x001F41B9 File Offset: 0x001F23B9
			// (set) Token: 0x060056E8 RID: 22248 RVA: 0x001F41C1 File Offset: 0x001F23C1
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool IgnoreCollisions { get; set; }

			// Token: 0x170014FA RID: 5370
			// (get) Token: 0x060056E9 RID: 22249 RVA: 0x001F41CA File Offset: 0x001F23CA
			// (set) Token: 0x060056EA RID: 22250 RVA: 0x001F41D2 File Offset: 0x001F23D2
			[Serialize(7f, IsPropertySaveable.Yes, "Increasing the damping makes the limb stop rotating more quickly.", "", false)]
			[Editable]
			public float AngularDamping { get; set; }

			// Token: 0x170014FB RID: 5371
			// (get) Token: 0x060056EB RID: 22251 RVA: 0x001F41DB File Offset: 0x001F23DB
			// (set) Token: 0x060056EC RID: 22252 RVA: 0x001F41E3 File Offset: 0x001F23E3
			[Serialize(1f, IsPropertySaveable.Yes, "Higher values make AI characters prefer attacking this limb.", "", false)]
			[Editable(MinValueFloat = 0.1f, MaxValueFloat = 10f)]
			public float AttackPriority { get; set; }

			// Token: 0x170014FC RID: 5372
			// (get) Token: 0x060056ED RID: 22253 RVA: 0x001F41EC File Offset: 0x001F23EC
			// (set) Token: 0x060056EE RID: 22254 RVA: 0x001F41F4 File Offset: 0x001F23F4
			[Serialize("0, 0", IsPropertySaveable.Yes, "The position which is used to lead the IK chain to the IK goal. Only applicable if the limb is hand or foot.", "", false)]
			[Editable]
			public Vector2 PullPos { get; set; }

			// Token: 0x170014FD RID: 5373
			// (get) Token: 0x060056EF RID: 22255 RVA: 0x001F41FD File Offset: 0x001F23FD
			// (set) Token: 0x060056F0 RID: 22256 RVA: 0x001F4205 File Offset: 0x001F2405
			[Serialize("0, 0", IsPropertySaveable.Yes, "Only applicable if this limb is a foot. Determines the \"neutral position\" of the foot relative to a joint determined by the \"RefJoint\" parameter. For example, a value of {-100, 0} would mean that the foot is positioned on the floor, 100 units behind the reference joint.", "", false)]
			[Editable]
			public Vector2 StepOffset { get; set; }

			// Token: 0x170014FE RID: 5374
			// (get) Token: 0x060056F1 RID: 22257 RVA: 0x001F420E File Offset: 0x001F240E
			// (set) Token: 0x060056F2 RID: 22258 RVA: 0x001F4216 File Offset: 0x001F2416
			[Serialize(-1, IsPropertySaveable.Yes, "The id of the refecence joint. Determines which joint is used as the \"neutral x-position\" for the foot movement. For example in the case of a humanoid-shaped characters this would usually be the waist. The position can be offset using the StepOffset parameter. Only applicable if this limb is a foot.", "", false)]
			[Editable]
			public int RefJoint { get; set; }

			// Token: 0x170014FF RID: 5375
			// (get) Token: 0x060056F3 RID: 22259 RVA: 0x001F421F File Offset: 0x001F241F
			// (set) Token: 0x060056F4 RID: 22260 RVA: 0x001F4227 File Offset: 0x001F2427
			[Serialize("0, 0", IsPropertySaveable.Yes, "Relative offset for the mouth position (starting from the center). Only applicable for LimbType.Head. Used for eating.", "", false)]
			[Editable(DecimalCount = 2, MinValueFloat = -10f, MaxValueFloat = 10f)]
			public Vector2 MouthPos { get; set; }

			// Token: 0x17001500 RID: 5376
			// (get) Token: 0x060056F5 RID: 22261 RVA: 0x001F4230 File Offset: 0x001F2430
			// (set) Token: 0x060056F6 RID: 22262 RVA: 0x001F4238 File Offset: 0x001F2438
			[Serialize(50f, IsPropertySaveable.Yes, "How much torque is applied on the head while updating the eating animations?", "", false)]
			[Editable]
			public float EatTorque { get; set; }

			// Token: 0x17001501 RID: 5377
			// (get) Token: 0x060056F7 RID: 22263 RVA: 0x001F4241 File Offset: 0x001F2441
			// (set) Token: 0x060056F8 RID: 22264 RVA: 0x001F4249 File Offset: 0x001F2449
			[Serialize(2f, IsPropertySaveable.Yes, "How strong a linear impulse is applied on the head while updating the eating animations?", "", false)]
			[Editable]
			public float EatImpulse { get; set; }

			// Token: 0x17001502 RID: 5378
			// (get) Token: 0x060056F9 RID: 22265 RVA: 0x001F4252 File Offset: 0x001F2452
			// (set) Token: 0x060056FA RID: 22266 RVA: 0x001F425A File Offset: 0x001F245A
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float ConstantTorque { get; set; }

			// Token: 0x17001503 RID: 5379
			// (get) Token: 0x060056FB RID: 22267 RVA: 0x001F4263 File Offset: 0x001F2463
			// (set) Token: 0x060056FC RID: 22268 RVA: 0x001F426B File Offset: 0x001F246B
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float ConstantAngle { get; set; }

			// Token: 0x17001504 RID: 5380
			// (get) Token: 0x060056FD RID: 22269 RVA: 0x001F4274 File Offset: 0x001F2474
			// (set) Token: 0x060056FE RID: 22270 RVA: 0x001F427C File Offset: 0x001F247C
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(DecimalCount = 2, MinValueFloat = 0f, MaxValueFloat = 10f)]
			public float AttackForceMultiplier { get; set; }

			// Token: 0x17001505 RID: 5381
			// (get) Token: 0x060056FF RID: 22271 RVA: 0x001F4285 File Offset: 0x001F2485
			// (set) Token: 0x06005700 RID: 22272 RVA: 0x001F428D File Offset: 0x001F248D
			[Serialize(1f, IsPropertySaveable.Yes, "How much damage must be done by the attack in order to be able to cut off the limb. Note that it's evaluated after the damage modifiers.", "", false)]
			[Editable(DecimalCount = 0, MinValueFloat = 0f, MaxValueFloat = 1000f)]
			public float MinSeveranceDamage { get; set; }

			// Token: 0x17001506 RID: 5382
			// (get) Token: 0x06005701 RID: 22273 RVA: 0x001F4296 File Offset: 0x001F2496
			// (set) Token: 0x06005702 RID: 22274 RVA: 0x001F429E File Offset: 0x001F249E
			[Serialize(true, IsPropertySaveable.Yes, "Disable if you don't want to allow severing this joint while the creature is alive. Note: Does nothing if the 'Severance Probability Modifier' in the joint settings is 0 (default). Also note that the setting doesn't override certain limitations, e.g. severing the main limb, or legs of a walking creature is not allowed.", "", false)]
			[Editable]
			public bool CanBeSeveredAlive { get; set; }

			// Token: 0x17001507 RID: 5383
			// (get) Token: 0x06005703 RID: 22275 RVA: 0x001F42A7 File Offset: 0x001F24A7
			// (set) Token: 0x06005704 RID: 22276 RVA: 0x001F42AF File Offset: 0x001F24AF
			[Serialize(10f, IsPropertySaveable.Yes, "How long it takes for the severed limb to fade out", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, ValueStep = 1f)]
			public float SeveredFadeOutTime { get; set; } = 10f;

			// Token: 0x17001508 RID: 5384
			// (get) Token: 0x06005705 RID: 22277 RVA: 0x001F42B8 File Offset: 0x001F24B8
			// (set) Token: 0x06005706 RID: 22278 RVA: 0x001F42C0 File Offset: 0x001F24C0
			[Serialize(false, IsPropertySaveable.Yes, "Should the tail angle be applied on this limb? If none of the limbs have been defined to use the angle and an angle is defined in the animation parameters, the first tail limb is used.", "", false)]
			[Editable]
			public bool ApplyTailAngle { get; set; }

			// Token: 0x17001509 RID: 5385
			// (get) Token: 0x06005707 RID: 22279 RVA: 0x001F42C9 File Offset: 0x001F24C9
			// (set) Token: 0x06005708 RID: 22280 RVA: 0x001F42D1 File Offset: 0x001F24D1
			[Serialize(false, IsPropertySaveable.Yes, "Should this limb be moved like a tail when swimming? Always true for tail limbs. On tails, disable by setting SineFrequencyMultiplier to 0.", "", false)]
			[Editable]
			public bool ApplySineMovement { get; set; }

			// Token: 0x1700150A RID: 5386
			// (get) Token: 0x06005709 RID: 22281 RVA: 0x001F42DA File Offset: 0x001F24DA
			// (set) Token: 0x0600570A RID: 22282 RVA: 0x001F42E2 File Offset: 0x001F24E2
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(ValueStep = 0.1f, DecimalCount = 2)]
			public float SineFrequencyMultiplier { get; set; }

			// Token: 0x1700150B RID: 5387
			// (get) Token: 0x0600570B RID: 22283 RVA: 0x001F42EB File Offset: 0x001F24EB
			// (set) Token: 0x0600570C RID: 22284 RVA: 0x001F42F3 File Offset: 0x001F24F3
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(ValueStep = 0.1f, DecimalCount = 2)]
			public float SineAmplitudeMultiplier { get; set; }

			// Token: 0x1700150C RID: 5388
			// (get) Token: 0x0600570D RID: 22285 RVA: 0x001F42FC File Offset: 0x001F24FC
			// (set) Token: 0x0600570E RID: 22286 RVA: 0x001F4304 File Offset: 0x001F2504
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0, 100, ValueStep = 1f, DecimalCount = 1)]
			public float BlinkFrequency { get; set; }

			// Token: 0x1700150D RID: 5389
			// (get) Token: 0x0600570F RID: 22287 RVA: 0x001F430D File Offset: 0x001F250D
			// (set) Token: 0x06005710 RID: 22288 RVA: 0x001F4315 File Offset: 0x001F2515
			[Serialize(0.2f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0.01f, 10f, 1, ValueStep = 1f, DecimalCount = 2)]
			public float BlinkDurationIn { get; set; }

			// Token: 0x1700150E RID: 5390
			// (get) Token: 0x06005711 RID: 22289 RVA: 0x001F431E File Offset: 0x001F251E
			// (set) Token: 0x06005712 RID: 22290 RVA: 0x001F4326 File Offset: 0x001F2526
			[Serialize(0.5f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0.01f, 10f, 1, ValueStep = 1f, DecimalCount = 2)]
			public float BlinkDurationOut { get; set; }

			// Token: 0x1700150F RID: 5391
			// (get) Token: 0x06005713 RID: 22291 RVA: 0x001F432F File Offset: 0x001F252F
			// (set) Token: 0x06005714 RID: 22292 RVA: 0x001F4337 File Offset: 0x001F2537
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0, 10, ValueStep = 1f, DecimalCount = 2)]
			public float BlinkHoldTime { get; set; }

			// Token: 0x17001510 RID: 5392
			// (get) Token: 0x06005715 RID: 22293 RVA: 0x001F4340 File Offset: 0x001F2540
			// (set) Token: 0x06005716 RID: 22294 RVA: 0x001F4348 File Offset: 0x001F2548
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(-360, 360, ValueStep = 1f, DecimalCount = 0)]
			public float BlinkRotationIn { get; set; }

			// Token: 0x17001511 RID: 5393
			// (get) Token: 0x06005717 RID: 22295 RVA: 0x001F4351 File Offset: 0x001F2551
			// (set) Token: 0x06005718 RID: 22296 RVA: 0x001F4359 File Offset: 0x001F2559
			[Serialize(45f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(-360, 360, ValueStep = 1f, DecimalCount = 0)]
			public float BlinkRotationOut { get; set; }

			// Token: 0x17001512 RID: 5394
			// (get) Token: 0x06005719 RID: 22297 RVA: 0x001F4362 File Offset: 0x001F2562
			// (set) Token: 0x0600571A RID: 22298 RVA: 0x001F436A File Offset: 0x001F256A
			[Serialize(50f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float BlinkForce { get; set; }

			// Token: 0x17001513 RID: 5395
			// (get) Token: 0x0600571B RID: 22299 RVA: 0x001F4373 File Offset: 0x001F2573
			// (set) Token: 0x0600571C RID: 22300 RVA: 0x001F437B File Offset: 0x001F257B
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool OnlyBlinkInWater { get; set; }

			// Token: 0x17001514 RID: 5396
			// (get) Token: 0x0600571D RID: 22301 RVA: 0x001F4384 File Offset: 0x001F2584
			// (set) Token: 0x0600571E RID: 22302 RVA: 0x001F438C File Offset: 0x001F258C
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool UseTextureOffsetForBlinking { get; set; }

			// Token: 0x17001515 RID: 5397
			// (get) Token: 0x0600571F RID: 22303 RVA: 0x001F4395 File Offset: 0x001F2595
			// (set) Token: 0x06005720 RID: 22304 RVA: 0x001F439D File Offset: 0x001F259D
			[Serialize("0.5, 0.5", IsPropertySaveable.Yes, "", "", false)]
			[Editable(DecimalCount = 2, MinValueFloat = 0f, MaxValueFloat = 1f)]
			public Vector2 BlinkTextureOffsetIn { get; set; }

			// Token: 0x17001516 RID: 5398
			// (get) Token: 0x06005721 RID: 22305 RVA: 0x001F43A6 File Offset: 0x001F25A6
			// (set) Token: 0x06005722 RID: 22306 RVA: 0x001F43AE File Offset: 0x001F25AE
			[Serialize("0.5, 0.5", IsPropertySaveable.Yes, "", "", false)]
			[Editable(DecimalCount = 2, MinValueFloat = 0f, MaxValueFloat = 1f)]
			public Vector2 BlinkTextureOffsetOut { get; set; }

			// Token: 0x17001517 RID: 5399
			// (get) Token: 0x06005723 RID: 22307 RVA: 0x001F43B7 File Offset: 0x001F25B7
			// (set) Token: 0x06005724 RID: 22308 RVA: 0x001F43BF File Offset: 0x001F25BF
			[Serialize(TransitionMode.Linear, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public TransitionMode BlinkTransitionIn { get; private set; }

			// Token: 0x17001518 RID: 5400
			// (get) Token: 0x06005725 RID: 22309 RVA: 0x001F43C8 File Offset: 0x001F25C8
			// (set) Token: 0x06005726 RID: 22310 RVA: 0x001F43D0 File Offset: 0x001F25D0
			[Serialize(TransitionMode.Linear, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public TransitionMode BlinkTransitionOut { get; private set; }

			// Token: 0x17001519 RID: 5401
			// (get) Token: 0x06005727 RID: 22311 RVA: 0x001F43D9 File Offset: 0x001F25D9
			// (set) Token: 0x06005728 RID: 22312 RVA: 0x001F43E1 File Offset: 0x001F25E1
			[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
			public int HealthIndex { get; set; }

			// Token: 0x1700151A RID: 5402
			// (get) Token: 0x06005729 RID: 22313 RVA: 0x001F43EA File Offset: 0x001F25EA
			// (set) Token: 0x0600572A RID: 22314 RVA: 0x001F43F2 File Offset: 0x001F25F2
			[Serialize(0.3f, IsPropertySaveable.Yes, "", "", false)]
			public float Friction { get; set; }

			// Token: 0x1700151B RID: 5403
			// (get) Token: 0x0600572B RID: 22315 RVA: 0x001F43FB File Offset: 0x001F25FB
			// (set) Token: 0x0600572C RID: 22316 RVA: 0x001F4403 File Offset: 0x001F2603
			[Serialize(0.05f, IsPropertySaveable.Yes, "", "", false)]
			public float Restitution { get; set; }

			// Token: 0x1700151C RID: 5404
			// (get) Token: 0x0600572D RID: 22317 RVA: 0x001F440C File Offset: 0x001F260C
			// (set) Token: 0x0600572E RID: 22318 RVA: 0x001F4414 File Offset: 0x001F2614
			[Serialize(true, IsPropertySaveable.Yes, "Can the limb enter submarines? Only valid if the ragdoll's CanEnterSubmarine is set to Partial, otherwise the limb can enter if the ragdoll can.", "", false)]
			[Editable]
			public bool CanEnterSubmarine { get; private set; }

			// Token: 0x1700151D RID: 5405
			// (get) Token: 0x0600572F RID: 22319 RVA: 0x001F441D File Offset: 0x001F261D
			// (set) Token: 0x06005730 RID: 22320 RVA: 0x001F4425 File Offset: 0x001F2625
			[Serialize(LimbType.None, IsPropertySaveable.Yes, "When set to something else than None, this limb will be hidden if the limb of the specified type is hidden.", "", false)]
			[Editable]
			public LimbType InheritHiding { get; set; }

			// Token: 0x06005731 RID: 22321 RVA: 0x001F4430 File Offset: 0x001F2630
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

			// Token: 0x06005732 RID: 22322 RVA: 0x001F4678 File Offset: 0x001F2878
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

			// Token: 0x06005733 RID: 22323 RVA: 0x001F46DC File Offset: 0x001F28DC
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

			// Token: 0x06005734 RID: 22324 RVA: 0x001F4740 File Offset: 0x001F2940
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

			// Token: 0x06005735 RID: 22325 RVA: 0x001F47CC File Offset: 0x001F29CC
			public bool AddDamageModifier()
			{
				RagdollParams.DamageModifierParams damageModifierParams;
				return this.TryAddSubParam<RagdollParams.DamageModifierParams>(base.CreateElement("damagemodifier", Array.Empty<object>()), (ContentXElement e, RagdollParams c) => new RagdollParams.DamageModifierParams(e, c), out damageModifierParams, this.DamageModifiers, null);
			}

			// Token: 0x06005736 RID: 22326 RVA: 0x001F4817 File Offset: 0x001F2A17
			public bool RemoveAttack()
			{
				if (this.RemoveSubParam<RagdollParams.AttackParams>(this.Attack, null))
				{
					this.Attack = null;
					return true;
				}
				return false;
			}

			// Token: 0x06005737 RID: 22327 RVA: 0x001F4832 File Offset: 0x001F2A32
			public bool RemoveSound()
			{
				if (this.RemoveSubParam<RagdollParams.SoundParams>(this.Sound, null))
				{
					this.Sound = null;
					return true;
				}
				return false;
			}

			// Token: 0x06005738 RID: 22328 RVA: 0x001F484D File Offset: 0x001F2A4D
			public bool RemoveLight()
			{
				if (this.RemoveSubParam<RagdollParams.LightSourceParams>(this.LightSource, null))
				{
					this.LightSource = null;
					return true;
				}
				return false;
			}

			// Token: 0x06005739 RID: 22329 RVA: 0x001F4868 File Offset: 0x001F2A68
			public bool RemoveDamageModifier(RagdollParams.DamageModifierParams damageModifier)
			{
				return this.RemoveSubParam<RagdollParams.DamageModifierParams>(damageModifier, this.DamageModifiers);
			}

			// Token: 0x0600573A RID: 22330 RVA: 0x001F4878 File Offset: 0x001F2A78
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

			// Token: 0x0600573B RID: 22331 RVA: 0x001F48F0 File Offset: 0x001F2AF0
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

			// Token: 0x040030D5 RID: 12501
			public readonly RagdollParams.SpriteParams normalSpriteParams;

			// Token: 0x040030D6 RID: 12502
			public readonly RagdollParams.SpriteParams damagedSpriteParams;

			// Token: 0x040030D7 RID: 12503
			public readonly RagdollParams.DeformSpriteParams deformSpriteParams;

			// Token: 0x040030D8 RID: 12504
			public readonly List<RagdollParams.DecorativeSpriteParams> decorativeSpriteParams = new List<RagdollParams.DecorativeSpriteParams>();

			// Token: 0x040030DD RID: 12509
			private string name;
		}

		// Token: 0x020008B0 RID: 2224
		public class DecorativeSpriteParams : RagdollParams.SpriteParams
		{
			// Token: 0x0600573C RID: 22332 RVA: 0x001F498F File Offset: 0x001F2B8F
			public DecorativeSpriteParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
			}
		}

		// Token: 0x020008B1 RID: 2225
		public class DeformSpriteParams : RagdollParams.SpriteParams
		{
			// Token: 0x1700151E RID: 5406
			// (get) Token: 0x0600573D RID: 22333 RVA: 0x001F4999 File Offset: 0x001F2B99
			// (set) Token: 0x0600573E RID: 22334 RVA: 0x001F49A1 File Offset: 0x001F2BA1
			public RagdollParams.DeformationParams Deformation { get; private set; }

			// Token: 0x0600573F RID: 22335 RVA: 0x001F49AA File Offset: 0x001F2BAA
			public DeformSpriteParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				this.Deformation = new RagdollParams.DeformationParams(element, ragdoll);
				base.SubParams.Add(this.Deformation);
			}
		}

		// Token: 0x020008B2 RID: 2226
		public class SpriteParams : RagdollParams.SubParam
		{
			// Token: 0x1700151F RID: 5407
			// (get) Token: 0x06005740 RID: 22336 RVA: 0x001F49D2 File Offset: 0x001F2BD2
			// (set) Token: 0x06005741 RID: 22337 RVA: 0x001F49DA File Offset: 0x001F2BDA
			[Serialize("0, 0, 0, 0", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public Rectangle SourceRect { get; set; }

			// Token: 0x17001520 RID: 5408
			// (get) Token: 0x06005742 RID: 22338 RVA: 0x001F49E3 File Offset: 0x001F2BE3
			// (set) Token: 0x06005743 RID: 22339 RVA: 0x001F49EB File Offset: 0x001F2BEB
			[Serialize("0.5, 0.5", IsPropertySaveable.Yes, "The origin of the sprite relative to the collider.", "", false)]
			[Editable(DecimalCount = 3)]
			public Vector2 Origin { get; set; }

			// Token: 0x17001521 RID: 5409
			// (get) Token: 0x06005744 RID: 22340 RVA: 0x001F49F4 File Offset: 0x001F2BF4
			// (set) Token: 0x06005745 RID: 22341 RVA: 0x001F49FC File Offset: 0x001F2BFC
			[Serialize(0f, IsPropertySaveable.Yes, "The Z-depth of the limb relative to other limbs of the same character. 1 is front, 0 is behind.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 3)]
			public float Depth { get; set; }

			// Token: 0x17001522 RID: 5410
			// (get) Token: 0x06005746 RID: 22342 RVA: 0x001F4A05 File Offset: 0x001F2C05
			// (set) Token: 0x06005747 RID: 22343 RVA: 0x001F4A0D File Offset: 0x001F2C0D
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string Texture { get; set; }

			// Token: 0x17001523 RID: 5411
			// (get) Token: 0x06005748 RID: 22344 RVA: 0x001F4A16 File Offset: 0x001F2C16
			// (set) Token: 0x06005749 RID: 22345 RVA: 0x001F4A1E File Offset: 0x001F2C1E
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool IgnoreTint { get; set; }

			// Token: 0x17001524 RID: 5412
			// (get) Token: 0x0600574A RID: 22346 RVA: 0x001F4A27 File Offset: 0x001F2C27
			// (set) Token: 0x0600574B RID: 22347 RVA: 0x001F4A2F File Offset: 0x001F2C2F
			[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public Color Color { get; set; }

			// Token: 0x17001525 RID: 5413
			// (get) Token: 0x0600574C RID: 22348 RVA: 0x001F4A38 File Offset: 0x001F2C38
			// (set) Token: 0x0600574D RID: 22349 RVA: 0x001F4A40 File Offset: 0x001F2C40
			[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "Target color when the character is dead.", "", false)]
			[Editable]
			public Color DeadColor { get; set; }

			// Token: 0x17001526 RID: 5414
			// (get) Token: 0x0600574E RID: 22350 RVA: 0x001F4A49 File Offset: 0x001F2C49
			// (set) Token: 0x0600574F RID: 22351 RVA: 0x001F4A51 File Offset: 0x001F2C51
			[Serialize(0f, IsPropertySaveable.Yes, "How long it takes to fade into the dead color? 0 = Not applied.", "", false)]
			[Editable(DecimalCount = 1, MinValueFloat = 0f, MaxValueFloat = 10f)]
			public float DeadColorTime { get; set; }

			// Token: 0x17001527 RID: 5415
			// (get) Token: 0x06005750 RID: 22352 RVA: 0x001F4A5A File Offset: 0x001F2C5A
			public override string Name
			{
				get
				{
					return "Sprite";
				}
			}

			// Token: 0x06005751 RID: 22353 RVA: 0x001F4A61 File Offset: 0x001F2C61
			public SpriteParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
			}

			// Token: 0x06005752 RID: 22354 RVA: 0x001F4A6B File Offset: 0x001F2C6B
			public string GetTexturePath()
			{
				if (!string.IsNullOrWhiteSpace(this.Texture))
				{
					return this.Texture;
				}
				return base.Ragdoll.Texture;
			}
		}

		// Token: 0x020008B3 RID: 2227
		public class DeformationParams : RagdollParams.SubParam
		{
			// Token: 0x06005753 RID: 22355 RVA: 0x001F4A8C File Offset: 0x001F2C8C
			public DeformationParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
			}
		}

		// Token: 0x020008B4 RID: 2228
		public class ColliderParams : RagdollParams.SubParam
		{
			// Token: 0x17001528 RID: 5416
			// (get) Token: 0x06005754 RID: 22356 RVA: 0x001F4A96 File Offset: 0x001F2C96
			// (set) Token: 0x06005755 RID: 22357 RVA: 0x001F4AB7 File Offset: 0x001F2CB7
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

			// Token: 0x17001529 RID: 5417
			// (get) Token: 0x06005756 RID: 22358 RVA: 0x001F4AC0 File Offset: 0x001F2CC0
			// (set) Token: 0x06005757 RID: 22359 RVA: 0x001F4AC8 File Offset: 0x001F2CC8
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Radius { get; set; }

			// Token: 0x1700152A RID: 5418
			// (get) Token: 0x06005758 RID: 22360 RVA: 0x001F4AD1 File Offset: 0x001F2CD1
			// (set) Token: 0x06005759 RID: 22361 RVA: 0x001F4AD9 File Offset: 0x001F2CD9
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Height { get; set; }

			// Token: 0x1700152B RID: 5419
			// (get) Token: 0x0600575A RID: 22362 RVA: 0x001F4AE2 File Offset: 0x001F2CE2
			// (set) Token: 0x0600575B RID: 22363 RVA: 0x001F4AEA File Offset: 0x001F2CEA
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
			public float Width { get; set; }

			// Token: 0x1700152C RID: 5420
			// (get) Token: 0x0600575C RID: 22364 RVA: 0x001F4AF3 File Offset: 0x001F2CF3
			// (set) Token: 0x0600575D RID: 22365 RVA: 0x001F4AFB File Offset: 0x001F2CFB
			[Serialize(BodyType.Dynamic, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public BodyType BodyType { get; set; }

			// Token: 0x0600575E RID: 22366 RVA: 0x001F4B04 File Offset: 0x001F2D04
			public ColliderParams(ContentXElement element, RagdollParams ragdoll, string name = null) : base(element, ragdoll)
			{
				this.Name = name;
			}

			// Token: 0x0400311C RID: 12572
			private string name;
		}

		// Token: 0x020008B5 RID: 2229
		public class LightSourceParams : RagdollParams.SubParam
		{
			// Token: 0x1700152D RID: 5421
			// (get) Token: 0x0600575F RID: 22367 RVA: 0x001F4B15 File Offset: 0x001F2D15
			// (set) Token: 0x06005760 RID: 22368 RVA: 0x001F4B1D File Offset: 0x001F2D1D
			public RagdollParams.LightSourceParams.LightTexture Texture { get; private set; }

			// Token: 0x06005761 RID: 22369 RVA: 0x001F4B28 File Offset: 0x001F2D28
			public LightSourceParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				ContentXElement lightTextureElement = element.GetChildElement("lighttexture");
				ContentXElement contentXElement = null;
				if (lightTextureElement != contentXElement)
				{
					this.Texture = new RagdollParams.LightSourceParams.LightTexture(lightTextureElement, ragdoll);
					base.SubParams.Add(this.Texture);
				}
			}

			// Token: 0x02000E8B RID: 3723
			public class LightTexture : RagdollParams.SubParam
			{
				// Token: 0x170016A6 RID: 5798
				// (get) Token: 0x06006A31 RID: 27185 RVA: 0x00225C3E File Offset: 0x00223E3E
				public override string Name
				{
					get
					{
						return "Light Texture";
					}
				}

				// Token: 0x170016A7 RID: 5799
				// (get) Token: 0x06006A32 RID: 27186 RVA: 0x00225C45 File Offset: 0x00223E45
				// (set) Token: 0x06006A33 RID: 27187 RVA: 0x00225C4D File Offset: 0x00223E4D
				[Serialize("Content/Lights/pointlight_bright.png", IsPropertySaveable.Yes, "", "", false)]
				[Editable]
				public string Texture { get; private set; }

				// Token: 0x170016A8 RID: 5800
				// (get) Token: 0x06006A34 RID: 27188 RVA: 0x00225C56 File Offset: 0x00223E56
				// (set) Token: 0x06006A35 RID: 27189 RVA: 0x00225C5E File Offset: 0x00223E5E
				[Serialize("0.5, 0.5", IsPropertySaveable.Yes, "", "", false)]
				[Editable(DecimalCount = 2)]
				public Vector2 Origin { get; set; }

				// Token: 0x170016A9 RID: 5801
				// (get) Token: 0x06006A36 RID: 27190 RVA: 0x00225C67 File Offset: 0x00223E67
				// (set) Token: 0x06006A37 RID: 27191 RVA: 0x00225C6F File Offset: 0x00223E6F
				[Serialize("1.0, 1.0", IsPropertySaveable.Yes, "", "", false)]
				[Editable(DecimalCount = 2)]
				public Vector2 Size { get; set; }

				// Token: 0x06006A38 RID: 27192 RVA: 0x00225C78 File Offset: 0x00223E78
				public LightTexture(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
				{
				}
			}
		}

		// Token: 0x020008B6 RID: 2230
		public class AttackParams : RagdollParams.SubParam
		{
			// Token: 0x1700152E RID: 5422
			// (get) Token: 0x06005762 RID: 22370 RVA: 0x001F4B74 File Offset: 0x001F2D74
			// (set) Token: 0x06005763 RID: 22371 RVA: 0x001F4B7C File Offset: 0x001F2D7C
			public Attack Attack { get; private set; }

			// Token: 0x06005764 RID: 22372 RVA: 0x001F4B88 File Offset: 0x001F2D88
			public AttackParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				this.Attack = new Attack(element, ragdoll.SpeciesName.Value);
			}

			// Token: 0x06005765 RID: 22373 RVA: 0x001F4BB8 File Offset: 0x001F2DB8
			public override bool Deserialize(ContentXElement element = null, bool recursive = true)
			{
				base.Deserialize(element, recursive);
				Attack attack = this.Attack;
				ContentXElement element2 = element ?? base.Element;
				RagdollParams ragdoll = base.Ragdoll;
				attack.Deserialize(element2, ((ragdoll != null) ? ragdoll.SpeciesName.ToString() : null) ?? "null");
				return base.SerializableProperties != null;
			}

			// Token: 0x06005766 RID: 22374 RVA: 0x001F4C16 File Offset: 0x001F2E16
			public override bool Serialize(ContentXElement element = null, bool recursive = true)
			{
				base.Serialize(element, recursive);
				this.Attack.Serialize(element ?? base.Element);
				return true;
			}

			// Token: 0x06005767 RID: 22375 RVA: 0x001F4C38 File Offset: 0x001F2E38
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

			// Token: 0x06005768 RID: 22376 RVA: 0x001F4CC0 File Offset: 0x001F2EC0
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

			// Token: 0x06005769 RID: 22377 RVA: 0x001F4D88 File Offset: 0x001F2F88
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

		// Token: 0x020008B7 RID: 2231
		public class DamageModifierParams : RagdollParams.SubParam
		{
			// Token: 0x1700152F RID: 5423
			// (get) Token: 0x0600576A RID: 22378 RVA: 0x001F4DE6 File Offset: 0x001F2FE6
			// (set) Token: 0x0600576B RID: 22379 RVA: 0x001F4DEE File Offset: 0x001F2FEE
			public DamageModifier DamageModifier { get; private set; }

			// Token: 0x0600576C RID: 22380 RVA: 0x001F4DF8 File Offset: 0x001F2FF8
			public DamageModifierParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
				this.DamageModifier = new DamageModifier(element, ragdoll.SpeciesName.Value, true);
			}

			// Token: 0x0600576D RID: 22381 RVA: 0x001F4E28 File Offset: 0x001F3028
			public override bool Deserialize(ContentXElement element = null, bool recursive = true)
			{
				base.Deserialize(element, recursive);
				this.DamageModifier.Deserialize(element ?? base.Element);
				return base.SerializableProperties != null;
			}

			// Token: 0x0600576E RID: 22382 RVA: 0x001F4E57 File Offset: 0x001F3057
			public override bool Serialize(ContentXElement element = null, bool recursive = true)
			{
				base.Serialize(element, recursive);
				this.DamageModifier.Serialize(element ?? base.Element);
				return true;
			}

			// Token: 0x0600576F RID: 22383 RVA: 0x001F4E7E File Offset: 0x001F307E
			public override void Reset()
			{
				base.Reset();
				this.DamageModifier.Deserialize(base.OriginalElement);
			}
		}

		// Token: 0x020008B8 RID: 2232
		public class SoundParams : RagdollParams.SubParam
		{
			// Token: 0x17001530 RID: 5424
			// (get) Token: 0x06005770 RID: 22384 RVA: 0x001F4E9C File Offset: 0x001F309C
			public override string Name
			{
				get
				{
					return "Sound";
				}
			}

			// Token: 0x17001531 RID: 5425
			// (get) Token: 0x06005771 RID: 22385 RVA: 0x001F4EA3 File Offset: 0x001F30A3
			// (set) Token: 0x06005772 RID: 22386 RVA: 0x001F4EAB File Offset: 0x001F30AB
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string Tag { get; private set; }

			// Token: 0x06005773 RID: 22387 RVA: 0x001F4EB4 File Offset: 0x001F30B4
			public SoundParams(ContentXElement element, RagdollParams ragdoll) : base(element, ragdoll)
			{
			}
		}

		// Token: 0x020008B9 RID: 2233
		public abstract class SubParam : ISerializableEntity
		{
			// Token: 0x17001532 RID: 5426
			// (get) Token: 0x06005774 RID: 22388 RVA: 0x001F4EBE File Offset: 0x001F30BE
			// (set) Token: 0x06005775 RID: 22389 RVA: 0x001F4EC6 File Offset: 0x001F30C6
			public virtual string Name { get; set; }

			// Token: 0x17001533 RID: 5427
			// (get) Token: 0x06005776 RID: 22390 RVA: 0x001F4ECF File Offset: 0x001F30CF
			// (set) Token: 0x06005777 RID: 22391 RVA: 0x001F4ED7 File Offset: 0x001F30D7
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

			// Token: 0x17001534 RID: 5428
			// (get) Token: 0x06005778 RID: 22392 RVA: 0x001F4EE0 File Offset: 0x001F30E0
			// (set) Token: 0x06005779 RID: 22393 RVA: 0x001F4EE8 File Offset: 0x001F30E8
			public ContentXElement Element { get; set; }

			// Token: 0x17001535 RID: 5429
			// (get) Token: 0x0600577A RID: 22394 RVA: 0x001F4EF1 File Offset: 0x001F30F1
			// (set) Token: 0x0600577B RID: 22395 RVA: 0x001F4EF9 File Offset: 0x001F30F9
			public ContentXElement OriginalElement { get; protected set; }

			// Token: 0x17001536 RID: 5430
			// (get) Token: 0x0600577C RID: 22396 RVA: 0x001F4F02 File Offset: 0x001F3102
			// (set) Token: 0x0600577D RID: 22397 RVA: 0x001F4F0A File Offset: 0x001F310A
			public List<RagdollParams.SubParam> SubParams { get; set; } = new List<RagdollParams.SubParam>();

			// Token: 0x17001537 RID: 5431
			// (get) Token: 0x0600577E RID: 22398 RVA: 0x001F4F13 File Offset: 0x001F3113
			// (set) Token: 0x0600577F RID: 22399 RVA: 0x001F4F1B File Offset: 0x001F311B
			public RagdollParams Ragdoll { get; private set; }

			// Token: 0x06005780 RID: 22400 RVA: 0x001F4F24 File Offset: 0x001F3124
			public virtual string GenerateName()
			{
				return this.Element.Name.ToString();
			}

			// Token: 0x06005781 RID: 22401 RVA: 0x001F4F36 File Offset: 0x001F3136
			protected ContentXElement CreateElement(string name, params object[] attrs)
			{
				return new XElement(name, attrs).FromPackage(this.Element.ContentPackage);
			}

			// Token: 0x06005782 RID: 22402 RVA: 0x001F4F54 File Offset: 0x001F3154
			public SubParam(ContentXElement element, RagdollParams ragdoll)
			{
				this.Element = element;
				this.OriginalElement = new ContentXElement(element.ContentPackage, element);
				this.Ragdoll = ragdoll;
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			}

			// Token: 0x06005783 RID: 22403 RVA: 0x001F4FAC File Offset: 0x001F31AC
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

			// Token: 0x06005784 RID: 22404 RVA: 0x001F500C File Offset: 0x001F320C
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

			// Token: 0x06005785 RID: 22405 RVA: 0x001F5060 File Offset: 0x001F3260
			public virtual void SetCurrentElementAsOriginalElement()
			{
				this.OriginalElement = this.Element;
				this.SubParams.ForEach(delegate(RagdollParams.SubParam sp)
				{
					sp.SetCurrentElementAsOriginalElement();
				});
			}

			// Token: 0x06005786 RID: 22406 RVA: 0x001F5098 File Offset: 0x001F3298
			public virtual void Reset()
			{
				this.Deserialize(this.OriginalElement, false);
				this.SubParams.ForEach(delegate(RagdollParams.SubParam sp)
				{
					sp.Reset();
				});
			}
		}
	}
}
