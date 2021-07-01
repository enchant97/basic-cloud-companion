using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BasicCloudApi
{
    public static class Helpers
    {
        /// <summary>
        /// Gets the parent directory of given path,
        /// will return empty string if it is root path
        /// </summary>
        /// <param name="currDirectory">The current directory</param>
        /// <returns>The parent directory</returns>
        public static string GetParentDir(string currDirectory)
        {
            var lastSlashI = currDirectory.LastIndexOf("/");
            if (lastSlashI == -1)
            {
                return "";
            }
            return currDirectory.Substring(0, lastSlashI);
        }
        /// <summary>
        /// write HTTP content to a file
        /// </summary>
        /// <param name="filepath">the filepath to save to</param>
        /// <param name="content">the HTTP content to save</param>
        public static async Task WriteHttpContentToFile(string filepath, HttpContent content)
        {
            using var fileStream = new FileStream(filepath, FileMode.CreateNew);
            await content.CopyToAsync(fileStream);
        }
    }
}
