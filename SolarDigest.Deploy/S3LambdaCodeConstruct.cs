using Amazon.CDK;
using Amazon.CDK.AWS.S3;

namespace SolarDigest.Deploy
{
    internal class S3LambdaCodeConstruct : Construct
    {
        public Bucket CodeBucket { get; }

        public S3LambdaCodeConstruct(Construct scope)
            : base(scope, "LambdaCode")
        {
            CodeBucket = new Bucket(this, "LambdaCodeBucket", new BucketProps
            {
                BucketName = Constants.S3LambdaCodeBucketName
            });
        }
    }
}