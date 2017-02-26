using System;
using Tasks.Factory;
using Windows.ApplicationModel.Background;

namespace Tasks {
    public sealed class UpdateTodayQuote : IBackgroundTask {
        BackgroundTaskDeferral _deferral;
        volatile bool _cancelRequested = false;

        private void StartTask(IBackgroundTaskInstance taskInstance) {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
        }

        private void EndTask() {
            _deferral.Complete();
        }

        public async void Run(IBackgroundTaskInstance taskInstance) {
            StartTask(taskInstance);

            var rand = new Random();
            var isOutdated = true;
            var restoredQuotes = await Settings.RestoreQuotesFromStorageAsync();

            if (restoredQuotes != null) {
                isOutdated = CheckTime();
            }

            var newQuotes = isOutdated ? 
                await Data.FetchNewQuotesAsync() : 
                await Settings.ExtractListAsyncFrom(restoredQuotes);

            if (newQuotes == null) {
                EndTask();
            }

            var pick = rand.Next(newQuotes.Count);

            TileDesigner.Update(newQuotes, pick);
            await Settings.SaveConfigAsync(newQuotes, pick, isOutdated);

            EndTask();
        }

        private bool CheckTime() {
            DateTimeOffset lastTime = Settings.GetLastTimeFetch();
            var timelapse = DateTimeOffset.Now.Subtract(lastTime);
            return timelapse.Hours > 6;
        }

        /// <summary>
        /// Indicate that the background task is canceled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reason"></param>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason) {
            _cancelRequested = true;
        }
    }
}
