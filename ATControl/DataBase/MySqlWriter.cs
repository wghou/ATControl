using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataBase
{
    /// <summary>
    /// 仅仅是为了在泛函 InsertValue<T>(T data) 中添加限定
    /// </summary>
    public abstract class mysqlData
    {
        /// <summary>
        /// 检查数据格式等是否正确
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckData() { return true; }
    }

    /// <summary>
    /// 数据库写入类
    /// </summary>
    public class MySqlWriter
    {
        private static readonly Logger nlogger = LogManager.GetCurrentClassLogger();

        private static SqlSugarClient _sqlDB;

        /// <summary>
        /// MySql 的配置文件，主要是 connecting string
        /// </summary>
        private class SqlConfig
        {
            public string server = "localhost";
            public string user = "root";
            public string pwd = "123123";
            public string database = "autotest";

            public string ConnectionString {
                get { return "server=" + server + "; user=" + user + "; pwd=" + pwd + "; database=" + database; } }
        }
        static SqlConfig _sqlConfig = new SqlConfig();

        private class RecordDefaultValue
        {
            public string name = "wghou";
        }

        public bool Init()
        {
            if (_sqlDB != null) return true;

            // json config file
            string confFile = "mysqlConfig.json";
            try
            {
                System.IO.StreamReader file = System.IO.File.OpenText(confFile);
                JsonTextReader reader = new JsonTextReader(file);
                JObject obj = (JObject)JToken.ReadFrom(reader);

                // 设置控温表
                if (obj.ContainsKey("config"))
                {
                    JObject child = (JObject)obj["config"];
                    if (child.ContainsKey("server")) _sqlConfig.server = (string)child["server"];
                    if (child.ContainsKey("user")) _sqlConfig.user = (string)child["user"];
                    if (child.ContainsKey("pwd")) _sqlConfig.pwd = (string)child["pwd"];
                    if (child.ContainsKey("database")) _sqlConfig.database = (string)child["database"];
                }

                if (obj.ContainsKey("RecordDefaultValue"))
                {
                    JObject child = (JObject)obj["RecordDefaultValue"];
                }
            }
            catch(Exception ex) {
                nlogger.Error("failed to load mysqlConfig.json");

                return false;
            }

            try
            {
                _sqlDB = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = _sqlConfig.ConnectionString,
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute,
                });

                _sqlDB.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Console.WriteLine(sql + "\r\n" +
                        _sqlDB.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                    Console.WriteLine();
                };

                //_sqlDB.CodeFirst.InitTables(typeof(StudentMoel));

                nlogger.Trace("Init the database successfully.");
            }
            catch(Exception ex)
            {
                nlogger.Error("Init the database failed with: " + ex.Message);
                return false;
            }

            return true;
        }


        /// <summary>
        /// 向数据库插入记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool InsertValue<T>(T data) where T: mysqlData, new()
        {
            // 初始化 sql db
            if(_sqlDB == null)
            {
                if (Init() == false) return false;
            }

            // todo: 如果有重复，怎么办？
            try
            {
                _sqlDB.Insertable(data).ExecuteCommand();

                nlogger.Trace("Insert value to the data base successfully.");
            }
            catch(Exception ex)
            {
                nlogger.Error("Insert value to the data base failed.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 向数据库插入记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool InsertValue<T>(List<T> datas) where T : mysqlData, new()
        {
            // 初始化 sql db
            if (_sqlDB == null)
            {
                if (Init() == false) return false;
            }

            nlogger.Warn("step 4 : Insert value to the data base. data len = " + datas.Count.ToString());

            // todo: 如果有重复，怎么办？
            try
            {
                _sqlDB.Insertable(datas).ExecuteCommand();

                nlogger.Trace("Insert value to the data base successfully.");
            }
            catch (Exception ex)
            {
                nlogger.Error("Insert value to the data base failed.");
                return false;
            }
            return true;
        }


        /// <summary>
        /// 按主键查询
        /// </summary>
        /// <typeparam name="T">查询类型-对应表</typeparam>
        /// <param name="pkValues">主键的值</param>
        /// <returns> 查询结果-List<T> </returns>
        public ISugarQueryable<T> QueryValue<T>() where T: mysqlData
        {
            // 初始化 sql db
            if (_sqlDB == null)
            {
                if (Init() == false) return null;
            }

            return _sqlDB.Queryable<T>();
        }
    }
}
