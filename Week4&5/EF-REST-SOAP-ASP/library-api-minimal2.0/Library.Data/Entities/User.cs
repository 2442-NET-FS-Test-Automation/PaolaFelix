using System.ComponentModel.DataAnnotations;

namespace Library.Data.Entities;

public class User
{
    public int Id {get; set;}
    [MaxLength(64)]
    public string UserName {get; set;} = "";
    public string PasswordHash {get; set;} = ""; // we NEVER store the password in plain text
    public string Role {get; set;} = "consumer";
}