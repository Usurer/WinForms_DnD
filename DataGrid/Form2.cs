using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DataGrid
{
    public partial class Form2 : Form
    {
        private Rectangle dragBoxFromMouseDown;
        private SuperPanel draggedPanel;
        private SuperPanel hoveredPanel; // parent, over which the mouse is located
        private Cursor panelCursor;

        //http://stackoverflow.com/questions/550918/change-cursor-hotspot-in-winforms-net
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

       

        public static Cursor CreateCursorNoResize(Bitmap bmp)
        {

            IntPtr icon = bmp.GetHicon();
            IconInfo tmp = new IconInfo();
            GetIconInfo(icon, ref tmp);

            tmp.xHotspot = 0;
            tmp.yHotspot = 0;
            tmp.fIcon = false;

            icon = CreateIconIndirect(ref tmp);
            return new Cursor(icon);
        }

        public Form2()
        {
            InitializeComponent();

            ClientSize = new Size(905, 517);
            //basePanel.Size = new Size(881, 227);
            basePanel.Size = new Size(881, 10);
            basePanel.Location = new Point(12, 12);
            basePanel.BorderStyle = BorderStyle.FixedSingle;
            basePanel.AllowDrop = true;

            basePanel.MouseEnter += RowTemplate_MouseLeave;

            basePanel.DragEnter += Control_DragEnter;
            basePanel.DragDrop += Row_DragDrop;
            //basePanel.MouseUp += Control_MouseUp;

            basePanel.GiveFeedback += Control_GiveFeedback;

            for (var i = 0; i < 3; i++)
            {
                var row = CreateRow(i);
                basePanel.Controls.Add(row);
                basePanel.Height = basePanel.Height + row.Height + 10;
            }
        }

        private void AssignMouseMoveHandler(Control control)
        {
            control.MouseMove += Control_MouseMove;
            control.GiveFeedback += Control_GiveFeedback;
            control.AllowDrop = true;

            foreach (Control child in control.Controls)
            {
                child.MouseMove += Control_MouseMove;
                child.GiveFeedback += Control_GiveFeedback;
                control.AllowDrop = true;

                if (child.Controls.Count > 0)
                {
                    AssignMouseMoveHandler(child);
                }
            }
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
                row.Location = new Point(3, 10 + (53 + 10)*row.RowIndex);
            }

            basePanel.Invalidate();
            basePanel.Update();
        }


        private int counter = 0;

        private SuperPanel CreateRow(int index)
        {
            var rowHeight = 53;

            var orderLabel = CreateOrderLabel(counter);
            counter = counter + 1;
            var flightProblem = CreateFlightProblemComboBox();
            var colorPanel = CreateColorPickerPanel();
            var symbol = CreateSymbolComboBox();
            var size = CreateSizeComboBox();
            var showLabel = CreateShowLabelCheckBox();
            var addControl = CreateAddControl();
            var removeControl = CreateRemoveControl();

            addControl.MouseClick += AddRow;
            removeControl.MouseClick += RemoveRow;

            var panel = CreateRowPanel(index, rowHeight, orderLabel, flightProblem, colorPanel, symbol, size, showLabel, addControl, removeControl);

            AssignRowEventHandlers(panel);
            AssignDragAnchorEventHandlers(orderLabel);

            return panel;
        }

        private void AssignDragAnchorEventHandlers(Label orderLabel)
        {
            orderLabel.MouseMove += DragAnchor_MouseMove;
            orderLabel.MouseDown += DragAnchor_MouseDown;
        }

        private void AssignRowEventHandlers(SuperPanel panel)
        {
            panel.DragOver += Control_DragOver;
            panel.DragOver += Control_MouseMove;

            panel.DragDrop += Row_DragDrop;

            panel.DragEnter += Control_DragEnter;
            panel.DragEnter += Control_MouseMove;

            AssignMouseMoveHandler(panel);
        }

        private static SuperPanel CreateRowPanel(int index, int rowHeight, Label orderLabel, ComboBox flightProblem,
            Panel colorPanel, ComboBox symbol, ComboBox size, CheckBox showLabel, Control addControl, Control removeControl)
        {
            var panel = new SuperPanel
            {
                Location = new Point(3, 10 + (rowHeight + 10)*index),
                Size = new Size(875, rowHeight),
                BorderStyle = BorderStyle.None,
                RowIndex = index
            };

            panel.Controls.Add(orderLabel);
            panel.Controls.Add(flightProblem);
            panel.Controls.Add(colorPanel);
            panel.Controls.Add(symbol);
            panel.Controls.Add(size);
            panel.Controls.Add(showLabel);
            panel.Controls.Add(addControl);
            panel.Controls.Add(removeControl);

            panel.AllowDrop = true;

            return panel;
        }

        private void Control_MouseMove(object sender, EventArgs e)
        {
            var panel = sender as SuperPanel;
            while (sender != this && panel == null)
            {
                sender = (sender as Control).Parent;
                panel = sender as SuperPanel;
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

            SetDragCursor();
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

        private void Control_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            if (draggedPanel != null && panelCursor != null)
            {
                Cursor = panelCursor;
                Cursor.Current = panelCursor;
            }
        }

        private void Control_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;

            SetDragCursor();
        }

        private void DragAnchor_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    // Proceed with the drag and drop, passing in the list item.                    
                    var dropEffect = basePanel.DoDragDrop(draggedPanel, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }

            SetDragCursor();
        }

        private void DragAnchor_MouseDown(object sender, MouseEventArgs e)
        {
            draggedPanel = (sender as Control).Parent as SuperPanel;
            if (draggedPanel != null)
            {
                // Remember the point where the mouse down occurred. 
                // The DragSize indicates the size that the mouse can move 
                // before a drag event should be started.                
                var dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width/2), e.Y - (dragSize.Height/2)),
                    dragSize);

                SetDragCursor();
            }
        }

        private void SetDragCursor()
        {
            if (draggedPanel != null && panelCursor == null)
            {
                var bmp = new Bitmap(draggedPanel.Width, draggedPanel.Height);
                draggedPanel.DrawToBitmap(bmp, new Rectangle(Point.Empty, bmp.Size));
                //optionally define a transparent color
                //bmp.MakeTransparent(draggedPanel.BackColor);

                //panelCursor = new Cursor(bmp.GetHicon());
                panelCursor = CreateCursorNoResize(bmp);
                Cursor = panelCursor;
                Cursor.Current = panelCursor;

                draggedPanel.Hide();
            }
        }

        private void ReleaseDragCursor()
        {
            if (draggedPanel != null)
            {
                draggedPanel.Show();
            }
            Cursor = DefaultCursor;
            panelCursor = null;
            draggedPanel = null;
        }

        private void Control_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;

            SetDragCursor();
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
                    if (row.RowIndex > fromIndex && row.RowIndex <= toIndex)
                    {
                        row.RowIndex = row.RowIndex - 1;
                    }

                    draggedPanel.RowIndex = toIndex;
                }
                else if (fromIndex > toIndex)
                {
                    if (row.RowIndex < fromIndex && row.RowIndex >= toIndex)
                    {
                        row.RowIndex = row.RowIndex + 1;
                    }

                    draggedPanel.RowIndex = toIndex;
                }

                row.Invalidate();
                row.Update();
            }

            UpdateRowsPositioning();
        }

        private void Row_DragDrop(object sender, DragEventArgs e)
        {
            if ( /*e.Effect == DragDropEffects.Copy*/true)
            {
                //ReorderRows();
                ReleaseDragCursor();
            }
        }

        private void AddRow(object sender, MouseEventArgs e)
        {
            var row = (sender as Control).Parent as SuperPanel;
            if (row != null)
            {
                var newRow = CreateRow(row.RowIndex + 1);
                foreach (SuperPanel panel in basePanel.Controls)
                {
                    if (panel.RowIndex >= newRow.RowIndex)
                    {
                        panel.RowIndex = panel.RowIndex + 1;
                    }
                }

                basePanel.Controls.Add(newRow);
                basePanel.Height = basePanel.Height + newRow.Height + 10;
                UpdateRowsPositioning();
            }
        }

        private void RemoveRow(object sender, MouseEventArgs e)
        {
            var row = (sender as Control).Parent as SuperPanel;
            if (row != null)
            {
                
                foreach (SuperPanel panel in basePanel.Controls)
                {
                    if (panel.RowIndex > row.RowIndex)
                    {
                        panel.RowIndex = panel.RowIndex - 1;
                    }
                }

                basePanel.Controls.Remove(row);
                basePanel.Height = basePanel.Height - row.Height - 10;
                UpdateRowsPositioning();
            }
        }

        private static CheckBox CreateShowLabelCheckBox()
        {
            return new CheckBox { Name = "showLabel", Location = new Point(604, 21), Size = new Size(20, 17) };
        }

        private static ComboBox CreateSizeComboBox()
        {
            return new ComboBox { Name = "size", Location = new Point(517, 18), Size = new Size(71, 21) };
        }

        private static ComboBox CreateSymbolComboBox()
        {
            return new ComboBox { Name = "symbol", Location = new Point(434, 18), Size = new Size(77, 21) };
        }

        private static Panel CreateColorPickerPanel()
        {
            return new Panel
            {
                Name = "colorPicker",
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(385, 18),
                Size = new Size(43, 21)
            };
        }

        private static ComboBox CreateFlightProblemComboBox()
        {
            return new ComboBox
            {
                Name = "flightProblem",
                Location = new Point(113, 18),
                Size = new Size(226, 21)
            };
        }

        private static Label CreateOrderLabel(int index)
        {
            return new Label
            {
                Name = "order",
                Text = index.ToString(),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(40, 21),
                Size = new Size(35, 13)
            };
        }

        private static Panel CreateAddControl()
        {
            return new Panel
            {
                Name = "add",
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(650, 18),
                Size = new Size(21, 21),
                BackColor = Color.DarkGreen,
            };
        }

        private static Panel CreateRemoveControl()
        {
            return new Panel
            {
                Name = "remove",
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(680, 18),
                Size = new Size(21, 21),
                BackColor = Color.Red
            };
        }
    }
}