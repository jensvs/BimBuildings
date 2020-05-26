using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimBuildings.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BimBuildings
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class ViewsOnSheets : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region//Utils
            StringBuilder sb = new StringBuilder();
            Collector collector = new Collector();
            LengthUnitConverter converter = new LengthUnitConverter();
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            #endregion

            #region//Check if current project is family
            if (doc.IsFamilyDocument)
            {
                Message.Display("Can't use command in family document", WindowType.Warning);
                return Result.Cancelled;
            }
            #endregion

            #region//select titleblock

            #endregion


            //TaskDialog.Show("test", sb.ToString());


            #region//Create new sheet
            //ViewSheet vs = ViewSheet.Create(doc, item.id)  

            #endregion

            return Result.Succeeded;

        }

    }
}
