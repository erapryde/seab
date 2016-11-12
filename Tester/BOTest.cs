using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessObjects;

namespace Tester
{
    [TestClass]
    public class BOTest
    {
        [TestMethod]
        public void TestSubject()
        {
            DBHelper helper = DBHelper.GetInstance();
            ExamEnquiry inq = new ExamEnquiry()
            {
                 Subject = "Chemistry (SPA)"
            };
            int count = helper.GetExamList(inq).Count;
            Assert.AreEqual(24, count);
        }
    }
}
