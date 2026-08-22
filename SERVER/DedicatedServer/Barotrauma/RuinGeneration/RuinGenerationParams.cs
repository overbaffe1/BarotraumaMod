using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;

namespace Barotrauma.RuinGeneration
{
	// Token: 0x020002E2 RID: 738
	internal class RuinGenerationParams : OutpostGenerationParams
	{
		// Token: 0x17000E03 RID: 3587
		// (get) Token: 0x06003158 RID: 12632 RVA: 0x00150FDF File Offset: 0x0014F1DF
		public override string Name
		{
			get
			{
				return "RuinGenerationParams";
			}
		}

		// Token: 0x17000E04 RID: 3588
		// (get) Token: 0x06003159 RID: 12633 RVA: 0x00150FE6 File Offset: 0x0014F1E6
		// (set) Token: 0x0600315A RID: 12634 RVA: 0x00150FEE File Offset: 0x0014F1EE
		[Serialize(true, IsPropertySaveable.Yes, "Are these params designed to be used for alien ruins targeted by missions. If false, the params are ignored when there's any missions targeting ruins.", "", false)]
		[Editable]
		public bool IsMissionReady { get; set; }

		// Token: 0x0600315B RID: 12635 RVA: 0x00150FF7 File Offset: 0x0014F1F7
		public RuinGenerationParams(ContentXElement element, RuinConfigFile file) : base(element, file)
		{
		}

		// Token: 0x0600315C RID: 12636 RVA: 0x00151004 File Offset: 0x0014F204
		public static void SaveAll()
		{
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				NewLineOnAttributes = true
			};
			IEnumerable<ContentPackage> packages = ContentPackageManager.LocalPackages;
			foreach (RuinGenerationParams generationParams in RuinGenerationParams.RuinParams)
			{
				foreach (RuinConfigFile configFile in packages.SelectMany((ContentPackage p) => p.GetFiles<RuinConfigFile>()))
				{
					if (!(configFile.Path != generationParams.ContentFile.Path))
					{
						XDocument doc = XMLExtensions.TryLoadXml(configFile.Path);
						if (doc != null)
						{
							SerializableProperty.SerializeProperties(generationParams, doc.Root, false, false);
							using (XmlWriter writer = XmlWriter.Create(configFile.Path.Value, settings))
							{
								doc.WriteTo(writer);
								writer.Flush();
							}
						}
					}
				}
			}
		}

		// Token: 0x0600315D RID: 12637 RVA: 0x0015113C File Offset: 0x0014F33C
		public override void Dispose()
		{
		}

		// Token: 0x04001866 RID: 6246
		public static readonly PrefabCollection<RuinGenerationParams> RuinParams = new PrefabCollection<RuinGenerationParams>();
	}
}
