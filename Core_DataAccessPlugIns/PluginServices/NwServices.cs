using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Core_DataAccessPlugIns.Models;
using Microsoft.SemanticKernel;
using System.Linq.Dynamic.Core;

namespace Core_DataAccessPlugIns.PluginServices
{
    public class NwServices
    {
        private readonly NwContext _context;

        public NwServices(NwContext context)
        {
            _context = context;
        }
        [KernelFunction("GetCustomerInfo")]
        [Description("Get customer information based on city and country.")]
        public async Task<List<Customer>> GetCustomersAsync(string? city, string? country)
        {
            List<Customer> customers = null;
            
            if(city is null && country is null)
            {
                customers = await _context.Customers.ToListAsync();
            }

            else if(city is not null && country is null)
            {
                customers = await _context.Customers
                    .Where(c => c.City == city)
                    .ToListAsync();
            }
            else if(city is null && country is not null)
            {
                customers = await _context.Customers
                    .Where(c => c.Country == country)
                    .ToListAsync();
            }
            else
            {
                customers = await _context.Customers
                    .Where(c => c.City == city && c.Country == country)
                    .ToListAsync();
            }

            return customers;
        }


        [KernelFunction("OrderDetailsDynamic")]
        [Description("Get order details with various operations like, sum, details by  shipcity,shipcountry")]
        public async Task<object> GetOrderDetailsAsync(
            //string? shipCity = null,
            //string? shipCountry = null,
            string? propertyName,
            string? propertyValue = null,
            string? operation=null)
        {
            object dynamicResult = null;

            switch (operation)
            {
                case "sum":

                    // In the context of System.Linq.Dynamic.Core, the keyword "it" is a placeholder for the current object in the collection—similar to how you'd use a lambda parameter like x in traditional LINQ:

                    // Dynamic Where clause
                    string filter = $"{propertyName} == @0";

                    var summary = _context.OrderDetailsDynamics
                        .Where(filter, propertyValue) // Filter dynamically
                        .GroupBy(propertyName, "it")
                        .Select("new (Key as GroupValue, Sum(Freight) as TotalFreight)")
                        .ToDynamicList();

                    dynamicResult = summary; // updated to assign to summary

                    break;
                default:
                    dynamicResult = await _context.OrderDetailsDynamics.ToListAsync();
                    break;
            }

            return dynamicResult;

        }

    }
}
