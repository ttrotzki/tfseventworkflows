using artiso.TFSEventWorkflows.TFSActivitiesLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Client;

namespace TFSEventWorkflowsTests
{
    
    
    /// <summary>
    ///This is a test class for GetParentWorkItemTest and is intended
    ///to contain all GetParentWorkItemTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GetParentWorkItemTest
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
        ///A test for GetParentWorkitem
        ///</summary>
        [TestMethod()]
        [DeploymentItem("artiso.TFSEventWorkflows.TFSActivitiesLib.dll")]
        public void GetParentWorkitemTest()
        {
            var tfs = new TfsTeamProjectCollection(new Uri("http://localhost:8080/tfs/defaultcollection"));
            WorkItemStore workItemStore = tfs.GetService<WorkItemStore>();
            var workItem = workItemStore.GetWorkItem(85);

            // TODO: private accessors using "Test References | .accessor" are deprecated
            // use reflection to call Invoke directly
            //GetParentWorkItem_Accessor target = new GetParentWorkItem_Accessor(); 
            //WorkItem expected = null; // TODO: Initialize to an appropriate value
            //WorkItem actual;
            //actual = target.GetParentWorkitem(workItem);
            //Assert.AreEqual(expected, actual);
        }
    }
}
