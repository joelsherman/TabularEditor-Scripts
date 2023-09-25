/*
 * Title: Use GPT-4 to auto-document your model measures
 * 
 * Author: Darren Gosbell, with prompt and model refinements by Joel Sherman
 * https://darren.gosbell.com/2023/02/automatically-generating-measure-descriptions-for-power-bi-and-analysis-services-with-chatgpt-and-tabular-editor/
 * 
 * This script, when executed, will loop through each measure in the model, calling and prompting GPT-4 to write a text description
 * of the measure, including its DAX syntax.  You will need an OpenAI API key for this script, see below, and review Darren's
 * article above.
 */

#r "System.Net.Http"
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

// You need to signin to https://platform.openai.com/ and create an API key for your profile then paste that key 
// into the apiKey constant below
const string apiKey = "<your api key here>";
const string uri = "https://api.openai.com/v1/completions";
const string question = "You are an expert in interpreting DAX. Explain the following DAX expression in a few sentences, and in simple business terms without using DAX function names:\n\n";

using (var client = new HttpClient()) {
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

    foreach (var t in Model.Tables)
    {
        foreach ( var m in t.Measures)
        {
            // Only uncomment the following when running from the command line or the script will 
            // show a popup after each measure
            //Info("Processing " + m.DaxObjectFullName) 
            //var body = new requestBody() { prompt = question + m.Expression   };
            var body = 
                "{ \"prompt\": " + JsonConvert.SerializeObject( question + m.Expression ) + 
                ",\"model\": \"gpt-4\" " +
                ",\"temperature\": 1 " +
                ",\"max_tokens\": 2048 " +
                ",\"stop\": \".\" }";

            var res = client.PostAsync(uri, new StringContent(body, Encoding.UTF8,"application/json"));
            res.Result.EnsureSuccessStatusCode();
            var result = res.Result.Content.ReadAsStringAsync().Result;
            var obj = JObject.Parse(result);
            var desc = obj["choices"][0]["text"].ToString().Trim();
            m.Description = desc + "\n=====\n" + m.Expression;
        }

    }
}