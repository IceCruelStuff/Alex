using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Plants
{
	public class MelonBlock : Block
	{
		public MelonBlock() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Plants;
		}
	}
}