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
            EntityCollection productCollection = getSpecificCollection(service, relationship, complaintId, "ko_productko");
            List<string> expensiveProductEntities = new List<string>();
            for (int i = 0; i < productCollection.Entities.Count; ++i)
            {
                Entity productEntity = productCollection[i];
                if (productEntity == null)
                    continue;
                decimal price = productEntity.GetAttributeValue<Money>("ko_price_ko").Value;
                Guid testUid = new Guid("b5a64316-edfa-ec11-82e5-0022480947e8");
                if (productEntity.Id == testUid && price >= 1000)
                    expensiveProductEntities.Add(productEntity.Id.ToString());
            }

            if (expensiveProductEntities.Count == 0)
                return;

            Relationship contactRelationship = new Relationship();
            contactRelationship.SchemaName = "ko_Contact_ko_ComplaintKO_ko_ComplaintKO";

            EntityCollection contactCollection = getSpecificCollection(service, contactRelationship, complaintId, "contact");
            for (int j = 0; j < contactCollection.Entities.Count; ++j)
            {
                Entity contact = contactCollection[j];
                if (contact == null)
                    continue;
                Entity Note = new Entity("annotation");
                Guid EntityToAttachTo = Guid.Parse(contact.Id.ToString()); // The GUID of the incident
                Note["objectid"] = new EntityReference("contact", contact.Id);
                Note["subject"] = "Expensive product removed from the complaint";
                Note["notetext"] = "The following products removed from complaint: " + String.Join(", ", expensiveProductEntities.ToArray());
                service.Create(Note);
              
            }


            string toto = "toto";
        }
        private EntityCollection getSpecificCollection(IOrganizationService service, Relationship relationship, Guid complaintId, string entityName)
        {
            // Stworz zapytanie
            QueryExpression query = new QueryExpression();

            // Skieruj zapytanie na konkretna tabele
            query.EntityName = entityName;

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
            return eCollection;
        }  
    }
}
