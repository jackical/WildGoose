using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WildGoose.Application.Organization.V10;
using WildGoose.Application.Organization.V10.Dto;
using WildGoose.Application.Organization.V10.Queries;

namespace WildGoose.Controllers.V10;

/// <summary>
/// 机构 ID/NAME 之类信息不认为是敏感信息，只要登录即可获取
/// </summary>
/// <param name="organizationService"></param>
/// <param name="memoryCache"></param>
[ApiController]
[Route("api/v1.0/organizations")]
[Authorize]
public class OrganizationController(OrganizationService organizationService, IMemoryCache memoryCache) : ControllerBase
{
    private static readonly string FilePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot/data/organizations.json");

    /// <summary>
    /// 查询最上三级所有机构
    /// </summary>
    /// <returns></returns>
    [HttpGet("base")]
    public async Task<IActionResult> GetBaseList()
    {
        var etag = HttpContext.Request.Headers.ETag.ToString();
        var tuple = await memoryCache.GetOrCreateAsync("WG_API_V10_ORG_BASE", async entry =>
        {
            (string ETag, byte[] Data) result;
            if (System.IO.File.Exists(FilePath))
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(FilePath);
                var md5 = Convert.ToHexString(MD5.HashData(bytes));
                result = new ValueTuple<string, byte[]>(md5, bytes);
            }
            else
            {
                result = (string.Empty, Array.Empty<byte>());
            }

            entry.SetValue(result);
            entry.SlidingExpiration = TimeSpan.FromMinutes(1);
            return result;
        });

        // 首次请求
        if (string.IsNullOrEmpty(etag))
        {
            HttpContext.Response.Headers.ETag = tuple.ETag;
            return new FileContentResult(tuple.Data, "application/json");
        }

        // 数据一置
        if (etag == tuple.ETag)
        {
            return StatusCode(304);
        }

        HttpContext.Response.Headers.ETag = tuple.ETag;
        return new FileContentResult(tuple.Data, "application/json");
    }

    [HttpGet("subList")]
    public Task<List<SubOrganizationDto>> GetSubListAsync([FromQuery] GetSubListQuery query)
    {
        return organizationService.GetSubListAsync(query);
    }

    [HttpGet("{id}/summary")]
    public Task<OrganizationSummaryDto> GetSummaryAsync([FromRoute] GetSummaryQuery query)
    {
        return organizationService.GetSummaryAsync(query);
    }

    // [HttpHead("users/{userId}")]
    // public Task<bool> ExistsUser([FromRoute] ExistsUserQuery query)
    // {
    //     return organizationService.ExistsUserAsync(query);
    // }

    // [HttpGet("contains")]
    // public Task<bool> IsUserInOrganizationWithInheritance(
    //     [FromQuery, StringLength(36), Required]
    //     string userId,
    //     [FromQuery, StringLength(50), Required]
    //     string code,
    //     [FromQuery, StringLength(10), Required]
    //     string type
    // )
    // {
    //     if ("inherit".Equals(type, StringComparison.OrdinalIgnoreCase))
    //     {
    //         return organizationService.IsUserInOrganizationWithInheritanceAsync(
    //             new IsUserInOrganizationWithInheritanceQuery
    //             {
    //                 UserId = userId,
    //                 Code = code
    //             });
    //     }
    //
    //     return organizationService.ExistsUserAsync(new ExistsUserQuery
    //     {
    //         UserId = userId,
    //     });
    // }

    // [HttpGet("my")]
    // public async Task<List<OrganizationDto>> GetMyListAsync()
    // {
    //     return await _organizationService.GetMyListAsync();
    // }
}