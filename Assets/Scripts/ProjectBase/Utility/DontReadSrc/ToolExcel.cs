using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using OfficeOpenXml;
//using Sirenix.OdinInspector;
//using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
//using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace JLExcelEditor.Editor.ToolEx
{
    public enum GetSheetType { 行, 列 }

    public static class ToolExs
    {
        /// <summary>
        /// 获取新的指定地址
        /// </summary>
        /// <param name="oldFilePath"></param>
        /// <param name="oldIndex"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string GetNewPath(string oldFilePath, int oldIndex, string suffix)
        {
            string newFilePath = $"{oldFilePath}{oldIndex}.{suffix}";
            while (File.Exists(newFilePath))
            {
                oldIndex++;
                newFilePath = $"{oldFilePath}{oldIndex}.{suffix}";
            }
            return newFilePath;
        }

        /// <summary>
        /// 获取对象的父级目录地址
        /// </summary>
        /// <param name="asset">资源对象</param>
        /// <typeparam name="T">资源的类型</typeparam>
        /// <returns>父级目录地址</returns>
        public static string GetFolderPath<T>(this T asset) where T : Object
        {
            string oldPath = AssetDatabase.GetAssetPath(asset);
            int suffixIndexOf = oldPath.LastIndexOf('/');
            string newPath = oldPath.Substring(0, suffixIndexOf);
            return oldPath.Replace(newPath, "");
        }

        /// <summary>
        /// 输入excel地址获取全部sheet名称List
        /// </summary>
        /// <param name="excelPath">excel的地址</param>
        /// <returns>全部sheet名称List</returns>
        public static List<string> GetExcelSheetName(this string excelPath)
        {
            List<string> sheetNames = new List<string>();
            FileInfo fileInfo = new FileInfo(excelPath);
            using (ExcelPackage excelData = new ExcelPackage(fileInfo))
            {
                ExcelWorksheets sheets = excelData.Workbook.Worksheets;
                sheetNames = sheets.Select(x => x.Name).ToList();
            }

            return sheetNames;
        }

        /// <summary>
        /// 输入excel文件获取全部sheet名称List
        /// </summary>
        /// <param name="excelFile">excel文件</param>
        /// <returns>全部sheet名称List</returns>
        public static List<string> GetExcelSheetName(DefaultAsset excelFile)
        {
            string path = AssetDatabase.GetAssetPath(excelFile);
            return GetExcelSheetName(path);
        }

        /// <summary>
        /// 获取表数据,并返回数据合集
        /// </summary>
        static List<string> GetSheetData(ExcelWorksheet tableSheet, int index, GetSheetType _getType = GetSheetType.行)
        {
            List<string> _datas = new List<string>();
            int countNum = _getType == GetSheetType.行 ? tableSheet.Dimension.Columns : tableSheet.Dimension.Rows;
            for (int i = 1; i <= countNum; i++)
            {
                if (_getType == GetSheetType.行)
                {
                    _datas.Add(tableSheet.GetValue(index, i) as string ?? "");
                }
                else
                {
                    _datas.Add(tableSheet.GetValue(i, index) as string ?? "");
                }
            }

            return _datas;
        }





        /// <summary>
        /// 获取初始名称
        /// </summary>
        public static string GetTypeName(this Type type)
        {
            if (type == typeof(bool)) return "bool";
            if (type == typeof(char)) return "char";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(sbyte)) return "sbyte";
            if (type == typeof(short)) return "short";
            if (type == typeof(int)) return "int";
            if (type == typeof(long)) return "long";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(string)) return "string";

            if (type.IsArray)
            {
                Type arrayType = type.Assembly.GetType(type.Name.Replace("[]", string.Empty));
                return $"{GetTypeName(arrayType)}[]";
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return $"List<{GetTypeName(type.GetGenericArguments()[0])}>";
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Debug.Log(type.GetGenericArguments()[0].Name + "," + type.GetGenericArguments()[1].Name);
                return $"Dictionary<{GetTypeName(type.GetGenericArguments()[0])}, {GetTypeName(type.GetGenericArguments()[1])}>";
            }
            return type.Name;
        }

        /// <summary>
        /// 获取所有字段信息
        /// </summary>
        public static FieldInfo[] GetFieldInfos(this Type type)
        {
            return type.GetFields(BindingFlags.DeclaredOnly |
                                  BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                  BindingFlags.NonPublic);
        }

        /// <summary>
        /// 对象手动序列化为字符串
        /// 值类型多个元素用 ","隔开
        /// 多个元素之间用 “;”隔开 例： List中 item1;item2
        /// 键值对中key与value 用 *隔开 例： key*value
        /// 不支持元组
        /// 其他类型直接转为string
        /// 是资源时转为资源地址。Sprite 例：  Assets/Resources/Image/item1.png
        /// 是资源集合时用尾号代表读取序号，此序号并非看到的切割的序号！ Sprite 例： Assets/Resources/Image/item1.png|1
        /// </summary>
        public static string ObjectToString(this object obj)
        {
            if (obj == null) return "";
            try
            {
                Type _type = obj.GetType();
                #region 默认数据
                if (_type.IsArray)
                {
                    Array sz = (Array)obj;
                    string str = "";
                    for (int i = 0; i < sz.Length; i++)
                    {
                        str += ObjectToString(sz.GetValue(i));
                        if (i == sz.Length - 1) continue;
                        str += ";";
                    }
                    return str;
                }
                if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    string str = "";

                    // 获取列表List<Man>长度
                    IList iList = (IList)obj;
                    for (int i = 0; i < iList.Count; i++)
                    {
                        str += ObjectToString(iList[i]);
                        if (i == iList.Count - 1) continue;
                        str += ";";
                    }
                    return str;
                }
                if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    string str = "";
                    IDictionary dic = obj as IDictionary;
                    int count = dic.Count;
                    IDictionaryEnumerator enu = dic.GetEnumerator();
                    for (int i = 0; i < count; i++)
                    {
                        enu.MoveNext();
                        str += $"{ObjectToString(enu.Key)}*{ObjectToString(enu.Value)}";
                        if (i == count - 1) continue;
                        str += ";";
                    }
                    return str;
                }
                if (_type.IsSubclassOf(typeof(Object)))
                {
                    string path = AssetDatabase.GetAssetPath((Object)obj);
                    Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
                    if (objs.Length <= 1) return path;

                    for (int i = 1; i < objs.Length; i++)
                    {
                        if (objs[i] == (Object)obj)
                        {
                            path += $"|{i - 1}";
                        }
                    }
                    return path;
                }

                return obj.ToString();
                #endregion
            }
            catch (Exception e)
            {
                Debug.Log("填入的数据不规范");
            }
            return "";
        }

        /// <summary>
        /// 将字符串反序列化为对象
        /// </summary>
        public static object StringToObject(this string valueStr, Type objType)
        {
            Debug.Log("---StringToObject---");
            object objData = null;
            objData = valueStr.ToUnityEngineObject(objType);
            if (objData != null) return objData;

            if (objType.IsEnum)
            {
                if (objType.IsDefined(typeof(FlagsAttribute), true))
                {
                    int _enumValue = 0;
                    string[] _split = valueStr.Split(',');
                    for (int i = 0; i < _split.Length; i++)
                    {
                        _enumValue += (int)Enum.Parse(objType, _split[i]);
                    }

                    objData = Enum.Parse(objType, _enumValue.ToString());
                }
                else
                {
                    objData = Enum.Parse(objType, valueStr);
                }
            }
            else if (objType.IsSubclassOf(typeof(Object)))
            {
                string[] _split = valueStr.Split('|');
                Debug.Log(valueStr + "  " + _split.Length);
                if (_split.Length == 1)
                {
                    objData = AssetDatabase.LoadAssetAtPath(valueStr, objType);
                }
                else
                {
                    string path = _split[0];
                    int _index = int.Parse(_split[1]);
                    object[] s = AssetDatabase.LoadAllAssetsAtPath(path);
                    objData = AssetDatabase.LoadAllAssetsAtPath(path)[_index + 1];
                }
            }
            else if (objType.IsArray)
            {
                valueStr = valueStr.Replace("\n", "");
                Type memberType = objType.GetElementType();
                string[] data = valueStr.Split(';');
                Array array = Array.CreateInstance(memberType, data.Length);
                for (int i = 0; i < data.Length; i++)
                {
                    array.SetValue(data[i].StringToObject(memberType), i);
                }
                return array;
            }
            else if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(List<>))
            {
                valueStr = valueStr.Replace("\n", "");
                string[] data = valueStr.Split(';');
                object list = Activator.CreateInstance(objType);
                Type valueType = objType.GetGenericArguments()[0];
                IList iList = (IList)list;
                for (int i = 0; i < data.Length; i++)
                {
                    iList.Add(data[i].StringToObject(valueType));
                }
                return iList;
            }
            else if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                valueStr = valueStr.Replace("\n", "");
                string[] datas = valueStr.Split(';');
                object dic = Activator.CreateInstance(objType);
                IDictionary iDic = (IDictionary)dic;
                Type keyType = objType.GetGenericArguments()[0];
                Type valueType = objType.GetGenericArguments()[1];

                for (int i = 0; i < datas.Length; i++)
                {
                    string[] data = datas[i].Split('*');
                    iDic.Add(data[0].StringToObject(keyType), data[1].StringToObject(valueType));
                }
                return iDic;
            }
            else
            {

                return valueStr.Format(objType);
            }
            return objData;
        }

        /// <summary>
        /// 转换为Unity的特殊值时
        /// </summary>
        /// <param name="valueStr"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static object ToUnityEngineObject(this string valueStr, Type objType)
        {
            object returnObj = null;
            if (objType.Namespace != "UnityEngine") return null;
            Debug.Log("---ToUnityEngineObject---");
            valueStr = valueStr.Replace("(", "").Replace(")", "");
            string[] sp = valueStr.Split(',');

            switch (objType.Name)
            {
                case nameof(Vector2):
                    returnObj = new Vector2((float)sp[0].StringToObject(typeof(float)), (float)sp[1].StringToObject(typeof(float)));
                    break;
                case nameof(Vector3):
                    returnObj = new Vector3((float)sp[0].StringToObject(typeof(float))
                                            , (float)sp[1].StringToObject(typeof(float))
                                            , (float)sp[2].StringToObject(typeof(float)));
                    break;
                case nameof(Vector4):
                    returnObj = new Vector4((float)sp[0].StringToObject(typeof(float))
                                            , (float)sp[1].StringToObject(typeof(float))
                                            , (float)sp[2].StringToObject(typeof(float))
                                            , (float)sp[3].StringToObject(typeof(float)));
                    break;
                case nameof(Rect):
                    returnObj = new Rect((float)sp[0].StringToObject(typeof(float))
                        , (float)sp[1].StringToObject(typeof(float))
                        , (float)sp[2].StringToObject(typeof(float))
                        , (float)sp[3].StringToObject(typeof(float)));
                    break;
                case nameof(Vector2Int):
                    returnObj = new Vector2Int((int)sp[0].StringToObject(typeof(int))
                                                , (int)sp[1].StringToObject(typeof(int)));
                    break;
                case nameof(Vector3Int):
                    returnObj = new Vector3Int((int)sp[0].StringToObject(typeof(int))
                                                , (int)sp[1].StringToObject(typeof(int))
                                                , (int)sp[2].StringToObject(typeof(int)));
                    break;
                case nameof(RectInt):
                    returnObj = new RectInt((int)sp[0].StringToObject(typeof(int))
                                        , (int)sp[1].StringToObject(typeof(int))
                                        , (int)sp[2].StringToObject(typeof(int))
                                        , (int)sp[3].StringToObject(typeof(int)));
                    break;
                case nameof(Color):
                    returnObj = new Color((float)sp[0].StringToObject(typeof(float))
                        , (float)sp[1].StringToObject(typeof(float))
                        , (float)sp[2].StringToObject(typeof(float))
                        , (float)sp[3].StringToObject(typeof(float)));
                    break;
                case nameof(Matrix4x4):
                    Matrix4x4 matrix4X4 = new Matrix4x4();
                    for (int i = 0; i < 16; i++)
                    {
                        matrix4X4[i] = (float)sp[i].StringToObject(typeof(float));
                    }
                    returnObj = matrix4X4;
                    break;
            }
            return returnObj;
        }

        /// <summary>
        /// string转为其他数据类型
        /// </summary>
        public static object Format(this string str, Type type)
        {
            Debug.Log("---Format---");
            if (string.IsNullOrEmpty(str)) return null;
            if (type == null) return str;

            return ConvertSimpleType(str, type);
        }
        private static object ConvertSimpleType(object value, Type destinationType)
        {
            object returnValue;
            if (value == null || destinationType.IsInstanceOfType(value)) return value;

            if (value is string str && str.Length == 0) return null;

            TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
            bool flag = converter.CanConvertFrom(value.GetType());
            if (!flag)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }
            if (!flag && !converter.CanConvertTo(destinationType))
            {
                throw new InvalidOperationException("无法转换成类型：" + value + "==>" + destinationType);
            }
            try
            {
                returnValue = flag ? converter.ConvertFrom(null, null, value) : converter.ConvertTo(null, null, value, destinationType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("类型转换出错：" + value + "==>" + destinationType, e);
            }
            return returnValue;
        }



        /// <summary>
        /// 创建实例脚本
        /// </summary>
        /// <param name="excelPath">表格地址</param>
        /// <param name="sheetName">使用的表中sheet名</param>
        /// <param name="scriptPath">生成的位置</param>
        /// <param name="className">生成的脚本类名</param>
        public static void CreateScript(string excelPath, string sheetName, string scriptPath, string className)
        {
            List<string> fieldNames = new List<string>();
            List<string> typeNames = new List<string>();
            List<string> attrNames = new List<string>();
            FileInfo fileInfo = new FileInfo(excelPath);
            using (ExcelPackage excelData = new ExcelPackage(fileInfo))
            {
                ExcelWorksheets sheets = excelData.Workbook.Worksheets;
                ExcelWorksheet tableSheet = sheets[sheetName];
                fieldNames = GetSheetData(tableSheet, 1);
                typeNames = GetSheetData(tableSheet, 2);
                attrNames = GetSheetData(tableSheet, 3);
            }

            string scriptStr = SpliceScriptStr(className, fieldNames, typeNames, attrNames);
            Create($"{scriptPath}/{className}.cs", scriptStr);
        }

        /// <summary>
        /// 利用模板拼接需要的脚本
        /// </summary>
        /// <param name="className">脚本的类名</param>
        /// <param name="fieldNames">字段名</param>
        /// <param name="typeNames">字段类型名</param>
        /// <param name="infos">注释或者特性</param>
        /// <returns></returns>
        public static string SpliceScriptStr(string className, List<string> fieldNames, List<string> typeNames,
            List<string> infos)
        {
            string scriptStr = Resources.Load<TextAsset>("ScriptDemo").text;
            string fieldName = "";
            for (int i = 0; i < fieldNames.Count; i++)
            {
                fieldName += $"    {infos[i] ?? ""}\n" +
                             $"    public {typeNames[i]} {fieldNames[i]};\n";

            }

            scriptStr = scriptStr.Replace("[#ClassName]", className)
                .Replace("[#Field]", fieldName)
                .Replace("[#FieldSet]", "");
            return scriptStr;
        }
        /// <summary>
        /// 利用模板拼接需要的脚本
        /// </summary>
        /// <param name="className">脚本的类名</param>
        /// <param name="fieldNames">字段名</param>
        /// <param name="typeNames">字段类型名</param>
        /// <returns></returns>
        public static string SpliceScriptStr(string className, List<string> fieldNames, List<string> typeNames,
            string scriptStr = "")
        {
            scriptStr = scriptStr == "" ? Resources.Load<TextAsset>("ScriptDemo").text : scriptStr;
            string fieldName = "";
            for (int i = 0; i < fieldNames.Count; i++)
            {
                fieldName += $"    public {typeNames[i]} {fieldNames[i]};\n";

            }

            scriptStr = scriptStr.Replace("[#ClassName]", className)
                .Replace("[#Field]", fieldName);
            return scriptStr;
        }



        /// <summary>
        /// 文本创建
        /// </summary>
        /// <param name="filePath">文本文件地址</param>
        /// <param name="data">使用的文本数据</param>
        public static void Create(string filePath, string data)
        {
            StreamWriter file = new StreamWriter(filePath, false);
            file.Write(data);
            file.Close();
            AssetDatabase.Refresh();
        }



        /// <summary>
        /// 导入ScriptableObject
        /// </summary>
        /// <param name="excelPath">读取excle文件的地址</param>
        /// <param name="sheetName">对应的sheet名称</param>
        /// <param name="filePath">ScriptableObject文件存放的位置</param>
        /// <param name="fileName">ScriptableObject文件的名称</param>
        /// <param name="_type">生成的对象类型信息</param>
        /// <param name="overwrite">生成时是否按行覆盖，默认为覆盖</param>
        /// <param name="isAppoint">是否指定起始行数和结尾行</param>
        /// <param name="mix">指定的起始行</param>
        /// <param name="max">指定的结尾行</param>
        public static void CreateSOData(string excelPath, string sheetName,
            string filePath, string fileName, Type _type, bool overwrite = true,
            bool isAppoint = false, int mix = 4, int max = 4)
        {
            FileInfo fileInfo = new FileInfo(excelPath);
            using (ExcelPackage excelData = new ExcelPackage(fileInfo))
            {
                ExcelWorksheets sheets = excelData.Workbook.Worksheets;
                ExcelWorksheet tableSheet = sheets[sheetName];
                FieldInfo[] propertys = GetFieldInfos(_type);
                int readStart = 4;
                int readEnd = tableSheet.Dimension.Rows;
                if (isAppoint)
                {
                    readStart = mix;
                    readEnd = max;
                }

                for (int i = readStart; i <= readEnd; i++)
                {
                    string str = "";
                    var soItem = SerializedScriptableObject.CreateInstance(_type);
                    for (int j = 1; j <= tableSheet.Dimension.Columns; j++)
                    {
                        string _value = "";
                        if (tableSheet.Cells[i, j].Value == null)
                        {
                            propertys[j - 1].SetValue(soItem, default);
                            Debug.Log($"[{i},{j}]数据为空");
                            continue;
                        }

                        _value = tableSheet.Cells[i, j].Value.ToString();
                        propertys[j - 1].SetValue(soItem, _value.StringToObject(propertys[j - 1].FieldType));
                    }

                    string path = filePath + "/" + fileName + (i - 4) + ".asset";
                    if (!overwrite)
                    {
                        path = GetNewPath(filePath + "/" + fileName, i - 4, "asset");
                    }

                    AssetDatabase.CreateAsset(soItem, path);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

        }

        /// <summary>
        /// 使用对应类型创建excel模板
        /// </summary>
        /// <param name="excelPath">Excel的地址</param>
        /// <param name="sheetName">表中对应的sheet名称</param>
        /// <param name="fileName">生成的表名称</param>
        /// <param name="_type">使用的对象类型</param>
        public static void CreateExcelTemp(string excelPath, string sheetName, string fileName, Type _type, bool isOverwrite = true)
        {
            string path = $"{excelPath}/{sheetName}.xlsx";

            if (!isOverwrite)
            {
                path = GetNewPath($"{excelPath}/{sheetName}", 1, "xlsx");
            }
            FileInfo fileInfo = new FileInfo(path);
            FieldInfo[] fields = GetFieldInfos(_type);
            List<string> fieldNames = new List<string>();
            List<string> fieldTypes = new List<string>();

            for (int i = 0; i < fields.Length; i++)
            {
                fieldNames.Add(fields[i].Name);
                fieldTypes.Add(fields[i].FieldType.GetTypeName());

            }

            using (ExcelPackage excelData = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet sheet = excelData.Workbook.Worksheets.Add(sheetName);
                for (int i = 0; i < fieldNames.Count; i++)
                {
                    sheet.SetValue(1, i + 1, fieldNames[i]);
                    sheet.SetValue(2, i + 1, fieldTypes[i]);
                    sheet.SetValue(3, i + 1, fieldNames[i] + "的注释和特性");
                }
                excelData.Save();
            }
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 导出List中的SO数据,在表格中生成
        /// </summary>
        /// <param name="objs">so数据</param>
        /// <param name="excelPath">存放导出的数据的excel表</param>
        /// <param name="sheetName">存放导出的数据的sheet表</param>
        /// <param name="_type">生成的数据类型</param>
        /// <param name="overwrite">生成时是否按行覆盖，默认为覆盖</param>
        /// <param name="isAppoint">是否指定起始行数和结尾行</param>
        /// <param name="mix">指定的起始行</param>
        public static void LoadListInputExcel(List<object> objs, string excelPath, string sheetName, Type _type,
            bool overwrite = true, bool isAppoint = false, int mix = 4)
        {
            FileInfo fileInfo = new FileInfo(excelPath);
            using (ExcelPackage excelData = new ExcelPackage(fileInfo))
            {
                ExcelWorksheets sheets = excelData.Workbook.Worksheets;
                ExcelWorksheet tableSheet = sheets[sheetName];
                FieldInfo[] fieldInfos = GetFieldInfos(_type);

                int readStart = 4;
                if (isAppoint)
                {
                    readStart = mix;
                }
                if (!overwrite)
                {
                    Debug.Log(tableSheet.Cells.Rows);
                    readStart = tableSheet.Cells.Rows + 1;
                }

                for (int i = 0; i < objs.Count; i++)
                {
                    for (int j = 0; j < fieldInfos.Length; j++)
                    {
                        var field = fieldInfos[j].GetValue(objs[i]);
                        tableSheet.SetValue(readStart + i, j + 1, ObjectToString(field));
                    }
                }
                excelData.Save();
            }
        }

    }
}