using Google.Cloud.Firestore;
using System;
using System.Linq;
using System.Threading.Tasks;
using upsa_api.Services.Interfaces;

namespace upsa_api.Services
{
    public class NewFirebaseService : IFirebaseService
    {
        private FirestoreDb fireStoreDb;

        public NewFirebaseService()
        {
            string filepath = @"D:\Temp\downloads\upsa-bsb-firebase-adminsdk-8bbcd-952c9d7eae.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filepath);
            fireStoreDb = FirestoreDb.Create("upsa-bsb");
        }

        public async Task<bool> SendMail()
        {
            //var docRef = fireStoreDb.Collection("proccess");
            //var snapshot = docRef.GetSnapshotAsync().GetAwaiter().GetResult();

            var collection = fireStoreDb.Collection("proccess");
            var snapshot = await collection.GetSnapshotAsync();
            var result = snapshot.Documents.Select(x => x.ConvertTo<Proccess>()).ToList();

            //if (snapshot.Exists)
            //{
            //    Process usr = snapshot.ConvertTo<Process>();
            //}
            //else
            //{
            //    return null;
            //}

            throw new NotImplementedException();
        }
    }
}
