using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataGrid
{
    public partial class Form1 : Form
    {
        private Rectangle dragBoxFromMouseDown;
        private object valueFromMouseDown;

        public Form1()
        {
            InitializeComponent();

            var source = new BindingList<Data>
            {
                new Data {Label = "Text 1", Color = "Red", Symbol = "DAL545"},
                new Data {Label = "Text 2", Color = "Blue", Symbol = "DAL000"},
            };

            dataGridView1.MouseMove += DataGridView1_MouseMove;
            dataGridView1.MouseDown += DataGridView1_MouseDown;
            dataGridView1.DragOver += DataGridView1_DragOver;
            dataGridView1.DragEnter += DataGridView1_DragEnter;
            dataGridView1.DragDrop += DataGridView1_DragDrop;

            dataGridView1.Columns[0].Name = "Label";
            dataGridView1.Columns[1].Name = "Value";

            string[] row1 = new string[] { "Text 1", "Val 1" };
            string[] row2 = new string[] { "Text 2", "Val 2" };
            string[] row3 = new string[] { "Text 3", "Val 3" };
            string[] row4 = new string[] { "Text 4", "Val 4" };
            dataGridView1.Rows.Add(row1);
            dataGridView1.Rows.Add(row2);
            dataGridView1.Rows.Add(row3);
            dataGridView1.Rows.Add(row4);

            //dataGridView1.GiveFeedback += Form1_GiveFeedback;

            //dataGridView1.DataSource = source;
        }

        private void Form1_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            SetDragCursor();
        }

        private void DataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void DataGridView1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    // Proceed with the drag and drop, passing in the list item.                    
                    DragDropEffects dropEffect = dataGridView1.DoDragDrop(valueFromMouseDown, DragDropEffects.Copy | DragDropEffects.Move);
                    
                }
            }

            if(valueFromMouseDown != null)
                SetDragCursor();
        }

        private void DataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            var hittestInfo = dataGridView1.HitTest(e.X, e.Y);
            if (hittestInfo.RowIndex != -1 && hittestInfo.ColumnIndex != -1)
            {
                valueFromMouseDown = dataGridView1.Rows[hittestInfo.RowIndex];
                if (valueFromMouseDown != null)
                {
                    // Remember the point where the mouse down occurred. 
                    // The DragSize indicates the size that the mouse can move 
                    // before a drag event should be started.                
                    Size dragSize = SystemInformation.DragSize;

                    // Create a rectangle using the DragSize, with the mouse position being
                    // at the center of the rectangle.
                    dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
                    


                }
            }
            else
                // Reset the rectangle if the mouse is not over an item in the ListBox.
                dragBoxFromMouseDown = Rectangle.Empty;
        }

        private void SetDragCursor()
        {
            Bitmap bmp = new Bitmap(dataGridView1.Width, (valueFromMouseDown as DataGridViewRow).Height);
            dataGridView1.DrawToBitmap(bmp, new Rectangle(Point.Empty, bmp.Size));
            //optionally define a transparent color
            //bmp.MakeTransparent(System.Drawing.Color.White);

            Cursor cur = new Cursor(bmp.GetHicon());
            Cursor.Current = cur;
        }

        private void DataGridView1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void DataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            // The mouse locations are relative to the screen, so they must be 
            // converted to client coordinates.
            Point clientPoint = dataGridView1.PointToClient(new Point(e.X, e.Y));

            // If the drag operation was a copy then add the row to the other control.
            if (e.Effect == DragDropEffects.Copy)
            {
                var rowValue = e.Data.GetData(typeof (DataGridViewRow)) as DataGridViewRow;
                var hittest = dataGridView1.HitTest(clientPoint.X, clientPoint.Y);

                try
                {
                    dataGridView1.Rows.Remove(rowValue);
                    dataGridView1.Rows.Add(rowValue);
                    dataGridView1.Refresh();

                    valueFromMouseDown = null;
                }
                catch (Exception ex)
                {
                    throw;
                }

                
/*                if (hittest.ColumnIndex != -1 && hittest.RowIndex != -1)
                {
                    //dataGridView2[hittest.ColumnIndex, hittest.RowIndex].Value = cellvalue;
                    
                }*/
            }
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            dataGridView1.DoDragDrop(dataGridView1, DragDropEffects.Move);
        }
    }
}
