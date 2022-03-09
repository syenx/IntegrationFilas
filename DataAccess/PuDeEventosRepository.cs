using Amazon.SecretsManager;
using apirabbit.DataAccess;
using Dapper;
using EDM.Infohub.BPO.Services.impl;
using EDM.Infohub.Integration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.DataAccess
{
    public class PuDeEventosRepository : SqlRepository<PuDeEventos>
    {
        private ILogger<PuDeEventosRepository> _logger;
        private readonly DatabaseService _databaseService;

        public PuDeEventosRepository(IConfiguration configuration, IAmazonSecretsManager secret, ILogger<PuDeEventosRepository> logger, DatabaseService databaseService)
        {
            this._logger = logger;
            this._config = configuration;
            this._secret = secret;
            _databaseService = databaseService;
        }

        public PuDeEventos Get(string cdInstrumento, DateTime dtLiquidacao, string tipoEvento, DateTime dtReferencia)
        {
            try
            {
                var puDeEventos = new PuDeEventos();
                puDeEventos = Connection.QueryFirstOrDefault<PuDeEventos>(  "SELECT * " +
                                                                            "FROM db_cad..pu_de_eventos " +
                                                                            "WHERE Codigo_Instrumento = '" + cdInstrumento +
                                                                                "' AND DataLiquidacao = '" + dtLiquidacao.ToString("yyyy/MM/dd") +
                                                                                "' AND Tipo_Evento = '" + tipoEvento +
                                                                                "' AND Data_Referencia = '" + dtReferencia.ToString("yyyy/MM/dd") + "'");
                return puDeEventos;
            }
            catch (Exception e)
            {
                _logger.LogError("Nao foi possivel executar o metodo get pu de eventos: " + e.Message);
                throw e;
            }
            finally 
            {
                CloseConnection();
            }
        }

        public int GerarPuEventoOperacao(PuDeEventos PUConfirmation)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@data_referencia", PUConfirmation.data_referencia, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                param.Add("@codigo_instrumento", PUConfirmation.codigo_instrumento, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@tipo_evento", PUConfirmation.tipo_evento, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@pu_do_evento", 0, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                param.Add("@pu_do_evento_open", PUConfirmation.pu_do_evento_open, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                param.Add("@situacao", "", System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@mensagem_deLiquidacao", "Sem informação da CETIP.", System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@data_movimento", DateTime.Now, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                param.Add("@usuario", _config["Usuario"].Substring(0, 8), System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@situacao_interna", "PEN", System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@pu_liquidacao", 0, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                param.Add("@saldo_nominal", 0, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                param.Add("@saldo_par", 0, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                param.Add("@incorporar_diferenca", null, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@usuario_confirmacao", null, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@data_confirmacao", null, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                param.Add("@renda_tributavel", 0, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                param.Add("@observacao", null, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@tipo_liquidacao", null, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@data_liquidacao", PUConfirmation.dataLiquidacao, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                param.Add("@tipo_premio", null, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                string query = "INSERT INTO Pu_De_Eventos (DATA_REFERENCIA, CODIGO_INSTRUMENTO, TIPO_EVENTO, PU_DO_EVENTO, PU_DO_EVENTO_OPEN, SITUACAO, MENSAGEM_DE_LIQUIDACAO, DATA_MOVIMENTO, USUARIO, SITUACAOINTERNA, PULIQUIDACAO, SALDONOMINAL, SALDOPAR, INCORPORARDIFERENCA, USUARIOCONFIRMACAO, DATACONFIRMACAO, RENDATRIBUTAVEL, OBSERVACAO, DATALIQUIDACAO, TIPOLIQUIDACAO, TIPOPREMIO) " +
                    "VALUES (@data_referencia, @codigo_instrumento, @tipo_evento, @pu_do_evento, @pu_do_evento_open, @situacao, @mensagem_deLiquidacao, @data_movimento, @usuario, @situacao_interna, @pu_liquidacao, @saldo_nominal, @saldo_par, @incorporar_diferenca, @usuario_confirmacao, @data_confirmacao, @renda_tributavel, @observacao, @data_liquidacao, @tipo_liquidacao, @tipo_premio)";

                Connection.Query<PuDeEventos>(query, param);
                return Get(PUConfirmation.codigo_instrumento, PUConfirmation.dataLiquidacao, PUConfirmation.tipo_evento, PUConfirmation.data_referencia).idEvento;
            }
            catch (Exception e)
            {
                _logger.LogError("Nao foi possivel gerar um novo pu de eventos");
                throw e;
            }
            finally
            {
                CloseConnection();
            }
        }

        public void UpdatePuDeEventos(PuDeEventos PUConfirmation)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@pu_do_evento_open", PUConfirmation.pu_do_evento_open, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                param.Add("@data_movimento", DateTime.Now, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                param.Add("@usuario", _config["Usuario"].Substring(0, 8), System.Data.DbType.String, System.Data.ParameterDirection.Input);
                param.Add("@id_evento", PUConfirmation.idEvento, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                string query = "UPDATE Pu_De_Eventos " +
                                "SET Pu_do_Evento_Open = @pu_do_evento_open, Data_Movimento = @data_movimento, Usuario = @usuario " +
                                "WHERE IdEvento = @id_evento";
                Connection.Query<PuDeEventos>(query, param);
            }
            catch (Exception e)
            {
                _logger.LogError("Nao foi possivel atualizar o pu de eventos");
                throw e;
            }
            finally
            {
                CloseConnection();
            }
        }

        public void DeletePuDeEventos(int idEvento)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@id_evento", idEvento, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                string query = "DELETE FROM Pu_De_Eventos " +
                               "WHERE IdEvento = @id_evento";
                Connection.Query<PuDeEventos>(query, param);
            }
            catch (Exception e)
            {
                _logger.LogError("Nao foi possivel excluir o pu de eventos");
                throw e;
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}