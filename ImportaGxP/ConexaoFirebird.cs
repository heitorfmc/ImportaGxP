using System.Data;
using System.IO;
using FirebirdSql.Data.FirebirdClient;
using Windows.System.Diagnostics.DevicePortal;
using System.Collections.Generic;
using FirebirdSql.Data.Isql;


namespace ImportaGxP
{
    public class ConexaoFirebird
    {
        public ConexaoFirebird(string caminho)
        {
            ConnectionString = $"Server=localhost;User=SYSDBA;Password=masterkey;Database={caminho}";
            connection = new FbConnection(ConnectionString);
            connection.Open();
            status = connection.State.ToString();
        }

        public void Conectar()
        {
            connection = new FbConnection(ConnectionString);
            connection.Open();
        }

        public DataTable ListarTabela(string nomeTabela)
        {
            dt = new DataTable();
            FbDataAdapter da = new FbDataAdapter($"select * from {nomeTabela}", connection);
            da.Fill(dt);
            return dt;
        }

        public string[] RetornaTabelas()
        {
            dt = new DataTable();
            FbDataAdapter da = new("SELECT a.RDB$RELATION_NAME FROM RDB$RELATIONS a", connection);
            da.Fill(dt);

            string[] ret = new string[dt.Rows.Count];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = dt.Rows[i][0].ToString();
            }
            return ret;
        }

        public DataTable ExecutarSelect(string comando)
        {
            dt = new DataTable();
            FbDataAdapter da = new FbDataAdapter(comando, connection);
            da.Fill(dt);
            return dt;
        }

        public string ExecutarComando(string comando)
        {
            using (FbConnection conn = new FbConnection(ConnectionString))
            {
                try
                {
                    FbCommand command = new FbCommand(comando, conn);
                    command.Connection.Open();
                    return command.ExecuteNonQuery().ToString();
                }
                catch (FbException fbex)
                {
                    return fbex.Message;
                }
            }
        }

        public string ExecutarQuery(string query)
        {
            using (FbConnection conn = new FbConnection(ConnectionString))
            {
                try
                {
                    FbScript script = new FbScript(query);
                    script.Parse();
                    FbBatchExecution fbe = new FbBatchExecution(connection);
                    fbe.AppendSqlStatements(script);
                    fbe.Execute();
                    return "1";
                }
                catch (FbException fbex)
                {
                    return fbex.Message;
                }
            }
        }

        public FbConnection connection;
        public DataTable dt;
        public string status;
        public string ConnectionString;
    }
}