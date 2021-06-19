using System.Collections.Generic;

public class PathTool
{
    private static PathTool _Instance;
    public static PathTool Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new PathTool();
            return _Instance;
        }
    }

    private Dictionary<PathType, string> pathDic = new Dictionary<PathType, string>();

    public PathTool()
    {
        InitPathDic();
    }

    void InitPathDic()
    {
        pathDic.Add(PathType.ConfigTable, "Configure/Tabs");
        pathDic.Add(PathType.ConfigClass, "Scripts/Cfg/AutoClass");
    }

    public string GetPathByType(PathType type)
    {
        string result = "";
        if (pathDic.ContainsKey(type))
        {
            pathDic.TryGetValue(type, out result);
        }
        return result;
    }

    public enum PathType
    {
        ConfigTable,ConfigClass
    }

    public void OnRelease()
    {
        pathDic.Clear();
    }
}

