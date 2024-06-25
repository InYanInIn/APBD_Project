using NUnit.Framework;
using RCP_Project.Controller;
using RCP_Project.Models;
using RCP_Project.DTO;
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RCP_Project.Service;

namespace UnitTests
{
    [TestFixture]
    public class BusinessLogicTests
    {
        private LocalDbContext _context;
        private ClientController _clientController;
        private ContractController _contractController;
        private RevenueController _revenueController;
        private SoftwareController _softwareController;
        private PaymentController _paymentController;
        private PaymentService _paymentService;
        private ExchangeService _exchangeService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LocalDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new LocalDbContext(options);
            _paymentService = new PaymentService(_context);
            _exchangeService = new ExchangeService(_context);
            _clientController = new ClientController(_context);
            _contractController = new ContractController(_context, _paymentService);
            _revenueController = new RevenueController(_context, _exchangeService);
            _softwareController = new SoftwareController(_context);
            _paymentController = new PaymentController(_context, _paymentService);
        }

        [Test]
        public async Task TestAddClient()
        {
            var result = await _clientController.AddIndividual(new IndividualAddDTO
            {
                FirstName = "Test",
                LastName = "User",
                PESEL = "12345678901",
                Address = "Test Address",
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            });

            Assert.That(((ObjectResult)result.Result).StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        }

        [Test]
        public async Task TestRemoveClient()
        {
            int clientId = 1; 

            var result = await _clientController.DeleteClient(clientId);

            Assert.That(result, Is.TypeOf<NoContentResult>());

            var individual = await _context.Individuals.FindAsync(clientId);
            if (individual != null)
            {
                Assert.That(individual.IsDeleted, Is.True);
            }
            else
            {
                var company = await _context.Companies.FindAsync(clientId);
                Assert.That(company, Is.Null);
            }
        }

        [Test]
        public async Task TestUpdateClient()
        {
            int clientId = 1;
            var updateDto = new IndividualUpdateDTO
            {
                FirstName = "Updated",
                LastName = "User",
                Address = "Updated Address",
                Email = "updated@example.com",
                PhoneNumber = "0987654321"
            };

            var result = await _clientController.UpdateIndividual(clientId, updateDto);

            Assert.That(result, Is.TypeOf<NoContentResult>());

            var updatedIndividual = await _context.Individuals.FindAsync(clientId);
            Assert.That(updatedIndividual.FirstName, Is.EqualTo(updateDto.FirstName));
            Assert.That(updatedIndividual.LastName, Is.EqualTo(updateDto.LastName));
            Assert.That(updatedIndividual.Address, Is.EqualTo(updateDto.Address));
            Assert.That(updatedIndividual.Email, Is.EqualTo(updateDto.Email));
            Assert.That(updatedIndividual.PhoneNumber, Is.EqualTo(updateDto.PhoneNumber));
        }
        
        [Test]
        public async Task TestAddSoftware()
        {
            var software = new SoftwareDTO
            {
                Name = "Test Software",
                Description = "Test Description",
                CurrentVersion = "1.0",
                UpfrontCost = 1000m,
                Category = "Test Category"
            };

            var result = await _softwareController.PostSoftware(software);

            Assert.That(((ObjectResult)result.Result).StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        }

        [Test]
        public async Task TestCreateContract()
        {
            var contractDto = new ContractCreateDTO
            {
                ClientID = 1,
                SoftwareID = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10),
                SupportExtensionYears = 1
            };

            var result = await _contractController.CreateContract(contractDto);

            Assert.That(((CreatedAtActionResult)result.Result).StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            var createdContract = (Contract)((CreatedAtActionResult)result.Result).Value;

            Assert.That(createdContract.ClientID, Is.EqualTo(contractDto.ClientID));
            Assert.That(createdContract.SoftwareID, Is.EqualTo(contractDto.SoftwareID));
            Assert.That(createdContract.StartDate.Date, Is.EqualTo(contractDto.StartDate.Date));
            Assert.That(createdContract.EndDate.Date, Is.EqualTo(contractDto.EndDate.Date));
        }

        [Test]
        public async Task TestIssuePaymentForContract()
        {
            var paymentDto = new PaymentCreateDTO
            {
                ContractID = 1,
                Amount = 1000,
                PaymentDate = DateTime.Now
            };

            var result = await _paymentController.PayForContract(paymentDto);

            Assert.That(((CreatedAtActionResult)result.Result).StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            var createdPayment = (Payment)((CreatedAtActionResult)result.Result).Value;

            Assert.That(createdPayment.ContractID, Is.EqualTo(paymentDto.ContractID));
            Assert.That(createdPayment.Amount, Is.EqualTo(paymentDto.Amount));
            Assert.That(createdPayment.PaymentDate.Date, Is.EqualTo(paymentDto.PaymentDate.Date));
        }

        [Test]
        public async Task TestCalculateRevenue()
        {
            var result = await _revenueController.CalculateCurrentRevenue(1, "USD");
            Assert.That(result.Value, Is.Not.Null);
        }

        [Test]
        public async Task TestCalculatePredictedRevenue()
        {
            var result = await _revenueController.CalculatePredictedRevenue(1, "USD");
            Assert.That(result.Value, Is.Not.Null);
        }
    }
}