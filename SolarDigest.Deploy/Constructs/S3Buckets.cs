using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using System;

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

            CreateBucket(Shared.Constants.S3Buckets.UploadsBucketName, config =>
            {
                config.LifecycleRules = new ILifecycleRule[]
                {
                    new LifecycleRule
                    {
                        AbortIncompleteMultipartUploadAfter = Duration.Days(1)
                    }
                };
            });

            CreateBucket(Shared.Constants.S3Buckets.DownloadsBucketName);
        }

        private void CreateBucket(string bucketName, Action<BucketProps> configAction = default)
        {
            var bucketProps = new BucketProps
            {
                // S3 does not allow upper case
                BucketName = $"{Shared.Helpers.GetAppVersionName()}-{bucketName}".ToLower(),
                RemovalPolicy = RemovalPolicy.RETAIN
            };

            configAction?.Invoke(bucketProps);

            _ = new Bucket(this, bucketName, bucketProps);
        }
    }
}