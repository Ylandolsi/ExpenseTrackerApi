using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApi.Dto.Expense;

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
//
// {
// "AccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoieWFzc2luZSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6Inllc3NnbWFpbDEuY29tIiwiZXhwIjoxNzM3Nzk4MzY0LCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MDM1IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAxIn0.4CD8NfqcaPAF3Ml918PQ77ivz5wRR1X1z1EXTk3fFuw",
// "RefreshToken": "hHZEu6Xf0ArltyRu9hVn/Olqpf1+7QsLe4ps2y94VO4="
// }
