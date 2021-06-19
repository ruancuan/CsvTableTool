/*Auto Create, Don't Edit !!!*/

using System;
using System.Data;

[Serializable]
public class NpcCsvItem : ConfigTable<NpcCsvItem>
{
	public string IconPath;
	public int TaskId;
	public string NpcName;
	public override NpcCsvItem Init(DataRow row)
	{
		NpcCsvItem info = new NpcCsvItem();
		info.ID =int.Parse(row[0].ToString());
		info.IconPath =row[1].ToString();
		info.TaskId =int.Parse(row[2].ToString());
		info.NpcName =row[3].ToString();
		return info;
	}
}

public class NpcCsvData :  ConfigBase< NpcCsvData,NpcCsvItem >
{
	public NpcCsvData()
	{
		m_path="Npc.csv";
	}
}


