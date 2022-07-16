using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;

namespace WarningPricePlugin
{
    public class CheckPriceEmpty : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Wez interfejs do logu
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Implementacja kodu - wez kontekst aplikacji
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Wez wartosci ktore sa w aplikacji na poziomie widoku Product
            Entity contextValues = (Entity)context.InputParameters.Values.ElementAt(0);
            // Jest tylko jeden obiekt w kolekcji wiec sprawdz czy kolekcja nie zwraca wartosci null
            if(contextValues != null)
            {
                // Znajdz atrybut o nazwie ko_price_ko ktory odpowiada cenie produktu
                Money price = contextValues.GetAttributeValue<Money>("ko_price_ko");
                if (price == null || string.IsNullOrEmpty(price.Value.ToString()))
                {
                    
                    // Jesli nie ma ceny (wartosc null) lub wartosc jest pusta do nie zapisuj rekordu i pokaz blad 
                    string message = "Operation cancelled - Product can not be created with price equal to null";
                    if (tracer != null)
                    {
                        tracer.Trace("{0}, Correlation Id: {1}, Initiating User: {2}",
                        message,
                        context.CorrelationId,
                        context.InitiatingUserId);
                    }
                    throw new InvalidPluginExecutionException(message);
                }
            }
        }
    }
}
