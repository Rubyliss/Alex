namespace Alex.Blocks.Minecraft
{
	public class BrewingStand : Block
	{
		public BrewingStand() : base(4530)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			LightValue = 1;
		}
	}
}
