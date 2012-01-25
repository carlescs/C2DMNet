using System.Collections.Generic;
using System.IO;

namespace C2DMNet.Util
{
    static class StreamReaderExtensions
    {
        public static IEnumerable<string> ReadLines(this StreamReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}