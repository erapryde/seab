using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.DAL;

namespace BusinessObjects
{
    public class DBHelper
    {
        private DBHelper helper;
        private DBHelper() { }
        public DBHelper GetInstance()
        {
            if (helper == null) return new DBHelper();
            else return helper;
        }

        public List<ExamEnquiryResult> GetExamList(ExamEnquiry enquiry)
        {
            using (productdbEntities db = new productdbEntities())
            {
                var query =
                    from ex in db.SEAB_EXAM
                    from sch in db.SEAB_SCHOOL.Where(x => ex.SCH_ID == x.SCH_ID)
                    from sub in db.SEAB_SUBJECT.Where(y => ex.SUBJ_CODE == ex.SUBJ_CODE)
                    .DefaultIfEmpty()
                    where ( 
                        (enquiry.Subject == null || enquiry.Subject == sub.SUBJ_NAME) 
                        && (enquiry.SchoolName == null || enquiry.SchoolName == sch.SCH_NAME)
                        && ((enquiry.StartDate == null && enquiry.EndDate == null) || (ex.EXAM_DATE <= enquiry.EndDate && ex.EXAM_DATE >= enquiry.StartDate))
                        )
                       
                    select new ExamEnquiryResult()
                    {
                        SubjectCode = ex.SUBJ_CODE,
                        SeatMax = ex.SEAT_MAX,
                        SeatUsed = ex.SEAT_USED == null ? 0 : ex.SEAT_USED.Value, 
                        ExamDate = ex.EXAM_DATE,
                        StartTime = ex.START_TIME,
                        EndTime = ex.END_TIME,
                        SchoolName = sch.SCH_NAME,
                        SchLat = sch.SCH_LOC_LAT,
                        SchLon = sch.SCH_LOC_LONG,
                        SubjectName = sub.SUBJ_NAME, 
                        Syllabus = sub.SYLLABUS
                    };

                return query.ToList();
               
            }
        }

    }
}
