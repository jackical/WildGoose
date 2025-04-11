using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WildGoose.Application.Permission.Internal.V10;
using WildGoose.Application.Permission.Internal.V10.Queries;

namespace WildGoose.Controllers.V10;

[Route("api/v1.0/permissions")]
[ApiController]
[Authorize("Bearer,Token")]
public class PermissionController(PermissionService permissionService, ILogger<PermissionController> logger)
    : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("enforce")]
    public async Task<IActionResult> EnforceAsync([FromBody] List<EnforceQuery> query)
    {
        if (query.Count == 0)
        {
            return Content("[]", "text/plain");
        }

        var builder = new StringBuilder("[");
        for (var i = 0; i < query.Count; ++i)
        {
            var v = await permissionService.EnforceAsync(query[i]);
            builder.Append(v ? "true" : "false");
            if (i != query.Count - 1)
            {
                builder.Append(',');
            }
        }

        builder.Append(']');
        var result = builder.ToString();
        return Content(result, "text/plain");
    }
}