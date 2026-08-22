using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;

namespace Barotrauma.RuinGeneration
{
	// Token: 0x020004D9 RID: 1241
	internal class RuinGenerationParams : OutpostGenerationParams
	{
		// Token: 0x170014A3 RID: 5283
		// (get) Token: 0x060050D3 RID: 20691 RVA: 0x002B88C7 File Offset: 0x002B6AC7
		public override string Name
		{
			get
			{
				return "RuinGenerationParams";
			}
		}

		// Token: 0x170014A4 RID: 5284
		// (get) Token: 0x060050D4 RID: 20692 RVA: 0x002B88CE File Offset: 0x002B6ACE
		// (set) Token: 0x060050D5 RID: 20693 RVA: 0x002B88D6 File Offset: 0x002B6AD6
		[Serialize(true, IsPropertySaveable.Yes, "Are these params designed to be used for alien ruins targeted by missions. If false, the params are ignored when there's any missions targeting ruins.", "", false)]
		[Editable]
		public bool IsMissionReady { get; set; }

		// Token: 0x060050D6 RID: 20694 RVA: 0x002B88DF File Offset: 0x002B6ADF
		public RuinGenerationParams(ContentXElement element, RuinConfigFile file) : base(element, file)
		{
		}

		// Token: 0x060050D7 RID: 20695 RVA: 0x002B88EC File Offset: 0x002B6AEC
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

		// Token: 0x060050D8 RID: 20696 RVA: 0x002B8A24 File Offset: 0x002B6C24
		public override void Dispose()
		{
		}

		// Token: 0x04002AC5 RID: 10949
		public static readonly PrefabCollection<RuinGenerationParams> RuinParams = new PrefabCollection<RuinGenerationParams>();
	}
}
