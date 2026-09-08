namespace SharedServices
{
/// <summary>
/// Extension methods for HTTP operations. This class provides utility methods for downloading files from a given URL, either as byte arrays or as Base64-encoded
/// strings. These methods can be used throughout the application to simplify the process of fetching and handling files from external sources, ensuring consistent
/// error handling and resource management when performing HTTP requests.
/// </summary>
public static class HttpExtensions
{
    /// <summary>
    /// Downloads a file from the specified URL and returns its name and content as a byte array. This method uses HttpClient to perform the HTTP GET request and retrieves
    /// the file content as a byte array.
    /// </summary>
    /// <param name="url">url.</param>
    /// <returns>File name and File Bytes.</returns>
    /// <exception cref="ArgumentException">ArgumentException.</exception>
    public static async Task<(string FileName, byte[] File)> DownloadBytesAsync(this string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }

        using var httpClient = new HttpClient();
        var fileBytes = await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
        return (Path.GetFileName(new Uri(url).AbsolutePath), fileBytes);
    }

    /// <summary>
    /// Downloads a file from the specified URL and returns its name and content as a Base64-encoded string. This method uses HttpClient to perform the HTTP GET request,
    /// retrieves
    /// the file content as a byte array, and then converts it to a Base64-encoded string.
    /// </summary>
    /// <param name="url">url.</param>
    /// <returns>string FileName, string Base64File.</returns>
    /// <exception cref="ArgumentException">ArgumentException.</exception>
    public static async Task<(string FileName, string Base64File)> DownloadFileAsBase64Async(this string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }

        using var httpClient = new HttpClient();
        var fileBytes = await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);

        var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
        var base64File = Convert.ToBase64String(fileBytes);

        return (fileName, base64File);
    }
}
}