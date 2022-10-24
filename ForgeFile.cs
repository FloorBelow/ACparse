using System;
using System.Collections.Generic;
using System.IO;
using ACSharp.ResourceTypes;
using System.Reflection;

namespace ACSharp {
	public class ForgeFile {
		public Forge forge;
		public uint datafileID;
		public long datafileOffset;
		public uint fileID;
		public uint fileType;
		public string name;
		//public byte[] data;
		public Resource resource;

		public ForgeFile(Forge forge, uint datafileID, uint fileID) { this.forge = forge; this.datafileID = datafileID; this.fileID = fileID; }
		public static Resource ReadResource(byte[] bytes) { return ReadResource(new BinaryReader(new MemoryStream(bytes))); }

		public static T ReadResource<T>(BinaryReader reader) where T : Resource {
			Resource r = ReadResource(reader);
			if (r is T) return (T)r;
			throw new Exception($"Expected {typeof(T)}, not {r.GetType()}");
		}

		public static Resource ReadResource(BinaryReader reader) {
			reader.BaseStream.Seek(4, SeekOrigin.Current);
			uint type = reader.ReadUInt32();
			switch (type) {

				//datablock
				case DataBlock.id: return new DataBlock(reader);

				//art
				case Mesh.id: return new Mesh(reader);
				case CompiledMesh.id: return new CompiledMesh(reader);

				//entity
				case Entity.id: return new Entity(reader);
				case EntityGroup.id: return new EntityGroup(reader);
				case EntityDescriptor.id: return new EntityDescriptor(reader);
				case GameStateData.id: return new GameStateData(reader);

				//visual
				case Visual.id: return new Visual(reader);
				case LODSelector.id: return new LODSelector(reader);
				case MeshInstanceData.id: return new MeshInstanceData(reader);
				case CompiledMeshInstance.id: return new CompiledMeshInstance(reader);
				case MeshInstanceMaterialInfo.id: return new MeshInstanceMaterialInfo(reader);
				case Resource1290E59E.id: return new Resource1290E59E(reader);

				//guidance
				case GuidanceSystem.id: return new GuidanceSystem(reader);
				case GuidanceObject.id: return new GuidanceObject(reader);
				case EdgeFilter.id: return new EdgeFilter(reader);
				case PartNode.id: return new PartNode(reader);

				//collision
				case InertComponent.id: return new InertComponent(reader);
				case RigidBody.id: return new RigidBody(reader);
				case CollisionFilterInfo.id: return new CollisionFilterInfo(reader);
				case BoundingVolume.id: return new BoundingVolume(reader);

				//material override
				case MaterialOverrider.id: return new MaterialOverrider(reader);
				case OverrideDefinition.id: return new OverrideDefinition(reader);

				//terrain?
				case Resource2DE22F13.id: return new Resource2DE22F13(reader);

				default: return new ResourceUnknown(reader);
			}
		}
	}

	public abstract class Resource { }

	public class ResourceUnknown : Resource {
		//public byte[] data;
		public ResourceUnknown(BinaryReader reader) { //data = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)); }
		}
	}

	public class ResourceRawData : Resource {
		public byte[] data;
		public ResourceRawData(byte[] data) { this.data = data; }
    }
}