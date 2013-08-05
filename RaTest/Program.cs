using Elastacloud.AzureManagement.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaTest
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                new SubscriptionManager("82f8e1cd-ee8d-414e-b6a5-be7b18a1fa89")
                    .GetVirtualMachinesManager()
                    .AddPublishSettingsFromFile(@"..\..\credentials.publishsettings")
                    .CreateVirtualMachineDeployment()
                    .WithRoleName("RARole")
                    .WithDeploymentName("RADeployment")
                    .AddToExistingCloudServiceWithName("two10ra")
                    .WithUsernameAndPassword("rich", "E3orange")
                    .WithVmOfSize(Elastacloud.AzureManagement.Fluent.Types.VmSize.Small)
                    .WithStorageAccountForVhds("portalvhdslrl1162zhkg83")
                    .WithDeploymentType("raimage1")
                    .WithComputerName("RATEST")
                    .Deploy();

                Console.WriteLine("DONE");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);            
            }
            Console.ReadKey();

        }
    }
}
