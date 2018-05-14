using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class ExecSQLController : ApiController
    {
        private DataSet ReturnDataSet_Private(Query sSQL)
        {
            DataSet dataSet = new DataSet();
            try
            {
                using (SqlConnection c = new SqlConnection(ApplicationDbContext.CONNECT_STRING))
                {
                    SqlCommand cmd = new SqlCommand(sSQL.query, c);
                    c.Open();
                    cmd.CommandTimeout = 0;
                    using (SqlDataAdapter a = new SqlDataAdapter(cmd))
                    {
                        a.Fill(dataSet);
                    }
                    c.Close();
                }
            }
            catch (Exception)
            {
                return null;
            }
            return dataSet;
        }

        private bool ExecSQL_Private(Query sSQL)
        {
            if (sSQL.query == null) return false;
            if (sSQL.query.Trim() == "") return false;
            SqlTransaction trans = null; ;
          
                using (SqlConnection c = new SqlConnection(ApplicationDbContext.CONNECT_STRING))
                {
                    SqlCommand cmd = new SqlCommand(sSQL.query, c);
                    c.Open();
                    cmd.CommandTimeout = 0;
                    trans = c.BeginTransaction();
                    cmd.Transaction = trans;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        trans.Commit();
                        c.Close();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            return true;
        }


        [HttpPost]
        [Route("api/SqlServer/ReturnDataTable")]
        public string ReturnDataTable([FromBody]Query id)
        {
            DataSet dataSet = ReturnDataSet_Private(id);
            if (dataSet == null) return "";
            int iCount =  dataSet.Tables.Count;
            if (iCount <1) return "";
            string sRet = "";
            sRet = JsonConvert.SerializeObject(dataSet.Tables[0], Formatting.Indented);
            return sRet;
        }

        [HttpPost]
        [Route("api/SqlServer/ReturnDataSet")]
        public string[] ReturnDataSet([FromBody]Query id)
        {
            DataSet dataSet = ReturnDataSet_Private(id);
            if (dataSet == null) return null;
            int iCount = dataSet.Tables.Count;
            if (iCount < 1) return null;
            string[] sRet = new string[iCount];
            for (int i = 0; i < iCount; i++)
            {
                sRet[i] = JsonConvert.SerializeObject(dataSet.Tables[i], Formatting.Indented);
            }
            return sRet;
        }

        [HttpPost]
        [Route("api/SqlServer/ExecSQL")]
        public bool Post([FromBody]Query sSQL)
        {
            return ExecSQL_Private(sSQL);
        }
    }

   public class Query
    {
        public string query { get; set; }
        //Các tham số dùng cho Parametters.
        public string[] fieldName { get; set; }
        public object[] value { get; set; }
    }
}
