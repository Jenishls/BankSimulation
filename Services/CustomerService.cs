using BankingConsole.Models;

namespace BankingConsole.Services
{
    public class CustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> CreateCustomer()
        {
            var customer = new Customer
            {
                Name = "John Doe",
                Email = "john@gmail.com",
                PhoneNumber = ""
            };

            return customer;
        }

        public async Task<Customer> UpdateCustomer(Customer customer)
        {
            customer.Email = "john.doe@gmail.com";
            return customer;
        }

        
    }
}