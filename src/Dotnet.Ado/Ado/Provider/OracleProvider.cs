﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using Dotnet.Ado.Common;

namespace Dotnet.Ado.Provider
{

    /// <summary>
    /// Oracle
    /// </summary>
    public class OracleProvider : DbProvider
    {

        public OracleProvider(string connectionString)
            : base(connectionString, OracleClientFactory.Instance, '"', '"', ':')
        {
        }

        public override string RowAutoID
        {
            get { return "select {0}.currval from dual"; }
        }

        public override bool SupportBatch
        {
            get { return true; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public override void PrepareCommand(DbCommand cmd)
        {
            base.PrepareCommand(cmd);

            foreach (DbParameter p in cmd.Parameters)
            {

                if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue)
                {
                    continue;
                }

                object value = p.Value;
                if (value == DBNull.Value)
                {
                    continue;
                }
                Type type = value.GetType();
                OracleParameter oracleParam = (OracleParameter)p;

                if (oracleParam.DbType != DbType.Guid && type == typeof(Guid))
                {
                    oracleParam.OracleType = OracleType.Char;
                    oracleParam.Size = 36;
                    continue;
                }

                if ((p.DbType == DbType.Time || p.DbType == DbType.DateTime) && type == typeof(TimeSpan))
                {
                    oracleParam.OracleType = OracleType.Double;
                    oracleParam.Value = ((TimeSpan)value).TotalDays;
                    continue;
                }

                switch (p.DbType)
                {
                    case DbType.Binary:
                        if (((byte[])value).Length > 2000)
                        {
                            oracleParam.OracleType = OracleType.Blob;
                        }
                        break;
                    case DbType.Time:
                        oracleParam.OracleType = OracleType.DateTime;
                        break;
                    case DbType.DateTime:
                        oracleParam.OracleType = OracleType.DateTime;
                        break;
                    case DbType.AnsiString:
                        if (value.ToString().Length > 4000)
                        {
                            oracleParam.OracleType = OracleType.Clob;
                        }
                        break;
                    case DbType.String:
                        if (value.ToString().Length > 2000)
                        {
                            oracleParam.OracleType = OracleType.NClob;
                        }
                        break;
                    case DbType.Object:
                        oracleParam.OracleType = OracleType.NClob;
                        p.Value = SerializationManager.Serialize(value);
                        break;
                    case DbType.Guid:
                        oracleParam.OracleType = OracleType.Char;//AnsiStringFixedLength  
                        p.Value = SerializationManager.Serialize(value);
                        break;
                }
            }




            //replace oracle specific function names in cmd.CommandText
            cmd.CommandText = cmd.CommandText
                .Replace("N'", "'")
                .Replace("len(", "length(")
                .Replace("substring(", "substr(")
                .Replace("getdate()", "to_char(current_date,'dd-mon-yyyy hh:mi:ss')")
                .Replace("isnull(", "nvl(");

            int startIndexOfCharIndex = cmd.CommandText.IndexOf("charindex(");
            while (startIndexOfCharIndex > 0)
            {
                int endIndexOfCharIndex = DataUtils.GetEndIndexOfMethod(cmd.CommandText, startIndexOfCharIndex + "charindex(".Length);
                string[] itemsInCharIndex = DataUtils.SplitTwoParamsOfMethodBody(
                    cmd.CommandText.Substring(startIndexOfCharIndex + "charindex(".Length,
                    endIndexOfCharIndex - startIndexOfCharIndex - "charindex(".Length));
                cmd.CommandText = cmd.CommandText.Substring(0, startIndexOfCharIndex)
                    + "instr(" + itemsInCharIndex[1] + "," + itemsInCharIndex[0] + ")"
                    + (cmd.CommandText.Length - 1 > endIndexOfCharIndex ?
                    cmd.CommandText.Substring(endIndexOfCharIndex + 1) : string.Empty);

                startIndexOfCharIndex = cmd.CommandText.IndexOf("charindex(", endIndexOfCharIndex);
            }

            //replace DATEPART with TO_CHAR(CURRENT_DATE,'XXXX')
            startIndexOfCharIndex = cmd.CommandText.IndexOf("datepart(");
            if (startIndexOfCharIndex > 0)
            {
                cmd.CommandText = cmd.CommandText
                    .Replace("datepart(year", "to_char('yyyy'")
                    .Replace("datepart(month", "to_char('mm'")
                    .Replace("datepart(day", "to_char('dd'");

                startIndexOfCharIndex = cmd.CommandText.IndexOf("to_char(");
                while (startIndexOfCharIndex > 0)
                {
                    int endIndexOfCharIndex = DataUtils.GetEndIndexOfMethod(cmd.CommandText, startIndexOfCharIndex + "to_char(".Length);
                    string[] itemsInCharIndex = DataUtils.SplitTwoParamsOfMethodBody(
                        cmd.CommandText.Substring(startIndexOfCharIndex + "to_char(".Length,
                        endIndexOfCharIndex - startIndexOfCharIndex - "to_char(".Length));
                    cmd.CommandText = cmd.CommandText.Substring(0, startIndexOfCharIndex)
                        + "to_char(" + itemsInCharIndex[1] + "," + itemsInCharIndex[0] + ")"
                        + (cmd.CommandText.Length - 1 > endIndexOfCharIndex ?
                        cmd.CommandText.Substring(endIndexOfCharIndex + 1) : string.Empty);

                    startIndexOfCharIndex = cmd.CommandText.IndexOf("to_char(", endIndexOfCharIndex);
                }
            }
        }


        /// <summary>
        /// 得到数据库SQL参数对象
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>返回SQL参数对象</returns>
        public override DbParameter GetDbParameter(string parameterName, object value)
        {
            if (value.GetType() == typeof(bool) || value.GetType() == typeof(bool?))
            {
                value = Convert.ToInt32(value);
            }
            return new OracleParameter(paramPrefixToken + parameterName, value);
        }

        /// <summary>
        /// 得到数据库SQL参数对象
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="value">参数值</param>
        /// <param name="dbType">数据类型</param>
        /// <returns>返回SQL参数对象</returns>
        public override DbParameter GetDbParameter(string parameterName, object value, DbType dbType)
        {
            OracleParameter parameter = new OracleParameter(paramPrefixToken + parameterName, value);
            parameter.DbType = dbType;
            return parameter;
        }


    }
}
