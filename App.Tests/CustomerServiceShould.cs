using App.DataAccess;
using App.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace App.Tests
{
    [TestFixture]
    public class CustomerServiceShould
    {
        private CustomerService _customerService;
        private Mock<ICompanyRepository> _mockCompanyRepo;
        private Mock<ICustomerCreditService> _mockCreditService;
        private Mock<ICustomerDataAccessWrapper> _mockDataAccessWrapper;
        private readonly int _companyId = 4;
        private readonly Customer _newCust = new()
        {
            Company = null,
            CreditLimit = 0,
            HasCreditLimit = false,
            DateOfBirth = new DateTime(1980, 3, 27),
            EmailAddress = "joe.bloggs@adomain.com",
            Firstname = "Joe",
            Surname = "Bloggs"
        };

        [SetUp]
        public void Setup()
        {
            _mockCompanyRepo = new Mock<ICompanyRepository>();
            _mockCreditService = new Mock<ICustomerCreditService>();
            _mockDataAccessWrapper = new Mock<ICustomerDataAccessWrapper>();
        }


        [Test(Description = "Happy path")]
        public void AddCustomer_HappyPath()
        {
            _mockCompanyRepo.Setup(x => x.GetById(_companyId))
                .Returns(new Company { Id = 4, Name = "ImportantClient", Classification = Classification.Gold });

            _mockCreditService.Setup(x => x.GetCreditLimit(_newCust.Firstname, _newCust.Surname, _newCust.DateOfBirth))
                .ReturnsAsync(500);

            _mockDataAccessWrapper.Setup(x => x.AddCustomer(It.IsAny<Customer>()));

            _customerService = new CustomerService(_mockCompanyRepo.Object, _mockCreditService.Object, _mockDataAccessWrapper.Object);
            var result = _customerService?.AddCustomer
                (_newCust.Firstname, _newCust.Surname, _newCust.EmailAddress, _newCust.DateOfBirth, _companyId);

            Assert.IsTrue(result);
        }

        [Test(Description = "No first name provided")]
        public void AddCustomer_NoFirstName_ReturnFalse()
        {
            _customerService = new CustomerService();

            var result = _customerService?.AddCustomer
                ("", _newCust.Surname, _newCust.EmailAddress, _newCust.DateOfBirth, _companyId);

            Assert.IsFalse(result);
        }

        [Test(Description = "No surname provided")]
        public void AddCustomer_NoSurname_ReturnFalse()
        {
            _customerService = new CustomerService();

            var result = _customerService?.AddCustomer
                (_newCust.Firstname, "", _newCust.EmailAddress, _newCust.DateOfBirth, _companyId);

            Assert.IsFalse(result);
        }

        [Test(Description = "Invalid email address provided")]
        public void AddCustomer_InvalidEmailAddress_ReturnFalse()
        {
            _customerService = new CustomerService();

            var result = _customerService?.AddCustomer
                (_newCust.Firstname, _newCust.Surname, "joebloggsadomaincom", _newCust.DateOfBirth, _companyId);

            Assert.IsFalse(result);
        }

        [Test(Description = "Customer found to be under 21")]
        [TestCase("2003/3/27")]
        [TestCase("2002/12/27")]
        [TestCase("2002/09/10")]
        public void AddCustomer_CustomerAgeUnder21_ReturnFalse(DateTime dob)
        {
            _customerService = new CustomerService();

            var result = _customerService?.AddCustomer
                (_newCust.Firstname, _newCust.Surname, _newCust.EmailAddress, dob, _companyId);

            Assert.IsFalse(result);
        }

        public static object[] CustomerCreditLimitUnder500TestCases =
        {
            new object[] { new Company { Id = 1, Name = "TestCo", Classification = Classification.Bronze }, 200 },
            new object[] { new Company { Id = 2, Name = "MyCoLtd", Classification = Classification.Silver }, 250 },
            new object[] { new Company { Id = 3, Name = "ImportantClient", Classification = Classification.Silver }, 150 }
        };

        [Test(Description = "Customer credit limit found to be under £500")]
        [TestCaseSource(nameof(CustomerCreditLimitUnder500TestCases))]
        public void AddCustomer_CreditLimitUnder500_ReturnFalse(Company co, int credLimit)
        {
            _mockCompanyRepo.Setup(x => x.GetById(co.Id))
                .Returns(co);

            _mockCreditService.Setup(x => x.GetCreditLimit(_newCust.Firstname, _newCust.Surname, _newCust.DateOfBirth))
                .ReturnsAsync(credLimit);

            _customerService = new CustomerService(_mockCompanyRepo.Object, _mockCreditService.Object);

            var result = _customerService?.AddCustomer
                (_newCust.Firstname, _newCust.Surname, _newCust.EmailAddress, _newCust.DateOfBirth, co.Id);

            Assert.IsFalse(result);
        }
    }
}
