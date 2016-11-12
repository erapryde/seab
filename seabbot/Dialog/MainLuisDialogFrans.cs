using BusinessObjects;
using ExamBot.Const;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ExamBot.Dialog
{
    [LuisModel("6dcb9387-f3f9-4613-9332-f52b226bda97", "f0264918b26f4f838bac2123e9092768")]
    [Serializable]
    public class MainLuisDialog : LuisDialog<object>
    {

        public const string DATE_FORMAT = "dd-MMM-yyyy";
        public const string DATETIME_FORMAT = "dd-MMM-yyyy HH:mm";
        public const string TIME_FORMAT = "HH:mm";

        public Mapping mapping = new Mapping();
        private DBHelper helper = DBHelper.GetInstance();
        private FakeData fake = new FakeData();


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
                    }else if (enRec.Type == "ExamSubject")
                    {
                        hasSubject = true;
                        eSubject = enRec.Entity;
                    }else if (enRec.Type == "ExamDate")
                    {
                        if (enRec.Entity.ToUpper() == "WHEN")
                        {
                            hasExamDate = false;
                            isWhen = true;
                        }else
                        {
                            hasExamDate = true;
                            isWhen = false;
                        }
                        
                        eExamDate = mapping.getDateMapping(enRec.Entity);

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
            }else
            {
                if (isExam && !hasSubject && !hasExamDate && !hasSyllabus)
                {
                    message = "Maybe... can provide more info on the **Subject** or **Exam Date** ?";
                }
                else if (hasSubject && !hasExamDate && !hasSyllabus)
                {
                    DateTime mExamDate = DateTime.Now;
                    string mSubject = mapping.getSubjectMapping(eSubject);

                    //get data
                    ExamEnquiry inq = new ExamEnquiry()
                    {Subject = mSubject};
                    List<ExamEnquiryResult> dataResList = helper.GetExamList(inq);
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
                    }else if (count == 1)
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
                        message = "Few **"+ mSubject +"Exams** found:\n\n";
                        foreach (ExamEnquiryResult res in SortedList)
                        {
                            message = message + "* **" + res.ExamDate.ToString(DATE_FORMAT) + "** at **08:00**\n\n";

                            //check proximity
                            int tempDateDiff = 99;
                            TimeSpan diff = res.ExamDate - DateTime.Now;
                            if (diff.TotalDays >= 0 && diff.TotalDays<=7)
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
                        context.Wait(MessageReceived);

                        //say another word
                        message = "Wah! **" + nearExam.SubjectName + "** Exam on **" + nearExam.ExamDate.ToString(DATETIME_FORMAT) + "** is very near. Remember to study yah";
                    }


                }
                else if (hasSubject && hasSyllabus)
                {
                    string mSubject = mapping.getSubjectMapping(eSubject);

                    if (isWhen)
                    {
                        message = "You have to study all the time :)";
                    }
                    else
                    {
                        //get data
                        ExamEnquiry inq = new ExamEnquiry()
                        { Subject = mSubject };
                        List<ExamEnquiryResult> dataResList = helper.GetExamList(inq);
                        //List<ExamEnquiryResult> dataResList = fake.getSubjectListTwo();

                        int count = dataResList.Count;
                        if (count == 0)
                        {
                            message = "Subject Name tooo chimz sia, something recognizable?";
                        }
                        else 
                        {
                            //filter
                            Dictionary<string,ExamEnquiryResult> filteredMap = new Dictionary<string,ExamEnquiryResult>();
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
                    }else
                    {
                        mExamDate = eExamDate.Value;
                    }
                                       
                    
                    //get data
                    ExamEnquiry inq = new ExamEnquiry()
                    { StartDate = mExamDate.AddHours(-1),
                    EndDate = mExamDate.AddHours(10)};
                    List<ExamEnquiryResult> dataResList = helper.GetExamList(inq);
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
                        context.Wait(MessageReceived);

                        //say another word
                        message = "Wah! All the Exam(s) is very near. Go study, go study!";
                    }
                }
                else if (hasSubject && hasExamDate) {

                    DateTime mExamDate;
                    if (eExamDate == null)
                    {
                        message = "hey, can try (**"+ DATE_FORMAT +"**)? I can't digest haha...";
                        await context.PostAsync(message);
                        context.Wait(MessageReceived);
                        return;
                    }
                    else
                    {
                        mExamDate = eExamDate.Value;
                    }
                    string mSubject = mapping.getSubjectMapping(eSubject);

                    //get data
                    ExamEnquiry inq = new ExamEnquiry()
                    { StartDate = mExamDate,
                        Subject = mSubject
                    };
                    List<ExamEnquiryResult> dataResList = helper.GetExamList(inq);
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
                        context.Wait(MessageReceived);

                        //say another word
                        message = "Wah! **" + nearExam.SubjectName + "** Exam on **" + nearExam.ExamDate.ToString(DATETIME_FORMAT) + "** is very near. Remember to study yah";
                    }

             

                                        
                }

                if (message==null || message.Trim().Equals(""))
                {
                    message = "Paiseh, buay hiaw... give me time, I promise I will learn and serve you better ...";
                }
           
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
            }catch (Exception e)
            {
                return null;
            }
        }
        

    }
}