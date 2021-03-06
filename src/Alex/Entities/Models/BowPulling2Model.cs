


using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Models 
{

	public partial class BowPulling2Model : EntityModel
	{
		public BowPulling2Model()
		{
			Name = "geometry.bow_pulling_2";
			VisibleBoundsWidth = 1;
			VisibleBoundsHeight = 1;
			VisibleBoundsOffset = new Vector3(0f, 0f, 0f);
			Texturewidth = 0;
			Textureheight = 0;
			Bones = new EntityModelBone[1]
			{
				new EntityModelBone(){ 
					Name = "bow",
					Parent = "",
					Pivot = new Vector3(0f,0f,0f),
					Rotation = new Vector3(0f,0f,0f),
					NeverRender = false,
					Mirror = false,
					Reset = false,
					Cubes = new EntityModelCube[0]
				},
			};
		}

	}

}