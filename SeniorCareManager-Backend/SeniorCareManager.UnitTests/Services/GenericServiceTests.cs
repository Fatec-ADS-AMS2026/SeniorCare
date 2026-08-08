using FluentAssertions;
using Moq;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Entities;

namespace SeniorCareManager.UnitTests.Services;

public class GenericServiceTests
{
    private readonly Mock<IReligionRepository> _repoMock;
    private readonly ReligionService _service;

    public GenericServiceTests()
    {
        _repoMock = new Mock<IReligionRepository>();
        _service = new ReligionService(_repoMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllEntities()
    {
        var religions = new List<Religion>
        {
            new(1, "Católica"),
            new(2, "Evangélica"),
        };
        _repoMock.Setup(r => r.Get()).ReturnsAsync(religions);

        var result = await _service.GetAll();

        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.Name == "Católica");
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsEntity()
    {
        var religion = new Religion(1, "Budista");
        _repoMock.Setup(r => r.GetById(1)).ReturnsAsync(religion);

        var result = await _service.GetById(1);

        result.Should().NotBeNull();
        result.Name.Should().Be("Budista");
    }

    [Fact]
    public async Task Create_CallsRepositoryAdd()
    {
        var religion = new Religion(0, "Espírita");
        _repoMock.Setup(r => r.Add(religion)).Returns(Task.CompletedTask);

        await _service.Create(religion);

        _repoMock.Verify(r => r.Add(religion), Times.Once);
    }

    [Fact]
    public async Task Update_ExistingId_CallsRepositoryUpdate()
    {
        var existing = new Religion(1, "Umbanda");
        var updated = new Religion(1, "Umbanda Atualizada");
        _repoMock.Setup(r => r.GetById(1)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.Update(updated, null)).Returns(Task.CompletedTask);

        await _service.Update(updated, 1);

        _repoMock.Verify(r => r.Update(updated, null), Times.Once);
    }

    [Fact]
    public async Task Update_NonExistingId_ThrowsKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetById(99)).ReturnsAsync((Religion)null!);

        var act = async () => await _service.Update(new Religion(99, "X"), 99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Remove_ExistingId_CallsRepositoryRemove()
    {
        var religion = new Religion(1, "Judaísmo");
        _repoMock.Setup(r => r.GetById(1)).ReturnsAsync(religion);
        _repoMock.Setup(r => r.Remove(religion)).Returns(Task.CompletedTask);

        await _service.Remove(1);

        _repoMock.Verify(r => r.Remove(religion), Times.Once);
    }

    [Fact]
    public async Task Remove_NonExistingId_ThrowsKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetById(99)).ReturnsAsync((Religion)null!);

        var act = async () => await _service.Remove(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
