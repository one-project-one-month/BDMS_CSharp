namespace BDMS.Shared
{
    public class Functions
    {
        public static string NormalizeCodeSegment(string value)
        {
            var compact = value.Trim().Replace(" ", string.Empty);
            return string.IsNullOrWhiteSpace(compact)
                ? "UNKNOWN"
                : compact.ToUpperInvariant();
        }
    }
}
