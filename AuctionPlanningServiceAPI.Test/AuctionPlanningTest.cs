using AuctionPlanningServiceAPI.Controllers;
using AuctionPlanningServiceAPI.Model;
using AuctionPlanningServiceAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuctionPlanningServiceAPI.Test;

public class AuctionPlanningTest
{

    private ILogger<AuctionPlanningController> _logger = null!;
    private IConfiguration _configuration = null!;


    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<AuctionPlanningController>>().Object;

        var myConfiguration = new Dictionary<string, string?>
        {
            {"AuctionPlanningServiceBrokerHost", "http://testhost.local"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }

    // Tests that the  method returns a CreatedAtActionResult object, when an auction is created correctly
    [Test]
    public async Task TestAddUAuctionEndpoint_valid_dto()
    {
        // Arrange
        var auctionDTO = CreateAuctionDTO("Test Auction");
        var auction = CreateAuction(1);

        var stubRepo = new Mock<IAuctionPlanningRepository>();

        stubRepo.Setup(svc => svc.AddAuction(auctionDTO))
            .Returns(Task.FromResult<Auction>(auction));

        var controller = new AuctionPlanningController(_logger, _configuration, stubRepo.Object);

        // Act        
        var result = await controller.AddAuction(auctionDTO);

        // Assert
        Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
        Assert.That((result as CreatedAtActionResult)?.Value, Is.TypeOf<Auction>());
    }

    // Tests that the method returns a BadRequestResult object, when the AddAuction method fails / throws an exception
    [Test]
    public async Task TestAddUserEndpoint_failure_posting()
    {
        // Arrange
        var auctionDTO = CreateAuctionDTO("Test Auction");
        var auction = CreateAuction(1);


        var stubRepo = new Mock<IAuctionPlanningRepository>();

        stubRepo.Setup(svc => svc.AddAuction(auctionDTO))
            .ThrowsAsync(new Exception());

        var controller = new AuctionPlanningController(_logger, _configuration, stubRepo.Object);

        // Act        
        var result = await controller.AddAuction(auctionDTO);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestResult>());
    }

    /// <summary>
    /// Helper method for creating AuctionDTO instance.
    /// </summary>
    /// <param name="articleID"></param>
    /// <returns></returns>
    private AuctionDTO CreateAuctionDTO(string articleID)
    {
        var auctionDTO = new AuctionDTO()
        {
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(1),
            ArticleID = articleID
        };

        return auctionDTO;
    }

    /// <summary>
    /// Helper method for creating Auction instance.
    /// </summary>
    /// <param name="highestBid"></param>
    /// <returns></returns>
    private Auction CreateAuction(int highestBid)
    {
        var auction = new Auction()
        {
            AuctionID = "Test AuctionID",
            HighestBid = highestBid,
            Bids = new List<Bid>(),
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(1),
            Views = 0,
            Article = new Article(),
            Comments = new List<Comment>()
        };
        return auction;
    }

}