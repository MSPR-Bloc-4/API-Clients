using Client_Api.Configuration;
using Client_Api.Repository.Interface;
using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Options;
using User = Client_Api.Model.User;

namespace Client_Api.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly FirebaseAuth? _auth;
        private readonly FirebaseAuthClient _firebaseAuthClient;
        private readonly CollectionReference _collectionReference;

        public UserRepository(FirestoreDb firestoreDb, FirebaseAuthClient firebaseAuthClient, FirebaseAuth? auth = null)
        { 
            _firestoreDb = firestoreDb;
            _firebaseAuthClient = firebaseAuthClient;
            _collectionReference = _firestoreDb.Collection("User");
            if (auth == null)
            {
                auth = FirebaseAuth.DefaultInstance;
            }
            _auth = auth;
        }

        public async Task<string> CreateUser(string email, string password, User user)
        {
            var userRecordArgs = new UserRecordArgs
            {
                Email = email,
                Password = password
            };

            UserRecord userRecord = await _auth.CreateUserAsync(userRecordArgs);
            user.CreatedAt = DateTime.UtcNow;
            await _collectionReference.Document(userRecord.Uid).SetAsync(user);

            return userRecord.Uid;
        }

        public async Task<User> GetUserById(string userId)
        {
            DocumentReference docRef = _collectionReference.Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                User user = snapshot.ConvertTo<User>();
                return user;
            }

            return null;
        }

        public async Task<List<User>> GetAllUsers()
        {
            QuerySnapshot snapshot = await _collectionReference.GetSnapshotAsync();
            List<User> users = new List<User>();

            foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                User user = documentSnapshot.ConvertTo<User>();
                users.Add(user);
            }

            return users;
        }

        public async Task UpdateUser(string userId, User user)
        {
            DocumentReference docRef = _collectionReference.Document(userId);
            await docRef.SetAsync(user, SetOptions.Overwrite);
        }

        public async Task DeleteUser(string userId)
        {
            DocumentReference docRef = _collectionReference.Document(userId);
            await docRef.DeleteAsync();
            await _auth.DeleteUserAsync(userId);
        }

        public async Task<string> LoginUser(string email, string password)
        {
            var auth = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(email, password);
            return auth.User.Credential.IdToken;
        }

        public async Task LogoutUser(string uid)
        {
            await _auth.RevokeRefreshTokensAsync(uid);
        }
    }
}
