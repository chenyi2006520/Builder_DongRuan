using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Maticsoft.Utility;
using Maticsoft.CodeHelper;
namespace Maticsoft.BuilderModel
{
    /// <summary>
    /// Model代码生成组件
    /// </summary>
    public class BuilderModel : IBuilder.IBuilderModel
    {        
        #region 公有属性
        protected string _modelname=""; //model类名
        protected string _namespace = "ETours"; //顶级命名空间名
        protected string _modelpath="";//实体类的命名空间
        protected string _tabledescription="";
        protected List<ColumnInfo> _fieldlist;
        
        /// <summary>
        /// 顶级命名空间名 
        /// </summary>        
        public string NameSpace
        {
            set { _namespace = value; }
            get { return _namespace; }
        }
        /// <summary>
        /// 实体类的命名空间
        /// </summary>
        public string Modelpath
        {
            set { _modelpath = value; }
            get { return _modelpath; }
        }
        /// <summary>
        /// model类名
        /// </summary>
        public string ModelName
        {
            set { _modelname = value; }
            get { return _modelname; }
        }
        /// <summary>
        /// 表的描述信息
        /// </summary>
        public string TableDescription
        {
            set { _tabledescription = value; }
            get { return _tabledescription; }
        }
        /// <summary>
        /// 选择的字段集合
        /// </summary>
        public List<ColumnInfo> Fieldlist
        {
            set { _fieldlist = value; }
            get { return _fieldlist; }
        }
        //语言文件
        public Hashtable Languagelist
        {
            get
            {
                return Maticsoft.CodeHelper.Language.LoadFromCfg("BuilderModel.lan");
            }
        }
        #endregion
                
       

        public BuilderModel()
        {            
        }        

        #region 生成完整Model类
        /// <summary>
        /// 生成完整Model类
        /// </summary>		
        public string CreatModel()
        {           
            StringPlus strclass = new StringPlus();
            strclass.AppendLine("using System;");
            strclass.AppendLine("using System.ComponentModel;");
            strclass.AppendLine("using System.ComponentModel.DataAnnotations;");
            strclass.AppendLine("using System.Data.Linq.Mapping;");
            strclass.AppendLine("using System.Collections;");
            strclass.AppendLine("using System.Collections.Generic;");
            strclass.AppendLine("namespace " + Modelpath);
            strclass.AppendLine("{");
            strclass.AppendSpaceLine(1, "/// <summary>");


            if (TableDescription.Length > 0)
            {
                strclass.AppendSpaceLine(1, "/// " + TableDescription.Replace("\r\n", "\r\n\t///"));
            }
            else
            {
                strclass.AppendSpaceLine(1, "/// " + _modelname + ":" + Languagelist["summary"].ToString());
            }            
            strclass.AppendSpaceLine(1, "/// </summary>");
            strclass.AppendSpaceLine(1, "[Serializable]");
            strclass.AppendSpaceLine(1, "[global::System.Data.Linq.Mapping.TableAttribute(Name = \"dbo." + _modelname + "\")]");
            strclass.AppendSpaceLine(1, "public partial class " + _modelname + "Model : INotifyPropertyChanging, INotifyPropertyChanged,IEnumerable");
            strclass.AppendSpaceLine(1, "{");
            strclass.AppendSpaceLine(2, "public " + _modelname + "()");
            strclass.AppendSpaceLine(2, "{}");
            strclass.AppendLine(CreatModelMethod());
            strclass.AppendSpaceLine(1, "}");
            strclass.AppendLine("}");
            strclass.AppendLine("");

            return strclass.ToString();
        }
        #endregion

        #region 生成Model属性部分
        /// <summary>
        /// 生成实体类的属性
        /// </summary>
        /// <returns></returns>
        public string CreatModelMethod()
        {

            StringPlus strclass = new StringPlus();
            StringPlus strclass1 = new StringPlus();
            StringPlus strclass2 = new StringPlus();
            strclass.AppendSpaceLine(2, "#region Model");

            foreach (ColumnInfo field in Fieldlist)
            {
                string columnName = field.ColumnName;
                string columnTypedb = field.TypeName;
                bool IsIdentity = field.IsIdentity;
                bool ispk = field.IsPrimaryKey;
                bool cisnull = field.Nullable;
                //string defValue=field.DefaultVal;
                string deText = field.Description;
                string dbLength = "";
                string columnType = CodeCommon.DbTypeToCS(columnTypedb);
                string isnull = "";
                if (CodeCommon.isValueType(columnType))
                {
                    if ((!IsIdentity) && (!ispk) && (cisnull))
                    {
                        isnull = "?";//代表可空类型
                    }
                }
                strclass1.AppendSpace(2, "private " + columnType + isnull + " _" + columnName.ToLower());//私有变量

                if (field.Length.Length > 0)//字符串类型长度
                {
                    switch (columnTypedb.ToLower())
                    {
                        case "nchar":
                        case "ntext":
                        case "nvarchar":
                        case "char":
                        case "text":
                        case "varchar":
                        case "string":
                        case"binary":
                            if (field.Length == "0")
                            {
                                dbLength = "(MAX)";
                            }
                            else
                            {
                                dbLength = "(" + field.Length.ToString() + ")";
                            }
                            break;
                    }
                }

                if (field.DefaultVal.Length > 0)//默认值
                {
                    switch (columnType.ToLower())
                    {                        
                        case "int":
                        case "long":
                            strclass1.Append("=" + field.DefaultVal.Trim().Replace("'", ""));  
                            break;
                        case "bool":
                        case "bit":
                            {
                                string val=field.DefaultVal.Trim().Replace("'", "").ToLower();
                                if(val=="1"||val=="true")
                                {
                                    strclass1.Append("= true" );
                                }
                                else
                                {
                                    strclass1.Append("= false");
                                }
                                
                            }
                            break;
                        case "nchar":
                        case "ntext":
                        case "nvarchar":                          
                        case "char":
                        case "text":
                        case "varchar":
                        case "string":
                            if (field.DefaultVal.Trim().StartsWith("N'"))
                            {
                                strclass1.Append("=" + field.DefaultVal.Trim().Remove(0, 1).Replace("'", "\""));  
                            }
                            else
                            {
                                if (field.DefaultVal.Trim().IndexOf("'") > -1)
                                {
                                    strclass1.Append("=" + field.DefaultVal.Trim().Replace("'", "\""));
                                }
                                else
                                {
                                    strclass1.Append("= \"" + field.DefaultVal.Trim().Replace("(", "").Replace(")", "") + "\"");
                                }
                            }
                            break;
                        case "datetime":
                            if (field.DefaultVal == "getdate"||
                                field.DefaultVal == "Now()"||
                                field.DefaultVal == "Now"||
                                field.DefaultVal == "CURRENT_TIME" ||
                                field.DefaultVal == "CURRENT_DATE"
                                )
                            {
                                strclass1.Append("= DateTime.Now");                                
                            }
                            else
                            {
                                strclass1.Append("= Convert.ToDateTime(" + field.DefaultVal.Trim().Replace("'", "\"") + ")");
                            }
                            break;
                        case "uniqueidentifier":
                            {
                                //if (field.DefaultVal == "newid")
                                //{
                                //    strclass1.Append("=" + field.DefaultVal.Trim().Replace("'", ""));
                                //}                                
                            }
                            break;
                        case "decimal":
                        case "double":
                        case "float":
                            {
                                strclass1.Append("=" + field.DefaultVal.Replace("'", "").Replace("(", "").Replace(")", "").ToLower() + "M");                                
                            }
                            break;
                        //case "sys_guid()":
                        //    break;
                        default:                            
                        //    strclass1.Append("=" + field.DefaultVal);
                            break;

                    }                    
                }                
                strclass1.AppendLine(";");

                strclass2.AppendSpaceLine(2, "/// <summary>");
                strclass2.AppendSpaceLine(2, "/// " + deText);
                strclass2.AppendSpaceLine(2, "/// </summary>");

                if (ispk)//Linq to sql 的主键映射
                {
                    if (deText.Length <= 0)
                    {
                        strclass2.AppendSpaceLine(2, "[Display(Name = \"" + columnName + "\")]");
                    }
                    else
                    {
                        strclass2.AppendSpaceLine(2, "[Display(Name = \"" + deText + "\")]");
                    }
                    strclass2.AppendSpaceLine(2, "[Required]");
                    strclass2.AppendSpaceLine(2, "[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = \"_" + columnName.ToLower() + "\", AutoSync = AutoSync.Always, DbType = \"" + columnTypedb + " NOT NULL IDENTITY\", IsDbGenerated = true)]");
                }
                else
                {
                    if (deText.Length <= 0)
                    {
                        strclass2.AppendSpaceLine(2, "[Display(Name = \"" + columnName + "\")]");
                        strclass2.AppendSpaceLine(2, "[Required(ErrorMessage = \"" + columnName + "不能为空\")]");
                    }
                    else
                    {
                        strclass2.AppendSpaceLine(2, "[Display(Name = \"" + deText + "\")]");
                        strclass2.AppendSpaceLine(2, "[Required(ErrorMessage = \"" + deText + "不能为空\")]");
                    }

                    if (cisnull)
                    {
                        strclass2.AppendSpaceLine(2, "[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = \"_" + columnName.ToLower() + "\",DbType = \"" + columnTypedb + dbLength + "\")]");
                    }
                    else
                    {
                        strclass2.AppendSpaceLine(2, "[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = \"_" + columnName.ToLower() + "\",DbType = \"" + columnTypedb + dbLength + " NOT NULL\")]");
                    }
                }

                strclass2.AppendSpaceLine(2, "public " + columnType + isnull + " " + columnName);//属性
                strclass2.AppendSpaceLine(2, "{");
                strclass2.AppendSpaceLine(3, "set{" + " _" + columnName.ToLower() + "=value;}");
                strclass2.AppendSpaceLine(3, "get{return " + "_" + columnName.ToLower() + ";}");
                strclass2.AppendSpaceLine(2, "}");
            }
            strclass.Append(strclass1.Value);

            //strclass.Append("private List<article_albums> _albums;");

            strclass.Append(strclass2.Value);

            //strclass.Append(" /// <summary>");
            //strclass.Append("/// 图片相册列表");
            //strclass.Append("/// </summary>");
            //strclass.Append("public List<article_albums> albums");
            //strclass.Append("{");
            //strclass.Append("set { _albums = value; }");
            //strclass.Append("get { return _albums; }");
            //strclass.Append(" }");

            strclass.AppendSpaceLine(2, "#endregion Model");


            strclass.AppendSpaceLine(2, "#region INotifyPropertyChanging 成员");
            strclass.AppendSpaceLine(2, "public event PropertyChangingEventHandler PropertyChanging;");
            strclass.AppendSpaceLine(2, "#endregion");
            strclass.AppendSpaceLine(2, "#region INotifyPropertyChanged 成员");
            strclass.AppendSpaceLine(2, " public event PropertyChangedEventHandler PropertyChanged;");
            strclass.AppendSpaceLine(2, "#endregion");

            strclass.AppendSpaceLine(2, "#region IEnumerator 成员");
            strclass.AppendSpaceLine(2, " public IEnumerator GetEnumerator()");
            strclass.AppendSpaceLine(2, "{");
            strclass.AppendSpaceLine(2, " throw new NotImplementedException();");
            strclass.AppendSpaceLine(2, "}");
            strclass.AppendSpaceLine(2, "#endregion");

            return strclass.ToString(); 
        }

        #endregion
    }
}
