using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000598 RID: 1432
	public class StorageServiceConfig : IStorageServiceConfig, IService, IDisposable, IEquatable<StorageServiceConfig>
	{
		// Token: 0x170015A0 RID: 5536
		// (get) Token: 0x0600570A RID: 22282 RVA: 0x002D34B8 File Offset: 0x002D16B8
		[Nullable(1)]
		[CompilerGenerated]
		protected virtual Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(StorageServiceConfig);
			}
		}

		// Token: 0x170015A1 RID: 5537
		// (get) Token: 0x0600570B RID: 22283 RVA: 0x002D34C4 File Offset: 0x002D16C4
		// (set) Token: 0x0600570C RID: 22284 RVA: 0x002D34CC File Offset: 0x002D16CC
		public string LocalModsDirectory { get; set; }

		// Token: 0x170015A2 RID: 5538
		// (get) Token: 0x0600570D RID: 22285 RVA: 0x002D34D5 File Offset: 0x002D16D5
		// (set) Token: 0x0600570E RID: 22286 RVA: 0x002D34DD File Offset: 0x002D16DD
		public string WorkshopModsDirectory { get; set; }

		// Token: 0x170015A3 RID: 5539
		// (get) Token: 0x0600570F RID: 22287 RVA: 0x002D34E6 File Offset: 0x002D16E6
		// (set) Token: 0x06005710 RID: 22288 RVA: 0x002D34EE File Offset: 0x002D16EE
		public string GameSettingsConfigPath { get; set; }

		// Token: 0x170015A4 RID: 5540
		// (get) Token: 0x06005711 RID: 22289 RVA: 0x002D34F7 File Offset: 0x002D16F7
		// (set) Token: 0x06005712 RID: 22290 RVA: 0x002D34FF File Offset: 0x002D16FF
		public string TempDownloadsDirectory { get; set; }

		// Token: 0x170015A5 RID: 5541
		// (get) Token: 0x06005713 RID: 22291 RVA: 0x002D3508 File Offset: 0x002D1708
		public string LocalDataSavePath
		{
			get
			{
				return Path.Combine(StorageServiceConfig.ExecutionLocation, "Data/Mods").CleanUpPathCrossPlatform(true, "");
			}
		}

		// Token: 0x170015A6 RID: 5542
		// (get) Token: 0x06005714 RID: 22292 RVA: 0x002D3524 File Offset: 0x002D1724
		public string LocalDataPathRegex
		{
			get
			{
				return "%ModDir%";
			}
		}

		// Token: 0x170015A7 RID: 5543
		// (get) Token: 0x06005715 RID: 22293 RVA: 0x002D352B File Offset: 0x002D172B
		public string RunLocation
		{
			get
			{
				return StorageServiceConfig.ExecutionLocation;
			}
		}

		// Token: 0x170015A8 RID: 5544
		// (get) Token: 0x06005716 RID: 22294 RVA: 0x002D3532 File Offset: 0x002D1732
		public string LocalPackageDataPath
		{
			get
			{
				return Path.Combine(this.LocalDataSavePath, this.LocalDataPathRegex);
			}
		}

		// Token: 0x06005717 RID: 22295 RVA: 0x002D3545 File Offset: 0x002D1745
		public void Dispose()
		{
		}

		// Token: 0x170015A9 RID: 5545
		// (get) Token: 0x06005718 RID: 22296 RVA: 0x002D3547 File Offset: 0x002D1747
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06005719 RID: 22297 RVA: 0x002D354C File Offset: 0x002D174C
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("StorageServiceConfig");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600571A RID: 22298 RVA: 0x002D3598 File Offset: 0x002D1798
		[NullableContext(1)]
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("LocalModsDirectory = ");
			builder.Append(this.LocalModsDirectory);
			builder.Append(", WorkshopModsDirectory = ");
			builder.Append(this.WorkshopModsDirectory);
			builder.Append(", GameSettingsConfigPath = ");
			builder.Append(this.GameSettingsConfigPath);
			builder.Append(", TempDownloadsDirectory = ");
			builder.Append(this.TempDownloadsDirectory);
			builder.Append(", LocalDataSavePath = ");
			builder.Append(this.LocalDataSavePath);
			builder.Append(", LocalDataPathRegex = ");
			builder.Append(this.LocalDataPathRegex);
			builder.Append(", RunLocation = ");
			builder.Append(this.RunLocation);
			builder.Append(", LocalPackageDataPath = ");
			builder.Append(this.LocalPackageDataPath);
			builder.Append(", IsDisposed = ");
			builder.Append(this.IsDisposed.ToString());
			return true;
		}

		// Token: 0x0600571B RID: 22299 RVA: 0x002D369A File Offset: 0x002D189A
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(StorageServiceConfig left, StorageServiceConfig right)
		{
			return !(left == right);
		}

		// Token: 0x0600571C RID: 22300 RVA: 0x002D36A6 File Offset: 0x002D18A6
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(StorageServiceConfig left, StorageServiceConfig right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x0600571D RID: 22301 RVA: 0x002D36BC File Offset: 0x002D18BC
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<LocalModsDirectory>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<WorkshopModsDirectory>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<GameSettingsConfigPath>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<TempDownloadsDirectory>k__BackingField);
		}

		// Token: 0x0600571E RID: 22302 RVA: 0x002D3735 File Offset: 0x002D1935
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as StorageServiceConfig);
		}

		// Token: 0x0600571F RID: 22303 RVA: 0x002D3744 File Offset: 0x002D1944
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(StorageServiceConfig other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(this.<LocalModsDirectory>k__BackingField, other.<LocalModsDirectory>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<WorkshopModsDirectory>k__BackingField, other.<WorkshopModsDirectory>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<GameSettingsConfigPath>k__BackingField, other.<GameSettingsConfigPath>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<TempDownloadsDirectory>k__BackingField, other.<TempDownloadsDirectory>k__BackingField));
		}

		// Token: 0x06005721 RID: 22305 RVA: 0x002D37D5 File Offset: 0x002D19D5
		[CompilerGenerated]
		protected StorageServiceConfig([Nullable(1)] StorageServiceConfig original)
		{
			this.LocalModsDirectory = original.<LocalModsDirectory>k__BackingField;
			this.WorkshopModsDirectory = original.<WorkshopModsDirectory>k__BackingField;
			this.GameSettingsConfigPath = original.<GameSettingsConfigPath>k__BackingField;
			this.TempDownloadsDirectory = original.<TempDownloadsDirectory>k__BackingField;
		}

		// Token: 0x06005722 RID: 22306 RVA: 0x002D3810 File Offset: 0x002D1A10
		public StorageServiceConfig()
		{
			this.LocalModsDirectory = Path.GetFullPath("LocalMods").CleanUpPath();
			this.WorkshopModsDirectory = Path.GetFullPath(ContentPackage.WorkshopModsDir).CleanUpPath();
			this.GameSettingsConfigPath = Path.GetFullPath(string.IsNullOrEmpty(GameSettings.CurrentConfig.SavePath) ? SaveUtil.DefaultSaveFolder : GameSettings.CurrentConfig.SavePath).CleanUpPath();
			this.TempDownloadsDirectory = Path.GetFullPath("TempMods_Download").CleanUpPath();
			base..ctor();
		}

		// Token: 0x04002CA2 RID: 11426
		private static readonly string ExecutionLocation = Directory.GetCurrentDirectory().CleanUpPathCrossPlatform(true, "");
	}
}
