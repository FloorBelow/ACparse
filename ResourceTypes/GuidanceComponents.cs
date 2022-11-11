using System;
using System.IO;

namespace ACSharp.ResourceTypes {

	public class GuidanceSystem : Resource {
		public const uint id = 1437539390;
		public GuidanceObject[] guidanceObjects;
		public EdgeFilter edgeFilter;

		public GuidanceSystem(BinaryReader reader) {
			reader.BaseStream.Seek(1, SeekOrigin.Current);
			guidanceObjects = new GuidanceObject[reader.ReadUInt32()];
			for(int i = 0; i < guidanceObjects.Length; i++) {
				guidanceObjects[i] = ForgeFile.ReadResource<GuidanceObject>(reader);
			}
			reader.BaseStream.Seek(reader.ReadUInt32() * 2, SeekOrigin.Current);
			edgeFilter = ForgeFile.ReadResource<EdgeFilter>(reader);
		}
	}

	public class GuidanceObject : Resource {
		public const uint id = 877635161;

		public GuidanceObject(BinaryReader reader) {
			if(Games.current == Game.AC1) reader.BaseStream.Seek(45, SeekOrigin.Current);
			else reader.BaseStream.Seek(29, SeekOrigin.Current);
		}
	}

	public class EdgeFilter : Resource {
		public const uint id = 1352418642;
		public PartNode[] partNodes;

		public EdgeFilter(BinaryReader reader) {
			reader.BaseStream.Seek(31, SeekOrigin.Current);
			reader.BaseStream.Seek(reader.ReadUInt32() * 2, SeekOrigin.Current);
			reader.BaseStream.Seek(24, SeekOrigin.Current);
			partNodes = new PartNode[reader.ReadUInt32()];
			for (int i = 0; i < partNodes.Length; i++) {
				partNodes[i] = ForgeFile.ReadResource<PartNode>(reader);
			}
			reader.BaseStream.Seek(1, SeekOrigin.Current);
		}
	}

	public class PartNode : Resource {
		public const uint id = 1400117596;

		public PartNode(BinaryReader reader) {
			reader.BaseStream.Seek(12, SeekOrigin.Current);
		}
	}

}
