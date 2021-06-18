using AllOverIt.Aws.Cdk.AppSync;
using AllOverIt.Extensions;
using SolarDigest.Deploy.Helpers;
using System.Collections.Generic;

namespace SolarDigest.Deploy
{
    internal sealed class SolarDigestMappingTemplates : IMappingTemplates
    {
        private string _defaultRequestMapping;
        private string _defaultResponseMapping;
        private readonly IDictionary<string, string> _functionRequestMappings = new Dictionary<string, string>();
        private readonly IDictionary<string, string> _functionResponseMappings = new Dictionary<string, string>();

        public string DefaultRequestMapping
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

        public string DefaultResponseMapping
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

        public void RegisterRequestMapping(string functionName, string mapping)
        {
            _functionRequestMappings.Add(functionName, mapping);
        }

        public void RegisterResponseMapping(string functionName, string mapping)
        {
            _functionResponseMappings.Add(functionName, mapping);
        }

        public string GetRequestMapping(string functionName)
        {
            if (functionName.IsNullOrEmpty())
            {
                return DefaultRequestMapping;
            }

            var mapping = _functionRequestMappings.GetValueOrDefault(functionName);

            return mapping ?? DefaultRequestMapping;
        }

        public string GetResponseMapping(string functionName)
        {
            if (functionName.IsNullOrEmpty())
            {
                return DefaultResponseMapping;
            }

            var mapping = _functionResponseMappings.GetValueOrDefault(functionName);

            return mapping ?? DefaultResponseMapping;
        }
    }
}