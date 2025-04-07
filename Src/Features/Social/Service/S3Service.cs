using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IConfiguration config)
    {
        _bucketName = config["AWS:BucketName"];
        _s3Client = new AmazonS3Client(config["AWS:AccessKey"], config["AWS:SecretKey"], RegionEndpoint.GetBySystemName(config["AWS:Region"]));
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var fileTransferUtility = new TransferUtility(_s3Client);

        // Upload file lên S3
        await fileTransferUtility.UploadAsync(fileStream, _bucketName, fileName);

        // Trả về URL truy cập file
        return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
    }
}
