using System.ComponentModel.DataAnnotations;

namespace PasteMyst.Web.Models;

public class PageRequest
{
    public int Page { get; set; }

    [Range(1, 30)] public int PageSize { get; set; } = 15;
}