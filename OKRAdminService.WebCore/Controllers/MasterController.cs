using Microsoft.AspNetCore.Mvc;
using OKRAdminService.Common;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace OKRAdminService.WebCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ApiControllerBase
    {
        private readonly IMasterService _masterService;
        private readonly IDistributedCache _distributedCache;
        private readonly IConfiguration _configuration;

        public MasterController(IIdentityService identityService, IMasterService masterService, IDistributedCache distributedCache, IConfiguration configuration) : base(identityService)
        {
            Console.WriteLine("MasterController Const Called ");
            _masterService = masterService;
            _distributedCache = distributedCache;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetAllMaster")]
        public async Task<IActionResult> GetAllMasterDetails()
        {
            var payloadGet = new PayloadCustom<MasterResponse>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var cacheKey = TenantId + AppConstants.GetAllMaster;
            string serializedList;
            var redisList = await _distributedCache.GetAsync(cacheKey);
            if (redisList != null)
            {
                serializedList = Encoding.UTF8.GetString(redisList);
                var resDeserializeObject = JsonConvert.DeserializeObject<MasterResponse>(serializedList);
                payloadGet.Entity = resDeserializeObject;
            }
            else
            {
                payloadGet.Entity = await _masterService.GetAllMasterDetailsAsync();
                serializedList = JsonConvert.SerializeObject(payloadGet.Entity);
                redisList = Encoding.UTF8.GetBytes(serializedList);
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(_configuration.GetValue<int>("Redis:ExpiryTime")))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(_configuration.GetValue<int>("Redis:SlidingExpireTime")));
                await _distributedCache.SetAsync(cacheKey, redisList, options);
            }

            if (payloadGet.Entity != null)
            {
                payloadGet.MessageType = MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("MasterDetails", "No Record Found");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payloadGet);
        }

        [HttpGet]
        [Route("GetAllOkrFilters")]
        public async Task<IActionResult> GetOkrFiltersMaster([Required] long organisationId)
        {
            var payloadGet = new PayloadCustom<OkrStatusMasterDetails>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var cacheKey = TenantId + AppConstants.OkrFilters + organisationId;
            string serializedList;
            var redisList = await _distributedCache.GetAsync(cacheKey);
            if (redisList != null)
            {
                serializedList = Encoding.UTF8.GetString(redisList);
                var resDeserializeObject = JsonConvert.DeserializeObject<OkrStatusMasterDetails>(serializedList);
                payloadGet.Entity = resDeserializeObject;
            }
            else
            {
                payloadGet.Entity = await _masterService.GetOkrFiltersMasterAsync(organisationId);
                serializedList = JsonConvert.SerializeObject(payloadGet.Entity);
                redisList = Encoding.UTF8.GetBytes(serializedList);
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(_configuration.GetValue<int>("Redis:ExpiryTime")))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(_configuration.GetValue<int>("Redis:SlidingExpireTime")));
                await _distributedCache.SetAsync(cacheKey, redisList, options);
            }
            if (payloadGet.Entity != null)
            {
                payloadGet.MessageType = MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("MasterDetails", "No Record Found");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payloadGet);
        }

        [HttpGet]
        [Route("AssignmentTypes")]
        public async Task<IActionResult> GetAssignmentTypeMasterAsync()
        {
            var payloadGet = new PayloadCustom<List<AssignmentTypeResponse>>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var cacheKey = TenantId + AppConstants.AssignmentTypes;
            string serializedList;
            var redisList = await _distributedCache.GetAsync(cacheKey);
            if (redisList != null)
            {
                serializedList = Encoding.UTF8.GetString(redisList);
                var resDeserializeObject = JsonConvert.DeserializeObject<List<AssignmentTypeResponse>>(serializedList);
                payloadGet.Entity = resDeserializeObject;
            }
            else
            {
                payloadGet.Entity = await _masterService.GetAssignmentTypeMasterAsync();
                serializedList = JsonConvert.SerializeObject(payloadGet.Entity);
                redisList = Encoding.UTF8.GetBytes(serializedList);
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(_configuration.GetValue<int>("Redis:ExpiryTime")))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(_configuration.GetValue<int>("Redis:SlidingExpireTime")));
                await _distributedCache.SetAsync(cacheKey, redisList, options);
            }

            if (payloadGet.Entity != null && payloadGet.Entity.Count > 0)
            {
                payloadGet.MessageType = MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("MasterDetails", "No Record Found");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payloadGet);
        }

        [HttpGet]
        [Route("Metrices")]
        public async Task<IActionResult> GetAllMetricMasterAsync()
        {
            var payloadGet = new PayloadCustom<List<MetricMasterResponse>>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var cacheKey = TenantId + AppConstants.Metrics;
            string serializedList;
            var redisList = await _distributedCache.GetAsync(cacheKey);
            if (redisList != null)
            {
                serializedList = Encoding.UTF8.GetString(redisList);
                var resDeserializeObject = JsonConvert.DeserializeObject<List<MetricMasterResponse>>(serializedList);
                payloadGet.Entity = resDeserializeObject;
            }
            else
            {
                payloadGet.Entity = await _masterService.GetAllMetricMasterAsync();
                serializedList = JsonConvert.SerializeObject(payloadGet.Entity);
                redisList = Encoding.UTF8.GetBytes(serializedList);
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(_configuration.GetValue<int>("Redis:ExpiryTime")))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(_configuration.GetValue<int>("Redis:SlidingExpireTime")));
                await _distributedCache.SetAsync(cacheKey, redisList, options);
            }

            if (payloadGet.Entity != null && payloadGet.Entity.Count > 0)
            {
                payloadGet.MessageType = MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("MasterDetails", "No Record Found");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payloadGet);
        }

        [HttpGet]
        [Route("OkrMasterData")]
        public async Task<IActionResult> GetAllOkrMasterAsync()
        {
            var payloadGet = new PayloadCustom<GetAllOkrMaster>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var cacheKey = TenantId + AppConstants.OkrMasterData;
            string serializedList;
            var redisList = await _distributedCache.GetAsync(cacheKey);
            if (redisList != null)
            {
                serializedList = Encoding.UTF8.GetString(redisList);
                var resDeserializeObject = JsonConvert.DeserializeObject<GetAllOkrMaster>(serializedList);
                payloadGet.Entity = resDeserializeObject;
            }
            else
            {
                payloadGet.Entity = await _masterService.GetAllOkrMaster();
                serializedList = JsonConvert.SerializeObject(payloadGet.Entity);
                redisList = Encoding.UTF8.GetBytes(serializedList);
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(_configuration.GetValue<int>("Redis:ExpiryTime")))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(_configuration.GetValue<int>("Redis:SlidingExpireTime")));
                await _distributedCache.SetAsync(cacheKey, redisList, options);
            }

            if (payloadGet.Entity != null)
            {
                payloadGet.MessageType = MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("MasterDetails", "No Record Found");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payloadGet);
        }


        [HttpPost]
        [Route("ClearCache")]
        public async Task<IActionResult> ClearRedisCache(int organizationId)
        {
            var payload = new PayloadCustom<string>();
            var getAllUsersCacheKey = TenantId + AppConstants.GetAllUsers;
            var organizationCycleDetailsCacheKey = TenantId + AppConstants.OrganizationCycleDetails + organizationId;
            var teamsByIdCacheKey = TenantId + AppConstants.TeamsById + organizationId;
            var teamsDetailsCacheKey = TenantId + AppConstants.TeamsDetails;
            var okrFiltersCacheKey = TenantId + AppConstants.OkrFilters + organizationId;
            var getAllMasterCacheKey = TenantId + AppConstants.GetAllMaster;
            var assignmentTypesCacheKey = TenantId + AppConstants.AssignmentTypes;
            var metricsCacheKey = TenantId + AppConstants.Metrics;
            var okrMasterDataCacheKey = TenantId + AppConstants.OkrMasterData;
            
            await _distributedCache.RemoveAsync(getAllUsersCacheKey);
            await _distributedCache.RemoveAsync(organizationCycleDetailsCacheKey);
            await _distributedCache.RemoveAsync(teamsByIdCacheKey);
            await _distributedCache.RemoveAsync(teamsDetailsCacheKey);
            await _distributedCache.RemoveAsync(okrFiltersCacheKey);
            await _distributedCache.RemoveAsync(getAllMasterCacheKey);
            await _distributedCache.RemoveAsync(assignmentTypesCacheKey);
            await _distributedCache.RemoveAsync(metricsCacheKey);
            await _distributedCache.RemoveAsync(okrMasterDataCacheKey);
            payload.MessageType = MessageType.Success.ToString();
            payload.IsSuccess = true;
            payload.Status = Response.StatusCode;
            return Ok(payload);
        }

    }
}
