using Microservicio.Application.DTOs;
using Microservicio.Application.Interfaces;
using Microservicio.Application.Validators;
using Microservicio.Domain.Models;
using Microservicio.Infrastructure.Repository;
using Microsoft.Extensions.Logging;

namespace Microservicio.Application.Services
{
    /// <summary>
    /// Provides operations for managing and validating table dummy entities.
    /// </summary>
    /// <remarks>This service is responsible for retrieving, validating, and updating table dummy entities. It
    /// interacts with the repository layer to fetch and persist data and uses a validator to ensure the integrity of
    /// the entities.</remarks>
    /// <param name="logger"></param>
    /// <param name="validatorTable"></param>
    /// <param name="tableDummyRepository"></param>
    public class TableDummyService(
        ILogger<TableDummyDomain> logger,
        TableDummyValidator validatorTable,
        TableDummyRepository tableDummyRepository
        ) : ITableDummyService
    {

        #region Properties and Constructor
        private readonly ILogger<TableDummyDomain> _logger = logger;
        private readonly TableDummyValidator _validatorTemplate = validatorTable;
        private readonly TableDummyRepository _repository = tableDummyRepository;
        #endregion


        public async Task<TableDummyDtoResult> GetTableDummy(TableDummyDto tableDummyDto)
        {
            var jsonStringSignatures = string.Empty;
            _logger.LogInformation("Start Service");

            #region Get from DB
            var uniqueRegister = await _repository.GetById(tableDummyDto.IdRegister);
            #endregion

            #region Validate Existence
            await _validatorTemplate.ValidateAsync(uniqueRegister);
            #endregion
            if (uniqueRegister != null)
            {
                uniqueRegister.Description = "Updated Description";
                var idResult = await _repository.AddOrUpdate(uniqueRegister);
            }

            _logger.LogInformation("Finally Service");

            return new TableDummyDtoResult
            {
                tableDummy = uniqueRegister
            };

        }
    }
}
