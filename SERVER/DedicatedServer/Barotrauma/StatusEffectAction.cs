using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200001D RID: 29
	internal class StatusEffectAction : EventAction
	{
		// Token: 0x06000404 RID: 1028 RVA: 0x00020EA0 File Offset: 0x0001F0A0
		private void ServerWrite(IEnumerable<Entity> targets)
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(21);
			outmsg.WriteByte(2);
			outmsg.WriteIdentifier(this.ParentEvent.Prefab.Identifier);
			outmsg.WriteUInt16((ushort)this.actionIndex);
			outmsg.WriteUInt16((ushort)targets.Count<Entity>());
			foreach (Entity target in targets)
			{
				outmsg.WriteUInt16(target.ID);
			}
			foreach (Client c in GameMain.Server.ConnectedClients)
			{
				ServerPeer serverPeer = GameMain.Server.ServerPeer;
				if (serverPeer != null)
				{
					serverPeer.Send(outmsg, c.Connection, DeliveryMethod.Reliable, true);
				}
			}
		}

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000405 RID: 1029 RVA: 0x00020F8C File Offset: 0x0001F18C
		// (set) Token: 0x06000406 RID: 1030 RVA: 0x00020F94 File Offset: 0x0001F194
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity or entities the status effect should target.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06000407 RID: 1031 RVA: 0x00020FA0 File Offset: 0x0001F1A0
		public StatusEffectAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.actionIndex = 0;
			foreach (ContentXElement subElement in parentEvent.Prefab.ConfigElement.Descendants())
			{
				if (subElement == element)
				{
					break;
				}
				this.actionIndex++;
			}
			foreach (ContentXElement subElement2 in element.Elements())
			{
				string a = subElement2.Name.ToString().ToLowerInvariant();
				if (a == "statuseffect")
				{
					List<StatusEffect> list = this.effects;
					ContentXElement element2 = subElement2;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
					defaultInterpolatedStringHandler.AppendFormatted("StatusEffectAction");
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					list.Add(StatusEffect.Load(element2, defaultInterpolatedStringHandler.ToStringAndClear()));
				}
			}
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x000210D4 File Offset: 0x0001F2D4
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x000210DC File Offset: 0x0001F2DC
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x000210E8 File Offset: 0x0001F2E8
		public override void Update(float deltaTime)
		{
			StatusEffectAction.<>c__DisplayClass11_0 CS$<>8__locals1;
			CS$<>8__locals1.deltaTime = deltaTime;
			if (this.isFinished)
			{
				return;
			}
			IEnumerable<Entity> eventTargets = this.ParentEvent.GetTargets(this.TargetTag);
			foreach (StatusEffect effect in this.effects)
			{
				foreach (Entity target in eventTargets)
				{
					if (effect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
					{
						List<ISerializableEntity> nearbyTargets = new List<ISerializableEntity>();
						effect.AddNearbyTargets(target.WorldPosition, nearbyTargets);
						using (List<ISerializableEntity>.Enumerator enumerator3 = nearbyTargets.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								ISerializableEntity nearbyTarget = enumerator3.Current;
								StatusEffectAction.<Update>g__ApplyOnTarget|11_0(nearbyTarget as Entity, effect, ref CS$<>8__locals1);
							}
							continue;
						}
					}
					StatusEffectAction.<Update>g__ApplyOnTarget|11_0(target, effect, ref CS$<>8__locals1);
				}
			}
			this.ServerWrite(eventTargets);
			this.isFinished = true;
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x00021214 File Offset: 0x0001F414
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("StatusEffectAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x00021280 File Offset: 0x0001F480
		[CompilerGenerated]
		internal static void <Update>g__ApplyOnTarget|11_0(Entity target, StatusEffect effect, ref StatusEffectAction.<>c__DisplayClass11_0 A_2)
		{
			Item targetItem = target as Item;
			if (targetItem != null)
			{
				effect.Apply(effect.type, A_2.deltaTime, target, targetItem.AllPropertyObjects, null);
				return;
			}
			effect.Apply(effect.type, A_2.deltaTime, target, target as ISerializableEntity, null);
		}

		// Token: 0x040001D8 RID: 472
		private readonly List<StatusEffect> effects = new List<StatusEffect>();

		// Token: 0x040001D9 RID: 473
		private readonly int actionIndex;

		// Token: 0x040001DB RID: 475
		private bool isFinished;
	}
}
