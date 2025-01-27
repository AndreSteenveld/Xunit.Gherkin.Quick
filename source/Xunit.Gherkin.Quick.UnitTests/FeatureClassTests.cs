using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace UnitTests
{
    public sealed class FeatureClassTests
    {
        [Fact]
        public void FromFeatureInstance_Creates_FeatureClass_With_Default_FilePath_If_No_Attribute()
        {
            //arrange.
            var featureInstance = new FeatureWithoutFilePath();

            //act.
            var sut = FeatureClass.FromFeatureInstance(featureInstance);

            //assert.
            Assert.NotNull(sut);
            Assert.Equal($"{nameof(FeatureWithoutFilePath)}.feature", sut.FeatureFilePath);
        }

        private sealed class FeatureWithoutFilePath : Feature
        {
        }

        [Fact]
        public void FromFeatureInstance_Creates_FeatureClass_With_FilePath_From_Attribute()
        {
            //arrange.
            var featureInstance = new FeatureWithFilePath();

            //act.
            var sut = FeatureClass.FromFeatureInstance(featureInstance);

            //assert.
            Assert.NotNull(sut);
            Assert.Equal(FeatureWithFilePath.PathFor_FeatureWithFilePath, sut.FeatureFilePath);
        }

        [FeatureFile(PathFor_FeatureWithFilePath)]
        private sealed class FeatureWithFilePath : Feature
        {
            public const string PathFor_FeatureWithFilePath = "some file path const 123";
        }

        [Fact]
        public void FromFeatureInstance_DoesNotAllow_AsyncVoid_Steps()
        {
            var featureInstance = new FeatureWithAsyncVoidStep();

            //act / assert.
            Assert.Throws<InvalidOperationException>(() => FeatureClass.FromFeatureInstance(featureInstance));
        }

        private sealed class FeatureWithAsyncVoidStep : Feature
        {
            [Given("Any step text")]
            public async void StepWithAsyncVoid()
            {
                await Task.CompletedTask;
            }
        }

        [Fact]
        public void ExtractScenario_Extracts_Scenario_Without_Background()
        {
            //arrange.
            var scenarioName = "some scenario name 123";
            var featureInstance = new FeatureWithMatchingScenarioStepsToExtract();
            var sut = FeatureClass.FromFeatureInstance(featureInstance);
            var gherkinScenario = CreateGherkinDocument(scenarioName,
                new string[]
                {
                    "Given " + FeatureWithMatchingScenarioStepsToExtract.ScenarioStep1Text.Replace(@"(\d+)", "12", StringComparison.InvariantCultureIgnoreCase),
                    "And " + FeatureWithMatchingScenarioStepsToExtract.ScenarioStep2Text.Replace(@"(\d+)", "15", StringComparison.InvariantCultureIgnoreCase),
                    "When " + FeatureWithMatchingScenarioStepsToExtract.ScenarioStep3Text,
                    "Then " + FeatureWithMatchingScenarioStepsToExtract.ScenarioStep4Text.Replace(@"(\d+)", "27", StringComparison.InvariantCultureIgnoreCase)
                }).Feature.Children.First() as Gherkin.Ast.Scenario;

            //act.
            var scenario = sut.ExtractScenario(gherkinScenario);

            //assert.
            Assert.NotNull(scenario);
        }		

        private static Gherkin.Ast.GherkinDocument CreateGherkinDocument(
            string scenario,
            string[] steps,
            Gherkin.Ast.StepArgument stepArgument = null,
            string[] backgroundSteps = null)
        {

            var definitions = new List<global::Gherkin.Ast.StepsContainer>
            {
                new Gherkin.Ast.Scenario(
                        new Gherkin.Ast.Tag[0],
                        null,
                        "Scenario",
                        scenario,
                        null,
                        steps.Select(s =>
                        {
                            var spaceIndex = s.IndexOf(' ');
                            return new Gherkin.Ast.Step(
                                null,
                                s.Substring(0, spaceIndex).Trim(),
                                s.Substring(spaceIndex).Trim(),
                                stepArgument);
                        }).ToArray(),
                        Array.Empty<global::Gherkin.Ast.Examples>())
            };

            if (backgroundSteps != null)
            {
                definitions.Add(
                    new Gherkin.Ast.Background(
                        null,
                        null,
                        "background",
                        null,
                        backgroundSteps.Select(s =>
                        {
                            var spaceIndex = s.IndexOf(' ');
                            return new Gherkin.Ast.Step(
                                null,
                                s.Substring(0, spaceIndex).Trim(),
                                s.Substring(spaceIndex).Trim(),
                                stepArgument);
                        }).ToArray()));
            }

            return new Gherkin.Ast.GherkinDocument(
                new Gherkin.Ast.Feature(new Gherkin.Ast.Tag[0], null, null, null, null, null, definitions.ToArray()),
                new Gherkin.Ast.Comment[0]);
        }

        private sealed class FeatureWithMatchingScenarioStepsToExtract : Feature
        {
            public List<KeyValuePair<string, object[]>> CallStack { get; } = new List<KeyValuePair<string, object[]>>();

            [Given("a background step")]
            public void GivenBackground()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(GivenBackground), null));
            }

            [Given("Non matching given")]
            public void NonMatchingStep1_before()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep1_before), null));
            }

            public const string ScenarioStep1Text = @"I chose (\d+) as first number";

            [Given(ScenarioStep1Text)]
            public void ScenarioStep1(int firstNumber)
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(ScenarioStep1), new object[] { firstNumber }));
            }

            [Given("Non matching given")]
            public void NonMatchingStep1_after()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep1_after), null));
            }

            [And("Non matching and")]
            public void NonMatchingStep2_before()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep2_before), null));
            }

            public const string ScenarioStep2Text = @"I chose (\d+) as second number";

            [And(ScenarioStep2Text)]
            public void ScenarioStep2(int secondNumber)
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(ScenarioStep2), new object[] { secondNumber }));
            }

            [And("Non matching and")]
            public void NonMatchingStep2_after()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep2_after), null));
            }

            [When("Non matching when")]
            public void NonMatchingStep3_before()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep3_before), null));
            }

            public const string ScenarioStep3Text = "I press add";

            [When(ScenarioStep3Text)]
            public void ScenarioStep3()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(ScenarioStep3), null));
            }

            [When("Non matching when")]
            public void NonMatchingStep3_after()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep3_after), null));
            }

            [Then("Non matching then")]
            public void NonMatchingStep4_before()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep4_before), null));
            }

            public const string ScenarioStep4Text = @"the result should be (\d+) on the screen";

            [Then(ScenarioStep4Text)]
            public void ScenarioStep4(int result)
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(ScenarioStep4), new object[] { result }));
            }

            [Then("Non matching then")]
            public void NonMatchingStep4_after()
            {
                CallStack.Add(new KeyValuePair<string, object[]>(nameof(NonMatchingStep4_after), null));
            }
        }

        [Fact]
        public void ExtractScnario_Extracts_Scenario_With_DataTable()
        {
            //arrange.
            var scenarioName = "scenario213";
            var featureInstance = new FeatureWithDataTableScenarioStep();
            var sut = FeatureClass.FromFeatureInstance(featureInstance);

            var gherknScenario = CreateGherkinDocument(scenarioName,
                    new string[]
                    {
                        "When " + FeatureWithDataTableScenarioStep.Steptext
                    },
                    new Gherkin.Ast.DataTable(new Gherkin.Ast.TableRow[]
                    {
                        new Gherkin.Ast.TableRow(null, new Gherkin.Ast.TableCell[]
                        {
                            new Gherkin.Ast.TableCell(null, "First argument"),
                            new Gherkin.Ast.TableCell(null, "Second argument"),
                            new Gherkin.Ast.TableCell(null, "Result"),
                        }),
                        new Gherkin.Ast.TableRow(null, new Gherkin.Ast.TableCell[]
                        {
                            new Gherkin.Ast.TableCell(null, "1"),
                            new Gherkin.Ast.TableCell(null, "2"),
                            new Gherkin.Ast.TableCell(null, "3"),
                        }),
                        new Gherkin.Ast.TableRow(null, new Gherkin.Ast.TableCell[]
                        {
                            new Gherkin.Ast.TableCell(null, "a"),
                            new Gherkin.Ast.TableCell(null, "b"),
                            new Gherkin.Ast.TableCell(null, "c"),
                        })
                    })).Feature.Children.First() as Gherkin.Ast.Scenario;

            //act.
            var scenario = sut.ExtractScenario(gherknScenario);

            //assert.
            Assert.NotNull(scenario);
        }

        private sealed class FeatureWithDataTableScenarioStep : Feature
        {
            public Gherkin.Ast.DataTable ReceivedDataTable { get; private set; }

            public const string Steptext = "Some step text";

            [When(Steptext)]
            public void When_DataTable_Is_Expected(Gherkin.Ast.DataTable dataTable)
            {
                ReceivedDataTable = dataTable;
            }
        }

        [Fact]
        public void ExtractScenario_Extracts_Scenario_With_DocString()
        {
            //arrange.
            var featureInstance = new FeatureWithDocStringScenarioStep();
            var sut = FeatureClass.FromFeatureInstance(featureInstance);
            var scenarioName = "scenario-121kh2";
            var docStringContent = @"some content
    +++
    with multi lines
    ---
    in it";
            var gherkinScenario = CreateGherkinDocument(scenarioName,
                    new string[] { "Given " + FeatureWithDocStringScenarioStep.StepWithDocStringText },
                    new Gherkin.Ast.DocString(null, null, docStringContent))
                    .Feature.Children.First() as Gherkin.Ast.Scenario;

            //act.
            var scenario = sut.ExtractScenario(gherkinScenario);

            //assert.
            Assert.NotNull(scenario);
        }

        private sealed class FeatureWithDocStringScenarioStep : Feature
        {
            public Gherkin.Ast.DocString ReceivedDocString { get; private set; }

            public const string StepWithDocStringText = "Step with docstirng";

            [Given(StepWithDocStringText)]
            public void Step_With_DocString_Argument(Gherkin.Ast.DocString docString)
            {
                ReceivedDocString = docString;
            }
        }

        [Fact]
        public void ExtractScenario_Extracts_Steps_With_Multiple_Patterns()
        {
            //arrange.
            var sut = FeatureClass.FromFeatureInstance(new FeatureWithMultipleStepPatterns());

            //act.
            var scenario = sut.ExtractScenario(CreateGherkinDocument("scenario 123", new string[]
            {
                "Given something else"
            }).Feature.Scenarios().First());

            //assert.
            Assert.NotNull(scenario);
        }

        private sealed class FeatureWithMultipleStepPatterns : Feature
        {
            [Given("something")]
            [Given("something else")]
            [And("something")]
            [And("something else")]
            [When("something")]
            [When("something else")]
            [And("something")]
            [And("something else")]
            [But("something")]
            [But("something else")]
            public void Step_With_Multiple_Patterns()
            { }
        }
    }
}
