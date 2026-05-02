using InterpolationApi.Models;
using InterpolationApi.Operations.GeneratePresignedUrls;
using InterpolationApi.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InterpolationApiTest.Operations;

[TestClass]
public class GeneratePresignedUrlsOperationTests
{
    private Mock<IS3Service> _s3Service = null!;
    private Mock<IDynamoDbService> _dynamoDbService = null!;
    private Mock<ILogger<GeneratePresignedUrlsOperation>> _logger = null!;
    private GeneratePresignedUrlsOperation _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _s3Service = new Mock<IS3Service>();
        _dynamoDbService = new Mock<IDynamoDbService>();
        _logger = new Mock<ILogger<GeneratePresignedUrlsOperation>>();
        _sut = new GeneratePresignedUrlsOperation(_s3Service.Object, _dynamoDbService.Object, _logger.Object);
    }

    // --- Success path ---

    [TestMethod]
    public async Task ExecuteAsync_WhenInputIsValid_ReturnsUploadIdWithUplPrefix()
    {
        var (input, ct) = ValidInput();
        SetupS3();

        var result = await _sut.ExecuteAsync(input, ct);

        Assert.StartsWith("upl_", result.UploadId);
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenInputIsValid_ReturnsKeysContainingUploadIdAndFileNames()
    {
        var (input, ct) = ValidInput();
        SetupS3();

        var result = await _sut.ExecuteAsync(input, ct);

        StringAssert.Contains(result.Start.Key, result.UploadId);
        StringAssert.Contains(result.Start.Key, input.StartFile.Name);
        StringAssert.Contains(result.End.Key, result.UploadId);
        StringAssert.Contains(result.End.Key, input.EndFile.Name);
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenInputIsValid_MapS3PresignedDataToResult()
    {
        var (input, ct) = ValidInput();
        var startData = new PresignedPostData { Url = "https://s3/start", Fields = new() { ["key"] = "start-key" } };
        var endData   = new PresignedPostData { Url = "https://s3/end",   Fields = new() { ["key"] = "end-key" } };

        _s3Service.SetupSequence(s => s.GeneratePresignedPostAsync(It.IsAny<string>(), It.IsAny<string>(), ct))
            .ReturnsAsync(startData)
            .ReturnsAsync(endData);

        var result = await _sut.ExecuteAsync(input, ct);

        Assert.AreEqual(startData.Url, result.Start.Upload.Url);
        Assert.AreEqual(startData.Fields["key"], result.Start.Upload.Fields["key"]);
        Assert.AreEqual(endData.Url, result.End.Upload.Url);
        Assert.AreEqual(endData.Fields["key"], result.End.Upload.Fields["key"]);
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenInputIsValid_CallsS3ServiceTwiceWithCorrectContentTypes()
    {
        var (input, ct) = ValidInput();
        SetupS3();

        await _sut.ExecuteAsync(input, ct);

        _s3Service.Verify(s => s.GeneratePresignedPostAsync(It.IsAny<string>(), input.StartFile.ContentType, ct), Times.Once);
        _s3Service.Verify(s => s.GeneratePresignedPostAsync(It.IsAny<string>(), input.EndFile.ContentType, ct), Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenInputIsValid_CallsDynamoDbPutItemOnce()
    {
        var (input, ct) = ValidInput();
        SetupS3();

        await _sut.ExecuteAsync(input, ct);

        _dynamoDbService.Verify(d => d.PutItemAsync(input.Email, It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), ct), Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenInputIsValid_PutItemAttributesContainBothKeys()
    {
        var (input, ct) = ValidInput();
        SetupS3();
        Dictionary<string, string>? capturedAttributes = null;

        _dynamoDbService
            .Setup(d => d.PutItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), ct))
            .Callback<string, string, Dictionary<string, string>, CancellationToken>((_, _, attrs, _) => capturedAttributes = attrs)
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(input, ct);

        Assert.IsNotNull(capturedAttributes);
        Assert.IsTrue(capturedAttributes.ContainsKey("startFrameKey"));
        Assert.IsTrue(capturedAttributes.ContainsKey("endFrameKey"));
        StringAssert.Contains(capturedAttributes["startFrameKey"], input.StartFile.Name);
        StringAssert.Contains(capturedAttributes["endFrameKey"], input.EndFile.Name);
    }

    // --- Validation failures ---

    [TestMethod]
    public async Task ExecuteAsync_WhenEmailIsEmpty_ThrowsArgumentException()
    {
        var (input, ct) = ValidInput();
        input.Email = "";

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await _sut.ExecuteAsync(input, ct));
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenEmailIsWhitespace_ThrowsArgumentException()
    {
        var (input, ct) = ValidInput();
        input.Email = "   ";

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await _sut.ExecuteAsync(input, ct));
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenStartFileContentTypeIsEmpty_ThrowsArgumentException()
    {
        var (input, ct) = ValidInput();
        input.StartFile.ContentType = "";

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await _sut.ExecuteAsync(input, ct));
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenStartFileSizeIsZero_ThrowsArgumentException()
    {
        var (input, ct) = ValidInput();
        input.StartFile.Size = 0;

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await _sut.ExecuteAsync(input, ct));
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenStartFileSizeIsNegative_ThrowsArgumentException()
    {
        var (input, ct) = ValidInput();
        input.StartFile.Size = -1;

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await _sut.ExecuteAsync(input, ct));
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenEndFileContentTypeIsEmpty_ThrowsArgumentException()
    {
        var (input, ct) = ValidInput();
        input.EndFile.ContentType = "";

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await _sut.ExecuteAsync(input, ct));
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenEndFileSizeIsZero_ThrowsArgumentException()
    {
        var (input, ct) = ValidInput();
        input.EndFile.Size = 0;

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await _sut.ExecuteAsync(input, ct));
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenEndFileSizeIsNegative_ThrowsArgumentException()
    {
        var (input, ct) = ValidInput();
        input.EndFile.Size = -1;

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await _sut.ExecuteAsync(input, ct));
    }

    // --- Helpers ---

    private static (GeneratePresignedUrlsInput input, CancellationToken ct) ValidInput() =>
    (
        new GeneratePresignedUrlsInput
        {
            Email = "test@example.com",
            StartFile = new UploadFile { Name = "start.jpg", ContentType = "image/jpeg", Size = 1024 },
            EndFile   = new UploadFile { Name = "end.png",   ContentType = "image/png",  Size = 2048 }
        },
        CancellationToken.None
    );

    private void SetupS3()
    {
        _s3Service
            .Setup(s => s.GeneratePresignedPostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PresignedPostData { Url = "https://s3/bucket", Fields = [] });
    }
}
