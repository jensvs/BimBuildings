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
        public enum Categories
        {
            walls = BuiltInCategory.OST_Walls,
            floors = BuiltInCategory.OST_Floors
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var collector = new Collector();

            StringBuilder sb = new StringBuilder();
            foreach (var element in collector.GetElements(commandData, Categories.walls))
            {
                sb.Append(element + "\n");
            }
            TaskDialog.Show("Title", sb.ToString());

            

            return Result.Succeeded;
        }
    }
}
