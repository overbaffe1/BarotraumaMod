using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000527 RID: 1319
	public interface IStorageService : IService, IDisposable
	{
		// Token: 0x17001511 RID: 5393
		// (get) Token: 0x0600546A RID: 21610
		// (set) Token: 0x0600546B RID: 21611
		bool UseCaching { get; set; }

		// Token: 0x0600546C RID: 21612
		void PurgeCache();

		// Token: 0x0600546D RID: 21613
		void PurgeFileFromCache(string absolutePath);

		// Token: 0x0600546E RID: 21614
		void PurgeFilesFromCache(params string[] absolutePaths);

		// Token: 0x0600546F RID: 21615
		Result<XDocument> LoadLocalXml(ContentPackage package, string localFilePath);

		// Token: 0x06005470 RID: 21616
		Result<byte[]> LoadLocalBinary(ContentPackage package, string localFilePath);

		// Token: 0x06005471 RID: 21617
		Result<string> LoadLocalText(ContentPackage package, string localFilePath);

		// Token: 0x06005472 RID: 21618
		Result SaveLocalXml(ContentPackage package, string localFilePath, XDocument document);

		// Token: 0x06005473 RID: 21619
		Result SaveLocalBinary(ContentPackage package, string localFilePath, in byte[] bytes);

		// Token: 0x06005474 RID: 21620
		Result SaveLocalText(ContentPackage package, string localFilePath, in string text);

		// Token: 0x06005475 RID: 21621
		Task<Result<XDocument>> LoadLocalXmlAsync(ContentPackage package, string localFilePath);

		// Token: 0x06005476 RID: 21622
		Task<Result<byte[]>> LoadLocalBinaryAsync(ContentPackage package, string localFilePath);

		// Token: 0x06005477 RID: 21623
		Task<Result<string>> LoadLocalTextAsync(ContentPackage package, string localFilePath);

		// Token: 0x06005478 RID: 21624
		Task<Result> SaveLocalXmlAsync(ContentPackage package, string localFilePath, XDocument document);

		// Token: 0x06005479 RID: 21625
		Task<Result> SaveLocalBinaryAsync(ContentPackage package, string localFilePath, byte[] bytes);

		// Token: 0x0600547A RID: 21626
		Task<Result> SaveLocalTextAsync(ContentPackage package, string localFilePath, string text);

		// Token: 0x0600547B RID: 21627
		Result<XDocument> LoadPackageXml(ContentPath filePath);

		// Token: 0x0600547C RID: 21628
		Result<byte[]> LoadPackageBinary(ContentPath filePath);

		// Token: 0x0600547D RID: 21629
		Result<string> LoadPackageText(ContentPath filePath);

		// Token: 0x0600547E RID: 21630
		ImmutableArray<ValueTuple<ContentPath, Result<XDocument>>> LoadPackageXmlFiles(ImmutableArray<ContentPath> filePaths);

		// Token: 0x0600547F RID: 21631
		ImmutableArray<ValueTuple<ContentPath, Result<byte[]>>> LoadPackageBinaryFiles(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06005480 RID: 21632
		ImmutableArray<ValueTuple<ContentPath, Result<string>>> LoadPackageTextFiles(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06005481 RID: 21633
		Result<ImmutableArray<string>> FindFilesInPackage(ContentPackage package, string localSubfolder, string regexFilter, bool searchRecursively);

		// Token: 0x06005482 RID: 21634
		Task<Result<XDocument>> LoadPackageXmlAsync(ContentPath filePath);

		// Token: 0x06005483 RID: 21635
		Task<Result<byte[]>> LoadPackageBinaryAsync(ContentPath filePath);

		// Token: 0x06005484 RID: 21636
		Task<Result<string>> LoadPackageTextAsync(ContentPath filePath);

		// Token: 0x06005485 RID: 21637
		Task<ImmutableArray<ValueTuple<ContentPath, Result<XDocument>>>> LoadPackageXmlFilesAsync(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06005486 RID: 21638
		Task<ImmutableArray<ValueTuple<ContentPath, Result<byte[]>>>> LoadPackageBinaryFilesAsync(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06005487 RID: 21639
		Task<ImmutableArray<ValueTuple<ContentPath, Result<string>>>> LoadPackageTextFilesAsync(ImmutableArray<ContentPath> filePaths);

		// Token: 0x06005488 RID: 21640
		Result<XDocument> TryLoadXml(string filePath, Encoding encoding = null);

		// Token: 0x06005489 RID: 21641
		Result<string> TryLoadText(string filePath, Encoding encoding = null);

		// Token: 0x0600548A RID: 21642
		Result<byte[]> TryLoadBinary(string filePath);

		// Token: 0x0600548B RID: 21643
		Result TrySaveXml(string filePath, in XDocument document, Encoding encoding = null);

		// Token: 0x0600548C RID: 21644
		Result TrySaveText(string filePath, in string text, Encoding encoding = null);

		// Token: 0x0600548D RID: 21645
		Result TrySaveBinary(string filePath, in byte[] bytes);

		// Token: 0x0600548E RID: 21646
		Result<bool> FileExists(string filePath);

		// Token: 0x0600548F RID: 21647
		Result<bool> DirectoryExists(string directoryPath);

		// Token: 0x06005490 RID: 21648
		Task<Result<XDocument>> TryLoadXmlAsync(string filePath, Encoding encoding = null);

		// Token: 0x06005491 RID: 21649
		Task<Result<string>> TryLoadTextAsync(string filePath, Encoding encoding = null);

		// Token: 0x06005492 RID: 21650
		Task<Result<byte[]>> TryLoadBinaryAsync(string filePath);

		// Token: 0x06005493 RID: 21651
		Task<Result> TrySaveXmlAsync(string filePath, XDocument document, Encoding encoding = null);

		// Token: 0x06005494 RID: 21652
		Task<Result> TrySaveTextAsync(string filePath, string text, Encoding encoding = null);

		// Token: 0x06005495 RID: 21653
		Task<Result> TrySaveBinaryAsync(string filePath, byte[] bytes);
	}
}
