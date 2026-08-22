using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000414 RID: 1044
	public interface IStorageService : IService, IDisposable
	{
		// Token: 0x17000FCA RID: 4042
		// (get) Token: 0x06003B4E RID: 15182
		// (set) Token: 0x06003B4F RID: 15183
		bool UseCaching { get; set; }

		// Token: 0x06003B50 RID: 15184
		void PurgeCache();

		// Token: 0x06003B51 RID: 15185
		void PurgeFileFromCache(string absolutePath);

		// Token: 0x06003B52 RID: 15186
		void PurgeFilesFromCache(params string[] absolutePaths);

		// Token: 0x06003B53 RID: 15187
		Result<XDocument> LoadLocalXml(ContentPackage package, string localFilePath);

		// Token: 0x06003B54 RID: 15188
		Result<byte[]> LoadLocalBinary(ContentPackage package, string localFilePath);

		// Token: 0x06003B55 RID: 15189
		Result<string> LoadLocalText(ContentPackage package, string localFilePath);

		// Token: 0x06003B56 RID: 15190
		Result SaveLocalXml(ContentPackage package, string localFilePath, XDocument document);

		// Token: 0x06003B57 RID: 15191
		Result SaveLocalBinary(ContentPackage package, string localFilePath, in byte[] bytes);

		// Token: 0x06003B58 RID: 15192
		Result SaveLocalText(ContentPackage package, string localFilePath, in string text);

		// Token: 0x06003B59 RID: 15193
		Task<Result<XDocument>> LoadLocalXmlAsync(ContentPackage package, string localFilePath);

		// Token: 0x06003B5A RID: 15194
		Task<Result<byte[]>> LoadLocalBinaryAsync(ContentPackage package, string localFilePath);

		// Token: 0x06003B5B RID: 15195
		Task<Result<string>> LoadLocalTextAsync(ContentPackage package, string localFilePath);

		// Token: 0x06003B5C RID: 15196
		Task<Result> SaveLocalXmlAsync(ContentPackage package, string localFilePath, XDocument document);

		// Token: 0x06003B5D RID: 15197
		Task<Result> SaveLocalBinaryAsync(ContentPackage package, string localFilePath, byte[] bytes);

		// Token: 0x06003B5E RID: 15198
		Task<Result> SaveLocalTextAsync(ContentPackage package, string localFilePath, string text);

		// Token: 0x06003B5F RID: 15199
		Result<XDocument> LoadPackageXml(ContentPath filePath);

		// Token: 0x06003B60 RID: 15200
		Result<byte[]> LoadPackageBinary(ContentPath filePath);

		// Token: 0x06003B61 RID: 15201
		Result<string> LoadPackageText(ContentPath filePath);

		// Token: 0x06003B62 RID: 15202
		ImmutableArray<ValueTuple<ContentPath, Result<XDocument>>> LoadPackageXmlFiles(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06003B63 RID: 15203
		ImmutableArray<ValueTuple<ContentPath, Result<byte[]>>> LoadPackageBinaryFiles(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06003B64 RID: 15204
		ImmutableArray<ValueTuple<ContentPath, Result<string>>> LoadPackageTextFiles(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06003B65 RID: 15205
		Result<ImmutableArray<string>> FindFilesInPackage(ContentPackage package, string localSubfolder, string regexFilter, bool searchRecursively);

		// Token: 0x06003B66 RID: 15206
		Task<Result<XDocument>> LoadPackageXmlAsync(ContentPath filePath);

		// Token: 0x06003B67 RID: 15207
		Task<Result<byte[]>> LoadPackageBinaryAsync(ContentPath filePath);

		// Token: 0x06003B68 RID: 15208
		Task<Result<string>> LoadPackageTextAsync(ContentPath filePath);

		// Token: 0x06003B69 RID: 15209
		Task<ImmutableArray<ValueTuple<ContentPath, Result<XDocument>>>> LoadPackageXmlFilesAsync(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06003B6A RID: 15210
		Task<ImmutableArray<ValueTuple<ContentPath, Result<byte[]>>>> LoadPackageBinaryFilesAsync(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06003B6B RID: 15211
		Task<ImmutableArray<ValueTuple<ContentPath, Result<string>>>> LoadPackageTextFilesAsync(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06003B6C RID: 15212
		Result<XDocument> TryLoadXml(string filePath, Encoding encoding = null);

		// Token: 0x06003B6D RID: 15213
		Result<string> TryLoadText(string filePath, Encoding encoding = null);

		// Token: 0x06003B6E RID: 15214
		Result<byte[]> TryLoadBinary(string filePath);

		// Token: 0x06003B6F RID: 15215
		Result TrySaveXml(string filePath, in XDocument document, Encoding encoding = null);

		// Token: 0x06003B70 RID: 15216
		Result TrySaveText(string filePath, in string text, Encoding encoding = null);

		// Token: 0x06003B71 RID: 15217
		Result TrySaveBinary(string filePath, in byte[] bytes);

		// Token: 0x06003B72 RID: 15218
		Result<bool> FileExists(string filePath);

		// Token: 0x06003B73 RID: 15219
		Result<bool> DirectoryExists(string directoryPath);

		// Token: 0x06003B74 RID: 15220
		Task<Result<XDocument>> TryLoadXmlAsync(string filePath, Encoding encoding = null);

		// Token: 0x06003B75 RID: 15221
		Task<Result<string>> TryLoadTextAsync(string filePath, Encoding encoding = null);

		// Token: 0x06003B76 RID: 15222
		Task<Result<byte[]>> TryLoadBinaryAsync(string filePath);

		// Token: 0x06003B77 RID: 15223
		Task<Result> TrySaveXmlAsync(string filePath, XDocument document, Encoding encoding = null);

		// Token: 0x06003B78 RID: 15224
		Task<Result> TrySaveTextAsync(string filePath, string text, Encoding encoding = null);

		// Token: 0x06003B79 RID: 15225
		Task<Result> TrySaveBinaryAsync(string filePath, byte[] bytes);
	}
}
