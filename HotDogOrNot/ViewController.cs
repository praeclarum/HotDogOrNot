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
using SceneKit;
using AVFoundation;
using CoreImage;

namespace HotDogOrNot
{
	public partial class ViewController : UIViewController
	{
		ARSCNView arView;

		UIImageView imgView;

		MLModel model;
		VNCoreMLRequest classificationRequest;
		bool classifying;
		bool arkitSupported;

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
						classificationRequest = new VNCoreMLRequest (nvModel, HandleVNRequest);
					}
				}
			}
			if (error != null) {
				Console.WriteLine ($"ERROR LOADING MODEL: {error}");
			}

			arkitSupported = ARConfiguration.IsSupported;

			if (arkitSupported) {
				arView = new ARSCNView () {
					Frame = View.Bounds,
					AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
				};
				arView.AddGestureRecognizer (new UITapGestureRecognizer (HandleARTapped));
				View.AddSubview (arView);
			}
			else {
				imgView = new UIImageView (View.Bounds) {
					BackgroundColor = UIColor.Black,
					ContentMode = UIViewContentMode.ScaleAspectFill,
					UserInteractionEnabled = true,
					Frame = View.Bounds,
					AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
				};
				imgView.AddGestureRecognizer (new UITapGestureRecognizer (HandleImageTapped));
				View.AddSubview (imgView);
			}
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (arkitSupported) {
				var config = new ARWorldTrackingConfiguration {
					WorldAlignment = ARWorldAlignment.Gravity,
				};
				arView.Session.Run (config, (ARSessionRunOptions)0);
			}
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			if (arkitSupported) {
				arView.Session.Pause ();
			}
		}

		void HandleARTapped ()
		{
			if (classifying)
				return;

			var image = arView.Session?.CurrentFrame?.CapturedImage;
			if (image == null) {
				Console.WriteLine ("NO IMAGE");
				return;
			}

			classifying = true;

			var handler = new VNImageRequestHandler (image, CGImagePropertyOrientation.Up, new VNImageOptions ());

			Task.Run (() => {
				handler.Perform (new[] { classificationRequest }, out var error);
				if (error != null) {
					Console.WriteLine ($"ERROR PERFORMING REQUEST: {error}");
				}
			});
		}

		void HandleImageTapped ()
		{
			if (classifying)
				return;

			var picker = new UIImagePickerController {
				AllowsEditing = false,
				SourceType = UIImagePickerControllerSourceType.Camera
			};

			picker.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;

			picker.FinishedPickingMedia += (s, e) => {
				base.DismissViewController (true, () => {
					var image = e.OriginalImage;
					imgView.Image = image;

					var ciImage = new CIImage (image);

					Task.Run (() => {
						var handler = new VNImageRequestHandler (ciImage, new VNImageOptions ());
						handler.Perform (new[] { classificationRequest }, out var error);
						if (error != null) {
							Console.WriteLine ($"ERROR PERFORMING REQUEST: {error}");
						}
					});
				});
			};

			PresentViewController (picker, true, null);
		}

		void HandleVNRequest (VNRequest request, NSError error)
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
			var name = observation.Identifier.Replace ('-', ' ').ToUpperInvariant ();
			var title = good ? $"{name}" : $"maybe {name}";
			var message = $"I am {Math.Round (observation.Confidence * 100)}% sure.";

			BeginInvokeOnMainThread (() => {
				var alert = UIAlertController.Create (title, message, UIAlertControllerStyle.Alert);
				alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, _ => { }));
				Console.WriteLine (title + "-" + message);
				PresentViewController (alert, true, null);
			});
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
		}
	}
}
