using System.Threading.Tasks;

namespace ConsoleService
{
    public interface IConsoleApp
    {
        public Task<int> Execute();
    }
}