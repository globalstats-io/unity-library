# unity-library
The integration of the globalstats.io API into a library for unity

This is still a work-in-progress. Please let us know any feedback you have or special use cases you have.

If you are looking for a .Net implementation that you can also use in Unity you can take a look at https://github.com/Yucked/StatsIO

## Usage
Simply add the .cs file to your project.

### Preparations
First, before using the library in your project, you have to set a few paramters:
```
GlobalstatsIO.api_id = "Your_API_ID";
GlobalstatsIO.api_secret = "Your_API_Secret";
```
You can set those anywhere in the code. The values are static and will be kept as long as you game is running.

### Submitting Scores
To submit a player score instatiante a object, add the values and call share()
```
GlobalstatsIO gs = new GlobalstatsIO();

string user_name = "Nickname";

Dictionary<string, string> values = new Dictionary<string, string> ();
values.Add ("score", Score.score.ToString());
values.Add ("shots", Score.shots_fired.ToString());
values.Add ("time", (Score.play_time * 1000).ToString());

if (gs.share ("", user_name, values)) {
    // Success
}
else {
    // An Error occured
}
```
The share call will store the users name and the ID it received back from globalstst.io.
This allows you to send updated to the score by simply doing the same call again with the new values.

You can check if there is a ID stored via this variable
```
if (GlobalstatsIO.statistic_id != "") {
    // A statistic was already shared, new calls to share() will do an update
}
```
You can also simply reset it by assigning an empty string to the value.

### Linking Scores
Most of the time you want to allow the user to link his scores with his globalstats.io account.
In this case you can do this with following lines
```
GlobalstatsIO gs = new GlobalstatsIO();
gs.linkStatistic ();
Application.OpenURL (GlobalstatsIO.link_data.url);
```
Of course, this will use the data from the prior share() call for linking.

### Retrieving Leaderboard 
You can also fetch the current top positions of your leaderboard with a GTD of your choice. You can retrieve that leaderboard with following lines:

```
GlobalstatsIO gs = new GlobalstatsIO();
string gtd = "score"
int limit = 2;
gs.getLeaderboard (gtd, limit);
```

In this case we want the leaderboard of the GTD score. The limit is the number players you want to fetch, which has to be between 1 and 100. 

## Feedback
If you encounter any issues you can create an issue here on github.
Furthermore feel free to report any issues, ask questions or submit suggestions via email to feedback@globalstats.io or via the feedback form on our website https://globalstats.io
