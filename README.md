# PlantHunter

Planthunter is a app that will help KUPU and the DLNR with finding endagerad plant species. With the collection of plant data they can find and protect these plants.

Planthunter is an easy to use app for hikers, citizen scientists and botanists. Take a hike and snap some pictures of plants. Guess the name and hope that you are correct.

If you are correct you get points. Be the one with the most points!

![image](https://raw.githubusercontent.com/HACC2018/Canoe-tree/master/PlantHunter.Mobile/PlantHunter.Mobile.Android/Resources/drawable-hdpi/bg_splash.png)

Build status:
[![Build status](https://allardsoeters.visualstudio.com/Canoe-tree-2/_apis/build/status/Canoe-tree-2-Azure%20Web%20App%20for%20ASP.NET-CI)](https://allardsoeters.visualstudio.com/Canoe-tree-2/_build/latest?definitionId=28)

## Running the Backend

There is an running sample [https://planthunter-2-dev-as.azurewebsites.net/](https://planthunter-2-dev-as.azurewebsites.net/)

Make sure you have visual studio installed and .net core 2.1 [dot.net](https://dot.net)

After cloning or downloading the sample you should be able to run it using an In Memory database immediately.

## Running mobile

We uploaded the Android version. You can download it via the following link:
[![Build status](https://build.appcenter.ms/v0.1/apps/9f176d19-b4a3-4408-8df8-7857a7eafde0/branches/master/badge)](https://appcenter.ms)[Download for android](https://install.appcenter.ms/orgs/hacc2018/apps/planthunter/distribution_groups/all)
**Known bug: the app crashes when you open it for the first time (after giving permission). After the crash just open the app again. Also when giving permission for taking a picture, just press "Take picture" again.** 

The mobile part is a bit more tricky. Make sure you have installed Xamarin, you can select the mobile development options during the install of visual studio.

Becasue of time we only fouccused on the android version. This version is running as it supposed to be. iOs and UWP we haven't tested it, but should work to.
