using System;
using System.IO;

namespace ACSharp.ResourceTypes {

	public class InertComponent : Resource {
		public const uint id = 240796938;
		public RigidBody rigidBody;

		public InertComponent(BinaryReader reader) {
			reader.BaseStream.Seek(9, SeekOrigin.Current);
			if (Games.current == Games.AC2) reader.BaseStream.Seek(2, SeekOrigin.Current);
			rigidBody = ForgeFile.ReadResource<RigidBody>(reader);
		}
	}

	public class RigidBody : Resource {
		public const uint id = 579813418;
		public CollisionFilterInfo collisionFilterInfo;
		public BoundingVolume boundingVolume;
		
		public RigidBody(BinaryReader reader) {
			collisionFilterInfo = ForgeFile.ReadResource<CollisionFilterInfo>(reader);
			reader.BaseStream.Seek(121, SeekOrigin.Current);
			if(Games.current == Games.AC1) //TODO ploblems with ac2 rigidbody
			boundingVolume = ForgeFile.ReadResource<BoundingVolume>(reader);
		}
	}

	public class CollisionFilterInfo : Resource {
		public const uint id = 1139775938;

		public CollisionFilterInfo(BinaryReader reader) {
			if (reader.ReadUInt32() == 11) reader.BaseStream.Seek(353, SeekOrigin.Current);
			reader.BaseStream.Seek(10, SeekOrigin.Current);
			if (Games.current == Games.AC2) reader.BaseStream.Seek(6, SeekOrigin.Current);
		}
	}

	public class BoundingVolume : Resource {
		public const uint id = 1256993910;
		float[] bounds;

		public BoundingVolume(BinaryReader reader) {
			bounds = new float[6];
			for(int i = 0; i < 6; i++) bounds[i] = reader.ReadSingle();
			reader.BaseStream.Seek(4, SeekOrigin.Current);
		}
	}
}
