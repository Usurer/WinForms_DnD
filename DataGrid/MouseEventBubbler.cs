using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataGrid
{
    class MouseEventBubbler
    {
        private readonly Control _attachTo;

        public MouseEventBubbler(Control attachTo)
        {
            _attachTo = attachTo;

            _attachTo.MouseMove += _attachTo_MouseMove;

            _attachTo.ControlAdded += _attachTo_ControlAdded;
            _attachTo.ControlRemoved += _attachTo_ControlRemoved;

            foreach (Control control in _attachTo.Controls)
            {
                AttachToControl(control);
            }
        }

        public void _attachTo_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        public event MouseEventHandler MouseMove;

        private void _attachTo_ControlAdded(object sender, ControlEventArgs e)
        {
            AttachToControl(e.Control);
        }

        private void _attachTo_ControlRemoved(object sender, ControlEventArgs e)
        {
            DetachFromControl(e.Control);
        }

        private void AttachToControl(Control c)
        {
            c.MouseMove += Child_MouseMove;
            c.ControlAdded += Child_ControlAdded;
            c.ControlRemoved += Child_ControlRemoved;
            AttachToChildren(c);
        }

        private void AttachToChildren(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                AttachToControl(child);
            }
        }

        private void DetachFromControl(Control c)
        {
            DetachFromChildren(c);
            c.MouseMove -= Child_MouseMove;
            c.ControlAdded -= Child_ControlAdded;
            c.ControlRemoved -= Child_ControlRemoved;
        }

        private void DetachFromChildren(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                DetachFromControl(child);
            }
        }

        private void Child_ControlAdded(object sender, ControlEventArgs e)
        {
            AttachToControl(e.Control);
        }

        private void Child_ControlRemoved(object sender, ControlEventArgs e)
        {
            DetachFromControl(e.Control);
        }

        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            var pt = e.Location;
            var child = (Control)sender;
            do
            {
                //pt.Offset(child.Left, child.Top);
                child = child.Parent;
            }
            while (child != _attachTo);
            
            var newArgs = new MouseEventArgs(e.Button, e.Clicks, pt.X, pt.Y, e.Delta);
            //OnMouseMove(newArgs);
        }

        private void OnMouseMove(MouseEventArgs newArgs)
        {
            var h = MouseMove;
            if (h != null)
            {
                h(this, newArgs);
            }
        }
    }
}
