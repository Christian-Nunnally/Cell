using Cell.Model;
using Cell.ViewModel.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cell.Execution
{
    public class CollectionReference
    {
        public string CollectionName { get; set; }

        public PluginFunction ResolveReferenceFunction { get; set; }

        public CollectionReference()
        {
            // Needs to track cell value changes

            // Should I add a central cell value change tracker like cellpopulatemanager and make cell populate manager use it?
        }
    }
}
