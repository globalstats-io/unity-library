# unity-library
The integration of the globalstats.io API into a library for unity

This is still a work-in-progress. Please let us know any feedback you have or special use cases you have.

If you are looking for a .Net implementation that you can also use in Unity you can take a look at https://github.com/Yucked/StatsIO

## Usage
Simply add the .cs file to your project.

OR

Include this repository as a package:
- In Unity 2021+
    1. Open the Package Manager (Window -> Package Manager).
    2. Click the plus sign button ("+") and select "Add package from git URL..."
    3. Enter the URL of this repository and click "Add".
- In previous versions, add the following line to your Packages/manifest.json file under dependencies:

    ```
    "io.gitstats.unity": "URL of this repo"
    ```

To get the URL of this repository to use in any of the options above, click on the green "Code" button at the top of this page, then copy the HTTPS URL.

### Submitting Scores
To submit a player score instantiate a object, add the values and call Share()
```
private const string GlobalstatsIOApiId = "YourApiIdHere";
private const string GlobalstatsIOApiSecret = "YourApiSecretHere";

var gs = new GlobalstatsIOClient(GlobalstatsIOApiId, GlobalstatsIOApiSecret);

string user_name = "Nickname";

Dictionary<string, string> values = new Dictionary<string, string> ();
values.Add ("score", Score.score.ToString());
values.Add ("shots", Score.shots_fired.ToString());
values.Add ("time", (Score.play_time * 1000).ToString());

// use StartCoroutine to submit the score asynchronously and use the optional callback parameter
StartCoroutine(gs.Share ("", user_name, values, CallbackMethod)));

void CallbackMethod(bool success){
    if (success){
        // do something with success
    }
    else {
        // do something with error
    }
}

```
The Share call will store the users name and the ID it received back from globalstats.io.
This allows you to send updated to the score by simply doing the same call again with the new values.

You can check if there is a ID stored via this variable
```
if (gs.statistic_id != "") {
    // A statistic was already shared, new calls to share() will do an update
}
```
You can also simply reset it by assigning an empty string to the value.

### Linking Scores
Most of the time you want to allow the user to link his scores with his globalstats.io account.
In this case you can do this with following lines
```
GlobalstatsIO gs = new GlobalstatsIO();
StartCoroutine(gs.LinkStatistic(CallbackMethod));

void CallbackMethod(bool success){
    if (success){
        // do something with success, like redirecting to the webpage
        Application.OpenURL (gs.link_data.url);
    }
    else {
        // do something with error
    }
}
```
Of course, this will use the data from the prior share() call for linking.

### Retrieving Leaderboard
You can also fetch the current top positions of your leaderboard with a GTD of your choice. You can retrieve that leaderboard with following lines:

```
GlobalstatsIO gs = new GlobalstatsIO();
string gtd = "score"
int limit = 2;

StartCoroutine(gs.GetLeaderboard(gtd, limit, CallbackMethod));

void CallbackMethod(Leaderboard leaderboard){
    if (leaderboard != null){
        // do something with leaderboard
    }
    else {
        // do something with error
    }
}
```

In this case we want the leaderboard of the GTD score. The limit is the number players you want to fetch, which has to be between 1 and 100.

## Feedback
If you encounter any issues you can create an issue here on github.
Furthermore feel free to report any issues, ask questions or submit suggestions via email to feedback@globalstats.io or via the feedback form on our website https://globalstats.io
