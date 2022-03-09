using EDM.Infohub.BPO.RabbitMQ;
using EDM.Infohub.Integration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace EDM.Infohub.Integration.Services.Impl
{
    public class EDMLuzService : IEDMLuzService
    {
        private readonly ILogger<EDMLuzService> _logger;
        private readonly HttpClient _client;
        private readonly IConfiguration _config;


        public EDMLuzService(IConfiguration config, ILogger<EDMLuzService> logger)
        {
            _logger = logger;
            _config = config;
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_config["InfohubLuzUri"]);
            //client.DefaultRequestHeaders.Add("Authorization", _config["LuzSettings:Token"]);
        }
        public async Task<List<Fluxos>> FluxoPapel(DateTime data, string papel)
        {
            // RelatorioPapelFluxos([FromQuery] DateTime data, [FromQuery] string papel)
            try
            {
                string Uri = "v1/Fluxo/" + papel + "?data=" + data.ToString("yyyy-MM-dd");
                HttpResponseMessage response = await _client.GetAsync(Uri);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);
                _logger.LogInformation(responseBody);
                return JsonConvert.DeserializeObject<List<Fluxos>>(responseBody); ;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Os fluxos do ativo {papel} para a data {data} nao foram encontrados: " + e.Message);
                return new List<Fluxos>();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw e;
            }
        }

        public async Task<List<Fluxos>> FluxoPapel(DateTime data, string papel, string idRequisicaoEvento, string dataRequisicaoRastreamento, string usuarioRastreamento, bool papelHomologacao)
        {
            // RelatorioPapelFluxos([FromQuery] DateTime data, [FromQuery] string papel)
            try
            {
                string Uri = "v1/Fluxo/" + papel + "?data=" + data.ToString("yyyy-MM-dd") + "&dataRastreamento=" + dataRequisicaoRastreamento + "&usuario=" + usuarioRastreamento + "&idEvento=" + idRequisicaoEvento + "&homolog=" + papelHomologacao;
                HttpResponseMessage response = await _client.GetAsync(Uri);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);
                _logger.LogInformation(responseBody);
                return JsonConvert.DeserializeObject<List<Fluxos>>(responseBody); ;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Os fluxos do ativo {papel} para a data {data} nao foram encontrados: " + e.Message);
                return new List<Fluxos>();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw e;
            }
        }

        public async Task<DadosCaracteristicos> CadastroPapel(string papel)
        {
            try
            {
                string Uri = "v1/Caracteristica/" + papel;
                HttpResponseMessage response = await _client.GetAsync(Uri);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);
                _logger.LogInformation(responseBody);
                return JsonConvert.DeserializeObject<DadosCaracteristicos>(responseBody); ;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"O cadastro do ativo {papel} nao foi encontrado: " + e.Message);
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw e;
            }
        }
    }
}
