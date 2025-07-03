using System.Text.Json;
using System.Text.RegularExpressions;
using Core_DataAccessPlugIns.Models;
using Core_DataAccessPlugIns.PluginServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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

        //public async Task<string> GetCustomersByCityAndCountry(string city, string country)
        public async Task<string> GetCustomersByCityAndCountry(string userMessage)
        {
            // read the prompt and parse it to get city and country
            var dictionary = await ParsePromptToDictionaryValues(userMessage);
            string city = dictionary.ContainsKey("CityName") ? dictionary["CityName"] : null;
            string country = dictionary.ContainsKey("CountryName") ? dictionary["CountryName"] : null;

             

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

        //public async Task<string> GetFreightDetailsAsync(
        //    string? propertyname = null,
        //    string? propertyvalue = null,
        //    string? operation = null)
        public async Task<string> GetFreightDetailsAsync(
            string userMessage)
        {
            // read the prompt and parse it to get propertyname, propertyvalue and operation
            var dictionary = await ParsePromptToDictionaryValues(userMessage);
            string propertyname = string.Empty;
            string propertyvalue = string.Empty;
            string operation = dictionary.ContainsKey("Operation") ? dictionary["Operation"] : null;

            if (dictionary.ContainsKey("City") && !string.IsNullOrEmpty(dictionary["City"])) 
            {
                propertyname = "City";
                propertyvalue = dictionary["City"];
            }
            else if(dictionary.ContainsKey("Country") && !string.IsNullOrEmpty(dictionary["Country"]))
            {
                propertyname = "Country";
                propertyvalue = dictionary["Country"];
            }
            else if(dictionary.ContainsKey("ShipName") && !string.IsNullOrEmpty(dictionary["ShipName"]))
            {
                propertyname = "ShipName";
                propertyvalue = dictionary["ShipName"];
            }
            else if(dictionary.ContainsKey("EmployeeName") && !string.IsNullOrEmpty(dictionary["EmployeeName"]))
            {
                propertyname = "EmployeeName";
                propertyvalue = dictionary["EmployeeName"];
            }
            else
            {
                throw new ArgumentException("Invalid property name in the prompt.");
            }   



            //string userMessage = $"Get me the freight details for {propertyname}, {propertyvalue} with operation {operation}";
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


        private async Task<Dictionary<string, string>> ParsePromptToDictionaryValues(string prompt)
        {
            var dictionary = new Dictionary<string, string>();

            // Step 2: Define the prompt template
            /*
             Semantic Kernel expects a variable named "input" in the arguments. But if you pass a plain string, it automatically maps it to the default input variable. If you use KernelArguments, then you must ensure the variable name matches exactly.
            Use {{$input}} in the prompt and pass a plain string.
             */
            string systemprompt = @"
            Extract the following information from the text:
            - CustomerName
            - City
            - Country
            - ShipName
            - EmployeeName
            - Operation
            Here the operation is like sum, average, etc.
            Return the result in JSON format.

            Text: {{$input}}

            JSON:
            ";


            // Lets load the semantic Funciton
            var function = _kernel.CreateFunctionFromPrompt(
             promptTemplate: systemprompt,
             executionSettings: new OpenAIPromptExecutionSettings
             {
                 MaxTokens = 200,
                 Temperature = 0,
                 TopP = 1
             });
            // This name must match with {{$input}} in the prompt
            var input = new KernelArguments
            {
                ["input"] = prompt
            };
            // Invoke the function
            var result = await _kernel.InvokeAsync(function, input);
            
            // Step 6: Parse and display the result
            var json = result.GetValue<string>();
            dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return dictionary;
        }
    }
}
