using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003C4 RID: 964
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class ServerContentPackage : INetSerializableStruct
	{
		// Token: 0x17000F52 RID: 3922
		// (get) Token: 0x060037DC RID: 14300 RVA: 0x00176AC8 File Offset: 0x00174CC8
		// (set) Token: 0x060037DD RID: 14301 RVA: 0x00176AF3 File Offset: 0x00174CF3
		public Md5Hash Hash
		{
			get
			{
				Md5Hash result;
				if ((result = this.cachedHash) == null)
				{
					result = (this.cachedHash = Md5Hash.BytesAsHash(this.HashBytes));
				}
				return result;
			}
			set
			{
				this.cachedHash = value;
				this.HashBytes = value.ByteRepresentation;
			}
		}

		// Token: 0x17000F53 RID: 3923
		// (get) Token: 0x060037DE RID: 14302 RVA: 0x00176B08 File Offset: 0x00174D08
		public DateTime InstallTime
		{
			get
			{
				DateTime dateTime = this.cachedDateTime.GetValueOrDefault();
				if (this.cachedDateTime == null)
				{
					dateTime = DateTime.UtcNow + TimeSpan.FromSeconds(this.InstallTimeDiffInSeconds);
					this.cachedDateTime = new DateTime?(dateTime);
					return dateTime;
				}
				return dateTime;
			}
		}

		// Token: 0x17000F54 RID: 3924
		// (get) Token: 0x060037DF RID: 14303 RVA: 0x00176B55 File Offset: 0x00174D55
		[Nullable(2)]
		public RegularPackage RegularPackage
		{
			[NullableContext(2)]
			get
			{
				return ContentPackageManager.RegularPackages.FirstOrDefault((RegularPackage p) => p.Name.Equals(this.Name) && p.Hash.Equals(this.Hash)) ?? ContentPackageManager.RegularPackages.FirstOrDefault((RegularPackage p) => p.Hash.Equals(this.Hash));
			}
		}

		// Token: 0x17000F55 RID: 3925
		// (get) Token: 0x060037E0 RID: 14304 RVA: 0x00176B87 File Offset: 0x00174D87
		[Nullable(2)]
		public CorePackage CorePackage
		{
			[NullableContext(2)]
			get
			{
				return ContentPackageManager.CorePackages.FirstOrDefault((CorePackage p) => p.Name.Equals(this.Name) && p.Hash.Equals(this.Hash)) ?? ContentPackageManager.CorePackages.FirstOrDefault((CorePackage p) => p.Hash.Equals(this.Hash));
			}
		}

		// Token: 0x17000F56 RID: 3926
		// (get) Token: 0x060037E1 RID: 14305 RVA: 0x00176BB9 File Offset: 0x00174DB9
		[Nullable(2)]
		public ContentPackage ContentPackage
		{
			[NullableContext(2)]
			get
			{
				return this.RegularPackage ?? this.CorePackage;
			}
		}

		// Token: 0x060037E2 RID: 14306 RVA: 0x00176BCB File Offset: 0x00174DCB
		public ServerContentPackage()
		{
		}

		// Token: 0x060037E3 RID: 14307 RVA: 0x00176BF4 File Offset: 0x00174DF4
		public ServerContentPackage(ContentPackage contentPackage, SerializableDateTime referenceTime)
		{
			this.Name = contentPackage.Name;
			this.Hash = contentPackage.Hash;
			ContentPackageId ugcId;
			this.UgcId = (contentPackage.UgcId.TryUnwrap(out ugcId) ? ugcId.StringRepresentation : "");
			this.IsMandatory = !contentPackage.Files.All((ContentFile f) => f is SubmarineFile);
			this.IsVanilla = (contentPackage == ContentPackageManager.VanillaCorePackage);
			SerializableDateTime installTime;
			this.InstallTimeDiffInSeconds = (contentPackage.InstallTime.TryUnwrap(out installTime) ? ((uint)(installTime - referenceTime).TotalSeconds) : 0U);
		}

		// Token: 0x060037E4 RID: 14308 RVA: 0x00176CD0 File Offset: 0x00174ED0
		public string GetPackageStr()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 2);
			defaultInterpolatedStringHandler.AppendLiteral("\"");
			defaultInterpolatedStringHandler.AppendFormatted(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral("\" (hash ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Hash.ShortRepresentation);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001BF7 RID: 7159
		[NetworkSerialize(280)]
		public string Name = "";

		// Token: 0x04001BF8 RID: 7160
		[NetworkSerialize(283, ArrayMaxSize = 65535)]
		public byte[] HashBytes = Array.Empty<byte>();

		// Token: 0x04001BF9 RID: 7161
		[NetworkSerialize(286)]
		public string UgcId = "";

		// Token: 0x04001BFA RID: 7162
		[NetworkSerialize(289)]
		public uint InstallTimeDiffInSeconds;

		// Token: 0x04001BFB RID: 7163
		[NetworkSerialize(292)]
		public bool IsMandatory;

		// Token: 0x04001BFC RID: 7164
		[NetworkSerialize(295)]
		public bool IsVanilla;

		// Token: 0x04001BFD RID: 7165
		[Nullable(2)]
		private Md5Hash cachedHash;

		// Token: 0x04001BFE RID: 7166
		private DateTime? cachedDateTime;
	}
}
