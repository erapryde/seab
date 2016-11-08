using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot.Utilities
{
    [Serializable]
    public class Amount
    {
        public string Currency { get; set; }
        public double Total { get; set; }
    }
}