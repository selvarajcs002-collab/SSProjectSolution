using System;
using System.Text.RegularExpressions;

namespace SSProjectSolution.Utilities
{
    public static class NumberToWordsConverter
    {
        private static readonly string[] Units =
        {
            "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
            "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
        };

        private static readonly string[] Tens =
        {
            "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };

        public static string ConvertToWords(decimal amount)
        {
            if (amount < 0)
                return "Minus " + ConvertToWords(Math.Abs(amount));

            long totalPaise = (long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
            long rupees = totalPaise / 100;
            int paise = (int)(totalPaise % 100);

            if (rupees == 0 && paise == 0)
                return "Rupees Zero Only";

            string words;
            if (rupees > 0 && paise > 0)
                words = "Rupees " + ConvertNumberToWords(rupees) + " and " + ConvertNumberToWords(paise) + " Paise Only";
            else if (rupees > 0)
                words = "Rupees " + ConvertNumberToWords(rupees) + " Only";
            else
                words = "Paise " + ConvertNumberToWords(paise) + " Only";

            return Regex.Replace(words, @"\s+", " ").Trim();
        }

        private static string ConvertNumberToWords(long number)
        {
            if (number == 0)
                return "Zero";

            string words = "";

            if (number / 10000000 > 0)
            {
                words += ConvertNumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if (number / 100000 > 0)
            {
                words += ConvertNumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if (number / 1000 > 0)
            {
                words += ConvertNumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if (number / 100 > 0)
            {
                words += ConvertNumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                if (number < 20)
                    words += Units[number];
                else
                {
                    words += Tens[number / 10];
                    if (number % 10 > 0)
                        words += " " + Units[number % 10];
                }
            }

            return words.Trim();
        }
    }
}
