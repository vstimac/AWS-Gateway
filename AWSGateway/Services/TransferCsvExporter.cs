using AWSGateway.Models;
using AWSGateway.Utils;
using System.Globalization;
using System.Text;

namespace AWSGateway.Services
{
    // izvoz filtriranih prijenosa u CSV - brojevi u invarijantnom formatu (točka kao decimalni separator), vrijeme lokalno
    public class TransferCsvExporter
    {
        public void Export(string outputPath, List<TransferModels> transfers)
        {
            // UTF-8 s BOM oznakom - Excel tada ispravno prikazuje hrvatske znakove
            using (StreamWriter writer = new StreamWriter(outputPath, false, new UTF8Encoding(true)))
            {
                writer.WriteLine("VrijemeLokalno,Smjer,Bucket,ObjectKey,LokalnaPutanja,VelicinaBajtova,PrenesenoBajtova,TrajanjeMs,BrzinaMBps,Status,Greska,Korisnik,AwsIdentitet");

                foreach (TransferModels transfer in transfers)
                {
                    StringBuilder line = new StringBuilder();

                    line.Append(FormatUtils.EscapeCsv(FormatLocalTime(transfer))).Append(',');
                    line.Append(FormatUtils.EscapeCsv(transfer.Direction)).Append(',');
                    line.Append(FormatUtils.EscapeCsv(transfer.Bucket)).Append(',');
                    line.Append(FormatUtils.EscapeCsv(transfer.ObjectKey)).Append(',');
                    line.Append(FormatUtils.EscapeCsv(transfer.LocalPath)).Append(',');
                    line.Append(transfer.FileSizeBytes.ToString(CultureInfo.InvariantCulture)).Append(',');
                    line.Append(transfer.BytesTransferred.ToString(CultureInfo.InvariantCulture)).Append(',');
                    line.Append(FormatNullable(transfer.DurationMs, "F0")).Append(',');
                    line.Append(FormatNullable(transfer.GetSpeedMBps(), "F3")).Append(',');
                    line.Append(FormatUtils.EscapeCsv(transfer.Status)).Append(',');
                    line.Append(FormatUtils.EscapeCsv(transfer.ErrorMessage)).Append(',');
                    line.Append(FormatUtils.EscapeCsv(transfer.Username)).Append(',');
                    line.Append(FormatUtils.EscapeCsv(transfer.AwsIdentity));

                    writer.WriteLine(line.ToString());
                }
            }
        }

        private string FormatLocalTime(TransferModels transfer)
        {
            DateTime local = transfer.GetCreatedAtLocal();

            if (local == DateTime.MinValue)
            {
                return string.Empty;
            }

            return local.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        // prazno polje za nepoznatu vrijednost (npr. trajanje starih zapisa)
        private string FormatNullable(double? value, string format)
        {
            if (value.HasValue == false)
            {
                return string.Empty;
            }

            return value.Value.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}