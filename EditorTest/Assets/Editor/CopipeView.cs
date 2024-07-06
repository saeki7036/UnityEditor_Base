using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;
[InitializeOnLoad]
public class CopipeView : EditorWindow
{
    [MenuItem("Editor/OriginalEditerPanels/CopipeView")]
    public static void ShowWindow()
    {
        GetWindow<CopipeView>("CopipeView");
    }
  
    private GameObject currentObj;
    private bool sameCheck;
    private Vector3Int quantity;
    private Vector3 spacing, basePosition;
    
    private void OnGUI()
    {
        using (new EditorGUILayout.VerticalScope())
        {
           
            currentObj = EditorGUILayout.ObjectField("GameObject", currentObj, typeof(GameObject), true) as GameObject;
            sameCheck = EditorGUILayout.Toggle("その場所にも配置する",sameCheck);
            spacing = EditorGUILayout.Vector3Field("配置間隔", spacing);
            basePosition = EditorGUILayout.Vector3Field("配置基準", basePosition);
            quantity = EditorGUILayout.Vector3IntField("配置個数", quantity);

            for(int i = 0; i < 3; i++)
            {
                if(quantity[i] < 0)
                    quantity[i] = 0;
            }

            if (GUILayout.Button("生成"))
            {
                if(currentObj != null)
                SetObject(sameCheck,quantity, spacing, basePosition, currentObj);
            }
        }
    }
    
    private void SetObject(bool Check, Vector3Int Quant,Vector3 Spac, Vector3 BasePos,GameObject Current)
    {
        if (Quant == Vector3Int.zero) return;

        List <Vector3> list = new List<Vector3>();

        for (int X = Quant.x; X >= 0; X--)
        {
            if (TooManySkip(X, Quant.x))
                continue;
            for (int Y = Quant.y; Y >= 0; Y--)
            {
                if (TooManySkip(Y, Quant.y))
                    continue;
                for (int Z = Quant.z; Z >= 0; Z--)
                {
                    if (TooManySkip(Z, Quant.z))
                        continue;
                    Vector3 CreatePosition =  new Vector3
                        (BasePos.x + Spac.x * X, BasePos.y + Spac.y * Y, BasePos.z + Spac.z * Z);

                    if(!list.Contains(CreatePosition))
                        list.Add(CreatePosition);
                }
            }
        }
       
        if (!Check) list.Remove(BasePos);

        Create(Current, list);

        Debug.Log(list.Count+ "個生成しました。");
    }
    private bool TooManySkip(int current,int limit) 
    { 
        return current == limit && current > 0; 
    }
    private void Create(GameObject Current, List<Vector3> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            Instantiate(Current, list[i], Quaternion.identity);
        }
    }
}
