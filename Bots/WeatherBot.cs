// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using OpenWeatherMap;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

using Microsoft.Bot.Connector;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler
    {

        private readonly string[] _cards =
        {
            Path.Combine(".", "Resources", "Weather.json"),
          

        };

        private static Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        private static JObject readFileforUpdate_jobj(string filepath)
        {
            var json = File.ReadAllText(filepath);
            var jobj = JsonConvert.DeserializeObject(json);
            JObject Jobj_card = JObject.FromObject(jobj) as JObject;
            return Jobj_card;
        }
        private static Attachment UpdateAdaptivecardAttachment(JObject updateAttch)
        {
            
            var adaptiveCardAttch = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(updateAttch.ToString()),
            };
            return adaptiveCardAttch;
        }
        private readonly string[] _images =
    {
          Path.Combine(".", "Resources", "weatherbot.jpg"),
        };
        public static string ImageToBase64(string filePath)
        {
            Byte[] bytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(bytes);
            return "data:image/jpg;base64," + base64String;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var client = new OpenWeatherMapClient("47d4faed8ac5819ab1e805525716ee78");
            var CloudImage = "http://messagecardplayground.azurewebsites.net/assets/Mostly%20Cloudy-Square.png";
            var DizzleImage = "http://messagecardplayground.azurewebsites.net/assets/Drizzle-Square.png";
            var rainImage  = "https://raw.githubusercontent.com/zayedrais/WeatherBot/master/rain.png";
            var stormImage = "https://raw.githubusercontent.com/zayedrais/WeatherBot/master/storm.png";
            var sunImage = "https://raw.githubusercontent.com/zayedrais/WeatherBot/master/sun.png";
            var currentWeather = await client.CurrentWeather.GetByName(turnContext.Activity.Text);
            var search =await client.Search.GetByName("Chennai");
            var forcast  = await client.Forecast.GetByName("Chennai");
            var curtTemp = currentWeather.Temperature.Value - 273.15;
            var MaxTemp  = currentWeather.Temperature.Max -273.15;
            var MinTemp  = currentWeather.Temperature.Min -273.15;
            var updateCard = readFileforUpdate_jobj(_cards[0]);
            JToken cityName = updateCard.SelectToken("body[0].text");
            JToken tdyDate = updateCard.SelectToken("body[1].text");
            JToken curTemp = updateCard.SelectToken("body[2].columns[1].items[0].text");
            JToken maxTem = updateCard.SelectToken("body[2].columns[3].items[0].text");
            JToken minTem = updateCard.SelectToken("body[2].columns[3].items[1].text");
            JToken weatherImageUrl = updateCard.SelectToken("body[2].columns[0].items[0].url");

 
            cityName.Replace(currentWeather.City.Name);
            curTemp.Replace(curtTemp.ToString("N0"));
            tdyDate.Replace(DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            maxTem.Replace("Max" +" "+MaxTemp.ToString("N0"));
            minTem.Replace("Min" + " "+MinTemp.ToString("N0"));
            var n = currentWeather.Clouds.Name;
           
            if(n=="overcast clouds")
            {
                weatherImageUrl.Replace(rainImage);
            }
            else if (n.Contains("clouds"))
            {
                weatherImageUrl.Replace(CloudImage);
            }
            else if (n.Contains("sky"))
            {
                weatherImageUrl.Replace(sunImage);
            }
            else if (n.Contains("rain"))
            {
             weatherImageUrl.Replace(rainImage);
            }
            else if(n.Contains("storm") || n.Contains("thunder"))
            {
             weatherImageUrl.Replace(stormImage);
            }           

            var updateWeatherTem = UpdateAdaptivecardAttachment(updateCard);
          
            await turnContext.SendActivityAsync(MessageFactory.Attachment(updateWeatherTem), cancellationToken);
            
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            IConversationUpdateActivity iConversationUpdated = turnContext.Activity as IConversationUpdateActivity;
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var card = new HeroCard
                    {
                    Title = "Welcome to WeatherBot",
                    Text = @"Type the city name for get the weather report",
                    Images = new List<CardImage>() { new CardImage(ImageToBase64(_images[0])) },
                    };
            
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
                }
            }
        }
    }
}
