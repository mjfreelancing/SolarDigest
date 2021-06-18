using Amazon.CDK;
using Amazon.CDK.AWS.S3;

namespace SolarDigest.Deploy.Constructs
{
    internal sealed class S3Buckets : Construct
    {
        public S3Buckets(Construct scope)
            : base(scope, "Buckets")
        {
            // todo: this needs to be created in another stack - since the source code needs to be
            // placed in there before this deployment is performed.

            //CreateBucket(Constants.S3Buckets.LambdaSourceCodeBucketName);

            CreateBucket(Constants.S3Buckets.UploadsBucketName);
            CreateBucket(Constants.S3Buckets.DownloadsBucketName);
        }

        private void CreateBucket(string bucketName)
        {
            _ = new Bucket(this, bucketName, new BucketProps
            {
                BucketName = bucketName,
                RemovalPolicy = RemovalPolicy.RETAIN
            });
        }
    }
}