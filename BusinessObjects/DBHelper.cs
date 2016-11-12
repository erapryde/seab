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
        private static DBHelper helper;
        private DBHelper() { }
        public static DBHelper GetInstance()
        {
            if (helper == null) helper = new DBHelper();
            return helper;
        }

        public List<Tuple<string, double, double> > GetSchoolList()
        {
            try
            {
                using (productdbEntities db = new productdbEntities())
                {
                    return db.SEAB_SCHOOL.ToList().Select(res => Tuple.Create(res.SCH_NAME, (double)res.SCH_LOC_LAT, (double)res.SCH_LOC_LONG)).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new List<Tuple<string, double, double> >();
        }

        public List<ExamEnquiryResult> GetExamList(ExamEnquiry enquiry)
        {
            try
            {
                using (productdbEntities db = new productdbEntities())
                {
                    var query =
                        from ex in db.SEAB_EXAM
                        from sch in db.SEAB_SCHOOL
                        from sub in db.SEAB_SUBJECT
                        .DefaultIfEmpty()
                        where 
                            (
                                ex.SCH_ID == sch.SCH_ID && sub.SUBJ_CODE == ex.SUBJ_CODE
                            )
                            && 
                            (
                                (enquiry.Subject == null || enquiry.Subject == sub.SUBJ_NAME)
                                && (enquiry.SchoolName == null || enquiry.SchoolName == sch.SCH_NAME)
                                && ((enquiry.StartDate == null || enquiry.EndDate == null) || (ex.EXAM_DATE <= enquiry.EndDate && ex.EXAM_DATE >= enquiry.StartDate))
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
                            //ex.EXAM_TIMETABLE
                        };

                    return query.ToList();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new List<ExamEnquiryResult>();
        }

    }
}
