using EDM.Infohub.Integration.DataAccess;
using EDM.Infohub.Integration.Models;
using EDM.Infohub.Integration.Services;
using EDM.Infohub.Integration.Services.Impl;
using EDMFixedIncomeOnService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EDM.Infohub.Integration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IEDMCommonService _edmCommonService;
        private IFixedIncomeOn _fixedIncomeOn;
        private PuDeEventosRepository _puDeEventos;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IEDMCommonService edmCommonService, IFixedIncomeOn fixedIncomeOn, PuDeEventosRepository puDeEventos)
        {
            _logger = logger;
            _edmCommonService = edmCommonService;
            _fixedIncomeOn = fixedIncomeOn;
            _puDeEventos = puDeEventos;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var teste = _fixedIncomeOn.GetFIOnInstrumentBySNA("LUZ-BISA22");
            teste[0].AtivoTvm = false;
            teste[0].Usuario = "Integration";

            teste[0].CgeFiduciario = 1234;
            teste[0].CodIndice = "IPCA";
            teste[0].CurrencyRateFrequency = "Mes";
            teste[0].CurrencyRateType = "JUROS";
            foreach (var curva in teste[0].Curvas) {
                curva.CodInstrumento = teste[0].CodCetip;
                curva.CurvaPrincipal = teste[0].CodIndice;
                curva.DataBase = DateTime.Now;
            }
            foreach (var fluxo in teste[0].Fluxos)
            {
                fluxo.CodInstrumento = teste[0].CodCetip;
                fluxo.CodIndice = teste[0].CodIndice;
                fluxo.Usuario = "Integration";
            }
            teste[0].Excecoes = teste[0].Excecoes == null ? new List<FIOnInstrumentExceptionDataContract>().ToArray() : teste[0].Excecoes ;
            return Ok(_fixedIncomeOn.InsertUpdateInstrument(teste[0]));
        }

        [HttpGet]
        [Route("teste")]
        public void Get1()
        {
            //var pu =_puDeEventos.Get("CVSB970101", new DateTime(1997, 02, 01), "Incorporacao", new DateTime(1997, 02, 01));

        }
    }
}
