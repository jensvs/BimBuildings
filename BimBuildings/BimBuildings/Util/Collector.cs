using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System.Windows.Forms;

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

        //*********************************************** Roofs ********************************************************************//
        public List<RoofBase> GetRoofs(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Roofs = collector.OfCategory(BuiltInCategory.OST_Roofs).WhereElementIsNotElementType().ToElements();
            List<RoofBase> List_Roofs = new List<RoofBase>();

            foreach (RoofBase w in Roofs)
            { List_Roofs.Add(w); }

            return List_Roofs;
        }

        public List<RoofBase> GetRoofs(Document doc, ElementId viewId)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, viewId);
            ICollection<Element> Roofs = collector.OfCategory(BuiltInCategory.OST_Roofs).WhereElementIsNotElementType().ToElements();
            List<RoofBase> List_Roofs = new List<RoofBase>();

            foreach (RoofBase w in Roofs)
            { List_Roofs.Add(w); }

            return List_Roofs;
        }

        public List<RoofType> GetRoofTypes(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Roofs = collector.OfCategory(BuiltInCategory.OST_Roofs).WhereElementIsElementType().ToElements();
            List<RoofType> List_Roofs = new List<RoofType>();

            foreach (RoofType w in Roofs)
            { List_Roofs.Add(w); }

            return List_Roofs;
        }

        //*********************************************** Dimensions ********************************************************************//
        public List<DimensionType> GetDimensionTypes(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Dimensions = collector.OfClass(typeof(DimensionType)).ToElements();
            List<DimensionType> List_Dimensions = new List<DimensionType>();

            foreach(DimensionType w in Dimensions)
            {
                if(w.FamilyName.Contains("Linear"))
                {
                    List_Dimensions.Add(w);
                }
            }
            return List_Dimensions;
        }

        public DimensionType GetDimensionTypeByName(Document doc, string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Dimensions = collector.OfClass(typeof(DimensionType)).ToElements();
            List<DimensionType> List_Dimensions = new List<DimensionType>();

            foreach (DimensionType w in Dimensions)
            {
                if (w.Name == name && w.FamilyName.Contains("Linear"))
                {
                    List_Dimensions.Add(w);
                    return List_Dimensions[0];
                }
            }
            return List_Dimensions[0];
        }
    }
}