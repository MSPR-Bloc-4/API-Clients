using Client_Api.Model;

namespace Client_Api.Repository.Interface;
public interface IUserRepository
{
    Task<string> CreateUser(string email, string password, User user);
    Task<User> GetUserById(string userId);
    Task<List<User>> GetAllUsers();
    Task UpdateUser(string userId, User user);
    Task DeleteUser(string userId);
    Task<string> LoginUser(string email, string password);
    Task LogoutUser(string uid);
}