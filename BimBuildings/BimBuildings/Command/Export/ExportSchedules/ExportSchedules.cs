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
using System.IO;

namespace BimBuildings.Command.Export.ExportSchedules
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class ExportSchedules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region//Utils
            //String Builder
            StringBuilder sb = new StringBuilder();

            //Collector
            Collector collector = new Collector();

            //UnitConvertor
            LengthUnitConverter converter = new LengthUnitConverter();

            // Application context.
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            #endregion

            #region//Check if we are in the Revit project , not in family one.
            if (doc.IsFamilyDocument)
            {
                Message.Display("Can't use command in family document", WindowType.Warning);
                return Result.Cancelled;
            }
            #endregion

            #region//create new directory
            string append = "schedules ";
            string pathName = doc.PathName.ToString();
            string title = doc.Title.ToString();
            int titleLength = title.Length + 4;
            string newpath = pathName.Remove(pathName.Length - titleLength, titleLength);

            string folder = newpath + append + DateTime.Now.ToString("yyyMMdd HH'u'mm'm'");
            Directory.CreateDirectory(folder);
            //sb.Append(folder);
            #endregion

            #region//ViewSchedule collector
            List<View> List_ViewSchedules = collector.GetViewSchedules(doc, ViewType.Schedule);
            //sb.Append("\n"+ List_ViewSchedules);
            #endregion

            #region//Export Options
            ViewScheduleExportOptions options = new ViewScheduleExportOptions();
            #endregion

            #region//Export Schedules
            foreach (ViewSchedule V in List_ViewSchedules)
            {
                V.Export(folder, V.Name + ".csv", options);
            }
            #endregion

            //TaskDialog.Show("TEST", sb.ToString());
            return Result.Succeeded;

        }
    }
}
