namespace FakeItEasy.Benchmark
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Running;
    using BenchmarkDotNet.Exporters;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Toolchains.CsProj;
    using System.Linq;
    using BenchmarkDotNet.Exporters.Csv;
    using System.Collections.Generic;

    public interface IFake
    {
        int IntReturnWith0Parameters();
        int IntReturnWith1IntParameter(int i);
        int IntReturnWith1ObjectParameter(object o);
        int IntReturnWith1OutIntParameter(out int i);
        int IntReturnWithEnumerableParameter(IEnumerable<int> ints);
    }

    public static class ArgumentExtension
    {
        public static int IsEqualTo7(this IArgumentConstraintManager<int> manager)
        {
            return manager.IsEqualTo(7);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var filter = "--filter=*";
            if (args[0].StartsWith("--filter="))
            {
                filter = args[0];
                args = args.Skip(1).ToArray();
            }

            var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                .Run(new[] { filter }, new Config(args));
        }

        private class Config : ManualConfig
        {
            public Config(string[] versions)
            {
                // Specify jobs with different versions of the same Nuget package to benchmark.
                // The Nuget versions referenced on these jobs must be greater or equal to the 
                // same Nuget version referenced in this benchmark project.
                for (int i = 0; i < versions.Length; ++i)
                {
                    var job = Job.MediumRun
                    // .WithInvocationCount(100_000)
                    .WithIterationCount(10)
                    .WithLaunchCount(1)
                    // .WithWarmupCount(1)
                    .With(CsProjCoreToolchain.Current.Value)
                    .WithNuGet("FakeItEasy", versions[i]).WithId(versions[i]);

                    if (i == 0)
                    {
                        job = job.AsBaseline();
                    }

                    Add(job);
                }

                Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
                Add(DefaultConfig.Instance.GetLoggers().ToArray());
                Add(CsvExporter.Default);
                Add(MarkdownExporter.GitHub);
                // Add(RPlotExporter.Default);
            }
        }

        public class ReturnsBenchmarks
        {
            private IFake fake = A.Fake<IFake>();

            [GlobalSetup]
            public void Init()
            {
                this.fake = A.Fake<IFake>();
                A.CallTo(() => fake.IntReturnWith0Parameters()).Returns(1);
            }

            [Benchmark()]
            public int Returns()
            {
                return fake.IntReturnWith0Parameters();
            }
        }

        public class ArgumentConstraintBenchmarks
        {
            private IFake fake = A.Fake<IFake>();

            [GlobalSetup]
            public void Init()
            {
                A.Fake<IFake>();
            }

            [Benchmark]
            public void NoParams()
            {
                A.CallTo(() => this.fake.IntReturnWith0Parameters()).Returns(1);
            }

            [Benchmark(Description = "A<int>._")]
            public void Underscore()
            {
                A.CallTo(() => this.fake.IntReturnWith1IntParameter(A<int>._)).Returns(1);
            }

            [Benchmark(Description = "7")]
            public void Literal()
            {
                A.CallTo(() => this.fake.IntReturnWith1IntParameter(7)).Returns(1);
            }

            [Benchmark(Description = "That.IsEqualTo(7)")]
            public void ThatIsEqualTo()
            {
                A.CallTo(() => this.fake.IntReturnWith1IntParameter(A<int>.That.IsEqualTo(7))).Returns(1);
            }

            [Benchmark(Description = "That.IsEqualTo7()")]
            public void ThatIsEqualTo7()
            {
                A.CallTo(() => this.fake.IntReturnWith1IntParameter(A<int>.That.IsEqualTo7())).Returns(1);
            }

            [Benchmark(Description = "That.IsGreaterThan(7)")]
            public void ThatIsGreaterThan()
            {
                A.CallTo(() => this.fake.IntReturnWith1IntParameter(A<int>.That.IsGreaterThan(7))).Returns(1);
            }

            [Benchmark(Description = "That.Matches(i => i == 7)")]
            public void ThatMatches()
            {
                A.CallTo(() => this.fake.IntReturnWith1IntParameter(A<int>.That.Matches(i => i == 7))).Returns(1);
            }

            [Benchmark(Description = "That.IsNull()")]
            public void ThatIsNull()
            {
                A.CallTo(() => this.fake.IntReturnWith1ObjectParameter(A<object>.That.IsNull())).Returns(1);
            }

            [Benchmark(Description = "That.Not.IsNull()")]
            public void ThatNotIsNull()
            {
                A.CallTo(() => this.fake.IntReturnWith1ObjectParameter(A<object>.That.Not.IsNull())).Returns(1);
            }

            [Benchmark(Description = "That.IsSameAs(this)")]
            public void ThatIsSameAs()
            {
                A.CallTo(() => this.fake.IntReturnWith1ObjectParameter(A<object>.That.IsSameAs(this))).Returns(1);
            }

            [Benchmark(Description = "That.IsInstanceOf(typeof(string))")]
            public void ThatIsInstanceOf()
            {
                A.CallTo(() => this.fake.IntReturnWith1ObjectParameter(A<object>.That.IsInstanceOf(typeof(string)))).Returns(1);
            }

            [Benchmark]
            public void Out()
            {
                int i;
                A.CallTo(() => this.fake.IntReturnWith1OutIntParameter(out i)).Returns(1);
            }

            [Benchmark(Description = "That.IsSameSequenceAs(Enumerable.Range(1, 5))")]
            public void EnumerableComparedAgainstEnumerable()
            {
                A.CallTo(() => this.fake.IntReturnWithEnumerableParameter(A<IEnumerable<int>>.That.IsSameSequenceAs(Enumerable.Range(1, 5)))).Returns(1);
            }

            [Benchmark(Description = "That.IsSameSequenceAs(1, 2, 3, 4, 5)")]
            public void EnumerableComparedAgainstParams()
            {
                A.CallTo(() => this.fake.IntReturnWithEnumerableParameter(A<IEnumerable<int>>.That.IsSameSequenceAs(1, 2, 3, 4, 5))).Returns(1);
            }
        }
    }
}