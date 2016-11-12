using SeabBot.Utility;
using BusinessObjects;
using BusinessObjects.DAL;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using SeabBot.Const;
using System.Configuration;
using System.Net;
using System.Runtime.Serialization.Json;
using BingRestServices.DataContracts;
using System.Text;
//using Google.Maps.Geocoding;

namespace SeabBot.Dialog
{
    [LuisModel("6dcb9387-f3f9-4613-9332-f52b226bda97", "f0264918b26f4f838bac2123e9092768")]
    [Serializable]
    public class MainDialog : LuisDialog<object>
    {
        #region telemetry constants 

        private const string TELE_HANDLED = "Handled";

        private const string TELE_USER_TEXT = "User Text";

        private const string TELE_SELECTED_CHOICE = "SelectedChoice";

        private const string TELE_USER_CHOICE = "LicOneDialog-UserChoice";

        #endregion

        public const string DATE_FORMAT = "dd-MMM-yyyy";
        public const string DATETIME_FORMAT = "dd-MMM-yyyy HH:mm";
        public const string TIME_FORMAT = "HH:mm";

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I may not be the best to chat for non **Exam** related stuff...So sorry");
            context.Wait(MessageReceived);
            //context.Call(new LocationDialog(), LocationDialogCompleted);
            
        }

        public async Task LocationDialogCompleted(IDialogContext context, IAwaitable<bool> result)
        {
            
        }

        #region Frans Code 

        [LuisIntent("getExamInfo")]
        public async Task getExamInfo(IDialogContext context, LuisResult result)
        {
            string message = "";
            try
            {
                //find the specific intention
                bool isEntityExist = result.Entities.Count > 0;
                bool isExam = false;
                bool hasSubject = false;
                bool hasExamDate = false;
                bool hasSyllabus = false;
                bool isWhen = false;

                string eSubject = null;
                DateTime? eExamDate = null;

                //indicator portion
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

                            eExamDate = Mapping.GetInstance().getDateMapping(enRec.Entity);

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
                    message = "Paiseh<sorry>, simi ar?";
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
                        string mSubject = Mapping.GetInstance().getSubjectMapping(eSubject);

                        //get data
                        ExamEnquiry inq = new ExamEnquiry()
                        { Subject = mSubject };
                        List<ExamEnquiryResult> dataResList = DBHelper.GetInstance().GetExamList(inq);
                        //List<ExamEnquiryResult> dataResList = fake.getSubjectListOne();

                        //filtering
                        Dictionary<string, ExamEnquiryResult> tempMap = new Dictionary<string, ExamEnquiryResult>();
                        foreach (ExamEnquiryResult res in dataResList)
                        {
                            if (!tempMap.Keys.Contains(res.SubjectName))
                            {
                                tempMap.Add(res.SubjectName, res);
                            }
                        }
                        List<ExamEnquiryResult> workDataList = tempMap.Values.ToList();
                        int count = workDataList.Count;

                        bool isNear = false;
                        ExamEnquiryResult nearExam = null;
                        if (count == 0)
                        {
                            message = "Shiok, no exam registered yet, but it is always good to prepare";
                        }
                        else if (count == 1)
                        {
                            ExamEnquiryResult res = workDataList[0];
                            message = "There is **" + res.SubjectName + "** exam\n\non **" + res.ExamDate.ToString(DATE_FORMAT) + "** at **08:00**\n\n";

                            //check proximity
                            TimeSpan diff = res.ExamDate - DateTime.Now;
                            if (diff.TotalDays >= 0 && diff.TotalDays <= 7)
                            {
                                nearExam = res;
                                isNear = true;
                            }

                        }
                        else
                        {

                            List<ExamEnquiryResult> SortedList = workDataList.OrderBy(o => o.ExamDate).ToList();
                            message = "Few **" + mSubject + "Exams** found:\n\n";
                            foreach (ExamEnquiryResult res in SortedList)
                            {
                                message = message + "* **" + res.ExamDate.ToString(DATE_FORMAT) + "** at **08:00**\n\n";

                                //check proximity
                                int tempDateDiff = 99;
                                TimeSpan diff = res.ExamDate - DateTime.Now;
                                if (diff.TotalDays >= 0 && diff.TotalDays <= 7)
                                {
                                    if (tempDateDiff > diff.TotalDays)
                                    {
                                        tempDateDiff = int.Parse(Math.Floor(diff.TotalDays).ToString());
                                        nearExam = res;
                                        isNear = true;
                                    }

                                }
                            }
                        }

                        //continue chat
                        if (isNear == true)
                        {
                            //send back exam list
                            await context.PostAsync(message);

                            //say another word
                            message = "Wah! **" + nearExam.SubjectName + "** Exam on **" + nearExam.ExamDate.ToString(DATETIME_FORMAT) + "** is very near. Remember to study yah";
                        }


                    }
                    else if (hasSubject && hasSyllabus)
                    {
                        string mSubject = Mapping.GetInstance().getSubjectMapping(eSubject);

                        if (isWhen)
                        {
                            message = "You have to study all the time :)";
                        }
                        else
                        {
                            //get data
                            ExamEnquiry inq = new ExamEnquiry()
                            { Subject = mSubject };
                            List<ExamEnquiryResult> dataResList = DBHelper.GetInstance().GetExamList(inq);
                            //List<ExamEnquiryResult> dataResList = fake.getSubjectListTwo();

                            int count = dataResList.Count;
                            if (count == 0)
                            {
                                message = "Subject Name tooo chimz sia, something recognizable?";
                            }
                            else
                            {
                                //filter
                                Dictionary<string, ExamEnquiryResult> filteredMap = new Dictionary<string, ExamEnquiryResult>();
                                foreach (ExamEnquiryResult res in dataResList)
                                {
                                    if (!filteredMap.ContainsKey(res.SubjectName))
                                    {
                                        filteredMap.Add(res.SubjectName, res);
                                    }
                                }

                                //size handling
                                if (filteredMap.Count == 1)
                                {
                                    ExamEnquiryResult res = filteredMap.Values.First();
                                    message = "For **" + res.SubjectName + "** you can find the syllabus: **" + res.Syllabus + "**";
                                }
                                else
                                {
                                    message = "Few **Subjects** found: \n\n";
                                    foreach (ExamEnquiryResult res in filteredMap.Values)
                                    {
                                        message = message + "* " + res.SubjectName + " at " + res.Syllabus + "\n\n";
                                    }
                                }

                            }


                        }
                    }
                    else if (hasSyllabus && !hasSubject && !hasExamDate)
                    {
                        message = "Maybe... can provide more info on the **Subject** or **Exam Date** ?";
                    }
                    else if (!hasSubject && hasExamDate)
                    {
                        DateTime mExamDate;
                        if (eExamDate == null)
                        {
                            message = "hey, can try (**" + DATE_FORMAT + "**)? I can't digest haha...";
                            await context.PostAsync(message);
                            context.Wait(MessageReceived);
                            return;
                        }
                        else
                        {
                            mExamDate = eExamDate.Value;
                        }


                        //get data
                        ExamEnquiry inq = new ExamEnquiry()
                        {
                            StartDate = mExamDate.AddHours(-1),
                            EndDate = mExamDate.AddHours(10)
                        };
                        List<ExamEnquiryResult> dataResList = DBHelper.GetInstance().GetExamList(inq);
                        //List<ExamEnquiryResult> dataResList = fake.getSubjectListThree();

                        //filtering
                        Dictionary<string, ExamEnquiryResult> tempMap = new Dictionary<string, ExamEnquiryResult>();
                        foreach (ExamEnquiryResult res in dataResList)
                        {
                            if (!tempMap.Keys.Contains(res.SubjectName))
                            {
                                tempMap.Add(res.SubjectName, res);
                            }
                        }
                        List<ExamEnquiryResult> workDataList = tempMap.Values.ToList();
                        int count = workDataList.Count;

                        bool isNear = false;
                        ExamEnquiryResult nearExam = null;
                        if (count == 0)
                        {
                            message = "Shiok, no exam registered on that day, but it is always good to prepare";
                        }
                        else if (count == 1)
                        {

                            ExamEnquiryResult res = workDataList[0];
                            message = "There is **" + res.SubjectName + "** exam\n\non **" + res.ExamDate.ToString(DATE_FORMAT) + "** at **08:00** \n\n";

                            //check proximity
                            TimeSpan diff = res.ExamDate - DateTime.Now;
                            if (diff.TotalDays >= 0 && diff.TotalDays <= 7)
                            {
                                nearExam = res;
                                isNear = true;
                            }

                        }
                        else
                        {

                            List<ExamEnquiryResult> SortedList = workDataList.OrderBy(o => o.SubjectName).ToList();
                            message = "Few **Exams** found on " + mExamDate.ToString(DATE_FORMAT) + ":\n\n";
                            foreach (ExamEnquiryResult res in SortedList)
                            {
                                message = message + "* **" + res.SubjectName + "** at **08:00** \n\n";

                                //check proximity
                                TimeSpan diff = mExamDate - DateTime.Now;
                                if (diff.TotalDays >= 0 && diff.TotalDays <= 7)
                                {
                                    isNear = true;
                                }
                            }
                        }

                        //continue chat
                        if (isNear == true)
                        {
                            //send back exam list
                            await context.PostAsync(message);

                            //say another word
                            message = "Wah! All the Exam(s) is very near. Go study, go study!";
                        }
                    }
                    else if (hasSubject && hasExamDate)
                    {

                        DateTime mExamDate;
                        if (eExamDate == null)
                        {
                            message = "hey, can try (**" + DATE_FORMAT + "**)? I can't digest haha...";
                            await context.PostAsync(message);
                            context.Wait(MessageReceived);
                            return;
                        }
                        else
                        {
                            mExamDate = eExamDate.Value;
                        }
                        string mSubject = Mapping.GetInstance().getSubjectMapping(eSubject);

                        //get data
                        ExamEnquiry inq = new ExamEnquiry()
                        {
                            StartDate = mExamDate,
                            Subject = mSubject
                        };
                        List<ExamEnquiryResult> dataResList = DBHelper.GetInstance().GetExamList(inq);
                        //List<ExamEnquiryResult> dataResList = fake.getSubjectListOne();
                        int count = dataResList.Count;

                        bool isNear = false;
                        ExamEnquiryResult nearExam = null;
                        if (count == 0)
                        {
                            message = "Hmm... **no** ler... you might wanna try another subject or date?";
                        }
                        else if (count == 1)
                        {
                            ExamEnquiryResult res = dataResList[0];
                            message = "There is **" + res.SubjectName + "** exam\n\non **" + res.ExamDate.ToString(DATETIME_FORMAT) + "** at \n\n**" + res.SchoolName + "**";

                            //check proximity
                            TimeSpan diff = res.ExamDate - DateTime.Now;
                            if (diff.TotalDays >= 0 && diff.TotalDays <= 7)
                            {
                                nearExam = res;
                                isNear = true;
                            }

                        }
                        else
                        {

                            List<ExamEnquiryResult> SortedList = dataResList.OrderBy(o => o.ExamDate).ToList();
                            message = "Few **" + mSubject + "Exams** found:\n\n";
                            foreach (ExamEnquiryResult res in SortedList)
                            {
                                message = message + "* **" + res.ExamDate.ToString(DATETIME_FORMAT) + "** at " + res.SchoolName + "\n\n";

                                //check proximity
                                int tempDateDiff = 99;
                                TimeSpan diff = res.ExamDate - DateTime.Now;
                                if (diff.TotalDays >= 0 && diff.TotalDays <= 7)
                                {
                                    if (tempDateDiff > diff.TotalDays)
                                    {
                                        tempDateDiff = int.Parse(Math.Floor(diff.TotalDays).ToString());
                                        nearExam = res;
                                        isNear = true;
                                    }

                                }
                            }
                        }

                        //continue chat
                        if (isNear == true)
                        {
                            //send back exam list
                            await context.PostAsync(message);

                            //say another word
                            message = "Wah! **" + nearExam.SubjectName + "** Exam on **" + nearExam.ExamDate.ToString(DATETIME_FORMAT) + "** is very near. Remember to study yah";
                        }

                    }

                    if (message == null || message.Trim().Equals(""))
                    {
                        message = "Paiseh, buay hiaw... give me time, I promise I will learn and serve you better ...";
                    }

                }

            }
            catch(Exception ex)
            {
                message = ex.Message;
            }
            //send back response
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        public object getState(IDialogContext context, string key)
        {
            try
            {
                return context.UserData.Get<string>(key);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #endregion

        private Response GetResponse(Uri uri)
        {
            WebClient wc = new WebClient();
            System.IO.Stream str = wc.OpenRead(uri);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Response));
            Response ob = (Response)ser.ReadObject(str);
            return ob;
        }

        #region Andy's code 

        //[LuisIntent("getVenueInfo")]
        //public async Task getVenueInfo(IDialogContext context, LuisResult result)
        //{
        //    string message = "";

        //    //find the specific intention
        //    bool isEntityExist = result.Entities.Count > 0;
        //    //process
        //    if (!isEntityExist)
        //    {
        //        message = "Paiseh, simi ar?";
        //    }
        //    else
        //    {
        //        PromptDialog.Text(context, childCompleted, "What is your location?");
        //        foreach (EntityRecommendation enRec in result.Entities)
        //        {
        //            if (enRec.Type == "ExamSubject")
        //            {
        //                string eSubject = Mapping.GetInstance().getSubjectMapping(enRec.Entity);
        //                context.PrivateConversationData.SetValue("lres", eSubject);
        //            }
        //        }


        //    }
        //}

        [LuisIntent("getVenueInfo")]
        public async Task getVenueInfo(IDialogContext context, LuisResult result)
        {
            string eSubject = null;

            //find the specific intention
            bool isEntityExist = result.Entities.Count > 0;
            //process
            if (!isEntityExist)
            {
                eSubject = "Chemistry";
                //message = "Paiseh, simi ar?";
            }

            foreach (EntityRecommendation enRec in result.Entities)
            {
                if (enRec.Type == "ExamSubject")
                {
                    eSubject = enRec.Entity;
                    context.PrivateConversationData.SetValue("lres", (string)enRec.Entity);
                }
            }
            PromptDialog.Text(context, childCompleted, "What is your location?");
        }


        //private async Task childCompleted(IDialogContext context, IAwaitable<string> result)
        //{

        //    string key = ConfigurationManager.AppSettings["BingKey"].ToString();
        //    string query = await result;

        //    Uri geocodeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Locations?q={0}&key={1}", query, key));

        //    Response obj = GetResponse(geocodeRequest);
        //    double? lat = null;
        //    double? lon = null; 
        //    if(obj != null && obj.ResourceSets.Count() > 0 && obj.ResourceSets.FirstOrDefault().Resources.Count() >0)
        //    {
        //        BingRestServices.DataContracts.Location loc = (BingRestServices.DataContracts.Location)obj.ResourceSets.FirstOrDefault().Resources.FirstOrDefault();
        //        BingRestServices.DataContracts.Point pt = loc.Point;
        //        if (pt.Coordinates.Count() == 2)
        //        {
        //            lat = pt.Coordinates[0];
        //            lon = pt.Coordinates[1];
        //        }
        //    }

        //    string message = "";
        //    string rec = null;

        //    try
        //    {
        //        context.PrivateConversationData.TryGetValue("lres", out rec);
        //    }
        //    catch (Exception e)
        //    {
        //        await context.PostAsync("Bot encountered error: " + e.Message);
        //    }

        //    if (rec != null && lat != null && lon != null)
        //    {
        //        double locx = lat.Value/ 180 * Math.PI;
        //        double locy = lon.Value / 180 * Math.PI;

        //        var schoolist = new List<School>();
        //        //schoolist = distance(locx, locy);//for hardcoding

        //        #region Get list of schools 

        //        ExamEnquiry enq = new ExamEnquiry()
        //        {
        //            Subject = rec
        //        };
        //        List<ExamEnquiryResult> li = DBHelper.GetInstance().GetExamList(enq).Where(y=>y.SeatUsed < y.SeatMax).ToList();

        //        double dis, dlon, dlat, a, c;

        //        li.ForEach(x =>
        //        {

        //            //Haversine formula
        //            dlat = (double)x.SchLat / 180 * Math.PI - locx;
        //            dlon = (double)x.SchLon / 180 * Math.PI - locy;
        //            a = (Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2)) + Math.Cos((double)x.SchLat / 180 * Math.PI) * Math.Cos(locx) * (Math.Sin(dlon / 2)) * (Math.Sin(dlon / 2));
        //            c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        //            dis = 6371 * c; //distance from location to school1 in km

        //            schoolist.Add(new School { name = x.SchoolName, dis = Math.Round(dis, 2) });
        //        });
        //        #endregion

        //        if (schoolist.Count() == 0)
        //        {
        //            message = string.Format("There is no spare capacity for **{0}** in other EXAM CENTRES. \n" +
        //                "Please continue to your **own EXAM CENTRE** before the end of the paper. \n" +
        //                "You will be given the full duration.", rec);
        //            await context.PostAsync(message);
        //        }
        //        else
        //        {
        //            int count = 1;
        //            message = string.Format("We have found {0} closest locations for {1} based on your current location. \n 1 being the closest and {0} being the furthest from your location \n", schoolist.Count(), rec);
        //            await context.PostAsync(message);

        //            List<string> qlist = new List<string>();
        //            const string baseurlquery = "http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/";

        //            schoolist.OrderBy(o => o.dis).ToList().ForEach(x =>
        //            {
        //                message = string.Format(count + ". **{0}** is **{1} km** away. \n\n", x.name, x.dis);
        //                count ++;

        //                ExamEnquiryResult res = li.FirstOrDefault(y => y.SchoolName == x.name);
        //                string tmp = baseurlquery + string.Format("{0},{1}/15??mapSize=500,500&pp={2},{3};21;{4}&key={5}", res.SchLat, res.SchLon, res.SchLat, res.SchLon, count, key);
        //                List<CardImage> imgs = new List<CardImage>()
        //                {
        //                    new CardImage(tmp,null,null)
        //                };
        //                var msg = context.MakeMessage();
        //                msg.Attachments = new List<Attachment> { new HeroCard(null, null, message, imgs).ToAttachment() };
        //                context.PostAsync(msg);
        //            });
        //        }
        //    }
        //    else
        //    {
        //        message = Helper.ReadConfig().Message.FirstOrDefault(x=>x.ID== "LocationNotFound").ToString();
        //        await context.PostAsync(message);
        //    }
        //    context.Wait(MessageReceived);
        //}

        //public List<School> distance(double locx, double locy)
        //{
        //    //hardcode the school locations
        //    double school1x = 1.367353 / 180 * Math.PI;
        //    double school2x = 1.348712 / 180 * Math.PI;
        //    double school3x = 1.289453 / 180 * Math.PI;
        //    double school4x = 1.343172 / 180 * Math.PI;
        //    double school1y = 103.8426 / 180 * Math.PI;
        //    double school2y = 103.944485 / 180 * Math.PI;
        //    double school3y = 103.823742 / 180 * Math.PI;
        //    double school4y = 103.70949 / 180 * Math.PI;

        //    double dis1, dis2, dis3, dis4, dlon, dlat, a, c, d;

        //    //Haversine formula
        //    dlat = school1x - locx;
        //    dlon = school1y - locy;
        //    a = (Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2)) + Math.Cos(school1x) * Math.Cos(locx) * (Math.Sin(dlon / 2)) * (Math.Sin(dlon / 2));
        //    c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        //    dis1 = 6371 * c; //distance from location to school1 in km
        //                     //Haversine formula
        //    dlat = school2x - locx;
        //    dlon = school2y - locy;
        //    a = (Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2)) + Math.Cos(school2x) * Math.Cos(locx) * (Math.Sin(dlon / 2)) * (Math.Sin(dlon / 2));
        //    c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        //    dis2 = 6371 * c; //distance from location to school2 in km
        //                     //Haversine formula
        //    dlat = school3x - locx;
        //    dlon = school3y - locy;
        //    a = (Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2)) + Math.Cos(school3x) * Math.Cos(locx) * (Math.Sin(dlon / 2)) * (Math.Sin(dlon / 2));
        //    c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        //    dis3 = 6371 * c; //distance from location to school3 in km
        //                     //Haversine formula
        //    dlat = school4x - locx;
        //    dlon = school4y - locy;
        //    a = (Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2)) + Math.Cos(school4x) * Math.Cos(locx) * (Math.Sin(dlon / 2)) * (Math.Sin(dlon / 2));
        //    c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        //    dis4 = 6371 * c; //distance from location to school4 in km

        //    var schoolist = new List<School>();
        //    schoolist.Add(new School { name = "SchoolN", dis = Math.Round(dis1, 2) });
        //    schoolist.Add(new School { name = "SchoolE", dis = Math.Round(dis2, 2) });
        //    schoolist.Add(new School { name = "SchoolS", dis = Math.Round(dis3, 2) });
        //    schoolist.Add(new School { name = "SchoolW", dis = Math.Round(dis4, 2) });

        //    return schoolist;
        //}

        private async Task childCompleted(IDialogContext context, IAwaitable<string> result)
        {
            string key = ConfigurationManager.AppSettings["BingKey"].ToString();
            string query = await result + ", Singapore";

            Uri geocodeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Locations?q={0}&key={1}", query, key));

            Response obj = GetResponse(geocodeRequest);
            double? lat = null;
            double? lon = null;
            if (obj != null && obj.ResourceSets.Count() > 0 && obj.ResourceSets.FirstOrDefault().Resources.Count() > 0)
            {
                BingRestServices.DataContracts.Location loc = (BingRestServices.DataContracts.Location)obj.ResourceSets.FirstOrDefault().Resources.FirstOrDefault();
                BingRestServices.DataContracts.Point pt = loc.Point;
                if (pt.Coordinates.Count() == 2)
                {
                    lat = pt.Coordinates[0];
                    lon = pt.Coordinates[1];
                }
            }

            string message = "";
          
            double locx = lat.Value / 180 * Math.PI;
            double locy = lon.Value / 180 * Math.PI;
            //Retrieve global lres into rec
            string rec = null;
            try
            {
                context.PrivateConversationData.TryGetValue("lres", out rec);
            }
            catch (Exception e)
            {
                Console.WriteLine("Hello");
            }
            var schoolist = new List<schoolstat>();
            schoolist = distance(locx, locy, rec);


            #region original code 
            //message = string.Format("The 2 closest locations for {0} near **{1}** are. \n \n" +
            //    "1. **{2}** is **{3} km** away. \n" +
            //    "2. **{4}** is **{5} km** away. \n \n" +
            //    "You may head for these schools for the {6} paper in the event of a major transport disruption.", rec, loc.FormattedAddress,
            //    schoolist[0].NAME, Math.Round(schoolist[0].dist, 2), schoolist[1].NAME, Math.Round(schoolist[1].dist, 2), rec);

            //await context.PostAsync(message);
            //context.Wait(MessageReceived);
            #endregion

            if (schoolist.Count() == 0)
            {
                message = string.Format("There is no spare capacity for **{0}** in other EXAM CENTRES. \n" +
                    "Please continue to your **own EXAM CENTRE** before the end of the paper. \n" +
                    "You will be given the full duration.", rec);
                await context.PostAsync(message);
            }
            else
            {
                int count = 1;
                message = string.Format("We have found {0} closest locations for {1} based on your current location with 1 being the closest and {0} being the furthest from your location \n", Math.Min(3, schoolist.Count()), rec);
                await context.PostAsync(message);

                const string baseurlquery = "http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/";
                List<schoolstat> schli = schoolist.OrderBy(o => o.dist).ToList();
                for (int i = 0; i < schli.Count() && i < 3; ++i)
                {
                    schoolstat x = schli[i];
                    schoolstat res = schoolist.FirstOrDefault(y => y.NAME == x.NAME);
                    
                    string tmp = baseurlquery + string.Format("{0},{1}/15??mapSize=500,500&pp={0},{1};21;{2}&key={3}", 180 * res.LAT / Math.PI , 180 * res.LON/Math.PI , count, key);
                   

                    #region get location details 
                    //http://dev.virtualearth.net/REST/v1/Locations/47.64054,-122.12934?o=xml&key=BingMapsKey
                    Uri rr1 = new Uri(string.Format("http://dev.virtualearth.net/REST/v1/Locations/{0},{1}?key={2}", 180 * res.LAT / Math.PI, 180 * res.LON / Math.PI, key));

                    Response rr2 = GetResponse(rr1);
                    string addr = null;
                    if (rr2 != null && rr2.ResourceSets.Count() > 0 && rr2.ResourceSets.FirstOrDefault().Resources.Count() > 0)
                    {
                        BingRestServices.DataContracts.Location loc = (BingRestServices.DataContracts.Location)rr2.ResourceSets.FirstOrDefault().Resources.FirstOrDefault();
                        
                        addr = loc.Address.AddressLine + "," + loc.Address.CountryRegion;
                        message = string.Format(count + ". {0} is **{1} km** away @ {2} \n\n", x.NAME, Math.Round(x.dist, 2), addr);
                    }
                    #endregion 

                    List<CardImage> imgs = new List<CardImage>()
                    {
                        new CardImage(tmp,null,null)
                    };
                    var msg = context.MakeMessage();
                    msg.Attachments = new List<Attachment> { new HeroCard(null, null, message, imgs).ToAttachment() };
                    await context.PostAsync(msg);
                    count++;
                };
            }
            context.Wait(MessageReceived);
        }

        string toDeg(double rad)
        {
            int deg = (int)Math.Floor(rad);
            double rem = 60*(rad - deg);
            int minutes = (int)Math.Floor(rem);
            rem -= minutes;
            rem *= 60;
            float seconds = (float) Math.Round(rem, 1);
            return deg + "°" + " " + minutes + "'" + " " + seconds + "\"";
        }

        public List<schoolstat> distance(double locx, double locy, string rec)
        {
            //hardcode the schools into EGW
            var EGW = new List<schoolstat>();
            EGW.Add(new schoolstat { NAME = "Fajar Secondary", LAT = 1.36867 / 180 * Math.PI, LON = 103.770848 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = false, JAPANESE = true });
            EGW.Add(new schoolstat { NAME = "Hong Kah Secondary", LAT = 1.351759 / 180 * Math.PI, LON = 103.770848 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = true, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "MacPherson ", LAT = 1.327132 / 180 * Math.PI, LON = 103.888668 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = true, JAPANESE = true });
            EGW.Add(new schoolstat { NAME = "Yio Chu Kang Secondary", LAT = 1.377635 / 180 * Math.PI, LON = 103.842103 / 180 * Math.PI, ENGLISH = true, LITERATURE = false, FRENCH = false, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "Beatty Secondary", LAT = 1.341540 / 180 * Math.PI, LON = 103.770848 / 180 * Math.PI, ENGLISH = true, LITERATURE = false, FRENCH = true, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "Boon Lay Secondary", LAT = 1.342172 / 180 * Math.PI, LON = 103.701945 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = false, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "St. Andrew's Secondary", LAT = 1.331368 / 180 * Math.PI, LON = 103.865853 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = false, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "Dunman Secondary", LAT = 1.360673 / 180 * Math.PI, LON = 103.956206 / 180 * Math.PI, ENGLISH = true, LITERATURE = false, FRENCH = true, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "Mayflower Secondary", LAT = 1.368331 / 180 * Math.PI, LON = 103.835438 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = false, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "Pioneer Secondary", LAT = 1.347267 / 180 * Math.PI, LON = 103.694569 / 180 * Math.PI, ENGLISH = true, LITERATURE = false, FRENCH = false, JAPANESE = true });
            EGW.Add(new schoolstat { NAME = "Serangoon Secondary", LAT = 1.374435 / 180 * Math.PI, LON = 103.902512 / 180 * Math.PI, ENGLISH = true, LITERATURE = false, FRENCH = true, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "Chong Boon Secondary", LAT = 1.363994 / 180 * Math.PI, LON = 103.858355 / 180 * Math.PI, ENGLISH = true, LITERATURE = false, FRENCH = true, JAPANESE = true });
            EGW.Add(new schoolstat { NAME = "Jurongville Secondary", LAT = 1.351493 / 180 * Math.PI, LON = 103.729161 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = true, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "Bukit View Secondary", LAT = 1.34572 / 180 * Math.PI, LON = 103.752298 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = false, JAPANESE = true });
            EGW.Add(new schoolstat { NAME = "Chung Cheng High School (main)", LAT = 1.418737 / 180 * Math.PI, LON = 103.837311 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = false, JAPANESE = true });
            EGW.Add(new schoolstat { NAME = "Henderson Secondary", LAT = 1.284953 / 180 * Math.PI, LON = 103.819578 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = true, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "New Town Secondary", LAT = 1.309151 / 180 * Math.PI, LON = 103.773605 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = false, JAPANESE = true });
            EGW.Add(new schoolstat { NAME = "Peirce Secondary", LAT = 1.36613 / 180 * Math.PI, LON = 103.829903 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = false, JAPANESE = true });
            EGW.Add(new schoolstat { NAME = "Whitley Secondary", LAT = 1.355497 / 180 * Math.PI, LON = 103.843265 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = false, JAPANESE = true });
            EGW.Add(new schoolstat { NAME = "Yishun Secondary", LAT = 1.428129 / 180 * Math.PI, LON = 103.828833 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = true, JAPANESE = false });
            EGW.Add(new schoolstat { NAME = "Siglap Secondary", LAT = 1.37916 / 180 * Math.PI, LON = 103.939304 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = false, JAPANESE = true });
            EGW.Add(new schoolstat { NAME = "Yuying Secondary", LAT = 1.356754 / 180 * Math.PI, LON = 103.890092 / 180 * Math.PI, ENGLISH = true, LITERATURE = true, FRENCH = true, JAPANESE = false });

            switch (rec.ToUpper().Trim())
            {
                case "SCIENCE":
                    EGW = EGW.Where(i => i.ENGLISH == true).ToList();
                    break;
                case "CHEMISTRY":
                    EGW = EGW.Where(i => i.LITERATURE == true).ToList();
                    break;
                case "PHYSICS":
                    EGW = EGW.Where(i => i.FRENCH == true).ToList();
                    break;
                case "BIOLOGY":
                    EGW = EGW.Where(i => i.JAPANESE == true).ToList();
                    break;
                case "FRENCH":
                    EGW = EGW.Where(i => i.FRENCH == true).ToList();
                    break;
                default:
                    break;
            }

            double school1x;
            double school1y;
            double dis1, dis2, dis3, dis4, dlon, dlat, a, c;

            for (int i = 0; i < EGW.Count; i++)
            {
                school1x = EGW[i].LAT;
                school1y = EGW[i].LON;

                dlat = school1x - locx;
                dlon = school1y - locy;
                a = (Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2)) + Math.Cos(school1x) * Math.Cos(locx) * (Math.Sin(dlon / 2)) * (Math.Sin(dlon / 2));
                c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                EGW[i].dist = 6371 * c; //distance from location to school1 in km
            }
            EGW = EGW.OrderBy(o => o.dist).ToList();

            return EGW;
        }
    }

    //Make class school
    public class schoolstat
    {
        public string NAME { get; set; }
        public double LAT { get; set; }
        public double LON { get; set; }
        public bool ENGLISH { get; set; }
        public bool LITERATURE { get; set; }
        public bool FRENCH { get; set; }
        public bool JAPANESE { get; set; }
        public double dist { get; set; }
    }
    #endregion


}