using artiso.TFSEventWorkflows.TFSActivitiesLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;

namespace TFSEventWorkflowsTests
{
    using System.Activities.Expressions;

    /// <summary>
    ///This is a test class for CreateWorkspaceTest and is intended
    ///to contain all CreateWorkspaceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CreateWorkspaceTest
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
        public void ExecuteCreateWorkspaceTest()
        {
            var activity = new CreateWorkspace();
            activity.LocalPath = @"C:\TFSUpload";
            activity.ServerPath = "$/Markus_Test/UploadTest2";
            activity.TFSCollectionUrl = new InArgument<Uri>(new LambdaValue<Uri>((env) => (new Uri("http://localhost:8080/tfs/DefaultCollection"))));
            activity.WorkspaceName = "TestMethodWorkspace";
            activity.WorkspaceComment = "TestMethodWorkspace";
            var output = WorkflowInvoker.Invoke(activity);
            Assert.IsNotNull(output["Workspace"]);
        }
    }
}
