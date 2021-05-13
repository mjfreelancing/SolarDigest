using AllOverIt.Aws.Cdk.AppSync;
using System.Text;

namespace SolarDigest.Deploy
{
    internal sealed class SolarDigestMappingTemplates : IMappingTemplates
    {
        private string _requestMapping;
        private string _responseMapping;

        public string RequestMapping
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
                //    @"    ""arguments"": ""$utils.escapeJavaScript($utils.toJson($ctx.arguments))""",
                //    "    }",
                //    @"}"
                //);

                _requestMapping ??= CreateTemplate(
                    "{",
                    @"  ""version"" : ""2017-02-28"",",
                    @"  ""operation"": ""Invoke"",",
                    @"  ""payload"": $util.toJson($context.arguments)",
                    @"}"
                );

                return _requestMapping;
            }
        }

        public string ResponseMapping
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

                _responseMapping ??= CreateTemplate(
                    "$util.toJson($context.result.payload)"
                );

                return _responseMapping;
            }
        }


        private static string CreateTemplate(params string[] lines)
        {
            var builder = new StringBuilder();

            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }

            return builder.ToString();
        }
    }
}