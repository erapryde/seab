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
    public class ExamEnquiryResult
    {
        [DataMember]
        public string SchoolName { get; set; }

        [DataMember]
        public decimal SchLat { get; set; }

        [DataMember]
        public decimal SchLon { get; set; }

        [DataMember]
        public string SubjectCode { get; set; }

        [DataMember]
        public string SubjectName { get; set; }

        [DataMember]
        public string Syllabus { get; set;  }

        [DataMember]
        public int SeatMax { get; set; }

        [DataMember]
        public int SeatUsed { get; set;  }

        [DataMember]
        public DateTime ExamDate { get; set;  }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

    }
}
