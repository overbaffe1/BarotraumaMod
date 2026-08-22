using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.BasicDescriptors;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000416 RID: 1046
	public class DefaultLuaRegistrar : IDefaultLuaRegistrar, IService, IDisposable
	{
		// Token: 0x17000FCB RID: 4043
		// (get) Token: 0x06003B7B RID: 15227 RVA: 0x001899AC File Offset: 0x00187BAC
		// (set) Token: 0x06003B7C RID: 15228 RVA: 0x001899B4 File Offset: 0x00187BB4
		public bool IsDisposed { get; private set; }

		// Token: 0x06003B7D RID: 15229 RVA: 0x001899BD File Offset: 0x00187BBD
		public DefaultLuaRegistrar(ILoggerService loggerService, ILuaUserDataService userDataService, ISafeLuaUserDataService safeUserDataService)
		{
			this._userDataService = userDataService;
			this._safeUserDataService = safeUserDataService;
			this._loggerService = loggerService;
		}

		// Token: 0x06003B7E RID: 15230 RVA: 0x001899DC File Offset: 0x00187BDC
		private unsafe void RegisterShared()
		{
			this._userDataService.RegisterType("System.TimeSpan");
			this._userDataService.RegisterType("System.Exception");
			this._userDataService.RegisterType("System.Console");
			this._userDataService.RegisterType("System.Exception");
			this._userDataService.RegisterType("Barotrauma.Success`2");
			this._userDataService.RegisterType("Barotrauma.Failure`2");
			this._userDataService.RegisterType("Barotrauma.Range`1");
			this._userDataService.RegisterType("Barotrauma.ItemPrefab");
			this._userDataService.RegisterType("Barotrauma.InputType");
			int num = 3;
			List<Assembly> list = new List<Assembly>(num);
			CollectionsMarshal.SetCount<Assembly>(list, num);
			Span<Assembly> span = CollectionsMarshal.AsSpan<Assembly>(list);
			*span[0] = typeof(DefaultLuaRegistrar).Assembly;
			*span[1] = typeof(Identifier).Assembly;
			*span[2] = typeof(Vector2).Assembly;
			List<Assembly> assembliesToScan = list;
			foreach (Type type in assembliesToScan.SelectMany((Assembly a) => a.GetTypes()))
			{
				if (!type.IsEnum && !type.Name.StartsWith("<") && !type.IsDefined(typeof(CompilerGeneratedAttribute)) && this._safeUserDataService.IsAllowed(type.FullName))
				{
					this._userDataService.RegisterType(type.FullName);
				}
			}
			this._userDataService.RegisterType("Barotrauma.LuaSByte");
			this._userDataService.RegisterType("Barotrauma.LuaByte");
			this._userDataService.RegisterType("Barotrauma.LuaInt16");
			this._userDataService.RegisterType("Barotrauma.LuaUInt16");
			this._userDataService.RegisterType("Barotrauma.LuaInt32");
			this._userDataService.RegisterType("Barotrauma.LuaUInt32");
			this._userDataService.RegisterType("Barotrauma.LuaInt64");
			this._userDataService.RegisterType("Barotrauma.LuaUInt64");
			this._userDataService.RegisterType("Barotrauma.LuaSingle");
			this._userDataService.RegisterType("Barotrauma.LuaDouble");
			this._userDataService.RegisterType("Barotrauma.Level+InterestingPosition");
			this._userDataService.RegisterType("Barotrauma.Networking.RespawnManager+TeamSpecificState");
			this._userDataService.RegisterType("Barotrauma.CharacterParams+AIParams");
			this._userDataService.RegisterType("Barotrauma.CharacterParams+TargetParams");
			this._userDataService.RegisterType("Barotrauma.CharacterParams+InventoryParams");
			this._userDataService.RegisterType("Barotrauma.CharacterParams+HealthParams");
			this._userDataService.RegisterType("Barotrauma.CharacterParams+ParticleParams");
			this._userDataService.RegisterType("Barotrauma.CharacterParams+SoundParams");
			this._userDataService.RegisterType("Barotrauma.FabricationRecipe+RequiredItemByIdentifier");
			this._userDataService.RegisterType("Barotrauma.FabricationRecipe+RequiredItemByTag");
			this._userDataService.MakeFieldAccessible(this._userDataService.RegisterType("Barotrauma.StatusEffect"), "user");
			this._userDataService.RegisterType("Barotrauma.ContentPackageManager+PackageSource");
			this._userDataService.RegisterType("Barotrauma.ContentPackageManager+EnabledPackages");
			this._userDataService.RegisterType("System.Xml.Linq.XElement");
			this._userDataService.RegisterType("System.Xml.Linq.XName");
			this._userDataService.RegisterType("System.Xml.Linq.XAttribute");
			this._userDataService.RegisterType("System.Xml.Linq.XContainer");
			this._userDataService.RegisterType("System.Xml.Linq.XDocument");
			this._userDataService.RegisterType("System.Xml.Linq.XNode");
			this._userDataService.RegisterType("Barotrauma.Networking.ServerSettings+SavedClientPermission");
			this._userDataService.RegisterType("Barotrauma.Inventory+ItemSlot");
			this._userDataService.MakeFieldAccessible(this._userDataService.RegisterType("Barotrauma.Items.Components.CustomInterface"), "customInterfaceElementList");
			this._userDataService.RegisterType("Barotrauma.Items.Components.CustomInterface+CustomInterfaceElement");
			this._userDataService.RegisterType("Barotrauma.DebugConsole+Command");
			IUserDataDescriptor descriptor = this._userDataService.RegisterType("Barotrauma.NetLobbyScreen");
			this._userDataService.MakeFieldAccessible(descriptor, "subs");
			this._userDataService.RegisterType("FarseerPhysics.Dynamics.Body");
			this._userDataService.RegisterType("FarseerPhysics.Dynamics.World");
			this._userDataService.RegisterType("FarseerPhysics.Dynamics.Fixture");
			this._userDataService.RegisterType("FarseerPhysics.ConvertUnits");
			this._userDataService.RegisterType("FarseerPhysics.Collision.AABB");
			this._userDataService.RegisterType("FarseerPhysics.Collision.ContactFeature");
			this._userDataService.RegisterType("FarseerPhysics.Collision.ManifoldPoint");
			this._userDataService.RegisterType("FarseerPhysics.Collision.ContactID");
			this._userDataService.RegisterType("FarseerPhysics.Collision.Manifold");
			this._userDataService.RegisterType("FarseerPhysics.Collision.RayCastInput");
			this._userDataService.RegisterType("FarseerPhysics.Collision.ClipVertex");
			this._userDataService.RegisterType("FarseerPhysics.Collision.RayCastOutput");
			this._userDataService.RegisterType("FarseerPhysics.Collision.EPAxis");
			this._userDataService.RegisterType("FarseerPhysics.Collision.ReferenceFace");
			this._userDataService.RegisterType("FarseerPhysics.Collision.Collision");
			this._userDataService.RegisterType("Voronoi2.DoubleVector2");
			this._userDataService.RegisterType("Voronoi2.Site");
			this._userDataService.RegisterType("Voronoi2.Edge");
			this._userDataService.RegisterType("Voronoi2.Halfedge");
			this._userDataService.RegisterType("Voronoi2.VoronoiCell");
			this._userDataService.RegisterType("Voronoi2.GraphEdge");
			this._userDataService.RegisterType("Barotrauma.PrefabCollection`1");
			this._userDataService.RegisterType("Barotrauma.PrefabSelector`1");
			this._userDataService.RegisterType("Barotrauma.Pair`2");
			this._userDataService.RegisterExtensionType("Barotrauma.MathUtils");
			this._userDataService.RegisterExtensionType("Barotrauma.XMLExtensions");
			StandardUserDataDescriptor itemPrefabDescriptor = (StandardUserDataDescriptor)this._userDataService.RegisterType("Barotrauma.ItemPrefab");
			itemPrefabDescriptor.AddMember("GetItemPrefab", new MethodMemberDescriptor(typeof(ModUtils.ItemPrefab).GetMethod("GetItemPrefab", BindingFlags.Static | BindingFlags.NonPublic), InteropAccessMode.Default));
			StandardUserDataDescriptor clientDescriptor = (StandardUserDataDescriptor)this._userDataService.RegisterType("Barotrauma.Networking.Client");
			clientDescriptor.AddMember("ClientList", new PropertyMemberDescriptor(typeof(ModUtils.Client).GetProperty("ClientList", BindingFlags.Static | BindingFlags.NonPublic), InteropAccessMode.LazyOptimized));
			clientDescriptor.AddMember("SteamID", new DefaultLuaRegistrar.SteamIDMemberDescriptor());
			clientDescriptor.AddMember("UnbanPlayer", new MethodMemberDescriptor(typeof(ModUtils.Client).GetMethod("UnbanPlayer", BindingFlags.Static | BindingFlags.NonPublic), InteropAccessMode.LazyOptimized));
			clientDescriptor.AddMember("BanPlayer", new MethodMemberDescriptor(typeof(ModUtils.Client).GetMethod("BanPlayer", BindingFlags.Static | BindingFlags.NonPublic), InteropAccessMode.LazyOptimized));
			this._userDataService.RegisterExtensionType(typeof(ClientExtensions).FullName);
			this._userDataService.RegisterExtensionType(typeof(ItemExtensions).FullName);
			this._userDataService.RegisterExtensionType(typeof(MapEntityExtensions).FullName);
			this._userDataService.RegisterExtensionType(typeof(QualityExtensions).FullName);
			IUserDataDescriptor toolBox = UserData.RegisterType(typeof(ToolBox), InteropAccessMode.Default, null);
		}

		// Token: 0x06003B7F RID: 15231 RVA: 0x0018A130 File Offset: 0x00188330
		private void RegisterServer()
		{
			this._userDataService.RegisterType("Barotrauma.Character+TeamChangeEventData");
		}

		// Token: 0x06003B80 RID: 15232 RVA: 0x0018A143 File Offset: 0x00188343
		public void RegisterAll()
		{
			this.RegisterShared();
			this.RegisterServer();
		}

		// Token: 0x06003B81 RID: 15233 RVA: 0x0018A151 File Offset: 0x00188351
		public void Dispose()
		{
			this.IsDisposed = true;
		}

		// Token: 0x04001D63 RID: 7523
		private readonly ILuaUserDataService _userDataService;

		// Token: 0x04001D64 RID: 7524
		private readonly ISafeLuaUserDataService _safeUserDataService;

		// Token: 0x04001D65 RID: 7525
		private readonly ILoggerService _loggerService;

		// Token: 0x02000D22 RID: 3362
		private class SteamIDMemberDescriptor : IMemberDescriptor
		{
			// Token: 0x17001647 RID: 5703
			// (get) Token: 0x0600665B RID: 26203 RVA: 0x0021E176 File Offset: 0x0021C376
			public bool IsStatic
			{
				get
				{
					return false;
				}
			}

			// Token: 0x17001648 RID: 5704
			// (get) Token: 0x0600665C RID: 26204 RVA: 0x0021E179 File Offset: 0x0021C379
			public string Name
			{
				get
				{
					return "SteamID";
				}
			}

			// Token: 0x17001649 RID: 5705
			// (get) Token: 0x0600665D RID: 26205 RVA: 0x0021E180 File Offset: 0x0021C380
			public MemberDescriptorAccess MemberAccess
			{
				get
				{
					return MemberDescriptorAccess.CanRead;
				}
			}

			// Token: 0x0600665E RID: 26206 RVA: 0x0021E184 File Offset: 0x0021C384
			public DynValue GetValue(Script script, object obj)
			{
				Client client = obj as Client;
				if (client != null)
				{
					return DynValue.FromObject(script, ModUtils.Client.GetSteamId(client));
				}
				throw new NotImplementedException();
			}

			// Token: 0x0600665F RID: 26207 RVA: 0x0021E1B2 File Offset: 0x0021C3B2
			public void SetValue(Script script, object obj, DynValue value)
			{
				throw new NotImplementedException();
			}
		}
	}
}
