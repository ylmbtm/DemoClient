using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using CFG;

namespace MAP
{
public class MAPWorldMap : MonoBehaviour
{
    [MAPFieldAttri] public int            MapID         = 0;
    [MAPFieldAttri] public string         MapName       = string.Empty;
    [NonSerialized] public Action         MapLoadFinish = null;
    [NonSerialized] public DTMap          MapData       = null;

public void AddGroup<T>() where T :
    MAPContainer
    {
        T g = new GameObject(typeof(T).Name).AddComponent<T>();
        g.transform.parent = transform;
        g.transform.localPosition = Vector3.zero;
        g.transform.localEulerAngles = Vector3.zero;
        g.transform.localScale = Vector3.one;
    }

    public T    GetGroup<T>()
    {
        return GetComponentInChildren<T>();
    }

public void TriggerElement<T>(int id) where T :
    MAPElement
    {
        MAPGroup<T> g = GetGroup<MAPGroup<T>>();
        if (g == null)
        {
            return;
        }
        T element = g.GetElement(id);
        if (element == null)
        {
            return;
        }
        element.Trigger();
    }

public void ReleaseElement<T>(int id) where T :
    MAPElement
    {
        MAPGroup<T> g = GetGroup<MAPGroup<T>>();
        if (g == null)
        {
            return;
        }
        T element = g.GetElement(id);
        if (element == null)
        {
            return;
        }
        element.Release();
    }

    public void EnterWorld(int worldID)
    {
        this.MapID = worldID;
        this.Import();
        if (MapLoadFinish != null)
        {
            MapLoadFinish();
            MapLoadFinish = null;
        }
        this.Enter();
    }

    public void AddGroups()
    {
        AddGroup<MAPGroupBorn>();
        AddGroup<MAPGroupArea>();
        AddGroup<MAPGroupWave>();
        AddGroup<MAPGroupPortal>();
        AddGroup<MAPGroupBarrier>();
        AddGroup<MAPGroupCondition>();
        /*AddGroup<MAPGroupMutiPoint>();
        AddGroup<MAPGroupPath>();
        AddGroup<MAPGroupAction>();*/
    }

public void ExportTo<MapElem, DtElem>(List<DtElem> DtElemList)  where MapElem :
MAPElement where DtElem :
    DTElement
    {
        List<MapElem> list = GetGroup<MAPGroup<MapElem>>().GetElements();
        for (int i = 0; i < list.Count; i++)
        {
            DtElemList.Add(list[i].Export() as DtElem);
        }
    }

public void ImportFrom<MapElem, DtElem>(List<DtElem> DtElemList) where MapElem :
MAPElement  where DtElem :
    DTElement
    {
        MAPGroup<MapElem> group = GetGroup<MAPGroup<MapElem>>();
        if(group == null)
        {
            return;
        }

        for (int i = 0; i < DtElemList.Count; i++)
        {
            MapElem element = group.AddElement();
            element.Import(DtElemList[i]);
        }
    }

    public void ImportFromActions(List<MapAction> cfgList)
    {
        MAPGroupAction group = GetGroup<MAPGroupAction>();
        for (int i = 0; i < cfgList.Count; i++)
        {
            MapAction d = cfgList[i];
            string typeName = d.GetType().Name;
            Type type = Type.GetType(string.Format("MAP.{0}", typeName.Replace("Map", "MAP")));
            group.AddEvent(d.ID, type);
        }

        List<MAPAction> elements = group.GetElements();
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].Import(cfgList[i]);
        }
    }

    public void Export()
    {
        MAPWorldMap worldMap = this;
        DTMap  data     = new DTMap();
        data.MapID          = worldMap.MapID;
        data.MapName        = worldMap.MapName;
        ExportTo<MAPBorn,        DTBorn>(data.MapBorns);             //出生点
        ExportTo<MAPCondition,   DTCondition>(data.MapConditions);   //副本胜负条件
        ExportTo<MAPArea,        DTArea>(data.MapAreas);             //区域
        ExportTo<MAPWave,        DTWave>(data.MapWaves);             //刷怪的波次
        ExportTo<MAPPortal,      DTPortal>(data.MapPortals);
        ExportTo<MAPBarrier,     DTBarrier>(data.MapBarriers);       //空气墙
        /*
        ExportDCFGByElement<MAPPath,          MapPath>(       data.MapPaths        );
        ExportDCFGByElement<MAPAction,        MapAction>(     data.MapActions      );
        */
        string fileName = string.Format("{0}/Resources/Text/Map/{1}.xml", Application.dataPath, data.MapID);
        data.Save(fileName);
    }

    public void Import()
    {
        this.transform.DestroyChildren();
        this.AddGroups();
        string fileName = string.Format("Text/Map/{0}.xml", this.MapID);
        DTMap data  = new DTMap();
        data.Load(fileName);
        this.MapID      = data.MapID;
        this.MapName    = data.MapName;

        ImportFrom<MAPBorn,        DTBorn>     (data.MapBorns);
        ImportFrom<MAPCondition,   DTCondition>(data.MapConditions);
        ImportFrom<MAPArea,        DTArea>     (data.MapAreas);
        ImportFrom<MAPWave,        DTWave>     (data.MapWaves);
        ImportFrom<MAPPortal,      DTPortal>   (data.MapPortals);
        ImportFrom<MAPBarrier,     DTBarrier>  (data.MapBarriers);
        /*
        ImportDCFGByElement<MAPPath,          MapPath>(       data.MapPaths        );
        ImportDCFGByActions(data.MapActions);*/
        MapData = data;
    }

    public void Enter()
    {
    }

}
}