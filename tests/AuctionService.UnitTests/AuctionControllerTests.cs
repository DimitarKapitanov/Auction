using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly IMapper _mapper;
    private readonly AuctionsController _controller;
    private readonly Fixture _fixture;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);

        _controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = Helpers.GetClaimsPrincipal()
                }
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_Returns10Auction()
    {
        // Arrange
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
        _auctionRepo.Setup(repo => repo.GetAuctionAsync(null)).ReturnsAsync(auctions);

        // Act
        var result = await _controller.GetAllAuctions(null);

        // Assert
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuctionsById_WithValidGuid_ReturnsAuction()
    {
        // Arrange
        var auction = _fixture.Create<AuctionDto>();

        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // Act
        var result = await _controller.GetAuctionById(auction.Id);

        // Assert
        Assert.Equal(auction.Make, result.Value.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }

    [Fact]
    public async Task GetAuctionsById_WithInvalidGuid_ReturnsNotFound()
    {
        // Arrange
        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        // Act
        var result = await _controller.GetAuctionById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtActionResult()
    {
        var createAuctionDto = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.CreateAuction(createAuctionDto);
        var createdAtActionResult = result.Result as CreatedAtActionResult;
        // Assert
        Assert.NotNull(createdAtActionResult);
        Assert.Equal("GetAuctionById", createdAtActionResult.ActionName);
        Assert.IsType<AuctionDto>(createdAtActionResult.Value);
    }
    [Fact]
    public async Task CreateAuction_ReturnsBadRequest_WhenSaveChangesFails()
    {
        // Arrange
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        // Act
        var result = await _controller.CreateAuction(auction);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not save changes to the database", (result.Result as BadRequestObjectResult).Value);
    }
    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        // Arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();
        auction.Seller = "test";

        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        // Arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();
        auction.Seller = "test1";

        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        // Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync((Auction)null);

        // Act
        var result = await _controller.UpdateAuction(Guid.NewGuid(), updateAuctionDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
        auction.Seller = "test";

        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAuction(auction.Id);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        // Act
        var result = await _controller.DeleteAuction(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "not-test";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
            .ReturnsAsync(auction);

        // act
        var result = await _controller.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns400Response()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();
        auction.Seller = "test";

        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteAuction(auction.Id);
        // assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
