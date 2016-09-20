#include "pch.h"
#include "LockScreenUpdater.h"
#include <string>
#include <sstream>
#include <robuffer.h>
#include <vector>

using namespace OptimizedTasks;
using namespace concurrency;
using namespace Windows::ApplicationModel::Background;
using namespace Windows::ApplicationModel;
using namespace Windows::Foundation;
using namespace Windows::Graphics::Imaging;
using namespace Windows::Storage::Streams;
using namespace Windows::System::UserProfile;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Markup;
using namespace Windows::UI::Xaml::Media::Imaging;

// Private variables
String^ _lockscreenBackgroundName = "LockscreenBackgroundName";
String^ _lockscreenBackgroundPath = "LockscreenBackgroundPath";

String^ _dailyQuoteContent = "DailyQuoteContent";
String^ _dailyQuoteAuthor = "DailyQuoteAuthor";

LockScreenUpdater::LockScreenUpdater()
{
}

void LockScreenUpdater::OnRun(IBackgroundTaskInstance^ taskInstance)
{
	Agile<BackgroundTaskDeferral^> deferral = Agile<BackgroundTaskDeferral^>(taskInstance->GetDeferral());
	taskInstance->Canceled += ref new BackgroundTaskCanceledEventHandler(this, &LockScreenUpdater::OnCanceled);


	if (UserProfilePersonalizationSettings::Current->IsSupported) {
		String^ prevName = RetrieveLockscreenBackgroundName();
		String^ newName = GenerateAppBackgroundName(prevName);

		create_task(StorageFile::GetFileFromApplicationUriAsync(ref new Uri("ms-appx:///Assets/lockScreen.xml"))).then([](StorageFile^ xamlFile) {
			return FileIO::ReadTextAsync(xamlFile);

		}).then([this, newName](String^ content) {

			String^ uri = "https://unsplash.it/720/1080?random";
			Uri^ _uri = ref new Uri(uri);
			ApplicationData^ current = ApplicationData::Current;

			return create_task(StorageFile::CreateStreamedFileFromUriAsync(newName, _uri, RandomAccessStreamReference::CreateFromUri(_uri)))
				.then([current, newName](StorageFile^ f) { // Download image
				return f->CopyAsync(current->LocalFolder, newName, NameCollisionOption::ReplaceExisting);

			}).then([](StorageFile^ f2) {
				return f2;

			}).then([this, newName, content](StorageFile^ file) {
				SaveLockscreenBackground(file);

				String^ quoteContent = RetrieveQuoteContent();
				String^ quoteAuthor = RetrieveQuoteAuthor();

				std::wstringstream streamContent;
				std::wstringstream croppedContent;
				streamContent << quoteContent->Data();

				if (streamContent.str().length() > 194) // max 194 chars
				{
					croppedContent << streamContent.str().substr(0, 191) << "...";
				}
				else
				{
					croppedContent << streamContent.str();
				}

				auto size = Window::Current->Bounds;

				Grid^ root = (Grid^)XamlReader::Load(content);
				root->Width = size.Width;
				root->Height = size.Height;

				std::wstringstream imgName;
				imgName << L"ImgBackground";
				Image^ img = (Image^)root->FindName(ref new String(imgName.str().c_str()));
				img->Source = ref new BitmapImage(ref new Uri(file->Path));

				// Author
				std::wstringstream tbAuthorName;
				tbAuthorName << L"TbAuthor";
				TextBlock^ tbAuthor = (TextBlock^)root->FindName(ref new String(tbAuthorName.str().c_str()));
				tbAuthor->Text = quoteAuthor;

				// Content
				std::wstringstream tbContentName;
				tbContentName << L"TbContent";
				TextBlock^ tbContent = (TextBlock^)root->FindName(ref new String(tbContentName.str().c_str()));
				tbContent->Text = ref new String(croppedContent.str().c_str());

				RenderTargetBitmap^ rtb = ref new RenderTargetBitmap();
				return create_task(rtb->RenderAsync(root, size.Width, size.Height)).then([rtb]() {
					return rtb->GetPixelsAsync();
				}).then([rtb, newName](IBuffer^ buffer) {
					String^ lockscreen = "lockscrren" + newName;

					return create_task(ApplicationData::Current->LocalFolder->CreateFileAsync(lockscreen, CreationCollisionOption::ReplaceExisting)).then([buffer, rtb](StorageFile^ contentFile) {
						return create_task(contentFile->OpenAsync(FileAccessMode::ReadWrite)).then([](IRandomAccessStream^ output) {
							return BitmapEncoder::CreateAsync(BitmapEncoder::PngEncoderId, output);
						}).then([buffer, rtb](BitmapEncoder^ encoder) {

							IUnknown* pUnk = reinterpret_cast<IUnknown*>(buffer);
							IBufferByteAccess* pBufferByteAccess = nullptr;
							pUnk->QueryInterface(IID_PPV_ARGS(&pBufferByteAccess));
							byte *pixels = nullptr;
							pBufferByteAccess->Buffer(&pixels);

							Array<unsigned char>^ data = ref new Array<unsigned char>(pixels, rtb->PixelWidth * rtb->PixelHeight * 4);
							encoder->SetPixelData(BitmapPixelFormat::Bgra8, BitmapAlphaMode::Premultiplied, rtb->PixelWidth, rtb->PixelHeight, 96, 96, data);

							return encoder->FlushAsync();
						}).then([contentFile]() {
							return Windows::System::UserProfile::UserProfilePersonalizationSettings::Current->TrySetLockScreenImageAsync(contentFile);
						});
					});
				});
			});			
		}).then([deferral](bool ok) {
			deferral->Complete();
		});

	}
	else {
		// todo ...
		deferral->Complete();
	}

}

void LockScreenUpdater::OnCanceled(Windows::ApplicationModel::Background::IBackgroundTaskInstance^ taskInstance, BackgroundTaskCancellationReason reason)
{
	// TODO: Add code to notify the background task that it is cancelled.
	CancelRequested = true;
}

String^ LockScreenUpdater::RetrieveLockscreenBackgroundName()
{
	ApplicationData^ current = ApplicationData::Current;
	ApplicationDataContainer^ localSettings = current->LocalSettings;
	String^ name = localSettings->Values->Lookup(_lockscreenBackgroundName)->ToString();
	return name;
}

String^ LockScreenUpdater::GenerateAppBackgroundName(String ^ prevName)
{
	String^ name1 = "wall1.png";
	String^ name2 = "wall2.png";

	if (prevName == name1) {
		return name2;
	}
	return name1;
}

void LockScreenUpdater::SaveLockscreenBackground(StorageFile^ background)
{
	ApplicationData^ current = ApplicationData::Current;
	ApplicationDataContainer^ localSettings = current->LocalSettings;
	localSettings->Values->Insert(_lockscreenBackgroundName, background->Name);
	localSettings->Values->Insert(_lockscreenBackgroundPath, background->Path);
}

String^ LockScreenUpdater::RetrieveQuoteContent() {
	ApplicationData^ current = ApplicationData::Current;
	ApplicationDataContainer^ localSettings = current->LocalSettings;
	return localSettings->Values->Lookup(_dailyQuoteContent)->ToString();
}

String^ LockScreenUpdater::RetrieveQuoteAuthor() {
	ApplicationData^ current = ApplicationData::Current;
	ApplicationDataContainer^ localSettings = current->LocalSettings;
	return localSettings->Values->Lookup(_dailyQuoteAuthor)->ToString();
}