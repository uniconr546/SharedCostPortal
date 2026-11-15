using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCost.Application.DTOs;
using SharedCost.Application.Services;

namespace SharedCost.API.Controllers;

[ApiController]
[Route("api/cost-pools")]
[Authorize]
public class CostPoolsController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CostPoolsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CostPoolDto>>> GetAll(
        [FromQuery] string? kyApDung,
        [FromQuery] string? trangThai,
        [FromQuery] string? keyword,
        CancellationToken ct) =>
        Ok(await _catalogService.GetCostPoolsAsync(kyApDung, trangThai, keyword, ct));

    [Authorize(Policy = "CanManageCatalog")]
    [HttpPost]
    public async Task<ActionResult<CostPoolDto>> Save([FromBody] SaveCostPoolRequest request, CancellationToken ct)
    {
        var result = await _catalogService.SaveCostPoolAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}

[ApiController]
[Route("api/drivers")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public DriversController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DriverDto>>> GetAll(
        [FromQuery] string? trangThai,
        [FromQuery] string? keyword,
        CancellationToken ct) =>
        Ok(await _catalogService.GetDriversAsync(trangThai, keyword, ct));

    [Authorize(Policy = "CanManageCatalog")]
    [HttpPost]
    public async Task<ActionResult<DriverDto>> Save([FromBody] SaveDriverRequest request, CancellationToken ct) =>
        Ok(await _catalogService.SaveDriverAsync(request, ct));
}

[ApiController]
[Route("api/driver-values")]
[Authorize]
public class DriverValuesController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public DriverValuesController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DriverValueDto>>> GetAll(
        [FromQuery] string? maDriver,
        [FromQuery] string? maTarget,
        [FromQuery] string? kyPhanBo,
        CancellationToken ct) =>
        Ok(await _catalogService.GetDriverValuesAsync(maDriver, maTarget, kyPhanBo, ct));

    [Authorize(Policy = "CanManageCatalog")]
    [HttpPost]
    public async Task<ActionResult<DriverValueDto>> Save([FromBody] SaveDriverValueRequest request, CancellationToken ct) =>
        Ok(await _catalogService.SaveDriverValueAsync(request, ct));
}

[ApiController]
[Route("api/targets")]
[Authorize]
public class TargetsController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public TargetsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TargetDto>>> GetAll(
        [FromQuery] string? trangThai,
        [FromQuery] string? keyword,
        CancellationToken ct) =>
        Ok(await _catalogService.GetTargetsAsync(trangThai, keyword, ct));

    [Authorize(Policy = "CanManageCatalog")]
    [HttpPost]
    public async Task<ActionResult<TargetDto>> Save([FromBody] SaveTargetRequest request, CancellationToken ct) =>
        Ok(await _catalogService.SaveTargetAsync(request, ct));
}
