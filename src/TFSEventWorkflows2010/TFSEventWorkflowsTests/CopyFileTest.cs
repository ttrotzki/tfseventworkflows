using artiso.TFSEventWorkflows.TFSActivitiesLib.Workspace_Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;

namespace TFSEventWorkflowsTests
{
    using System.IO;

    /// <summary>
    ///This is a test class for CopyFileTest and is intended
    ///to contain all CopyFileTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CopyFileTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        [DeploymentItem("artiso.TFSEventWorkflows.TFSActivitiesLib.dll")]
        public void ExecuteCopyFileTest()
        {
            string targetPath = @"C:\Temp\Targetfolder";
            var activity = new CopyFile();
            activity.SourcePath = @"C:\Temp\Targetfolder\aaa\bbb\ccc\New Microsoft Word Document.docx";
            activity.TargetPath = targetPath;
            WorkflowInvoker.Invoke(activity);

            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "New Microsoft Word Document.docx")));
        }
    }
}
