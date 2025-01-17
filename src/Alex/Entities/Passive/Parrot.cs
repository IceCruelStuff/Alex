using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class Parrot : TameableMob
	{
		/*private ParrotType _variant = ParrotType.RedBlue;

		public ParrotType Variant
		{
			get
			{
				return _variant;
			}
			set
			{
				_variant = value;

				string texture = _variant.ToString().ToLower();

				if (_variant == ParrotType.RedBlue)
					texture = "red_blue";
				else if (_variant == ParrotType.YellowBlue)
					texture = "yellow_blue";

				//TryUpdateTexture("minecraft:parrot", texture);
			}
		}*/

		public ParrotType ParrotVariant => (ParrotType) base.Variant;

		public Parrot(World level) : base((EntityType)0, level)
		{
			Height = 0.9;
			Width = 0.5;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 19 && entry is MetadataVarInt varint)
			{
				Variant = varint.Value;
			}
		}

		public enum ParrotType
		{
			RedBlue = 0,
			Blue = 1,
			Green = 2,
			YellowBlue = 3,
			Grey = 4
		}
	}
}