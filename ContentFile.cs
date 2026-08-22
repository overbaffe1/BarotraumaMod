using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x02000224 RID: 548
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class ContentFile
	{
		// Token: 0x060036AD RID: 13997 RVA: 0x002135DC File Offset: 0x002117DC
		public static bool IsLegacyContentType(XElement contentFileElement, ContentPackage package, bool logWarning)
		{
			Identifier elemName = contentFileElement.NameAsIdentifier();
			if (elemName == "TraitorMissions")
			{
				if (logWarning)
				{
					DebugConsole.AddWarning("The content type \"TraitorMission\" in content package \"" + package.Name + "\" is no longer supported. Traitor missions should be implemented using the scripted event system and the content type TraitorEvents.", package);
				}
				return true;
			}
			return false;
		}

		// Token: 0x060036AE RID: 13998 RVA: 0x00213620 File Offset: 0x00211820
		public static Result<ContentFile, ContentPackage.LoadError> CreateFromXElement(ContentPackage contentPackage, XElement element)
		{
			Identifier elemName = element.NameAsIdentifier();
			ContentFile.TypeInfo type = ContentFile.Types.FirstOrDefault((ContentFile.TypeInfo t) => t.Names.Contains(elemName));
			ContentPath filePath = element.GetAttributeContentPath("file", contentPackage);
			if (type == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Invalid content type \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(elemName);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				return ContentFile.<CreateFromXElement>g__fail|4_0(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			}
			if (filePath == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(43, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("No content path defined for file of type \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(elemName);
				defaultInterpolatedStringHandler2.AppendLiteral("\"");
				return ContentFile.<CreateFromXElement>g__fail|4_0(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
			}
			Result<ContentFile, ContentPackage.LoadError> result;
			using (DebugConsole.ErrorCatcher errorCatcher = DebugConsole.ErrorCatcher.Create())
			{
				try
				{
					filePath = type.MutateContentPath(filePath);
					if (!File.Exists(filePath.FullPath))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(50, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Failed to load file \"");
						defaultInterpolatedStringHandler3.AppendFormatted<ContentPath>(filePath);
						defaultInterpolatedStringHandler3.AppendLiteral("\" of type \"");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(elemName);
						defaultInterpolatedStringHandler3.AppendLiteral("\": file not found.");
						result = ContentFile.<CreateFromXElement>g__fail|4_0(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
					}
					else
					{
						ContentFile file = type.CreateInstance(contentPackage, filePath);
						if (file == null)
						{
							result = ContentFile.<CreateFromXElement>g__fail|4_0("Content type " + type.Type.Name + " is not implemented correctly", null);
						}
						else if (errorCatcher.Errors.Any<ColoredText>())
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(66, 2);
							defaultInterpolatedStringHandler4.AppendLiteral("Errors were issued to the debug console when loading \"");
							defaultInterpolatedStringHandler4.AppendFormatted<ContentPath>(filePath);
							defaultInterpolatedStringHandler4.AppendLiteral("\" of type \"");
							defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(elemName);
							defaultInterpolatedStringHandler4.AppendLiteral("\"");
							result = ContentFile.<CreateFromXElement>g__fail|4_0(defaultInterpolatedStringHandler4.ToStringAndClear(), null);
						}
						else
						{
							result = Result<ContentFile, ContentPackage.LoadError>.Success(file);
						}
					}
				}
				catch (Exception e)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(35, 3);
					defaultInterpolatedStringHandler5.AppendLiteral("Failed to load file \"");
					defaultInterpolatedStringHandler5.AppendFormatted<ContentPath>(filePath);
					defaultInterpolatedStringHandler5.AppendLiteral("\" of type \"");
					defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(elemName);
					defaultInterpolatedStringHandler5.AppendLiteral("\": ");
					defaultInterpolatedStringHandler5.AppendFormatted(e.Message);
					result = ContentFile.<CreateFromXElement>g__fail|4_0(defaultInterpolatedStringHandler5.ToStringAndClear(), e);
				}
			}
			return result;
		}

		// Token: 0x060036AF RID: 13999 RVA: 0x002138A0 File Offset: 0x00211AA0
		protected ContentFile(ContentPackage contentPackage, ContentPath path)
		{
			this.ContentPackage = contentPackage;
			this.Path = path;
			this.Hash = this.CalculateHash();
		}

		// Token: 0x060036B0 RID: 14000
		public abstract void LoadFile();

		// Token: 0x060036B1 RID: 14001
		public abstract void UnloadFile();

		// Token: 0x060036B2 RID: 14002
		public abstract void Sort();

		// Token: 0x060036B3 RID: 14003 RVA: 0x002138C2 File Offset: 0x00211AC2
		public virtual void Preload(Action<Sprite> addPreloadedSprite)
		{
		}

		// Token: 0x060036B4 RID: 14004 RVA: 0x002138C4 File Offset: 0x00211AC4
		public virtual Md5Hash CalculateHash()
		{
			return Md5Hash.CalculateForFile(this.Path.Value, Md5Hash.StringHashOptions.IgnoreWhitespace);
		}

		// Token: 0x17000E83 RID: 3715
		// (get) Token: 0x060036B5 RID: 14005 RVA: 0x002138D7 File Offset: 0x00211AD7
		public bool NotSyncedInMultiplayer
		{
			get
			{
				return ContentFile.Types.Any((ContentFile.TypeInfo t) => t.Type == base.GetType() && t.NotSyncedInMultiplayer);
			}
		}

		// Token: 0x060036B6 RID: 14006 RVA: 0x002138EF File Offset: 0x00211AEF
		[CompilerGenerated]
		internal static Result<ContentFile, ContentPackage.LoadError> <CreateFromXElement>g__fail|4_0(string error, [Nullable(2)] Exception exception = null)
		{
			return Result<ContentFile, ContentPackage.LoadError>.Failure(new ContentPackage.LoadError(error, exception));
		}

		// Token: 0x04001C0A RID: 7178
		public static readonly ImmutableHashSet<ContentFile.TypeInfo> Types = (from t in ReflectionUtils.GetDerivedNonAbstract<ContentFile>()
		select new ContentFile.TypeInfo(t)).ToImmutableHashSet<ContentFile.TypeInfo>();

		// Token: 0x04001C0B RID: 7179
		public readonly ContentPackage ContentPackage;

		// Token: 0x04001C0C RID: 7180
		public readonly ContentPath Path;

		// Token: 0x04001C0D RID: 7181
		public readonly Md5Hash Hash;

		// Token: 0x02000EEB RID: 3819
		[Nullable(0)]
		public class TypeInfo
		{
			// Token: 0x060087A3 RID: 34723 RVA: 0x003A444C File Offset: 0x003A264C
			public TypeInfo(Type type)
			{
				this.Type = type;
				RequiredByCorePackage reqByCoreAttribute = type.GetCustomAttribute<RequiredByCorePackage>();
				this.RequiredByCorePackage = (reqByCoreAttribute != null);
				NotSyncedInMultiplayer notSyncedInMultiplayerAttribute = type.GetCustomAttribute<NotSyncedInMultiplayer>();
				this.NotSyncedInMultiplayer = (notSyncedInMultiplayerAttribute != null);
				this.AlternativeTypes = ((reqByCoreAttribute != null) ? reqByCoreAttribute.AlternativeTypes : null);
				this.contentPathMutator = this.Type.GetMethod("MutateContentPath", BindingFlags.Static | BindingFlags.Public);
				HashSet<Identifier> names = new HashSet<Identifier>
				{
					type.Name.RemoveFromEnd("File", StringComparison.Ordinal).ToIdentifier()
				};
				AlternativeContentTypeNames customAttribute = type.GetCustomAttribute(false);
				ImmutableHashSet<Identifier> altNames = (customAttribute != null) ? customAttribute.Names : null;
				if (altNames != null)
				{
					names.UnionWith(altNames);
				}
				this.Names = names.ToImmutableHashSet<Identifier>();
			}

			// Token: 0x060087A4 RID: 34724 RVA: 0x003A44FF File Offset: 0x003A26FF
			public ContentPath MutateContentPath(ContentPath path)
			{
				MethodInfo methodInfo = this.contentPathMutator;
				return ((ContentPath)((methodInfo != null) ? methodInfo.Invoke(null, new object[]
				{
					path
				}) : null)) ?? path;
			}

			// Token: 0x060087A5 RID: 34725 RVA: 0x003A4528 File Offset: 0x003A2728
			[return: Nullable(2)]
			public ContentFile CreateInstance(ContentPackage contentPackage, ContentPath path)
			{
				return (ContentFile)Activator.CreateInstance(this.Type, new object[]
				{
					contentPackage,
					path
				});
			}

			// Token: 0x04005438 RID: 21560
			public readonly Type Type;

			// Token: 0x04005439 RID: 21561
			public readonly bool RequiredByCorePackage;

			// Token: 0x0400543A RID: 21562
			public readonly bool NotSyncedInMultiplayer;

			// Token: 0x0400543B RID: 21563
			[Nullable(new byte[]
			{
				2,
				1
			})]
			public readonly ImmutableHashSet<Type> AlternativeTypes;

			// Token: 0x0400543C RID: 21564
			public readonly ImmutableHashSet<Identifier> Names;

			// Token: 0x0400543D RID: 21565
			[Nullable(2)]
			private readonly MethodInfo contentPathMutator;
		}
	}
}
