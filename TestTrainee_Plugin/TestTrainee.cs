using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TestTrainee_Plugin
{
    public class TestTrainee : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //ITracingService tracingService =
            //(ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            Entity entity = (Entity)context.InputParameters["Target"];

            if (entity.Contains("ms_departamento"))
            {
                IOrganizationServiceFactory serviceFactory = 
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                string fetchXml =
                @"<fetch version=""1.0"" output-format=""xml - platform"" mapping=""logical"" distinct=""false""><entity name = ""ms_reserva"" >
                    < attribute name = ""ms_reservaid"" />
                    < attribute name = ""ms_reserva"" />
                    < attribute name = ""createdon"" />
                    < order attribute = ""ms_reserva"" descending = ""false"" />
                    < filter type = ""and"" >
                        < condition attribute = ""statecode"" operator= ""eq"" value = ""0"" />
                        < condition attribute = ""ms_departamento"" operator= ""eq"" value = ""af361822-8111-ec11-b6e7-002248376565"" />
                    </ filter >
                    </ entity >
                    </ fetch > 
                ";
                fetchXml = string.Format(fetchXml, entity.Id);
                var qe = new FetchExpression(fetchXml);
                var result = service.RetrieveMultiple(qe);
                foreach (var e in result.Entities)
                {
                    Entity updatedDepart = new Entity(e.LogicalName);
                    updatedDepart.Id = e.Id;
                    updatedDepart["ms_departamento"] = entity["ms_departamento"];
                    service.Update(updatedDepart);
                }
            }
            
        }
    }
}