using Google.Cloud.Firestore;

namespace Client_Api.Model;

[FirestoreData]
public class User
{
    [FirestoreDocumentId]
    public string? Id { get; set; }
    [FirestoreProperty]
    public string FirstName { get; set; }
    [FirestoreProperty]
    public string LastName { get; set; }
    [FirestoreProperty]
    public string Username { get; set; }
    [FirestoreProperty]
    public DateTime CreatedAt { get; set; }
    [FirestoreProperty]
    public Adress Adress { get; set; }
}