using System;
using System.Collections.Generic;
using Massive.SqlServer;
using Microsoft.Extensions.Configuration;

namespace Common.DataAccess
{
    public interface IMassiveOrmDataAccess 
    {
        Func<string, DynamicModel> FromTable { get; }
    }

    public class MassiveOrmDataAccess : IMassiveOrmDataAccess
    {
        public Func<string, DynamicModel> FromTable {get;}
        public MassiveOrmDataAccess(IConnectionStringProvider connectionStringProvider) =>
            FromTable = tableName => new DynamicModel("ecom-conn-str", tableName: tableName, connectionStringProvider: connectionStringProvider);
    }

    public class AppSettingsBasedConfigurationProvider : IConnectionStringProvider
    {
        private readonly IConfiguration _config;
        public AppSettingsBasedConfigurationProvider(IConfiguration config) => _config = config;
            
        public string GetConnectionString(string connectionStringName) => _config.GetValue<string>(connectionStringName);
        public string GetProviderName(string connectionStringName) => "System.Data.SqlClient";
    }
}