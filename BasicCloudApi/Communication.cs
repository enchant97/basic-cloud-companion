using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BasicCloudApi
{
    public class Communication
    {
        /// <summary>
        /// The current version that Communication is compatible with
        /// </summary>
        public static Version ApiVersion = new Version("0.1.0");
        /// <summary>
        /// The auth token to use when authentication is required
        /// </summary>
        public Types.Token AuthToken { get; private set; }
        /// <summary>
        /// The base server url used (where the api is)
        /// </summary>
        public string BaseUrl { get; set; }
        /// <summary>
        /// generates auth header using stored AuthToken
        /// </summary>
        private System.Net.Http.Headers.AuthenticationHeaderValue AuthHeader { get { return new("Bearer", AuthToken.access_token); } }
        public Communication(string base_url)
        {
            BaseUrl = base_url;
        }
        public Communication(string base_url, Types.Token auth_token)
        {
            BaseUrl = base_url;
            AuthToken = auth_token;
        }
        /// <summary>
        /// checks a response for known HTTP errors,
        /// will not handle OK codes
        /// </summary>
        /// <param name="statusCode">the HTTP status code</param>
        private static void CheckForResponseErrors(HttpStatusCode statusCode)
        {
            throw statusCode switch
            {
                HttpStatusCode.Unauthorized => new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized),
                _ => new HttpRequestException("Unhandled HTTP error"),
            };
        }
        /// <summary>
        /// Get the api version that is running on the server
        /// </summary>
        public async Task<Types.ApiVersion> GetApiVersion()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(BaseUrl + "/api/version");
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            var versionString = await response.Content.ReadFromJsonAsync<Types.ApiVersionString>();
            var version = new Types.ApiVersion(
                new Version(versionString.version),
                new Version(versionString.oldest_compatible)
            );
            Debug.WriteLine("got api version: " + version.ToString());
            return version;
        }
        /// <summary>
        /// Create a new account
        /// </summary>
        /// <param name="username">the username to use</param>
        /// <param name="password">the password to use</param>
        public async Task PostCreateAccount(string username, string password)
        {
            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync(
                BaseUrl + "/api/users/",
                new { username, password }
            );
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            Debug.WriteLine("got ok from create account");
        }
        /// <summary>
        /// Get a login token using provided details
        /// </summary>
        /// <param name="username">the users username</param>
        /// <param name="password">the users password</param>
        /// <returns>the access token</returns>
        public async Task<Types.Token> PostLoginToken(string username, string password, bool save_token)
        {
            var values = new Dictionary<string, string> { { "username", username }, { "password", password } };
            var form_content = new FormUrlEncodedContent(values);
            using var client = new HttpClient();
            var response = await client.PostAsync(BaseUrl + "/token", form_content);
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            var token = await response.Content.ReadFromJsonAsync<Types.Token>();
            if (token.token_type != "bearer") { throw new Exception("Unknown token type"); }
            if (save_token) { AuthToken = token; }
            Debug.WriteLine("got bearer token: " + token.access_token);
            return token;
        }
        /// <summary>
        /// Get the directory "roots"
        /// </summary>
        /// <returns>the directory roots</returns>
        public async Task<Types.DirectoryRoots> GetDirectoryRoots()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = AuthHeader;
            var response = await client.GetAsync(BaseUrl + "/api/directory/roots");
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            var roots = await response.Content.ReadFromJsonAsync<Types.DirectoryRoots>();
            Debug.WriteLine("got directory roots: " + roots.ToString());
            return roots;
        }
        /// <summary>
        /// Get a directory's content
        /// </summary>
        /// <param name="directory">the current directory</param>
        /// <returns>the directory's content</returns>
        public async Task<Types.Content[]> PostDirectoryContents(string directory)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = AuthHeader;

            var response = await client.PostAsJsonAsync(BaseUrl + "/api/directory/contents", new { directory });
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            var contents = await response.Content.ReadFromJsonAsync<Types.Content[]>();
            Debug.WriteLine("got directory roots: " + contents.ToString());
            return contents;
        }
        /// <summary>
        /// Create a directory
        /// </summary>
        /// <param name="directory">the current directory</param>
        /// <param name="name">the new directory name</param>
        public async Task PostCreateDirectory(string directory, string name)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = AuthHeader;
            var response = await client.PostAsJsonAsync(BaseUrl + "/api/directory/mkdir", new { directory, name });
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            Debug.WriteLine("got ok from mkdir request");
        }
        /// <summary>
        /// Delete a directory
        /// </summary>
        /// <param name="directory">the directory to delete</param>
        public async Task DeleteDirectory(string directory)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = AuthHeader;
            var httpMessage = new HttpRequestMessage(HttpMethod.Delete, BaseUrl + "/api/directory/rm")
            {
                Content = new StringContent(JsonSerializer.Serialize(new { directory }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(httpMessage);
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            Debug.WriteLine("got ok from delete directory request");
        }
        /// <summary>
        /// Download a directory as a zip file
        /// </summary>
        /// <param name="directory">the directory to download</param>
        /// <returns>the file content</returns>
        public async Task<HttpContent> DownloadDirectoryAsZip(string directory)
        {
            string encodedDirectory = Convert.ToBase64String(Encoding.UTF8.GetBytes(directory));

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = AuthHeader;
            var response = await client.GetAsync(BaseUrl + "/api/directory/download/" + encodedDirectory);
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            Debug.WriteLine("got content from download directory as zip request");
            return response.Content;
        }
        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="filePath">the filepath to delete</param>
        public async Task DeleteFile(string filePath)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = AuthHeader;
            var httpMessage = new HttpRequestMessage(HttpMethod.Delete, BaseUrl + "/api/file/rm")
            {
                Content = new StringContent(JsonSerializer.Serialize(new { file_path = filePath }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(httpMessage);
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            Debug.WriteLine("got ok from delete file request");
        }
        /// <summary>
        /// Download the file content
        /// </summary>
        /// <param name="filePath">the filepath to download</param>
        public async Task<HttpContent> DownloadFile(string filePath)
        {
            string encodedFilePath = Convert.ToBase64String(Encoding.UTF8.GetBytes(filePath));

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = AuthHeader;
            var response = await client.GetAsync(BaseUrl + "/api/file/download/" + encodedFilePath);
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            Debug.WriteLine("got content from download file request");
            return response.Content;
        }
        /// <summary>
        /// Upload a file
        /// </summary>
        /// <param name="fileContent">the file to upload</param>
        /// <param name="fileName">the filename to use for the file</param>
        /// <param name="directory">the directory path to upload to</param>
        public async Task PostUploadFile(ByteArrayContent fileContent, string fileName, string directory)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = AuthHeader;
            MultipartFormDataContent multiPart = new();
            StringContent directoryContent = new(directory);
            multiPart.Add(directoryContent, "directory");
            multiPart.Add(fileContent, "file", fileName);
            var response = await client.PostAsync(BaseUrl + "/api/file/upload/overwrite", multiPart);
            if (!response.IsSuccessStatusCode) { CheckForResponseErrors(response.StatusCode); }
            Debug.WriteLine("got ok from upload file request");
        }
    }
}
