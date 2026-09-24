namespace AWSGateway.Utils
{
    public class FormatUtils
    {
        // pretvaramo broj bajtova
        public static string FormatFileSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };

            double size = bytes;
            int unitsCounter = 0;

            while (size >= 1024 && unitsCounter < units.Length - 1)
            {
                size = size / 1024;
                unitsCounter++;
            }

            return size.ToString("0.##") + " " + units[unitsCounter];
        }

        public static string FormatDuration(double totalSeconds)
        {
            if (totalSeconds < 60)
            {
                return totalSeconds.ToString("0.0") + "s";

            }

            int minutes = (int)(totalSeconds / 60);
            double remainingSeconds = totalSeconds % 60;

            return minutes + "m" + remainingSeconds.ToString("0") + "s";

        }

        // CSV polje - navodnici kad vrijednost sadrzi zarez, navodnik ili novi red (RFC 4180)
        public static string EscapeCsv(string value)
        {
            string safeValue = value;

            if (safeValue == null)
            {
                safeValue = string.Empty;
            }

            bool needsQuotes = safeValue.Contains(",") || safeValue.Contains("\"") || safeValue.Contains("\n") || safeValue.Contains("\r");

            if (needsQuotes)
            {
                string escaped = safeValue.Replace("\"", "\"\"");
                return "\"" + escaped + "\"";
            }

            return safeValue;
        }

        public class DateFormatter
        {
            public string FormatRelative(DateTime date)
            {
                TimeSpan difference = DateTime.Now - date;
                
                if (difference.TotalMinutes < 1)
                {
                    return "upravo sada";
                }
                if (difference.TotalHours < 1)
                {
                    return (int)difference.TotalMinutes + "min";
                }
                if (difference.TotalDays < 1)
                {
                    return (int)difference.TotalHours + "h";
                }


                int days = (int)difference.TotalDays;

                if (days == 1)
                {
                    return "1 dan";
                }

                return days + " dana";

            }
        }

    }
}
