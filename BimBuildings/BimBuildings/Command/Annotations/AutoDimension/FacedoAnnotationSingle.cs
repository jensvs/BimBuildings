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
                DimensionType genericModelDimensionOrdinate = collector.GetLinearDimensionTypeByName(doc, "FAC - Ordinate - 2mm - Krukhoogte");
                DimensionType nestedFamilyDimensionOrdinate = collector.GetLinearDimensionTypeByName(doc, "FAC - Ordinate - 2mm - Stelkozijn");
                #endregion

                #region//Get directions for dimensions
                XYZ widthDirection = sectionView.RightDirection.Normalize();
                XYZ heigthDirection = new XYZ(0, 0, 1);
                #endregion

                #region//Get nested family !!!MDK WORDT NOG VERVANGEN DOOR FAC KAN ERROR VEROORZAKEN
                string nestedFamilyName = "31_MDK_GM_stelkozijn_lijn";
                FamilyInstance nestedFamily = null;

                ICollection<ElementId> subComponentIds1 = genericModelFamily.GetSubComponentIds();
                foreach (ElementId id in subComponentIds1)
                {
                    if (doc.GetElement(id).Name == nestedFamilyName)
                    {
                        nestedFamily = doc.GetElement(id) as FamilyInstance;
                    }
                }
                #endregion

                #region//Get nested family
                nestedFamilyName = "31_FAC_GM_vak_vleugel";
                List<FamilyInstance> nestedFamilyList = new List<FamilyInstance>();

                ICollection<ElementId> subComponentIds2 = genericModelFamily.GetSubComponentIds();
                foreach (ElementId id in subComponentIds2)
                {
                    FamilyInstance fInstance = doc.GetElement(id) as FamilyInstance;
                    FamilySymbol fSymbol = fInstance.Symbol;

                    if(fSymbol.FamilyName == nestedFamilyName)
                    {
                        nestedFamilyList.Add(doc.GetElement(id) as FamilyInstance);
                    }
                }

                FamilyInstance nestedFamily2 = nestedFamilyList.First();
                #endregion

                #region//Get type parameters of element
                double MDK_offset_vooraanzicht = genericModelSymbol.LookupParameter("MDK_offset_vooraanzicht").AsDouble();
                double MDK_hoogte = genericModelSymbol.LookupParameter("MDK_hoogte").AsDouble();
                double MDK_breedte = genericModelSymbol.LookupParameter("MDK_breedte").AsDouble();
                string MDK_merk = genericModelSymbol.LookupParameter("MDK_merk").AsString();
                #endregion

                #region//Check if generic model is in same direction as view
                XYZ genericModelDir = GetReferenceDirection(genericModelFamily.GetReferences(FamilyInstanceReferenceType.CenterFrontBack).First(), doc);

                if (genericModelDir.ToString() != widthDirection.ToString())
                {
                    Message.Display("The generic model isn't parallel to the active view.", WindowType.Error);
                    return Result.Cancelled;
                }
                #endregion

                #region//Get locationpoint of selected element
                LocationPoint location = genericModelFamily.Location as LocationPoint;
                XYZ locationpoint = location.Point;
                #endregion

                #region//Create endpoints for line creation
                XYZ genericModelHeight = GetDistance(locationpoint, widthDirection, MDK_breedte, 150);
                XYZ genericModelWidth = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht - converter.ConvertToFeet(150));
                XYZ genericModelHeight2 = GetDistance(locationpoint, widthDirection, MDK_breedte, 300);
                XYZ genericModelHeight3 = GetDistance(locationpoint, widthDirection, MDK_breedte, 450);
                XYZ genericModelWidth2 = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht - converter.ConvertToFeet(300));
                XYZ nestedFamilyHeight = GetDistance(locationpoint, widthDirection, 0, -150);
                XYZ nestedFamilyWidth = new XYZ(locationpoint.X, locationpoint.Y, locationpoint.Z + MDK_offset_vooraanzicht + MDK_hoogte + converter.ConvertToFeet(150));
                #endregion

                #region//Create line for dimension
                Line genericModelHeightLine = Line.CreateBound(genericModelHeight, genericModelHeight + heigthDirection * 100);
                Line genericModelWidthLine = Line.CreateBound(genericModelWidth, genericModelWidth + widthDirection * 100);
                Line genericModelHeightLine2 = Line.CreateBound(genericModelHeight2, genericModelHeight2 + heigthDirection * 100);
                Line genericModelHeightLine3 = Line.CreateBound(genericModelHeight3, genericModelHeight3 + heigthDirection * 100);
                Line genericModelWidthLine2 = Line.CreateBound(genericModelWidth2, genericModelWidth2 + widthDirection * 100);
                Line nestedFamilyHeightLine = Line.CreateBound(nestedFamilyHeight, nestedFamilyHeight + heigthDirection * 100);
                Line nestedFamilyWidthLine = Line.CreateBound(nestedFamilyWidth, nestedFamilyWidth + widthDirection * 100);
                #endregion

                #region// Get references which refer to the reference planes in the family
                ReferenceArray genericModelHeightref = new ReferenceArray();
                ReferenceArray genericModelHeight2ref = new ReferenceArray();
                ReferenceArray genericModelHeight3ref = new ReferenceArray();
                ReferenceArray genericModelWidthref = new ReferenceArray();
                ReferenceArray genericModelWidth2ref = new ReferenceArray();
                ReferenceArray nestedFamilyWidthref = new ReferenceArray();
                ReferenceArray nestedFamilyHeightref = new ReferenceArray();

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

                foreach (Reference reference in nestedFamily2.GetReferences(FamilyInstanceReferenceType.WeakReference))
                {
                    string name = nestedFamily2.GetReferenceName(reference);
                    if (name.Contains("krukhoogte_binnen"))
                    {
                        genericModelHeight3ref.Append(reference);
                    }
                }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Top))
                { genericModelHeightref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Bottom))
                { genericModelHeightref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Left))
                { genericModelWidthref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Right))
                { genericModelWidthref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Top))
                { genericModelHeight2ref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Bottom))
                { genericModelHeight2ref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Left))
                { genericModelWidth2ref.Append(e); }

                foreach (var e in genericModelFamily.GetReferences(FamilyInstanceReferenceType.Right))
                { genericModelWidth2ref.Append(e); }

                foreach (var e in nestedFamily.GetReferences(FamilyInstanceReferenceType.Left))
                { nestedFamilyWidthref.Append(e); }

                foreach (var e in nestedFamily.GetReferences(FamilyInstanceReferenceType.Right))
                { nestedFamilyWidthref.Append(e); }

                foreach (var e in nestedFamily.GetReferences(FamilyInstanceReferenceType.Top))
                { nestedFamilyHeightref.Append(e); }

                foreach (var e in nestedFamily.GetReferences(FamilyInstanceReferenceType.Bottom))
                { nestedFamilyHeightref.Append(e); }
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

                    #region//Create Dimensions
                    doc.Create.NewDimension(sectionView, genericModelHeightLine, genericModelHeightref, genericModelDimension);
                    doc.Create.NewDimension(sectionView, genericModelWidthLine, genericModelWidthref, genericModelDimension);
                    doc.Create.NewDimension(sectionView, nestedFamilyHeightLine, nestedFamilyHeightref, nestedFamilyDimension);
                    doc.Create.NewDimension(sectionView, nestedFamilyWidthLine, nestedFamilyWidthref, nestedFamilyDimension);

                    if(genericModelHeightref.Size != 2)
                    {
                        doc.Create.NewDimension(sectionView, genericModelHeightLine2, genericModelHeight2ref, genericModelDimension);
                    }

                    if(genericModelWidthref.Size !=2)
                    {
                        doc.Create.NewDimension(sectionView, genericModelWidthLine2, genericModelWidth2ref, genericModelDimension);
                    }

                    #endregion

                    TaskDialog.Show("fff", sb.ToString());

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
                    FailureHandlingOptions failureHandlingOptions = t.GetFailureHandlingOptions();
                    FailureHandler failureHandler = new FailureHandler();
                    failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                    failureHandlingOptions.SetClearAfterRollback(true);
                    t.SetFailureHandlingOptions(failureHandlingOptions);

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
