# Citations 365
A quotes app written for Windows 10.

Read meaningful quotes everyday.

<a href="https://www.microsoft.com/store/apps/9nblggh68cv1?ocid=badge"><img src="https://assets.windowsphone.com/f2f77ec7-9ba9-4850-9ebe-77e366d08adc/English_Get_it_Win_10_InvariantCulture_Default.png" alt="Get it on Windows 10" width="200" /></a>

# screenshots
## on mobile
### home
<img src="screenshot_mobile.png" alt="citations365" height="400" />

### lockscreen
<img src="lockscreen.png" alt="lockscreen" height="400" />


## on desktop
### home
<img src="screenshot_desktop.png" alt="citations365" height="400" />

### author page
<img src="screenshot_author_desktop.png" alt="citations365" height="400" />

## tile animation
<img src="./livetile.gif"/>

# features
* new quotes every day
* save the nicest quotes in your favorites
* share quotes to the world
* browse the database by famous authors (not available for all languages)

# setup
Steps to build and run this project:

1. Clone or download this repository
2. (Optional) Unzip the archive to your favorite location
3. Navigate to the ```Citations 365/``` folder
4. Open ```Citations 365.sln``` in [Visual Studio](https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=Community&rel=15)
5. Choose your favorite platform and click on Run :)


# platforms

* Windows Mobile 10
* Windows 10

# sources

French

* [Evene](http://evene.lefigaro.fr/)

English

* [QuotesOnDesign](https://quotesondesign.com/api-v4-0/)

# architecture overview

This section describes the way I've organized my files and directories
to build this app in the clearest way possible.

## Views

All the views are localized inside the ```Views/``` folder, except for the ```App.xaml``` and ```App.xaml.cs``` which is the main app's view page.

## Data

All data are managed inside the ```Data/``` folder.

For more information, visit the corresponding folders.

# contributing

You can contribute to improve this project by:

* creating a pull request
* submitting new ideas / features suggestions 
* reporting a bug.

# todo

* Mashup multi-sources for each language
* Add option to apply generated images to wallpaper (currently only for locksreen)
* Adapt generated images for desktop devices (good looking only on mobile devices currently)
* Add a service to allow others apps to query quote data from _Citations 365_
* Add In-App-Purchases/Donation button