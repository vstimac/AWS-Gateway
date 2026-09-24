using System;
using System.Collections.Generic;
using System.Text;

namespace AWSGateway.Models
{
    public class AWSResource
    {
        public enum ResourceType
        {
            EC2Instance, S3Bucket, S3Object
        }

        public string ResourceId { get; set; }
        public ResourceType Type { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public long Size { get; set; }

        // klasa pohrane S3 objekta kako je vraća AWS (npr. STANDARD, GLACIER) - prazno za ostale vrste resursa
        public string StorageClass { get; set; }

        public AWSResource()
        {
            ResourceId = string.Empty;
            Type = ResourceType.S3Object; // krecem od s3 npr
            Name = string.Empty;
            Region = "eu-north-1";
            Status = "unknown";
            CreatedAt = DateTime.Now;
            StorageClass = string.Empty;
        }

        // "[Type] Name (Region)"
        public string GetDisplayName()
        {
            string typeString;

            switch (Type)
            {
                case ResourceType.EC2Instance:
                    typeString = "EC2";
                    break;
                case ResourceType.S3Bucket:
                    typeString = "Bucket";
                    break;
                case ResourceType.S3Object:
                    typeString = "Object";
                    break;
                default:
                    typeString = "Unknown";
                    break;
            }

            return "[" + typeString + "] " + Name + " (" + Region + ")";
        }

        // provjera
        public bool IsActive()
        {
            if (string.IsNullOrEmpty(Status))
            {
                return false;
            }

            if (Type == ResourceType.EC2Instance)
            {
                return Status.Equals("running", StringComparison.OrdinalIgnoreCase);
            }

            if (string.IsNullOrEmpty(ResourceId))
            {
                return false;
            }

            if (Status.Equals("deleted", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        // "mapa" u S3 - prazan objekt čiji ključ završava s "/" (stvaraju ga konzola i neki alati), nema sadržaja za preuzimanje
        public bool IsFolderMarker()
        {
            return Type == ResourceType.S3Object && Name.EndsWith("/");
        }

        // arhivske klase - objekt se ne može čitati (download, presigned poveznica) dok se ne vrati iz arhive (restore)
        // NAPOMENA: arhivski slojevi INTELLIGENT_TIERING klase ne vide se iz popisa objekata, pa ovdje nisu obuhvaćeni
        public bool IsArchived()
        {
            return StorageClass == "GLACIER" || StorageClass == "DEEP_ARCHIVE";
        }
    }
}