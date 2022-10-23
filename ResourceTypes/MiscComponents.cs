using System;
using System.IO;

namespace ACSharp.ResourceTypes {

	public class MaterialOverrider : Resource {
		public const uint id = 2023832567;
		public MaterialOverrider(BinaryReader reader) {
			reader.BaseStream.Seek(5, SeekOrigin.Current);
			if (!(ForgeFile.ReadResource(reader) is OverrideDefinition)) Console.WriteLine("BROKEN");
		}
	}

	public class OverrideDefinition : Resource {
		public const uint id = 1241307071;
		public OverrideDefinition(BinaryReader reader) {
			reader.BaseStream.Seek(9, SeekOrigin.Current);
		}
	}

	public class Resource2DE22F13 : Resource {
		public const uint id = 321905197;

		public Resource2DE22F13(BinaryReader reader) {
			//Console.WriteLine("GROUND STUFF");
			reader.BaseStream.Seek(51, SeekOrigin.Current);
		}
	}
}