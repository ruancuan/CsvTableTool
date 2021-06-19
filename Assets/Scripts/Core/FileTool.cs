using UnityEngine;
using System.IO;
using System.Text;
using System.Data;

public class FileTool
{
    //返回读取的配置信息
    public static DataTable ReadCsv(string filePath)
    {
        Encoding encoding = GetFileEncoding(filePath);
        DataTable dataTable = new DataTable();
        FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        StreamReader streamReader = new StreamReader(fileStream, encoding);

        string strLine = "";
        string[] tableHead = null;
        string[] tableRowData = null;
        int columnCount = 0;
        bool isFirst = true;
        while ((strLine = streamReader.ReadLine()) != null)
        {
            if (isFirst)
            {
                isFirst = false;
                tableHead = strLine.Split(',');
                columnCount = tableHead.Length;
                for (int i = 0; i < columnCount; i++)
                {
                    DataColumn dataColumn = new DataColumn(tableHead[i]);
                    dataTable.Columns.Add(dataColumn);
                }
            }
            else
            {
                tableRowData = strLine.Split(',');
                DataRow dataRow = dataTable.NewRow();
                for (int i = 0; i < columnCount; i++)
                {
                    dataRow[i] = tableRowData[i];
                }
                dataTable.Rows.Add(dataRow);
            }
        }
        return dataTable;
    }

    public static Encoding GetFileEncoding(string filePath)
    {
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            BinaryReader binaryReader = new BinaryReader(fileStream);
            byte[] buffer = binaryReader.ReadBytes(2);
            if (buffer[0] >= 0xEF)
            {
                if (buffer[0] == 0xEF && buffer[1] == 0xBB)
                    return Encoding.UTF8;
                else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                    return Encoding.BigEndianUnicode;
                else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                    return Encoding.Unicode;
            }
            return Encoding.Default;
        }
    }
    
    //写文件
    public static bool WriteCodeStrToSave(string writeFilePath, string codeFileName, string classCodeStr)
    {
        if (string.IsNullOrEmpty(codeFileName) || string.IsNullOrEmpty(classCodeStr))
            return false;
        //检查导出路径
        if (!Directory.Exists(writeFilePath))
            Directory.CreateDirectory(writeFilePath);
        //写文件，生成CS类文件
        StreamWriter sw = new StreamWriter(writeFilePath + "/" + codeFileName + ".cs");
        sw.WriteLine(classCodeStr);
        sw.Close();
        //
        UnityEditor.AssetDatabase.Refresh();
        return true;
    }
}
