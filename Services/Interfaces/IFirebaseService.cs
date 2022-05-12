using System.Threading.Tasks;

namespace upsa_api.Services.Interfaces
{
    public interface IFirebaseService
    {
        Task<bool> SendMail();
    }
}