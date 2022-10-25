﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkorpFiles.Memorizer.Api.ApiModels.ApiEntities;
using SkorpFiles.Memorizer.Api.ApiModels.Requests.Repository;
using SkorpFiles.Memorizer.Api.ApiModels.Responses;
using SkorpFiles.Memorizer.Api.Models.Exceptions;
using SkorpFiles.Memorizer.Api.Models.Interfaces.BusinessLogic;

namespace SkorpFiles.Memorizer.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RepositoryController:ControllerWithUserInfo
    {
        private readonly IEditingLogic _editingLogic;
        private readonly IMapper _mapper;

        public RepositoryController(IEditingLogic editingLogic, UserManager<IdentityUser> userManager, IUserStore<IdentityUser> userStore, IMapper mapper):base(userManager,userStore)
        {
            _editingLogic = editingLogic;
            _mapper = mapper;
        }

        [Route("Questionnaires")]
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> QuestionnairesAsync(GetQuestionnairesRequest request)
        {
            var userGuid = await GetCurrentUserGuidAsync();
            var result = await _editingLogic.GetQuestionnairesAsync(userGuid, _mapper.Map<Models.RequestModels.GetQuestionnairesRequest>(request));
            return Ok(_mapper.Map<GetQuestionnairesResponse>(result));
        }

        [Route("Questionnaire/{idOrCode}")]
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> QuestionnaireAsync(string idOrCode)
        {
            object idOrCodeObj;
            IdOrCode isIdOrCode;

            try
            {
                isIdOrCode = GetIdType(idOrCode, out idOrCodeObj);
            }
            catch(ArgumentException)
            {
                return BadRequest("Unsupported ID parameter: only GUID or integer are supported.");
            }

            Models.Questionnaire resultQuestionnaire;

            try
            {
                if (isIdOrCode == IdOrCode.Id)
                {
                    resultQuestionnaire = await _editingLogic.GetQuestionnaireAsync(
                        await GetCurrentUserGuidAsync(), (Guid)idOrCodeObj);
                }
                else if (isIdOrCode == IdOrCode.Code)
                {
                    resultQuestionnaire = await _editingLogic.GetQuestionnaireAsync(
                        await GetCurrentUserGuidAsync(), (int)idOrCodeObj);
                }
                else
                    throw new NotImplementedException("Enum method is not implemented.");
            }
            catch(AccessDeniedForUserException e)
            {
                return Unauthorized(e.Message);
            }

            if (resultQuestionnaire != null)
                return Ok(_mapper.Map<Questionnaire>(resultQuestionnaire));
            else
                return NotFound("Requested questionnaire is not found.");
        }

        private enum IdOrCode
        {
            Id,
            Code
        }

        private static IdOrCode GetIdType(string idOrCode, out object result)
        {
            bool isId = Guid.TryParse(idOrCode, out Guid guidResult);
            bool isCode = int.TryParse(idOrCode, out int intResult);
            if (isId)
            {
                result = guidResult;
                return IdOrCode.Id;
            }
            else if (isCode)
            {
                result = intResult;
                return IdOrCode.Code;
            }
            else
                throw new ArgumentException("The value is not a GUID ID or an int code.");
        }
    }
}
