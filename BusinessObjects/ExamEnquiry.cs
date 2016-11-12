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
    public class ExamEnquiry
    {
        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public DateTime? StartDate { get; set; }

        [DataMember]
        public DateTime? EndDate { get; set; }

        [DataMember]
        public string SchoolName { get; set; }
    }
}
