using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using upsa_api.Models;
using upsa_api.Services.Interfaces;

namespace upsa_api.Services
{
    public class MessageService : IMessageService
    {
        private readonly ILogger<MessageService> logger;
        private readonly IEmailService emailService;
        private readonly IThemisService _themisService;
        

        public MessageService(ILogger<MessageService> _logger,
            IEmailService _emailService,
            IThemisService themisService)
        {
            emailService = _emailService;
            _themisService = themisService;
            logger = _logger;
        }

        public async Task<bool> SendNotifyToAvocados(NotifyAvocadoModel notify)
        {
            var body = BindSendMessage(notify);

            var person1 = await _themisService.GetPerson(666); // Julio
            if (person1 == null)
                return false;

            var person2 = await _themisService.GetPerson(663); // Manoel Valter

            try
            {
                await emailService.SendMailAsync(new List<string> { person1.Email, person2?.Email }, null, null, null, "Divergência de processo", body, 3);
                return true;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw ex;
            }
        }

        private string BindSendMessage(NotifyAvocadoModel notify) => $@"
                <h3>Divergência no processo {notify?.ProcessNumber}</h3>

                <h4>Observação</h4>
                <em>{notify?.Observation}</em>
                
                <h4>Prazo Interno</h4>
                <strong>Prazo 1</strong> {notify?.InternalDate1} - <strong>Prazo 2</strong> {notify?.InternalDate2}<BR/>

                <h4>Prazo Judicial</h4>
                <strong>Prazo 1</strong> {notify?.CourtDate1} - <strong>Prazo 2</strong> {notify?.CourtDate2}<BR/>
            ";
    }
}