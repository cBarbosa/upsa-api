using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using upsa_api.Models;
using upsa_api.Services.Interfaces;

namespace upsa_api.Services
{
    public class MessageService : IMessageService
    {
        private readonly ILogger<MessageService> logger;
        private readonly IEmailService _emailService;
        private readonly FirebaseService _firebaseService;


        public MessageService(ILogger<MessageService> _logger,
            IEmailService emailService,
            FirebaseService firebaseService)
        {
            _emailService = emailService;
            _firebaseService = firebaseService;
            logger = _logger;
        }

        public async Task<bool> SendNotifyToAvocados(NotifyAvocadoModel notify)
        {
            var body = BindSendMessage(notify);
            var _bcc = new List<string> { "charles.barbosa@smile.tec.br" };
            var _avocados = await _firebaseService.GetEmailUsersByProfile("all", new CancellationToken());

            try
            {
                await _emailService.SendMailAsync(_avocados, null, _bcc, null, "Divergência de processo", body, 3);
                return true;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw ex;
            }
        }

        public async Task<bool> SendDaylyNotification()
        {
            var _bcc = new List<string> { "charles.barbosa@smile.tec.br" };

            try
            {
                var result = await _firebaseService.GetReportEmail();
                await _emailService.SendMailAsync(result.to, null, _bcc, null, "Relatório de Processos", result.bodyMessage, 3);
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
                <strong>Prazo 1</strong> {notify?.InternalDate1} - <strong>Prazo 2</strong> {notify?.InternalDate2}<br />

                <h4>Prazo Judicial</h4>
                <strong>Prazo 1</strong> {notify?.CourtDate1} - <strong>Prazo 2</strong> {notify?.CourtDate2}<br />
                <hr>
            ";
    }
}