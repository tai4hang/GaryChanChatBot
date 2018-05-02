using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System;
using System.Net;

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.ProjectOxford.Vision;
using System.Web.Configuration;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly ICaptionService captionService = new MicrosoftCognitiveCaptionService();
        private static readonly bool IsSpellCorrectionEnabled = bool.Parse(WebConfigurationManager.AppSettings["IsSpellCorrectionEnabled"]);

        private readonly BingSpellCheckService spellService = new BingSpellCheckService();

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                string message;
                Activity reply;

                try
                {
                    message = await this.GetCaptionAsync(activity, connector);
                    reply = activity.CreateReply(message);
                    //Activity reply = activity.CreateReply(message);
                    await connector.Conversations.ReplyToActivityAsync(reply);

                }
                catch (ArgumentException e)
                {
                    //message = "Did you upload an image? I'm more of a visual person. " +
                    //    "Try sending me an image or an image URL";

                    //Trace.TraceError(e.ToString());

                    if (activity.Text.Contains("lp-sup"))
                    {
                        if (IsSpellCorrectionEnabled)
                        {
                            try
                            {
                                activity.Text = await this.spellService.GetCorrectedTextAsync(activity.Text);
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError(ex.ToString());
                            }
                        }
                        await Conversation.SendAsync(activity, () => new RootLuisDialog());
                    }
                    else
                    {
                        // calculate some   thing for us to return
                        int length = (activity.Text ?? string.Empty).Length;

                        // return our reply to the user
                        reply = activity.CreateReply($"echo: You sent {activity.Text} which was {length} characters");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }
                catch (Exception e)
                {
                    message = "Oops! Something went wrong. Try again later";
                    if (e is ClientException && (e as ClientException).Error.Message.ToLowerInvariant().Contains("access denied"))
                    {
                        message += " (access denied - hint: check your APIKEY at web.config).";
                    }

                    //int length = (activity.Text ?? string.Empty).Length;
                    //reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                    reply = activity.CreateReply(message);
                    Trace.TraceError(e.ToString());
                    //Activity reply = activity.CreateReply(message);
                    await connector.Conversations.ReplyToActivityAsync(reply);

                }

            }
            else
            {
                this.HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        private static async Task<Stream> GetImageStream(ConnectorClient connector, Attachment imageAttachment)
        {
            using (var httpClient = new HttpClient())
            {
                // The Skype attachment URLs are secured by JwtToken,
                // you should set the JwtToken of your bot as the authorization header for the GET request your bot initiates to fetch the image.
                // https://github.com/Microsoft/BotBuilder/issues/662
                var uri = new Uri(imageAttachment.ContentUrl);
                if (uri.Host.EndsWith("skype.com") && uri.Scheme == "https")
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync(connector));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                }

                return await httpClient.GetStreamAsync(uri);
            }
        }

        /// <summary>
        /// Gets the href value in an anchor element.
        /// </summary>
        ///  Skype transforms raw urls to html. Here we extract the href value from the url
        /// <param name="text">Anchor tag html.</param>
        /// <param name="url">Url if valid anchor tag, null otherwise</param>
        /// <returns>True if valid anchor element</returns>
        private static bool TryParseAnchorTag(string text, out string url)
        {
            var regex = new Regex("^<a href=\"(?<href>[^\"]*)\">[^<]*</a>$", RegexOptions.IgnoreCase);
            url = regex.Matches(text).OfType<Match>().Select(m => m.Groups["href"].Value).FirstOrDefault();
            return url != null;
        }

        /// <summary>
        /// Gets the JwT token of the bot. 
        /// </summary>
        /// <param name="connector"></param>
        /// <returns>JwT token of the bot</returns>
        private static async Task<string> GetTokenAsync(ConnectorClient connector)
        {
            var credentials = connector.Credentials as MicrosoftAppCredentials;
            if (credentials != null)
            {
                return await credentials.GetTokenAsync();
            }

            return null;
        }

        /// <summary>
        /// Gets the caption asynchronously by checking the type of the image (stream vs URL)
        /// and calling the appropriate caption service method.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="connector">The connector.</param>
        /// <returns>The caption if found</returns>
        /// <exception cref="ArgumentException">The activity doesn't contain a valid image attachment or an image URL.</exception>
        private async Task<string> GetCaptionAsync(Activity activity, ConnectorClient connector)
        {
            var imageAttachment = activity.Attachments?.FirstOrDefault(a => a.ContentType.Contains("image"));
            if (imageAttachment != null)
            {
                using (var stream = await GetImageStream(connector, imageAttachment))
                {
                    return await this.captionService.GetCaptionAsync(stream);
                }
            }

            string url;
            if (TryParseAnchorTag(activity.Text, out url))
            {
                return await this.captionService.GetCaptionAsync(url);
            }

            if (Uri.IsWellFormedUriString(activity.Text, UriKind.Absolute))
            {
                return await this.captionService.GetCaptionAsync(activity.Text);
            }

            // If we reach here then the activity is neither an image attachment nor an image URL.
            throw new ArgumentException("The activity doesn't contain a valid image attachment or an image URL.");
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}