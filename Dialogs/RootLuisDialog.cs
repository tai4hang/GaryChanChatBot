namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;

    [LuisModel("991b234c-debf-46e1-9d12-cf03a833d203", "ccf7ff79ae2c46afb4aa73e29aacfe0b")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private const string EntityGeographyCity = "builtin.geography.city";

        private const string EntityUnitLoadName = "UnitLoad";
        private const string EntityLocationName = "Location";

        private const string EntityAirportCode = "AirportCode";

        private IList<string> titleOptions = new List<string> { "¡°Very stylish, great stay, great staff¡±", "¡°good unitLoad awful meals¡±", "¡°Need more attention to little things¡±", "¡°Lovely small unitLoad ideally situated to explore the area.¡±", "¡°Positive surprise¡±", "¡°Beautiful suite and resort¡±" };

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("SearchUnitLoads")]
        public async Task Search(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync($"Welcome to the Unit Load inventory list! We are analyzing your message: '{message.Text}'...");

            var unitLoadQuery = new UnitLoadQuery();

            EntityRecommendation cityEntityRecommendation;
            EntityRecommendation locationEntityRecommendation;

            if (result.TryFindEntity(EntityUnitLoadName, out cityEntityRecommendation))
            {
                cityEntityRecommendation.Type = "UldId";
            }

            //var unitLoadsFormDialog = new FormDialog<UnitLoadQuery>(unitLoadQuery, this.BuildUnitLoadsForm, FormOptions.PromptInStart, result.Entities);

            //context.Call(unitLoadsFormDialog, this.ResumeAfterUnitLoadsFormDialog);
        }

        [LuisIntent("ShowCompUtil")]
        public async Task ShowCompUtil(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            //await context.PostAsync($"Welcome to the Unit Load inventory list! We are analyzing your message: '{message.Text}'...");

            var compUtilQuery = new CompUtilQuery();

            var computilsFormDialog = new FormDialog<CompUtilQuery>(compUtilQuery, this.BuildCompUtilsForm, FormOptions.PromptInStart, result.Entities);

            context.Call(computilsFormDialog, this.ResumeAfterCompUtilsFormDialog);
        }

        [LuisIntent("ShowUnitLoadsReviews")]
        public async Task Reviews(IDialogContext context, LuisResult result)
        {
            EntityRecommendation unitLoadEntityRecommendation;
            EntityRecommendation locationEntityRecommendation;

            if (result.TryFindEntity(EntityUnitLoadName, out unitLoadEntityRecommendation))
            {
                await context.PostAsync($"Looking for inventory data of '{unitLoadEntityRecommendation.Entity}'...");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                for (int i = 0; i < 5; i++)
                {
                    var random = new Random(i);
                    ThumbnailCard thumbnailCard = new ThumbnailCard()
                    {
                        Title = this.titleOptions[random.Next(0, this.titleOptions.Count - 1)],
                        Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris odio magna, sodales vel ligula sit amet, vulputate vehicula velit. Nulla quis consectetur neque, sed commodo metus.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = "https://upload.wikimedia.org/wikipedia/en/e/ee/Unknown-person.gif" }
                        },
                    };

                    resultMessage.Attachments.Add(thumbnailCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hi! Try asking me things like 'search AKE12345KE', 'check FR9Q0026' or 'show me the compartment utilization'");

            context.Wait(this.MessageReceived);
        }

        private IForm<CompUtilQuery> BuildCompUtilsForm()
        {
            OnCompletionAsyncDelegate<CompUtilQuery> processCompUtilsSearch = async (context, state) =>
            {
                var message = "Calculating comp util";
                if (!string.IsNullOrEmpty(state.Zone))
                {
                    message += $" in {state.Zone}...";
                }

                await context.PostAsync(message);
            };

            return new FormBuilder<CompUtilQuery>()
                .Field(nameof(CompUtilQuery.Zone), (state) => string.IsNullOrEmpty(state.Zone))
                .OnCompletion(processCompUtilsSearch)
                .Build();
        }

        private async Task ResumeAfterCompUtilsFormDialog(IDialogContext context, IAwaitable<CompUtilQuery> result)
        {
            try
            {
                var searchQuery = await result;

                // for demo only, it should invoke service to get comp util
                string reply = $"Zone {searchQuery.Zone}:( utilization: 60%; reshuffling buffer: LHx2...)";

                await context.PostAsync(reply);

                ////var compUtils = await this.GetCompUtilsAsync(searchQuery);
                ////await context.PostAsync($"I found {unitLoads.Count()} unitLoads:");

                //var resultMessage = context.MakeMessage();
                //resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                //resultMessage.Attachments = new List<Attachment>();

                //foreach (var unitLoad in unitLoads)
                //{
                //    HeroCard heroCard = new HeroCard()
                //    {
                //        Title = unitLoad.UldId,
                //        Subtitle = $"Compartment: {unitLoad.Location}. {unitLoad.LoadingStatus} ${unitLoad.Flight} ${unitLoad.Contour} .",
                //        Images = new List<CardImage>()
                //        {
                //            new CardImage() { Url = unitLoad.Image }
                //        },
                //        Buttons = new List<CardAction>()
                //        {
                //            new CardAction()
                //            {
                //                Title = "More details",
                //                Type = ActionTypes.OpenUrl,
                //                Value = $"https://www.bing.com/search?q=unitLoads+in+" + HttpUtility.UrlEncode(unitLoad.Location)
                //            }
                //        }
                //    };

                //    resultMessage.Attachments.Add(heroCard.ToAttachment());
                //}

                //await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        private async Task<IEnumerable<UnitLoad>> GetLocationsAsync(UnitLoadQuery searchQuery)
        {
            var unitLoads = new List<UnitLoad>();

            // Filling the unitLoads results manually just for demo purposes
            for (int i = 1; i <= 5; i++)
            {
                var random = new Random(i);
                UnitLoad unitLoad = new UnitLoad()
                {
                    UldId = $"{searchQuery.UldId ?? searchQuery.Zone} UnitLoad {i}",
                    Location = searchQuery.UldId ?? searchQuery.Zone,
                    LoadingStatus = "Loaded",
                    Flight = "",
                    Contour = "LH",
                    Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=UnitLoad+{i}&w=500&h=260"
                };

                unitLoads.Add(unitLoad);
            }

            unitLoads.Sort((h1, h2) => h1.UldId.CompareTo(h2.UldId));

            return unitLoads;
        }
    }
}