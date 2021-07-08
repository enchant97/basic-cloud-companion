using System;
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
        /// Join a path part with a base directory path
        /// </summary>
        /// <param name="basePath">The base directory path</param>
        /// <param name="pathPart">The path part to join with the base</param>
        /// <returns>The joined path</returns>
        public static string JoinBasePath(string basePath, string pathPart)
        {
            if (!string.IsNullOrEmpty(basePath))
            {
                return basePath + "/" + pathPart;
            }
            return pathPart;
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
        /// <summary>
        /// Read a file and put into HTTP ByteArrayContent object
        /// </summary>
        /// <param name="filepath">the filepath to read from</param>
        /// <returns>the HTTP content to upload</returns>
        public static async Task<ByteArrayContent> ReadFileByteContent(string filepath)
        {
            return new ByteArrayContent(await File.ReadAllBytesAsync(filepath));
        }
        /// <summary>
        /// Check whether a app version is compatible with the server version
        /// </summary>
        /// <param name="appVersion">the current app version</param>
        /// <param name="serverVersion">the server version</param>
        /// <returns>whether the app is compatible</returns>
        public static bool IsAppCompatible(Version appVersion, Types.ApiVersion serverVersion)
        {
            if (
                appVersion.CompareTo(serverVersion.oldest_compatible) >= 0 &&
                appVersion.CompareTo(serverVersion.version) <= 0)
            {
                return true;
            }
            return false;
        }
    }
}
