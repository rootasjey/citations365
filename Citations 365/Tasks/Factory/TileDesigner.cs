using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Generic;
using Tasks.Models;
using Windows.UI.Notifications;

namespace Tasks.Factory {
    public sealed class TileDesigner {
        public static void Update(IList<Quote> quotes, int start) {
            if (quotes == null || quotes?.Count < 1) {
                return;
            }

            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.Clear();
            tileUpdater.EnableNotificationQueue(true);

            start = start + 5 > quotes.Count ? (start - 5) : start;

            for (int i = start; i < (start + 5); i++) {
                tileUpdater.Update(CreateNotification(quotes[i]));
            }
        }

        private static TileNotification CreateNotification(Quote quote) {
            var content = new TileContent() {
                Visual = new TileVisual() {
                    TileMedium = CreateBinding(quote),
                    TileWide = CreateBinding(quote),
                    TileLarge = CreateBinding(quote)
                }
            };

            return new TileNotification(content.GetXml());
        }

        private static TileBinding CreateBinding(Quote quote) {
            return new TileBinding() {
                Content = new TileBindingContentAdaptive() {
                    TextStacking = TileTextStacking.Center,
                    Children = {
                        new AdaptiveText() {
                            Text = quote.Author,
                            HintStyle = AdaptiveTextStyle.CaptionSubtle,
                            HintAlign = AdaptiveTextAlign.Center
                        },
                        new AdaptiveText() {
                            Text = quote.Content,
                            HintStyle = AdaptiveTextStyle.Base,
                            HintAlign = AdaptiveTextAlign.Center,
                            HintWrap = true
                        }
                    }
                }
            };
        }
    }
}
