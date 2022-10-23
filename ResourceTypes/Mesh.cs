using System;
using System.IO;

namespace ACSharp.ResourceTypes {
	public class Mesh : Resource {
		public const uint id = 1096652136;
		public CompiledMesh compiledMesh;
		public Mesh(BinaryReader reader) {
			reader.BaseStream.Seek(14, SeekOrigin.Current);
			Resource file = ForgeFile.ReadResource(reader);
			//compiledMesh = ForgeFile.ReadFile<CompiledMesh>(reader);
			if (file is CompiledMesh) compiledMesh = (CompiledMesh)file;
		}
	}

	public class CompiledMesh : Resource {
		public const uint id = 4238218645;
		public VERT[] verts;
		public ushort[] idx;
		public CompiledMesh(BinaryReader reader) {
			if(Games.current == Games.AC1) {
				reader.BaseStream.Seek(8, SeekOrigin.Current);
				uint vertTableWidth = reader.ReadUInt32();
				uint vertTableLength = reader.ReadUInt32();
				uint idxTableSize = reader.ReadUInt32();
				reader.BaseStream.Seek(20, SeekOrigin.Current);
				if (vertTableWidth != 24) {
					verts = new VERT[0];
					idx = new ushort[0];
					Console.WriteLine($"Unknown vert table width {vertTableWidth}");
					return;
				}
				verts = new VERT[vertTableLength / vertTableWidth];
				for (int i = 0; i < verts.Length; i++) verts[i] = new VERT(reader, vertTableWidth);
				idx = new ushort[idxTableSize / 2];
				for (int i = 0; i < idx.Length; i++) idx[i] = reader.ReadUInt16();
			} else {
				//AC2
				reader.BaseStream.Seek(14, SeekOrigin.Current);
				byte vertTableWidth = reader.ReadByte();
				reader.BaseStream.Seek(reader.ReadUInt32() * 32, SeekOrigin.Current); //primatives 1
				reader.BaseStream.Seek(reader.ReadUInt32() * 32, SeekOrigin.Current); //primatives 2
				uint vertTableLength = reader.ReadUInt32();
				verts = new VERT[vertTableLength / vertTableWidth];
				for (int i = 0; i < verts.Length; i++) verts[i] = new VERT(reader, vertTableWidth);
				uint idxTableSize = reader.ReadUInt32();
				idx = new ushort[idxTableSize / 2];
				for (int i = 0; i < idx.Length; i++) idx[i] = reader.ReadUInt16();
			}
			
		}

		public struct VERT {
			short x; short y; short z; short scale;
			public VERT(BinaryReader reader, uint stride) {
				if (stride == 20) {
					x = reader.ReadInt16();
					y = reader.ReadInt16();
					z = reader.ReadInt16();
					scale = reader.ReadInt16();
					reader.BaseStream.Seek(12, SeekOrigin.Current);
				} else if (stride == 24) {
					x = reader.ReadInt16();
					y = reader.ReadInt16();
					z = reader.ReadInt16();
					scale = reader.ReadInt16();
					reader.BaseStream.Seek(16, SeekOrigin.Current);
				} else {
					x = 0; y = 0; z = 0; scale = short.MaxValue;
				}

			}
			public float[] getPosition() {
				float factor = Math.Abs(scale / 262144f);
				return new float[3] { x * factor, y * factor, z * factor };
			}
		}
	}
}
