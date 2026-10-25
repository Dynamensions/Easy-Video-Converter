using Dynamensions.EasyVideoConverter.Core.Processes;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dynamensions.EasyVideoConverter.Tests
{
    public class TranscodeServiceTests
    {
        [Fact]
        public void TranscodeService_Constructor_ShouldInitializeLoggerAndEncodeTimeEstimator()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Core.Services.ITranscodeService>>();
            var transcodeMock = new Mock<ITranscodeService>();
            var encodeTimeEstimatorMock = new Mock<EncodeTimeEstimatorProcess>();

        }

    }
}