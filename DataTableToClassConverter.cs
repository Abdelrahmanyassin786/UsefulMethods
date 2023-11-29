        internal static class Datatable_To_Class
        {
            /// <summary>
            /// Converts the <typeparamref name="DataTable"/> passed to an <typeparamref name="IEnumerable"/><![CDATA[<]]><typeparamref name="T1"/><![CDATA[>]]> that contains <typeparamref name="T2"/> as a child field
            /// </summary>
            /// <typeparam name="T1">Type of parent obj that will be returned</typeparam>
            /// <typeparam name="T2">Type of child obj nested inside <typeparamref name="T1"/></typeparam>
            /// <param name="dt"><typeparamref name="DataTable"/> to convert</param>
            /// <param name="Name_Of_T2_In_T1">The field name of <typeparamref name="T2"/> in <typeparamref name="T1"/></param>
            /// <param name="FieldsToLinkT1_to_T2">The column names that will be used to link <typeparamref name="T2"/> to <typeparamref name="T1"/></param>
            /// <returns>If <paramref name="dt"/> is <see langword="null"/> returns  <see langword="new"/> <typeparamref name="List"/><![CDATA[<]]><typeparamref name="T1"/>>()</returns>
            internal static IEnumerable<T1> Datatable_To_List_Obj_Nested<T1, T2>(DataTable dt, string Name_Of_T2_In_T1, params string[] FieldsToLinkT1_to_T2) where T1 : class, new()
                                                                                                                                                              where T2 : class, new()
            {
                if (dt == null)
                    return new List<T1>();

                T1 t1;
                T2 t2;

                List<T1> T1_List = new List<T1>();
                List<T2> T2_List = new List<T2>();

                string[] T1_Fields = typeof(T1).GetFields().AsEnumerable().Select(a => a.Name).ToArray();
                string[] T2_Fields = typeof(T2).GetFields().AsEnumerable().Select(a => a.Name).ToArray();

                DataTable dt1 = new DataTable();
                DataTable dt2 = new DataTable();

                dt1 = DataTable_Helper.Get_Distinct_Rows_On(dt, T1_Fields);

                foreach (DataRow row in dt1.Rows)
                {
                    t1 = new T1();
                    t1 = DataRow_To_Obj<T1>(row);
                    DataTable T2_Linked_To_This_Row = dt.Select(DataTable_Helper.Select_Statement(row, FieldsToLinkT1_to_T2)).CopyToDataTable();
                    dt2 = DataTable_Helper.Get_Distinct_Rows_On(T2_Linked_To_This_Row, T2_Fields);
                    foreach (DataRow row2 in dt2.Rows)
                    {
                        t2 = new T2();
                        t2 = DataRow_To_Obj<T2>(row2);
                        T2_List.Add(t2);
                    }
                    Populate_Nested_Objs_Field_Info<T1, List<T2>>(t1, Name_Of_T2_In_T1, T2_List);
                    T2_List = new List<T2>();
                    T1_List.Add(t1);
                }

                return T1_List;
            }

            /// <summary>
            /// Converts the passed <typeparamref name="DataTable"/> to an <typeparamref name="IEnumerable"/><![CDATA[<]]><typeparamref name="T1"/>>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="dt"></param>
            /// <returns>If <paramref name="dt"/> is <see langword="null"/> returns  <see langword="new"/> <typeparamref name="List"/><![CDATA[<]]><typeparamref name="T"/>></returns>
            internal static IEnumerable<T> Datatable_To_List_Obj<T>(this DataTable dt) where T : class, new()
            {
                if (dt == null)
                    return new List<T>();

                List<T> list = new List<T>();
                foreach (DataRow dr in dt.Rows)
                {
                    T obj = DataRow_To_Obj<T>(dr);
                    list.Add(obj);
                }
                return list;
            }
            /// <summary>
            /// Takes a <typeparamref name="DataRow"/>, and returns a new instance of type <typeparamref name="T"/> with the fields of the class populated with the values of the row passed.
            /// <br></br>
            /// Only the Column Names that exist in the passed object <typeparamref name="T"/> will be populated.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="dr"></param>
            /// <returns>If <paramref name="dr"/> is <see langword="null"/> returns <see langword="new"/> <typeparamref name="T"/>()</returns>
            internal static T DataRow_To_Obj<T>(DataRow dr) where T : class, new()
            {
                if (dr == null)
                    return new T();

                T Obj = new T();

                foreach (DataColumn c in dr.Table.Columns)
                {
                    Populate_Obj_Field_Info<T>(Obj, c.ColumnName, dr[c.ColumnName].ToString());
                }

                return Obj;
            }
            /// <summary>
            /// Populates the <paramref name="fieldName"/> with the <paramref name="fieldValue"></paramref> for the passed <paramref name="obj"/>
            /// </summary>
            /// <typeparam name="T">Type of <paramref name="obj"/> </typeparam>
            /// <param name="obj">obj to fill</param>
            /// <param name="fieldName">field Name of <paramref name="obj"/></param>
            /// <param name="fieldValue">value to fill in <paramref name="fieldName"/></param>
            private static void Populate_Obj_Field_Info<T>(T obj, string fieldName, string fieldValue) where T : class
            {
                if (typeof(T).GetFields().ToList().Exists(p => p.Name.ToLower() == (fieldName.ToLower())) && fieldValue != null)
                {
                    FieldInfo fieldInfo = obj.GetType().GetField(fieldName);
                    dynamic _fieldValue = Convert_To_Field_Type(fieldInfo.FieldType.FullName, fieldValue);
                    fieldInfo.SetValue(obj, (_fieldValue));
                }
            }
            /// <summary>
            /// inserts an object <paramref name="childValue"/> into <paramref name="parentObj"/> 
            /// </summary>
            /// <typeparam name="T1">Type of <paramref name="parentObj"/></typeparam>
            /// <typeparam name="T2">Type of <paramref name="childValue"/>; could be another class of any IEnumerable for example</typeparam>
            /// <param name="parentObj"></param>
            /// <param name="childName">Name of <paramref name="childValue"/> in <paramref name="parentObj"/></param>
            /// <param name="childValue"></param>
            private static void Populate_Nested_Objs_Field_Info<T1, T2>(T1 parentObj, string childName, T2 childValue) where T1 : class
                                                                                                                       where T2 : class
            {
                if (typeof(T1).GetFields().ToList().Exists(p => p.Name.ToLower() == (childName.ToLower())) && childValue != null)
                {
                    FieldInfo fieldInfo = parentObj.GetType().GetField(childName);
                    fieldInfo.SetValue(parentObj, childValue);
                }
            }
            /// <summary>
            /// converts <paramref name="value"/> to the passed in <paramref name="fieldType"/>
            /// </summary>
            private static dynamic Convert_To_Field_Type(string fieldType, string value)
            {
                dynamic result = null;
                bool Parsed = false;

                Decimal asd = 0;

                switch (fieldType)
                {
                    case "System.String":
                        {
                            result = value;
                            break;
                        }
                    case "System.Boolean":
                    case "System.bool":
                        {
                            bool b;
                            Parsed = bool.TryParse(value, out b);
                            if (!Parsed)
                                result = false;
                            else
                                result = b;
                            break;
                        }
                    case "System.Byte":
                        {
                            Byte b;
                            Parsed = Byte.TryParse(value, out b);
                            if (!Parsed)
                                result = new Byte();
                            else
                                result = b;
                            break;
                        }
                    case "System.Char":
                        {
                            char c;
                            Parsed = char.TryParse(value, out c);
                            if (!Parsed)
                                result = string.Empty[0];
                            else
                                result = c;
                            break;
                        }
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        {
                            int i;
                            Parsed = int.TryParse(value, out i);
                            if (!Parsed)
                                result = (int)0;
                            else
                                result = i;
                            break;
                        }
                    case "System.Double":
                        {
                            Double d;
                            Parsed = Double.TryParse(value, out d);
                            if (!Parsed)
                                result = (Double)0;
                            else
                                result = d;
                            break;
                        }
                    case "System.Decimal":
                        {
                            Decimal d;
                            Parsed = Decimal.TryParse(value, out (d));
                            if (!Parsed)
                                result = (Decimal)d;
                            else
                                result = d;
                            break;
                        }
                    case "System.Single":
                    case "System.float":
                        {
                            float f;
                            Parsed = float.TryParse(value, out f);
                            if (!Parsed)
                                result = (float)0;
                            else
                                result = f;
                            break;
                        }
                    case "System.DateTime":
                        {
                            DateTime d;
                            Parsed = DateTime.TryParse(value, out d);
                            if (!Parsed)
                                result = new DateTime();
                            else
                                result = d;
                            break;
                        }
                    default:
                        {
                            return null;
                        }
                }
                return result;
            }
        }
        internal static class DataTable_Helper
        {
            internal static string Select_Statement(DataRow dr, string[] Column_Names, string AND_OR = "AND")
            {
                AND_OR = " " + AND_OR + " ";
                StringBuilder sb = new StringBuilder();

                foreach (string col in Column_Names)
                {
                    if (!dr[col].Equals(DBNull.Value))
                    {
                        sb.Append(col);
                        sb.Append(" = ");
                        if (dr[col].GetType() == typeof(string))
                        {
                            sb.Append('\'');
                            sb.Append(dr[col].ToString().Replace("'", "''"));
                            sb.Append('\'');
                        }
                        else
                        {
                            sb.Append(dr[col].ToString());
                        }
                        sb.Append(AND_OR);
                    }
                }
                sb.Append(" 1=1 ");

                return sb.ToString();
            }

            /// <summary>
            /// Creates and returns a <see langword="new"/> <typeparamref name="DataTable"/> that contains only distinct rows, based on <paramref name="Column_Names"/>.
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="Column_Names">The column names that will be used to get distinct rows </param>
            /// <returns>Returns a <typeparamref name="DataTable"/> with the <paramref name="Column_Names"/> passed and all rows are distinct</returns>
            internal static DataTable Get_Distinct_Rows_On(DataTable dt, string[] Column_Names)
            {
                DataView view = new DataView(dt);
                StringBuilder Sort = new StringBuilder();
                string[] _column_Names = null;
                bool is_first = true;

                foreach (string col in Column_Names)
                {
                    if (!is_first)
                        Sort.Append(",");

                    if (dt.Columns.Contains(col))
                    {
                        Sort.Append(col);
                        is_first = false;
                    }
                }

                view.Sort = Sort.ToString().Trim().TrimEnd(',');
                _column_Names = Sort.ToString().Trim().TrimEnd(',').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                return view.ToTable(true, _column_Names);
            }
        }
