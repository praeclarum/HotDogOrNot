using System;

using UIKit;
using ARKit;
using CoreVideo;
using CoreGraphics;

namespace HotDogOrNot
{
	public partial class ViewController : UIViewController
	{
		readonly ARSCNView cameraView = new ARSCNView ();

		protected ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			cameraView.Frame = View.Bounds;
			cameraView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			cameraView.AddGestureRecognizer (new UITapGestureRecognizer (HandleTapped));
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
