using System;
using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SqlLib
{
    public class DapperHelper
    {
        ///ProviderName="定義資料庫類型"
        ///max pool 依照需求大小建立
        // <add name="DefaultConn" connectionString="Data Source=MSI;Initial Catalog=HAWOOO;User ID=Account;Password=Password;min pool size=5;max pool size=512;connect timeout=20;" providerName="System.Data.SqlClient" />

        //<configuration>
        //  <connectionStrings>
        //    <add name="default"
        //         connectionString="server=localhost; user id=root; password=******; database=northwind"
        //         providerName="MySql.Data.MySqlClient"/> 
        //  </connectionStrings>
        //</configuration>
        private readonly string _connStr;
        private readonly ConnectionStringSettings _settings = ConfigurationManager.ConnectionStrings["DefaultConn"];
        //private readonly string DefaultConnStr = Settings.ConnectionString;
        private DbProviderFactory GetDbType()
        {
            return DbProviderFactories.GetFactory(_settings.ProviderName);
        }

        private IDbConnection CreateDapperConnection()
        {
            IDbConnection conn = GetDbType().CreateConnection();
            if (_connStr == null)
                conn.ConnectionString = _settings.ConnectionString;
            else
                conn.ConnectionString = _connStr;

            return conn;
        }

        public DapperHelper()
        {
        }

        public DapperHelper(string conStr)
        {
            _connStr = conStr;
        }
        /// <summary>
        /// 得到一組資訊
        /// </summary>
        /// <typeparam name="T">用於初始化或者轉化的對像</typeparam>
        /// <param name="sqlTxt">查詢SQL語句</param>
        /// <param name="cmdParms">包含條件或參數值的對像</param>
        /// <returns>返回 T 的集合</returns>
        public IEnumerable<T> Query<T>(string sqlTxt, object cmdParms = null)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                conn.Open();
                return conn.Query<T>(sqlTxt, cmdParms);
            }
        }
        /// <summary>
        /// 得到一組資訊
        /// </summary>
        /// <typeparam name="T">用於初始化或者轉化的對像</typeparam>
        /// <param name="sqlTxt">查詢SQL語句</param>
        /// <param name="cmdParms">包含條件或參數值的對像</param>
        /// <returns>返回 T 的集合</returns>
        public IEnumerable<dynamic> Query(string sqlTxt, object cmdParms = null)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                conn.Open();
                IEnumerable<dynamic> list = conn.Query(sqlTxt, cmdParms);
                return list;
            }
        }
        /// <summary>
        /// 得到一個對像
        /// <para>此方法只能返回一行資料時使用。</para>
        /// </summary>
        /// <typeparam name="T">用於初始化或者轉化的對像</typeparam>
        /// <param name="sqlTxt">查詢SQL語句</param>
        /// <param name="cmdParms">包含條件或參數值的對像</param>
        /// <returns>返回一個對像</returns>
        public T GetModel<T>(string sqlTxt, object cmdParms = null)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                conn.Open();
                return conn.Query<T>(sqlTxt, cmdParms).SingleOrDefault<T>();
            }
        }

        /// <summary>
        /// 執行SQL 並返回影響的行數
        /// </summary>
        /// <param name="sqlTxt">用於執行的SQL</param>
        /// <param name="cmdParms">包含條件或參數值的對像</param>
        /// <returns>影響的行數</returns>
        public int Execute(string sqlTxt, object cmdParms = null)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                conn.Open();
                int row = conn.Execute(sqlTxt, cmdParms);
                return row;
            }
        }
        /// <summary>
        /// 執行SQL 並返回第一行第一列的值
        /// </summary>
        /// <param name="sqlTxt">用於執行的SQL</param>
        /// <param name="cmdParms">包含條件或參數值的對像</param>
        /// <returns>第一行第一列的值</returns>
        public object ExecuteScalar(string sqlTxt, object cmdParms = null)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                conn.Open();
                object firstSelected = conn.ExecuteScalar(sqlTxt, cmdParms);
                return firstSelected;
            }
        }
        /// <summary>
        /// 執行SQL 並返回一個對像
        /// </summary>
        /// <typeparam name="T">用於初始化或者轉化的對像</typeparam>
        /// <param name="sqlTxt">用於執行的SQL</param>
        /// <param name="cmdParms">包含條件或參數值的對像</param>
        /// <returns>一個對像</returns>
        public T ExecuteScalar<T>(string sqlTxt, object cmdParms = null)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                conn.Open();
                T firstSelected = conn.ExecuteScalar<T>(sqlTxt, cmdParms);
                return firstSelected;
            }
        }
        /// <summary>
        /// 運行事務，執行SQL 並返回影響的行數
        /// </summary>
        /// <param name="sqlTxt">用於執行的SQL</param>
        /// <param name="cmdParms">包含條件或參數值的對像</param>
        /// <returns>影響的行數</returns>
        public int ExecuteTransaction(string sqlTxt, object cmdParms = null)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                conn.Open();
                IDbTransaction transaction = conn.BeginTransaction();
                int row = conn.Execute(sqlTxt, cmdParms, transaction, null, null);
                transaction.Commit();
                return row;
            }
        }
        /// <summary>
        /// 運行事務，執行多條SQL 並返回每條影響的行數
        /// </summary>
        /// <param name="execteParameter">多條運行的變數</param>
        /// <returns>按入口參數順序返回每條SQL的影響行數</returns>
        public int[] ExecuteTransaction(List<ExecuteParameter> execteParameter)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                conn.Open();
                IDbTransaction transaction = conn.BeginTransaction();
                List<int> rows = new List<int>();

                foreach (ExecuteParameter item in execteParameter)
                {
                    rows.Add(conn.Execute(item.SqlTxt, item.CmdObj, transaction, null, null));
                }
                transaction.Commit();
                return rows.ToArray();
            }
        }

        /// <summary>
        /// 查詢多資料集，返回一個針對資料集的Reader
        /// </summary>
        /// <param name="sqlTxt"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public SqlMapper.GridReader QueryMultiple(string sqlTxt, object cmdParms = null)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                conn.Open();
                SqlMapper.GridReader gridReader = conn.QueryMultiple(sqlTxt, cmdParms);
                return gridReader;
            }
        }

        public IQueryable<TResult> QueryMultiple<T1, T2, TResult>(string sqlTxt, Func<T1, T2, TResult> fnMap)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                var qObj = conn.Query<T1, T2, TResult>(sqlTxt, fnMap).AsQueryable();
                return qObj;
            }
        }

        public IQueryable<TResult> QueryMultiple<T1, T2, T3, TResult>(string sqlTxt, Func<T1, T2, T3, TResult> fnMap)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                var qObj = conn.Query<T1, T2, T3, TResult>(sqlTxt, fnMap).AsQueryable();
                return qObj;
            }
        }

        public IQueryable<TResult> QueryMultiple<T1, T2, T3, T4, TResult>(string sqlTxt, Func<T1, T2, T3, T4, TResult> fnMap)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                var qObj = conn.Query<T1, T2, T3, T4, TResult>(sqlTxt, fnMap).AsQueryable();
                return qObj;
            }
        }
        /// <summary>
        /// 執行存儲過程
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int ExecuteProc(string procName, DynamicParameters param)
        {
            using (IDbConnection conn = CreateDapperConnection())
            {
                conn.Open();
                return SqlMapper.Execute(conn, procName, param, null, null, CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// sql執行的變數
        /// </summary>
        public class ExecuteParameter
        {
            public ExecuteParameter()
            {
                CmdObj = null;
            }
            /// <summary>
            /// SQL語句
            /// </summary>
            public string SqlTxt { get; set; }
            /// <summary>
            /// 包含條件或參數值的對像
            /// </summary>
            public object CmdObj { get; set; }
        }
    }
}