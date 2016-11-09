using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data.SqlClient;
//引用
using Maticsoft.Utility;
using Maticsoft.IDBO;
using Maticsoft.CodeHelper;

namespace Maticsoft.BuilderDALParam
{
    /// <summary>
    /// 数据访问层代码构造器（Parameter方式）
    /// </summary>
    public class BuilderDAL : Maticsoft.IBuilder.IBuilderDAL
    {

        #region 私有变量

        /// <summary>
        /// 标识列，或主键字段	
        /// </summary>
        protected string _IdentityKey = "";
        /// <summary>
        /// 标识列，或主键字段类型 
        /// </summary>
        protected string _IdentityKeyType = "int";

        #endregion

        #region 公有属性
        IDbObject dbobj;
        private string _dbname;
        private string _tablename;
        private string _modelname; //model类名
        private string _dalname;//dal类名    
        private List<ColumnInfo> _fieldlist;
        private List<ColumnInfo> _keys; // 主键或条件字段列表        
        private string _namespace; //顶级命名空间名
        private string _folder; //所在文件夹
        private string _dbhelperName;//数据库访问类名           
        private string _modelpath;
        private string _dalpath;
        private string _idalpath;
        private string _iclass;
        private string _procprefix;

        public IDbObject DbObject
        {
            set { dbobj = value; }
            get { return dbobj; }
        }
        /// <summary>
        /// 库名
        /// </summary>
        public string DbName
        {
            set { _dbname = value; }
            get { return _dbname; }
        }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName
        {
            set { _tablename = value; }
            get { return _tablename; }
        }

        /// <summary>
        /// 选择要生成的字段集合
        /// </summary>
        public List<ColumnInfo> Fieldlist
        {
            set { _fieldlist = value; }
            get { return _fieldlist; }
        }
        /// <summary>
        /// 主键或条件字段的集合
        /// </summary>
        public List<ColumnInfo> Keys
        {
            set
            {
                _keys = value;
                foreach (ColumnInfo key in _keys)
                {
                    _IdentityKey = key.ColumnName;
                    _IdentityKeyType = key.TypeName;
                    if (key.IsIdentity)
                    {
                        _IdentityKey = key.ColumnName;
                        _IdentityKeyType = CodeCommon.DbTypeToCS(key.TypeName);
                        break;
                    }
                }
            }
            get { return _keys; }
        }
        /// <summary>
        /// 顶级命名空间名
        /// </summary>
        public string NameSpace
        {
            set { _namespace = value; }
            get { return _namespace; }
        }
        /// <summary>
        /// 所在文件夹
        /// </summary>
        public string Folder
        {
            set { _folder = value; }
            get { return _folder; }
        }

        /*============================*/

        /// <summary>
        /// 实体类的命名空间
        /// </summary>
        public string Modelpath
        {
            set { _modelpath = value; }
            get { return _modelpath; }
        }
        /// <summary>
        /// 实体类名
        /// </summary>
        public string ModelName
        {
            set { _modelname = value; }
            get { return _modelname + "Model"; }
        }
        /// <summary>
        /// 实体类的整个命名空间 + 类名，即等于 Modelpath+ModelName
        /// </summary>
        public string ModelSpace
        {
            get { return Modelpath + "." + ModelName; }
        }
        /*============================*/

        /// <summary>
        /// 数据层的命名空间
        /// </summary>
        public string DALpath
        {
            set { _dalpath = value; }
            get
            {
                return _dalpath;
            }
        }
        public string DALName
        {
            set { _dalname = value; }
            get { return _dalname; }
        }
        /*============================*/


        /// <summary>
        /// 接口的命名空间
        /// </summary>
        public string IDALpath
        {
            set { _idalpath = value; }
            get
            {
                return _idalpath;
            }
        }
        /// <summary>
        /// 接口类名
        /// </summary>
        public string IClass
        {
            set { _iclass = value; }
            get { return _iclass; }
        }
        /*============================*/

        /// <summary>
        /// 数据库访问类名
        /// </summary>
        public string DbHelperName
        {
            set { _dbhelperName = value; }
            get { return _dbhelperName; }
        }
        /// <summary>
        /// 存储过程前缀 
        /// </summary>       
        public string ProcPrefix
        {
            set { _procprefix = value; }
            get { return _procprefix; }
        }
        #endregion

        #region 构造属性

        /// <summary>
        /// 所选字段的 select 列表
        /// </summary>
        public string Fieldstrlist
        {
            get
            {
                StringPlus _fields = new StringPlus();
                foreach (ColumnInfo obj in Fieldlist)
                {
                    _fields.Append(obj.ColumnName + ",");
                }
                _fields.DelLastComma();
                return _fields.Value;
            }
        }

        /// <summary>
        /// 不同数据库类的前缀
        /// </summary>
        public string DbParaHead
        {
            get
            {
                return CodeCommon.DbParaHead(dbobj.DbType);
            }

        }
        /// <summary>
        ///  不同数据库字段类型
        /// </summary>
        public string DbParaDbType
        {
            get
            {
                return CodeCommon.DbParaDbType(dbobj.DbType);
            }
        }

        /// <summary>
        /// 存储过程参数 调用符号@
        /// </summary>
        public string preParameter
        {
            get
            {
                return CodeCommon.preParameter(dbobj.DbType);
            }
        }
        /// <summary>
        /// 主键或条件字段中是否有标识列
        /// </summary>
        public bool IsHasIdentity
        {
            get
            {
                return CodeCommon.IsHasIdentity(_keys);
            }
        }

        private string KeysNullTip
        {
            get
            {
                if (_keys.Count == 0)
                {
                    return "//该表无主键信息，请自定义主键/条件字段";
                }
                else
                {
                    return "";
                }
            }
        }

        //语言文件
        public Hashtable Languagelist
        {
            get
            {
                return Maticsoft.CodeHelper.Language.LoadFromCfg("BuilderDALParam.lan");
            }
        }
        #endregion

        #region 构造函数

        public BuilderDAL()
        {
        }
        public BuilderDAL(IDbObject idbobj)
        {
            dbobj = idbobj;
        }

        public BuilderDAL(IDbObject idbobj, string dbname, string tablename, string modelname, string dalName,
            List<ColumnInfo> fieldlist, List<ColumnInfo> keys, string namepace,
            string folder, string dbherlpername, string modelpath,
            string dalpath, string idalpath, string iclass)
        {
            dbobj = idbobj;
            _dbname = dbname;
            _tablename = tablename;
            _modelname = modelname;
            _dalname = dalName;
            _namespace = namepace;
            _folder = folder;
            _dbhelperName = dbherlpername;
            _modelpath = modelpath;
            _dalpath = dalpath;
            _idalpath = idalpath;
            _iclass = iclass;
            Fieldlist = fieldlist;
            Keys = keys;
            foreach (ColumnInfo key in _keys)
            {
                _IdentityKey = key.ColumnName;
                _IdentityKeyType = key.TypeName;
                if (key.IsIdentity)
                {
                    _IdentityKey = key.ColumnName;
                    _IdentityKeyType = CodeCommon.DbTypeToCS(key.TypeName);
                    break;
                }
            }
        }

        #endregion


        #region  根据列信息 得到参数的列表

        ///// <summary>
        ///// 得到Where条件语句 - Parameter方式 (例如：用于Exists  Delete  GetModel 的where)
        ///// </summary>
        ///// <param name="keys"></param>
        ///// <returns></returns>
        //public string GetWhereExpression(List<ColumnInfo> keys, bool IdentityisPrior)
        //{
        //    StringPlus strClass = new StringPlus();
        //    ColumnInfo field = Maticsoft.CodeHelper.CodeCommon.GetIdentityKey(keys);
        //    if ((IdentityisPrior) && (field != null)) //有标识字段
        //    {
        //        strClass.Append(field.ColumnName + "=" + preParameter + field.ColumnName);
        //    }
        //    else
        //    {
        //        foreach (ColumnInfo key in keys)
        //        {
        //            if (key.IsPrimaryKey || !key.IsIdentity)// if (key.IsPK)
        //            {
        //                strClass.Append(key.ColumnName + "=" + preParameter + key.ColumnName + " and ");
        //            }
        //        }
        //        strClass.DelLastChar("and");
        //    }
        //    return strClass.Value;
        //}

        ///// <summary>
        ///// 生成sql语句中的参数列表(例如：用于 Exists  Delete  GetModel 的where参数赋值)
        ///// </summary>
        ///// <param name="keys"></param>
        ///// <returns></returns>
        //public string GetPreParameter(List<ColumnInfo> keys, bool IdentityisPrior)
        //{
        //    StringPlus strclass = new StringPlus();
        //    StringPlus strclass2 = new StringPlus();
        //    strclass.AppendSpaceLine(3, "" + DbParaHead + "Parameter[] parameters = {");

        //    ColumnInfo field = Maticsoft.CodeHelper.CodeCommon.GetIdentityKey(keys);
        //    if ((IdentityisPrior) && (field != null)) //有标识字段
        //    {
        //        strclass.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + "" + field.ColumnName + "\", " + DbParaDbType + "." + CodeCommon.DbTypeLength(dbobj.DbType, field.TypeName, "") + ")");
        //        strclass2.AppendSpaceLine(3, "parameters[0].Value = " + field.ColumnName + ";");
        //    }
        //    else
        //    {
        //        int n = 0;
        //        foreach (ColumnInfo key in keys)
        //        {
        //            if (key.IsPrimaryKey || !key.IsIdentity) //if (key.IsPK)
        //            {
        //                strclass.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + "" + key.ColumnName + "\", " + DbParaDbType + "." + CodeCommon.DbTypeLength(dbobj.DbType, key.TypeName, "") + "),");
        //                strclass2.AppendSpaceLine(3, "parameters[" + n.ToString() + "].Value = " + key.ColumnName + ";");
        //                n++;
        //            }
        //        }
        //        strclass.DelLastComma();
        //    }
        //    strclass.AppendLine("};");
        //    strclass.Append(strclass2.Value);
        //    return strclass.Value;

        //}

        #endregion

        #region 数据层(整个类)

        /// <summary>
        /// 得到整个类的代码
        /// </summary>     
        public string GetDALCode(bool Maxid, bool Exists, bool Add, bool Update, bool Delete, bool GetModel, bool List)
        {
            StringPlus strclass = new StringPlus();
            strclass.AppendLine("using System;");
            strclass.AppendLine("using System.Text;");
            strclass.AppendLine("using System.Data;");
            strclass.AppendLine("using System.Data.Linq;");
            strclass.AppendLine("using System.Collections.Generic;");
            switch (dbobj.DbType)
            {
                case "SQL2008":
                case "SQL2005":
                case "SQL2000":
                case "SQL2012":
                    strclass.AppendLine("using System.Data.SqlClient;");
                    break;
                case "Oracle":
                    strclass.AppendLine("using System.Data.OracleClient;");
                    break;
                case "MySQL":
                    strclass.AppendLine("using MySql.Data.MySqlClient;");
                    break;
                case "OleDb":
                    strclass.AppendLine("using System.Data.OleDb;");
                    break;
                case "SQLite":
                    strclass.AppendLine("using System.Data.SQLite;");
                    break;
            }
            if (IDALpath != "")
            {
                strclass.AppendLine("using " + IDALpath + ";");
            }
            //strclass.AppendLine("using Maticsoft.DBUtility;//Please add references");
            strclass.AppendLine("using " + NameSpace + ".DBUtility;");
            //strclass.AppendLine("using " + NameSpace + ".IDAL;");
            strclass.AppendLine("namespace " + DALpath);
            strclass.AppendLine("{");
            strclass.AppendSpaceLine(1, "/// <summary>");
            strclass.AppendSpaceLine(1, "/// " + Languagelist["summary"].ToString() + ":" + DALName);
            strclass.AppendSpaceLine(1, "/// </summary>");
            strclass.AppendSpace(1, "public partial class " + DALName +"DAL");
            if (IClass != "")
            {
                strclass.Append(":" + IClass);
            }
            strclass.AppendLine("");
            strclass.AppendSpaceLine(1, "{");
            strclass.AppendSpaceLine(2, "public " + DALName + "DAL()");
            strclass.AppendSpaceLine(2, "{}");
            strclass.AppendSpaceLine(2, "#region  BasicMethod");

            #region  方法代码

            if (Maxid)
            {
                strclass.AppendLine(CreatGetMaxID());
            }
            if (Exists)
            {
                strclass.AppendLine(CreatExists());
            }
            if (Add)
            {
                strclass.AppendLine(CreatAdd());
            }
            if (Update)
            {
                strclass.AppendLine(CreatUpdate());
            }
            if (Delete)
            {
                strclass.AppendLine(CreatDelete());
            }
            if (GetModel)
            {
                strclass.AppendLine(CreatGetModel());
                strclass.AppendLine(CreatDataRowToModel());
            }
            if (List)
            {
                strclass.AppendLine(CreatGetList());
                strclass.AppendLine(CreatGetListByPage());
                strclass.AppendLine(CreatGetListByPageProc());
            }
            #endregion

            strclass.AppendSpaceLine(2, "#endregion  BasicMethod");

            strclass.AppendSpaceLine(2, "#region  ExtensionMethod");
            strclass.AppendLine("");
            strclass.AppendSpaceLine(2, "#endregion  ExtensionMethod");


            strclass.AppendSpaceLine(2, "#region Linq查询");
            strclass.AppendLine(GetTableModelList());
            strclass.AppendLine(GetQueryModelList());
            strclass.AppendSpaceLine(2, "#endregion");

            strclass.AppendSpaceLine(1, "}");
            strclass.AppendLine("}");
            strclass.AppendLine("");

            return strclass.ToString();
        }

        #endregion

        #region 数据层(使用Parameter实现)

        /// <summary>
        /// 得到最大ID的方法代码
        /// </summary>
        /// <param name="TabName"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public string CreatGetMaxID()
        {
            StringPlus strclass = new StringPlus();
            if (_keys.Count > 0)
            {
                string keyname = "";
                foreach (ColumnInfo obj in _keys)
                {
                    if (CodeCommon.DbTypeToCS(obj.TypeName) == "int")
                    {
                        keyname = obj.ColumnName;
                        if (obj.IsPrimaryKey)
                        {
                            strclass.AppendLine("");
                            strclass.AppendSpaceLine(2, "/// <summary>");
                            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetMaxId"].ToString());
                            strclass.AppendSpaceLine(2, "/// </summary>");
                            strclass.AppendSpaceLine(2, "public int GetMaxId()");
                            strclass.AppendSpaceLine(2, "{");
                            strclass.AppendSpaceLine(2, "return " + DbHelperName + ".GetMaxID(\"" + keyname + "\", \"" + _tablename + "\"); ");
                            strclass.AppendSpaceLine(2, "}");
                            break;
                        }
                    }
                }
            }
            return strclass.ToString();
        }

        /// <summary>
        /// 得到Exists方法的代码
        /// </summary>
        /// <param name="_tablename"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public string CreatExists()
        {
            StringPlus strclass = new StringPlus();
            if (_keys.Count > 0)
            {
                string strInparam = Maticsoft.CodeHelper.CodeCommon.GetInParameter(Keys, false);
                if (!string.IsNullOrEmpty(strInparam))
                {
                    strclass.AppendSpaceLine(2, "/// <summary>");
                    strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryExists"].ToString());
                    strclass.AppendSpaceLine(2, "/// </summary>");
                    strclass.AppendSpaceLine(2, "public bool Exists(" + strInparam + ")");
                    strclass.AppendSpaceLine(2, "{");
                    strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
                    strclass.AppendSpaceLine(3, "strSql.Append(\"select count(1) from " + _tablename + "\");");
                    strclass.AppendSpaceLine(3, "strSql.Append(\" where " + Maticsoft.CodeHelper.CodeCommon.GetWhereParameterExpression(Keys, false, dbobj.DbType) + "\");");

                    strclass.AppendLine(CodeCommon.GetPreParameter(Keys, false, dbobj.DbType));

                    strclass.AppendSpaceLine(3, "return " + DbHelperName + ".Exists(strSql.ToString(),parameters);");
                    strclass.AppendSpaceLine(2, "}");
                }
            }
            return strclass.Value;
        }

        /// <summary>
        /// 得到Add()的代码
        /// </summary>        
        public string CreatAdd()
        {
            if (ModelSpace == "")
            {
                //ModelSpace = "ModelClassName"; ;
            }
            StringPlus strclass = new StringPlus();
            StringPlus strclass1 = new StringPlus();
            StringPlus strclass2 = new StringPlus();
            StringPlus strclass3 = new StringPlus();
            StringPlus strclass4 = new StringPlus();
            strclass.AppendLine();
            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryadd"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            string strretu = "bool";
            if ((dbobj.DbType == "SQL2000" || dbobj.DbType == "SQL2005" ||
                dbobj.DbType == "SQL2008" || dbobj.DbType == "SQL2012" || dbobj.DbType == "SQLite") && (IsHasIdentity))
            {
                strretu = "int";
                if (_IdentityKeyType != "int")
                {
                    strretu = _IdentityKeyType;
                }
            }
            
            //方法定义头
            string strFun = CodeCommon.Space(2) + "public " + strretu + " Add(" + ModelSpace + " model)";
            strclass.AppendLine(strFun);
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"insert into " + _tablename + "(\");");
            strclass1.AppendSpace(3, "strSql.Append(\"");
            int n = 0;
            foreach (ColumnInfo field in Fieldlist)
            {
                string columnName = field.ColumnName;
                string columnType = field.TypeName;
                bool IsIdentity = field.IsIdentity;
                string Length = field.Length;
                bool nullable = field.Nullable;

                if (field.IsIdentity)
                {
                    continue;
                }
                strclass3.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + columnName + "\", " + DbParaDbType + "." + CodeCommon.DbTypeLength(dbobj.DbType, columnType, Length) + "),");
                strclass1.Append(columnName + ",");
                strclass2.Append(preParameter + columnName + ",");
                if ("uniqueidentifier" == columnType.ToLower())
                {
                    strclass4.AppendSpaceLine(3, "parameters[" + n + "].Value = Guid.NewGuid();");
                }
                else
                {
                    strclass4.AppendSpaceLine(3, "parameters[" + n + "].Value = model." + columnName + ";");
                }
                n++;
            }

            //去掉最后的逗号
            strclass1.DelLastComma();
            strclass2.DelLastComma();
            strclass3.DelLastComma();
            strclass1.AppendLine(")\");");
            strclass.Append(strclass1.ToString());
            strclass.AppendSpaceLine(3, "strSql.Append(\" values (\");");
            strclass.AppendSpaceLine(3, "strSql.Append(\"" + strclass2.ToString() + ")\");");
            if ((dbobj.DbType == "SQL2000" || dbobj.DbType == "SQL2005"
                || dbobj.DbType == "SQL2008" || dbobj.DbType == "SQL2012") && (IsHasIdentity))
            {
                strclass.AppendSpaceLine(3, "strSql.Append(\";select @@IDENTITY\");");
            }
            if ((dbobj.DbType == "SQLite") && (IsHasIdentity))
            {
                strclass.AppendSpaceLine(3, "strSql.Append(\";select LAST_INSERT_ROWID()\");");
            }
            strclass.AppendSpaceLine(3, "" + DbParaHead + "Parameter[] parameters = {");
            strclass.Append(strclass3.Value);
            strclass.AppendLine("};");
            strclass.AppendLine(strclass4.Value);


            //重新定义方法头            
            if (strretu == "void")
            {
                strclass.AppendSpaceLine(3, "" + DbHelperName + ".ExecuteSql(strSql.ToString(),parameters);");
            }
            else
                if (strretu == "bool")
                {
                    strclass.AppendSpaceLine(3, "int rows=" + DbHelperName + ".ExecuteSql(strSql.ToString(),parameters);");
                    strclass.AppendSpaceLine(3, "if (rows > 0)");
                    strclass.AppendSpaceLine(3, "{");
                    strclass.AppendSpaceLine(4, "return true;");
                    strclass.AppendSpaceLine(3, "}");
                    strclass.AppendSpaceLine(3, "else");
                    strclass.AppendSpaceLine(3, "{");
                    strclass.AppendSpaceLine(4, "return false;");
                    strclass.AppendSpaceLine(3, "}");
                }
                else
                {
                    strclass.AppendSpaceLine(3, "object obj = " + DbHelperName + ".GetSingle(strSql.ToString(),parameters);");
                    strclass.AppendSpaceLine(3, "if (obj == null)");
                    strclass.AppendSpaceLine(3, "{");
                    strclass.AppendSpaceLine(4, "return 0;");
                    strclass.AppendSpaceLine(3, "}");
                    strclass.AppendSpaceLine(3, "else");
                    strclass.AppendSpaceLine(3, "{");
                    switch (strretu)
                    {
                        case "int":
                            strclass.AppendSpaceLine(4, "return Convert.ToInt32(obj);");
                            break;
                        case "long":
                            strclass.AppendSpaceLine(4, "return Convert.ToInt64(obj);");
                            break;
                        case "decimal":
                            strclass.AppendSpaceLine(4, "return Convert.ToDecimal(obj);");
                            break;
                    }

                    strclass.AppendSpaceLine(3, "}");
                }
            strclass.AppendSpace(2, "}");
            return strclass.ToString();
        }

        /// <summary>
        /// 得到Update（）的代码
        /// </summary>
        /// <param name="DbName"></param>
        /// <param name="_tablename"></param>
        /// <param name="_key"></param>
        /// <param name="ModelName"></param>
        /// <returns></returns>
        public string CreatUpdate()
        {
            if (ModelSpace == "")
            {
                //ModelSpace = "ModelClassName"; ;
            }
            StringPlus strclass = new StringPlus();
            StringPlus strclass1 = new StringPlus();
            StringPlus strclass2 = new StringPlus();
            StringPlus strclass3 = new StringPlus();

            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryUpdate"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public bool Update(" + ModelSpace + " model)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"update " + _tablename + " set \");");
            int n = 0;

            if (Fieldlist.Count == 0)
            {
                Fieldlist = Keys;
            }

            //主键的字段语句，临时保存
            List<ColumnInfo> fieldpk = new List<ColumnInfo>();

            foreach (ColumnInfo field in Fieldlist)
            {
                string columnName = field.ColumnName;
                string columnType = field.TypeName;
                string Length = field.Length;
                bool IsIdentity = field.IsIdentity;
                bool isPK = field.IsPrimaryKey;

                if (field.IsIdentity || field.IsPrimaryKey || (Keys.Contains(field)))
                {
                    fieldpk.Add(field);
                    continue;
                }
                if (columnType.ToLower() == "timestamp")
                {
                    continue;
                }
                strclass1.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + columnName + "\", " + DbParaDbType + "." + CodeCommon.DbTypeLength(dbobj.DbType, columnType, Length) + "),");
                strclass2.AppendSpaceLine(3, "parameters[" + n + "].Value = model." + columnName + ";");
                n++;

                strclass3.AppendSpaceLine(3, "strSql.Append(\"" + columnName + "=" + preParameter + columnName + ",\");");
            }
            foreach (ColumnInfo field in fieldpk)
            {
                string columnName = field.ColumnName;
                string columnType = field.TypeName;
                string Length = field.Length;
                bool IsIdentity = field.IsIdentity;
                bool isPK = field.IsPrimaryKey;

                strclass1.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + columnName + "\", " + DbParaDbType + "." + CodeCommon.DbTypeLength(dbobj.DbType, columnType, Length) + "),");
                strclass2.AppendSpaceLine(3, "parameters[" + n + "].Value = model." + columnName + ";");
                n++;
            }


            if (strclass3.Value.Length > 0)
            {
                //去掉最后的逗号			
                strclass3.DelLastComma();
                strclass3.AppendLine("\");");
            }
            else
            {
                strclass3.AppendLine("#warning 系统发现缺少更新的字段，请手工确认如此更新是否正确！ ");
                foreach (ColumnInfo field in Fieldlist)
                {
                    string columnName = field.ColumnName;
                    strclass3.AppendSpaceLine(3, "strSql.Append(\"" + columnName + "=" + preParameter + columnName + ",\");");
                }
                if (Fieldlist.Count > 0)
                {
                    strclass3.DelLastComma();
                    strclass3.AppendLine("\");");
                }

            }

            strclass.Append(strclass3.Value);
            strclass.AppendSpaceLine(3, "strSql.Append(\" where " + CodeCommon.GetWhereParameterExpression(Keys, true, dbobj.DbType) + "\");");

            strclass.AppendSpaceLine(3, "" + DbParaHead + "Parameter[] parameters = {");
            strclass1.DelLastComma();
            strclass.Append(strclass1.Value);
            strclass.AppendLine("};");
            strclass.AppendLine(strclass2.Value);
            strclass.AppendSpaceLine(3, "int rows=" + DbHelperName + ".ExecuteSql(strSql.ToString(),parameters);");

            strclass.AppendSpaceLine(3, "if (rows > 0)");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "return true;");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "return false;");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(2, "}");
            return strclass.ToString();
        }



        /// <summary>
        /// 得到Delete的代码
        /// </summary>
        /// <param name="_tablename"></param>
        /// <param name="_key"></param>
        /// <returns></returns>
        public string CreatDelete()
        {
            StringPlus strclass = new StringPlus();

            #region 标识字段优先-删除
            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryDelete"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public bool Delete(" + CodeCommon.GetInParameter(Keys, true) + ")");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, KeysNullTip);
            strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
            //strclass.AppendSpaceLine(3, "strSql.Append(\"delete from " + _tablename + " \");");
            strclass.AppendSpaceLine(3, "strSql.Append(\"update " + _tablename + " set  DelFlag = 1 \" );");
            strclass.AppendSpaceLine(3, "strSql.Append(\" where " + CodeCommon.GetWhereParameterExpression(Keys, true, dbobj.DbType) + "\");");
            strclass.AppendLine(CodeCommon.GetPreParameter(Keys, true, dbobj.DbType));
            strclass.AppendSpaceLine(3, "int rows=" + DbHelperName + ".ExecuteSql(strSql.ToString(),parameters);");
            strclass.AppendSpaceLine(3, "if (rows > 0)");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "return true;");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "return false;");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(2, "}");

            #endregion

            #region 联合主键优先的删除(既有标识字段，又有非标识主键字段)

            if ((Maticsoft.CodeHelper.CodeCommon.HasNoIdentityKey(Keys)) && (Maticsoft.CodeHelper.CodeCommon.GetIdentityKey(Keys) != null))
            {
                strclass.AppendSpaceLine(2, "/// <summary>");
                strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryDelete"].ToString());
                strclass.AppendSpaceLine(2, "/// </summary>");
                strclass.AppendSpaceLine(2, "public bool Delete(" + Maticsoft.CodeHelper.CodeCommon.GetInParameter(Keys, false) + ")");
                strclass.AppendSpaceLine(2, "{");
                strclass.AppendSpaceLine(3, KeysNullTip);
                strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
                strclass.AppendSpaceLine(3, "strSql.Append(\"delete from " + _tablename + " \");");
                strclass.AppendSpaceLine(3, "strSql.Append(\" where " + CodeCommon.GetWhereParameterExpression(Keys, false, dbobj.DbType) + "\");");
                strclass.AppendLine(CodeCommon.GetPreParameter(Keys, false, dbobj.DbType));

                strclass.AppendSpaceLine(3, "int rows=" + DbHelperName + ".ExecuteSql(strSql.ToString(),parameters);");
                strclass.AppendSpaceLine(3, "if (rows > 0)");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "return true;");
                strclass.AppendSpaceLine(3, "}");
                strclass.AppendSpaceLine(3, "else");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "return false;");
                strclass.AppendSpaceLine(3, "}");

                strclass.AppendSpaceLine(2, "}");
            }

            #endregion

            #region 批量删除方法

            string keyField = "";
            if (Keys.Count == 1)
            {
                keyField = Keys[0].ColumnName;
            }
            else
            {
                foreach (ColumnInfo field in Keys)
                {
                    if (field.IsIdentity)
                    {
                        keyField = field.ColumnName;
                        break;
                    }
                }
            }
            if (keyField.Trim().Length > 0)
            {
                strclass.AppendSpaceLine(2, "/// <summary>");
                strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryDeletelist"].ToString());
                strclass.AppendSpaceLine(2, "/// </summary>");
                strclass.AppendSpaceLine(2, "public bool DeleteList(string " + keyField + "list )");
                strclass.AppendSpaceLine(2, "{");
                strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
                strclass.AppendSpaceLine(3, "strSql.Append(\"delete from " + _tablename + " \");");
                strclass.AppendSpaceLine(3, "strSql.Append(\" where " + keyField + " in (\"+" + keyField + "list + \")  \");");
                strclass.AppendSpaceLine(3, "int rows=" + DbHelperName + ".ExecuteSql(strSql.ToString());");
                strclass.AppendSpaceLine(3, "if (rows > 0)");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "return true;");
                strclass.AppendSpaceLine(3, "}");
                strclass.AppendSpaceLine(3, "else");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "return false;");
                strclass.AppendSpaceLine(3, "}");
                strclass.AppendSpaceLine(2, "}");
            }
            #endregion


            return strclass.Value;
        }



        /// <summary>
        /// 得到GetModel()的代码
        /// </summary>
        /// <param name="DbName"></param>
        /// <param name="_tablename"></param>
        /// <param name="_key"></param>
        /// <param name="ModelName"></param>
        /// <returns></returns>
        public string CreatGetModel()
        {
            if (ModelSpace == "")
            {
                //ModelSpace = "ModelClassName"; ;
            }
            StringPlus strclass = new StringPlus();
            strclass.AppendLine();
            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetModel"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public " + ModelSpace + " GetModel(" + Maticsoft.CodeHelper.CodeCommon.GetInParameter(Keys, true) + ")");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, KeysNullTip);
            strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
            strclass.AppendSpace(3, "strSql.Append(\"select  ");
            if (dbobj.DbType == "SQL2005" || dbobj.DbType == "SQL2000" || dbobj.DbType == "SQL2008" || dbobj.DbType == "SQL2012")
            {
                strclass.Append(" top 1 ");
            }
            strclass.AppendLine(Fieldstrlist + " from " + _tablename + " \");");
            strclass.AppendSpaceLine(3, "strSql.Append(\" where " + CodeCommon.GetWhereParameterExpression(Keys, true, dbobj.DbType) + "\");");

            strclass.AppendLine(CodeCommon.GetPreParameter(Keys, true, dbobj.DbType));

            strclass.AppendSpaceLine(3, "" + ModelSpace + " model=new " + ModelSpace + "();");
            strclass.AppendSpaceLine(3, "DataSet ds=" + DbHelperName + ".Query(strSql.ToString(),parameters);");
            strclass.AppendSpaceLine(3, "if(ds.Tables[0].Rows.Count>0)");
            strclass.AppendSpaceLine(3, "{");

            #region 字段赋值
            /*
            foreach (ColumnInfo field in Fieldlist)
            {
                string columnName = field.ColumnName;
                string columnType = field.TypeName;

                strclass.AppendSpaceLine(4, "if(ds.Tables[0].Rows[0][\"" + columnName + "\"]!=null && ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString()!=\"\")");
                strclass.AppendSpaceLine(4, "{");
                #region
                switch (CodeCommon.DbTypeToCS(columnType))
                {
                    case "int":
                        {
                            //strclass.AppendSpaceLine(4, "if(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString()!=\"\")");
                            //strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=int.Parse(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString());");
                            //strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "long":
                        {
                            //strclass.AppendSpaceLine(4, "if(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString()!=\"\")");
                            //strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=long.Parse(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString());");
                            //strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "decimal":
                        {
                            //strclass.AppendSpaceLine(4, "if(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString()!=\"\")");
                            //strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=decimal.Parse(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString());");
                            //strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "float":
                        {
                            //strclass.AppendSpaceLine(4, "if(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString()!=\"\")");
                            //strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=float.Parse(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString());");
                            //strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "DateTime":
                        {
                            //strclass.AppendSpaceLine(4, "if(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString()!=\"\")");
                            //strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=DateTime.Parse(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString());");
                            //strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "string":
                        {
                            //strclass.AppendSpaceLine(4, "if(ds.Tables[0].Rows[0][\"" + columnName + "\"]!=null)");
                            //strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString();");
                            //strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "bool":
                        {
                            //strclass.AppendSpaceLine(4, "if(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString()!=\"\")");
                            //strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "if((ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString()==\"1\")||(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString().ToLower()==\"true\"))");
                            strclass.AppendSpaceLine(5, "{");
                            strclass.AppendSpaceLine(6, "model." + columnName + "=true;");
                            strclass.AppendSpaceLine(5, "}");
                            strclass.AppendSpaceLine(5, "else");
                            strclass.AppendSpaceLine(5, "{");
                            strclass.AppendSpaceLine(6, "model." + columnName + "=false;");
                            strclass.AppendSpaceLine(5, "}");
                            //strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "byte[]":
                        {
                            //strclass.AppendSpaceLine(4, "if(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString()!=\"\")");
                            //strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=(byte[])ds.Tables[0].Rows[0][\"" + columnName + "\"];");
                            //strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "uniqueidentifier":
                    case "Guid":
                        {
                            //strclass.AppendSpaceLine(4, "if(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString()!=\"\")");
                            //strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "= new Guid(ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString());");
                            //strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    default:
                        strclass.AppendSpaceLine(5, "//model." + columnName + "=ds.Tables[0].Rows[0][\"" + columnName + "\"].ToString();");
                        break;
                }
                #endregion
                strclass.AppendSpaceLine(4, "}");
            }*/
            #endregion

            strclass.AppendSpaceLine(4, "return DataRowToModel(ds.Tables[0].Rows[0]);");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "return null;");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(2, "}");


            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetModel"].ToString());
            strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public " + ModelSpace + " GetModelByWhere(string strWhere)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, KeysNullTip);
            strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
            strclass.AppendSpace(3, "strSql.Append(\"select  ");
            if (dbobj.DbType == "SQL2005" || dbobj.DbType == "SQL2000" || dbobj.DbType == "SQL2008" || dbobj.DbType == "SQL2012")
            {
                strclass.Append(" top 1 ");
            }
            strclass.AppendLine(Fieldstrlist + " from " + _tablename + " \");");
            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" where \"+strWhere);");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "" + ModelSpace + " model=new " + ModelSpace + "();");
            strclass.AppendSpaceLine(3, "DataSet ds=" + DbHelperName + ".Query(strSql.ToString());");
            strclass.AppendSpaceLine(3, "if(ds.Tables[0].Rows.Count>0)");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "return DataRowToModel(ds.Tables[0].Rows[0]);");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "return null;");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(2, "}");


            return strclass.ToString();
        }


        /// <summary>
        /// DataRowToModel的代码
        /// </summary>      
        public string CreatDataRowToModel()
        {
            if (ModelSpace == "")
            {
                //ModelSpace = "ModelClassName"; ;
            }
            StringPlus strclass = new StringPlus();
            strclass.AppendLine();
            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetModel"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public " + ModelSpace + " DataRowToModel(DataRow row)" );
            strclass.AppendSpaceLine(2, "{");          
            strclass.AppendSpaceLine(3, "" + ModelSpace + " model=new " + ModelSpace + "();");

            strclass.AppendSpaceLine(3, "if (row != null)");
            strclass.AppendSpaceLine(3, "{");

            #region 字段赋值
            foreach (ColumnInfo field in Fieldlist)
            {
                string columnName = field.ColumnName;
                string columnType = field.TypeName;

                //strclass.AppendSpaceLine(4, "if(row[\"" + columnName + "\"]!=null && row[\"" + columnName + "\"].ToString()!=\"\")");
                //strclass.AppendSpaceLine(4, "{");
                #region
                switch (CodeCommon.DbTypeToCS(columnType))
                {
                    case "int":
                        {
                            strclass.AppendSpaceLine(4, "if(row[\"" + columnName + "\"]!=null && row[\"" + columnName + "\"].ToString()!=\"\")");
                            strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=int.Parse(row[\"" + columnName + "\"].ToString());");
                            strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "long":
                        {
                            strclass.AppendSpaceLine(4, "if(row[\"" + columnName + "\"]!=null && row[\"" + columnName + "\"].ToString()!=\"\")");
                            strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=long.Parse(row[\"" + columnName + "\"].ToString());");
                            strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "decimal":
                        {
                            strclass.AppendSpaceLine(4, "if(row[\"" + columnName + "\"]!=null && row[\"" + columnName + "\"].ToString()!=\"\")");
                            strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=decimal.Parse(row[\"" + columnName + "\"].ToString());");
                            strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "float":
                        {
                            strclass.AppendSpaceLine(4, "if(row[\"" + columnName + "\"]!=null && row[\"" + columnName + "\"].ToString()!=\"\")");
                            strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=float.Parse(row[\"" + columnName + "\"].ToString());");
                            strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "DateTime":
                        {
                            strclass.AppendSpaceLine(4, "if(row[\"" + columnName + "\"]!=null && row[\"" + columnName + "\"].ToString()!=\"\")");
                            strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=DateTime.Parse(row[\"" + columnName + "\"].ToString());");
                            strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "string":
                        {
                            strclass.AppendSpaceLine(4, "if(row[\"" + columnName + "\"]!=null)");
                            strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=row[\"" + columnName + "\"].ToString();");
                            strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "bool":
                        {
                            strclass.AppendSpaceLine(4, "if(row[\"" + columnName + "\"]!=null && row[\"" + columnName + "\"].ToString()!=\"\")");
                            strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "if((row[\"" + columnName + "\"].ToString()==\"1\")||(row[\"" + columnName + "\"].ToString().ToLower()==\"true\"))");
                            strclass.AppendSpaceLine(5, "{");
                            strclass.AppendSpaceLine(6, "model." + columnName + "=true;");
                            strclass.AppendSpaceLine(5, "}");
                            strclass.AppendSpaceLine(5, "else");
                            strclass.AppendSpaceLine(5, "{");
                            strclass.AppendSpaceLine(6, "model." + columnName + "=false;");
                            strclass.AppendSpaceLine(5, "}");
                            strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "byte[]":
                        {
                            strclass.AppendSpaceLine(4, "if(row[\"" + columnName + "\"]!=null && row[\"" + columnName + "\"].ToString()!=\"\")");
                            strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "=(byte[])row[\"" + columnName + "\"];");
                            strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    case "uniqueidentifier":
                    case "Guid":
                        {
                            strclass.AppendSpaceLine(4, "if(row[\"" + columnName + "\"]!=null && row[\"" + columnName + "\"].ToString()!=\"\")");
                            strclass.AppendSpaceLine(4, "{");
                            strclass.AppendSpaceLine(5, "model." + columnName + "= new Guid(row[\"" + columnName + "\"].ToString());");
                            strclass.AppendSpaceLine(4, "}");
                        }
                        break;
                    default:
                        strclass.AppendSpaceLine(5, "//model." + columnName + "=row[\"" + columnName + "\"].ToString();");
                        break;
                }
                #endregion
                //strclass.AppendSpaceLine(4, "}");
            }
            #endregion

            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "return model;");           
            strclass.AppendSpaceLine(2, "}");
            return strclass.ToString();
        }

        /// <summary>
        /// 得到GetList()的代码
        /// </summary>
        /// <param name="_tablename"></param>
        /// <param name="_key"></param>
        /// <returns></returns>
        public string CreatGetList()
        {
            StringPlus strclass = new StringPlus();
            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetList"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
            strclass.AppendSpaceLine(2, "public DataSet GetList(string strWhere)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"select \"); ");
            strclass.AppendSpaceLine(3, "strSql.Append(\" " + Fieldstrlist + " \");");
            strclass.AppendSpaceLine(3, "strSql.Append(\" FROM " + TableName + " \");");
            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" where \"+strWhere);");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "return " + DbHelperName + ".Query(strSql.ToString());");
            strclass.AppendSpaceLine(2, "}");


            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetList"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "/// <param name=\"customerSQL\">前多少条数据</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
            strclass.AppendSpaceLine(2, "public DataSet GetList(string customerSQL, string strWhere)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
            strclass.AppendSpace(3, "strSql.Append(\"select \"); ");

            strclass.AppendSpaceLine(3, "if(!string.IsNullOrWhiteSpace(customerSQL)) ");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append( customerSQL ); ");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(3, "strSql.Append(\" " + Fieldstrlist + " \");");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "strSql.Append(\" FROM " + TableName + " \");");
            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" where \"+strWhere);");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "return " + DbHelperName + ".Query(strSql.ToString());");
            strclass.AppendSpaceLine(2, "}");


            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetList"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "/// <param name=\"customerSQL\">自定义查询字段SQL语句</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"filedOrder\">排序字段,如:ID DESC</param>");
            strclass.AppendSpaceLine(2, "public DataSet GetList(string customerSQL, string strWhere, string filedOrder)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"select \"); ");

            strclass.AppendSpaceLine(3, "if(!string.IsNullOrWhiteSpace(customerSQL)) ");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append( customerSQL ); ");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(3, "strSql.Append(\" " + Fieldstrlist + " \");");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "strSql.Append(\" FROM " + TableName + " \");");
            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" where \"+strWhere);");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(filedOrder))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" order by \"+ filedOrder);");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "return " + DbHelperName + ".Query(strSql.ToString());");
            strclass.AppendSpaceLine(2, "}");

            if ((dbobj.DbType == "SQL2000") ||
               (dbobj.DbType == "SQL2005") ||
               (dbobj.DbType == "SQL2008") ||
               (dbobj.DbType == "SQL2012"))
            {
                strclass.AppendLine();
                strclass.AppendSpaceLine(2, "/// <summary>");
                strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetList2"].ToString());
                strclass.AppendSpaceLine(2, "/// </summary>");
                strclass.AppendSpaceLine(2, "/// <param name=\"Top\">前多少条数据</param>");
                strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
                strclass.AppendSpaceLine(2, "/// <param name=\"filedOrder\">排序字段,如:ID DESC</param>");
                strclass.AppendSpaceLine(2, "public DataSet GetTopList(int Top,string strWhere,string filedOrder)");
                strclass.AppendSpaceLine(2, "{");
                strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
                strclass.AppendSpaceLine(3, "strSql.Append(\"select \");");
                strclass.AppendSpaceLine(3, "if(Top>0)");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "strSql.Append(\" top \"+Top.ToString());");
                strclass.AppendSpaceLine(3, "}");
                strclass.AppendSpaceLine(3, "strSql.Append(\" " + Fieldstrlist + " \");");
                strclass.AppendSpaceLine(3, "strSql.Append(\" FROM " + TableName + " \");");

                strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "strSql.Append(\" where \"+strWhere);");
                strclass.AppendSpaceLine(3, "}");

                strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(filedOrder))");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "strSql.Append(\" order by \"+ filedOrder);");
                strclass.AppendSpaceLine(3, "}");

                strclass.AppendSpaceLine(3, "return " + DbHelperName + ".Query(strSql.ToString());");
                strclass.AppendSpaceLine(2, "}");



                strclass.AppendSpaceLine(2, "/// <summary>");
                strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetList2"].ToString());
                strclass.AppendSpaceLine(2, "/// </summary>");
                strclass.AppendSpaceLine(2, "/// <param name=\"Top\">前多少条数据</param>");
                strclass.AppendSpaceLine(2, "/// <param name=\"customerSQL\">自定义查询字段SQL语句</param>");
                strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
                strclass.AppendSpaceLine(2, "/// <param name=\"filedOrder\">排序字段,如:ID DESC</param>");
                strclass.AppendSpaceLine(2, "public DataSet GetTopList(int Top,string customerSQL ,string strWhere,string filedOrder)");
                strclass.AppendSpaceLine(2, "{");
                strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
                strclass.AppendSpaceLine(3, "strSql.Append(\"select \");");
                strclass.AppendSpaceLine(3, "if(Top>0)");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "strSql.Append(\" top \"+Top.ToString());");
                strclass.AppendSpaceLine(3, "}");

                strclass.AppendSpaceLine(3, "if(!string.IsNullOrWhiteSpace(customerSQL)) ");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "strSql.Append( customerSQL ); ");
                strclass.AppendSpaceLine(3, "}");
                strclass.AppendSpaceLine(3, "else");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(3, "strSql.Append(\" " + Fieldstrlist + " \");");
                strclass.AppendSpaceLine(3, "}");

                strclass.AppendSpaceLine(3, "strSql.Append(\" FROM " + TableName + " \");");

                strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "strSql.Append(\" where \"+strWhere);");
                strclass.AppendSpaceLine(3, "}");

                strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(filedOrder))");
                strclass.AppendSpaceLine(3, "{");
                strclass.AppendSpaceLine(4, "strSql.Append(\" order by \"+ filedOrder);");
                strclass.AppendSpaceLine(3, "}");

                strclass.AppendSpaceLine(3, "return " + DbHelperName + ".Query(strSql.ToString());");
                strclass.AppendSpaceLine(2, "}");
            }

            return strclass.Value;
        }


        /// <summary>
        /// 得到分页方法的代码
        /// </summary>        
        public string CreatGetListByPage()
        {
            StringPlus strclass = new StringPlus();            
            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["GetRecordCount"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public int GetRecordCount(string strWhere)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");            
            strclass.AppendSpaceLine(3, "strSql.Append(\"select count(1) FROM " + TableName + " \");");
            strclass.AppendSpaceLine(3, "if(strWhere.Trim()!=\"\")");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" where \"+strWhere);");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "object obj = DbHelperSQL.GetSingle(strSql.ToString());");
            strclass.AppendSpaceLine(3, "if (obj == null)");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "return 0;");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "return Convert.ToInt32(obj);");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(2, "}");



            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetList3"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public DataSet GetListByPage(string strWhere, string orderby, int startIndex, int endIndex)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql=new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"SELECT * FROM ( \");");
            strclass.AppendSpaceLine(3, "strSql.Append(\" SELECT ROW_NUMBER() OVER (\");");
            strclass.AppendSpaceLine(3, "if (!string.IsNullOrEmpty(orderby.Trim()))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\"order by T.\" + orderby );");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\"order by T." + _IdentityKey + " desc\");");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "strSql.Append(\")AS Row, T.*  from " + TableName + " T \");");
            strclass.AppendSpaceLine(3, "if (!string.IsNullOrEmpty(strWhere.Trim()))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" WHERE \" + strWhere);");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "strSql.Append(\" ) TT\");");
            strclass.AppendSpaceLine(3, "strSql.AppendFormat(\" WHERE TT.Row between {0} and {1}\", startIndex, endIndex);");
                       
            strclass.AppendSpaceLine(3, "return " + DbHelperName + ".Query(strSql.ToString());");
            strclass.AppendSpaceLine(2, "}");
        
            
            return strclass.Value;
        }
        

        /// <summary>
        /// 得到GetList()的代码
        /// </summary>
        /// <param name="_tablename"></param>
        /// <param name="_key"></param>
        /// <returns></returns>
        public string CreatGetListByPageProc()
        {
            StringPlus strclass = new StringPlus();
            strclass.AppendSpaceLine(2, "/*");
            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// " + Languagelist["summaryGetList3"].ToString());
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public DataSet GetList(int PageSize,int PageIndex,string strWhere)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "" + DbParaHead + "Parameter[] parameters = {");
            strclass.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + "tblName\", " + DbParaDbType + ".VarChar, 255),");
            strclass.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + "fldName\", " + DbParaDbType + ".VarChar, 255),");
            strclass.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + "PageSize\", " + DbParaDbType + "." + CodeCommon.CSToProcType(dbobj.DbType, "int") + "),");
            strclass.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + "PageIndex\", " + DbParaDbType + "." + CodeCommon.CSToProcType(dbobj.DbType, "int") + "),");
            strclass.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + "IsReCount\", " + DbParaDbType + "." + CodeCommon.CSToProcType(dbobj.DbType, "bit") + "),");
            strclass.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + "OrderType\", " + DbParaDbType + "." + CodeCommon.CSToProcType(dbobj.DbType, "bit") + "),");
            strclass.AppendSpaceLine(5, "new " + DbParaHead + "Parameter(\"" + preParameter + "strWhere\", " + DbParaDbType + ".VarChar,1000),");
            strclass.AppendSpaceLine(5, "};");
            strclass.AppendSpaceLine(3, "parameters[0].Value = \"" + this.TableName + "\";");
            strclass.AppendSpaceLine(3, "parameters[1].Value = \"" + this._IdentityKey + "\";");
            strclass.AppendSpaceLine(3, "parameters[2].Value = PageSize;");
            strclass.AppendSpaceLine(3, "parameters[3].Value = PageIndex;");
            strclass.AppendSpaceLine(3, "parameters[4].Value = 0;");
            strclass.AppendSpaceLine(3, "parameters[5].Value = 0;");
            strclass.AppendSpaceLine(3, "parameters[6].Value = strWhere;	");
            strclass.AppendSpaceLine(3, "return " + DbHelperName + ".RunProcedure(\"UP_GetRecordByPage\",parameters,\"ds\");");
            strclass.AppendSpaceLine(2, "}*/");
            return strclass.Value;
        }

        #endregion


        /// <summary>
        /// 获得linq 表查询
        /// </summary>
        /// <returns></returns>
        public string GetTableModelList()
        {
            StringPlus strclass = new StringPlus();
            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "///查询表对象的集合");
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public Table<" + ModelSpace + "> GetTableModelList()");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "string connectionString = PubConstant.GetConnectionString(\"ConnectionString\");");
            strclass.AppendSpaceLine(3, "DataContext db = new DataContext(connectionString);");;
            strclass.AppendSpaceLine(3, "var thisdata = db.ExecuteQuery<" + ModelSpace + ">(\"select * FROM " + _tablename + "\");");
            strclass.AppendSpaceLine(3, "Table<" + ModelSpace + "> modellist = db.GetTable<" + ModelSpace + ">();");
            strclass.AppendSpaceLine(3, "return modellist;");
            strclass.AppendSpaceLine(2, "}");
            return strclass.Value;
        }


        public string GetQueryModelList()
        {
            StringPlus strclass = new StringPlus();
            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// 直接执行 SQL 查询 (LINQ to SQL)");
            strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public IEnumerable<" + ModelSpace + "> GetQueryModelList(string strWhere)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql = new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"select * FROM  " + _tablename + "\");");
            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(3, "strSql.Append(\" where \" + strWhere);");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "string connectionString = PubConstant.GetConnectionString(\"ConnectionString\");");
            strclass.AppendSpaceLine(3, "DataContext db = new DataContext(connectionString);");
            strclass.AppendSpaceLine(3, "return db.ExecuteQuery<" + ModelSpace + ">(strSql.ToString());");
            strclass.AppendSpaceLine(2, "}");


            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// 直接执行自定义 SQL 查询 (LINQ to SQL)");
            strclass.AppendSpaceLine(2, "/// <param name=\"customerSQL\">自定义查询字段SQL语句</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public IEnumerable<" + ModelSpace + "> GetQueryModelList(string customerSQL, string strWhere)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql = new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"select \");");


            strclass.AppendSpaceLine(3, "if(!string.IsNullOrWhiteSpace(customerSQL)) ");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append( customerSQL ); ");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" * \" );");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "strSql.Append(\" FROM  " + _tablename + "\");");

            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(3, "strSql.Append(\" where \" + strWhere);");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "string connectionString = PubConstant.GetConnectionString(\"ConnectionString\");");
            strclass.AppendSpaceLine(3, "DataContext db = new DataContext(connectionString);");
            strclass.AppendSpaceLine(3, "return db.ExecuteQuery<" + ModelSpace + ">(strSql.ToString());");
            strclass.AppendSpaceLine(2, "}");



            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// 直接执行自定义 SQL 查询 (LINQ to SQL)");
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "/// <param name=\"customerSQL\">自定义查询字段SQL语句</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"filedOrder\">排序字段,如:ID DESC</param>");
            strclass.AppendSpaceLine(2, "public IEnumerable<" + ModelSpace + "> GetQueryModelList(string customerSQL, string strWhere,string filedOrder)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql = new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"select \");");


            strclass.AppendSpaceLine(3, "if(!string.IsNullOrWhiteSpace(customerSQL)) ");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append( customerSQL ); ");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" * \" );");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "strSql.Append(\" FROM  " + _tablename + "\");");

            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(3, "strSql.Append(\" where \" + strWhere);");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(filedOrder))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" order by \" + filedOrder);");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "string connectionString = PubConstant.GetConnectionString(\"ConnectionString\");");
            strclass.AppendSpaceLine(3, "DataContext db = new DataContext(connectionString);");
            strclass.AppendSpaceLine(3, "return db.ExecuteQuery<" + ModelSpace + ">(strSql.ToString());");
            strclass.AppendSpaceLine(2, "}");

            
            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// 直接执行 SQL 查询 (LINQ to SQL) 前几条数据");
            strclass.AppendSpaceLine(2, "/// <param name=\"Top\">前多少条数据</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "public IEnumerable<" + ModelSpace + "> GetQueryModelTopList(int Top, string strWhere)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql = new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"select \");");
            strclass.AppendSpaceLine(3, "if(Top>0) ");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(3, "   strSql.Append(\" top \"+Top.ToString()); ");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "strSql.Append(\" * FROM  " + _tablename + "\");");
            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(3, "strSql.Append(\" where \" + strWhere);");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "string connectionString = PubConstant.GetConnectionString(\"ConnectionString\");");
            strclass.AppendSpaceLine(3, "DataContext db = new DataContext(connectionString);");
            strclass.AppendSpaceLine(3, "return db.ExecuteQuery<" + ModelSpace + ">(strSql.ToString());");
            strclass.AppendSpaceLine(2, "}");


            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// 直接执行 SQL 查询 (LINQ to SQL) 前几条数据");
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "/// <param name=\"Top\">前多少条数据</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"filedOrder\">排序字段,如:ID DESC</param>");
            strclass.AppendSpaceLine(2, "public IEnumerable<" + ModelSpace + "> GetQueryModelTopList(int Top, string strWhere,string filedOrder)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql = new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"select \");");
            strclass.AppendSpaceLine(3, "if(Top>0) ");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(3, "   strSql.Append(\" top \"+Top.ToString()); ");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "strSql.Append(\" * FROM  " + _tablename + "\");");
            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(3, "strSql.Append(\" where \" + strWhere);");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(filedOrder))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" order by \" + filedOrder);");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "string connectionString = PubConstant.GetConnectionString(\"ConnectionString\");");
            strclass.AppendSpaceLine(3, "DataContext db = new DataContext(connectionString);");
            strclass.AppendSpaceLine(3, "return db.ExecuteQuery<" + ModelSpace + ">(strSql.ToString());");
            strclass.AppendSpaceLine(2, "}");

            strclass.AppendSpaceLine(2, "/// <summary>");
            strclass.AppendSpaceLine(2, "/// 直接执行自定义 SQL 查询 (LINQ to SQL) 前几条数据");
            strclass.AppendSpaceLine(2, "/// </summary>");
            strclass.AppendSpaceLine(2, "/// <param name=\"Top\">前多少条数据</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"customerSQL\">自定义查询字段SQL语句</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"strWhere\">条件查询语句</param>");
            strclass.AppendSpaceLine(2, "/// <param name=\"filedOrder\">排序字段,如:ID DESC</param>");
            strclass.AppendSpaceLine(2, "public IEnumerable<" + ModelSpace + "> GetQueryModelTopList(int Top, string customerSQL, string strWhere, string filedOrder)");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(3, "StringBuilder strSql = new StringBuilder();");
            strclass.AppendSpaceLine(3, "strSql.Append(\"select \");");
            strclass.AppendSpaceLine(3, "if(Top>0) ");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" top \"+Top.ToString()); ");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "if(!string.IsNullOrWhiteSpace(customerSQL)) ");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append( customerSQL ); ");
            strclass.AppendSpaceLine(3, "}");
            strclass.AppendSpaceLine(3, "else");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" * \" );");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "strSql.Append(\"  FROM  " + _tablename + "\");");
            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(strWhere))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" where \" + strWhere);");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "if (!string.IsNullOrWhiteSpace(filedOrder))");
            strclass.AppendSpaceLine(3, "{");
            strclass.AppendSpaceLine(4, "strSql.Append(\" order by \" + filedOrder);");
            strclass.AppendSpaceLine(3, "}");

            strclass.AppendSpaceLine(3, "string connectionString = PubConstant.GetConnectionString(\"ConnectionString\");");
            strclass.AppendSpaceLine(3, "DataContext db = new DataContext(connectionString);");
            strclass.AppendSpaceLine(3, "return db.ExecuteQuery<" + ModelSpace + ">(strSql.ToString());");
            strclass.AppendSpaceLine(2, "}");

            return strclass.Value;
        }
    }
}
