using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System.Linq;

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

        public DimensionType GetLinearDimensionTypeByName(Document doc, string name)
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

        public DimensionType GetSpotElevationTypeByName(Document doc, string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Dimensions = collector.OfClass(typeof(DimensionType)).ToElements();
            List<DimensionType> List_Dimensions = new List<DimensionType>();

            foreach (DimensionType w in Dimensions)
            {
                if (w.Name == name && w.FamilyName.Contains("Spot"))
                {
                    List_Dimensions.Add(w);
                    return List_Dimensions[0];
                }
            }
            return List_Dimensions[0];
        }

        //*********************************************** Views ********************************************************************//
        public List<View> GetViews(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Views = collector.OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            List<View> List_ViewsTemplates = new List<View>();
            List<View> List_Views = new List<View>();

            foreach(Element w in Views)
            {
                List_ViewsTemplates.Add((View)w);
            }

            foreach(View w in List_ViewsTemplates)
            {
                if(!w.IsTemplate)
                {
                    List_Views.Add(w);
                }
            }

            return List_Views;
        }

        public List<View> GetViews(Document doc, ViewType viewType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Views = collector.OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            List<View> List_ViewsTemplates = new List<View>();
            List<View> List_Views = new List<View>();

            foreach (Element w in Views)
            {
                List_ViewsTemplates.Add((View)w);
            }

            foreach (View w in List_ViewsTemplates)
            {
                if (!w.IsTemplate && w.ViewType == viewType)
                {
                    List_Views.Add(w);
                }
            }

            return List_Views;
        }

        //*********************************************** ViewSchedules ********************************************************************//
        public List<View> GetViewSchedules(Document doc, ViewType viewType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Views = collector.OfClass(typeof(ViewSchedule)).WhereElementIsNotElementType().ToElements();
            List<View> List_ViewSchedules = new List<View>();



            foreach (View w in Views)
            {
                if (!w.IsTemplate && w.ViewType == viewType)
                {
                    List_ViewSchedules.Add(w);
                }
            }

            return List_ViewSchedules;
        }

        //*********************************************** ViewsTemplates ********************************************************************//
        public List<View> GetViewTemplates(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Views = collector.OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            List<View> List_ViewsTemplates = new List<View>();
            List<View> List_Views = new List<View>();

            foreach (Element w in Views)
            {
                List_ViewsTemplates.Add((View)w);
            }

            foreach (View w in List_ViewsTemplates)
            {
                if (w.IsTemplate)
                {
                    List_Views.Add(w);
                }
            }
            return List_Views;
        }

        public List<View> GetViewTemplates(Document doc, ViewType viewType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Views = collector.OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            List<View> List_ViewsTemplates = new List<View>();
            List<View> List_Views = new List<View>();

            foreach (Element w in Views)
            {
                List_ViewsTemplates.Add((View)w);
            }

            foreach (View w in List_ViewsTemplates)
            {
                if (w.IsTemplate && w.ViewType == viewType)
                {
                    List_Views.Add(w);
                }
            }

            return List_Views;
        }

        //*********************************************** ViewFamilyTypes ********************************************************************//
        public List<ViewFamilyType> GetViewFamilyType(Document doc, ViewFamily viewFamily)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> viewFamilyTypes = collector.OfClass(typeof(ViewFamilyType)).ToElements();
            List<ViewFamilyType> List_viewFamilyTypes = new List<ViewFamilyType>();

            foreach(ViewFamilyType w in viewFamilyTypes)
            {
                if(w.ViewFamily == viewFamily)
                {
                    List_viewFamilyTypes.Add(w);
                }
            }

            return List_viewFamilyTypes;
        }

        //*********************************************** Generic Models ********************************************************************//
        public List<Element> GetGenericModels(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> GenericModels = collector.OfCategory(BuiltInCategory.OST_GenericModel).WhereElementIsNotElementType().ToElements();
            List<Element> List_GenericModels = new List<Element>();

            foreach (Element w in GenericModels)
            { List_GenericModels.Add(w); }

            return List_GenericModels;
        }

        public List<Element> GetGenericModels(Document doc, ElementId viewId)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, viewId);
            ICollection<Element> GenericModels = collector.OfCategory(BuiltInCategory.OST_GenericModel).WhereElementIsNotElementType().ToElements();
            List<Element> List_GenericModels = new List<Element>();

            foreach (Element w in GenericModels)
            { List_GenericModels.Add(w); }

            return List_GenericModels;
        }

        //*********************************************** TextnoteType ********************************************************************//
        public ElementId GetTextNoteTypeIdByName(Document doc, string name)
        {
            ElementId textId = null;

            // Collect all text note types
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<ElementId> textNoteTypes
                = collector.OfClass(typeof(TextNoteType))
                  .ToElementIds()
                  .ToList();

            // Get text note types as elements
            List<Element> elements = new List<Element>();
            foreach (ElementId noteId in textNoteTypes)
            {
                elements.Add(doc.GetElement(noteId));
            }

            // Get the text note type element ID by name
            foreach (Element e in elements)
            {
                if (e.Name == name)
                    textId = e.Id;
            }

            return textId;
        }

        //*********************************************** ModelLines ********************************************************************//
        public List<Element> GetModelLines(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Lines = collector.OfCategory(BuiltInCategory.OST_Lines).WhereElementIsNotElementType().ToElements();
            List<Element> List_Lines = new List<Element>();

            foreach (Element w in Lines)
            { List_Lines.Add(w); }

            return List_Lines;
        }

        public List<Element> GetModelLines(Document doc, ElementId viewId)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, viewId);
            ICollection<Element> Lines = collector.OfCategory(BuiltInCategory.OST_Lines).WhereElementIsNotElementType().ToElements();
            List<Element> List_Lines = new List<Element>();

            foreach (Element w in Lines)
            { List_Lines.Add(w); }

            return List_Lines;
        }

        //*********************************************** DetailLines ********************************************************************//
        public List<Element> GetDetailLines(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Lines = collector.OfClass(typeof(CurveElement)).WhereElementIsNotElementType().ToElements();
            List<Element> List_Lines = new List<Element>();

            foreach (Element w in Lines)
            {
                DetailLine dl = w as DetailLine;
                if (dl == null) continue;

                List_Lines.Add(w); 
            }

            return List_Lines;
        }

        public List<DetailLine> GetDetailLines(Document doc, ElementId viewId)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, viewId);
            ICollection<Element> Lines = collector.OfClass(typeof(CurveElement)).WhereElementIsNotElementType().ToElements();
            List<DetailLine> List_Lines = new List<DetailLine>();

            foreach (Element w in Lines)
            {
                DetailLine dl = w as DetailLine;
                if (dl == null) continue;

                List_Lines.Add(dl);
            }

            return List_Lines;
        }

        //*********************************************** SelectionFilterElement ********************************************************************//
        public List<SelectionFilterElement> GetSelectionFilter(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> Filters = collector.OfClass(typeof(SelectionFilterElement)).ToElements();
            List<SelectionFilterElement> List_Filters = new List<SelectionFilterElement>();

            foreach (SelectionFilterElement w in Filters)
            { List_Filters.Add(w); }

            return List_Filters;
        }

        //*********************************************** Title blocks ********************************************************************//
        public List<Element> GetTitleBlocks(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> TitleBlocks = collector.OfCategory(BuiltInCategory.OST_TitleBlocks).WhereElementIsElementType().ToElements();
            List<Element> List_TitleBlocks = new List<Element>();

            foreach (Element w in TitleBlocks)
            { List_TitleBlocks.Add(w); }

            return List_TitleBlocks;

        }
    }
}