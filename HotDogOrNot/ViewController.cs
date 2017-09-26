using System;

using Foundation;
using UIKit;
using ARKit;
using CoreVideo;
using CoreGraphics;
using CoreML;
using CoreFoundation;
using System.Threading.Tasks;
using Vision;
using ImageIO;
using System.Linq;

namespace HotDogOrNot
{
	public partial class ViewController : UIViewController
	{
		readonly ARSCNView cameraView = new ARSCNView ();
		MLModel model;
		VNCoreMLRequest classificationRequest;
		bool classifying;

		protected ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var modelUrl = NSBundle.MainBundle.GetUrlForResource ("HotDogOrNot", "mlmodel");
			var compiledModelUrl = MLModel.CompileModel (modelUrl, out var error);
			if (error == null) {
				model = MLModel.Create (compiledModelUrl, out error);
				Console.WriteLine ($"MODEL LOADED: {model}");
				if (error == null) {
					var nvModel = VNCoreMLModel.FromMLModel (model, out error);
					if (error == null) {
						classificationRequest = new VNCoreMLRequest (nvModel, HandleVNRequestCompletionHandler);
					}
				}
			}
			if (error != null) {
				Console.WriteLine ($"ERROR LOADING MODEL: {error}");
			}

			cameraView.AddGestureRecognizer (new UITapGestureRecognizer (HandleTapped));
			cameraView.Frame = View.Bounds;
			cameraView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			View.AddSubview (cameraView);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			var config = new ARWorldTrackingConfiguration {
				WorldAlignment = ARWorldAlignment.Gravity,
			};
			cameraView.Session.Run (config, (ARSessionRunOptions)0);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			cameraView.Session.Pause ();
		}

		void HandleTapped ()
		{
			if (classifying)
				return;
			
			var image = cameraView.Session?.CurrentFrame?.CapturedImage;
			if (image == null) {
				Console.WriteLine ("NO IMAGE");
				return;
			}

			var handler = new VNImageRequestHandler (image, CGImagePropertyOrientation.Up, new VNImageOptions ());

			Task.Run (() => {
				handler.Perform (new[] { classificationRequest }, out var error);
				if (error != null) {
					Console.WriteLine ($"ERROR PERFORMING REQUEST: {error}");
				}
			});

			Console.WriteLine (image);
		}

		void HandleVNRequestCompletionHandler (VNRequest request, NSError error)
		{
			classifying = false;

			if (error != null)
				return;
			
			var observations =
				request.GetResults<VNClassificationObservation> ()
				       .OrderByDescending (x => x.Confidence)
				       .ToList ();
			foreach (var o in observations) {
				Console.WriteLine ($"{o.Identifier} == {o.Confidence}");
			}
			ShowObservation (observations.First ());
		}

		void ShowObservation (VNClassificationObservation observation)
		{
			var good = observation.Confidence > 0.9;
			var name = observation.Identifier.Replace ('-', ' ');

			BeginInvokeOnMainThread (() => {
				var alert = new UIAlertController ();
				alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, _ => { }));
				alert.Message = good ? $"{name}" : $"maybe {name}";
				Console.WriteLine (alert.Message);
				PresentViewController (alert, true, null);
			});
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
		}
	}
}
