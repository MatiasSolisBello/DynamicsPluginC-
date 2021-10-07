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
            IPluginExecutionContext context = (IPluginExecutionContext)
            serviceProvider.GetService(typeof(IPluginExecutionContext));

            //Inicializar Trace
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //
            IOrganizationServiceFactory serviceFactory =
            (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            //
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);



            //valida si evento es de creacion -> sino no se ejecuta
            if (context.MessageName.Equals("create", StringComparison.InvariantCultureIgnoreCase))
            {
                tracer.Trace("context.MessageName distinto de create");


                //evitar caida de target -> condicional/entity
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];

                    //LogicalName: ms_reserva. StringComparison: Omitir mayusculas, ñ, tildes
                    if (!entity.LogicalName.Equals("ms_reserva", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;

                    }

                    try
                    {
                        if (entity.Contains("ms_departamento"))
                        {
                            tracer.Trace("TestTrainee_Plugin: Contiene ms_departamento");
                            //tracer.Trace("TestTrainee_Plugin: EntityLogicalName: " + entity.GetAttributeValue<EntityReference>("ms_departamento"));

                            //obtener atributos del entity
                            var departId = entity.GetAttributeValue<EntityReference>("ms_departamento").Id;
                            tracer.Trace("TestTrainee_Plugin: EntityLogicalName: " + entity.GetAttributeValue<EntityReference>("ms_departamento").LogicalName);

                            tracer.Trace("TestTrainee_Plugin: Inicio de fetchXML");

                            string fetchXml = @"
                        <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                            <entity name='ms_reserva'>
                                <attribute name='ms_reservaid' />
                                <attribute name='ms_reserva' />
                                <attribute name='createdon' />
                                <order attribute='ms_reserva' descending='false' />
                                <filter type='and'>
                                    <condition attribute='statecode' operator='eq' value='0' />
                                    <condition attribute='ms_departamento' operator='eq' value='{0}' />
                                </filter>
                            </entity>
                        </fetch>";

                            tracer.Trace("TestTrainee_Plugin: Fin de fetchXML");

                            fetchXml = string.Format(fetchXml, departId);
                            var qe = new FetchExpression(fetchXml);
                            tracer.Trace("TestTrainee_Plugin: qe =>" + qe);

                            EntityCollection result = service.RetrieveMultiple(qe);
                            tracer.Trace("TestTrainee_Plugin: result =>" + result);

                            //foreach (var e in result.Entities)
                            //{
                            //    Entity updatedDepart = new Entity(e.LogicalName);
                            //    updatedDepart.Id = e.Id;
                            //    updatedDepart["ms_departamento"] = entity["ms_departamento"];
                            //    service.Update(updatedDepart);
                            //}


                            tracer.Trace("Inicia ultima condicional");

                            if (result.Entities.Count() > 0)
                            {
                                tracer.Trace("Entramos a la condicional");
                                Entity updatedDepart = new Entity(entity.GetAttributeValue<EntityReference>("ms_departamento").LogicalName);

                                tracer.Trace("Condicional: " + updatedDepart);

                                updatedDepart.Id = entity.GetAttributeValue<EntityReference>("ms_departamento").Id;
                                tracer.Trace("Condicional: " + updatedDepart.Id);

                                //FALLA AQUI
                                OptionSetValueCollection estado = new OptionSetValueCollection();
                                estado.Add(new OptionSetValue(100000003));


                                updatedDepart["ms_estado"] = estado;
                                service.Update(updatedDepart);
                            }
                            tracer.Trace("Finaliza ultima condicional");
                        }

                    }


                    catch (Exception e)
                    {
                        //saca el error -> else -> (e.String)
                        throw new InvalidPluginExecutionException(e.Message);
                    }
                }

            }else if (context.MessageName.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
            {

            }
        }
    }
}