using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language {
    public static class Log {

        public static void Error(string message, string filename, int line, int position) {
            throw new Exception(String.Format("{0} @ {1}:{2}:{3}", message, filename, line, position));
        }

        public static void Warning(string message, string filename, int line, int position) {
            Console.WriteLine(String.Format("WARNING - {0} @ {1}:{2}:{3}", message, filename, line, position));
        }

    }
}
