using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Requests;

public class CheckVatRequest
{
    [Required]
    public string VatNumber { get; set; } = string.Empty;
}