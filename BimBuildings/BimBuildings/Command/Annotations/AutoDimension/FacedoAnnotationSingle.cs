using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BimBuildings.Command.Annotations.AutoDimension;
using Autodesk.Revit.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BimBuildings.Util;
using Document = Autodesk.Revit.DB.Document;

namespace BimBuildings.Command.Annotations.AutoDimension
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class FacedoAnnotationSingle : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //String Builder
            StringBuilder sb = new StringBuilder();

            //Collector
            Collector collector = new Collector();

            //UnitConvertor
            LengthUnitConverter converter = new LengthUnitConverter();

            // Application context.
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            // Check if we are in the Revit project , not in family one.
            if (doc.IsFamilyDocument)
            {
                Message.Display("Can't use command in family document", WindowType.Warning);
                return Result.Cancelled;
            }

            // Get access to current view.
            var sectionView = uidoc.ActiveView;

            // Check if Dimension can be created in currently active project view.
            bool canCreateDimensionInView = false;
            switch (sectionView.ViewType)
            {
                case ViewType.FloorPlan:
                    canCreateDimensionInView = false;
                    break;
                case ViewType.CeilingPlan:
                    canCreateDimensionInView = false;
                    break;
                case ViewType.Detail:
                    canCreateDimensionInView = false;
                    break;
                case ViewType.Elevation:
                    canCreateDimensionInView = true;
                    break;
                case ViewType.Section:
                    canCreateDimensionInView = true;
                    break;
            }

            // Check if Dimension can be created
            if (!canCreateDimensionInView)
            {
                Message.Display("Dimension can't be created in the current view.", WindowType.Warning);
                return Result.Cancelled;
            }

            //Check if activeView is an Elevation
            if(canCreateDimensionInView)
            {
                // Ask user to select one generic model.
                var selectionReference = uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Generic Models"), "Select one generic model.");
                Element selectionElement = doc.GetElement(selectionReference);
                FamilyInstance genericModelFamily = doc.GetElement(selectionElement.Id) as FamilyInstance;
                FamilySymbol genericModelSymbol = genericModelFamily.Symbol;

                // Checks if selection isn't empty
                if (genericModelFamily == null)
                {
                    Message.Display("You haven't selected a valid element.\nPlease selected another element.", WindowType.Error);
                    return Result.Cancelled;
                }
                #region//Get DimensionType
                DimensionType genericModelDimension = collector.GetLinearDimensionTypeByName(doc, "FAC - Diagonal - 2mm - Black");
                DimensionType nestedFamilyDimension = collector.GetLinearDimensionTypeByName(doc, "FAC - Diagonal - 2mm - Stelkozijn");

                DimensionType nestedFamilySpotElevation = collector.GetSpotElevationTypeByName(doc, "2.0mm - Stelkozijn (b.k) vlp");
                #endregion

                #region//Get directions for dimensions
                XYZ widthDirection = sectionView.RightDirection.Normalize();
                XYZ heigthDirection = new XYZ(0, 0, 1);
                #endregion

                #region//Get nested family 1
                string nestedFamilyName1 = "31_MDK_GM_stelkozijn_lijn";
                FamilyInstance nestedFamily1 = null;

                ICollection<ElementId> subComponentIds1 = genericModelFamily.GetSubComponentIds();
                foreach (ElementId id in subComponentIds1)
                {
                    if (doc.GetElement(id).Name == nestedFamilyName1)
                    {
                        nestedFamily1 = doc.GetElement(id) as FamilyInstance;
                    }
                }

                

                #endregion

                #region//Get nested family 2
                FamilyInstance nestedFamily2 = null;
                string nestedFamilyName2 = "Profiel_1";

                List<double> nestedFamilyOriginZ = new List<double>();
                List<FamilyInstance> nestedFamilies = new List<FamilyInstance>();

                FamilyInstance familyContainer = null;
                LocationCurve curveContainer = null;
                Line lineContainer = null;

                ICollection<ElementId> subComponentIds2 = genericModelFamily.GetSubComponentIds();
                foreach (ElementId id in subComponentIds2)
                {
                    if (doc.GetElement(id).Name == nestedFamilyName2)
                    {
                        nestedFamilies.Add(doc.GetElement(id) as FamilyInstance);
                        familyContainer = doc.GetElement(id) as FamilyInstance;
                        curveContainer = familyContainer.Location as LocationCurve;
                        lineContainer = curveContainer.Curve as Line;
                        nestedFamilyOriginZ.Add(lineContainer.Origin.Z);
                    }
                }

                double minValue = int.MaxValue;
                int minIndex;
                int index = -1;

                foreach (double num in nestedFamilyOriginZ)
                {
                    index++;
                    if (num <= minValue)
                    {
                        minValue = num;
                        minIndex = index;
                    }
                }

                nestedFamily2 = nestedFamilies[index];

                if (nestedFamily2 == null)
                {
                    Message.Display("There isn't a nested family in the element with the specified name.", WindowType.Error);
                    return Result.Cancelled;
                }
                #endregion

                #region//Get type parameters of element
                double MDK_breedte = genericModelSymbol.LookupParameter("MDK_breedte").AsDouble();
                string MDK_merk = genericModelSymbol.LookupParameter("MDK_merk").AsString();
                #endregion

                #region//Get instance parameters of element
                double MDK_offset_vooraanzicht = genericModelFamily.LookupParameter("MDK_offset_vooraanzicht").AsDouble();
                double MDK_hoogte = genericModelFamily.LookupParameter("MDK_hoogte").AsDouble();
                #endregion

                #region//Get count
                List<Element> genericModelList = collector.GetGenericModels(doc);
                List<Element> List_GenericModels = new List<Element>();
                List<Element> List_GenericModelsDraw = new List<Element>();
                List<Element> List_GenericModelsMirror = new List<Element>();
                FamilyInstance familyInstance = null;
                FamilySymbol familySymbol = null;

                foreach (Element genericModel in genericModelList)
                {
                    familyInstance = doc.GetElement(genericModel.Id) as FamilyInstance;
                    if (familyInstance.SuperComponent == null)
                    {
                        familySymbol = familyInstance.Symbol;
                        string mark = familySymbol.LookupParameter("MDK_merk").AsString();

                        if (mark == MDK_merk)
                        {
                            if (familyInstance.Mirrored)
                            {
                                List_GenericModelsMirror.Add(genericModel);
                                List_GenericModels.Add(genericModel);
                            }
                            else
                            {
                                List_GenericModelsDraw.Add(genericModel);
                                List_GenericModels.Add(genericModel);
                            }
                        }
                    }
                }

                int counttotal = List_GenericModels.Count;
                int countdraw = List_GenericModelsDraw.Count;
                int countmirror = List_GenericModelsMirror.Count;

                #endregion

                #region//Get direction of family
                LocationCurve locationCurve = nestedFamily2.Location as LocationCurve;
                Line locationLine = locationCurve.Curve as Line;
                XYZ dir = locationLine.Direction.Normalize();
                #endregion

                #region//Check if generic model is in same direction as view
                /*XYZ genericModelDir = GetReferenceDirection(genericModelFamily.GetReferences(FamilyInstanceReferenceType.CenterFrontBack).First(), doc);

                sb.Append(genericModelDir + "\n" + widthDirection);

                if (genericModelDir.ToString() != widthDirection.ToString())
                {
                    Message.Display("The generic model isn't parallel to the active view.", WindowType.Error);
                    return Result.Cancelled;
                }*/
                #endregion

                #region//Get locationpoint of selected element
                LocationPoint location = genericModelFamily.Location as LocationPoint;
                XYZ locationpoint = location.Point;
                #endregion

                #region//Create endpoints for line creation
                XYZ genericModelHeight = GetDistance(locationpoint, widthDirection, MDK_breedte, 300);
                XYZ genericModelWidth = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht + MDK_hoogte + converter.ConvertToFeet(300));
                XYZ nestedFamilyHeight = GetDistance(locationpoint, widthDirection, MDK_breedte, 150);
                XYZ nestedFamilyHeight2 = GetDistance(locationpoint, heigthDirection, MDK_hoogte - 18, -150);
                XYZ nestedFamilyWidth = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht + MDK_hoogte + converter.ConvertToFeet(150));
                XYZ textNoteLevelOrigin = locationpoint - widthDirection * converter.ConvertToFeet(500) + heigthDirection * converter.ConvertToFeet(200);
                XYZ textNoteHeaderOrigin = locationpoint - heigthDirection * converter.ConvertToFeet(200);
                XYZ textNoteInfoOrigin = locationpoint - heigthDirection * converter.ConvertToFeet(500);
                #endregion

                #region//Create line for dimension
                Line genericModelHeightLine = Line.CreateBound(genericModelHeight, genericModelHeight + heigthDirection * 100);
                Line genericModelWidthLine = Line.CreateBound(genericModelWidth, genericModelWidth + widthDirection * 100);
                Line nestedFamilyHeightLine = Line.CreateBound(nestedFamilyHeight, nestedFamilyHeight + heigthDirection * 100);
                Line nestedFamilyWidthLine = Line.CreateBound(nestedFamilyWidth, nestedFamilyWidth + widthDirection * 100);
                Line levelLine = Line.CreateBound(locationpoint - widthDirection * converter.ConvertToFeet(500), locationpoint + widthDirection * (MDK_breedte + converter.ConvertToFeet(1000)));
                #endregion

                #region// Get references which refer to the reference planes in the family
                ReferenceArray genericModelHeightref = new ReferenceArray();
                ReferenceArray genericModelWidthref = new ReferenceArray();
                ReferenceArray nestedFamilyWidthref = new ReferenceArray();
                ReferenceArray nestedFamilyHeightref = new ReferenceArray();

                //Reference nestedFamilyTop = nestedFamily1.GetReferences(FamilyInstanceReferenceType.Top).First();
                //Reference nestedFamilyBottom = nestedFamily1.GetReferences(FamilyInstanceReferenceType.Bottom).First();

                foreach(Reference reference in genericModelFamily.GetReferences(FamilyInstanceReferenceType.WeakReference))
                {
                    string name = genericModelFamily.GetReferenceName(reference);
                    if(name.Contains("center_tussenregel"))
                    {
                        genericModelHeightref.Append(reference);
                    }

                    if (name.Contains("center_tussenstijl"))
                    {
                        genericModelWidthref.Append(reference);
                    }
                }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Top))
                { genericModelHeightref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Bottom))
                { genericModelHeightref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.StrongReference))
                { genericModelWidthref.Append(e); }

                foreach (var e in nestedFamily1.GetReferences(FamilyInstanceReferenceType.StrongReference))
                { nestedFamilyWidthref.Append(e); }

                foreach (var e in nestedFamily1.GetReferences(FamilyInstanceReferenceType.Top))
                { nestedFamilyHeightref.Append(e); }

                foreach (var e in nestedFamily1.GetReferences(FamilyInstanceReferenceType.Bottom))
                { nestedFamilyHeightref.Append(e); }
                #endregion

                #region//Textnote Options
                TextNoteOptions textNoteLevelOptions = new TextNoteOptions();
                textNoteLevelOptions.HorizontalAlignment = HorizontalTextAlignment.Left;
                textNoteLevelOptions.TypeId = collector.GetTextNoteTypeIdByName(doc, "2.5mm - Black");

                TextNoteOptions textNoteHeaderOptions = new TextNoteOptions();
                textNoteHeaderOptions.HorizontalAlignment = HorizontalTextAlignment.Left;
                textNoteHeaderOptions.TypeId = collector.GetTextNoteTypeIdByName(doc, "3.5mm - Orange");

                TextNoteOptions textNoteInfoOptions = new TextNoteOptions();
                textNoteInfoOptions.HorizontalAlignment = HorizontalTextAlignment.Left;
                textNoteInfoOptions.TypeId = collector.GetTextNoteTypeIdByName(doc, "2.5mm - Black");
                #endregion

                #region//Info
                string info =   "Afmetingen:\t" + converter.ConvertToMetric(MDK_breedte, LengthUnitType.milimeter,0).ToString() + " x " + converter.ConvertToMetric(MDK_hoogte, LengthUnitType.milimeter, 0).ToString() + " mm\n" +
                                "Getekend:\t" + countdraw + "\n" +
                                "Gespiegeld:\t" + countmirror + "\n" +
                                "Totaal aantal:\t" + counttotal + "\n";
                #endregion

                #region//Create Annotations
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Create Annotations");

                    #region//Create and set workplane to place dimensions on
                    Plane plane = Plane.CreateByNormalAndOrigin(sectionView.ViewDirection, sectionView.Origin);
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                    sectionView.SketchPlane = sketchPlane;
                    #endregion

                    #region//Create Level Line
                    DetailCurve levelDetailLine = doc.Create.NewDetailCurve(sectionView, levelLine);
                    #endregion

                    #region//Create Textnotes
                    TextNote textNoteLevel = TextNote.Create(doc, sectionView.Id, textNoteLevelOrigin, "+vlp", textNoteLevelOptions);
                    TextNote textNoteHeader = TextNote.Create(doc, sectionView.Id, textNoteHeaderOrigin, MDK_merk, textNoteHeaderOptions);
                    TextNote textNoteInfo = TextNote.Create(doc, sectionView.Id, textNoteInfoOrigin, info, textNoteInfoOptions);
                    #endregion

                    if (MDK_offset_vooraanzicht > 0)
                    {
                        genericModelHeightref.Append(levelDetailLine.GeometryCurve.Reference);
                    }

                    nestedFamilyHeightref.Append(levelDetailLine.GeometryCurve.Reference);

                    #region//Create Dimensions
                    doc.Create.NewDimension(sectionView, genericModelHeightLine, genericModelHeightref, genericModelDimension);
                    doc.Create.NewDimension(sectionView, genericModelWidthLine, genericModelWidthref, genericModelDimension);
                    doc.Create.NewDimension(sectionView, nestedFamilyHeightLine, nestedFamilyHeightref, nestedFamilyDimension);
                    doc.Create.NewDimension(sectionView, nestedFamilyWidthLine, nestedFamilyWidthref, nestedFamilyDimension);
                    //doc.Create.NewSpotElevation(sectionView, nestedFamilyTop, nestedFamilyHeight, nestedFamilyHeight2, nestedFamilyHeight2, nestedFamilyHeight, true);

                    //TaskDialog.Show("fff", sb.ToString());
                    #endregion

                    tx.Commit();
                }
                #endregion
            }
            return Result.Succeeded;
        }

        public XYZ GetDistance(XYZ locationpoint, XYZ dir, double width, double distance)
        {
            LengthUnitConverter converter = new LengthUnitConverter();

            XYZ point = XYZ.Zero;
            double totaldistance = (width + converter.ConvertToFeet(distance));

            if(dir.X == -1 || dir.X == 1)
            {
                point = new XYZ(locationpoint.X + dir.X * totaldistance,
                        locationpoint.Y,
                        locationpoint.Z);
                return point;
            }
            else if(dir.Y == -1 || dir.Y == 1)
            {
                point = new XYZ(locationpoint.X,
                        locationpoint.Y + dir.Y * totaldistance,
                        locationpoint.Z);
                return point;
            }
            else
            {
                double degrees = Math.Round(Math.Atan2(dir.X, dir.Y) * (180 / Math.PI));
                double radians;
                double x;
                double y;

                if (degrees > 0 && degrees < 90)
                {
                    //+X, +Y
                    radians = degrees * (Math.PI / 180);
                    x = Math.Sin(radians) * totaldistance;
                    y = Math.Cos(radians) * totaldistance;

                    point = new XYZ(locationpoint.X + x, locationpoint.Y + y, locationpoint.Z);
                    return point;
                }
                else if(degrees > 90 && degrees < 180)
                {
                    //+X, -Y
                    degrees = degrees - 90;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Cos(radians) * totaldistance;
                    y = Math.Sin(radians) * totaldistance;

                    point = new XYZ(locationpoint.X + x, locationpoint.Y - y, locationpoint.Z);
                    return point;
                }
                else if(degrees > -90 && degrees < 0)
                {
                    //-X, +Y
                    degrees = degrees + 90;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Cos(radians) * totaldistance;
                    y = Math.Sin(radians) * totaldistance;

                    point = new XYZ(locationpoint.X - x, locationpoint.Y + y, locationpoint.Z);
                    return point;
                }
                else if(degrees > -180 && degrees < -90)
                {
                    //-X, -Y
                    degrees = degrees + 180;
                    radians = degrees * (Math.PI / 180);
                    x = Math.Sin(radians) * totaldistance;
                    y = Math.Cos(radians) * totaldistance;

                    point = new XYZ(locationpoint.X - x, locationpoint.Y - y, locationpoint.Z);
                    return point;
                }
                return point;
            }
        }

        public XYZ GetReferenceDirection(Reference ref1, Document doc)
        // returns the direction perpendicular to reference
        // returns XYZ.Zero on error;
        {
            XYZ res = XYZ.Zero;
            XYZ workPlaneNormal = doc.ActiveView.SketchPlane.GetPlane().Normal;
            if (ref1.ElementId == ElementId.InvalidElementId) return res;
            Element elem = doc.GetElement(ref1.ElementId);
            if (elem == null) return res;
            if (ref1.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_SURFACE || ref1.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_LINEAR)
            {
                // make a dimension to a point for direction

                XYZ bEnd = new XYZ(10, 10, 10);
                ReferenceArray refArr = new ReferenceArray();
                refArr.Append(ref1);
                Dimension dim = null;
                using (Transaction t = new Transaction(doc, "test"))
                {
                    t.Start();
                    using (SubTransaction st = new SubTransaction(doc))
                    {
                        st.Start();
                        ReferencePlane refPlane = doc.Create.NewReferencePlane(XYZ.Zero, bEnd, bEnd.CrossProduct(XYZ.BasisZ).Normalize(), doc.ActiveView);
                        ModelCurve mc = doc.Create.NewModelCurve(Line.CreateBound(XYZ.Zero, new XYZ(10, 10, 10)), SketchPlane.Create(doc, refPlane.Id));
                        refArr.Append(mc.GeometryCurve.GetEndPointReference(0));
                        dim = doc.Create.NewDimension(doc.ActiveView, Line.CreateBound(XYZ.Zero, new XYZ(10, 0, 0)), refArr);
                        ElementTransformUtils.MoveElement(doc, dim.Id, new XYZ(0, 0.1, 0));
                        st.Commit();
                    }
                    if (dim != null)
                    {
                        Curve cv = dim.Curve;
                        cv.MakeBound(0, 1);
                        XYZ pt1 = cv.GetEndPoint(0);
                        XYZ pt2 = cv.GetEndPoint(1);
                        res = pt2.Subtract(pt1).Normalize();
                    }
                    t.RollBack();
                }
            }
            return res;
        }
    }
}
