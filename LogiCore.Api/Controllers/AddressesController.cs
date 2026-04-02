using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.Services;
using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.DTOs;

namespace LogiCore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressAutocompleteService _service;

        public AddressesController(IAddressAutocompleteService service)
        {
            _service = service;
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete([FromQuery] string q)
        {
            var suggestions = await _service.GetSuggestionsAsync(q, 5);
            return Ok(suggestions);
        }


        [HttpPost("selected")]
        public async Task<IActionResult> Selected([FromBody] SelectedAddressDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Address)) return BadRequest();
            await _service.RecordSelectionAsync(dto.Address);
            return NoContent();
        }
    }
}
