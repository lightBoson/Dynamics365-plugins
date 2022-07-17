using System;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using System.Collections.Generic;

namespace CreatNote
{
    public class AddNote : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Wez interfejs do logu
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IExecutionContext executionContext = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));
            // Implementacja kodu - wez kontekst aplikacji
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            Relationship relationship = (Relationship)executionContext.InputParameters["Relationship"];
            if (!string.Equals(relationship.SchemaName, "ko_ComplaintKO_ko_ProductKO_ko_ProductKO", StringComparison.OrdinalIgnoreCase))
                return;

            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            EntityReference reference = (EntityReference)executionContext.InputParameters["Target"];
            Guid disassociatedProductId = reference.Id;

            EntityReferenceCollection complaintReferences = (EntityReferenceCollection)context.InputParameters["RelatedEntities"];
            EntityReference complaintReference = complaintReferences[0];
            if (complaintReference == null)
                return;
            Guid complaintId = complaintReference.Id;
            // Stworz zapytanie
            QueryExpression query = new QueryExpression();

            // Skieruj zapytanie na konkretna tabele
            query.EntityName = "ko_productko";

            // Zbierz wszystkie atrybuty tabeli
            query.ColumnSet = new ColumnSet(true);

            // Stworz obiekt filtru
            query.Criteria = new FilterExpression();

            // Zbierz kolekcje relacji
            RelationshipQueryCollection relatedEntity = new RelationshipQueryCollection();

            // Dodaj relacje i zapytanie do kolekcji
            relatedEntity.Add(relationship, query);

            // Stworz prosbe
            RetrieveRequest request = new RetrieveRequest();

            // Dodaj zapytanie do prosby
            request.RelatedEntitiesQuery = relatedEntity;

            // Dowiaz kolumn set i referencje do orderu po uid
            request.ColumnSet = new ColumnSet("ko_complaintkoid");
            request.Target = new EntityReference("ko_complaintko", complaintId);

            // Wykonaj zapytanie
            RetrieveResponse response = (RetrieveResponse)service.Execute(request);
            RelatedEntityCollection rEntities = (RelatedEntityCollection)response.Entity.RelatedEntities;
            EntityCollection eCollection = rEntities[relationship];

            for(int i = 0; i < eCollection.Entities.Count; ++i)
            {
                Entity productEntity = eCollection[i];
                if (productEntity == null)
                    continue;
                decimal price = productEntity.GetAttributeValue<Money>("ko_price_ko").Value;
                if (productEntity.Id == disassociatedProductId && price >= 1000)
                    throw new InvalidPluginExecutionException("Co ty w ogole robisz. Kosztuje wiecej niz 1000");
            }

            string toto = "toto";
        }
    }
}
