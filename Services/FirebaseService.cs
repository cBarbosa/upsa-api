using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace upsa_api.Services
{
    public class FirebaseService
    {
        private readonly FirestoreDb _fireStoreDb = null!;

        public FirebaseService(FirestoreDb fireStoreDb)
        {
            _fireStoreDb = fireStoreDb;
        }

        public async Task<SendEmail> GetReportEmail()
        {
            CancellationToken cancellationToken = new CancellationToken();
            var _DistribuitionsProcessList = string.Empty;
            var _DistribuitionsProcess = 0;
            var _processos = await GetAll<Proccess>(cancellationToken);
            
            foreach (var processo in _processos)
            {
                if (processo.DateFinal != null)
                {
                    DateTime.TryParseExact(processo.DateFinal, "d", new System.Globalization.CultureInfo("pt-BR"), System.Globalization.DateTimeStyles.None, out DateTime dataFinal);
                    if (dataFinal.CompareTo(DateTime.Now) > 0) {
                        _DistribuitionsProcessList += $@"<li>Processo <strong>{processo.Number}</strong><br />
                            Data: {dataFinal:d}<br />
                            Responsável: {(await Get<Users>(processo.Accountable, cancellationToken))?.DisplayName ?? "N/D"}</li>";
                        _DistribuitionsProcess++;
                    }
                }
            }

            var bodyMessage = @$"
                <h2>Lista de Processos para distribuir</h2>
                <ul>
                    {_DistribuitionsProcessList}
                </ul>
                <p>Total de processos para distribuição: {_DistribuitionsProcess}</p>
                <hr>
            ";

            return new SendEmail
            {
                to = await GetEmailUsersByProfile("avocado", cancellationToken),
                bodyMessage = bodyMessage
            };
        }

        public async Task<IReadOnlyCollection<string>> GetEmailUsersByProfile(string profile, CancellationToken ct)
        {
            try
            {
                var result = (await GetAll<Users>(ct))
                               .Where(x => x.Role.ToLower().Equals(profile))
                               .Select(y => y.Email).ToList();

                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task AddOrUpdate<T>(T entity, CancellationToken ct) where T : IFirebaseEntity
        {
            var document = _fireStoreDb.Collection(typeof(T).Name.ToLower()).Document(entity.Id);
            await document.SetAsync(entity, cancellationToken: ct);
        }

        public async Task<T> Get<T>(string id, CancellationToken ct) where T : IFirebaseEntity
        {
            var document = _fireStoreDb.Collection(typeof(T).Name.ToLower()).Document(id);
            var snapshot = await document.GetSnapshotAsync(ct);
            return snapshot.ConvertTo<T>();
        }

        public async Task<IReadOnlyCollection<T>> GetAll<T>(CancellationToken ct) where T : IFirebaseEntity
        {
            var collection = _fireStoreDb.Collection(typeof(T).Name.ToLower());
            var snapshot = await collection.GetSnapshotAsync(ct);
            return snapshot.Documents.Select(x => x.ConvertTo<T>()).ToList();
        }

        public async Task<IReadOnlyCollection<T>> WhereEqualTo<T>(string fieldPath, object value, CancellationToken ct) where T : IFirebaseEntity
        {
            return await GetList<T>(_fireStoreDb.Collection(typeof(T).Name.ToLower()).WhereEqualTo(fieldPath, value), ct);
        }

        // just add here any method you need here WhereGreaterThan, WhereIn etc ...

        private static async Task<IReadOnlyCollection<T>> GetList<T>(Query query, CancellationToken ct) where T : IFirebaseEntity
        {
            var snapshot = await query.GetSnapshotAsync(ct);
            return snapshot.Documents.Select(x => x.ConvertTo<T>()).ToList();
        }
    }

    public interface IFirebaseEntity
    {
        public string Id { get; set; }
    }

    [FirestoreData]
    public class Users: IFirebaseEntity
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty("displayName")]
        public string DisplayName { get; set; }

        [FirestoreProperty("email")]
        public string Email { get; set; }

        [FirestoreProperty("role")]
        public string Role { get; set; }

        [FirestoreProperty("themis_id")]
        public int? ThemisId { get; set; }

        public Users()
        {

        }

        public Users(string name)
        {
            DisplayName = name;
        }
    }

    [FirestoreData]
    public class Proccess : IFirebaseEntity
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty("accountable")]
        public string Accountable { get; set; }

        [FirestoreProperty("number")]
        public string Number { get; set; }

        [FirestoreProperty("author")]
        public string Author { get; set; }

        [FirestoreProperty("defendant")]
        public string Defendant { get; set; }

        [FirestoreProperty("themis_id")]
        public int? ThemisId { get; set; }

        [FirestoreProperty("active")]
        public bool Active { get; set; }

        [FirestoreProperty("date_final")]
        public string DateFinal { get; set; }

        [FirestoreProperty("created_at")]
        public Timestamp CreatedAt { get; set; }

        [FirestoreProperty("updated_at")]
        public Timestamp UpdatedAt { get; set; }

        [FirestoreProperty("deadline")]
        public Deadline[] Deadline { get; set; }

        public Proccess()
        {

        }
    }

    [FirestoreData]
    public class Deadline
    {
        [FirestoreProperty("checked")]
        public bool Checked { get; set; }

        [FirestoreProperty("created_at")]
        public Timestamp CreatedAt { get; set; }

        public Deadline()
        {

        }
    }

    public class FirebaseSettings
    {
        [JsonPropertyName("project_id")]
        public string ProjectId => "upsa-bsb";

        [JsonPropertyName("private_key_id")]
        public string PrivateKeyId => "294453929734-oi2fivnr2p5sn51snoc48f7q9omjo50a.apps.googleusercontent.com";

        //[JsonPropertyName("api_key")]
        //public string ApiKey => "AIzaSyC75MBqkdZfPU2ZI6CmiR0_IPsqjnlDcPk";

        //[JsonPropertyName("Auth_domain")]
        //public string AuthDomain => "upsa-bsb.firebaseapp.com";

        //[JsonPropertyName("Auth_domain")]
        //public string AuthDomain => "upsa-bsb.firebaseapp.com";

        // ... and so on

  //      "apiKey": "AIzaSyC75MBqkdZfPU2ZI6CmiR0_IPsqjnlDcPk",
  //"authDomain": "upsa-bsb.firebaseapp.com",
  //"projectId": "upsa-bsb",
  //"storageBucket": "upsa-bsb.appspot.com",
  //"messagingSenderId": "294453929734",
  //"appId": "1:294453929734:web:2d4c731e42b8f556b12087",
  //"measurementId": "G-3XKPW9B859"
    }

    public class SendEmail
    {
        public IEnumerable<string> to { get; set; }
        public string bodyMessage { get; set; }
    }
}