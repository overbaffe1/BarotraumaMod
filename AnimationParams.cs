using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001DA RID: 474
	internal abstract class AnimationParams : EditableParams, IMemorizable<AnimationParams>
	{
		// Token: 0x17000D62 RID: 3426
		// (get) Token: 0x060032B2 RID: 12978 RVA: 0x00209FEA File Offset: 0x002081EA
		// (set) Token: 0x060032B3 RID: 12979 RVA: 0x00209FF2 File Offset: 0x002081F2
		public Identifier SpeciesName { get; private set; }

		// Token: 0x17000D63 RID: 3427
		// (get) Token: 0x060032B4 RID: 12980 RVA: 0x00209FFC File Offset: 0x002081FC
		public bool IsGroundedAnimation
		{
			get
			{
				AnimationType animationType = this.AnimationType;
				return animationType - AnimationType.Walk <= 1 || animationType == AnimationType.Crouch;
			}
		}

		// Token: 0x17000D64 RID: 3428
		// (get) Token: 0x060032B5 RID: 12981 RVA: 0x0020A024 File Offset: 0x00208224
		public bool IsSwimAnimation
		{
			get
			{
				AnimationType animationType = this.AnimationType;
				return animationType - AnimationType.SwimSlow <= 1;
			}
		}

		// Token: 0x17000D65 RID: 3429
		// (get) Token: 0x060032B6 RID: 12982 RVA: 0x0020A045 File Offset: 0x00208245
		// (set) Token: 0x060032B7 RID: 12983 RVA: 0x0020A04D File Offset: 0x0020824D
		[Header("General", null)]
		[Serialize(AnimationType.NotDefined, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public virtual AnimationType AnimationType { get; protected set; }

		// Token: 0x17000D66 RID: 3430
		// (get) Token: 0x060032B8 RID: 12984 RVA: 0x0020A056 File Offset: 0x00208256
		// (set) Token: 0x060032B9 RID: 12985 RVA: 0x0020A05E File Offset: 0x0020825E
		[Header("Movement", null)]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2, MinValueFloat = 0f, MaxValueFloat = 20f, ValueStep = 0.1f)]
		public float MovementSpeed { get; set; }

		// Token: 0x17000D67 RID: 3431
		// (get) Token: 0x060032BA RID: 12986 RVA: 0x0020A067 File Offset: 0x00208267
		// (set) Token: 0x060032BB RID: 12987 RVA: 0x0020A06F File Offset: 0x0020826F
		[Serialize(1f, IsPropertySaveable.Yes, "The speed of the \"animation cycle\", i.e. how fast the character takes steps or moves the tail/legs/arms (the outcome depends what the clip is about)", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2, ValueStep = 0.01f)]
		public float CycleSpeed { get; set; }

		// Token: 0x17000D68 RID: 3432
		// (get) Token: 0x060032BC RID: 12988 RVA: 0x0020A078 File Offset: 0x00208278
		// (set) Token: 0x060032BD RID: 12989 RVA: 0x0020A098 File Offset: 0x00208298
		[Header("Orientation", null)]
		[Serialize(float.NaN, IsPropertySaveable.Yes, "", "", false)]
		[Editable(-360f, 360f, 1)]
		public float HeadAngle
		{
			get
			{
				if (!float.IsNaN(this.HeadAngleInRadians))
				{
					return MathHelper.ToDegrees(this.HeadAngleInRadians);
				}
				return float.NaN;
			}
			set
			{
				if (!float.IsNaN(value))
				{
					this.HeadAngleInRadians = MathHelper.ToRadians(value);
				}
			}
		}

		// Token: 0x17000D69 RID: 3433
		// (get) Token: 0x060032BE RID: 12990 RVA: 0x0020A0AE File Offset: 0x002082AE
		// (set) Token: 0x060032BF RID: 12991 RVA: 0x0020A0B6 File Offset: 0x002082B6
		public float HeadAngleInRadians { get; private set; } = float.NaN;

		// Token: 0x17000D6A RID: 3434
		// (get) Token: 0x060032C0 RID: 12992 RVA: 0x0020A0BF File Offset: 0x002082BF
		// (set) Token: 0x060032C1 RID: 12993 RVA: 0x0020A0DF File Offset: 0x002082DF
		[Serialize(float.NaN, IsPropertySaveable.Yes, "", "", false)]
		[Editable(-360f, 360f, 1)]
		public float TorsoAngle
		{
			get
			{
				if (!float.IsNaN(this.TorsoAngleInRadians))
				{
					return MathHelper.ToDegrees(this.TorsoAngleInRadians);
				}
				return float.NaN;
			}
			set
			{
				if (!float.IsNaN(value))
				{
					this.TorsoAngleInRadians = MathHelper.ToRadians(value);
				}
			}
		}

		// Token: 0x17000D6B RID: 3435
		// (get) Token: 0x060032C2 RID: 12994 RVA: 0x0020A0F5 File Offset: 0x002082F5
		// (set) Token: 0x060032C3 RID: 12995 RVA: 0x0020A0FD File Offset: 0x002082FD
		public float TorsoAngleInRadians { get; private set; } = float.NaN;

		// Token: 0x17000D6C RID: 3436
		// (get) Token: 0x060032C4 RID: 12996 RVA: 0x0020A106 File Offset: 0x00208306
		// (set) Token: 0x060032C5 RID: 12997 RVA: 0x0020A10E File Offset: 0x0020830E
		[Serialize(50f, IsPropertySaveable.Yes, "How much torque is used to rotate the head to the correct orientation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float HeadTorque { get; set; }

		// Token: 0x17000D6D RID: 3437
		// (get) Token: 0x060032C6 RID: 12998 RVA: 0x0020A117 File Offset: 0x00208317
		// (set) Token: 0x060032C7 RID: 12999 RVA: 0x0020A11F File Offset: 0x0020831F
		[Serialize(50f, IsPropertySaveable.Yes, "How much torque is used to rotate the torso to the correct orientation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float TorsoTorque { get; set; }

		// Token: 0x17000D6E RID: 3438
		// (get) Token: 0x060032C8 RID: 13000 RVA: 0x0020A128 File Offset: 0x00208328
		// (set) Token: 0x060032C9 RID: 13001 RVA: 0x0020A130 File Offset: 0x00208330
		[Header("Legs", null)]
		[Serialize(25f, IsPropertySaveable.Yes, "How much torque is used to rotate the feet to the correct orientation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float FootTorque { get; set; }

		// Token: 0x17000D6F RID: 3439
		// (get) Token: 0x060032CA RID: 13002 RVA: 0x0020A139 File Offset: 0x00208339
		// (set) Token: 0x060032CB RID: 13003 RVA: 0x0020A141 File Offset: 0x00208341
		[Header("Arms", null)]
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to rotate the arms to the IK position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float ArmIKStrength { get; set; }

		// Token: 0x17000D70 RID: 3440
		// (get) Token: 0x060032CC RID: 13004 RVA: 0x0020A14A File Offset: 0x0020834A
		// (set) Token: 0x060032CD RID: 13005 RVA: 0x0020A152 File Offset: 0x00208352
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to rotate the hands to the IK position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float HandIKStrength { get; set; }

		// Token: 0x060032CE RID: 13006 RVA: 0x0020A15C File Offset: 0x0020835C
		public static string GetDefaultFileName(Identifier speciesName, AnimationType animType)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
			defaultInterpolatedStringHandler.AppendFormatted(speciesName.Value.CapitaliseFirstInvariant());
			defaultInterpolatedStringHandler.AppendFormatted<AnimationType>(animType);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060032CF RID: 13007 RVA: 0x0020A194 File Offset: 0x00208394
		public static string GetDefaultFilePath(Identifier speciesName, AnimationType animType)
		{
			return Barotrauma.IO.Path.Combine(new string[]
			{
				AnimationParams.GetFolder(speciesName),
				AnimationParams.GetDefaultFileName(speciesName, animType) + ".xml"
			});
		}

		// Token: 0x060032D0 RID: 13008 RVA: 0x0020A1C0 File Offset: 0x002083C0
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
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, (prefab != null) ? prefab.ContentPackage : null, false, false);
				return string.Empty;
			}
			return AnimationParams.GetFolder(prefab.ConfigElement, prefab.FilePath.Value);
		}

		// Token: 0x060032D1 RID: 13009 RVA: 0x0020A250 File Offset: 0x00208450
		private static string GetFolder(ContentXElement root, string filePath)
		{
			ContentXElement childElement = root.GetChildElement("animations");
			string text;
			if (childElement == null)
			{
				text = null;
			}
			else
			{
				ContentPath attributeContentPath = childElement.GetAttributeContentPath("folder");
				text = ((attributeContentPath != null) ? attributeContentPath.Value : null);
			}
			string folder = text;
			if (string.IsNullOrEmpty(folder) || folder.Equals("default", StringComparison.OrdinalIgnoreCase))
			{
				folder = Barotrauma.IO.Path.Combine(new string[]
				{
					Barotrauma.IO.Path.GetDirectoryName(filePath),
					"Animations"
				});
			}
			return folder.CleanUpPathCrossPlatform(true, "");
		}

		// Token: 0x060032D2 RID: 13010 RVA: 0x0020A2C8 File Offset: 0x002084C8
		public static IEnumerable<string> FilterAndSortFiles(IEnumerable<string> filePaths, AnimationType type)
		{
			return (from f in filePaths
			where AnimationParams.<FilterAndSortFiles>g__AnimationPredicate|59_2(f, type)
			select f).OrderBy((string f) => f, StringComparer.OrdinalIgnoreCase);
		}

		// Token: 0x060032D3 RID: 13011 RVA: 0x0020A31D File Offset: 0x0020851D
		protected static T GetDefaultAnimParams<T>(Character character, AnimationType animType) where T : AnimationParams, new()
		{
			return AnimationParams.GetAnimParams<T>(character, animType, null, true);
		}

		// Token: 0x060032D4 RID: 13012 RVA: 0x0020A328 File Offset: 0x00208528
		protected static T GetAnimParams<T>(Character character, AnimationType animType, Either<string, ContentPath> file, bool throwErrors = true) where T : AnimationParams, new()
		{
			Identifier speciesName = character.SpeciesName;
			Identifier animSpecies = speciesName;
			if (!character.VariantOf.IsEmpty)
			{
				XDocument variantFile = character.Params.VariantFile;
				string text;
				if (variantFile == null)
				{
					text = null;
				}
				else
				{
					XElement childElement = variantFile.GetRootExcludingOverride().GetChildElement("animations", StringComparison.OrdinalIgnoreCase);
					if (childElement == null)
					{
						text = null;
					}
					else
					{
						ContentPath attributeContentPath = childElement.GetAttributeContentPath("folder", character.Prefab.ContentPackage);
						text = ((attributeContentPath != null) ? attributeContentPath.Value : null);
					}
				}
				string folder = text;
				if (folder.IsNullOrEmpty() || folder.Equals("default", StringComparison.OrdinalIgnoreCase))
				{
					animSpecies = character.Prefab.GetBaseCharacterSpeciesName(speciesName);
				}
			}
			return AnimationParams.GetAnimParams<T>(speciesName, animSpecies, character.Prefab.GetBaseCharacterSpeciesName(speciesName), animType, file, throwErrors);
		}

		// Token: 0x060032D5 RID: 13013 RVA: 0x0020A3D4 File Offset: 0x002085D4
		private static T GetAnimParams<T>(Identifier speciesName, Identifier animSpecies, Identifier fallbackSpecies, AnimationType animType, Either<string, ContentPath> file, bool throwErrors = true) where T : AnimationParams, new()
		{
			ContentPath contentPath = null;
			string fileName = null;
			if (file != null && !file.TryGet(out fileName))
			{
				file.TryGet(out contentPath);
			}
			ContentPackage contentPackage2;
			if ((contentPackage2 = ((contentPath != null) ? contentPath.ContentPackage : null)) == null)
			{
				CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(speciesName);
				contentPackage2 = ((characterPrefab != null) ? characterPrefab.ContentPackage : null);
			}
			ContentPackage contentPackage = contentPackage2;
			Dictionary<string, AnimationParams> animations;
			if (!AnimationParams.allAnimations.TryGetValue(speciesName, out animations))
			{
				animations = new Dictionary<string, AnimationParams>();
				AnimationParams.allAnimations.Add(speciesName, animations);
			}
			string text;
			if ((text = fileName) == null)
			{
				text = (((contentPath != null) ? contentPath.Value : null) ?? AnimationParams.GetDefaultFileName(animSpecies, animType));
			}
			string key = text;
			AnimationParams anim;
			if (animations.TryGetValue(key, out anim) && anim.AnimationType == animType)
			{
				return (T)((object)anim);
			}
			if (!contentPath.IsNullOrEmpty())
			{
				T animInstance = Activator.CreateInstance<T>();
				if (animInstance.Load(contentPath, speciesName))
				{
					if (animInstance.AnimationType == animType)
					{
						animations.TryAdd(contentPath.Value, animInstance);
						return animInstance;
					}
					List<string> list = AnimationParams.errorMessages;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[AnimationParams] Animation type mismatch. Expected: ");
					defaultInterpolatedStringHandler.AppendFormatted<AnimationType>(animType);
					defaultInterpolatedStringHandler.AppendLiteral(", Actual: ");
					defaultInterpolatedStringHandler.AppendFormatted<AnimationType>(animInstance.AnimationType);
					defaultInterpolatedStringHandler.AppendLiteral(". Using the default animation.");
					list.Add(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					List<string> list2 = AnimationParams.errorMessages;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(110, 4);
					defaultInterpolatedStringHandler2.AppendLiteral("[AnimationParams] Failed to load an animation ");
					defaultInterpolatedStringHandler2.AppendFormatted<T>(animInstance);
					defaultInterpolatedStringHandler2.AppendLiteral(" of type ");
					defaultInterpolatedStringHandler2.AppendFormatted<AnimationType>(animType);
					defaultInterpolatedStringHandler2.AppendLiteral(" from ");
					defaultInterpolatedStringHandler2.AppendFormatted(contentPath.Value);
					defaultInterpolatedStringHandler2.AppendLiteral(" for the character ");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(speciesName);
					defaultInterpolatedStringHandler2.AppendLiteral(". Using the default animation.");
					list2.Add(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
			}
			string selectedFile = null;
			string folder = AnimationParams.GetFolder(animSpecies);
			if (Directory.Exists(folder))
			{
				string[] files = Directory.GetFiles(folder);
				if (files.None(null))
				{
					AnimationParams.errorMessages.Add("[AnimationParams] Could not find any animation files from the folder: " + folder + ". Using the default animation.");
				}
				else
				{
					IEnumerable<string> filteredFiles = AnimationParams.FilterAndSortFiles(files, animType);
					if (filteredFiles.None(null))
					{
						List<string> list3 = AnimationParams.errorMessages;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(131, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("[AnimationParams] Could not find any animation files that match the animation type ");
						defaultInterpolatedStringHandler3.AppendFormatted<AnimationType>(animType);
						defaultInterpolatedStringHandler3.AppendLiteral(" from the folder: ");
						defaultInterpolatedStringHandler3.AppendFormatted(folder);
						defaultInterpolatedStringHandler3.AppendLiteral(". Using the default animation.");
						list3.Add(defaultInterpolatedStringHandler3.ToStringAndClear());
					}
					else if (string.IsNullOrEmpty(fileName))
					{
						string defaultFileName = AnimationParams.GetDefaultFileName(animSpecies, animType);
						selectedFile = (filteredFiles.FirstOrDefault((string path) => AnimationParams.<GetAnimParams>g__PathMatchesFile|63_0<T>(path, defaultFileName)) ?? filteredFiles.First<string>());
					}
					else
					{
						selectedFile = filteredFiles.FirstOrDefault((string path) => AnimationParams.<GetAnimParams>g__PathMatchesFile|63_0<T>(path, fileName));
						if (selectedFile == null)
						{
							List<string> list4 = AnimationParams.errorMessages;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(141, 2);
							defaultInterpolatedStringHandler4.AppendLiteral("[AnimationParams] Could not find an animation file that matches the name ");
							defaultInterpolatedStringHandler4.AppendFormatted(fileName);
							defaultInterpolatedStringHandler4.AppendLiteral(" and the animation type ");
							defaultInterpolatedStringHandler4.AppendFormatted<AnimationType>(animType);
							defaultInterpolatedStringHandler4.AppendLiteral(". Using the first file of the matching type.");
							list4.Add(defaultInterpolatedStringHandler4.ToStringAndClear());
							selectedFile = filteredFiles.First<string>();
						}
					}
				}
			}
			else
			{
				AnimationParams.errorMessages.Add("[AnimationParams] Invalid directory: " + folder + ". Using the default animation.");
			}
			if (selectedFile == null)
			{
				selectedFile = AnimationParams.GetDefaultFilePath(fallbackSpecies, animType);
			}
			if (AnimationParams.errorMessages.None(null))
			{
				DebugConsole.Log("[AnimationParams] Loading animations from " + selectedFile + ".");
			}
			T animationInstance = Activator.CreateInstance<T>();
			if (animationInstance.Load(ContentPath.FromRaw(contentPackage, selectedFile), speciesName))
			{
				animations.TryAdd(key, animationInstance);
			}
			else
			{
				List<string> list5 = AnimationParams.errorMessages;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(78, 4);
				defaultInterpolatedStringHandler5.AppendLiteral("[AnimationParams] Failed to load an animation ");
				defaultInterpolatedStringHandler5.AppendFormatted<T>(animationInstance);
				defaultInterpolatedStringHandler5.AppendLiteral(" at ");
				defaultInterpolatedStringHandler5.AppendFormatted(selectedFile);
				defaultInterpolatedStringHandler5.AppendLiteral(" of type ");
				defaultInterpolatedStringHandler5.AppendFormatted<AnimationType>(animType);
				defaultInterpolatedStringHandler5.AppendLiteral(" for the character ");
				defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(speciesName);
				list5.Add(defaultInterpolatedStringHandler5.ToStringAndClear());
			}
			foreach (string errorMsg in AnimationParams.errorMessages)
			{
				if (throwErrors)
				{
					DebugConsole.ThrowError(errorMsg, null, contentPackage, false, false);
				}
				else
				{
					DebugConsole.Log("Logging a supressed (potential) error: " + errorMsg);
				}
			}
			AnimationParams.errorMessages.Clear();
			return animationInstance;
		}

		// Token: 0x060032D6 RID: 13014 RVA: 0x0020A88C File Offset: 0x00208A8C
		public static void ClearCache()
		{
			AnimationParams.allAnimations.Clear();
		}

		// Token: 0x060032D7 RID: 13015 RVA: 0x0020A898 File Offset: 0x00208A98
		public static AnimationParams Create(string fullPath, Identifier speciesName, AnimationType animationType, Type animationParamsType)
		{
			if (animationParamsType == typeof(HumanWalkParams))
			{
				return AnimationParams.Create<HumanWalkParams>(fullPath, speciesName, animationType);
			}
			if (animationParamsType == typeof(HumanRunParams))
			{
				return AnimationParams.Create<HumanRunParams>(fullPath, speciesName, animationType);
			}
			if (animationParamsType == typeof(HumanSwimSlowParams))
			{
				return AnimationParams.Create<HumanSwimSlowParams>(fullPath, speciesName, animationType);
			}
			if (animationParamsType == typeof(HumanSwimFastParams))
			{
				return AnimationParams.Create<HumanSwimFastParams>(fullPath, speciesName, animationType);
			}
			if (animationParamsType == typeof(HumanCrouchParams))
			{
				return AnimationParams.Create<HumanCrouchParams>(fullPath, speciesName, animationType);
			}
			if (animationParamsType == typeof(FishWalkParams))
			{
				return AnimationParams.Create<FishWalkParams>(fullPath, speciesName, animationType);
			}
			if (animationParamsType == typeof(FishRunParams))
			{
				return AnimationParams.Create<FishRunParams>(fullPath, speciesName, animationType);
			}
			if (animationParamsType == typeof(FishSwimSlowParams))
			{
				return AnimationParams.Create<FishSwimSlowParams>(fullPath, speciesName, animationType);
			}
			if (animationParamsType == typeof(FishSwimFastParams))
			{
				return AnimationParams.Create<FishSwimFastParams>(fullPath, speciesName, animationType);
			}
			throw new NotImplementedException(animationParamsType.ToString());
		}

		// Token: 0x060032D8 RID: 13016 RVA: 0x0020A9A4 File Offset: 0x00208BA4
		public static T Create<T>(string fullPath, Identifier speciesName, AnimationType animationType) where T : AnimationParams, new()
		{
			if (animationType == AnimationType.NotDefined)
			{
				throw new Exception("Cannot create an animation file of type " + animationType.ToString());
			}
			Dictionary<string, AnimationParams> anims;
			if (!AnimationParams.allAnimations.TryGetValue(speciesName, out anims))
			{
				anims = new Dictionary<string, AnimationParams>();
				AnimationParams.allAnimations.Add(speciesName, anims);
			}
			string fileName = Barotrauma.IO.Path.GetFileNameWithoutExtension(fullPath);
			if (anims.ContainsKey(fileName))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[AnimationParams] Removing the old animation of type ");
				defaultInterpolatedStringHandler.AppendFormatted<AnimationType>(animationType);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Red), false);
				anims.Remove(fileName);
			}
			T instance = Activator.CreateInstance<T>();
			XElement animationElement = new XElement(AnimationParams.GetDefaultFileName(speciesName, animationType), new XAttribute("animationtype", animationType.ToString()));
			instance.doc = new XDocument(new object[]
			{
				animationElement
			});
			CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(speciesName);
			ContentPath contentPath = ContentPath.FromRaw(characterPrefab.ContentPackage, fullPath);
			instance.UpdatePath(contentPath);
			instance.IsLoaded = instance.Deserialize(animationElement);
			instance.Save(null, null);
			instance.Load(contentPath, speciesName);
			anims.Add(fileName, instance);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(54, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("[AnimationParams] New animation file of type ");
			defaultInterpolatedStringHandler2.AppendFormatted<AnimationType>(animationType);
			defaultInterpolatedStringHandler2.AppendLiteral(" created.");
			DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.GhostWhite), false);
			return instance;
		}

		// Token: 0x060032D9 RID: 13017 RVA: 0x0020AB40 File Offset: 0x00208D40
		public bool Serialize()
		{
			return base.Serialize(null);
		}

		// Token: 0x060032DA RID: 13018 RVA: 0x0020AB49 File Offset: 0x00208D49
		public bool Deserialize()
		{
			return base.Deserialize(null);
		}

		// Token: 0x060032DB RID: 13019 RVA: 0x0020AB52 File Offset: 0x00208D52
		protected bool Load(ContentPath file, Identifier speciesName)
		{
			if (this.Load(file))
			{
				this.SpeciesName = speciesName;
				return true;
			}
			return false;
		}

		// Token: 0x060032DC RID: 13020 RVA: 0x0020AB68 File Offset: 0x00208D68
		protected override void UpdatePath(ContentPath newPath)
		{
			Identifier speciesName = this.SpeciesName;
			if (speciesName == null)
			{
				base.UpdatePath(newPath);
				return;
			}
			string fileName = base.FileNameWithoutExtension;
			Dictionary<string, AnimationParams> animations;
			if (AnimationParams.allAnimations.TryGetValue(this.SpeciesName, out animations))
			{
				animations.Remove(fileName);
			}
			base.UpdatePath(newPath);
			if (animations != null && !animations.ContainsKey(fileName))
			{
				animations.Add(fileName, this);
			}
		}

		// Token: 0x060032DD RID: 13021 RVA: 0x0020ABCC File Offset: 0x00208DCC
		protected static string ParseFootAngles(Dictionary<int, float> footAngles)
		{
			return string.Join(",", (from kv in footAngles
			select kv.Key.ToString() + ": " + kv.Value.ToString("G", CultureInfo.InvariantCulture)).ToArray<string>());
		}

		// Token: 0x060032DE RID: 13022 RVA: 0x0020AC04 File Offset: 0x00208E04
		protected static void SetFootAngles(Dictionary<int, float> footAngles, string value)
		{
			footAngles.Clear();
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			string[] keyValuePairs = value.Split(',', StringSplitOptions.None);
			foreach (string joinedKvp in keyValuePairs)
			{
				string[] keyValuePair = joinedKvp.Split(':', StringSplitOptions.None);
				int limbIndex;
				float angle;
				if (keyValuePair.Length != 2 || !int.TryParse(keyValuePair[0].Trim(), out limbIndex) || !float.TryParse(keyValuePair[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out angle))
				{
					DebugConsole.ThrowError("Failed to parse foot angles (" + value + ")", null, null, false, false);
				}
				else
				{
					footAngles[limbIndex] = angle;
				}
			}
		}

		// Token: 0x060032DF RID: 13023 RVA: 0x0020ACA4 File Offset: 0x00208EA4
		public static Type GetParamTypeFromAnimType(AnimationType type, bool isHumanoid)
		{
			Type typeFromHandle;
			if (isHumanoid)
			{
				switch (type)
				{
				case AnimationType.Walk:
					typeFromHandle = typeof(HumanWalkParams);
					break;
				case AnimationType.Run:
					typeFromHandle = typeof(HumanRunParams);
					break;
				case AnimationType.SwimSlow:
					typeFromHandle = typeof(HumanSwimSlowParams);
					break;
				case AnimationType.SwimFast:
					typeFromHandle = typeof(HumanSwimFastParams);
					break;
				case AnimationType.Crouch:
					typeFromHandle = typeof(HumanCrouchParams);
					break;
				default:
					throw new NotImplementedException(type.ToString());
				}
				return typeFromHandle;
			}
			switch (type)
			{
			case AnimationType.Walk:
				typeFromHandle = typeof(FishWalkParams);
				break;
			case AnimationType.Run:
				typeFromHandle = typeof(FishRunParams);
				break;
			case AnimationType.SwimSlow:
				typeFromHandle = typeof(FishSwimSlowParams);
				break;
			case AnimationType.SwimFast:
				typeFromHandle = typeof(FishSwimFastParams);
				break;
			default:
				throw new NotImplementedException(type.ToString());
			}
			return typeFromHandle;
		}

		// Token: 0x17000D71 RID: 3441
		// (get) Token: 0x060032E0 RID: 13024 RVA: 0x0020AD8A File Offset: 0x00208F8A
		// (set) Token: 0x060032E1 RID: 13025 RVA: 0x0020AD92 File Offset: 0x00208F92
		public Memento<AnimationParams> Memento { get; protected set; } = new Memento<AnimationParams>();

		// Token: 0x060032E2 RID: 13026
		public abstract void StoreSnapshot();

		// Token: 0x060032E3 RID: 13027 RVA: 0x0020AD9C File Offset: 0x00208F9C
		protected void StoreSnapshot<T>() where T : AnimationParams, new()
		{
			if (this.doc == null)
			{
				DebugConsole.ThrowError("[AnimationParams] The source XML Document is null!", null, base.Path.ContentPackage, false, false);
				return;
			}
			this.Serialize();
			T t = Activator.CreateInstance<T>();
			t.IsLoaded = true;
			t.doc = new XDocument(this.doc);
			t.Path = base.Path;
			T copy = t;
			copy.Deserialize();
			copy.Serialize();
			this.Memento.Store(copy);
		}

		// Token: 0x060032E4 RID: 13028 RVA: 0x0020AE33 File Offset: 0x00209033
		public void Undo()
		{
			this.Deserialize(this.Memento.Undo().MainElement);
		}

		// Token: 0x060032E5 RID: 13029 RVA: 0x0020AE51 File Offset: 0x00209051
		public void Redo()
		{
			this.Deserialize(this.Memento.Redo().MainElement);
		}

		// Token: 0x060032E6 RID: 13030 RVA: 0x0020AE6F File Offset: 0x0020906F
		public void ClearHistory()
		{
			this.Memento.Clear();
		}

		// Token: 0x060032E9 RID: 13033 RVA: 0x0020AEBC File Offset: 0x002090BC
		[CompilerGenerated]
		internal static bool <FilterAndSortFiles>g__AnimationPredicate|59_2(string filePath, AnimationType type)
		{
			XDocument doc = XMLExtensions.TryLoadXml(filePath);
			return doc != null && doc.GetRootExcludingOverride().GetAttributeEnum("animationtype", AnimationType.NotDefined) == type;
		}

		// Token: 0x060032EA RID: 13034 RVA: 0x0020AEE9 File Offset: 0x002090E9
		[CompilerGenerated]
		internal static bool <GetAnimParams>g__PathMatchesFile|63_0<T>(string p, string f) where T : AnimationParams, new()
		{
			return Barotrauma.IO.Path.GetFileNameWithoutExtension(p).Equals(f, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x04001AB9 RID: 6841
		private static readonly Dictionary<Identifier, Dictionary<string, AnimationParams>> allAnimations = new Dictionary<Identifier, Dictionary<string, AnimationParams>>();

		// Token: 0x04001AC3 RID: 6851
		private static readonly List<string> errorMessages = new List<string>();
	}
}
