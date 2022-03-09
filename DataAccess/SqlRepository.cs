using Amazon.SecretsManager;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using EDM.Infohub.BPO.DataAccess;
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using EDM.Infohub.Integration;
using EDM.Infohub.BPO.Services.impl;

namespace apirabbit.DataAccess
{
    public abstract class SqlRepository<TEntity> : IRepository where TEntity : class
    {

        public IConfiguration _config;
        public IAmazonSecretsManager _secret;
        private SqlConnection _connection;
        protected string TableName { get; private set; }
        private string ConnString = null;

        public SqlRepository()
        {
            var type = typeof(TEntity);
            var attribute = type.GetCustomAttribute<TableAttribute>();
            if (attribute == null)
                throw new MissingFieldException($"The Type {type.Name} does not has the TableAttribute");

            this.TableName = attribute.Name;
        }

        protected SqlConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    ConnString ??= DatabaseService.GetSecret(_config, _secret, "Base");
                    _connection = new SqlConnection(ConnString);
                }

                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                return _connection;

            }
        }

        public virtual TEntity Get(TEntity item) => Connection.Get<TEntity>(item);
        public virtual IEnumerable<TEntity> GetAll() => Connection.GetAll<TEntity>();
        public virtual bool Update(TEntity item) => Connection.Update(item);
        public virtual bool Delete(TEntity item) => Connection.Delete<TEntity>(item);
        public virtual long Insert(TEntity item) => Connection.Insert<TEntity>(item);

        public override void Dispose()
        {
            this._connection?.Close();
            this._connection = null;
        }

        public void CloseConnection()
        {
            this._connection?.Close();
        }
    }
}
