using Microservicio.Application.DTOs;

namespace Microservicio.Application.Interfaces
{

    public interface ITableDummyService
    {
        /// <summary>
        /// Retrieves a table dummy result based on the provided input data.
        /// </summary>
        /// <param name="tableDummyDto">The input data used to generate the table dummy result. Cannot be <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a  <see
        /// cref="TableDummyDtoResult"/> object with the generated table dummy data.</returns>
        public Task<TableDummyDtoResult> GetTableDummy(TableDummyDto tableDummyDto);
    }
}
