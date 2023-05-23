using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;
using AuctionPlanningServiceAPI.Model;
using AuctionPlanningServiceAPI.Service;

namespace AuctionPlanningServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionPlanningController : ControllerBase
{
    private readonly ILogger<AuctionPlanningController> _logger;

    private readonly IConfiguration _config;

    private readonly IAuctionPlanningRepository _service;

    public AuctionPlanningController(ILogger<AuctionPlanningController> logger, IConfiguration config, IAuctionPlanningRepository service)
    {
        _logger = logger;
        _config = config;
        _service = service;
    }

    //POST - Adds a new auction
    [Authorize]
    [HttpPost("addAuction")]
    public async Task<Auction> AddAuction(AuctionDTO auctionDTO)
    {
        _logger.LogInformation($"[POST] addAuction endpoint reached");

        return await _service.AddAuction(auctionDTO);
    }

    //GET - Return a list of all auctions
    [HttpGet("getAll")]
    public async Task<List<Auction>> GetAllAuctions()
    {
        _logger.LogInformation($"[GET] getAll endpoint reached");

        return await _service.GetAllAuctions();
    }

    //GET - Return a list of all active auctions
    [HttpGet("getAllActive")]
    public async Task<List<Auction>> GetAllActiveAuctions()
    {
        return await _service.GetAllActiveAuctions();
    }

    //GET - Return a list of all auctions
    [HttpGet("categories/{categoryCode}")]
    public async Task<List<Category>> GetCategory(string categoryCode)
    {
        _logger.LogInformation($"[GET] categories/{categoryCode} endpoint reached");

        return await _service.GetCategory(categoryCode);
    }

    //DELETE - Removes an auction
    [Authorize]
    [HttpDelete("deleteAuction/{auctionId}")]
    public async Task<Auction> DeleteAuction(string auctionId)
    {
        _logger.LogInformation($"[DELETE] deleteAuction/{auctionId} endpoint reached");

        return await _service.DeleteAuction(auctionId);
    }

    // GET - Retrieves an auction by ID
    [HttpGet("getAuction/{auctionId}")]
    public async Task<Auction> GetAuction(string auctionId)
    {
        _logger.LogInformation($"[GET] getAuction/{auctionId} endpoint reached");

        return await _service.GetAuctionByID(auctionId);
    }

    // PUT - Updates an auction
    [Authorize]
    [HttpPut("updateAuction/{auctionId}")]
    public async Task<Auction> UpdateAuction(string auctionId, AuctionDTO auctionDTO)
    {
        _logger.LogInformation($"[PUT] updateAuction/{auctionId} endpoint reached");

        return await _service.UpdateAuction(auctionId, auctionDTO);
    }


}

