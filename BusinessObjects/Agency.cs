using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    [Serializable]
    [DataContract(Namespace = Constants.Namespace)]

    public class Agency
    {
        [DataMember]
        public string Name { get; set;  }

        [DataMember]
        public string Code { get; set;  }



    }
}
