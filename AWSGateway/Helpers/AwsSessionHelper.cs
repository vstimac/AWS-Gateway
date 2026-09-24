using Amazon.Runtime;

namespace AWSGateway.Helpers
{
    // prepoznaje je li iznimka posljedica istekle privremene (STS) sesije
    public static class AwsSessionHelper
    {
        public static bool IsExpiredSessionError(Exception ex)
        {
            Exception current = ex;

            // servisne klase cesto omotaju originalnu iznimku u novu Exception(poruka) - prolazimo kroz cijeli InnerException lanac
            while (current != null)
            {
                AmazonServiceException serviceException = current as AmazonServiceException;

                if (serviceException != null)
                {
                    if (serviceException.ErrorCode == "ExpiredToken" || serviceException.ErrorCode == "ExpiredTokenException")
                    {
                        return true;
                    }
                }

                current = current.InnerException;
            }

            return false;
        }
    }
}
