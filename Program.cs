using System;
using System.Windows.Forms;

using FinancialBoardsFetch.Forms;

using Microsoft.VisualBasic.ApplicationServices;

namespace FinancialBoardsFetch
{
    class Program : WindowsFormsApplicationBase
    {
        public static Guid Guid;

        public Program(bool isSingleInstance)
        {
            this.EnableVisualStyles = true;
            this.IsSingleInstance = isSingleInstance;
            this.MainForm = new MainForm();
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
        {
            ((MainForm)MainForm).RestoreFromTray();

            Program.ShowMessage(((MainForm)MainForm), $"An instance of '{Application.ProductName}' is already running.", "Already running!", 5000, MessageBoxButtons.OK, MessageBoxIcon.Information);

            e.BringToForeground = true;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            bool isSingleInstance = true;

            if (args.Length > 0)
                isSingleInstance = bool.Parse(args[0]);


            string NL = Environment.NewLine;
            string DL = NL + NL;
            try
            {
                new Program(isSingleInstance).Run(args);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application almost crashed.{DL}Reason for crash:{NL}{ex.Message}{DL}Stack trace:{DL}{ex.StackTrace}", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal static DialogResult ShowMessage(IWin32Window owner, string text, string caption = "", int timeout = 5000,
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            var dialog = AutoClosingMessageBox.Factory(showMethod: (caption, buttons) => MessageBox.Show(owner, text, caption, buttons, icon), caption: caption);

            return dialog.Show(timeout: timeout, buttons: buttons);
        }
    }
}
