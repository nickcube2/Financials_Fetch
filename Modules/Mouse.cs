using System.Drawing;
using System.Windows.Forms;

namespace FinancialBoardsFetch.Modules
{
    internal class Mouse : IMessageFilter
    {
        internal void EnableMouse()
        {
            Application.UseWaitCursor = false;
            Application.DoEvents();
            Application.RemoveMessageFilter(this);
            Application.DoEvents();
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 0x201 || m.Msg == 0x202 || m.Msg == 0x203) return true;
            if (m.Msg == 0x204 || m.Msg == 0x205 || m.Msg == 0x206) return true;
            return false;
        }

        internal void DisableMouse()
        {
            Application.UseWaitCursor = true;
            Application.DoEvents();
            Application.AddMessageFilter(this);
        }
    }
}
