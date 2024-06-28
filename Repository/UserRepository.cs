﻿using Client_Api.Configuration;
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
        private readonly FirebaseAuth _auth;
        private readonly FirebaseAuthClient _firebaseAuthClient;
        private readonly FirebaseConfig _firebaseConfig;

        public UserRepository(FirestoreDb firestoreDb, IOptions<FirebaseConfig> firebaseConfig, FirebaseAuthClient firebaseAuthClient)
        {
            _firestoreDb = firestoreDb;
            _firebaseConfig = firebaseConfig.Value;
            _firebaseAuthClient = firebaseAuthClient;
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(_firebaseConfig.ServiceAccountPath)
            });
            _auth = FirebaseAuth.DefaultInstance;
        }

        public async Task<string> CreateUser(string email, string password, User user)
        {
            var userRecordArgs = new UserRecordArgs
            {
                Email = email,
                Password = password
            };

            UserRecord userRecord = await _auth.CreateUserAsync(userRecordArgs);
            CollectionReference usersRef = _firestoreDb.Collection("Users");
            user.CreatedAt = DateTime.UtcNow;
            await usersRef.Document(userRecord.Uid).SetAsync(user);

            return userRecord.Uid;
        }

        public async Task<User> GetUserById(string userId)
        {
            DocumentReference docRef = _firestoreDb.Collection("Users").Document(userId);
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
            Query usersQuery = _firestoreDb.Collection("Users");
            QuerySnapshot snapshot = await usersQuery.GetSnapshotAsync();
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
            DocumentReference docRef = _firestoreDb.Collection("Users").Document(userId);
            await docRef.SetAsync(user, SetOptions.Overwrite);
        }

        public async Task DeleteUser(string userId)
        {
            DocumentReference docRef = _firestoreDb.Collection("Users").Document(userId);
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