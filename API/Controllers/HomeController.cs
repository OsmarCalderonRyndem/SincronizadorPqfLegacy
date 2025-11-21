using Microservicio.Application.DTOs;
using Microservicio.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.API.Controllers
{
    /// <summary>
    /// Provides endpoints for generating reports based on table data.
    /// </summary>
    /// <remarks>This controller is responsible for handling HTTP requests related to table data processing
    /// and report generation. It interacts with the <see cref="ITableDummyService"/> to perform the required
    /// operations.</remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        #region Properties and Constructor

        private readonly ITableDummyService _tableDummyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="tableDummyService">The service responsible for case of use.</param>
        public HomeController(ITableDummyService tableDummyService)
        {
            _tableDummyService = tableDummyService;
        }

        #endregion

       /// <summary>
       /// Generates a report based on the provided table data.
       /// </summary>
       /// <remarks>This method processes the provided table data and generates a report asynchronously. 
       /// The report is returned in the HTTP response body.</remarks>
       /// <param name="tableDummyDto">The data transfer object containing the table data used to generate the report.  This parameter cannot be
       /// null.</param>
       /// <returns>An <see cref="IActionResult"/> containing the generated report. The response is returned with an HTTP 200
       /// status code if the operation is successful.</returns>
        [HttpPost("tableDummy")]
        public async Task<IActionResult> GenerateReport([FromBody] TableDummyDto tableDummyDto)
        {
            var report = await _tableDummyService.GetTableDummy(tableDummyDto);
             return Ok(report);
        }
       
    }
}
