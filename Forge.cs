using ACSharp.ResourceTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

namespace ACSharp {
	public class Forge {

		Game game;
		public string filePath;
		public string name;
		uint version;
		uint entryCount;

		//TODO DEPRECATE - USED IN WRITING
		long idxTableOffset;
		long idxTableLength;
		long nameTableOffset;
		long nameTableLength;

		FILETABLE[] fileTables;
		public DATAFILEENTRY[] datafileTable;
		//IDXENTRY[] idxTable;
		//NAMEENTRY[] nameTable

		//maybe?
		bool edited;

		struct FILETABLE {
			public uint datafileCount;
			public uint unk1;
			public long indexTableOffset;
			public long nextFileTableOffset;
			public uint datafileStart;
			public uint datafileEnd;
			public long nameTableOffset;
        }

		public struct DATAFILEENTRY {
			public long dataOffset;
			public uint id;
			public uint dataSize;
			public string name;
		}
		/*
		public struct IDXENTRY {
			public long dataOffset;
			public uint id;
			public uint dataSize;
		}

		struct NAMEENTRY {
			uint dataSize;
			uint id;
			string name;
		}
		*/

		public Forge(string path, Game game = new Game()) {
			if (game.name == null) game = Games.AC1;
			Games.current = game;
			this.game = game;
			filePath = path;
			name = Path.GetFileNameWithoutExtension(path);
			Console.WriteLine("Opening " + name);
			BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));
			
			ACConsole.Assert(String.Concat(reader.ReadChars(8)) == "scimitar", "not a forge file");
			reader.BaseStream.Seek(1, SeekOrigin.Current);
			version = reader.ReadUInt32();


			//CUSTOM STUFF
			//reader.BaseStream.Seek(64, SeekOrigin.Begin);
			//if(reader.ReadUInt32() == 887)

			//DataHeader
			reader.BaseStream.Seek(reader.ReadUInt32(), SeekOrigin.Begin);
			entryCount = reader.ReadUInt32();
			datafileTable = new DATAFILEENTRY[entryCount];

			reader.BaseStream.Seek(24, SeekOrigin.Current);
			fileTables = new FILETABLE[reader.ReadUInt32()];
			long nextTableOffset = reader.ReadUInt32();

			for (int fileTable = 0; fileTable < fileTables.Length; fileTable++) {
				reader.BaseStream.Seek(nextTableOffset, SeekOrigin.Begin);
				fileTables[fileTable] = new FILETABLE() {
					datafileCount = reader.ReadUInt32(),
					unk1 = reader.ReadUInt32(),
					indexTableOffset = reader.ReadInt64(),
					nextFileTableOffset = reader.ReadInt64(),
					datafileStart = reader.ReadUInt32(),
					datafileEnd = reader.ReadUInt32(),
					nameTableOffset = reader.ReadInt64()
				};
				nextTableOffset = fileTables[fileTable].nextFileTableOffset;


				reader.BaseStream.Seek(fileTables[fileTable].indexTableOffset, SeekOrigin.Begin);
				for (uint i =0; i < fileTables[fileTable].datafileCount; i++) {
					datafileTable[i + fileTables[fileTable].datafileStart] = new DATAFILEENTRY() { dataOffset = reader.ReadInt64(), id = reader.ReadUInt32(), dataSize = reader.ReadUInt32() };
				}

				reader.BaseStream.Seek(fileTables[fileTable].nameTableOffset, SeekOrigin.Begin);
				for (uint i = 0; i < fileTables[fileTable].datafileCount; i++) {
					reader.BaseStream.Seek(44, SeekOrigin.Current);
					datafileTable[i + fileTables[fileTable].datafileStart].name = String.Concat(reader.ReadChars(128));
					datafileTable[i + fileTables[fileTable].datafileStart].name = datafileTable[i].name.TrimEnd('\0');
					//TODO does this break stuff on export?
					if (datafileTable[i + fileTables[fileTable].datafileStart].name == "") datafileTable[i + fileTables[fileTable].datafileStart].name = datafileTable[i + fileTables[fileTable].datafileStart].id.ToString();
					reader.BaseStream.Seek(16, SeekOrigin.Current);
				}
			}

			//idxTableLength = reader.BaseStream.Position - idxTableOffset;
			//nameTableLength = reader.BaseStream.Position - nameTableOffset;
			//Console.WriteLine($"IDXTABLE  {idxTableLength}");
			//Console.WriteLine($"NAMETABLE {nameTableLength}");
			reader.Close();
		}

		public int IndexOf(string startswith) {
			for(int i = 0; i < datafileTable.Length; i++) {
				if (datafileTable[i].name.StartsWith(startswith)) return i;
			}
			return 0;
		}


		public struct EntityEditEntry {
			public int datafile;
			public long datafileOffset;
			public long transformOffset;
			//public uint id;
			public float x;
			public float y;
			public float z;
		}

		public void EditEntity(EntityEditEntry editEntry) {
			long offset = editEntry.datafileOffset + editEntry.transformOffset;
			int datafileIndex = -1;
			for(int i = 0; i < datafileTable.Length; i++) {
				if(datafileTable[i].id == editEntry.datafile) {
					datafileIndex = i;
					break;
				}
			}
			byte[] data = DecompressDatafile(datafileIndex);
			byte[] xBytes = BitConverter.GetBytes(editEntry.x);
			byte[] yBytes = BitConverter.GetBytes(editEntry.y);
			byte[] zBytes = BitConverter.GetBytes(editEntry.z);
			Console.WriteLine($"datafile:  {editEntry.datafile}");
			Console.WriteLine($"datafileoffset:  {editEntry.datafileOffset}");
			Console.WriteLine($"transformOffset: {editEntry.transformOffset}");
			Console.WriteLine($"x: {offset + 12} - {editEntry.x}");
			Console.WriteLine($"y: {offset + 28} - {editEntry.y}");
			Console.WriteLine($"z: {offset + 44} - {editEntry.z}");
			Array.Copy(xBytes, 0, data, offset + 12, 4);
			Array.Copy(yBytes, 0, data, offset + 28, 4);
			Array.Copy(zBytes, 0, data, offset + 44, 4);
			RewriteDatafile(datafileIndex, data);
		}

		public void RewriteDatafile(int i, byte[] uncompressedData) {

			//AC2 stuff
			uint fileheaderextralength = 0;
			byte[] fileheaderextra = new byte[0];

			//This is the part we start writing the new file from
			BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open));
			long length = reader.BaseStream.Length;
			reader.BaseStream.Seek(datafileTable[i].dataOffset, SeekOrigin.Begin);
			byte[] fileHeader = reader.ReadBytes(game.filedataLength);
			if(Games.current == Games.AC2) {
				fileheaderextralength = reader.ReadUInt32();
				fileheaderextra = reader.ReadBytes((int)fileheaderextralength * 8);
			}
			reader.Close();

			//File id+size in the first compressed data section, actual data in the second
			ushort subfileCount = BitConverter.ToUInt16(uncompressedData, 0);
			//Console.WriteLine(subfileCount);
			//Console.WriteLine(2 + subfileCount * 8);

			//BinaryWriter writer = new BinaryWriter(File.Open("testnewdatafile.ac1", FileMode.Create));
			MemoryStream newDatafileStream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(newDatafileStream);
			writer.Write(fileHeader);
			if(Games.current == Games.AC2) {
				writer.Write(fileheaderextralength);
				writer.Write(fileheaderextra);
			}
			int firstBlockLength = 2 + subfileCount * game.subfileHeaderSize;
			if(game == Games.AC2) {
				Console.WriteLine(firstBlockLength);
				int extraCount = BitConverter.ToInt32(uncompressedData, firstBlockLength);
				firstBlockLength = firstBlockLength + 4 + extraCount;
				Console.WriteLine(firstBlockLength);
			}
			WriteCompressedBlock(writer, uncompressedData.Take(firstBlockLength).ToArray());
			WriteCompressedBlock(writer, uncompressedData.Skip(firstBlockLength).ToArray());
			writer.Flush();
			byte[] newDatafile = newDatafileStream.ToArray();
			writer.Close();
			//byte[] data = reader.ReadBytes((int)datafileTable[i].dataSize + 440);

			Console.WriteLine("SIZE COMPARISON");
			Console.WriteLine(datafileTable[i].dataSize);
			Console.WriteLine(newDatafile.Length);

			writer = new BinaryWriter(File.Open(filePath, FileMode.Open));
			writer.BaseStream.Seek(idxTableOffset + 16 * i, SeekOrigin.Begin);
			writer.Write(length);
			writer.BaseStream.Seek(4, SeekOrigin.Current);
			writer.Write(newDatafile.Length - game.filedataLength);
			writer.BaseStream.Seek(length, SeekOrigin.Begin);
			writer.Write(newDatafile);
			writer.Write(new byte[256]);

			//FOR TESTING, ZERO ORIGINAL DATAFILE
			//writer.BaseStream.Seek(datafileTable[i].dataOffset, SeekOrigin.Begin);
			//writer.Write(new byte[datafileTable[i].dataSize + 440]);

			writer.Flush();
			writer.Close();


			//File.WriteAllBytes(datafileTable[i].name + ".ac1", uncompressedData.ToArray());
			//WRITE UNCHANGED FILE
			/*
			BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Open));
			writer.BaseStream.Seek(idxTableOffset + 16 * i, SeekOrigin.Begin);
			writer.Write(length);
			writer.BaseStream.Seek(length, SeekOrigin.Begin);
			writer.Write(data);
			writer.Write(new byte[256]);
			writer.Flush();
			writer.Close();
			*/
			//reader.BaseStream.Seek(nameTableOffset + 188*i + 44, SeekOrigin.Begin);
			//Console.WriteLine(String.Concat(reader.ReadChars(128)));

			//reader.BaseStream.Seek(idxTableOffset + 16 * i, SeekOrigin.Begin);
			//Console.WriteLine(reader.BaseStream.Position);
		}

		void WriteCompressedBlock(BinaryWriter writer, byte[] data) {
			writer.Write(new byte[] { 0x33, 0xAA, 0xFB, 0x57, 0x99, 0xFA, 0x04, 0x10, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00, 0x80 });
			ushort chunkCount = (ushort) ((data.Length / 32768) + 1);
			writer.Write(chunkCount);
			Console.WriteLine(chunkCount);
			for(int i = 0; i < chunkCount - 1; i++) {
				writer.Write((ushort)32768);
				writer.Write((ushort)32768);
			}
			ushort lastChunkSize = (ushort)(data.Length % 32768);
			Console.WriteLine(lastChunkSize);
			Console.WriteLine(data.Length);
			if(lastChunkSize != 0) {
				writer.Write(lastChunkSize);
				writer.Write(lastChunkSize);
			}
			if(chunkCount > 1) {
				byte[] fullchunk = new byte[32768];
				for (int i = 0; i < chunkCount - 1; i++) {
					Console.WriteLine("Writing full chunk");
					Array.Copy(data, 32768 * i, fullchunk, 0 , 32768);
					writer.Write(new byte[] { 0xff, 0xff, 0xff, 0xff });
					writer.Write(fullchunk);
				}
			}
			if(lastChunkSize != 0) {
				Console.WriteLine("Writing last chunk");
				byte[] lastChunk = new byte[lastChunkSize];
				Array.Copy(data, 32768 * (chunkCount - 1), lastChunk, 0, lastChunkSize);
				writer.Write(new byte[] { 0xff, 0xff, 0xff, 0xff });
				writer.Write(lastChunk);
			}
			//writer.Write((ushort) ();
		}

		public byte[] DecompressDatafile(int datafileIndex) {
			BinaryReader r = new BinaryReader(File.Open(filePath, FileMode.Open));
			r.BaseStream.Seek(datafileTable[datafileIndex].dataOffset + game.filedataLength, SeekOrigin.Begin);
			if(game == Games.AC2) r.BaseStream.Seek(r.ReadUInt32() * 8, SeekOrigin.Current);
			byte[] uncompressedData = DecompressDatafile(r);
			r.Close();
			return uncompressedData;
		}

		byte[] DecompressDatafile(BinaryReader r) {
			//Decompress

			List<byte> uncompressedData = new List<byte>();
			byte b2 = r.ReadByte();
			if (b2 != 51) {
				Console.WriteLine(b2);
				r.Close();
				return new byte[0];
			}
			r.BaseStream.Seek(-1, SeekOrigin.Current);
			for (int b = 0; b < 2; b++) {
				//Compressed block
				r.BaseStream.Seek(10, SeekOrigin.Current);
				byte compression = r.ReadByte();
				r.BaseStream.Seek(4, SeekOrigin.Current);
				ushort chunkCount = r.ReadUInt16();
				//if (chunkCount == 1) Console.WriteLine(datafileTable[datafileIndex].name);
				ushort[] uncompressedSizes = new ushort[chunkCount];
				ushort[] compressedSizes = new ushort[chunkCount];
				for (int c = 0; c < chunkCount; c++) {
					uncompressedSizes[c] = r.ReadUInt16();
					compressedSizes[c] = r.ReadUInt16();
					//Console.WriteLine($"{compressedSizes[c]} - {uncompressedSizes[c]}");
					//if (uncompressedSizes[c] > 30000) Console.WriteLine(uncompressedSizes[c]);
					/*
					if(uncompressedSizes[c] > testLargestUncompressedBlockSize) {
						testLargestUncompressedBlockSize = uncompressedSizes[c];
						Console.WriteLine($"largest UNCOMPRESSED: {testLargestUncompressedBlockSize}");
					}
					if (compressedSizes[c] > testLargestCompressedBlockSize) {
						testLargestCompressedBlockSize = compressedSizes[c];
						Console.WriteLine($"largest   COMPRESSED: {testLargestCompressedBlockSize}");
					}
					*/
				}
				for (int c = 0; c < chunkCount; c++) {
					r.BaseStream.Seek(4, SeekOrigin.Current);
					if (uncompressedSizes[c] == compressedSizes[c]) uncompressedData.AddRange(r.ReadBytes(compressedSizes[c]));
					else uncompressedData.AddRange(Compression.Decompress(r.ReadBytes(compressedSizes[c]), compression, uncompressedSizes[c]));
				}
			}
			
			return uncompressedData.ToArray();
		}

		public enum ReadResourceType {
			Full,
			Empty,
			Raw
        }

		public ForgeFile[] OpenDatafile(int datafileIndex, Dictionary<uint, ForgeFile> fileDict = null, ReadResourceType readType = ReadResourceType.Full) {
			byte[] uncompressedData = DecompressDatafile(datafileIndex);
			if (uncompressedData.Length == 0) return null;
			BinaryReader r = new BinaryReader(new MemoryStream(uncompressedData));
			ushort fileCount = r.ReadUInt16();
			ForgeFile[] files = new ForgeFile[fileCount];
			uint[] sizes = new uint[fileCount];
			//Subfile headers
			for(int i = 0; i < fileCount; i++) {
				files[i] = new ForgeFile(this, datafileTable[datafileIndex].id, r.ReadUInt32());
				sizes[i] = r.ReadUInt32();
				if (game == Games.AC2) r.BaseStream.Seek(2, SeekOrigin.Current);
			}

			if (game == Games.AC2) {
				uint dataLayerSize = r.ReadUInt32();
				//if (dataLayerSize != 0) Console.WriteLine($"{datafileTable[datafileIndex].name} - {dataLayerSize}");
				r.BaseStream.Seek(dataLayerSize, SeekOrigin.Current);
			}

			//Subfiles
			long offset = r.BaseStream.Position;
			for(int i = 0; i < fileCount; i++) {
				files[i].fileType = r.ReadUInt32();
				int dataSize = r.ReadInt32();
				int nameLength = r.ReadInt32();

				//if(nameLength < 0) { Console.WriteLine("OHNO NAME LENGTH WRONG"); break; }
				//TODO does this break stuff on export?
				if (nameLength == 0) files[i].name = files[i].fileID.ToString();
				else files[i].name = String.Concat(r.ReadChars(nameLength));

				

				r.BaseStream.Seek(1, SeekOrigin.Current);
				if(files[i].fileType == Entity.id)
				//Console.WriteLine(files[i].name);
				//if(files[i].fileType == Mesh.id) Console.WriteLine($"{files[i].name} - {datafileTable[datafileIndex].name}");
				files[i].datafileOffset = r.BaseStream.Position;
				r.BaseStream.Seek(1, SeekOrigin.Current);
				if (readType == ReadResourceType.Full) files[i].resource = ForgeFile.ReadResource(r.ReadBytes(dataSize - 1));
				else if (readType == ReadResourceType.Raw) files[i].resource = new ResourceRawData(r.ReadBytes(dataSize - 1));
				offset += sizes[i];
				r.BaseStream.Seek(offset, SeekOrigin.Begin);
			}

			//populate dict
			if (fileDict != null) for (int i = 0; i < fileCount; i++) {
					if (fileDict.ContainsKey(files[i].fileID) && fileDict[files[i].fileID].name != files[i].name) Console.WriteLine($"ID COLLISION: {files[i].fileID} {fileDict[files[i].fileID].name}, {files[i].name}");
					fileDict[files[i].fileID] = files[i]; 
			}

			r.Close();
			return files;
		}


		//TODO NOTE NEITHER OF THESE WILL WORK IF THE NAME TABLE IS MORE THAN INT.MAX SIZE. SOMETHING TO WORRY ABOUT FOR LATER GAMES? THAT WOULD BE 2GB-ish
		public void SetModified() {
			if (edited) return;
			Console.WriteLine($"IDXTABLE  {idxTableLength}");
			Console.WriteLine($"NAMETABLE {nameTableLength}");
			//save original file tables
			BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open));
			reader.BaseStream.Seek(idxTableOffset, SeekOrigin.Begin);
			byte[] idxTable = reader.ReadBytes((int)idxTableLength);
			reader.BaseStream.Seek(nameTableOffset, SeekOrigin.Begin);
			byte[] nameTable = reader.ReadBytes((int)nameTableLength);
			reader.Close();
			edited = true;
			BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Open));
			writer.BaseStream.Seek(0, SeekOrigin.End);
			long idxPosition = writer.BaseStream.Position;
			writer.Write(idxTable);
			long namePosition = writer.BaseStream.Position;
			writer.Write(nameTable);
			writer.BaseStream.Seek(64, SeekOrigin.Begin);
			writer.Write((uint)887);
			writer.Write(idxPosition);
			writer.Write(idxTableLength);
			writer.Write(namePosition);
			writer.Write(nameTableLength);
			writer.Flush(); writer.Close();
		}

		public void Revert() {
			//if (!edited) return;
			BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open));
			reader.BaseStream.Seek(64, SeekOrigin.Begin);
			if (reader.ReadUInt32() != 887) return;
			long idxPosition = reader.ReadInt64();
			long idxSize = reader.ReadInt64();
			long namePosition = reader.ReadInt64();
			long nameSize = reader.ReadInt64();
			reader.BaseStream.Seek(idxPosition, SeekOrigin.Begin);
			byte[] origIdx = reader.ReadBytes((int)idxSize);
			reader.BaseStream.Seek(namePosition, SeekOrigin.Begin);
			byte[] origNames = reader.ReadBytes((int)nameSize);
			reader.Close();
			BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Open));
			writer.BaseStream.Seek(64, SeekOrigin.Begin);
			writer.BaseStream.Write(new byte[36], 0, 36);
			writer.BaseStream.Seek(idxTableOffset, SeekOrigin.Begin);
			writer.Write(origIdx);
			writer.BaseStream.Seek(nameTableOffset, SeekOrigin.Begin);
			writer.Write(origNames);
			writer.Flush();
			writer.BaseStream.SetLength(idxPosition); //TODO MAYBE SAVE ORIGINAL FILE SIZE IN HEADER
			writer.Close();
			edited = false;
		}
	}
}
