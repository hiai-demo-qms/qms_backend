namespace WebApplication1.Application.Models
{
    public class UpdateFileModel
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public IFormFile? FileUpload { get; set; }
    }
}
