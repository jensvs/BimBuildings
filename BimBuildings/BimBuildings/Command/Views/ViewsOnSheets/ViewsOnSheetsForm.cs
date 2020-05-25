namespace BimBuildings
{
    using System;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using BimBuildings.Util;

    using Autodesk.Revit.UI;
    using Autodesk.Revit.DB;


    public partial class ViewsOnSheetsForm : System.Windows.Forms.Form
    {
        public System.Drawing.Point mouseLocation;

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLocation = new System.Drawing.Point(e.X, e.Y);
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                System.Drawing.Point mousePose = System.Windows.Forms.Control.MousePosition;
                mousePose.Offset(mouseLocation.X, mouseLocation.Y);
                Location = mousePose;
            }
        }

        #region private members
        private UIDocument uidoc = null;

        #endregion

        #region constructor


        public ViewsOnSheetsForm(UIDocument uIDocument)
        {
            InitializeComponent();
            uidoc = uIDocument;
        }

        #endregion

        #region events
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

        private void ViewsOnSheetsForm_Load(object sender, System.EventArgs e)
        {
            PopulateTitleBlockList();
        }

        #endregion

        #region private methods

        private void PopulateTitleBlockList()
        {
            var doc = uidoc.Document;
            Collector collector = new Collector();

            List<ViewsOnSheetsData> titleBlocks = new List<ViewsOnSheetsData>();
            List<Element> elements = collector.GetTitleBlocks(doc);

            

            foreach (Element item in elements)
            {



                string familyName = item.

                titleBlocks.Add(new ViewsOnSheetsData() { element = item, familyandtypename = item.Name + ":" + fadf}) ;


            }
            #endregion

            cmbTitleBlock.DataSource = titleBlocks;
            cmbTitleBlock.DisplayMember = "element";
            cmbTitleBlock.ValueMember = "element";

        }

        public void cmbTitleBlock_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var doc = uidoc.Document;
            Element titleblock = doc.GetElement(((KeyValuePair<string, ElementId>)cmbTitleBlock.SelectedItem).Value);

            MessageBox.Show("test123", titleblock.Name);
        }


    }
}
