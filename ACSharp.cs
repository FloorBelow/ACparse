using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ACSharp {
	public struct Game {
		public string name;
		public int filedataLength;
		public int subfileHeaderSize;

		public static bool operator ==(Game g1, Game g2) { return g1.Equals(g2); }
		public static bool operator !=(Game g1, Game g2) { return !g1.Equals(g2); }

		public override bool Equals(object obj) {
			if (!(obj is Game)) return false;
			return ((Game)obj).name == name;
		}
	}

	public static class Games {
		public static Game current;
		public static readonly Game AC1 = new Game() { name = "AC1", filedataLength = 440, subfileHeaderSize = 8 };
		public static readonly Game AC2 = new Game() { name = "AC2", filedataLength = 440, subfileHeaderSize = 10 };
	}

	


	public static class ACConsole {
		public static string consoleText;
		public static void WriteLine(string text) { consoleText = consoleText == null ? text + "\n" : consoleText + text + "\n"; }

		public static void Assert(bool b, string message = null) {
			if (!b) {
				if (message != null) WriteLine("Assertion failed - " + message);
				else WriteLine("Assertion failed");
			}
		}
	}
}
