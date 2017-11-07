namespace LinkUs.CommandLine.Handlers
{
    public static class StringExtension
    {
        public static string ToNormalizedString(this object value)
        {
            if (value == null) {
                return "";
            }
            return value.ToString();
        }
    }
}