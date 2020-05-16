using NUnit.Framework;

namespace AnalysisUK.Tinamous.Media.BL.Tests
{
    [SetUpFixture]
    public class SetUpFixture
    {
        [SetUp]
        public void Setup()
        {
            AutoMapperConfiguration.Configure();
        }
    }
}