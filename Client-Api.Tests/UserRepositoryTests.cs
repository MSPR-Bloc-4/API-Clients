using Client_Api.Helper;
using Client_Api.Model;
using Client_Api.Repository;
using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Api.Gax;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Xunit;
using User = Client_Api.Model.User;

namespace Client_Api.Tests
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly FirebaseAuthClient _firebaseAuthClient;
        private readonly string _testCollectionName = "User";
        private readonly FirebaseAuth? _auth;

        public UserRepositoryTests()
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

            var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECTID") ?? JsonReader.GetFieldFromJsonFile("project_id");
            var apiKey = Environment.GetEnvironmentVariable("FIREBASE_APIKEY") ?? JsonReader.GetFieldFromJsonFile("api_key");
            var authDomain = Environment.GetEnvironmentVariable("FIREBASE_AUTHDOMAIN") ?? JsonReader.GetFieldFromJsonFile("auth_domain");
            var builder = new FirestoreDbBuilder
            {
                Credential = credential,
                ProjectId = projectId,
                DatabaseId = "test",  // Use the 'test' database for testing
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction
            };
            _firestoreDb = builder.Build();

            _firebaseAuthClient = new FirebaseAuthClient(new FirebaseAuthConfig
            {
                ApiKey = apiKey,
                AuthDomain = authDomain,
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                }
            });
            if (FirebaseApp.DefaultInstance == null)
            {
                _auth = FirebaseAuth.GetAuth(FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                }));
            }

            _auth = FirebaseAuth.DefaultInstance;
        }

        private string GenerateEmail()
        {
            return $"testuser_{Guid.NewGuid()}@gmail.com";
        }

        [Fact]
        public async Task CreateUser_Should_Add_User_To_Firestore()
        {
            var userRepository = new UserRepository(_firestoreDb, _firebaseAuthClient, _auth);
            var email = GenerateEmail();
            var password = "TestPassword123";
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                CreatedAt = DateTime.UtcNow,
                Adress = new Adress
                {
                    Street = "123 Test St",
                    City = "Test City",
                    Country = "Test Country",
                    ZipCode = 12345
                }
            };
            var userId = await userRepository.CreateUser(email, password, user);
            Assert.NotNull(userId);
            var retrievedUser = await GetUserFromFirestore(userId);
            AssertUserProperties(user, retrievedUser);
            await userRepository.DeleteUser(userId);
        }

        [Fact]
        public async Task GetUserById_Should_Retrieve_User_From_Firestore()
        {
            var userRepository = new UserRepository(_firestoreDb, _firebaseAuthClient, _auth);
            // Arrange
            var email = GenerateEmail();
            var password = "TestPassword123";
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                CreatedAt = DateTime.UtcNow,
                Adress = new Adress
                {
                    Street = "123 Test St",
                    City = "Test City",
                    Country = "Test Country",
                    ZipCode = 12345
                }
            };
            var userId = await userRepository.CreateUser(email, password, user);

            // Act
            var retrievedUser = await userRepository.GetUserById(userId);

            // Assert
            Assert.NotNull(retrievedUser);
            AssertUserProperties(user, retrievedUser);

            // Clean up
            await userRepository.DeleteUser(userId);
        }

        [Fact]
        public async Task GetAllUsers_Should_Retrieve_All_Users_From_Firestore()
        {
            var userRepository = new UserRepository(_firestoreDb, _firebaseAuthClient, _auth);
            var email1 = GenerateEmail();
            var email2 = GenerateEmail();
            var password = "TestPassword123";
            var user1 = new User
            {
                FirstName = "Test1",
                LastName = "User1",
                Username = "testuser1",
                CreatedAt = DateTime.UtcNow,
                Adress = new Adress
                {
                    Street = "123 Test St",
                    City = "Test City",
                    Country = "Test Country",
                    ZipCode = 12345
                }
            };
            var user2 = new User
            {
                FirstName = "Test2",
                LastName = "User2",
                Username = "testuser2",
                CreatedAt = DateTime.UtcNow,
                Adress = new Adress
                {
                    Street = "123 Test St",
                    City = "Test City",
                    Country = "Test Country",
                    ZipCode = 12345
                }
            };
            var userId1 = await userRepository.CreateUser(email1, password, user1);
            var userId2 = await userRepository.CreateUser(email2, password, user2);

            // Act
            var users = await userRepository.GetAllUsers();

            // Assert
            Assert.NotNull(users);

            // Clean up
            await userRepository.DeleteUser(userId1);
            await userRepository.DeleteUser(userId2);
        }

        [Fact]
        public async Task UpdateUser_Should_Update_User_In_Firestore()
        {
            var userRepository = new UserRepository(_firestoreDb, _firebaseAuthClient, _auth);
            var email = GenerateEmail();
            var password = "TestPassword123";
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                CreatedAt = DateTime.UtcNow,
                Adress = new Adress
                {
                    Street = "123 Test St",
                    City = "Test City",
                    Country = "Test Country",
                    ZipCode = 12345
                }
            };
            var userId = await userRepository.CreateUser(email, password, user);

            // Modify user details
            user.FirstName = "Updated Test";
            user.LastName = "Updated User";

            // Act
            await userRepository.UpdateUser(userId, user);
            var updatedUser = await userRepository.GetUserById(userId);

            // Assert
            Assert.Equal(user.FirstName, updatedUser.FirstName);
            Assert.Equal(user.LastName, updatedUser.LastName);

            // Clean up
            await userRepository.DeleteUser(userId);
        }

        [Fact]
        public async Task DeleteUser_Should_Delete_User_From_Firestore()
        {
            var userRepository = new UserRepository(_firestoreDb, _firebaseAuthClient, _auth);
            var email = GenerateEmail();
            var password = "TestPassword123";
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                CreatedAt = DateTime.UtcNow,
                Adress = new Adress
                {
                    Street = "123 Test St",
                    City = "Test City",
                    Country = "Test Country",
                    ZipCode = 12345
                }
            };
            var userId = await userRepository.CreateUser(email, password, user);

            // Act
            await userRepository.DeleteUser(userId);
            var deletedUser = await userRepository.GetUserById(userId);

            // Assert
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task LoginUser_Should_Return_Token()
        {
            var userRepository = new UserRepository(_firestoreDb, _firebaseAuthClient, _auth);
            var email = GenerateEmail();
            var password = "TestPassword123";
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Username = "testuser",
                CreatedAt = DateTime.UtcNow,
                Adress = new Adress
                {
                    Street = "123 Test St",
                    City = "Test City",
                    Country = "Test Country",
                    ZipCode = 12345
                }
            };
            var userId = await userRepository.CreateUser(email, password, user);

            // Act
            var token = await userRepository.LoginUser(email, password);

            // Assert
            Assert.NotNull(token);

            // Clean up
            await userRepository.DeleteUser(userId);
        }

        private async Task<User> GetUserFromFirestore(string userId)
        {
            var docRef = _firestoreDb.Collection(_testCollectionName).Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<User>();
            }
            return null;
        }

        private void AssertUserProperties(User expected, User actual)
        {
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.Username, actual.Username);
            Assert.Equal(expected.Adress.Street, actual.Adress.Street);
            Assert.Equal(expected.Adress.City, actual.Adress.City);
            Assert.Equal(expected.Adress.Country, actual.Adress.Country);
            Assert.Equal(expected.Adress.ZipCode, actual.Adress.ZipCode);
        }

        public void Dispose()
        {
            // Clean up Firestore data after tests
            ClearFirestoreCollection(_testCollectionName);
        }

        private async void ClearFirestoreCollection(string collectionName)
        {
            var collectionRef = _firestoreDb.Collection(collectionName);
            var query = collectionRef;
            var batch = _firestoreDb.StartBatch();

            // Batch delete documents
            var querySnapshot = await query.GetSnapshotAsync();
            foreach (var documentSnapshot in querySnapshot.Documents)
            {
                batch.Delete(documentSnapshot.Reference);
            }

            // Commit the batch
            await batch.CommitAsync();
        }
    }
}
