# Hotdog or Not

This is a small app demonstrating how to use CoreML with Xamarin to build an image detection app. ARKit is used to preview the camera and Vision is used to execute the model.

The model it uses was trained with [Microsoft's Custom Vision service](https://www.customvision.ai).

### iOS
All the interesting code is in [ViewController.cs](HotDogOrNot/ViewController.cs).

### Android
For Android all analysis is done using Custom Vision services. You must setup a model and then fill in the API Keys inside of **HotDogOrNotPage.xaml.cs**

```
Guid projectId = Guid.Parse("");
const string predictionKey = "";
```
### IMPORTANT
Note: Ensure in [Microsoft's Custom Vision service] that a default is set under the Performance Tab after training. (Beside the Prediction URL under the nav bar ). Otherwise the Line within the Predict Method :
```
var result = await endpoint.PredictImageAsync(projectId, stream);  
```
Will return a http response of "Not Found". This is due to the Prediction URL containing the iteration id appended to the end of the url. This is removed once default is set.

![Screenshot of the app detecting a hotdog](Blog/Results.jpg)
