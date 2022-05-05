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
    public class ValidarISBN : IPlugin
    {
        void IPlugin.Execute(IServiceProvider serviceProvider)
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
                    string isbnColumn = "new_isbn";
                    int isbn = entity.GetAttributeValue<int>(isbnColumn);

                    ConditionExpression condition = new ConditionExpression();
                    condition.AttributeName = isbnColumn;
                    condition.Operator = ConditionOperator.Equal;
                    condition.Values.Add(isbn);

                    FilterExpression filter = new FilterExpression();
                    filter.Conditions.Add(condition);

                    QueryExpression query = new QueryExpression("new_libro");
                    query.ColumnSet = new ColumnSet(true);
                    query.Criteria.AddFilter(filter);

                    EntityCollection result = service.RetrieveMultiple(query);

                    int total = result.Entities.Count;

                    if (total >= 1)
                    {
                        throw new InvalidPluginExecutionException("ISBN ya existente");

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
