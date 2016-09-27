using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DataGrid
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            ClientSize = new Size(905, 517);

            var basePanel = new DynamicPanel();
            basePanel.Initialize();
            
            this.Controls.Add(basePanel);

            //basePanel.Size = new Size(881, 227);

        }

        

        // TODO: Unsubscribe mouse handlers if needed
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        
    }
}