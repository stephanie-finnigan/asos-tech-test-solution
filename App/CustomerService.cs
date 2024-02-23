using App.DataAccess;
using App.Models;
using System;

namespace App
{
    public class CustomerService
    {
        private readonly ICompanyRepository _companyRepo;
        private readonly ICustomerCreditService _customerCreditService;
        private readonly ICustomerDataAccessWrapper _dataAccessWrapper;

        public CustomerService(ICompanyRepository companyRepo = null, ICustomerCreditService customerCreditService = null,
            ICustomerDataAccessWrapper dataAccessWrapper = null) 
        { 
            _companyRepo = companyRepo;
            _customerCreditService = customerCreditService;
            _dataAccessWrapper = dataAccessWrapper;
        }

        public bool AddCustomer(string firstName, string surname, string email, DateTime dateOfBirth, int companyId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(surname))
                return false;

            if (!email.Contains("@") && !email.Contains("."))
                return false;

            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if ((now.Month < dateOfBirth.Month) || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
                age--;

            if (age < 21)
                return false;

            var company = _companyRepo.GetById(companyId);

            var customer = new Customer
            {
                Company = company,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                Firstname = firstName,
                Surname = surname
            };

            var c = CheckCustomerCredit(customer);
            customer = c;

            if (customer.HasCreditLimit && customer.CreditLimit < 500)
                return false;

            _dataAccessWrapper.AddCustomer(customer);

            return true;
        }

        private Customer CheckCustomerCredit(Customer customer)
        {
            var creditLimit = _customerCreditService.GetCreditLimit(customer.Firstname, customer.Surname, customer.DateOfBirth).Result;

            if (customer.Company.Name == "VeryImportantClient")
            {
                // Skip credit check
                customer.HasCreditLimit = false;
            }
            else if (customer.Company.Name == "ImportantClient")
            {
                // Do credit check and double credit limit
                customer.HasCreditLimit = true;
                creditLimit *= 2;
                customer.CreditLimit = creditLimit;
            }
            else
            {
                // Do credit check
                customer.HasCreditLimit = true;
                customer.CreditLimit = creditLimit;
            }

            return customer;
        }
    }
}
