using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;

namespace AutomaticText
{
    public class AddText : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            {
                // Wez interfejs do logu
                ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                // Implementacja kodu - wez kontekst aplikacji
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                Entity contextValues = (Entity)context.InputParameters.Values.ElementAt(0);
      
                if (!contextValues.Contains("ko_description_productko"))         
                    contextValues.Attributes["ko_description_productko"] = "THIS TEXT WAS POPULATED BY PLUGIN";

            }
        }
    }
}
