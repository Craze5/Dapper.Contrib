using Dm;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Xunit;
using Xunit.Sdk;

namespace Dapper.Tests.Contrib
{
    // The test suites here implement TestSuiteBase so that each provider runs
    // the entire set of tests without declarations per method
    // If we want to support a new provider, they need only be added here - not in multiple places

    [XunitTestCaseDiscoverer("Dapper.Tests.SkippableFactDiscoverer", "Dapper.Tests.Contrib")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SkippableFactAttribute : FactAttribute
    {
    }

    public class SqlServerTestSuite : TestSuite
    {
        private const string DbName = "tempdb";
        public static string ConnectionString =>
            GetConnectionString("SqlServerConnectionString", $"Data Source=127.0.0.1;Initial Catalog={DbName};User ID=sa;Password=Db@2026#123!;");

        public override IDbConnection GetConnection() => new SqlConnection(ConnectionString);

        static SqlServerTestSuite()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                // ReSharper disable once AccessToDisposedClosure
                void dropTable(string name) => connection.Execute($"IF OBJECT_ID('{name}', 'U') IS NOT NULL DROP TABLE [{name}]; ");
                connection.Open();
                dropTable("Stuff");
                connection.Execute("CREATE TABLE Stuff (TheId int IDENTITY(1,1) not null, Name nvarchar(100) not null, Created DateTime null);");
                dropTable("People");
                connection.Execute("CREATE TABLE People (Id int IDENTITY(1,1) not null, Name nvarchar(100) not null);");
                dropTable("Users");
                connection.Execute("CREATE TABLE Users (Id int IDENTITY(1,1) not null, Name nvarchar(100) not null, Age int not null);");
                dropTable("Automobiles");
                connection.Execute("CREATE TABLE Automobiles (Id int IDENTITY(1,1) not null, Name nvarchar(100) not null);");
                dropTable("Results");
                connection.Execute("CREATE TABLE Results (Id int IDENTITY(1,1) not null, Name nvarchar(100) not null, [Order] int not null);");
                dropTable("ObjectX");
                connection.Execute("CREATE TABLE ObjectX (ObjectXId nvarchar(100) not null, Name nvarchar(100) not null);");
                dropTable("ObjectY");
                connection.Execute("CREATE TABLE ObjectY (ObjectYId int not null, Name nvarchar(100) not null);");
                dropTable("ObjectZ");
                connection.Execute("CREATE TABLE ObjectZ (Id int not null, Name nvarchar(100) not null);");
                dropTable("GenericType");
                connection.Execute("CREATE TABLE GenericType (Id nvarchar(100) not null, Name nvarchar(100) not null);");
                dropTable("NullableDates");
                connection.Execute("CREATE TABLE NullableDates (Id int IDENTITY(1,1) not null, DateValue DateTime null);");
            }
        }
    }

    public class MySqlServerTestSuite : TestSuite
    {
        public static string ConnectionString { get; } =
            GetConnectionString("MySqlConnectionString", $"Server=127.0.0.1;Database=tests;Uid=root;Pwd=Db@2026#123!;UseAffectedRows=false;");

        public override IDbConnection GetConnection()
        {
            if (_skip) Skip.Inconclusive("Skipping MySQL Tests - no server.");
            return new MySqlConnection(ConnectionString);
        }

        protected override string QuoteIdentifier(string identifier) => $"`{identifier}`";

        private static readonly bool _skip;

        static MySqlServerTestSuite()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    // ReSharper disable once AccessToDisposedClosure
                    void dropTable(string name) => connection.Execute($"DROP TABLE IF EXISTS `{name}`;");
                    connection.Open();
                    dropTable("Stuff");
                    connection.Execute("CREATE TABLE Stuff (TheId int not null AUTO_INCREMENT PRIMARY KEY, Name nvarchar(100) not null, Created DateTime null);");
                    dropTable("People");
                    connection.Execute("CREATE TABLE People (Id int not null AUTO_INCREMENT PRIMARY KEY, Name nvarchar(100) not null);");
                    dropTable("Users");
                    connection.Execute("CREATE TABLE Users (Id int not null AUTO_INCREMENT PRIMARY KEY, Name nvarchar(100) not null, Age int not null);");
                    dropTable("Automobiles");
                    connection.Execute("CREATE TABLE Automobiles (Id int not null AUTO_INCREMENT PRIMARY KEY, Name nvarchar(100) not null);");
                    dropTable("Results");
                    connection.Execute("CREATE TABLE Results (Id int not null AUTO_INCREMENT PRIMARY KEY, Name nvarchar(100) not null, `Order` int not null);");
                    dropTable("ObjectX");
                    connection.Execute("CREATE TABLE ObjectX (ObjectXId nvarchar(100) not null, Name nvarchar(100) not null);");
                    dropTable("ObjectY");
                    connection.Execute("CREATE TABLE ObjectY (ObjectYId int not null, Name nvarchar(100) not null);");
                    dropTable("ObjectZ");
                    connection.Execute("CREATE TABLE ObjectZ (Id int not null, Name nvarchar(100) not null);");
                    dropTable("GenericType");
                    connection.Execute("CREATE TABLE GenericType (Id nvarchar(100) not null, Name nvarchar(100) not null);");
                    dropTable("NullableDates");
                    connection.Execute("CREATE TABLE NullableDates (Id int not null AUTO_INCREMENT PRIMARY KEY, DateValue DateTime);");
                }
            }
            catch (MySqlException e)
            {
                if (e.Message.Contains("Unable to connect"))
                    _skip = true;
                else
                    throw;
            }
        }
    }

    public class PostgresTestSuite : TestSuite
    {
        public static string ConnectionString { get; } =
            GetConnectionString("PostgresConnectionString", $"Host=127.0.0.1;Database=tests;Username=postgres;Password=Db@2026#123!;");

        public override IDbConnection GetConnection()
        {
            if (_skip) Skip.Inconclusive("Skipping PostgreSQL Tests - no server.");
            return new NpgsqlConnection(ConnectionString);
        }

        private static readonly bool _skip;

        static PostgresTestSuite()
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    // ReSharper disable once AccessToDisposedClosure
                    void dropTable(string name) => connection.Execute($"DROP TABLE IF EXISTS {name};");
                    connection.Open();
                    dropTable("Stuff");
                    connection.Execute("CREATE TABLE Stuff (\"TheId\" serial NOT NULL PRIMARY KEY, \"Name\" varchar(100) NOT NULL, \"Created\" timestamp NULL);");
                    dropTable("People");
                    connection.Execute("CREATE TABLE People (\"Id\" serial NOT NULL PRIMARY KEY, \"Name\" varchar(100) NOT NULL);");
                    dropTable("Users");
                    connection.Execute("CREATE TABLE Users (\"Id\" serial NOT NULL PRIMARY KEY, \"Name\" varchar(100) NOT NULL, \"Age\" int NOT NULL);");
                    dropTable("Automobiles");
                    connection.Execute("CREATE TABLE Automobiles (\"Id\" serial NOT NULL PRIMARY KEY, \"Name\" varchar(100) NOT NULL);");
                    dropTable("Results");
                    connection.Execute("CREATE TABLE Results (\"Id\" serial NOT NULL PRIMARY KEY, \"Name\" varchar(100) NOT NULL, \"Order\" int NOT NULL);");
                    dropTable("ObjectX");
                    connection.Execute("CREATE TABLE ObjectX (\"ObjectXId\" varchar(100) NOT NULL, \"Name\" varchar(100) NOT NULL);");
                    dropTable("ObjectY");
                    connection.Execute("CREATE TABLE ObjectY (\"ObjectYId\" int NOT NULL, \"Name\" varchar(100) NOT NULL);");
                    dropTable("ObjectZ");
                    connection.Execute("CREATE TABLE ObjectZ (\"Id\" int NOT NULL, \"Name\" varchar(100) NOT NULL);");
                    dropTable("GenericType");
                    connection.Execute("CREATE TABLE GenericType (\"Id\" varchar(100) NOT NULL, \"Name\" varchar(100) NOT NULL);");
                    dropTable("NullableDates");
                    connection.Execute("CREATE TABLE NullableDates (\"Id\" serial NOT NULL PRIMARY KEY, \"DateValue\" timestamp NULL);");
                }
            }
            catch (NpgsqlException e)
            {
                if (e.Message.Contains("connect", StringComparison.OrdinalIgnoreCase))
                    _skip = true;
                else
                    throw;
            }
            catch (System.Net.Sockets.SocketException)
            {
                _skip = true;
            }
        }
    }

    public class SQLiteTestSuite : TestSuite
    {
        private const string FileName = "Test.DB.sqlite";
        public static string ConnectionString => $"Filename=./{FileName};Mode=ReadWriteCreate;";
        public override IDbConnection GetConnection() => new SqliteConnection(ConnectionString);

        static SQLiteTestSuite()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                connection.Execute("CREATE TABLE Stuff (TheId integer primary key autoincrement not null, Name nvarchar(100) not null, Created DateTime null) ");
                connection.Execute("CREATE TABLE People (Id integer primary key autoincrement not null, Name nvarchar(100) not null) ");
                connection.Execute("CREATE TABLE Users (Id integer primary key autoincrement not null, Name nvarchar(100) not null, Age int not null) ");
                connection.Execute("CREATE TABLE Automobiles (Id integer primary key autoincrement not null, Name nvarchar(100) not null) ");
                connection.Execute("CREATE TABLE Results (Id integer primary key autoincrement not null, Name nvarchar(100) not null, [Order] int not null) ");
                connection.Execute("CREATE TABLE ObjectX (ObjectXId nvarchar(100) not null, Name nvarchar(100) not null) ");
                connection.Execute("CREATE TABLE ObjectY (ObjectYId integer not null, Name nvarchar(100) not null) ");
                connection.Execute("CREATE TABLE ObjectZ (Id integer not null, Name nvarchar(100) not null) ");
                connection.Execute("CREATE TABLE GenericType (Id nvarchar(100) not null, Name nvarchar(100) not null) ");
                connection.Execute("CREATE TABLE NullableDates (Id integer primary key autoincrement not null, DateValue DateTime) ");
            }
        }
    }

    public class DmTestSuite : TestSuite
    {
        public static string ConnectionString { get; } =
            GetConnectionString("DmConnectionString", "Server=127.0.0.1;Port=5236;User Id=DORA_ORCH;PWD=Db@2026#123!;Schema=DORA_ORCH");

        public override IDbConnection GetConnection()
        {
            if (_skip) Skip.Inconclusive("Skipping DM (Dameng) Tests - no server.");
            return new DmConnection(ConnectionString);
        }

        protected override string ParameterPrefix() => ":";

        private static readonly bool _skip;

        static DmTestSuite()
        {
            try
            {
                using (var connection = new DmConnection(ConnectionString))
                {
                    connection.Open();

                    // DM does not support "DROP TABLE IF EXISTS" by default, so check USER_TABLES first.
                    // Table names are left unquoted (DM stores them upper-cased, matching unquoted DML),
                    // while column names are double-quoted to match DmAdapter's identifier quoting.
                    void dropTable(string name)
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = $"select count(*) from USER_TABLES where TABLE_NAME = '{name.ToUpper()}'";
                            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            {
                                cmd.CommandText = $"DROP TABLE {name}";
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    void execute(string sql)
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    dropTable("Stuff");
                    execute("CREATE TABLE Stuff (\"TheId\" int IDENTITY(1,1) not null, \"Name\" varchar(100) not null, \"Created\" timestamp null)");
                    dropTable("People");
                    execute("CREATE TABLE People (\"Id\" int IDENTITY(1,1) not null, \"Name\" varchar(100) not null)");
                    dropTable("Users");
                    execute("CREATE TABLE Users (\"Id\" int IDENTITY(1,1) not null, \"Name\" varchar(100) not null, \"Age\" int not null)");
                    dropTable("Automobiles");
                    execute("CREATE TABLE Automobiles (\"Id\" int IDENTITY(1,1) not null, \"Name\" varchar(100) not null)");
                    dropTable("Results");
                    execute("CREATE TABLE Results (\"Id\" int IDENTITY(1,1) not null, \"Name\" varchar(100) not null, \"Order\" int not null)");
                    dropTable("ObjectX");
                    execute("CREATE TABLE ObjectX (\"ObjectXId\" varchar(100) not null, \"Name\" varchar(100) not null)");
                    dropTable("ObjectY");
                    execute("CREATE TABLE ObjectY (\"ObjectYId\" int not null, \"Name\" varchar(100) not null)");
                    dropTable("ObjectZ");
                    execute("CREATE TABLE ObjectZ (\"Id\" int not null, \"Name\" varchar(100) not null)");
                    dropTable("GenericType");
                    execute("CREATE TABLE GenericType (\"Id\" varchar(100) not null, \"Name\" varchar(100) not null)");
                    dropTable("NullableDates");
                    execute("CREATE TABLE NullableDates (\"Id\" int IDENTITY(1,1) not null, \"DateValue\" timestamp null)");
                }
            }
            catch (DmException e)
            {
                if (e.Message.Contains("connect", StringComparison.OrdinalIgnoreCase) || e.Message.Contains("网络"))
                    _skip = true;
                else
                    throw;
            }
            catch (System.Net.Sockets.SocketException)
            {
                _skip = true;
            }
        }
    }


#if SQLCE
    public class SqlCETestSuite : TestSuite
    {
        const string FileName = "Test.DB.sdf";
        public static string ConnectionString => $"Data Source={FileName};";
        public override IDbConnection GetConnection() => new SqlCeConnection(ConnectionString);
            
        static SqlCETestSuite()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
            var engine = new SqlCeEngine(ConnectionString);
            engine.CreateDatabase();
            using (var connection = new SqlCeConnection(ConnectionString))
            {
                connection.Open();
                connection.Execute(@"CREATE TABLE Stuff (TheId int IDENTITY(1,1) not null, Name nvarchar(100) not null, Created DateTime null) ");
                connection.Execute(@"CREATE TABLE People (Id int IDENTITY(1,1) not null, Name nvarchar(100) not null) ");
                connection.Execute(@"CREATE TABLE Users (Id int IDENTITY(1,1) not null, Name nvarchar(100) not null, Age int not null) ");
                connection.Execute(@"CREATE TABLE Automobiles (Id int IDENTITY(1,1) not null, Name nvarchar(100) not null) ");
                connection.Execute(@"CREATE TABLE Results (Id int IDENTITY(1,1) not null, Name nvarchar(100) not null, [Order] int not null) ");
                connection.Execute(@"CREATE TABLE ObjectX (ObjectXId nvarchar(100) not null, Name nvarchar(100) not null) ");
                connection.Execute(@"CREATE TABLE ObjectY (ObjectYId int not null, Name nvarchar(100) not null) ");
                connection.Execute(@"CREATE TABLE ObjectZ (Id int not null, Name nvarchar(100) not null) ");
                connection.Execute(@"CREATE TABLE GenericType (Id nvarchar(100) not null, Name nvarchar(100) not null) ");
                connection.Execute(@"CREATE TABLE NullableDates (Id int IDENTITY(1,1) not null, DateValue DateTime null) ");
            }
            Console.WriteLine("Created database");
        }
    }
#endif
}
