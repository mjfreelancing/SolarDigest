using System.Threading.Tasks;

namespace SolarDigest.Cli.Commands
{
    public interface ICommand
    {
        Task Execute();
    }
}