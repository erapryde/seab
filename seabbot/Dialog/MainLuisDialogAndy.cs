using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace ExamBot.Dialog
{
    [LuisModel("6dcb9387-f3f9-4613-9332-f52b226bda97", "f0264918b26f4f838bac2123e9092768")]
    [Serializable]
    public class MainLuisDialog : LuisDialog<object>
    {

        public const string DATE_FORMAT = "dd-MMM-yyyy";
        public const string DATETIME_FORMAT = "dd-MMM-yyyy HH:mm";
        public const string TIME_FORMAT = "HH:mm";

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I may not be the best to chat for non **Exam** related stuff...So sorry");
            context.Wait(MessageReceived);
        }

        [LuisIntent("getExamInfo")]
        public async Task getExamInfo(IDialogContext context, LuisResult result)
        {
            string message = "";

            //find the specific intention
            bool isEntityExist = result.Entities.Count > 0;
            bool isExam = false;
            bool hasSubject = false;
            bool hasExamDate = false;
            bool hasStartTime = false;
            bool hasEndTime = false;
            bool hasLocation = false;
            bool hasSyllabus = false;
            bool isWhen = false;

            string eSubject = null;
            DateTime? eExamDate = null;
            DateTime? eStartTime = null;
            DateTime? eEndTime = null;
            string eLocation = null;

            if (isEntityExist)
            {
                foreach (EntityRecommendation enRec in result.Entities)
                {
                    if (enRec.Type == "ExamMain")
                    {
                        isExam = true;
                    }
                    else if (enRec.Type == "ExamSubject")
                    {
                        hasSubject = true;
                        eSubject = enRec.Entity;
                    }
                    else if (enRec.Type == "ExamDate")
                    {
                        if (enRec.Entity.ToUpper() == "WHEN")
                        {
                            hasExamDate = false;
                            isWhen = true;
                        }
                        else
                        {
                            hasExamDate = true;
                            isWhen = false;
                        }

                        //eExamDate = new DateTime(enRec.Entity);
                    }
                    else if (enRec.Type == "ExamSyllabus")
                    {
                        hasSyllabus = true;
                    }

                }
            }

            //process
            if (!isEntityExist)
            {
                message = "Paiseh, simi ar?";
            }
            else
            {
                if (isExam && !hasSubject && !hasExamDate && !hasSyllabus)
                {
                    message = "Maybe... can provide more info on the **Subject** or **Exam Date** ?";
                }
                else if (hasSubject && !hasExamDate && !hasSyllabus)
                {
                    DateTime mExamDate = DateTime.Now;
                    string mSubject = eSubject;
                    string mLocation = "Jean Andy School";

                    message = "There is **" + mSubject + "** exam\n\non: **" + mExamDate.ToString(DATE_FORMAT) + "** at \n\n**" + mLocation + "**";
                }
                else if (hasSubject && hasSyllabus)
                {
                    string mSubject = eSubject;
                    string mURL = "URL";

                    if (isWhen)
                    {
                        message = "You have to study all the time :)";
                    }
                    else
                    {
                        message = "For **" + mSubject + "** you can find the syllabus: **" + mURL + "**";
                    }
                }
                else if (hasSyllabus && !hasSubject && !hasExamDate)
                {
                    message = "Maybe... can provide more info on the **Subject** or **Exam Date** ?";
                }
                else if (!hasSubject && hasExamDate)
                {
                    string mSubject = "Subject";
                    string mLocation = "Jean Andy School";

                    message = "There is **" + mSubject + "** exam\n\nat **" + mLocation + "**";
                }
                else if (hasSubject && hasExamDate)
                {
                    DateTime mExamDate = DateTime.Now;
                    string mSubject = eSubject;
                    bool mHasExam = false;

                    if (mHasExam)
                    {
                        message = "Yes, there is exam";
                    }
                    else
                    {
                        message = "Hmm... **no** ler... you might wanna try another subject or date?";
                    }

                }

                if (message == null || message.Trim().Equals(""))
                {
                    message = "Paiseh, buay hiaw... give me time, I promise I will learn and serve you better ...";
                }

            }
            //send back response
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }


        [LuisIntent("getVenueInfo")]
        public async Task getVenueInfo(IDialogContext context, LuisResult result)
        {
            string message = "";

            //find the specific intention
            bool isEntityExist = result.Entities.Count > 0;
            //process
            if (!isEntityExist)
            {
                message = "Paiseh, simi ar?";
            }
            else
            {
                bool hasSubject = false;
                string eSubject = null;


                
                double locx = 1.288174 / 180 * Math.PI;
                double locy = 103.805375 / 180 * Math.PI;

                var schoolist = new List<school>();
                schoolist = distance(locx, locy);

                foreach (EntityRecommendation enRec in result.Entities)
                {
                    if (enRec.Type == "ExamSubject")
                    {
                        hasSubject = true;
                        eSubject = enRec.Entity;
                    }
                }

                if (eSubject.ToUpper() == "CHEMISTRY")//"CHEMISTRY (SPA)")
                {
                    message = string.Format("There is no spare capacity for **{0}** in other EXAM CENTRES. \n"+ 
                        "Please continue to your **own EXAM CENTRE** before the end of the paper. \n"+
                        "You will be given the full duration.", eSubject);
                } else if(eSubject.ToUpper() == "PHYSICS")//"PHYSICS (SPA)")
                {
                    List<school> list = schoolist.Where(i => i.name == "SchoolN" || i.name == "SchoolW").ToList();
                    List<school> SortedList = list.OrderBy(o => o.dis).ToList();
                    message = string.Format("We have found the following {0} closest locations for {1} based on your current location. \n \n" +
                        "1. **{2}** is **{3} km** away. \n" +
                        "2. **{4}** is **{5} km** away. \n \n" +
                        "You may head for these schools for the {6} paper in the event of a major transport disruption.", SortedList.Count,eSubject,
                        SortedList[0].name, SortedList[0].dis, SortedList[1].name, SortedList[1].dis, eSubject);
                    //School N, School W
                }
                else if (eSubject.ToUpper() == "BIOLOGY")//"BIOLOGY (SPA)")
                {
                    List<school> list = schoolist.Where(i => i.name == "SchoolS" || i.name == "SchoolW").ToList();
                    List<school> SortedList = list.OrderBy(o => o.dis).ToList();
                    message = string.Format("We have found the following {0} closest locations for {1} based on your current location. \n \n" +
                        "1. **{2}** is **{3} km** away. \n" +
                        "2. **{4}** is **{5} km** away. \n \n" +
                        "You may head for these schools for the {6} paper in the event of a major transport disruption.", SortedList.Count, eSubject,
                        SortedList[0].name, SortedList[0].dis, SortedList[1].name, SortedList[1].dis, eSubject);
                    //SchoolS, SchoolW
                }
                else if (eSubject.ToUpper() == "SCIENCE")//"SCIENCE")
                {
                    List<school> SortedList = schoolist.OrderBy(o => o.dis).ToList();
                    message = string.Format("We have found {0} locations for {1} based on your current location. \n \n" +
                        "1. **{2}** is **{3} km** away. \n" +
                        "2. **{4}** is **{5} km** away. \n \n" +
                        "You may head for these schools for the {6} paper in the event of a major transport disruption.", SortedList.Count, eSubject,
                        SortedList[0].name, SortedList[0].dis, SortedList[1].name, SortedList[1].dis, eSubject);
                    //SchoolS, SchoolW
                }

                
                //SortedList.ForEach(Console.WriteLine);
                //Console.ReadLine();

                //message = string.Format("The closest school to take **{0}** is **{1}** which is **{2}** km away", 
                    //eSubject ,SortedList[0].name, SortedList[0].dis);

            }
            //send back response
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }




        public List<school> distance(double locx, double locy)
        {
            //hardcode the school locations
            double school1x = 1.367353 / 180 * Math.PI;
            double school2x = 1.348712 / 180 * Math.PI;
            double school3x = 1.289453 / 180 * Math.PI;
            double school4x = 1.343172 / 180 * Math.PI;
            double school1y = 103.8426 / 180 * Math.PI;
            double school2y = 103.944485 / 180 * Math.PI;
            double school3y = 103.823742 / 180 * Math.PI;
            double school4y = 103.70949 / 180 * Math.PI;

            double dis1, dis2, dis3, dis4, dlon, dlat, a, c, d;

            //Haversine formula
            dlat = school1x - locx;
            dlon = school1y - locy;
            a = (Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2)) + Math.Cos(school1x) * Math.Cos(locx) * (Math.Sin(dlon / 2)) * (Math.Sin(dlon / 2));
            c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            dis1 = 6371 * c; //distance from location to school1 in km
                             //Haversine formula
            dlat = school2x - locx;
            dlon = school2y - locy;
            a = (Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2)) + Math.Cos(school2x) * Math.Cos(locx) * (Math.Sin(dlon / 2)) * (Math.Sin(dlon / 2));
            c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            dis2 = 6371 * c; //distance from location to school2 in km
                             //Haversine formula
            dlat = school3x - locx;
            dlon = school3y - locy;
            a = (Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2)) + Math.Cos(school3x) * Math.Cos(locx) * (Math.Sin(dlon / 2)) * (Math.Sin(dlon / 2));
            c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            dis3 = 6371 * c; //distance from location to school3 in km
                             //Haversine formula
            dlat = school4x - locx;
            dlon = school4y - locy;
            a = (Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2)) + Math.Cos(school4x) * Math.Cos(locx) * (Math.Sin(dlon / 2)) * (Math.Sin(dlon / 2));
            c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            dis4 = 6371 * c; //distance from location to school4 in km

            var schoolist = new List<school>();
            schoolist.Add(new school { name = "SchoolN", dis = Math.Round(dis1, 2) });
            schoolist.Add(new school { name = "SchoolE", dis = Math.Round(dis2, 2) });
            schoolist.Add(new school { name = "SchoolS", dis = Math.Round(dis3, 2) });
            schoolist.Add(new school { name = "SchoolW", dis = Math.Round(dis4, 2) });

            return schoolist;
        }
    }

    public class school
    {
        public double dis { get; set; }
        public string name { get; set; }
    }

    
}