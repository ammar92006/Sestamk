using System;
using System.Windows.Forms;
using Sestamk.Classes;
using Sestamk.Forms;

namespace Sestamk
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ── Startup: splash form owns the orchestrator and decides what's next ──
            StartupAction nextAction;
            using (var splash = new frmSplash())
            {
                Application.Run(splash);          // returns when splash closes itself
                nextAction = splash.NextAction;
            }

            switch (nextAction)
            {
                case StartupAction.LaunchLogin:
                    Application.Run(new Login());
                    break;

                case StartupAction.LaunchActivation:
                    using (var activationForm = new frmActivation())
                    {
                        if (activationForm.ShowDialog() == DialogResult.OK)
                            Application.Run(new Login());
                    }
                    break;

                case StartupAction.Exit:
                default:
                    return;
            }
        }
    }
}
