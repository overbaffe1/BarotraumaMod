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
	// Token: 0x0200012E RID: 302
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class ContentFile
	{
		// Token: 0x06001BCE RID: 7118 RVA: 0x000CDB44 File Offset: 0x000CBD44
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

		// Token: 0x06001BCF RID: 7119 RVA: 0x000CDB88 File Offset: 0x000CBD88
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

		// Token: 0x06001BD0 RID: 7120 RVA: 0x000CDE08 File Offset: 0x000CC008
		protected ContentFile(ContentPackage contentPackage, ContentPath path)
		{
			this.ContentPackage = contentPackage;
			this.Path = path;
			this.Hash = this.CalculateHash();
		}

		// Token: 0x06001BD1 RID: 7121
		public abstract void LoadFile();

		// Token: 0x06001BD2 RID: 7122
		public abstract void UnloadFile();

		// Token: 0x06001BD3 RID: 7123
		public abstract void Sort();

		// Token: 0x06001BD4 RID: 7124 RVA: 0x000CDE2A File Offset: 0x000CC02A
		public virtual void Preload(Action<Sprite> addPreloadedSprite)
		{
		}

		// Token: 0x06001BD5 RID: 7125 RVA: 0x000CDE2C File Offset: 0x000CC02C
		public virtual Md5Hash CalculateHash()
		{
			return Md5Hash.CalculateForFile(this.Path.Value, Md5Hash.StringHashOptions.IgnoreWhitespace);
		}

		// Token: 0x17000822 RID: 2082
		// (get) Token: 0x06001BD6 RID: 7126 RVA: 0x000CDE3F File Offset: 0x000CC03F
		public bool NotSyncedInMultiplayer
		{
			get
			{
				return ContentFile.Types.Any((ContentFile.TypeInfo t) => t.Type == base.GetType() && t.NotSyncedInMultiplayer);
			}
		}

		// Token: 0x06001BD7 RID: 7127 RVA: 0x000CDE57 File Offset: 0x000CC057
		[CompilerGenerated]
		internal static Result<ContentFile, ContentPackage.LoadError> <CreateFromXElement>g__fail|4_0(string error, [Nullable(2)] Exception exception = null)
		{
			return Result<ContentFile, ContentPackage.LoadError>.Failure(new ContentPackage.LoadError(error, exception));
		}

		// Token: 0x04000CFD RID: 3325
		public static readonly ImmutableHashSet<ContentFile.TypeInfo> Types = (from t in ReflectionUtils.GetDerivedNonAbstract<ContentFile>()
		select new ContentFile.TypeInfo(t)).ToImmutableHashSet<ContentFile.TypeInfo>();

		// Token: 0x04000CFE RID: 3326
		public readonly ContentPackage ContentPackage;

		// Token: 0x04000CFF RID: 3327
		public readonly ContentPath Path;

		// Token: 0x04000D00 RID: 3328
		public readonly Md5Hash Hash;

		// Token: 0x020008D2 RID: 2258
		[Nullable(0)]
		public class TypeInfo
		{
			// Token: 0x060057C6 RID: 22470 RVA: 0x001F5410 File Offset: 0x001F3610
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

			// Token: 0x060057C7 RID: 22471 RVA: 0x001F54C3 File Offset: 0x001F36C3
			public ContentPath MutateContentPath(ContentPath path)
			{
				MethodInfo methodInfo = this.contentPathMutator;
				return ((ContentPath)((methodInfo != null) ? methodInfo.Invoke(null, new object[]
				{
					path
				}) : null)) ?? path;
			}

			// Token: 0x060057C8 RID: 22472 RVA: 0x001F54EC File Offset: 0x001F36EC
			[return: Nullable(2)]
			public ContentFile CreateInstance(ContentPackage contentPackage, ContentPath path)
			{
				return (ContentFile)Activator.CreateInstance(this.Type, new object[]
				{
					contentPackage,
					path
				});
			}

			// Token: 0x04003158 RID: 12632
			public readonly Type Type;

			// Token: 0x04003159 RID: 12633
			public readonly bool RequiredByCorePackage;

			// Token: 0x0400315A RID: 12634
			public readonly bool NotSyncedInMultiplayer;

			// Token: 0x0400315B RID: 12635
			[Nullable(new byte[]
			{
				2,
				1
			})]
			public readonly ImmutableHashSet<Type> AlternativeTypes;

			// Token: 0x0400315C RID: 12636
			public readonly ImmutableHashSet<Identifier> Names;

			// Token: 0x0400315D RID: 12637
			[Nullable(2)]
			private readonly MethodInfo contentPathMutator;
		}
	}
}
