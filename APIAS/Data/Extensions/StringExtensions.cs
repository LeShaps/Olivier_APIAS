namespace APIAS.Utilities
{
    public static class StringExtensions
    {
        public static string ReplaceAll(this string ToClean, string Replacement, params string[] ToReplace)
        {
            foreach (string ToRep in ToReplace)
            {
                ToClean = ToClean.Replace(ToRep, Replacement);
            }

            return ToClean;
        }

        public static bool IsNullOrEmpty(this string ToTest)
        {
            return ToTest == null || ToTest == "";
        }
    }
}
