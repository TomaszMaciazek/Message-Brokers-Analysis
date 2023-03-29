using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Producer.RabbitMQ.API.Helpers;
using Producer.RabbitMQ.API.Services;

namespace Producer.RabbitMQ.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExperimentController : ControllerBase
    {
        private readonly IPublisherService _publisherService;

        public ExperimentController(IPublisherService publisherService)
        {
            _publisherService = publisherService;
        }

        [HttpGet("throughput/{seconds}/{size}")]
        public async Task<IActionResult> PublishThroughputExperiment([FromRoute] int seconds, [FromRoute] int size)
        {
            try
            {
                var bytes = BytesHelper.GetByteArray(size);
                var res = await _publisherService.SendMessagesForExperimentOne(seconds, bytes);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("transfer/{size}")]
        public async Task<IActionResult> TransferExperiment([FromRoute] int size)
        {
            try
            {
                var bytes = BytesHelper.GetByteArray(size);
                var res = await _publisherService.TransferTimeExperiment(bytes);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
