using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace TestTrainee_Plugin
{
    public class TestTrainee : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //data

            // Obtenga el servicio de rastreo
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtenga el contexto de ejecución del proveedor de servicios.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // La colección InputParameters contiene todos los datos
            // pasados en la solicitud de mensaje.  
            if (context.InputParameters.Contains("Target") &&
            context.InputParameters["Target"] is Entity)
            {
                // Obtenga la entidad de destino de los parámetros de entrada. 
                tracingService.Trace("Obtener la entidad");
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtenga la referencia de servicio de la organización
                // que necesitará para las llamadas de servicio web.  
                IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(
                    typeof(IOrganizationServiceFactory)
                );
                IOrganizationService service = 
                    serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    // Cree una actividad de tarea para hacer un seguimiento con el cliente de la cuenta en 7 días. 
                    Entity followup = new Entity("task");

                    followup["subject"] = "Send e-mail to the new customer.";
                    followup["description"] =
                        "Follow up with the customer. Check if there are any new issues that need resolution.";
                    followup["scheduledstart"] = DateTime.Now.AddDays(7);
                    followup["scheduledend"] = DateTime.Now.AddDays(7);
                    followup["category"] = context.PrimaryEntityName;

                    if (context.OutputParameters.Contains("id"))
                    {
                        Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        string regardingobjectidType = "account";

                        followup["regardingobjectid"] =
                        new EntityReference(regardingobjectidType, regardingobjectid);
                    }

                    tracingService.Trace("FollowupPlugin: Creating the task activity.");
                    service.Create(followup);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
