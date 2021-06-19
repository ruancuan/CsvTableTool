using System.Data;

/// <summary>
/// 表数据的基类
/// </summary>
public class ConfigTable<T> where T : ConfigTable<T>, new()
{
    public int ID;
    public ConfigTable()
    {

    }
    /// <summary>
    /// 根据 DataRow 各项初始化数据
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public virtual T Init(DataRow row)
    {
        T table = new T();
        return table;
    }
}
