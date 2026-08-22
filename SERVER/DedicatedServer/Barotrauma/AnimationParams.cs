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
	// Token: 0x020000DE RID: 222
	internal abstract class AnimationParams : EditableParams, IMemorizable<AnimationParams>
	{
		// Token: 0x170006FB RID: 1787
		// (get) Token: 0x060017B6 RID: 6070 RVA: 0x000C37F6 File Offset: 0x000C19F6
		// (set) Token: 0x060017B7 RID: 6071 RVA: 0x000C37FE File Offset: 0x000C19FE
		public Identifier SpeciesName { get; private set; }

		// Token: 0x170006FC RID: 1788
		// (get) Token: 0x060017B8 RID: 6072 RVA: 0x000C3808 File Offset: 0x000C1A08
		public bool IsGroundedAnimation
		{
			get
			{
				AnimationType animationType = this.AnimationType;
				return animationType - AnimationType.Walk <= 1 || animationType == AnimationType.Crouch;
			}
		}

		// Token: 0x170006FD RID: 1789
		// (get) Token: 0x060017B9 RID: 6073 RVA: 0x000C3830 File Offset: 0x000C1A30
		public bool IsSwimAnimation
		{
			get
			{
				AnimationType animationType = this.AnimationType;
				return animationType - AnimationType.SwimSlow <= 1;
			}
		}

		// Token: 0x170006FE RID: 1790
		// (get) Token: 0x060017BA RID: 6074 RVA: 0x000C3851 File Offset: 0x000C1A51
		// (set) Token: 0x060017BB RID: 6075 RVA: 0x000C3859 File Offset: 0x000C1A59
		[Header("General", null)]
		[Serialize(AnimationType.NotDefined, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public virtual AnimationType AnimationType { get; protected set; }

		// Token: 0x170006FF RID: 1791
		// (get) Token: 0x060017BC RID: 6076 RVA: 0x000C3862 File Offset: 0x000C1A62
		// (set) Token: 0x060017BD RID: 6077 RVA: 0x000C386A File Offset: 0x000C1A6A
		[Header("Movement", null)]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(DecimalCount = 2, MinValueFloat = 0f, MaxValueFloat = 20f, ValueStep = 0.1f)]
		public float MovementSpeed { get; set; }

		// Token: 0x17000700 RID: 1792
		// (get) Token: 0x060017BE RID: 6078 RVA: 0x000C3873 File Offset: 0x000C1A73
		// (set) Token: 0x060017BF RID: 6079 RVA: 0x000C387B File Offset: 0x000C1A7B
		[Serialize(1f, IsPropertySaveable.Yes, "The speed of the \"animation cycle\", i.e. how fast the character takes steps or moves the tail/legs/arms (the outcome depends what the clip is about)", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2, ValueStep = 0.01f)]
		public float CycleSpeed { get; set; }

		// Token: 0x17000701 RID: 1793
		// (get) Token: 0x060017C0 RID: 6080 RVA: 0x000C3884 File Offset: 0x000C1A84
		// (set) Token: 0x060017C1 RID: 6081 RVA: 0x000C38A4 File Offset: 0x000C1AA4
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

		// Token: 0x17000702 RID: 1794
		// (get) Token: 0x060017C2 RID: 6082 RVA: 0x000C38BA File Offset: 0x000C1ABA
		// (set) Token: 0x060017C3 RID: 6083 RVA: 0x000C38C2 File Offset: 0x000C1AC2
		public float HeadAngleInRadians { get; private set; } = float.NaN;

		// Token: 0x17000703 RID: 1795
		// (get) Token: 0x060017C4 RID: 6084 RVA: 0x000C38CB File Offset: 0x000C1ACB
		// (set) Token: 0x060017C5 RID: 6085 RVA: 0x000C38EB File Offset: 0x000C1AEB
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

		// Token: 0x17000704 RID: 1796
		// (get) Token: 0x060017C6 RID: 6086 RVA: 0x000C3901 File Offset: 0x000C1B01
		// (set) Token: 0x060017C7 RID: 6087 RVA: 0x000C3909 File Offset: 0x000C1B09
		public float TorsoAngleInRadians { get; private set; } = float.NaN;

		// Token: 0x17000705 RID: 1797
		// (get) Token: 0x060017C8 RID: 6088 RVA: 0x000C3912 File Offset: 0x000C1B12
		// (set) Token: 0x060017C9 RID: 6089 RVA: 0x000C391A File Offset: 0x000C1B1A
		[Serialize(50f, IsPropertySaveable.Yes, "How much torque is used to rotate the head to the correct orientation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float HeadTorque { get; set; }

		// Token: 0x17000706 RID: 1798
		// (get) Token: 0x060017CA RID: 6090 RVA: 0x000C3923 File Offset: 0x000C1B23
		// (set) Token: 0x060017CB RID: 6091 RVA: 0x000C392B File Offset: 0x000C1B2B
		[Serialize(50f, IsPropertySaveable.Yes, "How much torque is used to rotate the torso to the correct orientation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float TorsoTorque { get; set; }

		// Token: 0x17000707 RID: 1799
		// (get) Token: 0x060017CC RID: 6092 RVA: 0x000C3934 File Offset: 0x000C1B34
		// (set) Token: 0x060017CD RID: 6093 RVA: 0x000C393C File Offset: 0x000C1B3C
		[Header("Legs", null)]
		[Serialize(25f, IsPropertySaveable.Yes, "How much torque is used to rotate the feet to the correct orientation.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float FootTorque { get; set; }

		// Token: 0x17000708 RID: 1800
		// (get) Token: 0x060017CE RID: 6094 RVA: 0x000C3945 File Offset: 0x000C1B45
		// (set) Token: 0x060017CF RID: 6095 RVA: 0x000C394D File Offset: 0x000C1B4D
		[Header("Arms", null)]
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to rotate the arms to the IK position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float ArmIKStrength { get; set; }

		// Token: 0x17000709 RID: 1801
		// (get) Token: 0x060017D0 RID: 6096 RVA: 0x000C3956 File Offset: 0x000C1B56
		// (set) Token: 0x060017D1 RID: 6097 RVA: 0x000C395E File Offset: 0x000C1B5E
		[Serialize(1f, IsPropertySaveable.Yes, "How much force is used to rotate the hands to the IK position.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float HandIKStrength { get; set; }

		// Token: 0x060017D2 RID: 6098 RVA: 0x000C3968 File Offset: 0x000C1B68
		public static string GetDefaultFileName(Identifier speciesName, AnimationType animType)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
			defaultInterpolatedStringHandler.AppendFormatted(speciesName.Value.CapitaliseFirstInvariant());
			defaultInterpolatedStringHandler.AppendFormatted<AnimationType>(animType);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060017D3 RID: 6099 RVA: 0x000C39A0 File Offset: 0x000C1BA0
		public static string GetDefaultFilePath(Identifier speciesName, AnimationType animType)
		{
			return Barotrauma.IO.Path.Combine(new string[]
			{
				AnimationParams.GetFolder(speciesName),
				AnimationParams.GetDefaultFileName(speciesName, animType) + ".xml"
			});
		}

		// Token: 0x060017D4 RID: 6100 RVA: 0x000C39CC File Offset: 0x000C1BCC
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

		// Token: 0x060017D5 RID: 6101 RVA: 0x000C3A5C File Offset: 0x000C1C5C
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

		// Token: 0x060017D6 RID: 6102 RVA: 0x000C3AD4 File Offset: 0x000C1CD4
		public static IEnumerable<string> FilterAndSortFiles(IEnumerable<string> filePaths, AnimationType type)
		{
			return (from f in filePaths
			where AnimationParams.<FilterAndSortFiles>g__AnimationPredicate|59_2(f, type)
			select f).OrderBy((string f) => f, StringComparer.OrdinalIgnoreCase);
		}

		// Token: 0x060017D7 RID: 6103 RVA: 0x000C3B29 File Offset: 0x000C1D29
		protected static T GetDefaultAnimParams<T>(Character character, AnimationType animType) where T : AnimationParams, new()
		{
			return AnimationParams.GetAnimParams<T>(character, animType, null, true);
		}

		// Token: 0x060017D8 RID: 6104 RVA: 0x000C3B34 File Offset: 0x000C1D34
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

		// Token: 0x060017D9 RID: 6105 RVA: 0x000C3BE0 File Offset: 0x000C1DE0
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

		// Token: 0x060017DA RID: 6106 RVA: 0x000C4098 File Offset: 0x000C2298
		public static void ClearCache()
		{
			AnimationParams.allAnimations.Clear();
		}

		// Token: 0x060017DB RID: 6107 RVA: 0x000C40A4 File Offset: 0x000C22A4
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

		// Token: 0x060017DC RID: 6108 RVA: 0x000C41B0 File Offset: 0x000C23B0
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

		// Token: 0x060017DD RID: 6109 RVA: 0x000C434C File Offset: 0x000C254C
		public bool Serialize()
		{
			return base.Serialize(null);
		}

		// Token: 0x060017DE RID: 6110 RVA: 0x000C4355 File Offset: 0x000C2555
		public bool Deserialize()
		{
			return base.Deserialize(null);
		}

		// Token: 0x060017DF RID: 6111 RVA: 0x000C435E File Offset: 0x000C255E
		protected bool Load(ContentPath file, Identifier speciesName)
		{
			if (this.Load(file))
			{
				this.SpeciesName = speciesName;
				return true;
			}
			return false;
		}

		// Token: 0x060017E0 RID: 6112 RVA: 0x000C4374 File Offset: 0x000C2574
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

		// Token: 0x060017E1 RID: 6113 RVA: 0x000C43D8 File Offset: 0x000C25D8
		protected static string ParseFootAngles(Dictionary<int, float> footAngles)
		{
			return string.Join(",", (from kv in footAngles
			select kv.Key.ToString() + ": " + kv.Value.ToString("G", CultureInfo.InvariantCulture)).ToArray<string>());
		}

		// Token: 0x060017E2 RID: 6114 RVA: 0x000C4410 File Offset: 0x000C2610
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

		// Token: 0x060017E3 RID: 6115 RVA: 0x000C44B0 File Offset: 0x000C26B0
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

		// Token: 0x1700070A RID: 1802
		// (get) Token: 0x060017E4 RID: 6116 RVA: 0x000C4596 File Offset: 0x000C2796
		// (set) Token: 0x060017E5 RID: 6117 RVA: 0x000C459E File Offset: 0x000C279E
		public Memento<AnimationParams> Memento { get; protected set; } = new Memento<AnimationParams>();

		// Token: 0x060017E6 RID: 6118
		public abstract void StoreSnapshot();

		// Token: 0x060017E7 RID: 6119 RVA: 0x000C45A8 File Offset: 0x000C27A8
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

		// Token: 0x060017E8 RID: 6120 RVA: 0x000C463F File Offset: 0x000C283F
		public void Undo()
		{
			this.Deserialize(this.Memento.Undo().MainElement);
		}

		// Token: 0x060017E9 RID: 6121 RVA: 0x000C465D File Offset: 0x000C285D
		public void Redo()
		{
			this.Deserialize(this.Memento.Redo().MainElement);
		}

		// Token: 0x060017EA RID: 6122 RVA: 0x000C467B File Offset: 0x000C287B
		public void ClearHistory()
		{
			this.Memento.Clear();
		}

		// Token: 0x060017ED RID: 6125 RVA: 0x000C46C8 File Offset: 0x000C28C8
		[CompilerGenerated]
		internal static bool <FilterAndSortFiles>g__AnimationPredicate|59_2(string filePath, AnimationType type)
		{
			XDocument doc = XMLExtensions.TryLoadXml(filePath);
			return doc != null && doc.GetRootExcludingOverride().GetAttributeEnum("animationtype", AnimationType.NotDefined) == type;
		}

		// Token: 0x060017EE RID: 6126 RVA: 0x000C46F5 File Offset: 0x000C28F5
		[CompilerGenerated]
		internal static bool <GetAnimParams>g__PathMatchesFile|63_0<T>(string p, string f) where T : AnimationParams, new()
		{
			return Barotrauma.IO.Path.GetFileNameWithoutExtension(p).Equals(f, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x04000B94 RID: 2964
		private static readonly Dictionary<Identifier, Dictionary<string, AnimationParams>> allAnimations = new Dictionary<Identifier, Dictionary<string, AnimationParams>>();

		// Token: 0x04000B9E RID: 2974
		private static readonly List<string> errorMessages = new List<string>();
	}
}
