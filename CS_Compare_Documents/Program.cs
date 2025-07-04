using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.OpenAI;
using OpenAI.Chat;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Document Comparision");

string docIntelligenceEndpoint = "[AZURE-DOCUMENT-INTELLIGENCE-SERVICE-ENDPOINT]";
string docIntelligenceKey = "[AZURE-DOCUMENT-INTELLIGENCE-SERVICE]";

var documentIntelligenceClient = new DocumentIntelligenceClient(
    new Uri(docIntelligenceEndpoint),
    new AzureKeyCredential(docIntelligenceKey)
);

string azureOpenAIEndpoint = "[AZURE-OPEN-AI-SERVICE-ENDPOINT]";
string azureOpenAIKey = "[AZURE-OPEN-AI-KEY]";
string azureOpenAIModelVersion = "[M)DEL-VERSION]";
string deploymentName = "[AZURE-OPEN-AI-GPT-DEPLOYMENT-NAME]";

var openAIClient = new AzureOpenAIClient(
	new Uri(azureOpenAIEndpoint),
	new AzureKeyCredential(azureOpenAIKey)
);

try
{
    // code to read the path of the document file from the files directory of the current project
    // Modify the following code to read the file path from the files directory and not from the bin directory

    string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;

    // Combine with relative path to Files folder
    string filePath_1 = Path.Combine(projectRoot, "Files", "Leave-Policy-Aus.pdf");
    string filePath_2 = Path.Combine(projectRoot, "Files", "US_Labbor_LEave_Policy.pdf");

    string documentText_1 = await Extract_Text_From_Document(filePath_1);
    string documentText_2 = await Extract_Text_From_Document(filePath_2);

    // Use GPT to analyze the document text for comparison

    var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage("You are an AI that generates SQL queries."),
                    ChatMessage.CreateUserMessage($"Compare the following two documents:\n\nDocument 1:\n{documentText_1}\n\nDocument 2:\n{documentText_2}"),
                };

    var chatClient = openAIClient.GetChatClient(deploymentName);


    // Create a chat completion request

    var options = new ChatCompletionOptions
    {
        Temperature = 0.5f
    };


    var completion = await chatClient.CompleteChatAsync(messages, options);


    // Extract the text from the ChatMessageContent object
    string result = string.Join("", completion.Value.Content.Select(part => part.Text));
    

    Console.WriteLine($"Comparison Result:\n{result}");



}
catch (Exception ex)
{
    Console.WriteLine($"Error Occurred:{ex.Message}");
}


Console.ReadLine();

async Task<string> Extract_Text_From_Document(string file_path)
{
	string document_text = string.Empty;	
    try
	{
        using var stream = File.OpenRead(file_path);
        var binaryData = BinaryData.FromStream(stream);

        var doc_analysis_result = await documentIntelligenceClient.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", binaryData);

        AnalyzeResult result = doc_analysis_result.Value;

        foreach (var page in result.Pages)
        {
            //Console.WriteLine($"Page {page.PageNumber} has {page.Lines.Count} lines.");
            foreach (var line in page.Lines)
            {
                document_text += $"  Line: {line.Content}";
            }
        }
    }
	catch (Exception ex)
	{
		throw ex;
	}
	return document_text;
}