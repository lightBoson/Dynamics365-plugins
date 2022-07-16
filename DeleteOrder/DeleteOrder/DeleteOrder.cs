using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;

namespace DeleteOrder
{
    public class DeleteOrder : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Wez interfejs do logu
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Implementacja kodu - wez kontekst aplikacji
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            // Wez wartosc usuniego orderu
            Entity preImageEntity = context.PreEntityImages.Values.ElementAt<Entity>(0);
            if (preImageEntity == null)
                return;
            // Wez atrybut uid i numer referencyjny
            Guid uid = preImageEntity.GetAttributeValue<Guid>("ko_order_koid");
            string deletedOrderId = preImageEntity.GetAttributeValue<Guid>("ko_order_koid").ToString();
            string deletedOrderNb = preImageEntity.GetAttributeValue<string>("ko_referencenumberko");
            if (string.IsNullOrEmpty(deletedOrderNb))
                return;

            // Stworz zapytanie
            QueryExpression query = new QueryExpression();

            // Skieruj zapytanie na konkretna tabele
            query.EntityName = "ko_complaintko";

            // Zbierz wszystkie atrybuty tabeli
            query.ColumnSet = new ColumnSet(true);

            // Stworz obiekt relacji
            Relationship relationship = new Relationship();

            // Stworz obiekt filtru
            query.Criteria = new FilterExpression();
            
            // Nazwij relacje
            relationship.SchemaName = "ko_ko_complaintko_ko_order_ko";

            // Zbierz kolekcje relacji
            RelationshipQueryCollection relatedEntity = new RelationshipQueryCollection();

            // Dodaj relacje i zapytanie do kolekcji
            relatedEntity.Add(relationship, query);

            // Stworz prosbe
            RetrieveRequest request = new RetrieveRequest();

            // Dodaj zapytanie do prosby
            request.RelatedEntitiesQuery = relatedEntity;

            // Dowiaz kolumn set i referencje do orderu po uid
            request.ColumnSet = new ColumnSet("ko_order_koid");
            // Guid testUid = new Guid("065d32e5-d211-40be-962b-0877e6a7bebb");
            request.Target = new EntityReference("ko_order_ko", uid);

            // Wykonaj zapytanie
            RetrieveResponse response = (RetrieveResponse)service.Execute(request);
            RelatedEntityCollection rEntities = (RelatedEntityCollection)response.Entity.RelatedEntities;
            EntityCollection eCollection = rEntities[relationship];

            // Stworz formatke wiadomosci
            string msgTemplate = "Operation cancelled -selected Order can not be deleted as record is associated with Complain Record(s) number: ";
            // Stworz kontener complaint
            List<string> orderComplaints = new List<string>();
            // Iteruj po entities w celu znalezienia dowiazanych complaint
            for (int j = 0; j < eCollection.Entities.Count(); ++j)
            {
                Entity rEntity = eCollection.Entities.ElementAt<Entity>(j);
                if (rEntity != null) 
                {
                    orderComplaints.Add(rEntity.GetAttributeValue<string>("ko_referencenumbercomplaintko")); 
                }
            }
            if (orderComplaints.Count() != 0)
            {
                throw new InvalidPluginExecutionException(msgTemplate + String.Join(", ", orderComplaints.ToArray()));
            }
        }
    }
}
