using System;
using App;

namespace Harness
{
    public class Customer
    {
        public static void Main(string[] args)
        {

        	/*
			 *	You MUST NOT change this code. This is an existing consumer of the CustomerService and you must maintain
			 *  backwards compatibility.
        	*/

            var custService = new CustomerService();
            var addResult = custService.AddCustomer("Joe", "Bloggs", "joe.bloggs@adomain.com", new DateTime(1980, 3, 27), 4);
            Console.WriteLine("Adding Joe Bloggs was " + (addResult ? "successful" : "unsuccessful"));
        }
    }
}
