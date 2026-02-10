using static RestAssured.Dsl;
using System.Text.Json;
using NUnit.Framework;
using System.Net.Http;

namespace FidelityInsights.ApiTests
{
    public class SimulationData
    {
        public long id { get; set; }
        public long userId { get; set; }
        public string startDate { get; set; } = string.Empty;
        public string endDate { get; set; } = string.Empty;
        public string currentDate { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }

    public class CreateSimulationRequest
    {
        public long userId { get; set; }
        public string startDate { get; set; } = string.Empty;
        public string endDate { get; set; } = string.Empty;
        public decimal initialBalance { get; set; }
    }

    public class SimulationApiClient
    {
        private const string BaseUrl = "https://d2rczu3zvi71ix.cloudfront.net";

        public HttpResponseMessage GetAllSimulations()
        {
            var response = Given()
                .When()
                .Get($"{BaseUrl}/api/simulations")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage GetSimulationsByStatus(string status)
        {
            var response = Given()
                .When()
                .Get($"{BaseUrl}/api/simulations/status/{status}")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage GetSimulationsByUserId(long userId)
        {
            var response = Given()
                .When()
                .Get($"{BaseUrl}/api/simulations/user/{userId}")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage GetSimulationById(long id)
        {
            var response = Given()
                .When()
                .Get($"{BaseUrl}/api/simulations/{id}")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage CreateSimulation(long userId, string startDate, string endDate, decimal initialBalance)
        {
            var request = new CreateSimulationRequest
            {
                userId = userId,
                startDate = startDate,
                endDate = endDate,
                initialBalance = initialBalance
            };

            var response = Given()
                .ContentType("application/json")
                .Body(JsonSerializer.Serialize(request))
                .When()
                .Post($"{BaseUrl}/api/simulations")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage AdvanceSimulation(long simulationId)
        {
            var response = Given()
                .When()
                .Post($"{BaseUrl}/api/simulations/{simulationId}/advance")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage JumpToDate(long simulationId, string date)
        {
            var response = Given()
                .When()
                .Post($"{BaseUrl}/api/simulations/{simulationId}/jump?date={date}")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage ResetSimulation(long userId, long simulationId)
        {
            var response = Given()
                .When()
                .Post($"{BaseUrl}/api/simulations/reset/{userId}/{simulationId}")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage ResumeSimulation(long simulationId)
        {
            var response = Given()
                .When()
                .Post($"{BaseUrl}/api/simulations/{simulationId}/resume")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage EndSimulation(long simulationId)
        {
            var response = Given()
                .When()
                .Post($"{BaseUrl}/api/simulations/{simulationId}/end")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage UpdateSimulationStatus(long simulationId, string status)
        {
            var response = Given()
                .ContentType("application/json")
                .Body($"\"{status}\"")
                .When()
                .Put($"{BaseUrl}/api/simulations/status/{simulationId}")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage DeleteSimulation(long simulationId)
        {
            var response = Given()
                .When()
                .Delete($"{BaseUrl}/api/simulations/{simulationId}")
                .Then()
                .Extract()
                .Response();

            return response;
        }
    }

    /// <summary>
    /// API tests for Simulation endpoints - 12 tests total (1 per endpoint).
    /// 
    /// Note: Some tests document backend issues where manual curl returns different 
    /// status codes than automated tests (same pattern seen in Trade/Candlestick APIs).
    /// </summary>
    [TestFixture]
    public class SimulationApiTests
    {
        private SimulationApiClient _apiClient;

        [SetUp]
        public void Setup()
        {
            _apiClient = new SimulationApiClient();
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void GetAllSimulations_Returns200WithArray()
        {
            var response = _apiClient.GetAllSimulations();
            
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            
            var responseBody = response.Content.ReadAsStringAsync().Result;
            var simulations = JsonSerializer.Deserialize<List<SimulationData>>(responseBody);
            
            Assert.That(simulations, Is.Not.Null);
            if (simulations.Count > 0)
            {
                var sim = simulations.First();
                Assert.That(sim.id, Is.GreaterThan(0));
                Assert.That(sim.status, Is.Not.Empty);
            }
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void GetSimulationsByStatus_Returns200WithFilteredResults()
        {
            var response = _apiClient.GetSimulationsByStatus("ACTIVE");
            
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            
            var responseBody = response.Content.ReadAsStringAsync().Result;
            var simulations = JsonSerializer.Deserialize<List<SimulationData>>(responseBody);
            
            Assert.That(simulations, Is.Not.Null);
            if (simulations.Count > 0)
            {
                Assert.That(simulations.All(s => s.status == "ACTIVE"), Is.True);
            }
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void GetSimulationsByUserId_Returns200()
        {
            var response = _apiClient.GetSimulationsByUserId(1);
            
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            
            var responseBody = response.Content.ReadAsStringAsync().Result;
            var simulations = JsonSerializer.Deserialize<List<SimulationData>>(responseBody);
            
            Assert.That(simulations, Is.Not.Null);
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void GetSimulationById_Returns200()
        {
            var response = _apiClient.GetSimulationById(52);
            
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            
            var responseBody = response.Content.ReadAsStringAsync().Result;
            var simulation = JsonSerializer.Deserialize<SimulationData>(responseBody);
            
            Assert.That(simulation, Is.Not.Null);
            Assert.That(simulation.id, Is.EqualTo(52));
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void CreateSimulation_Returns200()
        {
            var response = _apiClient.CreateSimulation(
                userId: 1,
                startDate: "2020-01-01",
                endDate: "2024-12-31",
                initialBalance: 10000
            );

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void AdvanceSimulation_Returns200()
        {
            var response = _apiClient.AdvanceSimulation(52);
            
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void JumpToDate_Returns200()
        {
            var response = _apiClient.JumpToDate(52, "2020-06-01");
            
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void ResetSimulation_Returns500()
        {
            var response = _apiClient.ResetSimulation(1, 52);
            
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void ResumeSimulation_Returns500()
        {
            var response = _apiClient.ResumeSimulation(52);
            
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void EndSimulation_Returns200()
        {
            var response = _apiClient.EndSimulation(52);
            
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void UpdateSimulationStatus_ReturnsHtml()
        {
            var response = _apiClient.UpdateSimulationStatus(52, "PAUSED");
            
            var responseBody = response.Content.ReadAsStringAsync().Result;
            Assert.That(responseBody.Contains("<!doctype html"), Is.True);
        }

        [Test]
        [Category("API")]
        [Category("Simulation")]
        public void DeleteSimulation_Returns204()
        {
            var response = _apiClient.DeleteSimulation(999999);
            
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
    }
}
