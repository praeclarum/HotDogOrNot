using System;

using Foundation;
using UIKit;
using ARKit;
using CoreVideo;
using CoreGraphics;
using CoreML;

namespace HotDogOrNot
{
	public partial class ViewController : UIViewController
	{
		readonly ARSCNView cameraView = new ARSCNView ();
		MLModel model;

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
			var image = cameraView.Session?.CurrentFrame?.CapturedImage;
			if (image == null) {
				Console.WriteLine ("NO IMAGE");
				return;
			}

			Console.WriteLine (image);
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
		}
	}
}
