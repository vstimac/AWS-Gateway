using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AWSGateway.Helpers;
using AWSGateway.Utils;

namespace AWSGateway.Models
{
    // jedan zapis iz TransferHistory
    public class TransferModels
    {
        public int Id { get; set; }
        public string Direction { get; set; }

        // SourcePath/DestPath ostaju radi starih zapisa; novi zapisi imaju S3 i lokalnu stranu u zasebnim stupcima
        public string SourcePath { get; set; }
        public string DestPath { get; set; }

        public string Bucket { get; set; }
        public string ObjectKey { get; set; }
        public string LocalPath { get; set; }

        // veličina datoteke koja se prenosila - poznata i za neuspjele prijenose
        public long FileSizeBytes { get; set; }

        // stvarno preneseno - 0 za neuspjele i otkazane
        public long BytesTransferred { get; set; }

        // null za zapise nastale prije bilježenja trajanja
        public double? DurationMs { get; set; }

        public string Status { get; set; }
        public string ErrorMessage { get; set; }

        // UTC u obliku "yyyy-MM-dd HH:mm:ss" - za prikaz se pretvara u lokalno vrijeme
        public string CreatedAt { get; set; }

        public string ProfileName { get; set; }
        public string AwsIdentity { get; set; }
        public string Username { get; set; }

        public TransferModels()
        {
            Id = 0;
            Direction = string.Empty;
            SourcePath = string.Empty;
            DestPath = string.Empty;
            Bucket = string.Empty;
            ObjectKey = string.Empty;
            LocalPath = string.Empty;
            FileSizeBytes = 0;
            BytesTransferred = 0;
            DurationMs = null;
            Status = string.Empty;
            ErrorMessage = string.Empty;
            CreatedAt = string.Empty;
            ProfileName = string.Empty;
            AwsIdentity = string.Empty;
            Username = string.Empty;
        }

        // DateTime.MinValue ako zapis nema ispravno vrijeme
        public DateTime GetCreatedAtLocal()
        {
            DateTime createdAtUtc;

            bool parsed = DateTime.TryParse(
                CreatedAt,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out createdAtUtc);

            if (parsed == false)
            {
                return DateTime.MinValue;
            }

            return createdAtUtc.ToLocalTime();
        }

        // brzina u MB/s; null ako trajanje nije poznato ili ništa nije preneseno
        public double? GetSpeedMBps()
        {
            if (DurationMs.HasValue == false || DurationMs.Value <= 0 || BytesTransferred <= 0)
            {
                return null;
            }

            double megabytes = BytesTransferred / 1024.0 / 1024.0;
            double seconds = DurationMs.Value / 1000.0;

            return megabytes / seconds;
        }

        // prikazni tekst prati odabrani jezik sučelja (LanguageHelper); sirova vrijednost Status u bazi ostaje nepromijenjena
        public string GetStatusDisplay()
        {
            switch (Status)
            {
                case "completed":
                    return LanguageHelper.Get("report_status_completed");
                case "failed":
                    return LanguageHelper.Get("report_status_failed");
                case "cancelled":
                    return LanguageHelper.Get("report_status_cancelled");
                default:
                    return Status;
            }
        }

        public string GetDirectionDisplay()
        {
            switch (Direction)
            {
                case "upload":
                    return LanguageHelper.Get("upload");
                case "download":
                    return LanguageHelper.Get("download");
                default:
                    return Direction;
            }
        }

        public string GetDurationDisplay()
        {
            return FormatDurationMs(DurationMs);
        }

        // "-" za nepoznato trajanje, milisekunde za vrlo kratke prijenose, inače sekunde
        // statička - koristi je i tablica izvještaja koja ima samo broj milisekundi
        public static string FormatDurationMs(double? durationMs)
        {
            if (durationMs.HasValue == false)
            {
                return "-";
            }

            double milliseconds = durationMs.Value;

            if (milliseconds < 1000)
            {
                return milliseconds.ToString("F0") + " ms";
            }

            double seconds = milliseconds / 1000.0;

            if (seconds < 60)
            {
                return seconds.ToString("F1") + " s";
            }

            return FormatUtils.FormatDuration(seconds);
        }

        public string GetSpeedDisplay()
        {
            double? speed = GetSpeedMBps();

            if (speed.HasValue == false)
            {
                return "-";
            }

            return speed.Value.ToString("F2") + " MB/s";
        }

        public string GetS3Path()
        {
            if (string.IsNullOrEmpty(Bucket))
            {
                return string.Empty;
            }

            return "s3://" + Bucket + "/" + ObjectKey;
        }

        // skraćeni prikaz ARN-a: "arn:aws:iam::123:user/ime" -> "ime", "arn:aws:sts::123:assumed-role/rola/sesija" -> "rola (rola)"
        public string GetAwsIdentityDisplay()
        {
            if (string.IsNullOrEmpty(AwsIdentity))
            {
                return string.Empty;
            }

            string[] parts = AwsIdentity.Split('/');

            if (AwsIdentity.Contains(":assumed-role/") && parts.Length >= 2)
            {
                return LanguageHelper.Format("report_identity_role_suffix", parts[1]);
            }

            return parts[parts.Length - 1];
        }
    }

    // filter izvještaja - isti filter koriste tablica, sažetak, PDF i CSV
    public class TransferReportFilter
    {
        // null = bez donje granice
        public DateTime? FromUtc { get; set; }

        // prazno = bez filtra
        public string Direction { get; set; }
        public string Status { get; set; }
        public string Bucket { get; set; }
        public string SearchText { get; set; }

        // prazno = svi korisnici
        public string Username { get; set; }

        // opis filtra čitljiv korisniku - ide u zaglavlje PDF-a
        public string Description { get; set; }

        public TransferReportFilter()
        {
            FromUtc = null;
            Direction = string.Empty;
            Status = string.Empty;
            Bucket = string.Empty;
            SearchText = string.Empty;
            Username = string.Empty;
            Description = string.Empty;
        }
    }

    // sažetak za skup prijenosa koji odgovara filtru
    public class TransferReportSummary
    {
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public int CancelledCount { get; set; }

        public long UploadedBytes { get; set; }
        public long DownloadedBytes { get; set; }

        // null ako nijedan uspješan prijenos nema poznato trajanje
        public double? AverageSpeedMBps { get; set; }

        public string MostCommonError { get; set; }
        public int MostCommonErrorCount { get; set; }

        public TransferReportSummary()
        {
            TotalCount = 0;
            CompletedCount = 0;
            FailedCount = 0;
            CancelledCount = 0;
            UploadedBytes = 0;
            DownloadedBytes = 0;
            AverageSpeedMBps = null;
            MostCommonError = string.Empty;
            MostCommonErrorCount = 0;
        }

        // udio uspješnih među prijenosima s poznatim ishodom
        public double GetSuccessPercent()
        {
            int finished = CompletedCount + FailedCount + CancelledCount;

            if (finished == 0)
            {
                return 0;
            }

            return CompletedCount * 100.0 / finished;
        }
    }
}