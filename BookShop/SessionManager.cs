using System;
using System.Configuration;
using System.Windows.Forms;

namespace BookShop
{
    public static class SessionManager
    {
        private static Timer inactivityTimer;
        private static int timeoutSeconds;
        private static Form currentForm;

        public static void Initialize(Form form)
        {
            currentForm = form;

            string timeoutStr = ConfigurationManager.AppSettings["SessionTimeoutSeconds"];
            if (!int.TryParse(timeoutStr, out timeoutSeconds))
                timeoutSeconds = 30;

            inactivityTimer = new Timer();
            inactivityTimer.Interval = timeoutSeconds * 1000;
            inactivityTimer.Tick += InactivityTimer_Tick;
            inactivityTimer.Start();

            Application.AddMessageFilter(new ActivityMessageFilter());
        }

        public static void ResetTimer()
        {
            inactivityTimer?.Stop();
            inactivityTimer?.Start();
        }

        private static void InactivityTimer_Tick(object sender, EventArgs e)
        {
            inactivityTimer.Stop();

            LoginForm loginForm = new LoginForm();
            loginForm.Show();

            if (currentForm != null && !currentForm.IsDisposed)
            {
                currentForm.Hide();
            }
        }

        private class ActivityMessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == 0x200 || m.Msg == 0x100)
                {
                    ResetTimer();
                }
                return false;
            }
        }
    }
}