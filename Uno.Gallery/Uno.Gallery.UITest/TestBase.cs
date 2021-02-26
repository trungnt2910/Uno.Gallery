﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Uno.UITest;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;
using Uno.UITest.Selenium;
using Uno.UITests.Helpers;

namespace Uno.Gallery.UITests
{
    [TestFixture]
    public abstract class TestBase
    {
        private IApp _app;

        static TestBase()
        {
            AppInitializer.TestEnvironment.AndroidAppName = Constants.AndroidAppName;
            AppInitializer.TestEnvironment.WebAssemblyDefaultUri = Constants.WebAssemblyDefaultUri;
            AppInitializer.TestEnvironment.iOSAppName = Constants.iOSAppName;
            AppInitializer.TestEnvironment.AndroidAppName = Constants.AndroidAppName;
            AppInitializer.TestEnvironment.iOSDeviceNameOrId = Constants.iOSDeviceNameOrId;
            AppInitializer.TestEnvironment.CurrentPlatform = Constants.CurrentPlatform;

#if DEBUG
            AppInitializer.TestEnvironment.WebAssemblyHeadless = false;
#endif
        }

        protected IApp App
        {
            get => _app;
            private set
            {
                _app = value;
                Uno.UITest.Helpers.Queries.Helpers.App = value;
            }
        }
        
        [SetUp]
        public void SetUpTest() 
        {
            AppInitializer.ColdStartApp();
            App = AppInitializer.AttachToApp();
        }

        [TearDown]
        public void TearDownTest()
        {
            TakeScreenshot("teardown");
        }

        protected void NavigateToSection(params string[] sections)
        {
            if (!(sections?.Length > 0)) throw new ArgumentException("`sections` are null or empty", nameof(sections));

            OpenNavView();
           
			foreach (var section in sections)
            {
                var sectionResult = App.Query($"Section_{section}");
                if (!sectionResult.Any())
				{
                    App.ScrollDownTo($"Section_{section}", withinMarked: "MenuItemsScrollViewer");
                }
                App.WaitThenTap($"Section_{section}");
            }
        }

        protected void ShowMaterialTheme()
		{
            App.WaitThenTap("PART_MaterialRadioButton");
        }

        protected void ShowFluentTheme()
        {
            App.WaitThenTap("PART_FluentRadioButton");
        }

        protected void ShowNativeTheme()
        {
            App.WaitThenTap("PART_NativeRadioButton");
        }

        protected void OpenNavView()
		{
			if (!IsNavViewOpen())
			{
				App.WaitThenTap("NavToggle");

                //Give the nav view time to open up
                App.Wait(TimeSpan.FromSeconds(2));
            }
		}

		protected void CloseNavView()
        {
            if (IsNavViewOpen())
            {
                App.WaitThenTap("NavToggle");
            }
        }

        private bool IsNavViewOpen()
        {
            App.WaitForElement("RootSplitView", timeout: TimeSpan.FromSeconds(60));
            return App
                .Marked("RootSplitView")
                .GetDependencyPropertyValue<bool>("IsPaneOpen");
        }

        public FileInfo TakeScreenshot(string stepName)
        {
            var title = $"{TestContext.CurrentContext.Test.Name}_{stepName}"
                .Replace(" ", "_")
                .Replace(".", "_");

            var fileInfo = _app.Screenshot(title);

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.Name);
            if (fileNameWithoutExt != title)
            {
                var destFileName = Path
                    .Combine(Path.GetDirectoryName(fileInfo.FullName), title + Path.GetExtension(fileInfo.Name));

                if (File.Exists(destFileName))
                {
                    File.Delete(destFileName);
                }

                File.Move(fileInfo.FullName, destFileName);

                TestContext.AddTestAttachment(destFileName, stepName);

                fileInfo = new FileInfo(destFileName);
            }
            else
            {
                TestContext.AddTestAttachment(fileInfo.FullName, stepName);
            }

            return fileInfo;
        }

    }
}
