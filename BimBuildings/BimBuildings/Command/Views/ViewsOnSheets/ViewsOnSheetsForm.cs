using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimBuildings.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace BimBuildings
{
    public partial class ViewsOnSheetsForm : System.Windows.Forms.Form
    {
        #region//general form settings
        public System.Drawing.Point mouseLocation;

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLocation = new System.Drawing.Point(e.X, e.Y);
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                System.Drawing.Point mousePose = System.Windows.Forms.Control.MousePosition;
                mousePose.Offset(mouseLocation.X, mouseLocation.Y);
                Location = mousePose;
            }
        }
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
        #endregion

        #region//Utils
        StringBuilder sb = new StringBuilder();
        Collector collector = new Collector();
        LengthUnitConverter converter = new LengthUnitConverter();
        public UIDocument uidoc = null;
        var doc = uidoc.Document;
        #endregion


        public ViewsOnSheetsForm(UIDocument uIDocument)
        {
            InitializeComponent();
            uidoc = uIDocument;
        }

        private void ViewsOnSheetsForm_Load(object sender, System.EventArgs e)
        {
            PopulateTitleBlockList();
        }

        private void PopulateTitleBlockList()
        {
            var doc = uidoc.Document;
            Collector collector = new Collector();

            List<ViewsOnSheetsData> titleBlocks = new List<ViewsOnSheetsData>();
            List<Element> elements = collector.GetTitleBlocks(doc);



            foreach (Element item in elements)
            {
                ElementType itemtype = item as ElementType;


                titleBlocks.Add(new ViewsOnSheetsData() { element = item, familyandtypename = itemtype.FamilyName + ":" + item.Name });

            }
#endregion

            cmbTitleBlock.DataSource = titleBlocks;
            cmbTitleBlock.DisplayMember = "familyandtypename";
            cmbTitleBlock.ValueMember = "element";
            cmbTitleBlock. = "element";
        }

        public void cmbTitleBlock_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var doc = uidoc.Document;
            Element titleblock = Element;

            MessageBox.Show("test123", titleblock.Name);
        }
    }
}
