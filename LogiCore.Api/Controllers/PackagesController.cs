using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Package.GetPackagePublicHistory;
using LogiCore.Application.Common.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using AutoMapper;
using LogiCore.Application.Features.Packages;
using LogiCore.Application.Features.Package.MarkPackageAsDelivered;
using LogiCore.Application.Features.Package.MarkPackageAsCollected;
using LogiCore.Application.Features.Package.GetPackageForScanner;
using LogiCore.Application.Features.Package.MoveToDepot;
using LogiCore.Application.Features.Package.CollectPackage;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PackagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/packages?page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<Result<PagedResponse<PackageDto>>>> GetByPage([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllPackagesQuery(page, pageSize));
        return result;
    }

    // GET: api/packages/{id}
    [HttpGet("{id:guid}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<PackageDetailDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPackageByIdQuery(id));
        return result;
    }

    // POST: api/packages
    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<PackageDto>>> Create([FromBody] CreatePackageCommand request)
    {
        var result = await _mediator.Send(request);
        return result; // filter applyed in ResultActionFilter
    }

    // PUT: api/packages/{id}
    [HttpPut("{id:guid}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<PackageDto>>> Update(Guid id, [FromBody] UpdatePackageCommand request)
    {
        request = request with { Id = id }; // set the id from route to the command
        var result = await _mediator.Send(request);
        return result; // filter applyed in ResultActionFilter  
    }


    // POST: api/packages/{id}/deliver
    [HttpPost("{id:guid}/deliver")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<PackageDto>>> Deliver(Guid id)
    {
        var result = await _mediator.Send(new DeliverPackageCommand(id));
        return result;
    }

    // POST: api/packages/{id}/collect (Driver collects package from seller - Pending → InTransit)
    [HttpPost("{id:guid}/collect")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<bool>>> Collect(Guid id)
    {
        var result = await _mediator.Send(new CollectPackageCommand(id));
        return result;
    }

    // POST: api/packages/{id}/move-to-depot (Scanner action - move package to depot)
    [HttpPost("{id:guid}/move-to-depot")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<bool>>> MoveToDepot(Guid id)
    {
        var result = await _mediator.Send(new MovePackageToDepotCommand(id));
        return result;
    }

    // POST: api/packages/{id}/mark-delivered (Driver - marks package as delivered with location & notes)
    [HttpPost("{id:guid}/mark-delivered")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<bool>>> MarkDelivered(Guid id, [FromBody] MarkPackageAsDeliveredCommand request)
    {
        request = request with { PackageId = id };
        var result = await _mediator.Send(request);
        return result;
    }

    // POST: api/packages/{id}/mark-collected (Driver - marks package as collected for Pickup shipments)
    [HttpPost("{id:guid}/mark-collected")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<bool>>> MarkCollected(Guid id, [FromBody] MarkPackageAsCollectedCommand request)
    {
        request = request with { PackageId = id };
        var result = await _mediator.Send(request);
        return result;
    }

    // POST: api/packages/{id}/cancel
    [HttpPost("{id:guid}/cancel")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<PackageDto>>> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelPackageCommand(id));
        return result;
    }

    // POST: api/packages/bulk-depot (initial ingress)
    [HttpPost("bulk-depot")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<bool>>> BulkDepot([FromBody] BulkMoveToDepotCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }
    // POST: api/packages/bulk/cancel-or-return
    [HttpPost("bulk/cancel-or-return")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<bool>>> BulkCancelOrReturn([FromBody] BulkCancelPackagesCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }


    // GET: api/packages/tracking/{trackingNumber} (public minimal history)
    [HttpGet("tracking/{trackingNumber}")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<ActionResult<Result<PackagePublicHistoryDto?>>> GetByTracking(string trackingNumber)
    {
        var result = await _mediator.Send(new GetPackagePublicHistoryQuery(trackingNumber));
        return result;
    }

    // GET: api/packages/{id}/history (internal)
    [HttpGet("{id:guid}/history")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<IEnumerable<PackageInternalHistoryDto>>>> GetHistory(Guid id)
    {
        var result = await _mediator.Send(new LogiCore.Application.Features.Package.GetPackageHistory.GetPackageHistoryQuery(id));
        return result;
    }

    // GET: api/packages/scanner/tracking/{trackingNumber} (Scanner mode - validates package by tracking number from barcode)
    [HttpGet("scanner/tracking/{trackingNumber}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<PackageForScannerDto>>> GetForScannerByTracking(string trackingNumber)
    {
        var result = await _mediator.Send(new GetPackageForScannerByTrackingQuery(trackingNumber));
        return result;
    }

    // GET: api/packages/scanner/{id:guid} (Scanner mode - validates package for depot ingress by GUID)
    [HttpGet("scanner/{id:guid}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<PackageForScannerDto>>> GetForScanner(Guid id)
    {
        var result = await _mediator.Send(new GetPackageForScannerQuery(id));
        return result;
    }
}