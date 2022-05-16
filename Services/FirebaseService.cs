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
            var _DivergentProcessList = string.Empty;
            var _WaitingForProcessList = string.Empty;
            var _ProcessCount1 = 0;
            var _ProcessCount2 = 0;
            var _ProcessCount3 = 0;
            var _processos = await GetAll<Proccess>(cancellationToken);
            
            foreach (var processo in _processos)
            {
                if (processo.DateFinal != null)
                {
                    // Get all distribuited process until 1 week
                    DateTime.TryParseExact(processo.DateFinal, "d", new System.Globalization.CultureInfo("pt-BR"), System.Globalization.DateTimeStyles.None, out DateTime dataFinal);
                    if (dataFinal.CompareTo(DateTime.Now.AddDays(7)) < 1 && dataFinal.CompareTo(DateTime.Now) > 0)
                    {
                        _DistribuitionsProcessList += $@"<li>Processo <strong>{processo.Number}</strong><br />
                            Data: {dataFinal:d}<br />
                            Responsável: {(await Get<Users>(processo.Accountable, cancellationToken))?.DisplayName ?? "N/D"}</li>";
                        _ProcessCount1++;
                    }
                }

                // Get all divergent process
                if (processo.Deadline.Length == 2 && processo.DateFinal == null)
                {
                    if (processo.Deadline[0].CourtDate != processo.Deadline[1].CourtDate
                        || processo.Deadline[0].InternalDate != processo.Deadline[1].InternalDate)
                    {
                        _DivergentProcessList += $@"<li>Processo <strong>{processo.Number}</strong><br />
                            Responsável: {(await Get<Users>(processo.Accountable, cancellationToken))?.DisplayName ?? "N/D"}</li>";
                        _ProcessCount2++;
                    }
                }

                // Get all waiting analisys process
                if (processo.Deadline.Length == 1)
                {
                    _WaitingForProcessList += $@"<li>Processo <strong>{processo.Number}</strong></li>";
                    _ProcessCount3++;
                }
            }

            var bodyMessage = @$"
                <h2>Lista de processos distribuidos (Próximos 7 Dias)</h2>
                    <ul>{_DistribuitionsProcessList}</ul>
                    <p>Total de processos: {_ProcessCount1}</p><hr>
                <h2>Lista de processos divergentes</h2>
                    <ul>{_DivergentProcessList}</ul>
                    <p>Total de processos: {_ProcessCount2}</p><hr>
                <h2>Lista de processos pendentes para distribuição</h2>
                    <ul>{_WaitingForProcessList}</ul>
                    <p>Total de processos: {_ProcessCount3}</p>
                ";

            return new SendEmail
            {
                to = await GetEmailUsersByProfile("all", cancellationToken),
                bodyMessage = bodyMessage
            };
        }

        public async Task<IReadOnlyCollection<string>> GetEmailUsersByProfile(string profile, CancellationToken ct)
        {
            try
            {
                return !profile.ToLower().Equals("all")
                    ? (await GetAll<Users>(ct))
                               .Where(x => x.Role.ToLower().Equals(profile))
                               .Select(y => y.Email).ToList()
                    : (await GetAll<Users>(ct))
                                .Where(x => !x.Role.Equals("admin"))
                               .Select(y => y.Email).ToList();
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

        [FirestoreProperty("deadline_court_date")]
        public string CourtDate { get; set; }

        [FirestoreProperty("deadline_internal_date")]
        public string InternalDate { get; set; }

        [FirestoreProperty("deadline_interpreter")]
        public string Interpreter { get; set; }

        public Deadline()
        {

        }
    }

    public class FirebaseSettings
    {
        [JsonPropertyName("project_id")]
        public string ProjectId => "";

        [JsonPropertyName("private_key_id")]
        public string PrivateKeyId => "";
    }

    public class SendEmail
    {
        public IEnumerable<string> to { get; set; }
        public string bodyMessage { get; set; }
    }
}
