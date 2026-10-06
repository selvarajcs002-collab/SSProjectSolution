namespace SSProjectSolution.Services
{
    /// <summary>
    /// Resolves the GST number that belongs to one company.
    /// A GST value is returned only when the loaded row is the requested company.
    /// </summary>
    public static class CompanyGstResolver
    {
        public static string Resolve(int requestedCompanyId, int loadedCompanyId, string? loadedGst)
        {
            if (requestedCompanyId <= 0 || loadedCompanyId != requestedCompanyId)
                return string.Empty;

            return (loadedGst ?? string.Empty).Trim();
        }

        public static string Mask(string? gst)
        {
            if (string.IsNullOrWhiteSpace(gst))
                return "(empty)";

            var value = gst.Trim();
            if (value.Length <= 4)
                return "****";

            return string.Concat(value.AsSpan(0, 2), new string('*', value.Length - 4), value.AsSpan(value.Length - 2));
        }
    }
}
