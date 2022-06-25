using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Collections.Generic;

namespace CFG
{
    public class DTMap : DCFG
    {
        public int                  MapID;
        public string               MapName         = string.Empty;
        public List<DTBorn>         MapBorns        = new List<DTBorn>();
        public List<DTCondition>    MapConditions   = new List<DTCondition>();
        public List<DTArea>         MapAreas        = new List<DTArea>();
        public List<DTWave>         MapWaves        = new List<DTWave>();
        public List<DTPortal>       MapPortals      = new List<DTPortal>();
        public List<DTBarrier>      MapBarriers     = new List<DTBarrier>();

        //public List<MapPath>          MapPaths        = new List<MapPath>();
        // public List<MapAction>       MapActions      = new List<MapAction>();

        public override void Read(XmlElement os)
        {
            this.MapID           = os.GetInt32("MapID");
            this.MapName         = os.GetString("MapName");
            foreach (var current in GetChilds(os))
            {
                switch (current.Name)
                {
                    case "MapBorns":
                        this.MapBorns = ReadList<DTBorn>(current);
                        break;
                    case "MapAreas":
                        this.MapAreas        = ReadList<DTArea>(current);
                        break;
                    case "MapWaves":
                        {
                            List<XmlElement> childs = GetChilds(current);
                            for (int i = 0; i < childs.Count; i++)
                            {
                                XmlElement c = childs[i];
                                Type type = Type.GetType(string.Format("CFG.{0}", c.Name));
                                DTWave data = (DTWave)System.Activator.CreateInstance(type);
                                data.Read(c);
                                MapWaves.Add(data);
                            }
                        }
                        break;
                    case "MapPortals":
                        this.MapPortals = ReadList<DTPortal>(current);
                        break;
                    case "MapBarriers":
                        this.MapBarriers = ReadList<DTBarrier>(current);
                        break;
                    case "MapConditions":
                        this.MapConditions = ReadList<DTCondition>(current);
                        break;
                        /*
                    case "MapPaths":
                        this.MapPaths        = ReadList<MapPath>(current);
                        break;
                    case "MapActions":
                        {
                            List<XmlElement> childs = GetChilds(current);
                            for (int i = 0; i < childs.Count; i++)
                            {
                                XmlElement  c    = childs[i];
                                Type        type = Type.GetType(string.Format("CFG.{0}", c.Name));
                                MapAction   data = (MapAction)System.Activator.CreateInstance(type);
                                data.Read(c);
                                MapActions.Add(data);
                            }
                        }
                        break;*/
                }
            }
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "MapID",             MapID);
            DCFG.Write(doc, os, "MapName",           MapName);
            DCFG.Write(doc, os, "MapBorns",          MapBorns);
            DCFG.Write(doc, os, "MapAreas",          MapAreas);
            DCFG.Write(doc, os, "MapWaves",          MapWaves);
            DCFG.Write(doc, os, "MapPortals",        MapPortals);
            DCFG.Write(doc, os, "MapBarriers",       MapBarriers);
            DCFG.Write(doc, os, "MapConditions",     MapConditions);
            /*DCFG.Write(doc, os, "MapAreaMonsters",   MapAreaMonsters);
            DCFG.Write(doc, os, "MapMutiPoints",     MapMutiPoints);
            DCFG.Write(doc, os, "MapPaths",          MapPaths);
            
            DCFG.Write(doc, os, "MapActions",        MapActions);*/
        }
    }
}