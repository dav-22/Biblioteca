using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Biblioteca
{
    public class Validar : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            
            // The InputParameters collection contains all the data passed in the message request.  
            if (
                context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity
            )
            {

                //Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];
                

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    //DNI dl regisro entrante
                    int dni = entity.GetAttributeValue<int>("new_dni");
                  
                    //Comparo DNI entrante con los DNI de la tabla
                    ConditionExpression condition = new ConditionExpression();
                    condition.AttributeName = "new_dni";
                    condition.Operator = ConditionOperator.Equal;
                    condition.Values.Add(dni);

                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(condition);

                    QueryExpression query = new QueryExpression("new_socio");
                    query.ColumnSet = new ColumnSet(true);
                    query.Criteria.AddFilter(filter1);

                    EntityCollection result = service.RetrieveMultiple(query);

                    int total = result.Entities.Count;

                    if (total >= 1)
                    {
                        throw new InvalidPluginExecutionException("DNI ya existente");

                    }

                    //FetchXML para traer el nro max de socio
                    string fetch = @" 
                        <fetch top='1'> 
                           <entity name='new_socio'>   
                                <attribute name='new_nrodesocio' /> 
                                <order attribute='new_nrodesocio' descending = 'true' /> 
                           </entity> 
                        </fetch>";


                    EntityCollection fetchresult = service.RetrieveMultiple(new FetchExpression(fetch));

                    var socio = fetchresult.Entities.FirstOrDefault();

                    

                    if(socio != null)
                    {
                        
                        entity["new_nrodesocio"] = socio.GetAttributeValue<int>("new_nrodesocio") + 1;

                    }
                    else
                    {
                        entity["new_nrodesocio"] = 1;
                    }

                
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

//string fetch = @" 
//    <fetch distinct='false' mapping='logical' aggregate='true' top='1'> 
//        <entity name='new_socio'> 
//            <attribute name='new_nrodesocio' alias='max_nrodesocio' aggregate='max' /> 
//        </entity> 
//    </fetch>";