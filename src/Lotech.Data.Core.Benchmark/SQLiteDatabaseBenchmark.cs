using BenchmarkDotNet.Attributes;
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Lotech.Data.Benchmark
{
    [ShortRunJob, MemoryDiagnoser]
    public class SQLiteDatabaseBenchmark
    {
        static SQLiteDatabaseBenchmark()
        {
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
        }

        public IDatabase database;
        private DbConnection connection;

        [Params(1)]
        public int Count { get; set; }

        [GlobalSetup]
        public void TearUp()
        {
            sql += Count;

            connection = new SqliteConnection { ConnectionString = "Data Source=:memory:;" };

            database = new SQLiteConnectionDatabase(connection);

            database.ExecuteNonQuery(@"create table BenchmarkDataModel (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
   Code nvarchar(32) not null,
   Name nvarchar(128) not null,
   CreateTime TIMESTAMP not null,
   ModifyTime TIMESTAMP default CURRENT_TIMESTAMP ,
   Deleted bit not null,
   LongId bigint default 10,
   Content blob null
)");
            for (int i = 0; i < Count; i++)
            {
                database.InsertEntity(new BenchmarkDataModel
                {
                    Code = i.ToString("x"),
                    Content = BitConverter.GetBytes(i),
                    CreateTime = DateTime.Now.AddMilliseconds(i),
                    Deleted = i % 2 == 0,
                    ModifyTime = null,
                    Name = i.ToString()
                });
            }
        }

        [GlobalCleanup]
        public void TearDown()
        {
            connection.Close();
        }

        string sql = "SELECT * FROM BenchmarkDataModel WHERE ID <= ";
        //[Benchmark]
        public void Raw()
        {
            var entities = new List<BenchmarkDataModel>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    var mapper = Queries.ResultMapper<BenchmarkDataModel>.Instance;
                    var state = mapper.TearUp(database, reader);
                    while (reader.Read())
                    {
                        entities.Add(mapper.Map(reader, state));
                    }
                }
            }
            entities.ToArray();
        }

        //[Benchmark]
        public void RawDateTime()
        {
            var dateTimes = new List<DateTime?>(Count);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        dateTimes.Add(reader.GetDateTime(3));
                    }
                }
            }
        }

        [Benchmark]
        public void CompleteDynamic()
        {
            var model = database.ExecuteEntities(sql);
        }

        [Benchmark]
        public void DapperDynamic()
        {
            var model = connection.Query(sql).ToArray();
        }

        [Benchmark]
        public void Complete()
        {
            var model = database.ExecuteEntities<BenchmarkDataModel>(sql);
        }

        [Benchmark]
        public void Dapper()
        {
            var model = connection.Query<BenchmarkDataModel>(sql).ToArray();
        }


        //[Benchmark]
        public void CompleteDynamicOne()
        {
            var model = database.ExecuteEntity(sql);
        }

        //[Benchmark]
        public void DapperDynamicOne()
        {
            var model = connection.QueryFirstOrDefault(sql);
        }

        //[Benchmark]
        public void CompleteOne()
        {
            var model = database.ExecuteEntity<BenchmarkDataModel>(sql);
        }

        //[Benchmark]
        public void DapperOne()
        {
            var model = connection.QueryFirstOrDefault<BenchmarkDataModel>(sql);
        }

        //[Benchmark]
        public void DapperDateTime()
        {
            connection.Query<DateTime?>("SELECT CreateTime FROM BenchmarkDataModel").ToArray();
        }

        //[Benchmark]
        public void CompleteDateTime()
        {
            database.ExecuteEntities<DateTime?>("SELECT CreateTime FROM BenchmarkDataModel");
        }

        class SQLiteConnectionDatabase : DbConnectionDatabase, IDatabase
        {

            public SQLiteConnectionDatabase(DbConnection connection) : base(connection, null) { }

            public override string BuildParameterName(string name)
            {
                return "@" + name;
            }

            public override string QuoteName(string name)
            {
                return "[" + name + "]";
            }

            public override void InsertEntity<EntityType>(EntityType entity)
            {
                var model = entity as BenchmarkDataModel;
                using (var command = GetSqlStringCommand("INSERT INTO BenchmarkDataModel"
                    + "(Code, Name, CreateTime, ModifyTime, Deleted, Content)"
                    + "VALUES(@1, @2, @3, @4, @5, @6)"))
                {
                    AddInParameter(command, "@1", System.Data.DbType.String, model.Code);
                    AddInParameter(command, "@2", System.Data.DbType.String, model.Name);
                    AddInParameter(command, "@3", System.Data.DbType.DateTime, model.CreateTime);
                    AddInParameter(command, "@4", System.Data.DbType.DateTime, model.ModifyTime);
                    AddInParameter(command, "@5", System.Data.DbType.Boolean, model.Deleted);
                    AddInParameter(command, "@6", System.Data.DbType.Binary, model.Content);
                    ExecuteNonQuery(command);
                }
            }
        }
    }
}