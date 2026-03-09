using Microsoft.AspNetCore.Http;

namespace NIK.CORE.SERVICES.FILE.Dtos;

/// <summary>
/// Represents a request to upload a file to the storage system.
/// </summary>
public class UploadFileRequest
{
    /// <summary>
    /// The file content sent by the client using multipart/form-data.
    /// </summary>
    public IFormFile File { get; set; }

    /// <summary>
    /// Identifier of the target folder where the file will be stored.
    /// </summary>
    public string FolderId { get; set; }

    /// <summary>
    /// Optional display name of the file. 
    /// If not provided, the original file name will be used.
    /// </summary>
    public string? DisplayName { get; set; }
}