using AllOverIt.Aws.Cdk.AppSync.Helpers;
using AllOverIt.Aws.Cdk.AppSync.MappingTemplates;

namespace SolarDigest.Deploy
{
    internal sealed class SolarDigestMappingTemplates : MappingTemplatesBase
    {
        private string _defaultRequestMapping;
        private string _defaultResponseMapping;

        public override string DefaultRequestMapping
        {
            get
            {
                //_requestMapping ??= CreateTemplate(
                //    "{",
                //    @"  ""version"" : ""2017-02-28"",",
                //    @"  ""operation"": ""Invoke"",",
                //    @"  ""payload"": {",
                //    @"    ""username"": $utils.toJson($ctx.identity.claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']),",
                //    @"    ""source"": ""$utils.escapeJavaScript($utils.toJson($ctx.source))"",",
                //    @"    ""arguments"": ""$utils.escapeJavaScript($utils.toJson($ctx.args))""",
                //    "    }",
                //    @"}"
                //);

                _defaultRequestMapping ??= StringHelpers.Prettify(
                    @"
                    {
                      ""version"" : ""2017-02-28"",
                      ""operation"": ""Invoke"",
                      ""payload"": $util.toJson($ctx.args)
                    }"
                );

                return _defaultRequestMapping;
            }
        }

        public override string DefaultResponseMapping
        {
            get
            {
                //_responseMapping ??= CreateTemplate(
                //    "#if($ctx.error)",
                //    "  $util.error($ctx.error.message, $ctx.error.type)",
                //    "#end",
                //    "",
                //    @"#if($ctx.result.Status == ""Success"")",
                //    "  $ctx.result.Payload",
                //    @"#elseif($ctx.result.Status == ""ValidationError"")",
                //    "  $util.error($ctx.result.ValidationErrors, $ctx.result.Status)",
                //    "#else",
                //    "  $util.error($ctx.result.Status, $ctx.result.Status)",
                //    "#end"
                //);

                _defaultResponseMapping ??= StringHelpers.Prettify(
                    "$util.toJson($ctx.result.payload)"
                );

                return _defaultResponseMapping;
            }
        }
    }
}