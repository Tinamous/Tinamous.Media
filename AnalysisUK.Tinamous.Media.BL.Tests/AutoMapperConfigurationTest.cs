using NUnit.Framework;

namespace AnalysisUK.Tinamous.Alerts.BL.Tests
{
    [TestFixture]
    public class AutoMapperConfigurationTest
    {
        [Test]
        public void Automapping_IsValid()
        {
            // Arrange
            AutoMapper.Mapper.AssertConfigurationIsValid();
        }
    }
}