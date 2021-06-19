using System.Text;

public class StringTool
{
    private static StringTool _Instance;
    public static StringTool Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new StringTool();
            return _Instance;
        }
    }

    /// <summary>
    /// 字符串拼接
    /// </summary>
    /// <param name="str1"></param>
    /// <param name="str2"></param>
    /// <returns></returns>
    public string StringSplicing_Long(string str1, string str2)
    {
        StringBuilder result = new StringBuilder(str1.Length + str2.Length);
        result.Append(str1);
        result.Append(str2);
        return result.ToString();
    }

    public string StringSplicing_Short(string str1, string str2)
    {
        return str1 + str2;
    }

    /// <summary>
    /// 路径拼接
    /// </summary>
    /// <param name="str"></param>
    /// <param name="strArray"></param>
    /// <returns></returns>
    public string PathSplicing(string str1, string str2)
    {
        string result = "";
        result = StringSplicing_Short(str1, "/");
        result = StringSplicing_Long(result, str2);
        return result;
    }

    public string PathSplicing(string str, params string[] strArray)
    {
        string result = "";
        int len = strArray.Length;
        result = str;
        for (int i = 0; i < len; i++)
        {
            result = StringSplicing_Short(result, StringSplicing_Short("/", strArray[i]));
        }
        return result;
    }


}
