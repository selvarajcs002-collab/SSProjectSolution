using System;
using System.Text.RegularExpressions;

namespace SSProjectSolution.Utilities
{
    /// <summary>
    /// Outward DC numbers use SSE-{seq}/{year}-{year+1}, for example SSE-1014/2026-2027.
    /// Sequence is global for the financial year, not per company.
    /// </summary>
    public static class OutwardDcNumber
    {
        public const string Prefix = "SSE-";
        public const int SeedLastAllocated = 1013;
        public const string DocumentType = "OUTWARD";

        private static readonly Regex Pattern = new(
            @"^SSE-(\d{4,})/(\d{4})-(\d{4})$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string CurrentFinancialYear(DateTime? now = null)
        {
            var date = now ?? DateTime.Now;
            return $"{date.Year}-{date.Year + 1}";
        }

        public static string Format(int sequence, string financialYear)
        {
            if (sequence <= 0)
                throw new ArgumentOutOfRangeException(nameof(sequence), "Sequence must be greater than zero.");

            if (string.IsNullOrWhiteSpace(financialYear))
                throw new ArgumentException("Financial year is required.", nameof(financialYear));

            var seqText = sequence < 10000
                ? sequence.ToString("D4")
                : sequence.ToString();

            return $"{Prefix}{seqText}/{financialYear.Trim()}";
        }

        public static bool TryParse(string? dcNo, out int sequence, out string financialYear)
        {
            sequence = 0;
            financialYear = string.Empty;

            if (string.IsNullOrWhiteSpace(dcNo))
                return false;

            var match = Pattern.Match(dcNo.Trim());
            if (!match.Success)
                return false;

            if (!int.TryParse(match.Groups[1].Value, out sequence) || sequence <= 0)
                return false;

            var yearCurrent = match.Groups[2].Value;
            var yearNext = match.Groups[3].Value;
            if (!int.TryParse(yearCurrent, out var y1) || !int.TryParse(yearNext, out var y2) || y2 != y1 + 1)
                return false;

            financialYear = $"{yearCurrent}-{yearNext}";
            return true;
        }

        public static int NextSequence(int lastAllocated)
        {
            if (lastAllocated < SeedLastAllocated)
                lastAllocated = SeedLastAllocated;

            return lastAllocated + 1;
        }

        public static bool IsReuseAuthorized(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;

            return string.Equals(role.Trim(), "Administrator", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role.Trim(), "Admin", StringComparison.OrdinalIgnoreCase);
        }

        public static string ValidateReuse(string? dcNo, string? status, bool isActiveOnLiveTable, string? requestedFinancialYear, string? numberFinancialYear, string? reason, string? role)
        {
            if (!IsReuseAuthorized(role))
                return "You are not authorized to reuse a deleted DC number.";

            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 3)
                return "A reuse reason of at least 3 characters is required.";

            if (!TryParse(dcNo, out _, out var fy))
                return "The requested DC number is not a valid outward DC number.";

            if (!string.IsNullOrWhiteSpace(requestedFinancialYear)
                && !string.Equals(requestedFinancialYear.Trim(), fy, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(requestedFinancialYear.Trim(), numberFinancialYear?.Trim(), StringComparison.OrdinalIgnoreCase))
                return "The requested DC number belongs to a different financial year.";

            if (isActiveOnLiveTable)
                return "That DC number is already used by an active outward.";

            if (!string.Equals(status, "Deleted", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(status))
                    return "That DC number was never issued and cannot be reused.";

                if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(status, "Allocated", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(status, "Reserved", StringComparison.OrdinalIgnoreCase))
                    return "That DC number is not eligible for reuse.";

                if (string.Equals(status, "Reused", StringComparison.OrdinalIgnoreCase))
                    return "That DC number has already been reused.";

                return "That DC number is not eligible for reuse.";
            }

            return string.Empty;
        }
    }
}
