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
	// Token: 0x02000529 RID: 1321
	public class DefaultLuaRegistrar : IDefaultLuaRegistrar, IService, IDisposable
	{
		// Token: 0x17001512 RID: 5394
		// (get) Token: 0x06005497 RID: 21655 RVA: 0x002CE144 File Offset: 0x002CC344
		// (set) Token: 0x06005498 RID: 21656 RVA: 0x002CE14C File Offset: 0x002CC34C
		public bool IsDisposed { get; private set; }

		// Token: 0x06005499 RID: 21657 RVA: 0x002CE155 File Offset: 0x002CC355
		public DefaultLuaRegistrar(ILoggerService loggerService, ILuaUserDataService userDataService, ISafeLuaUserDataService safeUserDataService)
		{
			this._userDataService = userDataService;
			this._safeUserDataService = safeUserDataService;
			this._loggerService = loggerService;
		}

		// Token: 0x0600549A RID: 21658 RVA: 0x002CE174 File Offset: 0x002CC374
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
			this._userDataService.RegisterExtensionType(typeof(ClientExtensions).FullName);
			this._userDataService.RegisterExtensionType(typeof(ItemExtensions).FullName);
			this._userDataService.RegisterExtensionType(typeof(MapEntityExtensions).FullName);
			this._userDataService.RegisterExtensionType(typeof(QualityExtensions).FullName);
			IUserDataDescriptor toolBox = UserData.RegisterType(typeof(ToolBox), InteropAccessMode.Default, null);
			this._userDataService.RemoveMember(toolBox, "OpenFileWithShell");
		}

		// Token: 0x0600549B RID: 21659 RVA: 0x002CE878 File Offset: 0x002CCA78
		private void RegisterClient()
		{
			this._userDataService.RegisterType("Microsoft.Xna.Framework.Graphics.Effect");
			this._userDataService.RegisterType("Microsoft.Xna.Framework.Graphics.EffectParameterCollection");
			this._userDataService.RegisterType("Microsoft.Xna.Framework.Graphics.EffectParameter");
			this._userDataService.RegisterType("Microsoft.Xna.Framework.Graphics.SpriteBatch");
			this._userDataService.RegisterType("Microsoft.Xna.Framework.Graphics.Texture2D");
			this._userDataService.RegisterType("EventInput.KeyboardDispatcher");
			this._userDataService.RegisterType("EventInput.KeyEventArgs");
			this._userDataService.RegisterType("Microsoft.Xna.Framework.Input.Keys");
			this._userDataService.RegisterType("Microsoft.Xna.Framework.Input.KeyboardState");
			this._userDataService.RegisterType("Barotrauma.Anchor");
			this._userDataService.RegisterType("Barotrauma.Alignment");
			this._userDataService.RegisterType("Barotrauma.Pivot");
			this._userDataService.RegisterType("Barotrauma.Key");
			this._userDataService.RegisterType("Barotrauma.PlayerInput");
			this._userDataService.RegisterType("Barotrauma.Inventory+SlotReference");
		}

		// Token: 0x0600549C RID: 21660 RVA: 0x002CE984 File Offset: 0x002CCB84
		public void RegisterAll()
		{
			this.RegisterShared();
			this.RegisterClient();
		}

		// Token: 0x0600549D RID: 21661 RVA: 0x002CE992 File Offset: 0x002CCB92
		public void Dispose()
		{
			this.IsDisposed = true;
		}

		// Token: 0x04002C47 RID: 11335
		private readonly ILuaUserDataService _userDataService;

		// Token: 0x04002C48 RID: 11336
		private readonly ISafeLuaUserDataService _safeUserDataService;

		// Token: 0x04002C49 RID: 11337
		private readonly ILoggerService _loggerService;

		// Token: 0x02001342 RID: 4930
		private class SteamIDMemberDescriptor : IMemberDescriptor
		{
			// Token: 0x17001D07 RID: 7431
			// (get) Token: 0x060096EF RID: 38639 RVA: 0x003DA806 File Offset: 0x003D8A06
			public bool IsStatic
			{
				get
				{
					return false;
				}
			}

			// Token: 0x17001D08 RID: 7432
			// (get) Token: 0x060096F0 RID: 38640 RVA: 0x003DA809 File Offset: 0x003D8A09
			public string Name
			{
				get
				{
					return "SteamID";
				}
			}

			// Token: 0x17001D09 RID: 7433
			// (get) Token: 0x060096F1 RID: 38641 RVA: 0x003DA810 File Offset: 0x003D8A10
			public MemberDescriptorAccess MemberAccess
			{
				get
				{
					return MemberDescriptorAccess.CanRead;
				}
			}

			// Token: 0x060096F2 RID: 38642 RVA: 0x003DA814 File Offset: 0x003D8A14
			public DynValue GetValue(Script script, object obj)
			{
				Client client = obj as Client;
				if (client != null)
				{
					return DynValue.FromObject(script, ModUtils.Client.GetSteamId(client));
				}
				throw new NotImplementedException();
			}

			// Token: 0x060096F3 RID: 38643 RVA: 0x003DA842 File Offset: 0x003D8A42
			public void SetValue(Script script, object obj, DynValue value)
			{
				throw new NotImplementedException();
			}
		}
	}
}
