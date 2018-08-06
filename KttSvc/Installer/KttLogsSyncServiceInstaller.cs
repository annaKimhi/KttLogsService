using System.ComponentModel;
using System.ServiceProcess;

namespace KTT.Installer
{
    [RunInstaller(true)]
    public class KttLogsSyncServiceInstaller : System.Configuration.Install.Installer
    {
        public KttLogsSyncServiceInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();

            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Password = null;
            serviceProcessInstaller.Username = null;

            ServiceInstaller KttSyncInstaller = new ServiceInstaller();
            KttSyncInstaller.ServiceName = "KTT Synchronizer";
            KttSyncInstaller.StartType = ServiceStartMode.Automatic;

            Installers.AddRange(new System.Configuration.Install.Installer[] { serviceProcessInstaller, KttSyncInstaller });
        }
    }
}
