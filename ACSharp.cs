using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ACSharp {
	public enum Game : int {
		AC1 = 1,
		AC2 = 2,
		ACR = 3,
		ACFR = 4,

		AC3 = 5,
		ACV = 6,
		ACGA = 7,
		ACC = 8,

		ACU = 9,
		ACVI = 10,

		ACE = 11,
		ACD = 12,
		ACK = 13
    }

	public static class Games {
		public static Game current;
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
