using System;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using System.Collections.Generic;
using System.Globalization;



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

            RetrieveRequest requestComplaint = new RetrieveRequest();
            requestComplaint.ColumnSet = new ColumnSet("ko_referencenumbercomplaintko");
            requestComplaint.Target = new EntityReference("ko_complaintko", complaintReference.Id);
            // Wykonaj zapytanie do complaint
            RetrieveResponse responseComplaint = (RetrieveResponse)service.Execute(requestComplaint);
            string complaintReferenceNumber = responseComplaint.Entity.GetAttributeValue<string>("ko_referencenumbercomplaintko");
            Guid complaintId = complaintReference.Id;
            EntityCollection productCollection = getSpecificCollection(service, relationship, complaintId, "ko_productko");
            List<string> expensiveProductNames = new List<string>();
            for (int i = 0; i < productCollection.Entities.Count; ++i)
            {
                Entity productEntity = productCollection[i];
                if (productEntity == null)
                    continue;
                decimal price = productEntity.GetAttributeValue<Money>("ko_price_ko").Value;
                Guid testUid = new Guid("b5a64316-edfa-ec11-82e5-0022480947e8");
                if (productEntity.Id == testUid && price >= 1000)
                    expensiveProductNames.Add(productEntity.GetAttributeValue<string>("ko_nameko").ToString());
            }

            if (expensiveProductNames.Count == 0)
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
                Note["objectid"] = new EntityReference("contact", contact.Id);
                Note["subject"] = "Expensive product removed from the complaint";     
                Note["notetext"] = "The following products: " + String.Join(", ", expensiveProductNames.ToArray()) + " were removed from the Complaint number" + complaintReferenceNumber + "on " + DateTime.Now.ToString("dd/MM/yyyy") + " at " + DateTime.Now.ToString("hh:mm tt");
                service.Create(Note);
              
            }

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
