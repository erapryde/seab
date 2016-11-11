using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SeabBot.Utility
{
    [Serializable]
    public enum L1_MAINOPTIONS
    {

        [Description("Get Exam Info")]
        [EnumMember]
        GET_EXAM_INFO,

        [Description("Get Venue Info")]
        [EnumMember]
        GET_VENUE_INFO,

        [Description("Book Exam Slot")]
        [EnumMember]
        BOOK_EXAM_SLOT,

        [Description("Get Staff Directory")]
        [EnumMember]
        GET_STAFF_DIRECTORY,

        [Description("Or type your intent")]
        [EnumMember]
        OTHERS
    }

    [Serializable]
    public enum L2_EXAMINFOOPTIONS
    {
        [Description("Subject")]
        [EnumMember]
        GET_TIMETABLE_BY_SUBJECT,

        [Description("Subject and date")]
        [EnumMember]
        GET_TIMETABLE_BY_SUBJECT_AND_DATE,

        [Description("Subject and venue")]
        [EnumMember]
        GET_TIMETABLE_BY_SUBJECT_AND_VENUE,

        [Description("Date and venue")]
        [EnumMember]
        GET_TIMETABLE_BY_DATE_AND_VENUE,

        [Description("Syllabus by subject")]
        [EnumMember]
        GET_SYLLABUS_BY_SUBJECT,
    }

    [Serializable]
    public enum STATE
    {
        L0_ENQUIRE_L1_INTENT,
        L1_GET_EXAM_INFO,
        L2_GET_EXAM_INFO_BY_SUBJECT_TIME,
        L2_GET_EXAM_INFO_BY_SUBJECT,
        L2_GET_SYLLABUS_BY_SUBJECT,


        L1_SHOW_LOCATION,
        L1_ENQUIRE_NEAREST_CENTER,
        L1_BOOK_SEAT,

    }
}