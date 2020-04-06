using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

namespace BimBuildings.Util
{
    class Collector
    {
        //*********************************************** Walls ********************************************************************//
        public List<Wall> GetWalls(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Walls = collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements();
            List<Wall> List_Walls = new List<Wall>();

            foreach (Wall w in Walls)
            {List_Walls.Add(w);}

            return List_Walls;
        }

        public List<Wall> GetWalls(Document doc, ElementId viewId)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, viewId);
            ICollection<Element> Walls = collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements();
            List<Wall> List_Walls = new List<Wall>();

            foreach (Wall w in Walls)
            { List_Walls.Add(w); }

            return List_Walls;
        }

        public List<WallType> GetWallTypes(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Walls = collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsElementType().ToElements();
            List<WallType> List_Walls = new List<WallType>();

            foreach (WallType w in Walls)
            { List_Walls.Add(w); }

            return List_Walls;
        }

        //*********************************************** Floors ********************************************************************//
        public List<Floor> GetFloors(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Floors = collector.OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType().ToElements();
            List<Floor> List_Floors = new List<Floor>();

            foreach (Floor w in Floors)
            { List_Floors.Add(w); }

            return List_Floors;
        }

        public List<Floor> GetFloors(Document doc, ElementId viewId)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, viewId);
            ICollection<Element> Floors = collector.OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType().ToElements();
            List<Floor> List_Floors = new List<Floor>();

            foreach (Floor w in Floors)
            { List_Floors.Add(w); }

            return List_Floors;
        }

        public List<FloorType> GetFloorTypes(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Floors = collector.OfCategory(BuiltInCategory.OST_Floors).WhereElementIsElementType().ToElements();
            List<FloorType> List_Floors = new List<FloorType>();

            foreach (FloorType w in Floors)
            { List_Floors.Add(w); }

            return List_Floors;
        }
    }
}