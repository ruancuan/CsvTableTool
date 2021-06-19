using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

//Csv中间数据
public class CsvMediumData
{
    //Csv名字
    public string CsvName;
    //Dictionary<字段名称, 字段类型>，记录类的所有字段及其类型
    public Dictionary<string, string> propertyNameTypeDic;
    //List<一行数据>，List<Dictionary<字段名称, 一行的每个单元格字段值>>
    //记录类的所有字段值，按行记录
    public List<Dictionary<string, string>> allItemValueRowList;
}

public class CsvTool : EditorWindow
{
    private List<string> fileNameList = new List<string>();
    private List<string> filePathList = new List<string>();

    private string showNotify;
    private Vector2 scrollPosition = Vector2.zero;

    [MenuItem("GameTool/CsvTool")]
    private static void CreateCsvDataClass()
    {
        EditorWindow.GetWindow(typeof(CsvTool));
    }

    private void Awake()
    {
        titleContent.text = "生成配置类";

    }

    private void OnEnable()
    {
        showNotify = "";
        GetCsvFile();
    }

    private void OnDisable()
    {
        showNotify = "";
    }

    //读取指定路径下的Csv文件名
    private void GetCsvFile()
    {
        fileNameList.Clear();
        filePathList.Clear();

        string filePath = StringTool.Instance.PathSplicing(Application.dataPath, PathTool.Instance.GetPathByType(PathTool.PathType.ConfigTable));
        if (!Directory.Exists(filePath))
        {
            showNotify = "无效路径：" + filePath;
            return;
        }
        string[] CsvFileFullPaths = Directory.GetFiles(filePath, "*.csv");

        if (CsvFileFullPaths == null || CsvFileFullPaths.Length == 0)
        {
            showNotify = filePath + "路径下没有找到Csv文件";
            return;
        }

        filePathList.AddRange(CsvFileFullPaths);
        for (int i = 0; i < filePathList.Count; i++)
        {
            string fileName = filePathList[i].Split('/').LastOrDefault();
            fileName = filePathList[i].Split('\\').LastOrDefault();
            fileNameList.Add(fileName);
        }
        showNotify = "找到Csv文件：" + fileNameList.Count + "个";
    }

    //自动创建C#脚本
    private void SelectCsvToCodeByIndex()
    {
        string filePath = StringTool.Instance.PathSplicing(Application.dataPath, PathTool.Instance.GetPathByType(PathTool.PathType.ConfigTable));
        string[] CsvFileFullPaths = Directory.GetFiles(filePath, "*.csv");

        if (CsvFileFullPaths == null || CsvFileFullPaths.Length == 0)
        {
            Debug.Log("Csv file count == 0");
            return;
        }
        //遍历所有Csv，创建C#类
        for (int i = 0; i < CsvFileFullPaths.Length; i++)
        {
            ReadOneCsvToCode(CsvFileFullPaths[i]);
        }
    }

    //创建Csv对应的C#类
    public static void ReadOneCsvToCode(string CsvFileFullPath)
    {
        //解析Csv获取中间数据
        string str = StringTool.Instance.PathSplicing(Application.dataPath, PathTool.Instance.GetPathByType(PathTool.PathType.ConfigTable));
        DataTable data = FileTool.ReadCsv(CsvFileFullPath);
        if (data != null)
        {
            //根据数据生成C#脚本
            string classCodeStr = CreateCodeStrByCsvData(CsvFileFullPath, data.Columns);
            if (!string.IsNullOrEmpty(classCodeStr))
            {
                string tempStr = StringTool.Instance.PathSplicing(Application.dataPath, PathTool.Instance.GetPathByType(PathTool.PathType.ConfigClass));
                string fileName = CsvFileFullPath.Split('/').LastOrDefault();
                string CsvName = CsvFileFullPath.Split('\\').LastOrDefault();
                CsvName = CsvName.Split('.')[0];
                //Csv名字
                //写文件，生成CSharp.cs
                if (FileTool.WriteCodeStrToSave(tempStr, CsvName + "CsvData", classCodeStr))
                {
                    Debug.Log("<color=green>Auto Create Csv Scripts Success : </color>" + CsvName);
                    return;
                }
            }
        }
        //生成失败
        Debug.LogError("Auto Create Csv Scripts Fail : " + (CsvFileFullPath == null ? "" : CsvFileFullPath));
    }

    ////自动创建Asset文件
    //private void SelectCodeToAssetByIndex(int index)
    //{
    //    if (index >= 0 && index < filePathList.Count)
    //    {
    //        string fullPath = filePathList[index];
    //        CsvDataReader.CreateOneCsvAsset(fullPath);
    //    }
    //    else
    //    {
    //        CsvDataReader.CreateAllCsvAsset();
    //    }
    //}

    private static Dictionary<string, string> GetPropertyNameTypeDic(System.Data.DataColumnCollection cols)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        int len = cols.Count;
        for (int i = 0; i < len; i++)
        {
            string str = cols[i].ToString();
            string[] strs = str.Split('|');
            if (strs.Length >= 2)
            {
                dict.Add(strs[0], strs[1]);
            }
        }
        return dict;
    }

    //创建代码，生成数据C#类
    public static string CreateCodeStrByCsvData(string path, System.Data.DataColumnCollection cols)
    {
        if (cols == null)
            return null;
        if (path == "")
            return null;
        string fileName = path.Split('/').LastOrDefault();
        string CsvName = path.Split('\\').LastOrDefault();
        if (string.IsNullOrEmpty(CsvName))
            return null;
        string tempCsvName = CsvName;
        CsvName = CsvName.Split('.')[0];
        //Dictionary<字段名称, 字段类型>
        Dictionary<string, string> propertyNameTypeDic = GetPropertyNameTypeDic(cols);
        if (propertyNameTypeDic == null || propertyNameTypeDic.Count == 0)
            return null;
        //List<一行数据>，List<Dictionary<字段名称, 一行的每个单元格字段值>>
        //List<Dictionary<string, string>> allItemValueRowList = CsvMediumData.allItemValueRowList;
        //if (allItemValueRowList == null || allItemValueRowList.Count == 0)
        //    return null;
        //行数据类名
        string itemClassName = CsvName + "CsvItem";
        //整体数据类名
        string dataClassName = CsvName + "CsvData";

        //生成类
        StringBuilder classSource = new StringBuilder();
        classSource.Append("/*Auto Create, Don't Edit !!!*/\n");
        classSource.Append("\n");
        //添加引用
        classSource.Append("using System;\n");
        classSource.Append("using System.Data;\n");
        classSource.Append("\n");
        //生成行数据类，记录每行数据
        classSource.Append(CreateCsvRowItemClass(itemClassName, propertyNameTypeDic));
        classSource.Append("\n");
        //生成整体数据类，记录整个Csv的所有行数据
        classSource.Append(CreateCsvDataClass(dataClassName, itemClassName, tempCsvName));
        classSource.Append("\n");
        ////生成Asset操作类，用于自动创建Csv对应的Asset文件并赋值
        //classSource.Append(CreateCsvAssetClass(CsvMediumData));
        //classSource.Append("\n");
        return classSource.ToString();
    }

    //生成行数据类
    private static StringBuilder CreateCsvRowItemClass(string itemClassName, Dictionary<string, string> propertyNameTypeDic)
    {
        //生成Csv行数据类
        StringBuilder classSource = new StringBuilder();
        classSource.Append("[Serializable]\n");
        classSource.Append("public class " + itemClassName + string.Format(" : ConfigTable<{0}>\n", itemClassName));
        classSource.Append("{\n");
        //声明所有字段
        foreach (var item in propertyNameTypeDic)
        {
            classSource.Append(CreateCodeProperty(item.Key, item.Value));
        }
        classSource.Append(string.Format("\tpublic override {0} Init(DataRow row)\n", itemClassName));
        classSource.Append("\t{\n");
        classSource.Append(string.Format("\t\t{0} info = new {0}();\n",itemClassName));
        int idx = 0;
        foreach (var item in propertyNameTypeDic)
        {
            classSource.Append(CreateCodeValue(item.Key, item.Value,idx++));
        }
        classSource.Append(string.Format("\t\treturn info;\n"));
        classSource.Append("\t}\n"); 
        classSource.Append("}\n");
        return classSource;
    }

    //生成数据类
    private static StringBuilder CreateCsvDataClass(string dataClassName, string itemClassName,string tableName)
    {
        StringBuilder classSource = new StringBuilder();
        //classSource.Append("[CreateAssetMenu(fileName = \"" + dataClassName + "\", menuName = \"Csv To ScriptableObject/Create " + dataClassName + "\", order = 1)]\n");
        classSource.Append("public class " + dataClassName + string.Format(" :  ConfigBase< {0},{1} >\n", dataClassName, itemClassName));
        classSource.Append("{\n");
        classSource.Append(string.Format("\tpublic {0}()\n", dataClassName));
        classSource.Append("\t{\n");
        classSource.Append(string.Format("\t\tm_path=\"{0}\";\n",tableName));
        classSource.Append("\t}\n");
        //声明字段，行数据类数组
        //classSource.Append("\tpublic " + itemClassName + "[] items;\n");
        classSource.Append("}\n");
        return classSource;
    }

    private static string CreateCodeValue(string name,string type,int idx)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        //判断字段类型 
        if (type == "int" || type == "Int" || type == "INT")
            type = string.Format("int.Parse(row[{0}].ToString())", idx);
        else if (type == "float" || type == "Float" || type == "FLOAT")
            type = string.Format("float.Parse(row[{0}].ToString())", idx);
        else if (type == "bool" || type == "Bool" || type == "BOOL")
            type = string.Format("row[{0}]", idx);
        //else if (type.StartsWith("enum") || type.StartsWith("Enum") || type.StartsWith("ENUM"))
        //    type = type.Split('|').LastOrDefault();
        else
            type = string.Format("row[{0}].ToString()", idx);
        //声明
        string propertyStr = string.Format("\t\tinfo.{0} =",name) + type +";\n";
        return propertyStr;
    }

    //声明行数据类字段
    private static string CreateCodeProperty(string name, string type)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        if (name == "ID")
            return null;

        //判断字段类型
        if (type == "int" || type == "Int" || type == "INT")
            type = "int";
        else if (type == "float" || type == "Float" || type == "FLOAT")
            type = "float";
        else if (type == "bool" || type == "Bool" || type == "BOOL")
            type = "bool";
        else if (type.StartsWith("enum") || type.StartsWith("Enum") || type.StartsWith("ENUM"))
            type = type.Split('|').LastOrDefault();
        else
            type = "string";
        //声明
        string propertyStr = "\tpublic " + type + " " + name + ";\n";
        return propertyStr;
    }

    //生成Asset操作类
    private static StringBuilder CreateCsvAssetClass(CsvMediumData CsvMediumData)
    {
        if (CsvMediumData == null)
            return null;

        string CsvName = CsvMediumData.CsvName;
        if (string.IsNullOrEmpty(CsvName))
            return null;

        Dictionary<string, string> propertyNameTypeDic = CsvMediumData.propertyNameTypeDic;
        if (propertyNameTypeDic == null || propertyNameTypeDic.Count == 0)
            return null;

        List<Dictionary<string, string>> allItemValueRowList = CsvMediumData.allItemValueRowList;
        if (allItemValueRowList == null || allItemValueRowList.Count == 0)
            return null;

        string itemClassName = CsvName + "CsvItem";
        string dataClassName = CsvName + "CsvData";

        StringBuilder classSource = new StringBuilder();
        classSource.Append("#if UNITY_EDITOR\n");
        //类名
        classSource.Append("public class " + CsvName + "AssetAssignment\n");
        classSource.Append("{\n");
        //方法名
        classSource.Append("\tpublic static bool CreateAsset(List<Dictionary<string, string>> allItemValueRowList, string CsvAssetPath)\n");
        //方法体，若有需要可加入try/catch
        classSource.Append("\t{\n");
        classSource.Append("\t\tif (allItemValueRowList == null || allItemValueRowList.Count == 0)\n");
        classSource.Append("\t\t\treturn false;\n");
        classSource.Append("\t\tint rowCount = allItemValueRowList.Count;\n");
        classSource.Append("\t\t" + itemClassName + "[] items = new " + itemClassName + "[rowCount];\n");
        classSource.Append("\t\tfor (int i = 0; i < items.Length; i++)\n");
        classSource.Append("\t\t{\n");
        classSource.Append("\t\t\titems[i] = new " + itemClassName + "();\n");
        foreach (var item in propertyNameTypeDic)
        {
            classSource.Append("\t\t\titems[i]." + item.Key + " = ");

            classSource.Append(AssignmentCodeProperty("allItemValueRowList[i][\"" + item.Key + "\"]", propertyNameTypeDic[item.Key]));
            classSource.Append(";\n");
        }
        classSource.Append("\t\t}\n");
        classSource.Append("\t\t" + dataClassName + " CsvDataAsset = ScriptableObject.CreateInstance<" + dataClassName + ">();\n");
        classSource.Append("\t\tCsvDataAsset.items = items;\n");
        classSource.Append("\t\tif (!Directory.Exists(CsvAssetPath))\n");
        classSource.Append("\t\t\tDirectory.CreateDirectory(CsvAssetPath);\n");
        classSource.Append("\t\tstring pullPath = CsvAssetPath + \"/\" + typeof(" + dataClassName + ").Name + \".asset\";\n");
        classSource.Append("\t\tUnityEditor.AssetDatabase.DeleteAsset(pullPath);\n");
        classSource.Append("\t\tUnityEditor.AssetDatabase.CreateAsset(CsvDataAsset, pullPath);\n");
        classSource.Append("\t\tUnityEditor.AssetDatabase.Refresh();\n");
        classSource.Append("\t\treturn true;\n");
        classSource.Append("\t}\n");
        //
        classSource.Append("}\n");
        classSource.Append("#endif\n");
        return classSource;
    }

    //声明Asset操作类字段
    private static string AssignmentCodeProperty(string stringValue, string type)
    {
        //判断类型
        if (type == "int" || type == "Int" || type == "INT")
        {
            return "Convert.ToInt32(" + stringValue + ")";
        }
        else if (type == "float" || type == "Float" || type == "FLOAT")
        {
            return "Convert.ToSingle(" + stringValue + ")";
        }
        else if (type == "bool" || type == "Bool" || type == "BOOL")
        {
            return "Convert.ToBoolean(" + stringValue + ")";
        }
        else if (type.StartsWith("enum") || type.StartsWith("Enum") || type.StartsWith("ENUM"))
        {
            return "(" + type.Split('|').LastOrDefault() + ")(Convert.ToInt32(" + stringValue + "))";
        }
        else
            return stringValue;
    }



    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition,
               GUILayout.Width(position.width), GUILayout.Height(position.height));
        //自动创建C#脚本
        GUILayout.Space(10);
        GUILayout.Label("Csv To Script");
        if (GUILayout.Button("All Csv", GUILayout.Width(200), GUILayout.Height(30)))
        {
            SelectCsvToCodeByIndex();
        }
        //自动创建Asset文件
        //GUILayout.Space(20);
        //GUILayout.Label("Script To Asset");
        //for (int i = 0; i < fileNameList.Count; i++)
        //{
        //    if (GUILayout.Button(fileNameList[i], GUILayout.Width(200), GUILayout.Height(30)))
        //    {
        //        SelectCodeToAssetByIndex(i);
        //    }
        //}
        //if (GUILayout.Button("All Csv", GUILayout.Width(200), GUILayout.Height(30)))
        //{
        //    SelectCodeToAssetByIndex(-1);
        //}
        //
        GUILayout.Space(20);
        GUILayout.Label(showNotify);
        //
        GUILayout.EndScrollView();

    }
}
