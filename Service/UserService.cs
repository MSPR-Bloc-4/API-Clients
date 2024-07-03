using Client_Api.Configuration;
using Client_Api.Model;
using Client_Api.Repository.Interface;
using Client_Api.Service.Interface;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Options;

namespace Client_Api.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly PublisherServiceApiClient _publisherClient;
        private readonly FirebaseConfig _firebaseConfig;

        public UserService(IUserRepository userRepository, IOptions<FirebaseConfig> firebaseConfig)
        {
            GoogleCredential credential;
            if (Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS") != null)
            {
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS"))))
                {
                    credential = GoogleCredential.FromStream(stream);
                }
            }
            else
            {
                using (var stream = new FileStream("firebase_credentials.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream);
                }
            }
            _userRepository = userRepository;
            _firebaseConfig = firebaseConfig.Value;
            _publisherClient = new PublisherServiceApiClientBuilder
            {
                Credential = credential
            }.Build();
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
            TopicName topicName = new TopicName(_firebaseConfig.ProjectId, "user-deleted");
            PubsubMessage pubsubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(message)
            };
            await _publisherClient.PublishAsync(topicName, new[] { pubsubMessage });        
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