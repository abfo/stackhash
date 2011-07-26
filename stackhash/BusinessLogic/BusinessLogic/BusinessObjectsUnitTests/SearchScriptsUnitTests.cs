using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackHashBusinessObjects;

namespace BusinessObjectsUnitTests
{
    /// <summary>
    /// Summary description for SearchScriptsUnitTests
    /// </summary>
    [TestClass]
    public class SearchScriptsUnitTests
    {
        public SearchScriptsUnitTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Contains_1Command_0Lines_NoMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Hello", "Hello", false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_1Command_1Lines_NoMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"), 
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Hello", "Hello", false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_1Command_1Lines_Match()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "line", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_1Command_2Lines_MatchInFirstLine()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Another result line");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Output", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_1Command_2Lines_MatchInSecondLine()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Another result line");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "result", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_1Command_2Lines_MatchInBothLines()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Another result line");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "line", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_1Command_2Lines_NoMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Another result line");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "lines", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_1Command_1Lines_2SearchOptions_NoMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Hello", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "there", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_1Command_1Lines_2SearchOptions_FirstMatches()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Output", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "there", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_1Command_1Lines_2SearchOptions_SecondMatches()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Output2", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Output", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_1Command_1Lines_2SearchOptions_BothMatches()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Out", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "put", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_2Command_1Lines_FirstCmdMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Cmd1 line1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Cmd2 line1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd1", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_2Command_1Lines_SecondCmdMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Cmd1 line1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Cmd2 line1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd2", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_2Command_1Lines_BothCmdMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Cmd1 line1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Cmd2 line1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_2Command_1Lines_NoCmdMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Cmd1 line1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Cmd2 line1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd3", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_2Command_1Lines_2SearchOptions_FirstOptionMatches()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Cmd1 line1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Cmd2 line1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd1", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd3", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_2Command_1Lines_2SearchOptions_SecondOptionMatches()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Cmd1 line1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Cmd2 line1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd3", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd1", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_2Command_1Lines_2SearchOptions_NoOptionMatches()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Cmd1 line1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Cmd2 line1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd3", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd4", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void Contains_2Command_1Lines_2SearchOptions_BothOptionsMatche()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Cmd1 line1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[1].ScriptLineOutput.Add("Cmd2 line1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd1", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "Cmd2", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContain_1Command_0Lines_NoMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "Hello", "Hello", false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContain_1Command_1Lines_NoMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "Hello", "Hello", false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContain_2Command_1Lines_FirstCmdMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "1", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContain_2Command_1Lines_SecondCmdMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "2", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContain_2Command_1Lines_BothCmdMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "line", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContain_2Command_1Lines_NeitherCmdMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command2", "Comment2"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "Nomatch", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContain_1Command_2Lines_NoMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "Hello", "Hello", false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContain_1Command_2Lines_FirstLineMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "1", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContain_1Command_2Lines_SecondLineMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "2", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }
        [TestMethod]
        public void DoesNotContain_1Command_2Lines_BothLineMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "line", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }
        [TestMethod]
        public void DoesNotContain_1Command_2Lines_NeitherLineMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "3", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContainAndContains_1Command_2Lines_ContainsMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "2", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "1", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContainAndContains_1Command_2Lines_NotContainsMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "4", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "23", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }
        [TestMethod]
        public void DoesNotContainAndContains_1Command_2Lines_NotContainsAndContainsMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "2", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "23", null, false));

            Assert.AreEqual(true, scriptResult.Search(criteria));
        }

        [TestMethod]
        public void DoesNotContainAndContains_1Command_2Lines_NotContainsAndContainsDoNotMatch()
        {
            StackHashScriptResult scriptResult = new StackHashScriptResult();
            scriptResult.ScriptResults = new StackHashScriptLineResults();

            scriptResult.ScriptResults.Add(new StackHashScriptLineResult(new StackHashScriptLine("Command1", "Comment1"),
                new StackHashScriptLineOutput()));
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 1");
            scriptResult.ScriptResults[0].ScriptLineOutput.Add("Output line 2");

            StackHashSearchCriteria criteria = new StackHashSearchCriteria(new StackHashSearchOptionCollection());
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringContains, "4", null, false));
            criteria.SearchFieldOptions.Add(new StringSearchOption(
                StackHashObjectType.Script, "Script", StackHashSearchOptionType.StringDoesNotContain, "1", null, false));

            Assert.AreEqual(false, scriptResult.Search(criteria));
        }
    }
}
