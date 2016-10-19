using System.Linq;


namespace Language {
    public static class StringExtensions {
        public static string Slice (this string str, int start, int? end=null, int? stride=null) {
            if ((end ?? 0) < 0)
                end = str.Length - end;
            int range = end == null ? str.Length - start : end ?? 0 - start;
            try {
                string slice = str.Substring(start, range);
                if (stride == null)
                    return slice;
                return slice.Where((character, index) => index % 2 == 0).ToString();
            } catch {
                return null;
            }
        }

    }
}