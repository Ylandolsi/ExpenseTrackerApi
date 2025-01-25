using System.ComponentModel.DataAnnotations;

namespace Models.Entities.Dto.User;

public record UserLogin
{
    public string? email { get; init; }
    public string? phoneNumber { get; init; }
    [Required]
    public string? Password { get; init; }
}
// {
// "email": "yessgmail1.com" , 
// "phoneNumber": "211111111" ,
// "Password" : "50000" , 
// "Name":  "yassine" 
// }
