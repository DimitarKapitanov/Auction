using BiddingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Bid>> PlaseBid(string auctionId, int amount)
    {
        var auction = await DB.Find<Auction>().OneAsync(auctionId);
        if (auction == null)
        {
            // TODO: Check with auction service ifd that has auction
            return NotFound();
        }

        if (auction.Seller == User.Identity.Name)
        {
            return BadRequest("You can't bid on your own auction");
        }

        var bid = new Bid
        {
            Amount = amount,
            AuctionId = auctionId,
            Bidder = User.Identity.Name
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {
            var highestBid = await DB.Find<Bid>()
                       .Match(b => b.AuctionId == auctionId)
                       .Sort(b => b.Descending(b => b.Amount))
                       .ExecuteFirstAsync();

            if (highestBid != null && highestBid.Amount >= amount || highestBid == null)
            {
                bid.BidStatus = amount > auction.ReservePrice ? BidStatus.Accepted : BidStatus.AcceptedBelowReserve;
            }

            if (highestBid != null && bid.Amount <= amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }
        await DB.SaveAsync(bid);

        return Ok(bid);
    }

    [HttpGet]
    public async Task<ActionResult<List<Bid>>> GetBidsForAuction(string auctionId)
    {

        var bids = await DB.Find<Bid>()
       .Match(b => b.AuctionId == auctionId)
       .Sort(b => b.Descending(b => b.BidTime))
       .ExecuteAsync();

        return bids;
    }
}
