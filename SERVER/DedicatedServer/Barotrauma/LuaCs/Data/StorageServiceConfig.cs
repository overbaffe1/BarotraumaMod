using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200047C RID: 1148
	public class StorageServiceConfig : IStorageServiceConfig, IService, IDisposable, IEquatable<StorageServiceConfig>
	{
		// Token: 0x1700103B RID: 4155
		// (get) Token: 0x06003DA3 RID: 15779 RVA: 0x0018E50C File Offset: 0x0018C70C
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

		// Token: 0x1700103C RID: 4156
		// (get) Token: 0x06003DA4 RID: 15780 RVA: 0x0018E518 File Offset: 0x0018C718
		// (set) Token: 0x06003DA5 RID: 15781 RVA: 0x0018E520 File Offset: 0x0018C720
		public string LocalModsDirectory { get; set; }

		// Token: 0x1700103D RID: 4157
		// (get) Token: 0x06003DA6 RID: 15782 RVA: 0x0018E529 File Offset: 0x0018C729
		// (set) Token: 0x06003DA7 RID: 15783 RVA: 0x0018E531 File Offset: 0x0018C731
		public string WorkshopModsDirectory { get; set; }

		// Token: 0x1700103E RID: 4158
		// (get) Token: 0x06003DA8 RID: 15784 RVA: 0x0018E53A File Offset: 0x0018C73A
		// (set) Token: 0x06003DA9 RID: 15785 RVA: 0x0018E542 File Offset: 0x0018C742
		public string GameSettingsConfigPath { get; set; }

		// Token: 0x1700103F RID: 4159
		// (get) Token: 0x06003DAA RID: 15786 RVA: 0x0018E54B File Offset: 0x0018C74B
		public string LocalDataSavePath
		{
			get
			{
				return Path.Combine(StorageServiceConfig.ExecutionLocation, "Data/Mods").CleanUpPathCrossPlatform(true, "");
			}
		}

		// Token: 0x17001040 RID: 4160
		// (get) Token: 0x06003DAB RID: 15787 RVA: 0x0018E567 File Offset: 0x0018C767
		public string LocalDataPathRegex
		{
			get
			{
				return "%ModDir%";
			}
		}

		// Token: 0x17001041 RID: 4161
		// (get) Token: 0x06003DAC RID: 15788 RVA: 0x0018E56E File Offset: 0x0018C76E
		public string RunLocation
		{
			get
			{
				return StorageServiceConfig.ExecutionLocation;
			}
		}

		// Token: 0x17001042 RID: 4162
		// (get) Token: 0x06003DAD RID: 15789 RVA: 0x0018E575 File Offset: 0x0018C775
		public string LocalPackageDataPath
		{
			get
			{
				return Path.Combine(this.LocalDataSavePath, this.LocalDataPathRegex);
			}
		}

		// Token: 0x06003DAE RID: 15790 RVA: 0x0018E588 File Offset: 0x0018C788
		public void Dispose()
		{
		}

		// Token: 0x17001043 RID: 4163
		// (get) Token: 0x06003DAF RID: 15791 RVA: 0x0018E58A File Offset: 0x0018C78A
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06003DB0 RID: 15792 RVA: 0x0018E590 File Offset: 0x0018C790
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

		// Token: 0x06003DB1 RID: 15793 RVA: 0x0018E5DC File Offset: 0x0018C7DC
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

		// Token: 0x06003DB2 RID: 15794 RVA: 0x0018E6C5 File Offset: 0x0018C8C5
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(StorageServiceConfig left, StorageServiceConfig right)
		{
			return !(left == right);
		}

		// Token: 0x06003DB3 RID: 15795 RVA: 0x0018E6D1 File Offset: 0x0018C8D1
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(StorageServiceConfig left, StorageServiceConfig right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003DB4 RID: 15796 RVA: 0x0018E6E8 File Offset: 0x0018C8E8
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<LocalModsDirectory>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<WorkshopModsDirectory>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<GameSettingsConfigPath>k__BackingField);
		}

		// Token: 0x06003DB5 RID: 15797 RVA: 0x0018E74A File Offset: 0x0018C94A
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as StorageServiceConfig);
		}

		// Token: 0x06003DB6 RID: 15798 RVA: 0x0018E758 File Offset: 0x0018C958
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(StorageServiceConfig other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(this.<LocalModsDirectory>k__BackingField, other.<LocalModsDirectory>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<WorkshopModsDirectory>k__BackingField, other.<WorkshopModsDirectory>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<GameSettingsConfigPath>k__BackingField, other.<GameSettingsConfigPath>k__BackingField));
		}

		// Token: 0x06003DB8 RID: 15800 RVA: 0x0018E7D1 File Offset: 0x0018C9D1
		[CompilerGenerated]
		protected StorageServiceConfig([Nullable(1)] StorageServiceConfig original)
		{
			this.LocalModsDirectory = original.<LocalModsDirectory>k__BackingField;
			this.WorkshopModsDirectory = original.<WorkshopModsDirectory>k__BackingField;
			this.GameSettingsConfigPath = original.<GameSettingsConfigPath>k__BackingField;
		}

		// Token: 0x06003DB9 RID: 15801 RVA: 0x0018E800 File Offset: 0x0018CA00
		public StorageServiceConfig()
		{
			this.LocalModsDirectory = Path.GetFullPath("LocalMods").CleanUpPath();
			this.WorkshopModsDirectory = Path.GetFullPath(ContentPackage.WorkshopModsDir).CleanUpPath();
			this.GameSettingsConfigPath = Path.GetFullPath(string.IsNullOrEmpty(GameSettings.CurrentConfig.SavePath) ? SaveUtil.DefaultSaveFolder : GameSettings.CurrentConfig.SavePath).CleanUpPath();
			base..ctor();
		}

		// Token: 0x04001DB2 RID: 7602
		private static readonly string ExecutionLocation = Directory.GetCurrentDirectory().CleanUpPathCrossPlatform(true, "");
	}
}
