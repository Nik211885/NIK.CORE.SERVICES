namespace NIK.CORE.SERVICES.FILE.Dtos;

public class FolderDto
{
    public string Id {get; set;}
    public string Name { get; set; }
    public string ParentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
