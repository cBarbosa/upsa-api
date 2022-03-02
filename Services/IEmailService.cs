using System.Collections.Generic;
using System.Threading.Tasks;

namespace upsa_api.Services
{
    public interface IEmailService
    {
        Task SendMailAsync(
            IEnumerable<string> to
            , IEnumerable<string> cc
            , IEnumerable<string> bcc
            , IEnumerable<string> attachment
            , string subject
            , string body
            , int retryCount);
    }
}