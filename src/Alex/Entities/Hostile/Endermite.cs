using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Endermite : HostileMob
	{
		public Endermite(World level) : base(level)
		{
			Height = 0.3;
			Width = 0.4;
		}
	}
}
