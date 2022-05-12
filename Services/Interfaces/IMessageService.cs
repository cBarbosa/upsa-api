using System.Collections.Generic;
using System.Threading.Tasks;
using upsa_api.Models;

namespace upsa_api.Services.Interfaces
{
    public interface IMessageService
    {
        Task<bool> SendNotifyToAvocados(NotifyAvocadoModel notify);
        Task<bool> SendDaylyNotification();
    }
}