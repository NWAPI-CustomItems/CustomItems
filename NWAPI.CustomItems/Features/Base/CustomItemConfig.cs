using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWAPI.CustomItems.Features.Base
{
    public abstract class CustomItemConfig
    {
        public abstract string Name { get; set; }

        public abstract string Description { get; set; }

        public abstract float Weight { get; set; }

        public abstract ItemType ModelType { get; set; }
    }
}
