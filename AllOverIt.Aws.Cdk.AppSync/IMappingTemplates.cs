namespace AllOverIt.Aws.Cdk.AppSync
{
    public interface IMappingTemplates
    {
        string RequestMapping { get; }
        string ResponseMapping { get; }
    }
}