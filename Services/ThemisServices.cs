using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using upsa_api.Services.Interfaces;

namespace upsa_api.Services
{
    public class ThemisService : IThemisService
    {
        private readonly ILogger<ThemisService> _logger;
        private readonly IEmailService _emailService;
        private readonly string hostBaseAPI = "https://upsa.themisweb.penso.com.br/upsa/api/";
        private readonly string headerAuthAPI = "X-Aurum-Auth";
        private readonly string headerUserNamePasswordAPI = "cHJhem9zOjc5N2VhNzU5MGM5NGMwNzk1YTI5YThmNDI5OTMzNGQ1";

        public ThemisService(ILogger<ThemisService> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<bool> AddProcessFoward(
            string number,
            AndamentoProcessoInput andamento)
        {
            #region verifying entry data
            if (andamento.Advogado == null
                || andamento.Advogado.Id.Equals(0)
                || string.IsNullOrEmpty(andamento.Data)
                || string.IsNullOrEmpty(andamento.DataJudicial)
                || string.IsNullOrEmpty(andamento.Hora)
                || string.IsNullOrEmpty(andamento.Descricao)
                || DateTime.Compare(DateTime.Today, DateTime.Parse(andamento.Data)) >= 0
                || DateTime.Compare(DateTime.Parse(andamento.Data), DateTime.Parse(andamento.DataJudicial)) >= 0
                )
            {
                return false;
            }
            #endregion

            var process = await GetProcess(number);

            if (process == null)
                return false;

            var person = await GetPerson(andamento.Advogado.Id);

            if (person == null)
                return false;

            if (!process.Desdobramentos.Any())
                return false;

            try
            {
                var resultInterno = await AddAndamentoPorStatus(andamento, process.Desdobramentos.First().Id, 6);

                if (resultInterno == null)
                    return false;

                var resultadoJudicial = await AddAndamentoPorStatus(andamento, process.Desdobramentos.First().Id, 12);

                if (resultadoJudicial == null)
                    return false;

                //return await SendMessage("xbrown@gmail.com", resultInterno, resultadoJudicial);
                return await SendMessage(person?.Email ?? "xbrown@gmail.com", resultInterno, resultadoJudicial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<Processo> GetProcess(string number)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            try
            {

                var handler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Automatic,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                };
                using var http = new HttpClient(handler);
                http.DefaultRequestHeaders.Add(headerAuthAPI, headerUserNamePasswordAPI);
                var result = await http.GetAsync(new Uri($"{hostBaseAPI}/processos/numero/{number}/json"));

                var resultContent = await result.Content.ReadAsStringAsync();

                if (result.StatusCode != HttpStatusCode.OK)
                    return null;

                return JsonSerializer.Deserialize<Processo[]>(resultContent, options).Length > 0
                    ? JsonSerializer.Deserialize<Processo[]>(resultContent, options)[0]
                    : null;
            }
            catch (HttpRequestException re)
            {
                _logger.LogError(re, "HttpRequestException when calling the API");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when calling the API");
                throw;
            }
        }

        public async Task<Processo.Pessoa> GetPerson(int personId)
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            try
            {
                var handler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Automatic,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                };
                using var http = new HttpClient(handler);
                http.DefaultRequestHeaders.Add(headerAuthAPI, headerUserNamePasswordAPI);
                var result = await http.GetAsync(new Uri($"{hostBaseAPI}/pessoa/{personId}/json"));

                var resultContent = await result.Content.ReadAsStringAsync();

                if (result.StatusCode != HttpStatusCode.OK)
                    return null;

                return JsonSerializer.Deserialize<Processo.Pessoa>(resultContent, options);
            }
            catch (HttpRequestException re)
            {
                _logger.LogError(re, "HttpRequestException when calling the API");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when calling the API");
                throw;
            }
        }

        public async Task<Processo> PostProcess(Processo process)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                var handler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Automatic,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                };
                using var http = new HttpClient(handler);
                http.DefaultRequestHeaders.Add(headerAuthAPI, headerUserNamePasswordAPI);
                var _content = new StringContent(JsonSerializer.Serialize(process, options), Encoding.UTF8, "application/json");
                var result = await http.PostAsync(new Uri($"{hostBaseAPI}processo/novo/json"), _content);
                var resultContent = await result.Content.ReadAsStringAsync();

                if (result.StatusCode != HttpStatusCode.OK)
                    return null;

                return await GetProcess(process.Numero);
            }
            catch (HttpRequestException re)
            {
                _logger.LogError(re, "HttpRequestException when calling the API");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when calling the API");
                throw;
            }
        }

        #region private methods

        private async Task<AndamentoProcesso> AddAndamentoPorStatus(
            AndamentoProcessoInput andamento,
            int desdobramentoId,
            int tipoAndamento)
        {
            var _andamento = new AndamentoProcessoInput
            {
                Data = tipoAndamento.Equals(6) ? andamento.Data : andamento.DataJudicial,
                Descricao = andamento.Descricao,
                Advogado = new Processo.Pessoa { Id = andamento.Advogado.Id },
                Desdobramento = new Processo.Desdobramento { Id = desdobramentoId },
                Tipo = new Processo.Pessoa { Id = tipoAndamento }
            };

            return await ProcessFoward(_andamento);
        }

        private async Task<AndamentoProcesso> ProcessFoward(AndamentoProcessoInput andamento)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                var handler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Automatic,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                };
                using var http = new HttpClient(handler);
                http.DefaultRequestHeaders.Add(headerAuthAPI, headerUserNamePasswordAPI);
                var _content = new StringContent(JsonSerializer.Serialize(andamento, options), Encoding.UTF8, "application/json");
                var result = await http.PostAsync(new Uri($"{hostBaseAPI}/andamentos/novo/json"), _content);
                var resultContent = await result.Content.ReadAsStringAsync();

                if (result.StatusCode != HttpStatusCode.OK)
                    return null;

                return JsonSerializer.Deserialize<AndamentoProcesso>(resultContent, options);
            }
            catch (HttpRequestException re)
            {
                _logger.LogError(re, "HttpRequestException when calling the API");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when calling the API");
                throw;
            }
        }

        private async Task<bool> SendMessage(
            string fromMail,
            AndamentoProcesso resultInterno,
            AndamentoProcesso resultadoJudicial)
        {
            var _htmlBodyMessage = $@"
                <h3>Distribuição - {resultInterno?.Processo}</h3>
                <strong>Advogado Responsável:</strong> {resultInterno?.AdvogadoNome}<BR/>
                <strong>Cliente:</strong> {resultInterno?.ClienteNome}<BR/>
                <strong>Descrição:</strong> {resultInterno?.Descricao}<BR/>
                <strong>Data Inclusão:</strong> {resultInterno?.DataInclusao}<BR/>

                <h4>{resultInterno?.TipoNome}</h4>
                <strong>Data Hora:</strong> {resultInterno?.DataFormatada} {resultInterno?.HoraFormatada}<BR/>

                <h4>{resultadoJudicial?.TipoNome}</h4>
                <strong>Data Hora:</strong> {resultadoJudicial?.DataFormatada} {resultadoJudicial?.HoraFormatada}<BR/>
            ";

            try
            {
                await _emailService.SendMailAsync(new List<string> { fromMail }, null, null, null, "Distribuição de processo", _htmlBodyMessage, 3);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return true;
        }

        #endregion

        #region internal domain classes

        public class Processo
        {
            public int? Id { get; set; }
            public string Numero { get; set; }
            public string Tipo { get; set; }
            public string ParteAtiva { get; set; }
            public int Oculto { get; set; }
            public string Titulo { get; set; }
            public string Resumo { get; set; }
            public string Resumo2 { get; set; }
            public string Observacao { get; set; }
            public string Status { get; set; }
            public string StatusDescricao { get; set; }
            public string DataCadastro { get; set; }
            public string DataAtuGeral { get; set; }
            public string DataEntrada { get; set; }
            public string DataInicio { get; set; }
            public string DataDistribuicao { get; set; }
            public string DataEncerramento { get; set; }
            public Pessoa Advogado { get; set; }
            public Pessoa Cliente { get; set; }
            public Pessoa ParteInteressada { get; set; }
            public Pessoa ParteContraria { get; set; }
            public Pessoa Area { get; set; }
            public Pessoa Acao { get; set; }
            public Pessoa Dominio { get; set; }
            public Pessoa Instancia { get; set; }
            public Pessoa PosicaoParte { get; set; }
            public Pessoa UsuarioEncerramento { get; set; }
            public IEnumerable<Desdobramento> Desdobramentos { get; set; }

            public class Pessoa
            {
                public int Id { get; set; }
                public string Nome { get; set; }
                public int? TipoPessoa { get; set; }
                public string Email { get; set; }
                public string Ativo { get; set; }
            }

            //public class Tipo
            //{
            //    public int Id { get; set; }
            //    public string Nome { get; set; }
            //    public int? TipoPessoa { get; set; }
            //    public string Email { get; set; }
            //    public string Ativo { get; set; }
            //}

            public class Desdobramento
            {
                public int Id { get; set; }
            }
        }

        public class AndamentoProcesso
        {
            public int Id { get; set; }
            public int? ProcessoId { get; set; }
            public string Data { get; set; }
            public string DataFormatada { get; set; }
            public string HoraFormatada { get; set; }
            public string Descricao { get; set; }
            public string DescricaoCortada { get; set; }
            //public Processo.Pessoa Advogado { get; set; }
            public string AdvogadoNome { get; set; }
            //public Processo.Tipo Tipo { get; set; }
            public string TipoNome { get; set; }
            public int? IdDesdobramento { get; set; }
            public Processo.Desdobramento Desdobramento { get; set; }
            public string ClienteNome { get; set; }
            public string Processo { get; set; }
            public bool Pendente { get; set; }
            public string DataInclusao { get; set; }
            public string DataOrdem { get; set; }
            public string Hora { get; set; }
        }

        public class AndamentoProcessoInput
        {
            public string Data { get; set; }
            public string DataJudicial { get; set; }
            public string Descricao { get; set; }
            public Processo.Pessoa Advogado { get; set; }
            public Processo.Pessoa Tipo { get; set; }
            public Processo.Desdobramento Desdobramento { get; set; }
            public string Hora => "10:30";
        }

        #endregion
    }
}
