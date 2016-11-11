using System;
using System.Collections.Generic;

namespace SeabBot.Const
{

    [Serializable]
    public class Mapping
    {
        private static Mapping mapping { get; set; }

        public static Mapping GetInstance()
        {
            if (mapping == null) mapping = new Mapping();
            return mapping;
        }

        public Dictionary<string, string> subjectMap;
        
        private Mapping()
        {
            subjectMap = new Dictionary< string,string>();

            try
            {
                subjectMap.Add("chem", "Chemistry (SPA)");
                subjectMap.Add("chemistry", "Chemistry (SPA)");
                subjectMap.Add("chemical", "Chemistry (SPA)");
                subjectMap.Add("chemsity", "Chemistry (SPA)");
                subjectMap.Add("kimia", "Chemistry (SPA)");

                subjectMap.Add("physics", "Physics (SPA)");
                subjectMap.Add("phy", "Physics (SPA)");
                subjectMap.Add("physic", "Physics (SPA)");
                subjectMap.Add("phsics", "Physics (SPA)");
                subjectMap.Add("phys", "Physics (SPA)");

                subjectMap.Add("bio", "Biology (SPA)");
                subjectMap.Add("biology", "Biology (SPA)");
                subjectMap.Add("biolog", "Biology (SPA)");
                subjectMap.Add("bilogy", "Biology (SPA)");
                subjectMap.Add("biologi", "Biology (SPA)");

                subjectMap.Add("science", "Science (Chemistry, Biology)");
                subjectMap.Add("sci", "Science (Chemistry, Biology)");
                subjectMap.Add("sci-fi", "Science (Chemistry, Biology)");
                subjectMap.Add("scienec", "Science (Chemistry, Biology)");
                subjectMap.Add("ipa", "Science (Chemistry, Biology)");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception ex " + ex.Message);
            }


        }

        //mapper
        public string getSubjectMapping(string subject)
        {
            string result = subject;
            try
            {
                result = subjectMap[subject];
                return result;
            }
            catch (Exception e)
            {
                return subject;
            }
            
        }

        public DateTime? getDateMapping(string datee)
        {
            if (datee.ToLower() == "tomorrow" || datee.ToLower() == "tmr")
            {
                DateTime temp = DateTime.Today.AddDays(1);
                DateTime obj = new DateTime(temp.Year, temp.Month, temp.Day, 8, 0, 0);
                return obj;
            }else if (datee.ToLower() == "today" || datee.ToLower() == "tdy")
            {
                DateTime temp = DateTime.Today;
                DateTime obj = new DateTime(temp.Year, temp.Month, temp.Day, 8, 0, 0);
                return obj;
            }
            else if (datee.ToLower() == "yesterday" || datee.ToLower() == "ytd")
            {
                DateTime temp = DateTime.Today.AddDays(-1);
                DateTime obj = new DateTime(temp.Year, temp.Month, temp.Day, 8, 0, 0);
                return obj;
            }
            else
            {
                try
                {
                    DateTime temp = DateTime.Parse(datee);
                    DateTime obj = new DateTime(temp.Year, temp.Month, temp.Day, 8, 0, 0);
                    return obj;
                }catch (Exception e)
                {
                    return null;
                }
            }

        }

    }
}