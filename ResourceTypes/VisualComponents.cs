using System;
using System.IO;

namespace ACSharp.ResourceTypes {
	public class Visual : Resource {
		public const uint id = 3966078249;
		public LODSelector lodSelectorInstance;
		public MeshInstanceData meshInstanceData;

		public Visual(BinaryReader reader) {
			reader.BaseStream.Seek(6, SeekOrigin.Current);
			if (Games.current == Games.AC2) reader.BaseStream.Seek(2, SeekOrigin.Current);
			Resource resource = ForgeFile.ReadResource(reader);
			if (resource is LODSelector) lodSelectorInstance = (LODSelector)resource;
			else if (resource is MeshInstanceData) meshInstanceData = (MeshInstanceData)resource;
			reader.BaseStream.Seek(10, SeekOrigin.Current);
			if(Games.current == Games.AC1) reader.BaseStream.Seek(1, SeekOrigin.Current);
		}
	}

	public class LODSelector : Resource {
		public const uint id = 21197922;
		public MeshInstanceData lod0;
		public Resource1290E59E resourceID;

		public LODSelector(BinaryReader reader) {
			reader.BaseStream.Seek(5, SeekOrigin.Current);
			for (int i = 0; i < 5; i++) {
				if (reader.ReadByte() == 0) {
					Resource r = ForgeFile.ReadResource(reader);

					//MeshInstanceData meshInstanceData = ForgeFile.ReadFile<MeshInstanceData>(reader);
					if (lod0 == null) {
						if (r is MeshInstanceData) lod0 = (MeshInstanceData)r;
						else if (r is Resource1290E59E) resourceID = (Resource1290E59E)r;
					}
				}
			}
		}
	}

	public class MeshInstanceData : Resource {
		public const uint id = 1399756347;
		public uint meshID;
		public CompiledMeshInstance compiledMeshInstance;
		public MeshInstanceMaterialInfo[] matInfos;

		public MeshInstanceData(BinaryReader reader) {
			reader.BaseStream.Seek(1, SeekOrigin.Current);
			meshID = reader.ReadUInt32();
			if(Games.current == Games.AC1) {
				reader.BaseStream.Seek(4, SeekOrigin.Current);
				uint daCount = reader.ReadUInt32();
				for (int i = 0; i < daCount; i++) {
					reader.BaseStream.Seek(14, SeekOrigin.Current);
					if (reader.ReadByte() != 3) {
						//Should be its own resource type
						reader.BaseStream.Seek(8, SeekOrigin.Current);
						reader.BaseStream.Seek(reader.ReadUInt32(), SeekOrigin.Current);
					}
				}
				reader.BaseStream.Seek(1, SeekOrigin.Current);
				compiledMeshInstance = ForgeFile.ReadResource<CompiledMeshInstance>(reader);
			} else if (Games.current == Games.AC2) {
				reader.BaseStream.Seek(15, SeekOrigin.Current);
				uint count1 = reader.ReadUInt32();
				reader.BaseStream.Seek(count1 + 16, SeekOrigin.Current);
				uint matInfoCount = reader.ReadUInt32();
				matInfos = new MeshInstanceMaterialInfo[matInfoCount];
				for(int i = 0; i < matInfoCount; i++) {
					matInfos[i] = ForgeFile.ReadResource<MeshInstanceMaterialInfo>(reader);
				}
			}

		}
	}

	public class CompiledMeshInstance : Resource {
		public const uint id = 1130893339;

		public CompiledMeshInstance(BinaryReader reader) {
			reader.BaseStream.Seek(16, SeekOrigin.Current);
			reader.BaseStream.Seek(reader.ReadUInt32(), SeekOrigin.Current);
			reader.BaseStream.Seek(16, SeekOrigin.Current);
		}
	}

	public class MeshInstanceMaterialInfo: Resource {
		public const uint id = 2572942325;

		public MeshInstanceMaterialInfo(BinaryReader reader) {
			reader.BaseStream.Seek(16, SeekOrigin.Current);
		}
	}

	public class Resource1290E59E : Resource {
		public const uint id = 311485854;
		public uint meshID;

		public Resource1290E59E(BinaryReader reader) {
			reader.BaseStream.Seek(1, SeekOrigin.Current);
			meshID = reader.ReadUInt32();
		}
	}
}
