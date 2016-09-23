using System.Drawing;
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
}