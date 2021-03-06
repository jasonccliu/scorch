using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SystemCenter.Orchestrator.Integration;
using System.Management;
using System.Management.Instrumentation;
using SCCMInterop;
using Microsoft.ConfigurationManagement;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;


namespace SCCMExtension
{
    [Activity("Get SCCM Sub Collections")]
    public class GetSubCollections : IActivity
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
            designer.AddInput("Parent Collection ID").WithDefaultValue("AAA00000");
            designer.AddCorellatedData(typeof(collection));
            designer.AddOutput("Number of Collections");
        }
        public void Execute(IActivityRequest request, IActivityResponse response)
        {
            SCCMServer = settings.SCCMSERVER;
            userName = settings.UserName;
            password = settings.Password;

            String objID = request.Inputs["Parent Collection ID"].AsString();

            //Setup WQL Connection and WMI Management Scope
            WqlConnectionManager connection = CMInterop.connectSCCMServer(SCCMServer, userName, password);
            using(connection)
            {  
                IResultObject col = null;
                col = CMInterop.getSCCMSubCollections(connection, objID);

                if (col != null)
                {
                    response.WithFiltering().PublishRange(getObjects(col, connection));
                }
                response.Publish("Number of Collections", ObjCount);
            }
        }
        private IEnumerable<collection> getObjects(IResultObject objList, WqlConnectionManager connection)
        {
            foreach (IResultObject obj in objList)
            {
                IResultObject tempObjCol = CMInterop.getSCCMCollection(connection, "CollectionID LIKE '" + obj["subCollectionID"].StringValue + "'");
                foreach (IResultObject o in tempObjCol)
                {
                    ObjCount++;
                    yield return new collection(o);
                }
            }
        }
    }
}

