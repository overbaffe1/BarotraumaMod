using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Particles
{
	// Token: 0x02000451 RID: 1105
	[NullableContext(1)]
	[Nullable(0)]
	internal class ParticleEmitterPrefab
	{
		// Token: 0x170012DA RID: 4826
		// (get) Token: 0x06004A15 RID: 18965 RVA: 0x0028E8E4 File Offset: 0x0028CAE4
		[Nullable(2)]
		public ParticlePrefab ParticlePrefab
		{
			[NullableContext(2)]
			get
			{
				ParticlePrefab prefab;
				ParticlePrefab.Prefabs.TryGet(this.ParticlePrefabName, out prefab);
				return prefab;
			}
		}

		// Token: 0x170012DB RID: 4827
		// (get) Token: 0x06004A16 RID: 18966 RVA: 0x0028E905 File Offset: 0x0028CB05
		public ParticleDrawOrder DrawOrder
		{
			get
			{
				if (this.Properties.DrawOrder != ParticleDrawOrder.Default)
				{
					return this.Properties.DrawOrder;
				}
				ParticlePrefab particlePrefab = this.ParticlePrefab;
				if (particlePrefab == null)
				{
					return ParticleDrawOrder.Default;
				}
				return particlePrefab.DrawOrder;
			}
		}

		// Token: 0x06004A17 RID: 18967 RVA: 0x0028E934 File Offset: 0x0028CB34
		public ParticleEmitterPrefab(ContentXElement element)
		{
			ContentXElement contentXElement = null;
			if (element == contentXElement)
			{
				throw new ArgumentNullException("element");
			}
			this.Properties = new ParticleEmitterProperties(element);
			this.ParticlePrefabName = element.GetAttributeIdentifier("particle", "");
			this.ContentPackage = element.ContentPackage;
		}

		// Token: 0x06004A18 RID: 18968 RVA: 0x0028E992 File Offset: 0x0028CB92
		public ParticleEmitterPrefab(ParticlePrefab prefab, ParticleEmitterProperties properties)
		{
			this.Properties = properties;
			this.ParticlePrefabName = prefab.Identifier;
		}

		// Token: 0x040026B9 RID: 9913
		public readonly Identifier ParticlePrefabName;

		// Token: 0x040026BA RID: 9914
		public readonly ParticleEmitterProperties Properties;

		// Token: 0x040026BB RID: 9915
		[Nullable(2)]
		public readonly ContentPackage ContentPackage;
	}
}
