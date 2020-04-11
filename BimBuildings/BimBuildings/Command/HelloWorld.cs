using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BimBuildings.Util;

namespace BimBuildings.Command
{
    [Transaction(TransactionMode.Manual)]
    class HelloWorld : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var collector = new Collector();

            StringBuilder sb = new StringBuilder();
            foreach(DimensionType a in collector.GetDimensionTypes(doc))
            {
                sb.Append(a.Name + "\n");
            }
            TaskDialog.Show("Title", sb.ToString());

            return Result.Succeeded;
        }
    }
}
