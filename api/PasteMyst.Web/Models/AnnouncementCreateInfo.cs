using System.ComponentModel.DataAnnotations;

namespace PasteMyst.Web.Models;

public class AnnouncementCreateInfo
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }
}