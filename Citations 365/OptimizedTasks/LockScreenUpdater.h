#pragma once

using namespace Platform;
//using namespace Windows::System;
using namespace Windows::Storage;

namespace OptimizedTasks
{
	public ref class LockScreenUpdater sealed : Windows::UI::Xaml::Media::Imaging::XamlRenderingBackgroundTask
	{
	private:
		// Name generation
		String^ RetrieveLockscreenBackgroundName();
		String^ GenerateAppBackgroundName(String^ prevName);
		void SaveLockscreenBackground(StorageFile^ background);

		String^ RetrieveQuoteContent();
		String^ RetrieveQuoteAuthor();
	protected:
		void OnRun(Windows::ApplicationModel::Background::IBackgroundTaskInstance^ taskInstance) override;
	public:
		LockScreenUpdater();
	};
}
