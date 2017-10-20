using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.Cognitive.CustomVision;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace HotDogOrNot
{
    public partial class HotDogOrNotPage : ContentPage
    {
        Guid projectId = Guid.Parse("");
        const string predictionKey = "";
        PredictionEndpointCredentials predictionEndpointCredentials;
        PredictionEndpoint endpoint;
        MediaFile file;
        Stream savedStream;

        public HotDogOrNotPage()
        {
            InitializeComponent();
            predictionEndpointCredentials = new PredictionEndpointCredentials(predictionKey);
            endpoint = new PredictionEndpoint(predictionEndpointCredentials);
        }

        async void HandleTake_Clicked(object sender, System.EventArgs e)
        {

            file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 256,
                Directory = "Sample",
                Name = "test.jpg"
            });

            if (file == null)
                return;


            ImageMain.Source = ImageSource.FromStream(() =>
            {
                var stream = savedStream = file.GetStream();
                return stream;
            });
        }


        async void HandlePick_Clicked(object sender, System.EventArgs e)
        {
            file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 256
            });


            if (file == null)
                return;

            ImageMain.Source = ImageSource.FromStream(() =>
            {
                var stream = savedStream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }

        async void HandleAnalyze_Clicked(object sender, System.EventArgs e)
        {
            await Predict();
        }


        async Task Predict()
        {
            try
            {
                IsProcessing.IsVisible = true;
                var stream = file.GetStream();

                var result = await endpoint.PredictImageAsync(projectId,stream);
                
                var observations = result.Predictions
                        .OrderByDescending(x => x.Probability)
                        .ToList();
                

                var observation = result.Predictions.FirstOrDefault();
                if (observation != null)
                {
                    var good = observation.Probability > 0.8;
                    var name = observation.Tag.Replace('-', ' ').ToUpperInvariant();
                    var title = good ? $"{name}" : $"maybe {name}";
                    var message = good ? $"I am {Math.Round(observation.Probability * 100)}% sure." : "";
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                IsProcessing.IsVisible = false;
            }
        }

    }
}
