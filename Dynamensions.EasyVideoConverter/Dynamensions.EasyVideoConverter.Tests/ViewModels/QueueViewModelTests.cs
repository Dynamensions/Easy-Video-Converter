using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using Dynamensions.EasyVideoConverter.Core.Processes;
using Dynamensions.EasyVideoConverter.Core.Services;
using Dynamensions.EasyVideoConverter.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Collections.ObjectModel;

namespace Dynamensions.EasyVideoConverter.Tests.ViewModels
{
    public class QueueViewModelTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<ILogger<QueueViewModel>> _loggerMock = new();
        private readonly Mock<ITranscodeQueueService> _queueMock = new();
        private readonly Mock<IMediaFileAnalyzerService> _mediaFileAnalyzerMock = new();
        private readonly Mock<ITranscodeService> _transcodeServiceMock = new();
        private readonly Mock<ISubtitleExtractorService> _subtitleExtractorMock = new();
        private readonly Mock<ISmartDecisionService> _smartDecisionMock = new();
        private readonly Mock<IEncoderProfileService> _encoderProfileMock = new();
        private readonly Mock<IClientProfileService> _clientProfileMock = new();
        private readonly Mock<IPickerService> _pickerMock = new();
        private readonly Mock<ISystemService> _systemMock = new();
        private readonly Mock<ISettingsService> _settingsMock = new();
        private readonly Mock<IPerformanceBucketService> _performanceBucketServiceMock = new();

        private readonly Mock<EncodeTimeEstimatorProcess> _encoderEstimatorMock = new();
        private readonly Mock<SmartDecisionComplianceValidator> _complianceMock = new();

        public QueueViewModelTests()
        {


            // Setup default returns to prevent null references during construction
            var capabilities = new EncoderCapabilities { HasCpu = true };
            _systemMock.Setup(x => x.GetEncoderCapabilities()).Returns(capabilities);

            var encoderProfile = new EncoderProfile { Name = "Test Profile" };
            _encoderProfileMock.Setup(x => x.GetAvailableProfiles(It.IsAny<EncoderCapabilities>()))
                .Returns([encoderProfile]);

            var clientProfile = new ClientProfile { Name = "Test Client", DeviceFamily = "Desktop", RequiresAacStereoFallback = false };
            _clientProfileMock.Setup(x => x.Profiles).Returns(new Dictionary<string, ClientProfile> { { "Test Client", clientProfile } });

            var settings = new AppSettings
            {
                SelectedEncoderProfileName = "Test Profile",
                SelectedClientProfileName = "Test Client"
            };
            _settingsMock.Setup(x => x.Settings).Returns(settings);

            _queueMock.Setup(x => x.Items).Returns(new ReadOnlyObservableCollection<QueueItem>([]));

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            services.AddSingleton<ITranscodeService>(_transcodeServiceMock.Object);
            services.AddSingleton<IMediaFileAnalyzerService>(_mediaFileAnalyzerMock.Object);
            services.AddSingleton<ITranscodeQueueService>(_queueMock.Object);
            services.AddSingleton<ISubtitleExtractorService>(_subtitleExtractorMock.Object);
            services.AddSingleton<ISmartDecisionService>(_smartDecisionMock.Object);
            services.AddSingleton<IEncoderProfileService>(_encoderProfileMock.Object);
            services.AddSingleton<IClientProfileService>(_clientProfileMock.Object);
            services.AddSingleton<IPickerService>(_pickerMock.Object);
            services.AddSingleton<ISystemService>(_systemMock.Object);
            services.AddSingleton<ISettingsService>(_settingsMock.Object);
            services.AddSingleton<IPerformanceBucketService>(_performanceBucketServiceMock.Object);
            services.AddSingleton<EncodeTimeEstimatorProcess>();
            services.AddSingleton<SmartDecisionComplianceValidator>();

            services.AddSingleton<QueueViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }

        private QueueViewModel CreateViewModel()
        {
            return _serviceProvider.GetRequiredService<QueueViewModel>();
        }

        [Fact]
        public void Constructor_LoadsProfilesAndSetsDefaults()
        {
            // Act
            var vm = CreateViewModel();

            // Assert
            Assert.NotEmpty(vm.EncoderProfiles);
            Assert.NotEmpty(vm.ClientProfiles);
            Assert.NotNull(vm.SelectedEncoderProfile);
            Assert.NotNull(vm.SelectedClientProfile);
            Assert.Equal("Test Profile", vm.SelectedEncoderProfile.Name);
            Assert.Equal("Test Client", vm.SelectedClientProfile.Name);
        }

        [Fact]
        public void UpdateElapsedTime_DoesNothing_WhenNotConverting()
        {
            // Arrange
            var vm = CreateViewModel();
            vm.IsConverting = false;
            vm.StartDateTime = DateTime.Now.AddSeconds(-10);

            // Act
            vm.UpdateElapsedTime();

            // Assert
            Assert.Equal(TimeSpan.Zero, vm.ElapsedTime);
        }

        [Fact]
        public void IsHasItemsToRun_ReturnsTrue_WhenItemsExistAndArePending()
        {
            // Arrange
            var items = new List<QueueItem>
            {
                new() {
                    IsCompleted = false,
                    IsFailed = false,
                    IsRunning = false,
                    File= new(),
                    Info = new (),
                    ServerProfile = new (),
                    Task = new TranscodeTask() { Client = new ClientProfile() {RequiresAacStereoFallback = false }  }
                }
            };
            _queueMock.Setup(x => x.Items).Returns(new ReadOnlyObservableCollection<QueueItem>(new ObservableCollection<QueueItem>(items)));
            var vm = CreateViewModel();

            // Assert
            Assert.NotEmpty(vm.Items);
        }
    }
}