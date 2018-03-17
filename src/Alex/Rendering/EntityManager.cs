﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Alex.API.Graphics;
using Alex.Engine;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Graphics;
using Alex.Utils;
using Alex.Worlds;

using MiNET.Entities;
using MiNET.Net;
using MiNET.Utils;
using Veldrid;

namespace Alex.Rendering
{
    public class EntityManager : IDisposable
    {
		private ConcurrentDictionary<long, MiNET.Entities.Entity> Entities { get; }
		private ConcurrentDictionary<UUID, MiNET.Entities.Entity> EntityByUUID { get; }
		private GraphicsDevice Device { get; }

	    public int EntityCount => Entities.Count;
	    public int EntitiesRendered { get; private set; } = 0;
		private World World { get; }
	    public EntityManager(GraphicsDevice device, World world)
	    {
		    World = world;
		    Device = device;
			Entities = new ConcurrentDictionary<long, MiNET.Entities.Entity>();
			EntityByUUID = new ConcurrentDictionary<UUID, MiNET.Entities.Entity>();
	    }

	    public void Update(GameTime gameTime)
	    {
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities)
		    {
				entity.GetModelRenderer()?.Update(Device, gameTime, entity.KnownPosition.ToXnaVector3(), entity.KnownPosition.Yaw, entity.KnownPosition.Pitch);
		    }
	    }

	    public void Render(IRenderArgs args, Camera.Camera camera)
	    {
		    int renderCount = 0;
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities)
		    {
			    var entityBox = entity.GetBoundingBox();

				if (camera.BoundingFrustum.Contains(new Veldrid.Utilities.BoundingBox(entityBox.Min, entityBox.Max)) != Veldrid.Utilities.ContainmentType.Disjoint)
			    {
				    entity.GetModelRenderer()?.Render(args, camera, entity.KnownPosition.ToXnaVector3(), entity.KnownPosition.Yaw, entity.KnownPosition.Pitch);
				    renderCount++;
			    }
		    }

		    EntitiesRendered = renderCount;
	    }

	    public void Render2D(IRenderArgs args, Camera.Camera camera)
	    {
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities.Where(x =>
			    x.IsShowName && !string.IsNullOrWhiteSpace(x.NameTag) &&
			    (x.IsAlwaysShowName || Vector3.Distance(camera.Position, x.KnownPosition.ToXnaVector3()) < 16f)))
		    {
			    var entityBox = entity.GetBoundingBox();

			    if (camera.BoundingFrustum.Contains(
				        new Veldrid.Utilities.BoundingBox(entityBox.Min, entityBox.Max)) !=
			        Veldrid.Utilities.ContainmentType.Disjoint)
			    {
				    entity.RenderNametag(args, camera);
			    }
		    }
	    }

	    public void Dispose()
	    {
		    
	    }

	    public void UnloadEntities(ChunkCoordinates coordinates)
	    {
		    foreach (var entity in Entities.ToArray())
		    {
			    if (new ChunkCoordinates(entity.Value.KnownPosition).Equals(coordinates))
			    {
					Remove(entity.Value.GetUUID());
			    }
		    }
	    }

	    private void Remove(UUID entity, bool removeId = true)
	    {
		    if (EntityByUUID.TryRemove(entity, out Entity e))
		    {
			    if (removeId)
			    {
				    Entities.TryRemove(e.EntityId, out e);
			    }

			    e.DeleteData();
		    }
	    }

	    public bool AddEntity(long id, MiNET.Entities.Entity entity)
	    {
		    if (EntityByUUID.TryAdd(entity.GetUUID(), entity))
		    {
			    entity.IsAlwaysShowName = false;
			    entity.NameTag = $"Entity_{id}";
			    entity.HideNameTag = false;

			    if (!Entities.TryAdd(id, entity))
			    {
				    EntityByUUID.TryRemove(entity.GetUUID(), out Entity _);
				    return false;
			    }

			    return true;
		    }

		    return false;
	    }

	    public void Remove(long id)
	    {
		    if (Entities.TryRemove(id, out Entity entity))
		    {
				Remove(entity.GetUUID(), false);
		    }
	    }
    }
}
