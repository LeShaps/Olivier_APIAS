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
    }
}
