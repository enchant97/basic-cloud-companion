namespace BasicCloudApi.Types
{
    public record Token(string access_token, string token_type);
    public record DirectoryRoots(string shared, string home);
    public record ContentMeta(bool is_directory);
    public record Content(string name, ContentMeta meta);
}
