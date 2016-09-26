using System.Drawing;
using System.Windows.Forms;

namespace DataGrid
{
    public class SuperPanel : Panel
    {
        public bool IsHoveredOver { get; set; }

        public int RowIndex { get; set; }

        public override bool Equals(object obj)
        {
            var compareTo = obj as SuperPanel;
            if (compareTo != null)
            {
                return RowIndex == compareTo.RowIndex;
            }
            return false;
        }

        // WARNING: Do not use it for dictionaries etc because it is not reas-only value (hashtable hash should be immutable)
        public override int GetHashCode()
        {
            return RowIndex;
        }

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