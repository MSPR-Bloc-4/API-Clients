using Client_Api.Model;
using Client_Api.Repository.Interface;
using PubSubLibrary;

namespace Client_Api.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly PubSubService _pubSubService;

        public UserService(IUserRepository userRepository, PubSubService pubSubService)
        {
            _userRepository = userRepository;
            _pubSubService = pubSubService;
        }

        public async Task<string> CreateUser(string email, string password, User user)
        {
            return await _userRepository.CreateUser(email, password, user);
        }

        public async Task<User> GetUserById(string userId)
        {
            return await _userRepository.GetUserById(userId);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task UpdateUser(string userId, User user)
        {
            await _userRepository.UpdateUser(userId, user);
        }

        public async Task DeleteUser(string userId)
        {
            await _userRepository.DeleteUser(userId);

            string message = userId;
            await _pubSubService.PublishMessageAsync("user-deleted", message);
        }

        public async Task<string> LoginUser(string email, string password)
        {
            return await _userRepository.LoginUser(email, password);
        }

        public async Task LogoutUser(string uid)
        {
            await _userRepository.LogoutUser(uid);
        }
    }
}