using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

/// <summary>
/// 配置表的管理类基类
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="F"></typeparam>
public class ConfigBase<T, F>
    where F : ConfigTable<F>, new()
    where T : ConfigBase<T, F>
{
    public string m_path = "";
    
    protected Dictionary<int, F> m_Dict = new Dictionary<int, F>();
    public Dictionary<int,F> V_Dict
    {
        get
        {
            if (m_Dict.Count == 0)
            {
                V_Init();
            }
            return m_Dict;
        }
    }

    public void V_Init() 
    {
        string str = StringTool.Instance.PathSplicing(Application.dataPath, PathTool.Instance.GetPathByType(PathTool.PathType.ConfigTable));
        m_path = StringTool.Instance.PathSplicing(str, m_path);
        DataTable data = FileTool.ReadCsv(m_path);
        for (int i = 0; i < data.Rows.Count; i++)
        {
            F temp = new F();
            temp = temp.Init(data.Rows[i]);
            m_Dict.Add(temp.ID, temp);
        }
    }

    public void V_Release()
    {
        this.m_Dict.Clear();
    }
}
