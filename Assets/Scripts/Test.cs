using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NpcCsvData npcCsvData = new NpcCsvData();
        npcCsvData.V_Init();
        Debug.Log(npcCsvData);
    }
    
}
