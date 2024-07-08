using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cell.Plugin
{
    public abstract class CellTextProvider
    {
        public abstract string GetText(PluginContext context, Model.CellModel cellModel);
    }
}
