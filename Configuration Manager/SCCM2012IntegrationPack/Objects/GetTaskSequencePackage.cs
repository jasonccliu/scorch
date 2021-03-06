using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SystemCenter.Orchestrator.Integration;
using System.Management;
using System.Management.Instrumentation;
using SCCM2012Interop;
using Microsoft.ConfigurationManagement;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;


namespace SCCM2012IntegrationPack
{
    [Activity("Get SCCM Task Sequence Package")]
    public class GetTaskSequencePackage : IActivity
    {
        private ConnectionCredentials settings;
        private String userName = String.Empty;
        private String password = String.Empty;
        private String SCCMServer = String.Empty;

        private int ObjCount = 0;

        [ActivityConfiguration]
        public ConnectionCredentials Settings
        {
            get { return settings; }
            set { settings = value; }
        }
        public void Design(IActivityDesigner designer)
        {
            designer.AddInput("Filter").WithDefaultValue("PackageID LIKE '%'");
            designer.AddCorellatedData(typeof(taskSequencePackage));
            designer.AddOutput("Number of Task Sequence Packages");
        }
        public void Execute(IActivityRequest request, IActivityResponse response)
        {
            SCCMServer = settings.SCCMSERVER;
            userName = settings.UserName;
            password = settings.Password;

            String filter = request.Inputs["Filter"].AsString();

            //Setup WQL Connection and WMI Management Scope
            WqlConnectionManager connection = CM2012Interop.connectSCCMServer(SCCMServer, userName, password);
            using(connection)
            {  
                IResultObject col = null;
                col = CM2012Interop.getSCCMTaskSequencePackage(connection, filter);

                if (col != null)
                {
                    response.WithFiltering().PublishRange(getObjects(col));

                }
                response.Publish("Number of Task Sequence Packages", ObjCount);
            }
        }
        private IEnumerable<taskSequencePackage> getObjects(IResultObject objList)
        {
            foreach (IResultObject obj in objList)
            {
                ObjCount++;
                yield return new taskSequencePackage(obj);
            }
        }
    }
}

