using InvestmentPortfolio.Service.IService;
using InvestmentPortfolio.Service.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopReturnsMFController : ControllerBase
    {
        private readonly IMutualFundService _fundService;

        public TopReturnsMFController(IMutualFundService fundService)
        {
            _fundService = fundService;
        }

        [HttpGet("top-funds")]
        public async Task<IActionResult> GetTopFunds()
        {
            var topFunds = await _fundService.GetTopFundsAsync();
            return Ok(topFunds);
        }

    }
}
