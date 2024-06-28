namespace Client_Api.Model;

public class RegisterModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public Adress Adress { get; set; }
    public List<string> OrderIds { get; set; }
}