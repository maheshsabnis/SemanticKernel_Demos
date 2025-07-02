using System.Text.Json;
using System.Text.RegularExpressions;
using Core_DataAccessPlugIns.Models;
using Core_DataAccessPlugIns.PluginServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
namespace Core_DataAccessPlugIns.ChatServices
{
    public class ChatInfoGenerator
    {
        private IKernelBuilder _kernelBuilder;
        private Kernel _kernel;
        IChatCompletionService _chatCompletionService;
        AzureOpenAIPromptExecutionSettings settings;
        public ChatInfoGenerator(IConfiguration config, IServiceProvider serviceProvider)
        {
            _kernelBuilder = Kernel.CreateBuilder()
               .AddAzureOpenAIChatCompletion(
                 config["AzureAISettings:AIServiceDeploymentName"],
                   config["AzureAISettings:AIServiceEndpoint"],
                   config["AzureAISettings:AIServiceKey"]
                  );
            _kernel = _kernelBuilder.Build();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            // Resolve the dependency from the service provider
            var nwContext = serviceProvider.GetRequiredService<NwContext>();
            // Create an instance of your plugin with the dependency injected
            var nwServices = new NwServices(nwContext);
            // Register the plugin instance with the kernel
            _kernel.Plugins.AddFromObject(nwServices, "NwServices");

            settings = new AzureOpenAIPromptExecutionSettings
            {
                Temperature = 0.7f,
                MaxTokens = 1000,
                TopP = 0.9f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true),
                ServiceId = "default"

            };

        }

        public async Task<string> GetCustomersByCityAndCountry(string city, string country)
        {

            string userMessage = $"Get me the customers from {city}, {country}";

            string responseContents = string.Empty;
            try
            {

                // Get Customers
                var args = new KernelArguments
                {
                    ["city"] = city,
                    ["country"] = country,
                    ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
                    {
                        ["default"] = settings
                    }
                };
                var customersResult = await _kernel.Plugins.GetFunction("NwServices", "GetCustomerInfo").InvokeAsync(_kernel, args);
                var customers = customersResult.GetValue<List<Customer>>();
                var customerData = JsonSerializer.Serialize(customers, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                string prompt = $@"
                        You are a helpful assistant that provides customer information.

                        User asked: {userMessage}

                        Here is the customer data retrieved from the system:
                        {customerData}

                        Now, summarize this information in a friendly and concise way in Tabular format. Please include columns and rows in the table. Please do not include any additional text outside the table. Please ensure the table is well formatted and easy to read. The column width must be appropriate for the data.
                        ";



                var result = await _kernel.InvokePromptAsync(prompt, args);
                responseContents = result.ToString();

                // Code to convert the response to a more readable format e.g. Table format
                // put the following statement out of the table
                // Here is a summary of the customers from London in a concise table format:


                responseContents = Regex.Replace(responseContents, @"\s+", " ").Trim();
                responseContents = Regex.Replace(responseContents, @"\|", " | ").Trim();
                responseContents = Regex.Replace(responseContents, @"\n", "\n| ").Trim();


            }
            catch (Exception ex)
            {
                throw ex;
            }

            return responseContents;
        }

        public async Task<string> GetFreightDetailsAsync(
            string? propertyname = null,
            string? propertyvalue = null,
            string? operation = null)
        {
            string userMessage = $"Get me the freight details for {propertyname}, {propertyvalue} with operation {operation}";
            string responseContents = string.Empty;
            try
            {
                // Get Order Details
                var args = new KernelArguments
                {
                    ["propertyname"] = propertyname,
                    ["propertyvalue"] = propertyvalue,
                    ["operation"] = operation,
                    ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
                    {
                        ["default"] = settings
                    }
                };
                var orderDetailsResult = await _kernel.Plugins.GetFunction("NwServices", "OrderDetailsDynamic").InvokeAsync(_kernel, args);
                var orderDetails = orderDetailsResult.GetValue<object>();
                var orderData = JsonSerializer.Serialize(orderDetails, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                string prompt = $@"
                        You are a helpful assistant that provides freight details.
                        User asked: {userMessage}
                        Here is the order data retrieved from the system:
                        {orderData}
                        Now, summarize this information in a friendly and concise way in Tabular format. Please include columns and rows in the table. Please do not include any additional text outside the table. Please ensure the table is well formatted and easy to read. The column width must be appropriate for the data.
                        ";
                var result = await _kernel.InvokePromptAsync(prompt, args);
                responseContents = result.ToString();
                responseContents = Regex.Replace(responseContents, @"\s+", " ").Trim();
                responseContents = Regex.Replace(responseContents, @"\|", " | ").Trim();
                responseContents = Regex.Replace(responseContents, @"\n", "\n| ").Trim();

            }


            catch (Exception ex)
            {
                throw ex;
            }
            return responseContents;
        }

    }
}
