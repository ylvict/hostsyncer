using System.ServiceProcess;

namespace HostSyncer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new CGitHost()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
