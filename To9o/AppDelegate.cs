using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using System.IO;
using SQLite;
using CoreSpotlight;

namespace StoryboardTables
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		
		public override UIWindow Window {
			get;
			set;
		}

		#region Database set-up
		public static AppDelegate Current { get; private set; }
		public TaskManager TaskMgr { get; set; }
		SQLite.SQLiteConnection conn;
		#endregion


		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			Current = this;

			#region Database set-up
			var sqliteFilename = "TaskDB.db3";
			// we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
			// (they don't want non-user-generated data in Documents)
			string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
			string libraryPath = Path.Combine (documentsPath, "..", "Library"); // Library folder
			var path = Path.Combine(libraryPath, sqliteFilename);
			conn = new SQLite.SQLiteConnection(path);
			TaskMgr = new TaskManager(conn);
			#endregion

			Console.WriteLine ("bbbbbbbbbb FinishedLaunching");

			#region Quick Action
			var shouldPerformAdditionalDelegateHandling = true;

			// Get possible shortcut item
			if (launchOptions != null) {
				LaunchedShortcutItem = launchOptions [UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
				shouldPerformAdditionalDelegateHandling = (LaunchedShortcutItem == null);
			}
			#endregion

			return shouldPerformAdditionalDelegateHandling;
		}

		#region Quick Action
		public UIApplicationShortcutItem LaunchedShortcutItem { get; set; }
		public override void OnActivated (UIApplication application)
		{
			Console.WriteLine ("ccccccc OnActivated");

			// Handle any shortcut item being selected
			HandleShortcutItem(LaunchedShortcutItem);

			// Clear shortcut after it's been handled
			LaunchedShortcutItem = null;
		}
		// if app is already running
		public override void PerformActionForShortcutItem (UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
		{
			Console.WriteLine ("dddddddd PerformActionForShortcutItem");
			// Perform action
			var handled = HandleShortcutItem(shortcutItem);
			completionHandler(handled);
		}
		public bool HandleShortcutItem(UIApplicationShortcutItem shortcutItem) {
			Console.WriteLine ("eeeeeeeeeee HandleShortcutItem ");
			var handled = false;

			// Anything to process?
			if (shortcutItem == null) return false;

			// Take action based on the shortcut type
			switch (shortcutItem.Type) {
			case ShortcutIdentifiers.Add:
				Console.WriteLine ("QUICKACTION: Add new item");
				handled = true;
				break;
			case ShortcutIdentifiers.Share:
				Console.WriteLine ("QUICKACTION: Share summary of items");
				handled = true;
				break;
			}

			//HACK: show the detail viewcontroller
			ContinueNavigation ();

			Console.Write (handled);
			// Return results
			return handled;
		}
		#endregion

		#region NSUserActivity
		// http://www.raywenderlich.com/84174/ios-8-handoff-tutorial
		public override bool ContinueUserActivity (UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
		{
			Console.WriteLine ("ffffffff ContinueUserActivity");

			UIViewController tvc = null;
			if ((userActivity.ActivityType == ActivityTypes.Add) 
				|| (userActivity.ActivityType == ActivityTypes.Detail))
			{
				if (userActivity.UserInfo.Count == 0) {
					// new item

				} else {
					var uid = userActivity.UserInfo.ObjectForKey ((NSString)"id").ToString ();
					if (uid == "0") {
						Console.WriteLine ("No userinfo found for " + ActivityTypes.Detail);
					} else {
						Console.WriteLine ("Should display id " + uid);
						// handled in DetailViewController.RestoreUserActivityState
					}
				}
				tvc = ContinueNavigation ();
			}
			if (userActivity.ActivityType == CSSearchableItem.ActionType) {
				var uid = userActivity.UserInfo.ObjectForKey (CSSearchableItem.ActivityIdentifier).ToString();

				System.Console.WriteLine ("Show the detail for id:" + uid);

				tvc = ContinueNavigation ();
			}
			completionHandler(new NSObject[] {tvc});

			return true;
		}
		#endregion

		UIViewController ContinueNavigation (){
			Console.WriteLine ("gggggggggg ContinueNavigation");

			var sb = UIStoryboard.FromName ("MainStoryboard", null);
			var tvc = sb.InstantiateViewController("detailvc") as DetailViewController;

			var r = Window.RootViewController as UINavigationController;
			r.PopToRootViewController (false);

			tvc.SetTodo (new Task {Name="(new action)"}); // from 3D Touch menu
			r.PushViewController (tvc, false);
			return tvc;
		}

		#region Lifecycle
		//
		// This method is invoked when the application is about to move from active to inactive state.
		//
		// OpenGL applications should use this method to pause.
		//
		public override void OnResignActivation (UIApplication application)
		{
		}
		
		// This method should be used to release shared resources and it should store the application state.
		// If your application supports background exection this method is called instead of WillTerminate
		// when the user quits.
		public override void DidEnterBackground (UIApplication application)
		{
		}
		
		// This method is called as part of the transiton from background to active state.
		public override void WillEnterForeground (UIApplication application)
		{
		}
		
		// This method is called when the application is about to terminate. Save data, if needed. 
		public override void WillTerminate (UIApplication application)
		{
		}
		#endregion
	}
}
