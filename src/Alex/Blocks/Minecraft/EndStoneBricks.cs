namespace Alex.Blocks.Minecraft
{
	public class EndStoneBricks : Block
	{
		public EndStoneBricks() : base(8067)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			LightOpacity = 16;
		}
	}
}
