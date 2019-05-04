using System.ComponentModel.DataAnnotations;

namespace mohome_api.ViewModels
{
    public class DeleteAlbumModel
    {
    [Required]
    public int AlbumId { get; set; }
    }
}
