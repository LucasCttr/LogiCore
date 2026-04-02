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
        private readonly LogiCore.Application.Common.Interfaces.IAddressAutocompleteRepository _repo;

        public AddressesController(IAddressAutocompleteService service, LogiCore.Application.Common.Interfaces.IAddressAutocompleteRepository repo)
        {
            _service = service;
            _repo = repo;
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

        // Development helper: seed Redis with a list of addresses
        [HttpPost("seed")]
        public async Task<IActionResult> Seed([FromBody] IEnumerable<string> addresses)
        {
            if (addresses == null) return BadRequest();
            await _repo.SeedAsync(addresses);
            return Ok(new { count = addresses.Count() });
        }
    }
}
