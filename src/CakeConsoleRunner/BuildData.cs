using Cake.Core.IO;

namespace CakeConsoleRunner
{
    public class BuildData
    {
        public FilePath Solution { get; }
        public BuildData(
            FilePath solution
            )
        {
            Solution = solution;
        }
    }
}