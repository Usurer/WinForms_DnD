using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataGrid
{
    public class SuperPanel : Panel
    {
        public bool IsHoveredOver { get; set; }

        public int RowIndex { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            
            //using (SolidBrush brush = new SolidBrush(BackColor))
            //{
            //    e.Graphics.FillRectangle(brush, ClientRectangle);
            //}
            e.Graphics.DrawRectangle(IsHoveredOver ? Pens.CornflowerBlue : Pens.Red, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }
    }

    public partial class Form2 : Form
    {
        private Rectangle dragBoxFromMouseDown;
        private SuperPanel draggedPanel; 
        private SuperPanel hoveredPanel; // parent, over which the mouse is located

        public Form2()
        {
            
            InitializeComponent();
            
            this.ClientSize = new System.Drawing.Size(905, 517);
            basePanel.Size = new Size(881, 227);
            basePanel.Location = new Point(12, 12);
            basePanel.BorderStyle = BorderStyle.FixedSingle;
            basePanel.AllowDrop = true;

            basePanel.MouseEnter += RowTemplate_MouseLeave;

            for (var i = 0; i < 3; i++)
            {
                var row = CreateRow(RowTemplate_MouseMove, i);
                basePanel.Controls.Add(row);
            }
        }

        private void AssignMouseMoveHandler(Control control, MouseEventHandler handler)
        {
            control.MouseMove += handler;
            control.GiveFeedback += Drag_GiveFeedback;
            foreach (Control child in control.Controls)
            {
                child.MouseMove += handler;
                child.MouseUp += Child_MouseUp;
                child.GiveFeedback += Drag_GiveFeedback;
                if (child.Controls.Count > 0)
                {
                    AssignMouseMoveHandler(child, handler);
                }
            }
        }

        private void Child_MouseUp(object sender, MouseEventArgs e)
        {
            ReleaseDragCursor();
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

        private void UpdateRowsPositioning()
        {
            foreach (SuperPanel row in basePanel.Controls)
            {
                row.Location = new Point(3, 35 + (53 + 5)*row.RowIndex);
            }

            basePanel.Invalidate();
            basePanel.Update();
        }

        private Panel CreateRow(MouseEventHandler mouseMouseEventHandler, int index)
        {
            var rowHeight = 53;

            var orderLabel = new Label { Name = "order", Text = index.ToString(), BorderStyle = BorderStyle.FixedSingle, Location = new Point(40, 21), Size = new Size(35, 13)};
            var flightProblem = new ComboBox { Name = "flightProblem", Location = new Point(113, 18), Size = new Size(226, 21) };
            var colorPanel = new Panel() { Name = "colorPicker", BorderStyle = BorderStyle.FixedSingle, Location = new Point(385, 18), Size = new Size(43, 21) };
            var symbol = new ComboBox { Name = "symbol", Location = new Point(434, 18), Size = new Size(77, 21) };
            var size = new ComboBox { Name = "size", Location = new Point(517, 18), Size = new Size(71, 21) };
            var showLabel = new CheckBox() { Name = "showLabel", Location = new Point(604, 21), Size = new Size(80, 17) };

            var panel = new SuperPanel { Location = new Point(3, 35 + (rowHeight + 5) * index), Size = new Size(875, rowHeight), BorderStyle = BorderStyle.None, RowIndex = index };
            panel.Controls.Add(orderLabel);
            panel.Controls.Add(flightProblem);
            panel.Controls.Add(colorPanel);
            panel.Controls.Add(symbol);
            panel.Controls.Add(size);
            panel.Controls.Add(showLabel);

            panel.DragEnter += Row_DragEnter;
            panel.DragEnter += RowTemplate_MouseMove;
            panel.DragOver += Row_DragOver;
            panel.DragOver += RowTemplate_MouseMove;
            panel.DragDrop += Row_DragDrop;
            panel.AllowDrop = true;

            panel.MouseUp += Child_MouseUp;

            orderLabel.MouseMove += DragAnchor_MouseMove;
            orderLabel.MouseDown += DragAnchor_MouseDown;
            orderLabel.MouseUp += Child_MouseUp;

            AssignMouseMoveHandler(panel, mouseMouseEventHandler);

            panel.MouseMove += mouseMouseEventHandler;
            //panel.Click += delegate(object sender, EventArgs args) {  panel.IsHoveredOver = panel.IsHoveredOver ? false : true; panel.Controls["order"].Text = panel.IsHoveredOver.ToString(); panel.Invalidate(); panel.Update(); panel.Refresh(); };
            //panel.Paint += (sender, args) => { panel.Controls["order"].Text = panel.RowIndex.ToString(); panel.Controls["order"].Invalidate(); };
            return panel;
        }

        // TODO: Actually we don't need to handle all children mouse moves. We can set mouse over flag on Panel Mouse Move and remove it on Base Panel mouse move;
        private void RowTemplate_MouseMove(object sender, EventArgs e)
        {
            SuperPanel panel = sender as SuperPanel;
            while (sender != this && panel == null)
            {
                sender = (sender as Control).Parent;
            }

            if (panel != null)
            {
                hoveredPanel = panel;
                if (!panel.IsHoveredOver)
                {
                    panel.IsHoveredOver = true;
                    panel.Invalidate();
                    panel.Update();
                }

                if (draggedPanel != null)
                {
                    ReorderRows();
                }
            }

            if (draggedPanel != null)
            {
                SetDragCursor(draggedPanel);
            }
        }

        private void RowTemplate_MouseLeave(object sender, EventArgs e)
        {
            foreach (SuperPanel row in basePanel.Controls)
            {
                if (row.IsHoveredOver)
                {
                    row.IsHoveredOver = false;
                    row.Invalidate();
                    row.Update();
                }
            }

            hoveredPanel = null;
        }

        private void Drag_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (draggedPanel != null)
            {
                if (panelCursor != null)
                {
                    Cursor = panelCursor;
                    Cursor.Current = panelCursor;
                }
            }
        }

        private void Row_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;

            if (draggedPanel != null)
                SetDragCursor(draggedPanel);
        }

        private void DragAnchor_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    // Proceed with the drag and drop, passing in the list item.                    
                    DragDropEffects dropEffect = basePanel.DoDragDrop(draggedPanel, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }

            if (draggedPanel != null)
                SetDragCursor(draggedPanel);
        }

        private void DragAnchor_MouseDown(object sender, MouseEventArgs e)
        {
            draggedPanel = (sender as Control).Parent as SuperPanel;
            if (draggedPanel != null)
            {
                // Remember the point where the mouse down occurred. 
                // The DragSize indicates the size that the mouse can move 
                // before a drag event should be started.                
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
            }

            if (draggedPanel != null)
                SetDragCursor(draggedPanel);
        }

        private Cursor panelCursor = null;

        private void SetDragCursor(Panel panel)
        {
            if (draggedPanel != null)
            {
                if (panelCursor == null)
                {
                    var bmp = new Bitmap(panel.Width, panel.Height);
                    panel.DrawToBitmap(bmp, new Rectangle(Point.Empty, bmp.Size));
                    //optionally define a transparent color
                    //bmp.MakeTransparent(System.Drawing.Color.White);

                    panelCursor = new Cursor(bmp.GetHicon());
                    Cursor = panelCursor;
                    Cursor.Current = panelCursor;
                }
            }
        }

        private void ReleaseDragCursor()
        {
            Cursor = DefaultCursor;
            panelCursor = null;
            draggedPanel = null;
        }

        private void Row_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;

            if (draggedPanel != null)
                SetDragCursor(draggedPanel);
        }

        private void ReorderRows()
        {
            var toIndex = hoveredPanel.RowIndex;
            var fromIndex = draggedPanel.RowIndex;
            if (toIndex == fromIndex)
            {
                return;
            }

            foreach (SuperPanel row in basePanel.Controls)
            {
                if (toIndex > fromIndex)
                {
                    if (row.RowIndex > fromIndex)
                    {
                        if (row.RowIndex <= toIndex)
                            row.RowIndex = row.RowIndex - 1;
                    }
                    draggedPanel.RowIndex = toIndex;
                }
                else
                {
                    if (row.RowIndex > toIndex)
                    {
                        if (row.RowIndex <= fromIndex)
                            row.RowIndex = row.RowIndex + 1;
                    }
                    draggedPanel.RowIndex = toIndex + 1;
                }

                row.Invalidate();
                row.Update();
            }

            UpdateRowsPositioning();
        }

        private void Row_DragDrop(object sender, DragEventArgs e)
        {
            if (/*e.Effect == DragDropEffects.Copy*/true)
            {
                ReorderRows();
                ReleaseDragCursor();
                
            }
        }
    }
}
