using AWSGateway.Models;
using AWSGateway.Utils;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace AWSGateway.Services
{
    // PDF izvještaj za isti skup prijenosa i isti sažetak koji su prikazani u formi -
    // generator ne čita bazu, pa PDF uvijek odgovara odabranom filtru
    public class PdfReportGenerator
    {
        private const float BodyFontSize = 9;
        private const float SmallFontSize = 8;

        public void GenerateTransferReport(string outputPath, List<TransferModels> transfers, TransferReportSummary summary, TransferReportFilter filter, bool includeUserColumn)
        {
            using (PdfWriter writer = new PdfWriter(outputPath))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf, PageSize.A4.Rotate()))
            {
                // Arial iz Windows fontova - ugrađeni PDF fontovi nemaju hrvatske znakove
                string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                PdfFont font = PdfFontFactory.CreateFont(System.IO.Path.Combine(fontsFolder, "arial.ttf"), PdfEncodings.IDENTITY_H);
                PdfFont boldFont = PdfFontFactory.CreateFont(System.IO.Path.Combine(fontsFolder, "arialbd.ttf"), PdfEncodings.IDENTITY_H);

                document.SetFont(font);
                document.SetFontSize(BodyFontSize);

                AddHeader(document, boldFont, filter, transfers);
                AddSummary(document, boldFont, summary);
                AddTransfersByBucket(document, boldFont, transfers, includeUserColumn);
            }
        }

        private void AddHeader(Document document, PdfFont boldFont, TransferReportFilter filter, List<TransferModels> transfers)
        {
            Paragraph title = new Paragraph("AWS Gateway - izvještaj prijenosa");
            title.SetFont(boldFont);
            title.SetFontSize(15);
            title.SetTextAlignment(TextAlignment.CENTER);
            document.Add(title);

            Paragraph generated = new Paragraph("Generirano: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            generated.SetTextAlignment(TextAlignment.CENTER);
            document.Add(generated);

            document.Add(new Paragraph("Filter: " + filter.Description));

            string identities = BuildIdentityList(transfers);

            if (string.IsNullOrEmpty(identities) == false)
            {
                document.Add(new Paragraph("AWS identiteti: " + identities));
            }
        }

        // različiti AWS identiteti koji se pojavljuju u izvještaju (npr. IAM korisnik i rola)
        private string BuildIdentityList(List<TransferModels> transfers)
        {
            List<string> identities = new List<string>();

            foreach (TransferModels transfer in transfers)
            {
                string identity = transfer.GetAwsIdentityDisplay();

                if (string.IsNullOrEmpty(identity) == false && identities.Contains(identity) == false)
                {
                    identities.Add(identity);
                }
            }

            return string.Join(", ", identities);
        }

        private void AddSummary(Document document, PdfFont boldFont, TransferReportSummary summary)
        {
            Paragraph heading = new Paragraph("Sažetak");
            heading.SetFont(boldFont);
            heading.SetFontSize(12);
            document.Add(heading);

            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }));
            table.SetWidth(UnitValue.CreatePercentValue(50));

            AddSummaryRow(table, boldFont, "Broj prijenosa", summary.TotalCount.ToString());
            AddSummaryRow(table, boldFont, "Uspješnost", summary.GetSuccessPercent().ToString("F1") + " %");
            AddSummaryRow(table, boldFont, "Uspješno / neuspjelo / otkazano",
                summary.CompletedCount + " / " + summary.FailedCount + " / " + summary.CancelledCount);
            AddSummaryRow(table, boldFont, "Upload", FormatUtils.FormatFileSize(summary.UploadedBytes));
            AddSummaryRow(table, boldFont, "Download", FormatUtils.FormatFileSize(summary.DownloadedBytes));

            string speedText = "-";

            if (summary.AverageSpeedMBps.HasValue)
            {
                speedText = summary.AverageSpeedMBps.Value.ToString("F2") + " MB/s";
            }

            AddSummaryRow(table, boldFont, "Prosječna brzina", speedText);

            if (summary.MostCommonErrorCount > 0)
            {
                AddSummaryRow(table, boldFont, "Najčešća greška", summary.MostCommonError + " (" + summary.MostCommonErrorCount + "×)");
            }

            document.Add(table);
            document.Add(new Paragraph(" "));
        }

        private void AddSummaryRow(Table table, PdfFont boldFont, string label, string value)
        {
            table.AddCell(new Cell().Add(new Paragraph(label).SetFont(boldFont)));
            table.AddCell(new Cell().Add(new Paragraph(value)));
        }

        // prijenosi grupirani po bucketu, sa zbrojem po grupi
        private void AddTransfersByBucket(Document document, PdfFont boldFont, List<TransferModels> transfers, bool includeUserColumn)
        {
            if (transfers.Count == 0)
            {
                document.Add(new Paragraph("Nema prijenosa za odabrani filter."));
                return;
            }

            Dictionary<string, List<TransferModels>> groups = new Dictionary<string, List<TransferModels>>();
            List<string> bucketNames = new List<string>();

            foreach (TransferModels transfer in transfers)
            {
                string bucket = transfer.Bucket;

                if (string.IsNullOrEmpty(bucket))
                {
                    bucket = "(nepoznat bucket)";
                }

                if (groups.ContainsKey(bucket) == false)
                {
                    groups[bucket] = new List<TransferModels>();
                    bucketNames.Add(bucket);
                }

                groups[bucket].Add(transfer);
            }

            bucketNames.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string bucket in bucketNames)
            {
                List<TransferModels> bucketTransfers = groups[bucket];

                long bucketBytes = 0;

                foreach (TransferModels transfer in bucketTransfers)
                {
                    bucketBytes += transfer.BytesTransferred;
                }

                Paragraph groupHeader = new Paragraph("Bucket: " + bucket + " - prijenosa: " + bucketTransfers.Count
                    + ", preneseno: " + FormatUtils.FormatFileSize(bucketBytes));
                groupHeader.SetFont(boldFont);
                groupHeader.SetFontSize(11);
                document.Add(groupHeader);

                document.Add(BuildTransferTable(boldFont, bucketTransfers, includeUserColumn));
                document.Add(new Paragraph(" "));
            }
        }

        private Table BuildTransferTable(PdfFont boldFont, List<TransferModels> transfers, bool includeUserColumn)
        {
            List<string> headers = new List<string> { "Vrijeme", "Smjer", "Objekt (ključ)", "Veličina", "Trajanje", "Brzina", "Status" };
            List<float> widths = new List<float> { 2.2f, 1.2f, 5f, 1.4f, 1.3f, 1.4f, 1.4f };

            if (includeUserColumn)
            {
                headers.Add("Korisnik");
                widths.Add(1.6f);
            }

            Table table = new Table(UnitValue.CreatePercentArray(widths.ToArray()));
            table.UseAllAvailableWidth();

            foreach (string header in headers)
            {
                Cell headerCell = new Cell().Add(new Paragraph(header).SetFont(boldFont));
                headerCell.SetTextAlignment(TextAlignment.CENTER);
                table.AddHeaderCell(headerCell);
            }

            foreach (TransferModels transfer in transfers)
            {
                table.AddCell(FormatTime(transfer));
                table.AddCell(transfer.GetDirectionDisplay());
                table.AddCell(transfer.ObjectKey);
                table.AddCell(FormatUtils.FormatFileSize(transfer.FileSizeBytes));
                table.AddCell(transfer.GetDurationDisplay());
                table.AddCell(transfer.GetSpeedDisplay());

                Cell statusCell = new Cell().Add(new Paragraph(transfer.GetStatusDisplay()));

                if (transfer.Status == TransferReportCalculator.StatusFailed)
                {
                    statusCell.SetFontColor(ColorConstants.RED);
                }

                table.AddCell(statusCell);

                if (includeUserColumn)
                {
                    table.AddCell(transfer.Username);
                }

                // razlog greške u retku ispod neuspjelog prijenosa, preko cijele širine tablice
                if (string.IsNullOrWhiteSpace(transfer.ErrorMessage) == false)
                {
                    Cell errorCell = new Cell(1, headers.Count).Add(new Paragraph("Razlog: " + transfer.ErrorMessage));
                    errorCell.SetFontSize(SmallFontSize);
                    errorCell.SetFontColor(ColorConstants.DARK_GRAY);
                    table.AddCell(errorCell);
                }
            }

            return table;
        }

        private string FormatTime(TransferModels transfer)
        {
            DateTime local = transfer.GetCreatedAtLocal();

            if (local == DateTime.MinValue)
            {
                return "-";
            }

            return local.ToString("dd.MM.yyyy HH:mm");
        }
    }
}