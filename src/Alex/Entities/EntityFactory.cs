﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.Graphics.Models;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using fNbt.Tags;

using MiNET.Blocks;
using MiNET.Entities;
using MiNET.Net;
using MiNET.Utils;
using Veldrid;

namespace Alex.Entities
{
	public static class EntityFactory
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(EntityFactory));

		private static ConcurrentDictionary<string, Func<EntityModelRenderer>> _registeredRenderers =
			new ConcurrentDictionary<string, Func<EntityModelRenderer>>();

		public static void Load()
		{

		}

		public static bool TryLoadEntity(NbtCompound nbt, long entityId, out MiNET.Entities.Entity entity)
		{
			var id = nbt["id"].StringValue.Replace("minecraft:", "");
			var pos = nbt["Pos"];
			var rot = nbt["Rotation"];
			if (id != null && pos != null && Utils.EntityType.TryParse(id, true, out Utils.EntityType entityType))
			{
				var uuidLeast = nbt["UUIDLeast"].LongValue;
				var uuidMost = nbt["UUIDMost"].LongValue;

				Guid uuid = Extensions.GuidFromBits(uuidLeast, uuidMost);

				var renderer = GetEntityRenderer(id);
				if (renderer != null)
				{
					entity = entityType.Create(null);
					if (entity == null) return false;

					entity.EntityId = entityId;
					entity.SetUUID(new UUID(uuid.ToByteArray()));

					PlayerLocation position = new PlayerLocation((float) pos[0].DoubleValue, (float) pos[1].DoubleValue,
						(float) pos[2].DoubleValue, rot[0].FloatValue, rot[0].FloatValue, rot[1].FloatValue);

					entity.KnownPosition = position;

					entity.SetModelRenderer(renderer);

					return true;
				}
			}

			entity = null;
			return false;
		}

		public static EntityModelRenderer GetEntityRenderer(string name)
		{
			if (_registeredRenderers.TryGetValue(name, out var func))
			{
				return func();
			}

			return null;
		}

		public static void LoadModels(ResourceManager resourceManager, GraphicsDevice graphics, bool replaceModels)
		{
			foreach (var def in resourceManager.BedrockResourcePack.EntityDefinitions)
			{
				try
				{
					if (def.Value.Textures == null) continue;
					if (def.Value.Geometry == null) continue;
					if (def.Value.Textures.Count == 0) continue;
					if (def.Value.Geometry.Count == 0) continue;

					EntityModel model;
					if (resourceManager.BedrockResourcePack.EntityModels.TryGetValue(def.Value.Geometry.FirstOrDefault().Value,
						out model))
					{
						if (model != null)
						{
							var textures = def.Value.Textures;
							if (resourceManager.BedrockResourcePack.Textures.TryGetValue(textures.FirstOrDefault().Value,
								out Bitmap bmp))
							{
								var texture = TextureUtils.BitmapToTexture2D(graphics, bmp);

								string name = def.Key.Replace("definition.", "", StringComparison.InvariantCultureIgnoreCase).Replace("_", "");

								_registeredRenderers.AddOrUpdate(name,
									() => new EntityModelRenderer(model, texture),
									(s, func) =>
									{
										return () => new EntityModelRenderer(model, texture);
									});
							}
						}
					}


				}
				catch (Exception ex)
				{
					Log.Warn($"Failed to load model {def.Key}!", ex);
				}
			}

			return;
		}
	}
}
