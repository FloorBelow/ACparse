using System;
using System.IO;
using System.Collections.Generic;

namespace ACSharp.ResourceTypes {

	public class DataBlock : Resource {
		public const uint id = 2888548200;
		public uint[] files;
		public DataBlock(BinaryReader reader) {
			uint count = reader.ReadUInt32();
			files = new uint[count];
			for (int i = 0; i < count; i++) {
				if (Games.current == Games.AC2) reader.BaseStream.Seek(2, SeekOrigin.Current);
				files[i] = reader.ReadUInt32();
			}
		}
	}

	public class Entity : Resource {
		public const uint id = 159662430;
		public float[] transformationMatrix;
		public Visual visual;
		public GuidanceSystem guidanceSystem;
		public InertComponent inertComponent;
		public EntityDescriptor entityDescriptor;
		public Entity(BinaryReader reader) {
			transformationMatrix = new float[16];
			for (int i = 0; i < 16; i++) {
				transformationMatrix[i] = reader.ReadSingle();
			}
			if(Games.current == Games.AC1) {
				reader.BaseStream.Seek(6, SeekOrigin.Current);
				Resource resource = ForgeFile.ReadResource(reader);
				if (resource is MaterialOverrider) {
					//Console.WriteLine("Skipping material overrider");
					resource = ForgeFile.ReadResource(reader);
					//if (resource is Visual) Console.WriteLine("WE DID IT");
				}
				if (resource is Visual) {
					visual = (Visual)resource;
					resource = ForgeFile.ReadResource(reader);
					if (resource is GuidanceSystem) {
						guidanceSystem = (GuidanceSystem)resource;
						resource = ForgeFile.ReadResource(reader);
						if (resource is InertComponent) {
							//Console.WriteLine("FOUND COLLISION");
							inertComponent = (InertComponent)resource;
							resource = ForgeFile.ReadResource(reader);
							if (resource is EntityDescriptor) {
								entityDescriptor = (EntityDescriptor)resource;
								//Console.WriteLine("FOUND ENTITYDESCRIPTOR");
							}
						}
					}
				}
			} else {
				if (reader.ReadByte() != 3) return;
				uint componentCount = reader.ReadUInt32();
				for(int i = 0; i < componentCount; i++) {
					if (reader.ReadUInt16() != 4) return;
					Resource resource = ForgeFile.ReadResource(reader);
					if (resource is Visual) { 
						//Console.WriteLine("FOUND VISUAL"); 
						visual = (Visual)resource; 
					}
					else if (resource is GuidanceSystem) guidanceSystem = (GuidanceSystem)resource;
					else if (resource is InertComponent) inertComponent = (InertComponent)resource;
				}
				//read entitydescriptor
			}
			
		}

		public uint GetMeshID() {
			if (visual != null) {
				if (visual.lodSelectorInstance != null && visual.lodSelectorInstance.lod0 != null) return visual.lodSelectorInstance.lod0.meshID;
				if (visual.meshInstanceData != null) return visual.meshInstanceData.meshID;
			}
			return 0;
		}
	}

	public class EntityGroup : Resource {
		public const uint id = 1064578342;
		public List<Entity> entities;
		//This is gonna be fucked up but just roll with it, okay
		public EntityGroup(BinaryReader reader) {
			entities = new List<Entity>();
			byte[] checkBytes = new byte[4] { 0x5e, 0x41, 0x84, 0x09 };
			reader.BaseStream.Seek(103, SeekOrigin.Current);
			byte checkPos = 0;
			while (reader.BaseStream.Position < reader.BaseStream.Length) {
				if (reader.ReadByte() == checkBytes[checkPos]) {
					checkPos++;
					//Found entity
					if (checkPos == 4) {
						checkPos = 0;
						//Console.WriteLine("FOUND ENTITY IDENTIFIER");
						reader.BaseStream.Seek(-8, SeekOrigin.Current);
						Resource r = ForgeFile.ReadResource(reader);
						if (r is Entity && ((Entity)r).visual != null) {
							//Console.WriteLine("ENTITY IS REAL & HAS VISUAL");
							entities.Add((Entity)r);
						}
					}
				} else checkPos = 0;
			}
		}
	}

	public class EntityDescriptor: Resource {
		public const uint id = 1611799198;
		public GameStateData gameStateData;
		public EntityDescriptor(BinaryReader reader) {
			reader.BaseStream.Seek(17, SeekOrigin.Current);
			gameStateData = ForgeFile.ReadResource<GameStateData>(reader);
		}
	}

	public class GameStateData : Resource {
		public const uint id = 1930654782;
		public long transformationMatrixOffset; //This only works if the reader's base stream is a single file, not some datafile nonsense
		public float[] transformationMatrix;
		public GameStateData(BinaryReader reader) {
			reader.BaseStream.Seek(4, SeekOrigin.Current);
			reader.BaseStream.Seek(reader.ReadUInt32(), SeekOrigin.Current);
			int data2Size = reader.ReadInt32();
			if(data2Size >= 66) {
				reader.BaseStream.Seek(4, SeekOrigin.Current);
				transformationMatrixOffset = reader.BaseStream.Position;
				transformationMatrix = new float[16];
				for (int i = 0; i < 16; i++) {
					transformationMatrix[i / 4 + (i % 4) * 4] = reader.ReadSingle();
				}
			}
		}
	}
}
