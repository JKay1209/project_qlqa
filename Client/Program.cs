using Client.Network;
using project_qlqa;

namespace Client
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            SocketManager.Start();   // kết nối TCP tới Server, auto reconnect
            try
            {
                Application.Run(new main());
            }
            finally
            {
                SocketManager.StopAsync().GetAwaiter().GetResult();
            }
        }
    }
}